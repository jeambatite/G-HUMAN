# Etapa 1: Construcción (Build)
FROM node:18-alpine AS build
WORKDIR /app

# Instalar dependencias
COPY package*.json ./
RUN npm install

# Copiar código y construir el proyecto
COPY . .
RUN npm run build -- --configuration=production

# Etapa 2: Servidor (Nginx) para entregar los archivos
FROM nginx:alpine

# IMPORTANTE: Esta línea busca la carpeta 'browser' dentro de cualquier subcarpeta de 'dist'
COPY --from=build /app/dist/*/browser /usr/share/nginx/html

# Configuración para que las rutas de Angular funcionen al refrescar (F5)
RUN printf 'server { \n\
    listen 80; \n\
    location / { \n\
        root /usr/share/nginx/html; \n\
        index index.html; \n\
        try_files $uri $uri/ /index.html; \n\
    } \n\
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]