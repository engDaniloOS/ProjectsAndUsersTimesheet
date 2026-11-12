# Timesheet

APIs para lançamento de horas de usuários em projetos que atuaram.

### Projeto criado manualmente, sem uso de IA

## Activities

- [x] Estimativa de horas;
- [x] Estimativa de dias;
- [x] Entendimento do escopo;
- [x] Criação e configuração do repositório;
- [x] Inicialização do projeto em C# (.net 5);
- [x] Desenvolvimento do DB com Entity Framework Core 5;
- [x] Desenvolvimento das funcionalidades de autenticação;
- [x] Desenvolvimento das funcionalidades de projetos;
- [x] Desenvolvimento das funcionalidades de usuários;
- [x] Desenvolvimento das funcionalidades de horas;
- [x] Desenvolvimento do docker compose e dockerfile;
- [ ] Desenvolvimento de testes unitários;
- [ ] Code review
- [ ] Doc review

## Tecnologias

* 1 - O projeto foi desenvolvido na plataforma **.Net 5**, com a linguagem **C#**;
* 2 - Como ORM (Object Relational Mapping) foi utilizado o **Entity Framework (EF) Core 5**;
* 3 - O **SQL Server** foi escolhido como banco de dados;
* 4 - A configuração do DB foi feita em code first, utilizando o EF para criaçao das tabelas;
* 5 - O mapeamento entre objetos e dtos foi feito com o AutoMapper;
* 6 - Para testes locais foi utilizado banco de dados em arquivo, que consta no projeto;
* 7 - Para testes locais foi disponibilizado o Swagger;
* 8 - Para demais testes foi disponibilizado o dockerfile e docker-compose para executar a aplicação e DB em containeres;

## Decisões técnicas
* Devido ao tempo restrito para entrega das funcionalidades, o projeto não foi executado com TDD;
* Para melhor visualização e manutenção, o Desenvolvimento seguiu a clean architecture;

## Testes
* Ao ser executada pela primeira vez a aplicação irá construir as bases de dados, e independe da execução via docker ou local (arquivo de DB para ambientes windos);
* **Na primeira execução o usuário administrador será criado, com as credenciais Login: "admin" e Password: "admin". Esse usuário poderá ser utilizado para autenticação e acesso as APIs.**

## Testes locais
### Para testar a aplicação localmente:
- É necessário ter o .NET 5 SDK e AspNet 5 instalados no computador;
- Vale ressaltar que o banco de dados em arquivo só é compatível com sistemas windows;
- No diretório da solução (.sln) executar os comandos: "dotnet restore" - para baixar as dependências, "dotnet build" - para construir a aplicação, e no diretório do projeto "./Timesheet" (.csproj) executar o comando "dotnet run" para executar a aplicação.
- Ao executar a aplicação o swagger poderá ser acessado no endpoint "http://localhost:5000/swagger/index.html";

## Testes via Docker
### Para executar a aplicação em container:
- Com o docker instalado no ambiente, ir até o diretório do docker-compose;
- Executar o seguinte comando "docker-compose up --build", para montar as imagens do banco de dados SQL Server e da aplicação;
- Após a construção das imagens o docker irá iniciar os containeres do DB e App;
- Ps: Talvez alguns logs de exceções ocorram e sejam visualizados na aplicação. Isso pode ocorrer, uma vez que a aplicação poderá executar o EF para criação da tabela antes que o DB esteja completamente inicializado. Todavia, o container da app tentará executar até ter sucesso.** E isso não impactará na execução da aplicação**;
- Nesse cenário recomendo utilizar o Postman para realizar requisições aos endpoints. Não esquecer de adicionar um cabeçalho com a chave "Authorization", e valor "bearer token";
- A aplicação poderá ser acessada no endpoint "http://localhost:5000/";


## Endpoints
A documentação completa dos endpoints está em `openapi30.json`; exemplos de resposta em `response_examples.txt`. Todos sob `api/v1` e autenticados por `Authorization: Bearer <token>`, exceto o login.

