# Feature Specification: GaragePro Angular Frontend

**Feature Branch**: `004-angular-frontend`  
**Created**: 2026-04-28  
**Status**: Draft  
**Input**: User description: "Vamos criar o front de acordo com os endpoints existentes"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Autenticação e Acesso (Priority: P1)

Um colaborador da garagem acessa o sistema pelo navegador, informa suas credenciais e entra na área correspondente ao seu perfil. Ao sair, sua sessão é encerrada com segurança.

**Why this priority**: Sem autenticação nada mais funciona. É o ponto de entrada obrigatório para todas as demais funcionalidades.

**Independent Test**: Pode ser testado acessando a tela de login, informando e-mail e senha válidos, verificando o redirecionamento para a área inicial e depois realizando logout.

**Acceptance Scenarios**:

1. **Given** o usuário não autenticado na tela de login, **When** informa e-mail e senha válidos e confirma, **Then** é redirecionado para a tela inicial correspondente ao seu perfil e visualiza seu nome na interface.
2. **Given** o usuário autenticado, **When** clica em "Sair", **Then** a sessão é encerrada e ele é redirecionado para a tela de login.
3. **Given** o usuário na tela de login, **When** informa credenciais inválidas, **Then** vê uma mensagem de erro clara sem detalhes de segurança.
4. **Given** o token de sessão expirado, **When** tenta acessar qualquer tela protegida, **Then** é redirecionado automaticamente para o login.

---

### User Story 2 - Gestão de Clientes (Priority: P1)

Um Administrador ou Técnico gerencia a base de clientes da garagem: cadastra novos clientes com endereço, consulta dados completos, atualiza informações e remove clientes sem veículos vinculados.

**Why this priority**: Clientes são o núcleo do negócio; todos os demais recursos (veículos, ordens de serviço) dependem deles.

**Independent Test**: Pode ser testado criando um cliente com endereço, buscando-o na listagem, editando seus dados e tentando excluí-lo.

**Acceptance Scenarios**:

1. **Given** o usuário Admin ou Técnico na tela de clientes, **When** preenche o formulário com nome e pelo menos um endereço e salva, **Then** o cliente aparece na listagem com seus dados.
2. **Given** a listagem de clientes, **When** o usuário abre o detalhe de um cliente, **Then** vê os endereços e veículos vinculados.
3. **Given** o usuário Admin ou Técnico no detalhe de um cliente, **When** atualiza nome, e-mail, telefone ou documento e salva, **Then** os novos dados são refletidos imediatamente.
4. **Given** um cliente sem veículos vinculados, **When** o usuário tenta excluí-lo, **Then** o cliente é removido da listagem com confirmação.
5. **Given** um cliente com veículos vinculados, **When** o usuário tenta excluí-lo, **Then** vê uma mensagem explicando que não é possível remover enquanto houver veículos.
6. **Given** o usuário Financial na tela de clientes, **When** navega pela listagem ou abre um detalhe, **Then** não vê botões de criação, edição ou exclusão.

---

### User Story 3 - Gestão de Veículos e Transferência (Priority: P2)

Um Administrador ou Técnico cadastra veículos vinculados a clientes, atualiza suas informações e transfere a propriedade de um veículo para outro cliente preservando o histórico.

**Why this priority**: Veículos são o objeto principal de serviço; sua rastreabilidade de propriedade é diferencial do sistema.

**Independent Test**: Pode ser testado cadastrando um veículo para um cliente, consultando seu detalhe com histórico e realizando uma transferência para outro cliente.

**Acceptance Scenarios**:

1. **Given** o usuário Admin ou Técnico na tela de veículos, **When** preenche marca, modelo, ano, cor, placa e seleciona o cliente proprietário, **Then** o veículo aparece na listagem vinculado ao cliente.
2. **Given** o usuário visualizando o detalhe de um veículo, **When** abre a tela, **Then** vê o histórico completo de transferências de propriedade.
3. **Given** um veículo com proprietário atual, **When** o usuário Admin ou Técnico inicia uma transferência selecionando o cliente destino e confirma, **Then** a propriedade é atualizada e o evento aparece no histórico.
4. **Given** a listagem de veículos, **When** o usuário filtra por cliente, **Then** vê apenas os veículos daquele cliente.
5. **Given** o usuário Financial, **When** tenta acessar a área de veículos, **Then** o acesso é negado ou o menu não está disponível.

---

### User Story 4 - Gestão de Endereços de Clientes (Priority: P2)

Um Administrador ou Técnico adiciona novos endereços a um cliente existente, atualiza endereços cadastrados e remove endereços, respeitando a regra de ao menos um endereço por cliente.

**Why this priority**: Endereços são componente do cadastro de clientes e necessários para faturamento e localização.

