FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos y restaurar
COPY . .
RUN dotnet restore

# Publicar el proyecto
RUN dotnet publish -c Release -o out

# Imagen de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render asigna un puerto dinámico, .NET debe escucharlo
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "G-HUMAN.API.dll"]
