# Novo banco de dados — passo a passo

Tempo estimado: 20 a 30 minutos. Tudo pelo terminal do Visual Studio (PowerShell), na pasta raiz do back-end.

## 0. Pré-requisitos (uma vez)

```powershell
dotnet --version                     # precisa ser 8.x
dotnet tool install --global dotnet-ef   # se já tiver: dotnet tool update --global dotnet-ef
```

MySQL 8 rodando. Para o teste de carga, instale o k6: `winget install k6 --source winget`

## 1. Criar o banco e o usuário da aplicação

No MySQL Workbench, logado como root, abra e execute `database/01-criar-banco.sql`
(antes, troque `TroqueEstaSenha!` por uma senha sua).

## 2. Configurar os segredos locais

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=glichelp;User=glichelp_app;Password=SUA_SENHA;Pooling=true;MinimumPoolSize=10;MaximumPoolSize=100;ConnectionTimeout=5;" --project Presentation
dotnet user-secrets set "Jwt:Key" "COLOQUE-AQUI-UMA-CHAVE-ALEATORIA-COM-PELO-MENOS-32-CARACTERES" --project Presentation
```

Se o seu MySQL não for 8.0.x, ajuste `Database:ServerVersion` no `appsettings.json` (ex.: `8.4.0-mysql`).
Descubra a versão com `SELECT VERSION();`.

## 3. Gerar a migration nova e criar as tabelas

As migrations antigas foram apagadas de propósito (banco novo do zero).

```powershell
dotnet build
dotnet ef migrations add CriacaoInicial --project Infrastructure --startup-project Presentation
dotnet ef database update --project Infrastructure --startup-project Presentation
```

Opcional, para documentar: `dotnet ef migrations script -o database/schema.sql --project Infrastructure --startup-project Presentation`

## 4. Conferir a estrutura

No Workbench, execute `database/04-conferir-estrutura.sql`. Deve mostrar:
- nenhuma coluna `longtext`;
- índices `UX_Usuarios_Email` e `IX_RegistrosGlicemia_UsuarioId_Data_Hora`;
- 6 check constraints;
- no EXPLAIN, a coluna `key` usando `IX_RegistrosGlicemia_UsuarioId_Data_Hora`.

## 5. Rodar os testes automatizados

```powershell
dotnet test
```

(Os testes de login usam o banco configurado no passo 2, por isso o passo 3 vem antes.)

## 6. Teste de carga com 10.000 usuários

1. No Workbench, execute `database/02-seed-carga.sql` (10 mil usuários + 1,2 milhão de registros, ~1 min).
2. Suba a API em modo Release no ambiente de carga (limite de requisições alto):
   ```powershell
   dotnet run -c Release --project Presentation --launch-profile http --environment LoadTest
   ```
3. Em outro terminal, primeiro um aquecimento menor, depois o completo:
   ```powershell
   k6 run -e MAX_VUS=1000 load/k6-glichelp.js
   k6 run load/k6-glichelp.js
   ```
4. No resultado, olhe: `http_req_failed` (meta < 1%), `http_req_duration{tipo:leitura}` p(95) (meta < 300 ms),
   `http_req_duration{tipo:escrita}` p(95) (meta < 500 ms) e `erros_5xx` (meta 0).

Dicas:
- Se aparecer "Too many connections", rode no Workbench: `SET GLOBAL max_connections = 300;`
- k6 e API na mesma máquina disputam CPU; se o notebook saturar, o gargalo é a máquina, não o banco.
- Para apagar os dados de carga: `database/03-limpar-carga.sql`.
- Os scripts antigos `load-login.js` e `load-glicemia.js` mediam o rate limiter (um único usuário), por isso não servem para medir capacidade.

## O que mudou no código (resumo)

| Área | Mudança |
|---|---|
| Tabelas | Tipos certos (`varchar` com tamanho, `decimal`, `time(0)`), charset `utf8mb4`, datas `CriadoEm`/`AtualizadoEm`/`DesativadoEm` |
| Regras no banco | Check constraints: glicemia 20–600, "HI" sem valor numérico, dose 0–100, HGT alvo e fator 1–600, idade 1–120 |
| Índice | `(UsuarioId, Data, Hora)` em RegistrosGlicemia: histórico, dashboard e PDF em menos de 1 ms com 1,2 milhão de linhas |
| Dose e fator de sensibilidade | Agora `decimal` (aceita 4,5 U e fator 45,5) |
| Refeição e tipo de diabetes | Enums salvos como texto; a API aceita os textos do front, com ou sem emoji |
| E-mail | Salvo sempre em minúsculas e sem espaços |
| Senha | Propriedade renomeada para `SenhaHash` (a API continua recebendo `senha`) |
| Autenticação | "Usuário está ativo?" fica 2 min em cache; antes era uma consulta ao banco em TODA requisição |
| Leituras | `AsNoTracking`; listagem paginada (`?pagina=1&tamanho=100`, máx. 500) |
| Exclusões | Um único comando no banco (`ExecuteDelete`/`ExecuteUpdate`) |
| Conexões | `AddDbContextPool`, versão do MySQL fixa (não abre conexão ao iniciar), retry automático |
| PDF | Período máximo de 366 dias |
| Rate limiting | Limites no `appsettings.json`; ambiente `LoadTest` com limites altos |
| Testes | Novos testes em `Tests/NovoBancoTests.cs` |

## Contrato da API

Rotas iguais. Diferenças para o front:
- `GET /api/registro-glicemia` agora vem paginado e do mais recente para o mais antigo (padrão 100 itens).
- `refeicao` volta sem emoji (`"Almoço"`, `"Café da Manhã"`...); o front pode recolocar o emoji na exibição.
- `dose` e `fatorSensibilidade` podem vir com casa decimal.
