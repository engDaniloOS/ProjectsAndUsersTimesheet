# Feature Specification: TimeSheet API — lançamento de horas em projetos

**Feature Branch**: `001-timesheet-api`
**Created**: 2026-11-11
**Status**: Draft
**Input**: Sistema existente (V1) + decisões da entrevista de 2026-11-11, registradas em [docs/objetivo-do-sistema.md](../../docs/objetivo-do-sistema.md) §10–11 e [docs/arquitetura-do-sistema.md](../../docs/arquitetura-do-sistema.md) §13.

> **Como ler**: cada história indica **[EXISTE]** (já implementada na V1, comportamento verificado no código), **[PARCIAL]** (existe, mas diverge do requisito desejado) ou **[PLANEJADO]** (decidido, ainda não implementado). Esta spec descreve **o quê** e **por quê**, sem detalhes de implementação; decisões técnicas ficam em `docs/arquitetura-do-sistema.md`.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Autenticar-se (Priority: P1) **[EXISTE]**

Um usuário informa login e senha e recebe um token temporário para usar nas demais operações.

**Why this priority**: Todas as demais operações exigem autenticação; sem ela nada funciona.

**Independent Test**: Chamar o login com credenciais válidas e inválidas e usar o token retornado numa operação protegida.

**Acceptance Scenarios**:

1. **Given** um usuário cadastrado, **When** envia login e senha corretos, **Then** recebe um token e seus dados (id, nome, e-mail, login).
2. **Given** credenciais incorretas, **When** tenta autenticar, **Then** recebe resposta de não autorizado com a mensagem "Invalid credentials".
3. **Given** um token expirado (15 min) ou ausente, **When** acessa qualquer operação protegida, **Then** o acesso é negado.
4. **Given** a aplicação iniciada pela primeira vez, **When** o usuário `admin` tenta autenticar com a senha padrão, **Then** a autenticação é bem-sucedida.

---

### User Story 2 - Gerenciar usuários (Priority: P1) **[EXISTE]**

Um usuário autenticado cadastra, consulta e edita usuários (nome, e-mail, login, senha). Consultar um usuário mostra também os projetos em que ele atua.

**Why this priority**: Usuários são pré-requisito para projetos e lançamentos.

**Independent Test**: Criar um usuário, consultá-lo por id, editá-lo e verificar os dados atualizados.

**Acceptance Scenarios**:

1. **Given** dados válidos, **When** cria um usuário, **Then** o usuário é retornado com seu identificador.
2. **Given** um usuário existente, **When** é consultado, **Then** retorna seus dados e a lista de projetos.
3. **Given** um id inexistente, **When** consulta ou edita, **Then** recebe "não encontrado".
4. **Given** um login ou e-mail já usado por outro usuário, **When** cria ou edita um usuário com esse valor, **Then** a operação é rejeitada com conflito.
5. **Given** `name`, `email` ou `login` ausentes (ou `email` malformado), ou `password` ausente **na criação**, **When** cria/edita, **Then** recebe erro de validação com mensagem clara.
6. **Given** um usuário existente, **When** edita sem informar `password`, **Then** a senha atual é mantida; se `password` for informada em branco, a edição é rejeitada.

---

### User Story 3 - Gerenciar projetos e sua equipe (Priority: P1) **[EXISTE]**

Um usuário autenticado cria, lista, consulta e edita projetos (título, descrição) e define quais usuários participam. Editar um projeto substitui toda a lista de participantes.

**Why this priority**: O vínculo usuário↔projeto é a base da regra de lançamento de horas.

**Independent Test**: Criar projeto com dois usuários, consultá-lo, editá-lo com um só usuário e verificar a substituição.

**Acceptance Scenarios**:

1. **Given** usuários existentes, **When** cria um projeto informando seus ids, **Then** o projeto é retornado com os usuários vinculados (com login).
2. **Given** projetos cadastrados, **When** lista projetos, **Then** recebe todos com seus usuários.
3. **Given** um projeto existente, **When** edita título, descrição e ids de usuários, **Then** a lista de usuários passa a ser exatamente a informada.
4. **Given** um id de projeto inexistente, **When** consulta ou edita, **Then** recebe "não encontrado".
5. **Given** um id de usuário inexistente na lista, **When** cria/edita o projeto, **Then** a operação é rejeitada com mensagem clara *(PLANEJADO)*.

---

### User Story 4 - Lançar horas trabalhadas (Priority: P1) **[PARCIAL]**

Um usuário autenticado registra um intervalo de trabalho (início e fim) de um usuário em um projeto do qual ele participa, e pode corrigi-lo depois.

**Why this priority**: É a finalidade central do sistema.

