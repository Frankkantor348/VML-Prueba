# Imagen de compilación.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copio primero solo los .csproj para aprovechar la caché de capas: mientras no
# cambien las dependencias, NuGet no vuelve a restaurar en cada despliegue.
COPY src/VmlTest.Domain/VmlTest.Domain.csproj src/VmlTest.Domain/
COPY src/VmlTest.Application/VmlTest.Application.csproj src/VmlTest.Application/
COPY src/VmlTest.Infrastructure/VmlTest.Infrastructure.csproj src/VmlTest.Infrastructure/
COPY src/VmlTest.Api/VmlTest.Api.csproj src/VmlTest.Api/
RUN dotnet restore src/VmlTest.Api/VmlTest.Api.csproj

# Ahora sí el código fuente y la publicación en Release.
COPY src/ src/
RUN dotnet publish src/VmlTest.Api/VmlTest.Api.csproj -c Release -o /app/publish --no-restore

# Imagen de ejecución: solo el runtime, bastante más liviana que la del SDK.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Los proveedores gratuitos enrutan el tráfico al puerto que exponga el contenedor.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "VmlTest.Api.dll"]
