# Feature Specification: Endereço na Criação de Clientes

**Feature Branch**: `005-fix-user-address-create`
**Created**: 2026-04-28
**Status**: Draft
**Input**: User description: "Crie um bug na adicao de usuarios. Precisamos incluir o endereco, no momento o frontend nao disponibiliza a opcao na tela de cadastro, apenas para editar."

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Adicionar Endereço ao Cadastrar Novo Cliente (Priority: P1)

Um Administrador ou Técnico preenche o formulário de novo cliente e precisa informar ao menos um endereço antes de salvar. Atualmente, o formulário de criação não exibe a seção de endereços, então o cadastro falha ou é incompleto porque o sistema exige ao menos um endereço.

**Why this priority**: O sistema exige pelo menos um endereço para criar um cliente. Sem isso, a criação de novos clientes está bloqueada ou incompleta — impacto direto na operação diária da oficina.

**Independent Test**: Abrir o formulário "Novo Cliente", preencher os dados pessoais, adicionar um endereço e salvar com sucesso.

**Acceptance Scenarios**:

1. **Given** o usuário está no formulário de novo cliente, **When** ele abre a tela de cadastro, **Then** a seção "Endereços" está visível com um botão "Adicionar Endereço".
2. **Given** a seção de endereços está visível, **When** o usuário clica em "Adicionar Endereço" e preenche todos os campos obrigatórios, **Then** o endereço aparece listado no formulário antes de salvar.
3. **Given** o formulário de novo cliente com pelo menos um endereço preenchido, **When** o usuário clica em "Salvar", **Then** o cliente é criado com sucesso com o endereço informado.
4. **Given** o formulário de novo cliente sem nenhum endereço, **When** o usuário tenta salvar, **Then** o sistema exibe mensagem de erro informando que ao menos um endereço é obrigatório.
5. **Given** o usuário adicionou mais de um endereço no formulário, **When** ele remove um endereço que não é o único, **Then** o endereço é removido da lista sem erros.
6. **Given** o usuário adicionou apenas um endereço e tenta removê-lo, **When** clica em excluir, **Then** o sistema impede a remoção e exibe mensagem explicando que ao menos um endereço é obrigatório.

---

### User Story 2 — Editar Endereços de um Cliente Existente (Priority: P2)

Um Administrador ou Técnico acessa o formulário de edição de um cliente existente e consegue adicionar, editar ou remover endereços — funcionalidade já parcialmente existente.

**Why this priority**: Esta funcionalidade já existe no modo de edição. A prioridade é garantir paridade entre criação e edição para que nenhum endereço adicionado na criação se perca ao editar.

**Independent Test**: Abrir cliente existente, adicionar novo endereço e salvar — verificar que o endereço aparece na listagem.

**Acceptance Scenarios**:

1. **Given** o usuário está no formulário de edição de um cliente, **When** abre a tela, **Then** a seção "Endereços" exibe todos os endereços cadastrados para esse cliente.
2. **Given** o usuário edita um endereço existente e salva, **When** a edição é confirmada, **Then** os dados atualizados aparecem na listagem de endereços do cliente.

---

### Edge Cases

- O que acontece se o usuário tentar criar um cliente sem nenhum endereço? → Erro de validação antes de enviar ao sistema, informando que ao menos um endereço é obrigatório.
- O que acontece se o formulário de endereço for aberto e fechado sem preencher? → Nenhum endereço incompleto é adicionado à lista.
- O que acontece se campos obrigatórios do endereço (logradouro, número, bairro, cidade, estado, CEP) ficarem vazios? → Validação inline impede o salvamento do endereço com campos obrigatórios em branco.
- O que acontece se o tipo de endereço não for selecionado? → Campo obrigatório; validação impede prosseguir sem escolher o tipo.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O formulário de cadastro de novo cliente DEVE exibir a seção "Endereços" com a opção de adicionar ao menos um endereço antes de salvar.
- **FR-002**: O sistema DEVE validar que ao menos um endereço foi adicionado antes de permitir o salvamento do novo cliente.
- **FR-003**: O usuário DEVE conseguir adicionar múltiplos endereços durante a criação do cliente.
- **FR-004**: O usuário DEVE conseguir remover um endereço adicionado durante a criação, desde que não seja o único.
- **FR-005**: O sistema DEVE exibir mensagem de erro clara se o usuário tentar remover o único endereço durante a criação.
- **FR-006**: O formulário de endereço durante a criação DEVE exigir os mesmos campos obrigatórios que o formulário de edição: tipo (Residencial/Cobrança/Outro), logradouro, número, bairro, cidade, estado (2 letras) e CEP.
- **FR-007**: O campo "complemento" DEVE ser opcional tanto na criação quanto na edição.
- **FR-008**: A seção de endereços na criação DEVE ser visível apenas para usuários com perfil Administrador ou Técnico.
- **FR-009**: A funcionalidade de endereços no modo de edição DEVE continuar funcionando sem regressão.

### Key Entities

- **Cliente**: Representa o cliente da oficina; requer ao menos um endereço no cadastro. Atributos relevantes: nome, e-mail, telefone, documento.
- **Endereço**: Vinculado a um cliente; tipos possíveis: Residencial, Cobrança, Outro. Campos: tipo, logradouro, número, complemento (opcional), bairro, cidade, estado (UF), CEP.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% dos novos clientes criados pelo sistema possuem ao menos um endereço registrado — sem clientes sem endereço gerados pela interface.
- **SC-002**: O fluxo de criação de cliente com endereço é concluído em menos de 3 minutos por um usuário sem treinamento específico.
- **SC-003**: Mensagens de erro de validação de endereço são exibidas em menos de 1 segundo após a tentativa de salvar sem endereço.
- **SC-004**: Nenhuma regressão no fluxo de edição de endereços de clientes existentes após a correção.

## Assumptions

- "Usuários" na descrição original refere-se a **clientes da oficina** (entidade "Cliente"), não a usuários do sistema (Admin, Técnico, Financeiro). A distinção está baseada no fato de que apenas clientes possuem endereços e a funcionalidade de edição descrita já existe para clientes.
- O perfil Financeiro não tem permissão para criar ou editar clientes — sem alteração nesta regra.
- As regras de validação de endereço (campos obrigatórios, tipo enum, estado com 2 caracteres) são as mesmas já aplicadas no modo de edição.
- A adição de múltiplos endereços durante a criação segue o mesmo padrão visual do modo de edição (lista com botões de editar/excluir e botão "Adicionar Endereço").
- Não há integração automática de CEP (busca de endereço por CEP está fora do escopo).
- A regra de negócio de "ao menos 1 endereço obrigatório" é validada tanto no frontend (antes de enviar) quanto pelo backend (resposta 400).