**Independent Test**: Lançar um intervalo válido, tentar lançar para usuário fora do projeto e tentar intervalos inválidos.

**Acceptance Scenarios**:

1. **Given** um usuário vinculado ao projeto, **When** lança um intervalo válido, **Then** o lançamento é criado e retornado com projeto e usuário.
2. **Given** um usuário **não** vinculado ao projeto, **When** lança horas, **Then** o lançamento é recusado *(hoje responde "não encontrado"; ver FR-006)*.
3. **Given** um lançamento existente, **When** é editado com projeto/usuário/datas novos, **Then** o lançamento reflete os novos dados *(PLANEJADO: a edição deve revalidar as mesmas regras da criação)*.
4. **Given** `ended_at` menor ou igual a `started_at`, **When** lança ou edita, **Then** é rejeitado com erro de validação *(PLANEJADO)*.
5. **Given** `ended_at` no futuro, **When** lança ou edita, **Then** é rejeitado *(PLANEJADO)*.
6. **Given** o usuário já possui um intervalo que se cruza com o novo, **When** lança ou edita, **Then** é rejeitado por sobreposição *(PLANEJADO; escopo em aberto — ver [NEEDS CLARIFICATION: N1])*.
7. **Given** o usuário já tem lançado no dia um total que, somado ao novo, ultrapassa 8 horas, **When** lança ou edita, **Then** é rejeitado *(PLANEJADO; definição de "dia" em aberto — ver [NEEDS CLARIFICATION: N2])*.

---

### User Story 5 - Consultar horas de um projeto (Priority: P2) **[PARCIAL]**

Um usuário autenticado consulta os lançamentos de um projeto.

**Why this priority**: Permite conferir o que foi lançado, mas depende das histórias anteriores.

**Independent Test**: Lançar horas em um projeto e consultá-las.

**Acceptance Scenarios**:

1. **Given** lançamentos em um projeto, **When** consulta por projeto, **Then** recebe todos, com dados do projeto e do usuário.
2. **Given** um projeto sem lançamentos, **When** consulta, **Then** o resultado é uma lista vazia *(hoje retorna "não encontrado"; decisão D8 permite corrigir)*.
3. **Given** filtros de usuário e/ou período, **When** consulta, **Then** recebe apenas os lançamentos correspondentes, paginados *(PLANEJADO; filtros e paginação em aberto — ver [NEEDS CLARIFICATION: N4])*.

---

### User Story 6 - Totais e relatórios de horas (Priority: P2) **[PLANEJADO]**

Um usuário autenticado consulta o total de horas por projeto, por usuário e por período.

**Why this priority**: Entrega o valor de negócio do registro de horas, mas não bloqueia o registro em si.

**Independent Test**: Lançar horas conhecidas e conferir os totais por projeto, usuário e período.

**Acceptance Scenarios**:

1. **Given** lançamentos existentes, **When** solicita o total por projeto, **Then** recebe a soma das durações.
2. **Given** lançamentos de vários usuários, **When** solicita o total por usuário, **Then** recebe a soma por usuário.
3. **Given** um intervalo de datas, **When** solicita um relatório, **Then** só entram lançamentos do período *(tratamento de lançamento que cruza o limite do período em aberto — [NEEDS CLARIFICATION: N3])*.

---

### User Story 7 - Excluir dados (Priority: P3) **[PLANEJADO]**

Um usuário autenticado exclui usuários, projetos e lançamentos. A exclusão de usuário ou projeto remove também os lançamentos associados (decisão D5).

**Why this priority**: Necessário para completar o ciclo de vida dos dados, sem bloquear o fluxo principal.

**Independent Test**: Excluir um projeto com lançamentos e verificar que projeto e lançamentos deixam de existir.

**Acceptance Scenarios**:

1. **Given** um lançamento existente, **When** é excluído, **Then** deixa de aparecer nas consultas.
2. **Given** um projeto com lançamentos, **When** é excluído, **Then** o projeto e seus lançamentos são removidos.
3. **Given** um usuário com lançamentos, **When** é excluído, **Then** o usuário, sua credencial e seus lançamentos são removidos *(tratamento da credencial: [NEEDS CLARIFICATION: N6])*.
4. **Given** um id inexistente, **When** tenta excluir, **Then** recebe "não encontrado".

---

### User Story 8 - Listar usuários (Priority: P3) **[PLANEJADO]**

Um usuário autenticado lista os usuários cadastrados, com filtros e paginação.

**Why this priority**: Facilita escolher usuários ao montar projetos; hoje só é possível consultar por id.

**Independent Test**: Cadastrar vários usuários e listá-los com paginação.

**Acceptance Scenarios**:

