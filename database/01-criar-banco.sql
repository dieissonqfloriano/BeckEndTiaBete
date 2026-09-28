-- ============================================================
-- GlicHelp / TiaBete — criação do banco e do usuário da aplicação
-- Rodar UMA vez, logado como root no MySQL Workbench.
-- Troque a senha antes de executar.
-- ============================================================

CREATE DATABASE IF NOT EXISTS glichelp
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

-- Usuário só para a API (sem poderes de administrador no servidor).
CREATE USER IF NOT EXISTS 'glichelp_app'@'localhost' IDENTIFIED BY 'TroqueEstaSenha!';

-- CREATE/ALTER/INDEX/DROP/REFERENCES são necessários para o "dotnet ef database update".
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, DROP, REFERENCES
  ON glichelp.* TO 'glichelp_app'@'localhost';

FLUSH PRIVILEGES;

-- O teste de carga usa até 100 conexões da API; o padrão do MySQL é 151.
-- Se aparecer "Too many connections", aumente (vale até reiniciar o MySQL):
-- SET GLOBAL max_connections = 300;
