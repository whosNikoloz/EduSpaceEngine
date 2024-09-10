using Asp.Versioning;
using EduSpaceEngine.Data;
using EduSpaceEngine.GraphQL;
using EduSpaceEngine.Hubs;
using EduSpaceEngine.Services.Email;
using EduSpaceEngine.Services.Static;
using EduSpaceEngine.Services.User;
using EduSpaceEngine.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Globalization;
using System.Text;
using HotChocolate.AspNetCore.Playground;
using HotChocolate.AspNetCore;
using EduSpaceEngine.Services.Social;
using EduSpaceEngine.Services.Learn.Course;
using EduSpaceEngine.Services.Learn.Subject;
using EduSpaceEngine.Services.Learn.Lesson;
using EduSpaceEngine.Services.Learn.Level;
using EduSpaceEngine.Services.Learn.Test;
using EduSpaceEngine.Services.Learn.LearnMaterial;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
    options.OperationFilter<SwaggerDefaultValues>();
});

builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = new UrlSegmentApiVersionReader();

    //ApiVersionReader.Combine(new HeaderApiVersionReader("x-api-version"), new QueryStringApiVersionReader("api-version"));
});



builder.Services
    .AddApiVersioning()
    .AddApiExplorer(options =>
    {

        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });



/*builder.Services.AddDbContext<DataDbContext>(
                       options =>
                       {
                           var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                           if (!builder.Environment.IsDevelopment())
                           {
                               var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
                               connectionString = string.Format(connectionString, password);
                           }
                           options.UseSqlServer(connectionString);

                       });*/


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DataDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSignalR();

builder.Services.AddScoped<Query>();
builder.Services.AddScoped<Mutation>();
builder.Services.AddScoped<DataDbContext>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatiFuncs, StaticFuncs>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


//Learn
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILevelService, LevelService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ILearnMaterialService, LearnMaterialService>();


//builder.Services.AddSingleton<NotificationHub>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("eduspaceApp", builder =>
    {
        builder.WithOrigins("https://edu-space.vercel.app", "http://localhost:3000", "http://localhost:4200", "https://localhost:53377")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
    });
});


builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();
    //.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();

        // Build a swagger endpoint for each discovered API version
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });

    app.UsePlayground(new PlaygroundOptions
    {
        Path = "/playground",
        QueryPath = "/graphql"
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();


app.UseAuthorization();

app.UseCors("eduspaceApp");

app.MapControllers();

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<CommentHub>("/commentHub");

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();

    try
    {
        var retries = 5; // Number of retry attempts
        var delay = TimeSpan.FromSeconds(5); // Delay between retries

        for (int i = 0; i < retries; i++)
        {
            try
            {
                // Check if the database exists, create if not
                await CreateDatabaseIfNotExistsAsync(db, logger);

                var canConnect = await db.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogError("Failed to connect to the database.");
                    throw new Exception("Failed to connect to the database.");
                }

                await db.Database.MigrateAsync(); // Asynchronous migration

                var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
                logger.LogInformation("Database migration successful.!!!!!");
                break; // Exit the retry loop if successful
            }
            catch (SqlException ex) when (ex.Number == 18456)
            {
                logger.LogError(ex, "Login failed for user. Check SQL Server credentials.");
                throw; // Re-throw the exception to ensure the application exits gracefully
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration. Retrying...");
                await Task.Delay(delay);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to migrate database.");
        throw; // Re-throw the exception to ensure the application exits gracefully
    }
}


app.Run();




// Helper method to create database if it doesn't exist
async Task CreateDatabaseIfNotExistsAsync(DataDbContext db, ILogger logger)
{
    if (!await db.Database.CanConnectAsync())
    {
        try
        {
            logger.LogInformation("Database does not exist. Attempting to create...");

            await db.Database.EnsureCreatedAsync(); // Creates the database and its schema

            logger.LogInformation("Database created successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create database.");
            throw; // Re-throw the exception to ensure the application exits gracefully
        }
    }
}