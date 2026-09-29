FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["SmartPrompt.API/SmartPrompt.API.csproj", "SmartPrompt.API/"]
COPY ["SmartPrompt.Application/SmartPrompt.Application.csproj", "SmartPrompt.Application/"]
COPY ["SmartPrompt.Infrastructure/SmartPrompt.Infrastructure.csproj", "SmartPrompt.Infrastructure/"]
COPY ["SmartPrompt.Domain/SmartPrompt.Domain.csproj", "SmartPrompt.Domain/"]
RUN dotnet restore "SmartPrompt.API/SmartPrompt.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SmartPrompt.API"
RUN dotnet publish "SmartPrompt.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartPrompt.API.dll"]
