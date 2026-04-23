# Etapa 1: Construcción (Build) - CAMBIAMOS A NODE 22
FROM node:22-alpine AS build
WORKDIR /app

# Instalar dependencias
COPY package*.json ./
RUN npm install

# Copiar código y construir el proyecto
COPY . .
RUN npm run build -- --configuration=production

# Etapa 2: Servidor (Nginx)
FROM nginx:alpine

# Copiamos los archivos generados
COPY --from=build /app/dist/*/browser /usr/share/nginx/html

# Configuración para rutas de Angular (F5 fix)
RUN printf 'server { \n\
    listen 8080; \n\
    location / { \n\
        root /usr/share/nginx/html; \n\
        index index.html; \n\
        try_files $uri $uri/ /index.html; \n\
    } \n\
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]