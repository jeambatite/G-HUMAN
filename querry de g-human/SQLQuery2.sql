-- ══════════════════════════════════════════════
--   G-HUMAN DB — Script Completo
--   Base de datos para sistema de gestión RRHH
-- ══════════════════════════════════════════════

USE master;
GO

-- Crear base de datos
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'G_HUMAN_DB')
    CREATE DATABASE G_HUMAN_DB;
GO

USE G_HUMAN_DB;
GO

-- ══ TABLA: Roles ══
CREATE TABLE Roles (
    id     INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL
);

-- ══ TABLA: Permisos ══
CREATE TABLE Permisos (
    id     INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL
);

-- ══ TABLA: RolesPermisos ══
CREATE TABLE RolesPermisos (
    id_rol     INT NOT NULL REFERENCES Roles(id) ON DELETE CASCADE,
    id_permiso INT NOT NULL REFERENCES Permisos(id),
    PRIMARY KEY (id_rol, id_permiso)
);

-- ══ TABLA: Empleados ══
CREATE TABLE Empleados (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    genero       NVARCHAR(1)   NOT NULL,
    nombre       NVARCHAR(200) NOT NULL,
    email        NVARCHAR(200) NOT NULL UNIQUE,
    sueldo       DECIMAL(18,2) NOT NULL,
    fecha_i      DATE          NOT NULL,
    departamento NVARCHAR(100) NOT NULL,
    id_jefe      INT           NULL REFERENCES Empleados(id) ON DELETE NO ACTION,
    id_rol       INT           NOT NULL REFERENCES Roles(id) ON DELETE NO ACTION,
    estado       NVARCHAR(20)  NOT NULL CHECK (estado IN ('activo','inactivo','vacaciones','suspendido'))
);

-- ══ TABLA: DatosSensibles ══
CREATE TABLE DatosSensibles (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    id_empleado         INT           NOT NULL UNIQUE REFERENCES Empleados(id) ON DELETE CASCADE,
    tipo_documento      NVARCHAR(50)  NOT NULL,
    num_documento       NVARCHAR(50)  NOT NULL UNIQUE,
    tipo_sangre         NVARCHAR(5)   NOT NULL,
    fecha_nacimiento    DATE          NOT NULL,
    telefono            NVARCHAR(20)  NOT NULL,
    contacto_emergencia NVARCHAR(200) NOT NULL,
    tel_emergencia      NVARCHAR(20)  NOT NULL
);

-- ══ TABLA: Usuarios ══
CREATE TABLE Usuarios (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    id_empleado   INT           NOT NULL REFERENCES Empleados(id) ON DELETE NO ACTION,
    username      NVARCHAR(100) NOT NULL UNIQUE,
    password_hash NVARCHAR(500) NOT NULL,
    pin_hash      NVARCHAR(500) NULL,
    activo        BIT           NOT NULL DEFAULT 1
);

GO

-- ══════════════════════════════════════════════
--   SEED DATA
-- ══════════════════════════════════════════════

-- Roles
SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (id, nombre) VALUES
(1, 'Nivel 1'),
(2, 'Nivel 2'),
(3, 'Nivel 3'),
(4, 'Admin');
SET IDENTITY_INSERT Roles OFF;

-- Permisos
SET IDENTITY_INSERT Permisos ON;
INSERT INTO Permisos (id, nombre) VALUES
(1,  'Ver personal'),
(2,  'Editar datos basicos'),
(3,  'Ver sueldo'),
(4,  'Editar sueldo'),
(5,  'Agregar personal'),
(6,  'Eliminar personal'),
(7,  'Gestionar roles'),
(10, 'Ver datos sensibles');
SET IDENTITY_INSERT Permisos OFF;

-- RolesPermisos
-- Nivel 1: solo ver personal
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES
(1, 1);

-- Nivel 2: ver + editar basico
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES
(2, 1),
(2, 2);

-- Nivel 3: ver + editar + sueldo + datos sensibles
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES
(3, 1),
(3, 2),
(3, 3),
(3, 4),
(3, 10);

-- Admin: todos los permisos
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES
(4, 1),
(4, 2),
(4, 3),
(4, 4),
(4, 5),
(4, 6),
(4, 7),
(4, 10);

-- ══ Empleado Admin ══
SET IDENTITY_INSERT Empleados ON;
INSERT INTO Empleados (id, genero, nombre, email, sueldo, fecha_i, departamento, id_jefe, id_rol, estado)
VALUES (1, 'M', 'Juan Diaz', 'j.diaz@ghuman.com', 80000, '2018-01-01', 'Recursos Humanos', NULL, 4, 'activo');
SET IDENTITY_INSERT Empleados OFF;

-- Datos sensibles del Admin
SET IDENTITY_INSERT DatosSensibles ON;
INSERT INTO DatosSensibles (id, id_empleado, tipo_documento, num_documento, tipo_sangre, fecha_nacimiento, telefono, contacto_emergencia, tel_emergencia)
VALUES (1, 1, 'Cedula', '001-0000001-1', 'O+', '1985-06-15', '809-555-0001', 'Maria Diaz', '809-555-0002');
SET IDENTITY_INSERT DatosSensibles OFF;

-- Usuario Admin
-- password: Admin1234 (BCrypt hash)
-- Ejecutar primero GET /api/auth/hash/Admin1234 para obtener el hash correcto
-- y actualizar con: UPDATE Usuarios SET password_hash = 'HASH_AQUI' WHERE id = 1
SET IDENTITY_INSERT Usuarios ON;
INSERT INTO Usuarios (id, id_empleado, username, password_hash, pin_hash, activo)
VALUES (1, 1, 'admin', '$2a$11$reemplazarConHashReal', NULL, 1);
SET IDENTITY_INSERT Usuarios OFF;

GO

-- ══════════════════════════════════════════════
--   NOTA IMPORTANTE
-- ══════════════════════════════════════════════
-- El password_hash debe generarse desde el endpoint:
-- GET /api/auth/hash/Admin1234
-- Luego ejecutar:
-- UPDATE Usuarios SET password_hash = 'HASH_GENERADO' WHERE id = 1;
-- ══════════════════════════════════════════════