1. **Given** usuários cadastrados, **When** lista, **Then** recebe a página solicitada, sem expor senha.
2. **Given** nenhum usuário, **When** lista, **Then** recebe lista vazia.

---

### Edge Cases

- O que acontece quando o mesmo usuário lança intervalos que se tocam exatamente (fim de um = início de outro)? *(proposta: permitido, pois não se cruzam)*
- Como o sistema trata um lançamento que atravessa a meia-noite no limite diário de 8h? [NEEDS CLARIFICATION: N2]
- O que acontece ao editar o lançamento para outro projeto do qual o usuário não participa?
- O que acontece ao remover um usuário da equipe do projeto (edição do projeto) quando ele já tem horas lançadas nele? *(hoje as horas permanecem; regra desejada não definida)*
- Login ou e-mail que diferem só em maiúsculas/minúsculas (ex.: `Admin` e `admin`)? *(decidido, N5/D12: são o **mesmo** valor — a unicidade **não** diferencia maiúsculas de minúsculas; `Admin` conflita com `admin`)*
- O que acontece se a aplicação reiniciar? *(hoje o usuário `admin` é recriado a cada inicialização; com unicidade de login isso precisa ser resolvido)*
- O que acontece com o token ao expirar no meio de uma operação? *(acesso negado; sem renovação)*
- Qual o comportamento com horário local do servidor em ambiente de container, tipicamente UTC? (decisão D6 aceita; registrar como limitação)

---

## Requirements *(mandatory)*

### Functional Requirements

**Autenticação**
- **FR-001**: O sistema MUST autenticar usuário por login e senha e emitir token com expiração configurável (padrão 15 minutos). **[EXISTE]**
- **FR-002**: O sistema MUST exigir token válido em todas as operações, exceto autenticação. **[EXISTE]**
- **FR-003**: Todo usuário autenticado MUST ter as mesmas permissões (sem papéis; decisão D2). **[EXISTE]**

**Usuários e projetos**
- **FR-004**: O sistema MUST permitir criar, consultar por id e editar usuários, e consultar os projetos de um usuário. **[EXISTE]**
- **FR-005**: O sistema MUST permitir criar, listar, consultar e editar projetos, vinculando usuários; a edição substitui a lista de usuários. **[EXISTE]**
- **FR-005a**: O sistema MUST garantir login único e e-mail único entre usuários, **sem diferenciar maiúsculas de minúsculas**, rejeitando duplicidade com conflito (D7, D12). **[EXISTE]**
- **FR-005b**: O sistema MUST validar entradas obrigatórias (`password` obrigatória só na criação; opcional na edição, D11) e ids referenciados, respondendo com erro de validação e mensagem clara, sem depender de falhas internas. **[PARCIAL]** *(usuários: implementado; validação de ids referenciados em projetos pertence à US3 e está pendente)*
- **FR-005c**: O sistema MUST criar o usuário `admin` inicial apenas se ainda não existir. **[EXISTE]**

**Lançamento de horas**
- **FR-006**: O sistema MUST aceitar lançamento apenas para usuário vinculado ao projeto, na criação **e na edição**, distinguindo na resposta "recurso inexistente" de "usuário não pertence ao projeto". **[PARCIAL]**
- **FR-007**: O sistema MUST rejeitar lançamento cujo `ended_at` não seja posterior a `started_at`. **[PLANEJADO]**
- **FR-008**: O sistema MUST rejeitar lançamento com `ended_at` no futuro. **[PLANEJADO]**
- **FR-009**: O sistema MUST rejeitar lançamento que se sobreponha a outro do mesmo usuário [NEEDS CLARIFICATION: N1 — entre projetos diferentes ou só no mesmo projeto? vale na edição?]. **[PLANEJADO]**
- **FR-010**: O sistema MUST rejeitar lançamento que faça o total de um usuário num dia ultrapassar 8 horas [NEEDS CLARIFICATION: N2 — critério do "dia" e tratamento de intervalo que cruza a meia-noite]. **[PLANEJADO]**
- **FR-011**: O sistema MUST permitir consultar os lançamentos de um projeto, com dados de projeto e usuário. **[EXISTE]**
- **FR-012**: O sistema MUST permitir filtrar lançamentos por usuário e período e paginá-los [NEEDS CLARIFICATION: N4 — filtros, paginação padrão e ordenação]. **[PLANEJADO]**
- **FR-013**: O sistema MUST permitir editar um lançamento revalidando as regras FR-006 a FR-010 [NEEDS CLARIFICATION: N7]. **[PLANEJADO]**

**Relatórios**
- **FR-014**: O sistema MUST fornecer totais de horas por projeto, por usuário e por período [NEEDS CLARIFICATION: N3 — formato e filtros]. **[PLANEJADO]**

