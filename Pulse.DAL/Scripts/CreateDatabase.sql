IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Pulse')
BEGIN
    CREATE DATABASE Pulse;
END
GO

USE Pulse;
GO