| Recurso | Endpoints |
|---|---|
| Autenticação | `POST /authenticate` (token JWT, expira em 15 min) |
| Usuários | `GET /users/{ID}`, `POST /users`, `PUT /users/{ID}` |
| Projetos | `GET /projects`, `GET /projects/{project_id}`, `POST /projects`, `PUT /projects/{project_id}` |
| Horas | `POST /times`, `GET /times/{project_id}`, `PUT /times/{time_id}` |

Convenção de respostas: `400` com `{"message": ...}` em erro, `404` para recurso inexistente ou lista vazia, `401` para credencial inválida, `200` nos demais casos (inclusive criações).

Regra de negócio principal: um lançamento de horas só é aceito se o usuário pertence ao projeto (validado na criação, **não** na edição). Editar um projeto substitui toda a lista de usuários vinculados.

## Arquitetura
API monolítica em um único projeto (`TimeSheet/TimeSheet`), com camadas separadas por pastas:

| Pasta | Responsabilidade |
|---|---|
| `Configuration` | Composition root: DI, JWT, EF Core, AutoMapper, Swagger; migração e seed do admin no startup |
| `Connectors/Controllers` | Endpoints HTTP; traduzem o resultado do serviço em status HTTP |
| `Domain` | Serviços (regras de negócio), `UseCases` (interfaces), `Converters` (DTO ↔ entidade) e `Dtos` (contrato JSON em snake_case) |
| `DataProviders/Repository` | `DbContext` e repositórios (EF Core, SQL Server); banco local em `Infra/TimeSheet.mdf` |
| `Models` | Entidades: `User`, `Credential`, `Project`, `WorkTime` (User ⇄ Project é N:N) |
| `Migrations` | Migração EF `Initial` (gerada, não editar) |

Fluxo: `Controller → UseCase (interface) → Service → Repository → Context`, com `Converter`/DTO na fronteira. Os serviços não lançam exceção: devolvem um DTO com o campo `Error` preenchido.

Para adicionar uma funcionalidade: use case em `Domain/UseCases` → método no serviço → endpoint no controller → DTOs e `CreateMap` em `MapperProfile` → registro em `DependencyInjectionServiceConfig` (se novo serviço) → migração EF se o schema mudar.

## Dívidas técnicas conhecidas
- **Segurança**: senhas em texto puro; `SecurityKey` do JWT e senha `sa` versionadas; sem papéis/autorização por recurso (qualquer usuário autenticado edita qualquer dado); sem HTTPS redirection.
- **Seed do admin não é idempotente**: um novo `admin` é inserido a cada inicialização da aplicação (não só na primeira).
- **Arquitetura**: os contratos de repositório ficam em `DataProviders` e o `Domain` depende deles (inversão de dependência violada); entidades EF são usadas como modelo de domínio.
- **Validação**: só `CredentialDto` e `WorkTimeDto` têm `[Required]`; não há checagem de `ended_at > started_at`, nem de unicidade de login/e-mail; entradas inválidas viram `400` por exceção capturada.
- **Nomes**: `IProjectRepository.EditUser` edita projeto; classe `WortTimeConverter` com typo; `IGetWorkTimeById` é uma interface vazia.
- **Docker**: `ConnectionStrings__MyDatabase` no `docker-compose.yml` não é lida pelo código (a app usa `ConnectionStrings:Docker`); `dockerfile_old` não é usado; `ASPNETCORE_ENVIRONMENT` não é definido, então o Swagger (só em `Development`) provavelmente não fica disponível no container (hipótese).
- **Outros**: .NET 5 fora de suporte; logs via `Console.WriteLine`; sem testes automatizados.

## Documentação técnica detalhada
* [Arquitetura do Sistema](docs/arquitetura-do-sistema.md)
* [Objetivo do Sistema](docs/objetivo-do-sistema.md)
