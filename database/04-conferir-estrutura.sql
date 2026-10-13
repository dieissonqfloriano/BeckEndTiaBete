-- Confere se o banco ficou como planejado.
USE glichelp;

-- Tipos das colunas (não deve aparecer nenhum longtext)
SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'glichelp' AND TABLE_NAME IN ('Usuarios', 'RegistrosGlicemia')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- Índices (deve existir UX_Usuarios_Email e IX_RegistrosGlicemia_UsuarioId_Data_Hora)
SHOW INDEX FROM Usuarios;
SHOW INDEX FROM RegistrosGlicemia;

-- Regras (check constraints)
SELECT CONSTRAINT_NAME, CHECK_CLAUSE
FROM information_schema.CHECK_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = 'glichelp';

-- A consulta do histórico deve usar o índice composto (coluna "key")
EXPLAIN SELECT * FROM RegistrosGlicemia
WHERE UsuarioId = 1 AND Data BETWEEN CURDATE() - INTERVAL 30 DAY AND CURDATE()
ORDER BY Data, Hora;
