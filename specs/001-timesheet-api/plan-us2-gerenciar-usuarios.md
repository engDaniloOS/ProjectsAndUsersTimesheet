# Implementation Plan: User Story 2 — Gerenciar usuários

**Branch**: `001-timesheet-api` | **Date**: 2026-11-12 | **Spec**: [spec.md](spec.md) (US2, FR-005a, FR-005b, FR-005c)
**Status**: **Implementado em 2026-11-12** (62 testes automatizados passando; migração e API verificadas em SQL Server LocalDB). Pendências: ver "Resultado da implementação" no fim.
**Revisão 2026-11-12**: D12 revisada — unicidade **sem** diferenciar maiúsculas/minúsculas (substitui a versão case-sensitive).

## Summary

A US2 está **[PARCIAL]**: criar, consultar (com projetos) e editar usuário funcionam, e `404` para id inexistente já é devolvido (cenários 1–3). Faltam:

- **Cenário 4 — unicidade**: login e e-mail únicos, **sem diferenciar maiúsculas de minúsculas**, `409 Conflict` (FR-005a, D12).
- **Cenário 5 — validação**: `name`, `email` (formato), `login` obrigatórios; `password` obrigatória **só na criação** (D11); `400` no formato `{"message": ...}` (FR-005b).
- **Cenário 6 — senha opcional na edição**: omitir `password` mantém a senha atual (D11). Corrige também o comportamento atual de gravar `null` (Arquitetura S5).
- **Pré-requisito — seed idempotente** (FR-005c): sem isso o índice único derruba o startup a cada reinício.

Abordagem: **anotações nos DTOs + checagem de unicidade no serviço (mensagem amigável) + índice único com collation case-insensitive explícita no banco (garantia final e proteção contra corrida) + migração que limpa os dados existentes**. Mantém os padrões atuais (DTO de resultado com campo `[JsonIgnore]`, "serviço não lança").

## Technical Context

| Item | Valor (verificado no código) |
|---|---|
| Runtime | C# / .NET 5; EF Core 5.0.17 (SQL Server). Migração para .NET 10 fica para depois (§Sequenciamento) |
| Migrações | Única (`Initial`), aplicada no startup (`Database.Migrate()`) |
| Testes | **Não existem** hoje |
| Erro | Serviço devolve DTO com `Error` → controller `400 ErrorDto`; `Id == 0` → `404` |
| Validação atual | `UserDto` sem atributos; `[ApiController]` só valida `CredentialDto`/`WorkTimeDto` e responde `ProblemDetails` (não `ErrorDto`) |
| Colunas | `User.Email`: `nvarchar(450)`, índice **não único**; `Credentials.Login`: `nvarchar(max)`, sem índice; ambas nuláveis |
| Collation | Padrão do servidor (**Hipótese**: `SQL_Latin1_General_CP1_CI_AS`, insensível a maiúsculas). Não depender disso: o plano fixa a collation explicitamente (P6) |

## Decisões de design

| # | Decisão | Origem |
|---|---|---|
| P1 | Unicidade garantida por **índice único**; checagem prévia no serviço dá a mensagem amigável; violação do índice (corrida) também vira `409` | — |
| P2 | Conflito via `[JsonIgnore] bool IsConflict` em `UserOutDto` (precedente: `IsCredentialInvalid`) | — |
| P3 | Validação com **DataAnnotations** em `UserDto` | — |
| P4 | `InvalidModelStateResponseFactory` customizado → `400` com `ErrorDto{message}`. **Efeito colateral intencional**: também muda o `400` de `POST /authenticate` e `POST /times` | D8, FR-018 |
| P5 | **`password` opcional no `PUT`**: omitida/`null` ⇒ mantém a senha atual; informada em branco/whitespace ⇒ `400`. Na criação: obrigatória (validada em `UserService.SaveNewUser`, que devolve `Error` ⇒ `400`) | **D11** |
| P6 | Unicidade **case-insensitive** (D12): collation **explícita** `SQL_Latin1_General_CP1_CI_AS` em `Credentials.Login` e `User.Email` via `Property().UseCollation(...)` (EF Core 5), aplicada **somente quando `Database.IsSqlServer()`**. Explícita para não depender do padrão do servidor. Consultas EF com `==` herdam a collation da coluna, então a checagem do serviço e o índice concordam. Nos testes, um `Context` derivado aplica `NOCASE` (SQLite) após `base.OnModelCreating`, pois o padrão do SQLite (`BINARY`) é case-sensitive | **D12**, T11 |
| P7 | Ordem em `EditUser`: **404 (usuário não existe) → 400 (validação) → 409 (conflito, ignorando o próprio usuário)** | — |
| P8 | Mensagens: `"Login already in use"` / `"Email already in use"` (reporta o primeiro conflito, login antes de e-mail) | — |
| P9 | Seed `admin/admin` mantido, mas só criado se não existir login `admin` (comparação case-insensitive: `Admin` também conta). Idempotência **não** é mudança de segurança (D9 preservada) | FR-005c |
| P10 | Repositório traduz violação de unicidade (`DbUpdateException` com `SqlException` 2601/2627) em exceção própria `UniqueConstraintViolationException`; o serviço captura **essa** exceção e devolve `IsConflict`. Isola o detalhe do provedor e torna o serviço testável sem SQL Server | T10 |
| P11 | `EnsureAdminUser(Context)` extraído como método público/estático de `DatabaseManagementService`, separado de `Migrate()`, para ser testável sem migrações | T10 |

