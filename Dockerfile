# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY ["AppService.csproj", "."]
RUN dotnet restore "AppService.csproj"

# Copy source code
COPY . .
RUN dotnet build "AppService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AppService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "AppService.dll"]