**Independent Test**: Pode ser testado acessando o detalhe de um cliente, adicionando um endereço, editando-o e tentando remover o último endereço.

**Acceptance Scenarios**:

1. **Given** o usuário Admin ou Técnico no detalhe de um cliente, **When** adiciona um novo endereço com todos os campos obrigatórios, **Then** o endereço aparece na lista de endereços do cliente.
2. **Given** um endereço existente, **When** o usuário edita e salva os dados, **Then** as informações atualizadas são exibidas.
3. **Given** um cliente com mais de um endereço, **When** o usuário remove um deles, **Then** o endereço é removido da lista.
4. **Given** um cliente com apenas um endereço, **When** o usuário tenta removê-lo, **Then** vê uma mensagem explicando que o cliente precisa ter pelo menos um endereço.

---

### User Story 5 - Gestão de Produtos e Serviços (Priority: P2)

Um Administrador ou usuário Financeiro gerencia o catálogo de produtos e serviços da garagem: cadastra itens com nome, descrição e preço, atualiza e remove quando necessário.

**Why this priority**: Produtos e serviços formam a base para ordens de serviço e faturamento futuro.

**Independent Test**: Pode ser testado criando um produto, buscando-o na listagem, editando o preço e excluindo-o.

**Acceptance Scenarios**:

1. **Given** o usuário Admin ou Financial na tela de produtos, **When** cadastra um produto com nome e preço, **Then** ele aparece na listagem.
2. **Given** o usuário Admin ou Financial na tela de serviços, **When** cadastra um serviço com nome, descrição e preço, **Then** ele aparece na listagem.
3. **Given** um produto ou serviço existente, **When** o usuário atualiza seus dados, **Then** as mudanças são refletidas imediatamente.
4. **Given** um produto ou serviço existente, **When** o usuário exclui, **Then** ele é removido da listagem após confirmação.
5. **Given** o usuário Técnico, **When** tenta acessar a área de produtos ou serviços, **Then** o acesso é negado ou o menu não está disponível.

---

### User Story 6 - Gestão de Usuários do Sistema (Priority: P3)

Um Administrador gerencia os usuários que têm acesso ao sistema: cria contas com nome, e-mail, senha e perfil, atualiza dados e remove usuários.

**Why this priority**: Gestão de usuários é necessária para controle de acesso, mas é uma operação menos frequente e restrita a Admins.

**Independent Test**: Pode ser testado criando um novo usuário com perfil Técnico, verificando na listagem e atualizando seu perfil de acesso.

**Acceptance Scenarios**:

1. **Given** o usuário Admin na tela de usuários, **When** preenche nome, e-mail, senha e seleciona ao menos um perfil, **Then** o usuário aparece na listagem.
2. **Given** um usuário existente, **When** o Admin atualiza nome, e-mail ou perfis, **Then** os dados são atualizados.
3. **Given** um usuário existente, **When** o Admin exclui, **Then** o usuário é removido após confirmação.
4. **Given** usuário com perfil Técnico ou Financial autenticado, **When** tenta acessar a área de usuários, **Then** o acesso é negado.

---

### Edge Cases