### Consequências da case-insensitivity (D12)
- **Autenticação**: sem mudança de comportamento — `GetAuthenticateFrom` já compara com a collation padrão (insensível). Com a collation explícita na coluna `Login`, isso passa a ser **garantido** e não mais acidental.
- **Efeito sobre dados**: `Admin`/`admin` e `A@x.com`/`a@x.com` passam a ser o mesmo valor e conflitam.
- **Observação (fora de escopo, D9)**: `Credentials.Password` continua com a collation padrão do servidor; a comparação da senha segue insensível a maiúsculas. Não será alterado.

## Escopo

**Dentro**: unicidade login/e-mail, validação de `UserDto`, `password` opcional no `PUT`, formato uniforme de `400`, seed idempotente, migração com limpeza de dados, testes automatizados dos cenários 1–6, checklist de verificação manual, contrato e docs.

**Fora**: hash de senha e demais S1–S6 (D9); `DELETE`/listagem de usuários (US7/US8); validação de ids em projetos (US3); mensagens que expõem `ex.Message` (SC-005 global — registrar follow-up); migração para .NET 10.

## Arquivos afetados

```
TimeSheet/TimeSheet/
├─ Domain/Dtos/UserDto.cs                  # name/email/login [Required]; email [EmailAddress]; [MaxLength(450)]; password só não-branca se informada
├─ Domain/Dtos/UserOutDto.cs               # + [JsonIgnore] bool IsConflict
├─ Domain/UserService.cs                   # P5 (senha obrigatória na criação), P7, P10; corrigir "Erro when…" → "Error when…"
├─ DataProviders/Repository/Contracts/IUserRepository.cs   # + ExistsLogin / ExistsEmail (ignoreUserId)
├─ DataProviders/Repository/UserRepository.cs             # Exists*; EditUser não sobrescreve senha nula; P10
├─ DataProviders/Repository/UniqueConstraintViolationException.cs   # NOVO
├─ DataProviders/Repository/Context.cs     # Login/Email/Name required + MaxLength(450) + índice único + collation (P6)
├─ Connectors/Controllers/UsersController.cs   # 409 antes do ramo Error
├─ Configuration/DatabaseManagementService.cs  # P9, P11
├─ Configuration/ (nova classe) / Startup.cs   # P4
├─ Migrations/<timestamp>_UserUniqueLoginEmail.*  # gerada + limpeza SQL
TimeSheet.Tests/                               # NOVO (xUnit, NSubstitute, EF Core Sqlite 5.0.17)
openapi30.json, response_examples.txt, README.md, docs/, specs/…/spec.md
```

## Modelo de dados

| Item | Antes | Depois |
|---|---|---|
| `Credentials.Login` | `nvarchar(max)` nulável | `nvarchar(450)` `NOT NULL`, collation CI explícita, **índice único** |
| `User.Email` | `nvarchar(450)` nulável, índice simples | `NOT NULL`, collation CI explícita, **índice único** (substitui `IX_User_Email`) |
| `User.Name` | `nvarchar(450)` nulável | `NOT NULL` (índice simples mantido) |

### Migração com limpeza automática (D13)
Ordem dentro da migração (`migrationBuilder.Sql` + operações geradas):

