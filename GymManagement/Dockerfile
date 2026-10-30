# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY GymManagement.csproj ./

RUN dotnet restore GymManagement.csproj

COPY . .

RUN dotnet publish GymManagement.csproj \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_HTTP_PORTS=3000

EXPOSE 3000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GymManagement.dll"]
