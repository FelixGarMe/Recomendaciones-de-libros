FROM node:18-slim

# Establece el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copia los archivos de tu aplicación al contenedor
COPY package*.json ./
COPY . .

# Instala las dependencias de tu aplicación
RUN npm install

# Expone el puerto en el que se ejecuta tu aplicación
EXPOSE 3000

# Define el comando de inicio para ejecutar tu aplicación
CMD ["node", "server.js"]