1. **Preencher nulos**: `Login`, `Email`, `Name` nulos recebem placeholders determinísticos (`unknown-<Id>`, `unknown-<Id>@invalid.local`, `unknown-<Id>`). *Hipótese*: praticamente inexistentes; existem só porque nada validava antes.
2. **Mesclar duplicados de login** (comparação **case-insensitive**, `COLLATE SQL_Latin1_General_CP1_CI_AS` — `Admin` e `admin` contam como duplicados): manter o `User` de menor `Id` por login; reatribuir `WorkTime.UserId` do descartado para o mantido; reatribuir vínculos em `ProjectUser` (apagando antes os que gerariam chave duplicada `(ProjectsId, UsersId)`); apagar o `User` descartado e sua `Credential`.
3. **Mesclar duplicados de e-mail** com a mesma regra (menor `Id` vence). ⚠ Dois usuários com logins diferentes e o mesmo e-mail serão unificados e o login do descartado deixa de existir. Aceito conforme D13; **registrar no log da migração e no README**.
4. `AlterColumn` (tamanho, `NOT NULL`, collation) e `CreateIndex` únicos. **Revisar o SQL gerado** (`dotnet ef migrations script`) para confirmar que o EF descarta e recria os índices ao alterar a coluna.

`Down()` reverte apenas o schema; **os dados mesclados não são restaurados** (documentar).

## Contrato da API

| Endpoint | Mudança |
|---|---|
| `POST /users` | `name`, `email`, `login`, `password` obrigatórios; `email` com formato válido; `400` `{"message"}` e `409` `{"message"}` |
| `PUT /users/{ID}` | `name`, `email`, `login` obrigatórios; `password` **opcional** (omitida mantém); `400`/`409` |
| `GET /users/{ID}` | Sem mudança |
| `POST /authenticate` | `400` de validação passa a `{"message"}` (P4); comportamento de login inalterado (case-insensitive) |
| `POST /times` | `400` de validação passa a `{"message"}` (P4) |

Atualizar `openapi30.json` e `response_examples.txt`.

## Plano de execução (ordem obrigatória)

> Cada fase compila e termina com testes verdes. A Fase 4 só pode ir ao ar **depois** da Fase 1.

### Fase 0 — Rede de segurança
1. Criar `TimeSheet.Tests` (net5.0) na solução, referenciando `TimeSheet`. Pacotes: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `NSubstitute`, `Microsoft.EntityFrameworkCore.Sqlite` 5.0.17.
2. Testes de caracterização de `UserService` (repositório/converter dublados): cenários 1–3 no comportamento atual.
- **Verificação**: `dotnet test` verde sem mexer no código de produção.

### Fase 1 — Seed idempotente (FR-005c)
1. Extrair `EnsureAdminUser(Context)` (P11) e inserir só se não existir login `admin`.
2. Teste (SQLite in-memory, `EnsureCreated`): chamar duas vezes → uma linha `admin`.

### Fase 2 — Validação e formato de erro (cenários 5 e 6)
1. Anotar `UserDto` (P3, P5). `password`: `[RegularExpression(@".*\S.*")]` — `null` passa (ignorado), branco falha.
2. Implementar P4 em método estático testável (`BuildInvalidModelStateResponse`).
3. `UserService.SaveNewUser`: senha em branco/`null` ⇒ `Error = "The password field is required."`.
4. Testes: `Validator.TryValidateObject` por campo; resposta de P4 contém `message` agregada; criação sem senha ⇒ `Error`.

### Fase 3 — Unicidade no serviço e edição sem senha (cenários 4 e 6)
1. `IUserRepository.ExistsLogin/ExistsEmail(valor, ignoreUserId)` (comparação `==` ⇒ collation da coluna).
2. `UserOutDto.IsConflict`; `UserService.SaveNewUser`/`EditUser` (P7, P8); capturar `UniqueConstraintViolationException` (P10) ⇒ `IsConflict`.
3. `UserRepository.EditUser`: só atribuir `Credential.Password` se não nula (P5).
4. `UsersController`: `if (user.IsConflict) return Conflict(new ErrorDto{...})` **antes** do ramo `Error`.
5. Testes de serviço e controller (§Testes).

