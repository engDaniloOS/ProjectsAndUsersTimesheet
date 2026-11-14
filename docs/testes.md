# Testes

> Como testar o **TimeSheet API**: testes automatizados (`dotnet test`), coleção de testes de API no Insomnia e checklist de verificação manual.
> Complementa [Arquitetura do Sistema](arquitetura-do-sistema.md), [Objetivo do Sistema](objetivo-do-sistema.md) e a [spec](../specs/001-timesheet-api/spec.md).
> Estado em 2026-11-13. Cobertura atual: **apenas a User Story 2 (usuários)**; projetos e horas só têm testes de API (Insomnia), sem testes automatizados de código.

## 1. Visão geral

| Camada | Ferramenta | Onde | Roda sem infraestrutura? |
|---|---|---|---|
| Unidade e repositório | xUnit + NSubstitute + SQLite em memória | `TimeSheet/TimeSheet.Tests` | Sim |
| API (caixa-preta) | Insomnia (requests + Unit Tests) | [docs/insomnia/timesheet-insomnia.json](insomnia/timesheet-insomnia.json) | Não: precisa da API no ar |
| Verificações específicas de SQL Server | Manual | §4 | Não: precisa de SQL Server (LocalDB) |

Decisões (ver [Arquitetura §13.4](arquitetura-do-sistema.md)): sem testes automatizados dependentes de Docker/SQL Server; o que só o SQL Server consegue provar é verificado manualmente.

## 2. Testes automatizados (`TimeSheet.Tests`)

### Executar
No diretório da solução (`TimeSheet/`):

```
dotnet test
```

O projeto mira `net5.0` com `RollForward=LatestMajor`, então roda também em máquinas que só têm um runtime mais novo (ex.: .NET 10). Resultado atual: **62 testes passando**.

### Estrutura

| Pasta/arquivo | O que cobre |
|---|---|
| `Domain/UserServiceTests.cs` | Regras do `UserService` com repositório dublado: criar, consultar, id inexistente, login/e-mail duplicado (409), unicidade ignorando o próprio id, senha obrigatória na criação e opcional na edição, violação de unicidade vinda do repositório, erros inesperados |
| `Domain/UserDtoValidationTests.cs` | DataAnnotations de `UserDto`: obrigatórios, e-mail, tamanho máximo, senha opcional mas não em branco |
| `Connectors/UsersControllerTests.cs` | Mapeamento resultado → HTTP (`200`, `400`, `404`, `409`); `409` tem precedência sobre `400` |
| `Configuration/ApiBehaviorServiceConfigTests.cs` | Erros de validação viram `400` com `{"message"}` e a fábrica está registrada |
| `DataProviders/UserRepositoryTests.cs` | `ExistsLogin/ExistsEmail` (inclusive outra caixa e `ignoreUserId`), índices únicos, senha preservada na edição |
| `DataProviders/AdminSeedTests.cs` | `EnsureAdminUser` idempotente (inclusive `Admin` em outra caixa) |
| `Support/` | `SqliteDb` (banco em memória; `NOCASE` simula a collation CI do SQL Server) e `TestMapper` |

### Convenções
- Nome: `Metodo_Condicao_Resultado`.
- Asserções nativas do xUnit; dublês com NSubstitute.
- Repositórios: SQLite em memória com `EnsureCreated` (não usa as migrações, que são específicas de SQL Server).
- **Novo comportamento = teste primeiro** (TDD, conforme o fluxo SDD do projeto); a rastreabilidade com a spec está em [plan-us2-gerenciar-usuarios.md](../specs/001-timesheet-api/plan-us2-gerenciar-usuarios.md).

### Limites conhecidos
- Não cobrem: collation do SQL Server, tradução de erros `SqlException` 2601/2627, migrações, concorrência real (ver §4).
- Cobertura de projetos, horas e login ainda é zero.

## 3. Testes de API no Insomnia

Arquivo: [docs/insomnia/timesheet-insomnia.json](insomnia/timesheet-insomnia.json) (export v4, **30 requests** e **30 testes**).

### Importar
1. Insomnia → **Application → Import** (ou *Create → Import*) → selecione o arquivo → **Scan → Import**.
2. Abra o workspace **TimeSheet API**. O ambiente *Base Environment* traz `base_url` (`http://localhost:5000`), `admin_login` e `admin_password`; ajuste `base_url` se a API estiver em outra porta.
3. Suba a API: Docker (`docker-compose up --build -d` em `TimeSheet/`, porta `5000`) ou `dotnet run` (`http://localhost:5000`).

