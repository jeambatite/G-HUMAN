# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar el archivo de proyecto y restaurar dependencias
# (Asegúrate de que el nombre sea GHumanAPI.csproj)
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar el PROYECTO, no la solución
COPY . ./
RUN dotnet publish GHumanAPI.csproj -c Release -o out

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configurar el puerto para Railway
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# El nombre del DLL debe ser el que genera tu proyecto
ENTRYPOINT ["dotnet", "GHumanAPI.dll"]