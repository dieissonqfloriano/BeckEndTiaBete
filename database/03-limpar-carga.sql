-- Remove os usuários de carga (os registros saem junto pelo ON DELETE CASCADE).
USE glichelp;
DELETE FROM Usuarios WHERE Email LIKE 'carga%@teste.com';