### Executar
- **Testes**: aba **Tests** do workspace → suíte **TimeSheet API** → *Run Tests*. Cada teste envia sua request e valida status e, quando relevante, o corpo.
- **Manualmente**: execute as pastas na ordem. As requests encadeiam dados das anteriores com *response tags* (ex.: o id do usuário criado, o token do login). Se não houver resposta anterior, a request de origem é enviada automaticamente.
- Os dados criados usam login/e-mail com timestamp, então a coleção pode rodar várias vezes sem colidir. Comece sempre por **Criar usuário**: as requests de login após a edição usam a senha criada nele (`secret123`), e **Editar trocando a senha** a altera para `secret456`.

### Conteúdo

| Pasta | Requests (esperado) |
|---|---|
| **1. Autenticação** | Login (200) · senha inválida (401, `Invalid credentials`) · `ADMIN` em maiúsculas (200) · sem campos (400) · endpoint protegido sem token (401) |
| **2. Usuários** | Criar (200) · login duplicado `ADMIN` e do usuário criado (409) · e-mail duplicado em outra caixa (409) · sem campos, e-mail inválido, sem senha, senha em branco (400) · consultar (200) e inexistente (404) · editar sem senha (200, mantém a senha: login com a antiga continua válido) · editar com senha em branco (400) · editar com login de outro usuário (409) · trocar a senha (200) · editar inexistente (404) |
| **3. Projetos** | Criar com dois usuários (200) · listar (200) · consultar (200) e inexistente (404) · editar (200) |
| **4. Horas** | Lançar (200) · usuário fora do projeto (404, comportamento atual, ver FR-006) · listar por projeto (200) · editar lançamento (200) |

### Validação da coleção
Como o Insomnia não foi executado na máquina de autoria, o JSON foi validado com um executor mínimo que interpreta as *response tags*, a autenticação Bearer e o código dos Unit Tests, rodando contra a API real (SQL Server LocalDB, banco descartável): **30/30 passaram**. Diferenças de versão do Insomnia (sintaxe de `insomnia.send()`, formato do export) não foram testadas; se a importação reclamar, a estrutura é o formato v4 padrão (`workspace`, `environment`, `request_group`, `request`, `unit_test_suite`, `unit_test`).

### Manutenção
- Ao mudar um endpoint, atualize a request e o teste correspondentes **junto** com `openapi30.json` e `response_examples.txt`.
- Quando as validações de horas (US4) forem implementadas, acrescente os cenários de `ended_at <= started_at`, data futura, sobreposição e limite de 8h/dia e ajuste o teste "usuário fora do projeto" se o código HTTP mudar.
- Testes devem refletir comportamento **existente**: não há asserções para regras ainda planejadas.

## 4. Verificação manual (SQL Server)

Executar quando alterar o `Context`, as migrações ou o tratamento de unicidade. Usa o LocalDB (`(localdb)\MSSQLLocalDB`) com um banco descartável, nunca o `.mdf` versionado.

1. Gerar o SQL: `dotnet ef migrations script 0 <migração anterior>` e `dotnet ef migrations script <anterior> <nova>` (a partir de `TimeSheet/TimeSheet`; com `DOTNET_ROLL_FORWARD=Major` se não houver runtime 5).
2. Criar um banco novo, aplicar o primeiro script e inserir dados sujos (duplicados de `admin` em outra caixa, e-mail repetido em outra caixa, nulos, credencial órfã, horas e vínculos nos duplicados).
3. Aplicar o segundo script e conferir: um registro por login/e-mail, horas e vínculos reatribuídos, sem credenciais órfãs, colunas `NOT NULL` com collation `SQL_Latin1_General_CP1_CI_AS` e índices únicos ativos.
4. Tentar inserir `ADMIN` / `ADMIN@VIBBRA.COM.BR`: o banco deve rejeitar (erro 2601).
5. Subir a API apontando para o banco (`Database_OnDocker=true` e `ConnectionStrings__Docker=...`), rodar a coleção do Insomnia e reiniciar a API duas vezes: deve continuar **um** `admin`.

Última execução: 2026-11-12, migração `UserUniqueLoginEmail` (ver "Resultado da implementação" no plano da US2).

## 5. Pendências de teste

| Item | Situação |
|---|---|
| Testes automatizados de projetos, horas e login | Não existem |
| Corrida real em gravações simultâneas (tradução de `SqlException`) | Só simulada com exceção; não exercitada com concorrência |
| Execução no runtime .NET 5 | Testes e API foram executados em .NET 10 (roll-forward) |
| Build Docker com o `dockerfile` ajustado | Imagem reconstruída durante o suporte ao Swagger; acesso ao Swagger **não** foi confirmado |
| Importação da coleção no Insomnia | Validada só por executor mínimo equivalente |
