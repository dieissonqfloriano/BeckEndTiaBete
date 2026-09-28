-- ============================================================
-- Dados para TESTE DE CARGA (não usar em produção)
--   10.000 usuários: carga1@teste.com ... carga10000@teste.com
--   senha de todos: Carga@123
--   30 dias x 4 refeições por usuário = 1.200.000 registros
-- Rodar DEPOIS do "dotnet ef database update".
-- Leva de 1 a 3 minutos. Para apagar, use 03-limpar-carga.sql.
-- ============================================================

USE glichelp;

SET SESSION cte_max_recursion_depth = 10001;

-- 1) Usuários (o hash é BCrypt custo 11 de "Carga@123", igual ao que a API gera)
INSERT INTO Usuarios
  (Name, Email, SenhaHash, TipoDiabetes, FatorSensibilidade, HgtAlvo, Idade, Celular,
   Role, Ativo, DesativadoEm, EmailConfirmado, TentativasCodigo, CriadoEm, AtualizadoEm)
WITH RECURSIVE seq(n) AS (
  SELECT 1 UNION ALL SELECT n + 1 FROM seq WHERE n < 10000
)
SELECT
  CONCAT('Usuário Carga ', n),
  CONCAT('carga', n, '@teste.com'),
  '$2a$11$d44V5630p.ALZuAamphcq.bimR60vVUQEEhueJt2chKqDDmMyyeua',
  ELT(1 + (n % 4), 'Tipo1', 'Tipo2', 'Gestacional', 'Outro'),
  30 + (n % 40),
  100 + (n % 30),
  18 + (n % 60),
  NULL,
  'Usuario', 1, NULL, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
FROM seq;

-- 2) Registros: 30 dias x 4 refeições para cada usuário de carga
INSERT INTO RegistrosGlicemia
  (UsuarioId, Data, Hora, Glicemia, GlicemiaAcimaDoLimite, Dose, Refeicao, Observacao, CriadoEm, AtualizadoEm)
WITH RECURSIVE dias(d) AS (
  SELECT 0 UNION ALL SELECT d + 1 FROM dias WHERE d < 29
),
refeicoes(m, nome, hora) AS (
  SELECT 0, 'CafeDaManha', '07:30:00' UNION ALL
  SELECT 1, 'Almoco',      '12:15:00' UNION ALL
  SELECT 2, 'Lanche',      '16:00:00' UNION ALL
  SELECT 3, 'Jantar',      '19:45:00'
)
SELECT
  u.Id,
  DATE_SUB(CURDATE(), INTERVAL dias.d DAY),
  refeicoes.hora,
  70 + ((u.Id * 7 + dias.d * 13 + refeicoes.m * 31) % 180),
  0,
  ((u.Id + dias.d + refeicoes.m) % 20) / 2,
  refeicoes.nome,
  NULL,
  UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
FROM Usuarios u
CROSS JOIN dias
CROSS JOIN refeicoes
WHERE u.Email LIKE 'carga%@teste.com';

ANALYZE TABLE Usuarios, RegistrosGlicemia;

SELECT
  (SELECT COUNT(*) FROM Usuarios)          AS usuarios,
  (SELECT COUNT(*) FROM RegistrosGlicemia) AS registros;
