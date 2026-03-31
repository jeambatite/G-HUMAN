G-HUMAN: Enterprise Resource Planning (ERP) - Human Resources
[!WARNING]
AVISO DE DESARROLLO: Este proyecto se encuentra actualmente en fase activa de desarrollo. Algunas funcionalidades pueden estar incompletas, sujetas a cambios críticos o contener errores. No se recomienda su uso en entornos de producción.

📝 Descripción
G-HUMAN es una plataforma robusta de gestión de capital humano diseñada para centralizar procesos administrativos. El sistema implementa una arquitectura desacoplada, asegurando escalabilidad y una separación clara de responsabilidades entre el core de negocio y la interfaz de usuario.

Características Principales
Gestión de Talento: Administración completa de perfiles de empleados.

Seguridad Multicapa: Segregación de datos sensibles y públicos.

Control de Acceso: Sistema RBAC (Role-Based Access Control) mediante JWT.

Analytics: Panel visual de distribución salarial y departamental.

🏗️ Arquitectura Técnica
Backend (Core API)
Framework: .NET 8.0 (C#)

ORM: Entity Framework Core (Code First / Fluent API)

Seguridad: Autenticación asíncrona con JWT (JSON Web Tokens) y hashing BCrypt.

Patrones: Inyección de Dependencias, DTOs (Data Transfer Objects) y Repositorios.

Frontend (Client-Side)
Framework: Angular 17+

Estrategia: Componentes Standalone para optimización de bundle.

Protección: Guards de ruta y servicios interceptores para cabeceras de seguridad.

🛠️ Guía de Instalación y Despliegue
Requisitos Técnicos
SDK: .NET 8.0

Entorno de Ejecución: Node.js (v18 o superior)

Motor de BD: Microsoft SQL Server 2019+

Paso 1: Configuración de la Base de Datos
Cree una nueva instancia en SQL Server denominada G_HUMAN_DB.

Ejecute el script de migración inicial (localizado en /database/init.sql) para establecer el esquema de tablas y los roles maestros.

Paso 2: Configuración del Servidor (.NET)
Acceda al directorio del backend.

Configure su cadena de conexión en el archivo appsettings.json:

"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=G_HUMAN_DB;Trusted_Connection=True;TrustServerCertificate=True;"
}

Ejecute la restauración de paquetes y el lanzamiento:
dotnet restore
dotnet run

Paso 3: Configuración del Cliente (Angular)
Acceda al directorio del frontend.

Instale las dependencias del ecosistema:
npm install
ng serve -o

🔑 Acceso PrematuroPara pruebas de integración, utilice las siguientes credenciales de nivel de sistema:
Rol: Administrador
Usuario: admin
Contraseña: Admin1234