### Fase 4 — Banco: índice único, collation e migração
1. `Context.OnModelCreating`: `IsRequired`, `HasMaxLength(450)`, `HasIndex().IsUnique()`, e `UseCollation` condicionado a `Database.IsSqlServer()` (P6).
2. `dotnet ef migrations add UserUniqueLoginEmail` + limpeza SQL (passos 1–3 acima); revisar o script gerado.
3. `UserRepository.SaveNewUser/EditUser`: capturar `DbUpdateException` ⇒ se `SqlException` 2601/2627, lançar `UniqueConstraintViolationException` (P10).
4. Testes de repositório (SQLite): índice único rejeita duplicata exata; `Admin` e `admin` conflitam (contexto derivado com `NOCASE`); `ExistsLogin` ignora o próprio id.
5. **Checklist manual** (§Verificação manual).

### Fase 5 — Contrato e documentação
1. `openapi30.json` e `response_examples.txt`.
2. `README.md`: seed idempotente; limpeza automática na migração (com o aviso do passo 3).
3. `docs/arquitetura-do-sistema.md`: C1 e C5 resolvidos, S5 parcialmente resolvido (sem sobrescrita por `null`); `docs/objetivo-do-sistema.md` §6: regras novas.
4. `spec.md`: US2 e FR-005a/b/c de **[PARCIAL]/[PLANEJADO]** para **[EXISTE]**.

## Plano de testes automatizados (sem Docker nem SQL Server)

| Cenário da US2 | Teste | Nível |
|---|---|---|
| 1. Criar válido | `SaveNewUser` devolve usuário com Id e `login` | Unidade |
| 2. Consultar com projetos | `GetUserById` devolve projetos | Unidade |
| 3. Id inexistente | Consulta/edição ⇒ DTO vazio ⇒ `404`; editar inexistente **não** vira `409` (P7) | Unidade + controller |
| 4. Duplicidade | Criar/editar com login ou e-mail existente ⇒ `IsConflict` + mensagem; editar mantendo os próprios valores ⇒ ok; `UniqueConstraintViolationException` ⇒ `IsConflict`; `Admin` ≠ `admin` | Unidade + SQLite |
| 5. Obrigatórios | Cada campo ausente; e-mail malformado; criação sem senha ⇒ `400` com `message` | Validator + unidade |
| 6. Senha opcional | `PUT` sem `password` ⇒ senha preservada (SQLite); `password` branca ⇒ `400` | Unidade + SQLite |
| FR-005c | `EnsureAdminUser` duas vezes ⇒ um `admin` | SQLite |
| Controller | `409` quando `IsConflict`, `400` quando `Error`, `404` quando `Id == 0`, `200` caso contrário | Unidade |

**Não cobertos por teste automatizado** (dependem de SQL Server): collation `CI` explícita no banco, detecção `SqlException` 2601/2627, migração e limpeza de dados, formato `400` ponta a ponta. Cobertos pelo checklist abaixo.

### Verificação manual (executar a cada mudança em Fase 4; LocalDB, o mesmo usado no desenvolvimento local)
1. Fazer cópia do `Infra/TimeSheet.mdf` e inserir à mão: dois `admin`, um usuário com e-mail repetido, um com nulos, com `WorkTime` e `ProjectUser` ligados aos duplicados.
2. Rodar a aplicação: migração aplica sem erro; conferir por SQL que sobrou um registro por login/e-mail, que `WorkTime`/`ProjectUser` foram reatribuídos e que não há órfãos em `Credentials`.
3. Reiniciar a aplicação duas vezes: continua um único `admin`.
4. Via Swagger: criar um usuário e repetir login ou e-mail trocando só maiúsculas/minúsculas (`Maria`→`maria`, `a@x.com`→`A@x.com`) ⇒ `409`; `POST` sem campos ⇒ `400` com `message`; `PUT` sem `password` e depois autenticar com a senha antiga ⇒ `200`; autenticar com `Admin` ⇒ `200` (login insensível).
5. Banco novo (sem dados): migração completa aplica e os índices existem.

## Sequenciamento

- Concluir a US2 em .NET 5 **antes** da migração para .NET 10 (T1); a migração EF e os testes precisarão ser revistos na troca (Q2).
- Antes de US7 (exclusão): N6 depende do modelo de `Credential` deste plano.
- Independente das US4/US5.

## Riscos