**Exclusão e listagem**
- **FR-015**: O sistema MUST permitir excluir lançamentos, projetos e usuários; excluir projeto ou usuário MUST remover também seus lançamentos (D5). **[PLANEJADO]**
- **FR-016**: Ao excluir um usuário, o sistema MUST remover também sua credencial [NEEDS CLARIFICATION: N6 — quem pode excluir; pode excluir a si próprio ou o `admin`?]. **[PLANEJADO]**
- **FR-017**: O sistema MUST permitir listar usuários, sem expor senha [NEEDS CLARIFICATION: N4]. **[PLANEJADO]**

**Contrato da API**
- **FR-018**: O sistema MUST padronizar respostas: criação com status de "criado", lista vazia como sucesso com lista vazia, formato uniforme para listas, erros com `message` (D8). **[PLANEJADO]**
- **FR-019**: O contrato público (`openapi30.json`, `response_examples.txt`) MUST ser atualizado junto com cada mudança de comportamento. **[PLANEJADO]**

**Horário**
- **FR-020**: O sistema MUST interpretar e devolver datas/horas no horário local do servidor, sem conversão (D6). **[EXISTE]**

### Key Entities *(include if feature involves data)*

- **Usuário**: pessoa que atua em projetos e lança horas. Possui nome, e-mail e uma credencial (login e senha). Relaciona-se a muitos projetos e a muitos lançamentos. Login e e-mail devem ser únicos.
- **Credencial**: login e senha de acesso de um usuário; existe 1:1 com o usuário e é removida junto com ele.
- **Projeto**: iniciativa com título e descrição. Relaciona-se a muitos usuários (equipe) e a muitos lançamentos.
- **Lançamento de horas**: intervalo de trabalho com início e fim de um usuário em um projeto. Só existe para usuário da equipe do projeto e é removido junto com o usuário ou o projeto.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% das tentativas de lançamento inválidas (fim ≤ início, data futura, sobreposição, acima de 8h/dia, usuário fora do projeto) são rejeitadas e nenhum dado inválido é persistido.
- **SC-002**: 0 usuários com login ou e-mail duplicados após qualquer sequência de reinicializações e cadastros.
- **SC-003**: Um usuário consegue autenticar-se, criar um projeto com equipe e lançar horas em menos de 5 chamadas à API, sem preparação manual além do `admin` inicial.
- **SC-004**: O total de horas de um relatório é igual à soma das durações dos lançamentos do filtro, em 100% dos casos de teste.
- **SC-005**: Toda resposta de erro traz uma mensagem legível; nenhuma resposta expõe detalhes internos de exceção.
- **SC-006**: 100% das regras desta spec marcadas como **[PLANEJADO]** possuem pelo menos um teste automatizado de aceitação (hoje não há testes).
- **SC-007**: O contrato publicado coincide com o comportamento real em 100% dos endpoints.

---

## Assumptions

- Contexto de desafio técnico/portfólio, sem usuários reais (D1); requisitos de segurança são apenas documentados (D9). Senhas em texto puro, segredos versionados e `admin/admin` permanecem como limitações conhecidas.
- Não há papéis nem autorização por recurso (D2): qualquer usuário autenticado pode listar, editar e excluir qualquer dado.
- O contrato v1 pode ser alterado com quebra de compatibilidade (D8); não há clientes externos.
- Horário local do servidor (D6); em container tipicamente UTC.
- A plataforma será migrada para .NET 10 (D10); a migração é um requisito técnico e não altera os comportamentos acima.
- O banco é relacional, com criação/migração automática no startup.
- **Dependências**: [docs/objetivo-do-sistema.md](../../docs/objetivo-do-sistema.md) e [docs/arquitetura-do-sistema.md](../../docs/arquitetura-do-sistema.md) são as fontes das decisões D1–D10 e das lacunas N1–N7 citadas.

---

## Clarificações pendentes (resumo)

| ID | Pergunta | Bloqueia |
|---|---|---|
| N1 | Sobreposição: entre projetos diferentes ou só no mesmo? Vale na edição? | FR-009 |
| N2 | "Dia" do limite de 8h; intervalo que cruza a meia-noite | FR-010 |
| N3 | Totais e relatórios: formato e filtros | FR-014, US6 |
| N4 | Filtros, paginação e ordenação das listagens | FR-012, FR-017 |
| ~~N5~~ | *Resolvida (D12): a unicidade não diferencia maiúsculas/minúsculas; `admin` segue a regra* | — |
| N6 | Quem pode excluir; credencial órfã; excluir a si/`admin` | FR-015, FR-016 |
| N7 | Edição de horas revalida todas as regras? | FR-013 |
