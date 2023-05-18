# Timesheet

APIs para lançamento de horas de usuários em projetos que atuaram.

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
A documentação dos endpoints pode ser acessada no arquivo: openapi30.json
Os exemplos de response das requisições podem ser acessados no arquivo: response_examples.txt