- O que acontece quando o token de sessão expira enquanto o usuário está preenchendo um formulário longo?
- Como o sistema exibe erros de validação quando múltiplos campos estão inválidos ao mesmo tempo?
- O que acontece quando uma listagem paginada retorna zero resultados?
- Como o sistema se comporta quando a conexão com o servidor é perdida durante uma operação de salvamento?
- O que acontece quando dois usuários editam o mesmo registro ao mesmo tempo?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE apresentar uma tela de login acessível sem autenticação prévia que permita ao usuário informar e-mail e senha.
- **FR-002**: O sistema DEVE armazenar a sessão do usuário autenticado de forma que ele não precise logar novamente ao navegar entre telas.
- **FR-003**: O sistema DEVE redirecionar automaticamente para o login qualquer acesso a telas protegidas sem sessão válida.
- **FR-004**: O sistema DEVE exibir o nome e perfil do usuário logado na interface.
- **FR-005**: O sistema DEVE ocultar ou bloquear menus e ações de acordo com o perfil do usuário (Admin, Técnico, Financeiro).
- **FR-006**: O sistema DEVE permitir que Admins e Técnicos realizem operações completas de criação, leitura, atualização e exclusão de clientes.
- **FR-007**: O sistema DEVE permitir que usuários Financeiros visualizem a listagem e o detalhe de clientes sem acesso a operações de escrita.
- **FR-008**: O sistema DEVE exibir a listagem de clientes com paginação, mostrando nome, e-mail, telefone e quantidade de veículos.
- **FR-009**: O sistema DEVE exibir o detalhe de um cliente incluindo todos os seus endereços e veículos vinculados.
- **FR-010**: O sistema DEVE validar e exibir mensagens de erro claras quando campos obrigatórios não forem preenchidos ou estiverem em formato inválido.
- **FR-011**: O sistema DEVE impedir a exclusão de um cliente que possua veículos vinculados, exibindo mensagem explicativa.
- **FR-012**: O sistema DEVE permitir que Admins e Técnicos gerenciem endereços de clientes (adicionar, editar, remover).
- **FR-013**: O sistema DEVE impedir a remoção do último endereço de um cliente, exibindo mensagem explicativa.
- **FR-014**: O sistema DEVE permitir que Admins e Técnicos realizem operações completas de criação, leitura, atualização e exclusão de veículos.
- **FR-015**: O sistema DEVE exibir o detalhe de um veículo incluindo seu histórico de transferências de propriedade.
- **FR-016**: O sistema DEVE permitir que Admins e Técnicos transfiram a propriedade de um veículo para outro cliente, com campo para observações.
- **FR-017**: O sistema DEVE permitir filtrar a listagem de veículos por cliente.
- **FR-018**: O sistema DEVE permitir que Admins e usuários Financeiros realizem operações completas de criação, leitura, atualização e exclusão de produtos e serviços.
- **FR-019**: O sistema DEVE permitir que apenas Admins gerenciem usuários do sistema.
- **FR-020**: O sistema DEVE exibir confirmação de segurança antes de qualquer operação de exclusão.
- **FR-021**: Todas as listagens DEVEM suportar paginação com controles de navegação entre páginas.
- **FR-022**: O sistema DEVE exibir mensagens de feedback ao usuário após operações bem-sucedidas (criação, atualização, exclusão).

### Key Entities

- **Usuário do Sistema**: Colaborador com acesso ao sistema; possui nome, e-mail, senha e um ou mais perfis (Admin, Técnico, Financeiro).
- **Cliente**: Pessoa física ou jurídica atendida pela garagem; possui dados de contato, documento e pelo menos um endereço; pode ter múltiplos veículos vinculados.
- **Endereço**: Localização associada a um cliente; possui tipo (Residencial, Cobrança, Outro) e dados completos de logradouro.
- **Veículo**: Automóvel atendido pela garagem; possui placa única, marca, modelo, ano, cor e proprietário atual; mantém histórico de transferências.
- **Transferência de Veículo**: Registro histórico de mudança de propriedade de um veículo; preserva cliente de origem, cliente de destino, data e observações.
- **Produto**: Item físico oferecido pela garagem; possui nome, descrição opcional e preço.
- **Serviço**: Serviço executado pela garagem; possui nome, descrição opcional e preço.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Usuários conseguem realizar login e acessar a tela principal em menos de 10 segundos em condições normais de rede.
- **SC-002**: Usuários conseguem cadastrar um novo cliente com endereço em menos de 3 minutos a partir de dados já em mãos.
- **SC-003**: Usuários conseguem localizar um cliente existente na listagem em menos de 30 segundos.
- **SC-004**: 100% das ações disponíveis na interface estão de acordo com o perfil do usuário logado — nenhum usuário consegue realizar operações além do seu nível de acesso.
- **SC-005**: Erros de validação são exibidos de forma imediata, sem necessidade de navegar para outra tela.
- **SC-006**: A transferência de veículo entre clientes é concluída em menos de 2 minutos, incluindo a verificação no histórico.
- **SC-007**: Todas as listagens com mais de 20 registros exibem paginação funcional sem degradação perceptível de desempenho.
- **SC-008**: Mensagens de confirmação aparecem antes de toda operação de exclusão, eliminando exclusões acidentais.

## Assumptions

- Os usuários acessam o sistema via navegador desktop; suporte a dispositivos móveis está fora do escopo desta versão.
- A API backend já está implementada e funcional conforme documentada em `specs/002-core-crud-api/contracts/api-reference.md`.
- A autenticação é baseada em JWT Bearer token retornado pelo endpoint de login; o frontend armazena e renova o token conforme necessário.
- Perfis de usuário (Admin, Técnico, Financeiro) são fornecidos pelo backend no payload do token ou na resposta de login.
- Não há requisito de internacionalização para esta versão; o sistema será em português do Brasil.
- Não há requisito de tema escuro/claro para esta versão; um único tema visual é suficiente.
- O CEP não será consultado automaticamente para preenchimento de endereço nesta versão.
- A alteração de senha de usuários não está coberta pelos endpoints disponíveis e está fora do escopo desta versão.
- A listagem padrão exibirá 20 registros por página, alinhada ao padrão da API.
