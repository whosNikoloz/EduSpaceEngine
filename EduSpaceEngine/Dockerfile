# Use the official ASP.NET core image as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["EduSpaceEngine.csproj", "./"]
RUN dotnet restore "./EduSpaceEngine.csproj"

# Copy the entire solution and build the application
COPY . .
WORKDIR "/src/."
RUN dotnet build "EduSpaceEngine.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "EduSpaceEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage for the production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EduSpaceEngine.dll"]
