using Asp.Versioning;
using EduSpaceEngine.Data;
using EduSpaceEngine.Hubs;
using EduSpaceEngine.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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


builder.Services.AddDbContext<DataDbContext>(
                       options =>
                       {
                           var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                           if (!builder.Environment.IsDevelopment())
                           {
                               var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
                               connectionString = string.Format(connectionString, password);
                           }
                           options.UseSqlServer(connectionString);

                       });


builder.Services.AddSignalR();

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



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
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
//}

app.UseHttpsRedirection();

app.UseAuthentication();


app.UseAuthorization();

app.UseCors("eduspaceApp");

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<CommentHub>("/commentHub");

/*using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();
    db.Database.Migrate();
}*/


app.Run();
