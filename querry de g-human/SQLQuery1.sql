-- ══════════════════════════════════════
-- G_HUMAN_DB — Script de creacion
-- ══════════════════════════════════════
/*use master;
go
drop database G_HUMAN_DB;*/
CREATE DATABASE G_HUMAN_DB;
GO

USE G_HUMAN_DB;
GO

-- ROLES
CREATE TABLE Roles (
    id      INT IDENTITY(1,1) PRIMARY KEY,
    nombre  VARCHAR(50) NOT NULL UNIQUE
);
GO

-- PERMISOS
CREATE TABLE Permisos (
    id      INT IDENTITY(1,1) PRIMARY KEY,
    nombre  VARCHAR(100) NOT NULL UNIQUE
);
GO

-- ROLES_PERMISOS
-- Al eliminar un rol: se eliminan sus permisos en cascada
CREATE TABLE RolesPermisos (
    id_rol      INT NOT NULL,
    id_permiso  INT NOT NULL,
    PRIMARY KEY (id_rol, id_permiso),
    FOREIGN KEY (id_rol)     REFERENCES Roles(id)    ON DELETE CASCADE,
    FOREIGN KEY (id_permiso) REFERENCES Permisos(id) ON DELETE CASCADE
);
GO

-- EMPLEADOS
-- id_jefe: al eliminar un jefe, empleados a su cargo quedan sin jefe (SET NULL)
-- id_rol: no se permite eliminar un rol si tiene empleados asignados
CREATE TABLE Empleados (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    genero          VARCHAR(1)      NOT NULL,
    nombre          VARCHAR(100)    NOT NULL,
    email           VARCHAR(150)    NOT NULL UNIQUE,
    sueldo          DECIMAL(10,2)   NOT NULL DEFAULT 0,
    fecha_i         DATE            NOT NULL,
    departamento    VARCHAR(100)    NOT NULL,
    id_jefe         INT             NULL,
    id_rol          INT             NOT NULL,
    estado          VARCHAR(20)     NOT NULL DEFAULT 'activo'
                    CHECK (estado IN ('activo','vacaciones','inactivo','suspendido')),
    FOREIGN KEY (id_jefe) REFERENCES Empleados(id),
    FOREIGN KEY (id_rol)  REFERENCES Roles(id)
);
GO

CREATE TABLE DatosSensibles (
    id                    INT IDENTITY(1,1) PRIMARY KEY,
    id_empleado           INT          NOT NULL UNIQUE,
    tipo_documento        VARCHAR(50)  NOT NULL,
    num_documento         VARCHAR(50)  NOT NULL UNIQUE,
    tipo_sangre           VARCHAR(5)   NOT NULL,
    fecha_nacimiento      DATE         NOT NULL,
    telefono              VARCHAR(20)  NOT NULL,
    contacto_emergencia   VARCHAR(100) NOT NULL,
    tel_emergencia        VARCHAR(20)  NOT NULL,
    FOREIGN KEY (id_empleado) REFERENCES Empleados(id) ON DELETE CASCADE
);
GO

CREATE TABLE Usuarios (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    id_empleado     INT          NOT NULL UNIQUE,
    username        VARCHAR(50)  NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    pin_hash        VARCHAR(255) NULL,
    activo          BIT          NOT NULL DEFAULT 1,
    FOREIGN KEY (id_empleado) REFERENCES Empleados(id)
);
GO

-- ══════════════════════════════════════
-- DATOS INICIALES
-- ══════════════════════════════════════

-- Roles por defecto
INSERT INTO Roles (nombre) VALUES
('Nivel 1'), ('Nivel 2'), ('Nivel 3'), ('Admin');
GO

-- Permisos disponibles
INSERT INTO Permisos (nombre) VALUES
('Ver personal'),
('Editar datos basicos'),
('Ver sueldo'),
('Editar sueldo'),
('Agregar personal'),
('Eliminar personal'),
('Gestionar roles');
GO

-- Nivel 1: solo ver personal
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (1, 1);

-- Nivel 2: ver + editar basicos
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (2, 1), (2, 2);

-- Nivel 3: ver + editar + ver/editar sueldo
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (3, 1), (3, 2), (3, 3), (3, 4);

-- Admin: todos los permisos
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (4, 1), (4, 2), (4, 3), (4, 4), (4, 5), (4, 6), (4, 7);
GO

--select*  from RolesPermisos;

USE G_HUMAN_DB;

-- Insertar empleado Admin
INSERT INTO Empleados (genero, nombre, email, sueldo, fecha_i, departamento, id_jefe, id_rol, estado)
VALUES ('M', 'Juan Diaz', 'j.diaz@ghuman.com', 80000, '2018-01-01', 'Recursos Humanos', NULL, 4, 'activo');

-- Insertar datos sensibles del Admin
INSERT INTO DatosSensibles (id_empleado, tipo_documento, num_documento, tipo_sangre, fecha_nacimiento, telefono, contacto_emergencia, tel_emergencia)
VALUES (1, 'Cedula', '001-0000001-1', 'O+', '1985-06-15', '809-555-0001', 'Maria Diaz', '809-555-0002');

-- Insertar usuario Admin (password: Admin1234)
INSERT INTO Usuarios (id_empleado, username, password_hash, pin_hash, activo)
VALUES (1, 'admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.', NULL, 1);

/*
SELECT TABLE_NAME, COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS
ORDER BY TABLE_NAME;

--UPDATE Usuarios SET password_hash = '$2a$11$l6mgl2vRKK1p/EkbvDO1puZZUowAajt.zZuHkvCFXve0Q4ttmXHYK' WHERE id = 1

select * from Usuarios;
select * from Roles;
select* from Empleados;
select * from DatosSensibles;
delete  from empleados where id !=1;
delete  from Usuarios where id_empleado !=1;
SELECT num_documento FROM DatosSensibles;

delete  from empleados where id !=1;
delete from Usuarios where id !=1;
SELECT id, nombre FROM Permisos;
select * from Permisos;
/*
USE G_HUMAN_DB;
INSERT INTO Permisos (nombre) VALUES ('Ver datos sensibles');
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (3, 10);
INSERT INTO RolesPermisos (id_rol, id_permiso) VALUES (4, 10);
*/

SELECT e.nombre, ds.fecha_nacimiento 
FROM Empleados e
JOIN DatosSensibles ds ON ds.id_empleado = e.id;

update DatosSensibles set fecha_nacimiento='2026-03-18' where id_empleado=33;
update DatosSensibles set fecha_nacimiento='2026-03-19' where id_empleado=37;
*/
