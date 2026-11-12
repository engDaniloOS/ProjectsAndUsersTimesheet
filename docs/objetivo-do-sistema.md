# Objetivo do Sistema

> Documento oficial de produto/objetivo do **TimeSheet API**. Base para Spec-Driven Development.
> Gerado a partir do código e do README em 2026-11-11. Inferências estão marcadas como **Hipótese**.
> Complementar a: [Arquitetura do Sistema](arquitetura-do-sistema.md).

## 1. Propósito principal

Fornecer uma **API REST para lançamento de horas de usuários em projetos em que atuaram** (fonte: README — "APIs para lançamento de horas de usuários em projetos que atuaram"). O sistema registra *quem* trabalhou em *qual projeto*, *quando começou* e *quando terminou*.

## 2. Problemas que resolve

- Centralizar o cadastro de **usuários** e **projetos** e o vínculo N:N entre eles.
- Registrar **intervalos de trabalho** (início/fim) por usuário e projeto, impedindo lançamento em projeto do qual o usuário não participa (regra verificada em `WorkTimeService.Save`).
- Permitir **consulta** dos lançamentos por projeto e correção posterior de lançamentos.
- Proteger o acesso por **autenticação** (JWT).

Fora do escopo atual (não existe no código): cálculo/total de horas, relatórios, aprovação de horas, faturamento, exclusão de qualquer entidade, listagem de usuários, perfis/papéis, front-end.

## 3. Atores

| Ator | Descrição | Evidência |
|---|---|---|
| **Usuário autenticado** | Qualquer usuário com credencial válida. Todos têm as mesmas permissões. | `[Authorize]` sem policies |
| **Administrador inicial** | Usuário `admin`/`admin` (`admin@vibbra.com.br`) criado no startup; funcionalmente igual aos demais — **não há papel de admin** | `DatabaseManagementService` |
| **Cliente da API** | Swagger (dev), Postman ou aplicação consumidora (**Hipótese**: front-end/integração externa não incluído no repositório) | README |
| **Sistema/operador** | Quem executa e configura a aplicação (local ou Docker) | README, `docker-compose.yml` |

## 4. Funcionalidades centrais

Todas sob `api/v1`, autenticadas por `Authorization: Bearer <token>` (exceto login).

| Funcionalidade | Endpoint | Resultado |
|---|---|---|
| Autenticar | `POST /authenticate` | Token JWT (expira em 15 min, configurável) + dados do usuário; `401` se credencial inválida |
| Consultar usuário (com seus projetos) | `GET /users/{ID}` | Usuário + projetos; `404` se não existe |
| Cadastrar usuário | `POST /users` | Cria usuário com credencial (login/senha) |
| Editar usuário | `PUT /users/{ID}` | Atualiza nome, e-mail, login e senha |
| Listar projetos | `GET /projects` | Projetos com usuários; `404` se vazio |
| Consultar projeto | `GET /projects/{project_id}` | Projeto + usuários |
| Cadastrar projeto | `POST /projects` | Cria projeto e vincula usuários (`user_id` = lista de ids) |
| Editar projeto | `PUT /projects/{project_id}` | Atualiza título, descrição e **substitui** os usuários vinculados |
| Lançar horas | `POST /times` | Cria lançamento (projeto, usuário, `started_at`, `ended_at`) |
| Listar horas de um projeto | `GET /times/{project_id}` | Lançamentos do projeto; `404` se vazio |
| Editar lançamento | `PUT /times/{time_id}` | Atualiza projeto, usuário, início e fim |

Contratos detalhados: `openapi30.json` e `response_examples.txt` na raiz.

## 5. Principais fluxos de negócio

1. **Primeiro acesso**: aplicação inicia → aplica migrações → cria `admin` → cliente faz `POST /authenticate` com `admin/admin` → recebe token.
2. **Montar a equipe**: autenticado, cria usuários (`POST /users`) → cria projeto informando os ids dos usuários (`POST /projects`).
3. **Registrar horas**: `POST /times` com `project_id`, `user_id`, `started_at`, `ended_at`. Só é aceito se o projeto existe, tem usuários e o `user_id` pertence ao projeto; caso contrário a API responde `404`.
4. **Consultar horas**: `GET /times/{project_id}` retorna os lançamentos com dados do projeto e do usuário.
5. **Corrigir dados**: `PUT` em usuário, projeto ou lançamento (sem histórico de alterações).

```
admin ─► authenticate ─► (token) ─► users / projects ─► times (valida vínculo usuário↔projeto)
```

## 6. Regras de negócio existentes (verificadas no código)

