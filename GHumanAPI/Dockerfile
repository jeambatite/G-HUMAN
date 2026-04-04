# Etapa 1: Construcción
FROM node:18-alpine AS build
WORKDIR /app

# Copiar archivos de dependencias
COPY package*.json ./
RUN npm install

# Copiar todo el código y construir
COPY . .
RUN npm run build -- --configuration=production

# Etapa 2: Servidor Web (Nginx)
FROM nginx:alpine

# AQUÍ ESTÁ EL TRUCO: 
# Usamos un comodín (*) para encontrar la carpeta 'browser' sin importar el nombre del proyecto
COPY --from=build /app/dist/*/browser /usr/share/nginx/html

# Copiamos una configuración básica de Nginx para Angular (Rutas)
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