| Risco | Prob. | Mitigação |
|---|---|---|
| Limpeza da migração mescla usuários distintos com mesmo e-mail (perde o login descartado) | Média no `.mdf` versionado | Aceito em D13; log, README e checklist manual |
| `Down()` não restaura dados | Certa | Documentar; fazer backup do `.mdf` antes |
| EF não descartar/recriar índice ao alterar coluna | Média | Revisar `migrations script`; ajustar SQL manualmente se preciso |
| Parte do SQL Server sem teste automatizado | Certa | Decisão T10; checklist manual obrigatório na Fase 4 |
| P4 altera `400` de outros endpoints | Certa | Intencional (D8) |
| Escopo cresce para hash de senha | Média | Fora de escopo (D9) |

## Definição de pronto

- [ ] Cenários 1–6 da US2 cobertos por testes automatizados passando (`dotnet test`).
- [ ] Checklist de verificação manual executado e registrado no PR.
- [ ] Dois startups seguidos ⇒ um único `admin`; criar/editar com login ou e-mail existente ⇒ `409`; entradas inválidas ⇒ `400` com `message`; `PUT` sem `password` mantém a senha.
- [ ] Migração aplica em banco novo e na cópia do `.mdf` com dados sujos.
- [ ] `openapi30.json`, `response_examples.txt`, README, `docs/` e `spec.md` atualizados.

## Resultado da implementação (2026-11-12)

Seguida a ordem TDD: testes escritos primeiro (46 de 62 falhando com assinaturas mínimas em produção), depois implementação até 62/62 verdes.

**Entregue**: `TimeSheet.Tests` (xUnit 2.4.2, NSubstitute 5.1.0, EF Core Sqlite 5.0.17); `UserDto` com validação; `ApiBehaviorServiceConfig` (400 com `message`); `UserService` (senha obrigatória na criação, conflito 409, ordem 404 → 400 → 409); `UserRepository` (`ExistsLogin/ExistsEmail`, senha opcional na edição, tradução de violação de unicidade); `Context` (login/e-mail/nome obrigatórios, `MaxLength(450)`, índices únicos, collation `CI` explícita só em SQL Server); `DatabaseManagementService.EnsureAdminUser` idempotente; migração `UserUniqueLoginEmail` com limpeza de dados; `openapi30.json`, `response_examples.txt`, README e docs atualizados.

**Verificação manual executada** (LocalDB, banco descartável, já removido):
- Migração aplicada sobre dados sujos (3 variações de `admin`, e-mail duplicado em outra caixa, nulos, credencial órfã, horas e vínculos nos duplicados): sobraram os usuários 1, 4 e 6, horas e vínculos reatribuídos, colunas `NOT NULL` com collation `SQL_Latin1_General_CP1_CI_AS`, índices únicos ativos e inserções duplicadas (`ADMIN`) rejeitadas pelo banco.
- API ponta a ponta contra esse banco: `409` (login e e-mail em outra caixa), `400` com `message` (campos ausentes, e-mail inválido, senha em branco), `PUT` sem senha preserva a senha (login com a antiga continua válido), `404` para id inexistente mesmo com login existente, `200` ao manter os próprios login/e-mail em outra caixa, `401` sem token, e dois reinícios mantêm um único `admin`.

**Desvios em relação ao plano**
- O `TimeSheet.Tests` mira `net5.0` com `RollForward=LatestMajor`, pois só há runtime .NET 10 instalado nesta máquina.
- O `dockerfile` passou a restaurar/publicar `TimeSheet/TimeSheet.csproj` explicitamente; sem isso `dotnet publish` na solução incluiria o projeto de testes. **Não foi testado o build Docker.**
- O `.mdf` versionado **não** foi migrado nem alterado; a verificação usou um banco criado para o teste.

**Não verificado**
- O caminho de corrida (duas gravações simultâneas) que depende de `SqlException` 2601/2627 traduzido em `UniqueConstraintViolationException`: o serviço é testado com a exceção simulada e o banco real rejeitou a duplicata, mas a tradução em si não foi exercitada com concorrência real.
- `GET/PUT` de usuário inexistente retorna `404` com corpo `ProblemDetails` (comportamento do ASP.NET para `NotFound()` sem corpo), não `ErrorDto`; é anterior a este trabalho e não foi alterado.
- A aplicação foi executada em runtime .NET 10 (roll-forward), não no .NET 5.

**Follow-ups**: mensagens que expõem `ex.Message` ao cliente (SC-005); validação de `ProjectDto` (US3); `TimeSheet.Tests` ainda cobre apenas a US2.
