# Arquitetura do Sistema

> Documento oficial de arquitetura do **TimeSheet API**. Base para Spec-Driven Development.
> Gerado a partir da análise do código em 2026-11-11 (commit base `ca462d7`).
> Seção 13 (decisões e backlog) refletem requisitos-alvo ainda não implementados.
> Convenção: afirmações sem marcação são verificáveis no código; inferências estão marcadas como **Hipótese**.
> Documento complementar: [Objetivo do Sistema](objetivo-do-sistema.md).

---

## 1. Visão arquitetural

O sistema é uma **API REST monolítica** em **ASP.NET Core 5 (C#, `net5.0`)**, composta por **um único projeto** (`TimeSheet/TimeSheet/TimeSheet.csproj`) e uma solução (`TimeSheet/TimeSheet.sln`). A separação em camadas é feita **por pastas/namespaces**, não por assemblies.

Persistência: **SQL Server** via **Entity Framework Core 5 (code-first)**. Autenticação: **JWT Bearer** (HS256). Mapeamento: **AutoMapper 12**. Documentação de API: **Swashbuckle (Swagger)**.

### Diagrama de camadas (dependências reais, extraídas dos `using`)

```
                 ┌───────────────────────────────┐
                 │ Program / Startup             │  ponto de entrada
                 └───────────────┬───────────────┘
                                 │ chama
                 ┌───────────────▼───────────────┐
                 │ Configuration  (composition   │──── referencia TODAS as camadas
                 │ root: DI, JWT, EF, AutoMapper)│
                 └───────────────────────────────┘

  HTTP ─► Connectors/Controllers ─► Domain/UseCases (interfaces)
                                        │ implementadas por
                                        ▼
                                  Domain/*Service ──► Domain/Converters ──► AutoMapper
                                        │                    │
                                        │ usa                │ usa
                                        ▼                    ▼
                          DataProviders/Repository      Domain/Dtos
                          (contratos + implementações)
                                        │
                                        ▼
                                  Context (EF Core) ──► SQL Server
                                        │
                                        ▼
                                     Models (entidades)  ◄── usadas por TODAS as camadas
```

Observação: **os contratos de repositório (`IXxxRepository`) vivem em `DataProviders/Repository/Contracts`**, e `Domain` depende dessa pasta. A seta é `Domain → DataProviders`, o inverso do que a *Clean Architecture* (declarada no README) prescreve. Ver [§8](#8-riscos-técnicos-acoplamentos-e-violações).

## 2. Padrões utilizados

| Padrão | Onde | Evidência |
|---|---|---|
| Layered / "Clean Architecture" por pastas | Todo o projeto | README ("seguiu a clean architecture"); pastas `Connectors`, `Domain`, `DataProviders`, `Models` |
| Use Case por interface (ISP) | `Domain/UseCases` | `IProjectService : IGetProjects, IGetProjectById, ISaveNewProject, IEditProject` (idem Users, Times) |
| Service (implementa os use cases) | `Domain/*Service.cs` | `ProjectService`, `UserService`, `WorkTimeService`, `LoginService` |
| Repository | `DataProviders/Repository` | Interface + implementação por agregado; todos sobre `Context` |
| DTO + Converter | `Domain/Dtos`, `Domain/Converters` | Controllers nunca expõem entidades; converters encapsulam AutoMapper |
| Composition Root estático | `Configuration/*ServiceConfig` | `Startup.ConfigureServices` delega para classes estáticas |
| Result-object com campo `Error` | DTOs de saída | Serviços não lançam: capturam exceção e devolvem DTO com `Error` preenchido |
| Code-first migrations | `Migrations/` | Migração única `20230407005152_Initial` |
| Migração automática no startup | `DatabaseManagementService` | `context.Database.Migrate()` em `Startup.Configure` |

## 3. Regras arquiteturais (como o código funciona hoje)

Estas regras são **observadas** no código e devem ser mantidas, exceto onde indicado como "meta" (ver §9).

1. **Controllers são finos**: dependem apenas de interfaces de `Domain/UseCases` e de DTOs. Traduzem o DTO de resultado em status HTTP (§5.2). Nenhum controller acessa repositório ou `Models`.
2. **Controllers não expõem entidades**: entrada/saída somente via `Domain/Dtos`.
3. **Serviços orquestram**: validam regras de negócio, chamam repositórios e converters; capturam `Exception`, registram via `Console.WriteLine` e devolvem DTO com `Error`.
4. **Repositórios retornam entidades EF** (`Models`) e encapsulam `Include/ThenInclude`.
5. **Converters são a única fronteira DTO ↔ entidade**, e injetam `IMapper`. O perfil único está em `Configuration/MapperProfile.cs`.
6. **Injeção de dependência**: tudo registrado como `Transient` em `DependencyInjectionServiceConfig`; `IMapper` é `Singleton`; `Context` é `Scoped` (padrão de `AddDbContext`).
7. **Todos os endpoints exigem JWT** (`[Authorize]`), exceto `POST /api/v1/authenticate` (`[AllowAnonymous]`).
8. **JSON em `snake_case`** definido por `[JsonPropertyName]` em cada DTO (não há política global).
9. **Rotas versionadas manualmente**: prefixo `api/v1/...` no atributo `[Route]`.

## 4. Convenções técnicas

- **Nomenclatura**: pastas e namespaces seguem `TimeSheet.<Pasta>`; interfaces com prefixo `I`; DTOs de entrada `XxxDto`, de saída `XxxOutDto`; campos privados `_camelCase`.
- **Assincronia**: toda operação de I/O é `async/await` e retorna `Task<T>`.
- **Erros**: mensagem em inglês (com ocorrências de "Erro" em vez de "Error" em `UserService` e `WorkTimeService`), prefixo descritivo + `ex.Message`.
- **Not found**: convenção implícita — serviço devolve DTO com `Id == 0` e o controller responde `404`. Em listas, `Count == 0` → `404`.
- **Configuração**: `appsettings.json` (seções `ConnectionStrings`, `Authentication`) + variável de ambiente `Database_OnDocker` para escolher a connection string.
- **Estilo**: *expression-bodied constructors* para injeção de dependência única; arquivos com BOM UTF-8.
- **Idioma**: código e mensagens em inglês; README em português.

## 5. Separação de responsabilidades e fluxo de comunicação

### 5.1 Responsabilidades por camada

| Camada (pasta) | Responsabilidade | Não deve conter |
|---|---|---|
| `Connectors/Controllers` | Roteamento HTTP, autorização por atributo, mapeamento resultado → status | Regra de negócio, acesso a dados |
| `Domain/UseCases` | Contratos das operações de negócio | Implementação |
| `Domain/*Service` | Regras de negócio, orquestração, tratamento de erro, emissão de JWT (`LoginService`) | Consultas EF |
| `Domain/Converters` | Tradução DTO ↔ entidade | Regras de negócio |
| `Domain/Dtos` | Contrato de I/O da API (JSON) | Lógica |
| `DataProviders/Repository` | Persistência, consultas, transações | Regra de negócio |
| `Models` | Entidades persistidas | Lógica |
| `Migrations` | Evolução do schema (gerado pelo EF) | Edição manual |
| `Configuration` | Registro de DI, JWT, EF, Swagger, AutoMapper; migração e seed no startup | Regra de negócio |

### 5.2 Fluxo de uma requisição (exemplo: `POST /api/v1/times`)

```
Cliente ──JWT──► UseAuthentication/UseAuthorization
  ─► TimesController.SaveNewTime(WorkTimeDto)
  ─► IWorkTimeService.Save  (WorkTimeService)
       ├─ IProjectRepository.GetProjectById  (valida projeto e que o usuário pertence a ele)
       ├─ IWorkTimeConverter.ConvertFrom(dto) → entidade
       └─ IWorkTimeRepository.Save → recarrega com Project/User/Credential
  ─► IWorkTimeConverter.ConvertFrom(entidade) → WorkTimeOutDto
  ─► Controller: Error→400 | Id==0→404 | senão 200
```

Mapeamento resultado → HTTP (padrão em todos os controllers):

| Condição no DTO retornado | HTTP |
|---|---|
| `Error` preenchido | `400 BadRequest` com `ErrorDto{message}` |
| `Id == 0` / lista vazia | `404 NotFound` (corpo vazio) |
| `IsCredentialInvalid` (login) | `401 Unauthorized` |
| Caso contrário | `200 OK` |

Nota: não existe `201 Created`/`204`; criações retornam `200`.

### 5.3 Dependências entre módulos

| Módulo | Depende de |
|---|---|
| `Connectors` | `Domain/UseCases`, `Domain/Dtos` |
| `Domain/*Service` | `Domain/UseCases`, `Domain/Converters`, `Domain/Dtos`, `DataProviders/Repository` (contratos), `Models`; `LoginService` também `IConfiguration` e `System.IdentityModel.Tokens.Jwt` |
| `Domain/Converters` | `Domain/Dtos`, `Models`, `AutoMapper` |
| `DataProviders` | `Models`, `EF Core` |
| `Models` | (nenhuma) |
| `Configuration` | todos os módulos acima + ASP.NET Core, EF Core, JWT, Swashbuckle, AutoMapper |
| `Migrations` | `DataProviders/Repository/Context`, `EF Core` |

## 6. Modelo de dados (extraído de `Context.cs` e da migração)

```
Credentials(Id PK, Login nvarchar(max), Password nvarchar(max))
User(Id PK, Name ix, Email ix, CredentialId FK→Credentials)
Project(Id PK, Title ix, Description)
ProjectUser(ProjectsId, UsersId)   -- N:N User ⇄ Project, cascade
WorkTime(Id PK, ProjectId FK ix, UserId FK ix, Start, End)  -- cascade em ambos FKs
```

Não há índice/constraint **único** para `Credentials.Login` nem `User.Email`.

## 7. Dependências críticas

| Dependência | Versão | Observação |
|---|---|---|
| .NET / ASP.NET Core | 5.0 (`net5.0`) | **Fora de suporte** pela Microsoft (fim em maio/2022) |
| EF Core (+SqlServer, Design, Tools) | 5.0.17 | Acoplada à versão do runtime |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 5.0.17 | Autenticação |
| AutoMapper | 12.0.1 | Perfil único; `IMapper` singleton |
| Swashbuckle.AspNetCore | 5.6.3 | Habilitado **apenas** em `Development` |
| SQL Server | LocalDB (arquivo `.mdf`, só Windows) ou `mssql/server:2019-latest` (Docker) | Dois modos selecionados por `Database_OnDocker` |
| Docker / docker-compose | compose `3.9` | `TimeSheet/docker-compose.yml`, `TimeSheet/dockerfile` |

## 8. Riscos técnicos, acoplamentos e violações

Classificação: **[S]** segurança, **[A]** arquitetura, **[C]** correção/comportamento, **[O]** operação. Todos verificados no código.

### Segurança
- **[S1] Senhas em texto puro**: `Credential.Password` é gravada e comparada literalmente (`CredentialRepository.GetAuthenticateFrom`, `UserRepository.SaveNewUser/EditUser`).
- **[S2] Segredos versionados**: `SecurityKey` do JWT e senha `sa` do SQL Server estão em `appsettings.json` e `docker-compose.yml`.
- **[S3] Usuário `admin/admin`** criado automaticamente (README documenta isso como funcionalidade).
- **[S4] Sem autorização por recurso/papel**: qualquer usuário autenticado pode listar/editar qualquer usuário, projeto e lançamento de horas (`[Authorize]` sem policy; o JWT não carrega `user_id` nem papel — apenas `Name` e `Email`).
- **[S5]** `Edit` de usuário sobrescreve `Credential.Password` com o valor do DTO (se `null`, grava `null`).
- **[S6] Sem HTTPS redirection** (`UseHttpsRedirection` comentado) e `AllowedHosts: *`.

### Arquitetura
- **[A1] Inversão de dependência violada**: contratos de repositório estão em `DataProviders`; `Domain` depende da camada de infraestrutura. Em Clean Architecture a interface pertence ao domínio.
- **[A2] Entidades EF = modelo de domínio** (`Models` é anêmico e acoplado ao EF por convenção); serviços e converters manipulam entidades EF diretamente (inclusive navegação `User.Credential.Login`).
- **[A3] Serviços dependem de `IConfiguration` e de JWT** (`LoginService`), misturando regra de negócio e infraestrutura de segurança.
- **[A4] Serviço dependendo de repositório de outro agregado**: `WorkTimeService` usa `IProjectRepository`.
- **[A5] Projeto único**: limites de camada não são impostos pelo compilador; qualquer `using` cruza fronteiras.
- **[A6] `Configuration` conhece todas as camadas** (esperado em composition root, mas inclui regra: seed do admin em `DatabaseManagementService`).
- **[A7] Duplicação do segredo JWT**: lido separadamente em `AuthenticatorServiceConfig` (validação) e `LoginService` (emissão).
- **[A8] Sem testes automatizados** (item pendente no README; nenhum projeto de teste na solução).

### Correção / comportamento
- **[C1] Seed do admin não é idempotente**: `InitializeAdminUser` insere um novo `admin` **a cada inicialização** da aplicação (sem verificação de existência e sem índice único) → duplicatas em reinícios. O README afirma criação "na primeira execução".
- **[C2] Erros de entrada viram `400` via exceção**: ausência de validação (ex.: `ProjectDto.UsersIds == null`, `user_id` inexistente → `null` adicionado à coleção → exceção capturada). `UserDto`/`ProjectDto` não têm `[Required]`; só `CredentialDto` e `WorkTimeDto` têm.
- **[C3] Validação assimétrica de lançamento de horas**: `Save` valida que o usuário pertence ao projeto; `EditWorkTime` **não** valida (permite trocar projeto/usuário livremente). Nenhum dos dois valida `Start < End`.
- **[C4] `Save` retorna `WorkTimeOutDto` vazio para "projeto/usuário inválido"**, que o controller traduz em `404` — indistinguível de outros casos.
- **[C5] Duplicidade de login/e-mail não é verificada** ao criar/editar usuário.
- **[C6] Nome enganoso**: `IProjectRepository.EditUser` edita um **projeto**.
- **[C7] Tipos inconsistentes**: `WorkTime.Id` é `int`, mas `EditWorkTime` recebe `long`.
- **[C8] `UserRepository.GetUserByCredentialId` não inclui `Credential`**; `LoginService` lê `user.Credential.Login` — depende de *lazy/relationship fix-up* do EF (**Hipótese**: funciona porque `Credential` já foi rastreada na consulta anterior do mesmo `DbContext` scoped; é frágil).
- **[C9] Expiração do token** usa `DateTime.Now` (não UTC); `ExpiresIn` está em minutos (15).
- **[C10] Semântica inconsistente de respostas**: `GET /projects` retorna array puro; `GET /times/{id}` retorna objeto `{ "time": [...] }`; lista vazia → `404`.

### Operação
- **[O1] Migração + seed dentro de `Configure`**: no Docker a app pode subir antes do SQL Server; depende de `restart: on-failure` (documentado no README).
- **[O2] `docker-compose.yml` define `ConnectionStrings__MyDatabase`** (chave que o código **não** lê; o código lê `ConnectionStrings:Docker`, de `appsettings.json`) e com host `dbN` (inexistente). Variável sem efeito.
- **[O3] Swagger só em `Development`**; o compose não define `ASPNETCORE_ENVIRONMENT` (**Hipótese**: na imagem `aspnet:5.0` o padrão é `Production`, logo o Swagger não fica disponível em Docker, ao contrário do que o README sugere para execução local).
- **[O4] Logging via `Console.WriteLine`** nos serviços (sem `ILogger`).
- **[O5] Arquivos binários de banco (`TimeSheet.mdf/.ldf`) versionados** e copiados para a imagem Docker (`COPY . .`).
- **[O6]** `dockerfile_old` não é referenciado em nenhum lugar (órfão).

## 9. Diretrizes para futuras implementações

Regras de **meta** (alvo) para novas features, de forma a não aumentar a dívida:

1. **Nova funcionalidade = novo(s) use case(s)** em `Domain/UseCases/<Agregado>/` + método no serviço + endpoint no controller + DTOs + entrada no `MapperProfile` + registro em DI (se novo serviço).
2. **Não acessar repositórios ou `Models` em controllers.** Não colocar regra de negócio em controllers/repositórios.
3. **Novos contratos de repositório**: preferir criá-los na camada `Domain` (ou ao menos não ampliar o acoplamento atual); migrar os existentes é débito (A1).
4. **Validação de entrada explícita** (DataAnnotations ou FluentValidation) devolvendo `400` com mensagem clara, em vez de depender de exceções.
5. **Retornar semântica HTTP correta**: `201` em criação, `404` apenas para recurso inexistente, `409` para conflito (ex.: login duplicado), `403` para autorização negada.
6. **Segurança antes de novas features de usuário**: hash de senha (ex.: BCrypt/PBKDF2/`PasswordHasher`), segredos fora do repositório (variáveis de ambiente/User Secrets), `user_id` como claim, policies de autorização.
7. **Toda mudança de schema via migração EF** (`dotnet ef migrations add`); não editar migrações aplicadas.
8. **Seed idempotente** (verificar existência antes de inserir) e fora do caminho crítico de `Configure`.
9. **Usar `ILogger<T>`** em vez de `Console.WriteLine`.
10. **Testes**: toda nova regra de serviço acompanhada de teste unitário (serviços são testáveis via mocks de repositório/converter). Criar projeto de testes na solução.
11. **Atualizar a documentação** (este documento, [Objetivo](objetivo-do-sistema.md), `openapi30.json`, `response_examples.txt` e o `README.md` raiz) no mesmo PR da mudança.
12. **Plano de upgrade de runtime**: migrar de .NET 5 para versão LTS suportada; avaliar `Startup` → *minimal hosting*.

## 10. Estrutura de pastas

```
/
├─ README.md                    visão geral e instruções de execução
├─ openapi30.json               especificação OpenAPI 3.0 dos endpoints
├─ response_examples.txt        exemplos de resposta
├─ docs/                        documentação global (este diretório)
└─ TimeSheet/
   ├─ TimeSheet.sln
   ├─ dockerfile, dockerfile_old, docker-compose.yml
   └─ TimeSheet/                (projeto web)
      ├─ Program.cs, Startup.cs, appsettings*.json, Properties/
      ├─ Configuration/         composition root
      ├─ Connectors/Controllers/ entrada HTTP
      ├─ Domain/                serviços, use cases, converters, DTOs
      ├─ DataProviders/Repository/ persistência (EF Core)
      ├─ Models/                entidades
      └─ Migrations/            migrações EF
```

## 11. Pontos de entrada

| Ponto | Arquivo |
|---|---|
| Processo | `TimeSheet/TimeSheet/Program.cs` (`Main`) |
| Pipeline HTTP e DI | `TimeSheet/TimeSheet/Startup.cs` |
| API HTTP | `Connectors/Controllers/*Controller.cs` |
| Container | `TimeSheet/dockerfile` (`ENTRYPOINT dotnet TimeSheet.dll`) |

## 12. Módulos órfãos e código morto

- `Domain/UseCases/Times/IGetWorkTimeById.cs`: interface **vazia**, herdada por `IWorkTimeService`, sem método nem implementação (não existe endpoint para buscar um lançamento por id).
- `TimeSheet/dockerfile_old`: não referenciado por compose nem README.
- `Domain/Dtos/ProjectOutDto.Error`, `UserWithProjectsOutDto` (via herança): `Error` existe, mas só é preenchido em alguns caminhos.
- `appsettings.Development.json`: contém apenas `Logging` (sem sobrescrever outras chaves).
- Nenhum módulo de código completo está órfão: todos os serviços/repositórios/converters são registrados em DI e alcançáveis por um controller.

## 13. Decisões técnicas e backlog de especificações

Decisões de 2026-11-11 (origem: entrevista; ver [Objetivo §10](objetivo-do-sistema.md#10-decisões-de-produto-entrevista-de-2026-11-11)). **Nenhuma está implementada**; as seções 1–13 descrevem o código atual.

### 13.1 Decisões
| # | Decisão | Itens do §8 afetados |
|---|---|---|
| T1 | Migrar para **.NET 10** (runtime, pacotes, Dockerfile) | Dependências (§7), O3, O5 |
| T2 | Fuso = horário local do servidor; `DateTime.Now` permanece | C9 (passa a ser comportamento aceito) |
| T3 | Login e e-mail únicos (índice único no banco + `409`) | C1, C5 |
| T4 | Validações de horas: `Start < End`, sem sobreposição, sem datas futuras, ≤ 8h/dia por usuário | C3 |
| T5 | Exclusão em cascata (já é o comportamento das FKs de `WorkTime` e `ProjectUser`) | Migrations |
| T6 | Contrato da v1 pode ser corrigido com quebra | C10, C4 |
| T7 | Dívidas de segurança **apenas documentadas** (S1–S6 permanecem) | S1–S6 |
| T8 | Sem papéis: não implementar autorização por recurso | S4 (aceito) |

### 13.2 Backlog de especificações (candidatas a specs SDD)
1. **Validações de lançamento de horas** (T4): regras, mensagens, códigos HTTP e aplicação em `POST` e `PUT /times`. Depende de N1, N2, N7.
2. **Unicidade de usuário** (T3): migração com índices únicos; `409`; tratamento do seed (conflita com o `admin` duplicado do C1 → requer seed idempotente ou limpeza prévia). Depende de N5.
3. **Exclusão** de usuário/projeto/lançamento (`DELETE`): cascata (T5), tratamento da `Credential` órfã (N6).
4. **Listagem de usuários e filtros/paginação** (N4).
5. **Totais e relatórios** (N3).
6. **Correção do contrato v1** (T6): `201` em criação, lista vazia como `200 []`, formato uniforme de listas (`GET /times` retorna `{"time":[...]}`; `GET /projects` retorna array), renomear `ProjectDto.user_id` (lista) se desejado, e atualizar `openapi30.json`/`response_examples.txt`.
7. **Migração para .NET 10** (T1): avaliar *minimal hosting* (remover `Startup`), EF Core 10 (regerar/ajustar migração e snapshot), `JwtBearer`, AutoMapper e Swashbuckle compatíveis, imagens Docker `sdk/aspnet` 10, e a viabilidade do banco em arquivo LocalDB.
8. **Testes automatizados**: projeto de testes na solução (framework a definir — pendente).
9. **Logging** com `ILogger` em vez de `Console.WriteLine` (pendente de decisão).

### 13.3 Lacunas técnicas abertas
| # | Pergunta |
|---|---|
| Q1 | Migrar junto para *minimal hosting* ou manter `Program`/`Startup`? |
| Q2 | O EF Core 10 exige regerar a migração `Initial`, ou apenas atualizar o snapshot? (**Hipótese**: ajustar o snapshot basta; verificar na migração) |
| Q3 | ~~Como aplicar o seed idempotente sem violar T7?~~ *Resolvida (§13.4): idempotência não é mudança de segurança; `admin/admin` permanece* |
| Q4 | ~~Framework de testes~~ *Resolvida (§13.4): xUnit + NSubstitute + SQLite in-memory; sem testes dependentes de Docker* |
| Q5 | `Console.WriteLine` permanece ou passa a `ILogger`? |
| Q6 | Banco em arquivo (LocalDB, só Windows) continua suportado após a migração? |

### 13.4 Decisões adicionais (2026-11-12, planejamento da US2)

| # | Decisão |
|---|---|
| T9 | Testes: **xUnit** + **NSubstitute** (evita o histórico de controvérsia do Moq) + asserções nativas do xUnit (sem FluentAssertions, licença comercial na v8) |
| T10 | Testes de repositório com **SQLite in-memory**; **não** haverá testes automatizados dependentes de SQL Server em container (decisão do responsável). Partes específicas do SQL Server (collation, códigos de erro 2601/2627, migração) têm **checklist de verificação manual** |
| T11 | Unicidade com collation **case-insensitive explícita** (`SQL_Latin1_General_CP1_CI_AS`) em `Credentials.Login` e `User.Email`, aplicada por propriedade só quando o provedor é SQL Server (não depende da collation padrão do servidor). Testes SQLite usam `NOCASE` via contexto derivado |
| T12 | A migração de unicidade faz limpeza automática de dados (mescla duplicados, preenche nulos) |