- Credenciais inválidas → `401`.
- Lançamento exige `project_id`, `user_id`, `started_at`, `ended_at` (`[Required]`).
- Lançamento só para usuário **vinculado ao projeto** (na criação; **não** verificado na edição — ver [Arquitetura §8, C3](arquitetura-do-sistema.md#8-riscos-técnicos-acoplamentos-e-violações)).
- Editar projeto substitui integralmente a lista de usuários.
- Usuário e credencial são criados/editados juntos (1:1).

**Regras ausentes (não existem no código; não assumir):** `ended_at > started_at`, sobreposição de intervalos, unicidade de login/e-mail, limites de horas, exclusão/arquivamento, papéis.

## 7. Visão de produto

- **Estado atual**: V1 funcional ("Timesheet V1"), desenvolvido manualmente (README: "sem uso de IA") como entrega de desafio técnico (**Hipótese**: o domínio `vibbra.com.br` do admin e o formato do README sugerem processo seletivo/desafio; não confirmado no código).
- **Pendências declaradas no README**: testes unitários, code review, doc review.
- **Evolução natural sugerida (Hipótese, não implementada)**: hash de senha e papéis, exclusão lógica, totais/relatórios por projeto/usuário, validação de intervalos, paginação, atualização do runtime .NET.

## 8. Contexto operacional

| Aspecto | Descrição |
|---|---|
| Execução local | .NET 5 SDK, `dotnet run` no projeto; banco **LocalDB em arquivo** (`TimeSheet.mdf`, somente Windows); Swagger em `http://localhost:5000/swagger` (ambiente `Development`) |
| Execução Docker | `docker-compose up --build` em `TimeSheet/`; sobe SQL Server 2019 (`:1433`) e a API (`:5000`→80); `Database_OnDocker=true` seleciona a connection string `Docker` |
| Banco | Criado/migrado automaticamente no startup (EF Core code-first) |
| Autenticação | JWT HS256, emissor/audiência `TimeSheet`, expiração 15 min (`appsettings.json`) |
| Observabilidade | Logs padrão ASP.NET + `Console.WriteLine` de exceções |
| Testes | Não há; testes manuais via Swagger/Postman |

## 9. Glossário

- **Credential**: par login/senha de um usuário.
- **WorkTime / "time"**: lançamento de horas (intervalo início–fim) de um usuário em um projeto.
- **Project**: projeto com título, descrição e usuários participantes.

## 10. Decisões de produto (entrevista de 2026-11-11)

Decisões do responsável pelo sistema. São **requisitos-alvo**: **ainda não implementados** no código, que continua conforme descrito nas seções 1–9.

| # | Tema | Decisão |
|---|---|---|
| D1 | Contexto | Desafio técnico/portfólio; sem usuários reais nem previsão de produção |
| D2 | Permissões | Acesso aberto: qualquer usuário autenticado pode acessar/editar qualquer dado (sem papéis) |
| D3 | Validação de horas | `ended_at > started_at`; sem sobreposição de intervalos do mesmo usuário; sem datas futuras; máximo de **8h somadas por dia** por usuário |
| D4 | Novas funcionalidades | Totais e relatórios; exclusão; listagem de usuários e filtros |
| D5 | Exclusão | **Em cascata**: apagar usuário ou projeto remove também as horas lançadas associadas |
| D6 | Fuso horário | Horário local do servidor (sem conversão para UTC) |
| D7 | Unicidade | Login e e-mail únicos; duplicidade → `409 Conflict`; entradas inválidas → `400` com mensagem clara |
| D8 | Contrato | Pode ser corrigido na própria v1, aceitando quebra de compatibilidade (sem clientes externos) |
| D9 | Segurança | Apenas documentar como limitação conhecida (ver [Arquitetura §8](arquitetura-do-sistema.md#8-riscos-técnicos-acoplamentos-e-violações)) |
| D10 | Plataforma | Migrar para .NET 10 (ver [Arquitetura §13](arquitetura-do-sistema.md#13-decisões-técnicas-e-backlog-de-especificações)) |

Efeitos sobre as seções anteriores: o "Fora do escopo atual" da seção 2 (totais/relatórios, exclusão, listagem de usuários) passa a ser **escopo planejado**. A ausência de papéis (seção 3) é uma decisão (D2), não uma pendência.

## 11. Lacunas abertas (requerem decisão antes de especificar)

### Negócio
| # | Pergunta | Impacto |
|---|---|---|
| N1 | A regra de não sobreposição vale entre projetos diferentes do mesmo usuário ou só dentro do mesmo projeto? Vale também na edição? | Define a consulta de validação e o `PUT /times/{time_id}` |
| N2 | O "dia" do limite de 8h é a data de `started_at`? Lançamento que cruza a meia-noite conta inteiro no dia de início ou dividido? | Algoritmo da regra D3 |
| N3 | Quais totais/relatórios (por projeto, usuário, período) e em qual formato (JSON/CSV)? | Novos endpoints e DTOs |
| N4 | Quais filtros, paginação padrão e ordenação nas listagens? | Contrato das listagens |
| N5 | A unicidade de login/e-mail diferencia maiúsculas/minúsculas? O `admin` seed segue a regra? | Índices e comparação |
| N6 | Quem pode excluir? Pode excluir a si próprio ou o `admin`? O que ocorre com a `Credential` do usuário excluído (hoje a FK `User→Credentials` é `Restrict`, deixaria registro órfão)? | Regra de exclusão (D5) |
| N7 | Na edição de horas, revalidar vínculo usuário↔projeto, sobreposição e limite diário? | Consistência com a criação |

## 12. Premissas e hipóteses vigentes
- **Hipótese**: o domínio `vibbra.com.br` do admin e o formato do README indicam desafio de processo seletivo (compatível com D1; não confirmado no código).
- **Hipótese**: dado D6, no Docker o "horário local" será o fuso do container (normalmente UTC).
