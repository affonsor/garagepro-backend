# Feature Specification: Tela de Agendamento

**Feature Branch**: `005-scheduling-screen`
**Created**: 2026-04-28
**Status**: Draft
**Input**: User description: "Faca uma especificacao para a tela de agendamento. Provavelmente a tela com mais uso no sistema, deve ser elegante e facil de utilizar. O agendamento deve ser feito com hora inicio, e previsao de termino. Ira associar o cliente com o produto e servico a ser executado. O agendamento pode ser cancelado, concluido ou remarcado para outra data (deve ter uma tag de reagendado). Na tela de listagem de agendamento, deve exibir o valor total referente ao agendamento. No final deve sumarizar o valor dos agendamentos concluidos x cancelados x a realizar."

## Objetivo da Tela

A tela de agendamento deve ser a area operacional mais rapida do GaragePro: o usuario precisa consultar a agenda do dia, criar um novo atendimento, identificar atrasos, remarcar compromissos e fechar/cancelar agendamentos com poucos cliques.

A experiencia deve ser elegante, objetiva e densa o suficiente para uso diario. O foco visual principal e a agenda/listagem; botoes, filtros e resumos devem apoiar a tomada de decisao sem competir com os dados.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Consultar Agendamentos do Periodo (Priority: P1)

Um Administrador, Tecnico ou Financeiro acessa a tela de agendamentos e visualiza os compromissos do periodo selecionado com horario de inicio, previsao de termino, cliente, produto, servico, status, valor total e indicacao de remarcacao quando aplicavel.

**Why this priority**: A consulta da agenda e o uso mais frequente da tela. A equipe precisa entender rapidamente o que ja aconteceu, o que esta em andamento e o que ainda deve ser realizado.

**Independent Test**: Acessar a tela de agendamentos com registros em diferentes status e verificar se a listagem exibe todos os dados essenciais e o resumo financeiro final.

**Acceptance Scenarios**:

1. **Given** existem agendamentos no periodo selecionado, **When** o usuario abre a tela, **Then** a listagem exibe inicio, previsao de termino, cliente, produto, servico, valor total, status e acoes disponiveis.
2. **Given** um agendamento foi remarcado ao menos uma vez, **When** ele aparece na listagem, **Then** exibe uma tag "Remarcado" de forma visivel.
3. **Given** nao existem agendamentos para o periodo, **When** a tela carrega, **Then** exibe estado vazio com acao para criar um novo agendamento.
4. **Given** a listagem possui agendamentos concluidos, cancelados e a realizar, **When** a tela termina de carregar, **Then** o rodape exibe o total financeiro separado por status.

---

### User Story 2 - Criar Novo Agendamento (Priority: P1)

Um Administrador ou Tecnico cria um agendamento informando cliente, produto, servico, data/hora de inicio e previsao de termino. O sistema calcula o valor total a partir do produto e do servico selecionados.

**Why this priority**: A criacao rapida e confiavel reduz erros de agenda e evita retrabalho no atendimento.

**Independent Test**: Criar um agendamento com cliente, produto e servico existentes, salvar e confirmar que ele aparece na listagem do periodo correto com valor total calculado.

**Acceptance Scenarios**:

1. **Given** o usuario esta na tela de agendamentos, **When** clica em "Novo Agendamento", **Then** abre um formulario direto, com foco no campo de cliente.
2. **Given** o usuario selecionou um cliente, **When** busca produto e servico, **Then** o sistema permite escolher apenas itens ativos/disponiveis.
3. **Given** produto e servico possuem preco cadastrado, **When** ambos sao selecionados, **Then** o valor total do agendamento e exibido antes de salvar.
4. **Given** inicio e previsao de termino foram preenchidos, **When** a previsao de termino e anterior ou igual ao inicio, **Then** o sistema impede salvar e exibe erro claro.
5. **Given** todos os campos obrigatorios estao validos, **When** o usuario salva, **Then** o agendamento e criado com status "A realizar".

---

### User Story 3 - Concluir ou Cancelar Agendamento (Priority: P1)

Um Administrador ou Tecnico altera o status de um agendamento para concluido quando o servico foi executado, ou cancelado quando o atendimento nao vai mais acontecer.

**Why this priority**: Status confiavel e necessario para operacao, acompanhamento financeiro e previsao de demanda.

**Independent Test**: Alterar um agendamento "A realizar" para "Concluido" e outro para "Cancelado", verificando a atualizacao visual e os totais no rodape.

**Acceptance Scenarios**:

1. **Given** um agendamento esta "A realizar", **When** o usuario conclui o agendamento, **Then** o status muda para "Concluido" e o valor passa a compor o total de concluidos.
2. **Given** um agendamento esta "A realizar", **When** o usuario cancela o agendamento, **Then** o status muda para "Cancelado" e o valor passa a compor o total de cancelados.
3. **Given** um agendamento ja esta "Concluido", **When** o usuario abre suas acoes, **Then** nao ve opcoes de cancelar, concluir ou remarcar.
4. **Given** um agendamento ja esta "Cancelado", **When** o usuario abre suas acoes, **Then** nao ve opcoes de concluir ou remarcar.

---

### User Story 4 - Remarcar Agendamento (Priority: P1)

Um Administrador ou Tecnico remarca um agendamento para outra data e/ou horario, mantendo o mesmo cliente, produto, servico, valor e historico de alteracao.

**Why this priority**: Remarcacao e comum em uma oficina. A tag "Remarcado" evita confusao operacional e ajuda a equipe a identificar compromissos que ja mudaram de data.

**Independent Test**: Remarcar um agendamento "A realizar" para outra data e confirmar que ele aparece no novo periodo com a tag "Remarcado".

**Acceptance Scenarios**:

1. **Given** um agendamento esta "A realizar", **When** o usuario escolhe "Remarcar", **Then** abre um formulario curto com nova data/hora de inicio e nova previsao de termino.
2. **Given** os novos horarios sao validos, **When** o usuario confirma a remarcacao, **Then** o agendamento muda para a nova data e exibe a tag "Remarcado".
3. **Given** o agendamento foi remarcado, **When** o usuario abre o detalhe, **Then** consegue ver a data/hora original e a data/hora atual.
4. **Given** a nova previsao de termino e anterior ou igual ao novo inicio, **When** o usuario tenta confirmar, **Then** o sistema impede a remarcacao e exibe erro claro.

---

### User Story 5 - Filtrar, Buscar e Trabalhar Rapido (Priority: P2)

Um usuario usa filtros por periodo, status, cliente e busca textual para localizar rapidamente um agendamento, sem sair da tela principal.

**Why this priority**: Como sera uma tela de alto uso, pequenos atritos de navegacao se repetem muitas vezes ao dia.

**Independent Test**: Aplicar filtros de data, status e cliente, validar que a lista e os totais finais refletem apenas o conjunto filtrado.

**Acceptance Scenarios**:

1. **Given** o usuario seleciona um intervalo de datas, **When** aplica o filtro, **Then** a listagem mostra apenas agendamentos iniciados dentro do periodo.
2. **Given** o usuario filtra por status "Concluido", **When** a tela atualiza, **Then** o resumo financeiro tambem considera apenas os registros filtrados.
3. **Given** o usuario busca pelo nome do cliente, produto ou servico, **When** digita no campo de busca, **Then** a listagem retorna os agendamentos correspondentes.

---

### Edge Cases

- O que acontece quando dois usuarios tentam alterar o mesmo agendamento ao mesmo tempo?
- O que acontece se o produto ou servico selecionado for desativado antes do agendamento ser concluido?
- O que acontece se o usuario tentar remarcar para uma data no passado?
- O que acontece se um agendamento remarcado for posteriormente cancelado?
- Como o sistema deve se comportar quando o cliente, produto ou servico nao carrega por falha de rede?
- Como o valor total deve ser preservado se o preco do produto ou servico mudar depois da criacao do agendamento?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE disponibilizar uma tela de listagem de agendamentos acessivel pelo menu principal.
- **FR-002**: A listagem DEVE exibir, no minimo, data/hora de inicio, previsao de termino, cliente, produto, servico, valor total, status e acoes.
- **FR-003**: A listagem DEVE exibir uma tag "Remarcado" para todo agendamento que tenha sido remarcado ao menos uma vez.
- **FR-004**: O sistema DEVE permitir criar agendamento com cliente, produto, servico, data/hora de inicio e previsao de termino.
- **FR-005**: O sistema DEVE validar que a previsao de termino seja posterior a data/hora de inicio.
- **FR-006**: O sistema DEVE criar novos agendamentos com status inicial "A realizar".
- **FR-007**: O sistema DEVE calcular e exibir o valor total do agendamento como soma do valor do produto e do valor do servico selecionados.
- **FR-008**: O valor total calculado no momento da criacao DEVE ser preservado no agendamento, mesmo que o preco do produto ou servico mude depois.
- **FR-009**: O sistema DEVE permitir cancelar um agendamento em status "A realizar".
- **FR-010**: O sistema DEVE permitir concluir um agendamento em status "A realizar".
- **FR-011**: O sistema DEVE permitir remarcar um agendamento em status "A realizar" para outra data/hora.
- **FR-012**: Ao remarcar, o sistema DEVE manter o agendamento como "A realizar" e marcar o registro com a tag "Remarcado".
- **FR-013**: Ao remarcar, o sistema DEVE manter historico com data/hora original, nova data/hora, usuario responsavel e data da alteracao.
- **FR-014**: O sistema NAO DEVE permitir cancelar, concluir ou remarcar agendamentos que ja estejam "Concluidos" ou "Cancelados".
- **FR-015**: O sistema DEVE exibir confirmacao antes de cancelar um agendamento.
- **FR-016**: O sistema DEVE exibir feedback visual apos criar, cancelar, concluir ou remarcar um agendamento.
- **FR-017**: A tela DEVE permitir filtrar por periodo, status e cliente.
- **FR-018**: A tela DEVE permitir busca por cliente, produto ou servico.
- **FR-019**: O rodape da listagem DEVE sumarizar o valor total dos agendamentos "Concluidos", "Cancelados" e "A realizar".
- **FR-020**: O resumo financeiro DEVE respeitar os filtros aplicados na listagem.
- **FR-021**: O sistema DEVE exibir tambem a quantidade de agendamentos em cada grupo do resumo financeiro.
- **FR-022**: Administradores e Tecnicos DEVEM poder criar, concluir, cancelar e remarcar agendamentos.
- **FR-023**: Usuarios Financeiros DEVEM poder visualizar agendamentos e totais, sem permissoes de escrita.
- **FR-024**: Todas as datas e valores monetarios DEVEM ser exibidos no padrao pt-BR.

### UX Requirements

- **UX-001**: A tela inicial de agendamentos DEVE priorizar a agenda/listagem, evitando blocos explicativos ou conteudo introdutorio.
- **UX-002**: A acao "Novo Agendamento" DEVE estar sempre visivel no topo para usuarios com permissao.
- **UX-003**: Status DEVEM ser identificaveis por texto e cor, sem depender apenas da cor.
- **UX-004**: A tag "Remarcado" DEVE ser visualmente secundaria ao status, mas facilmente perceptivel.
- **UX-005**: Acoes por linha DEVEM usar icones com tooltip e menu de mais opcoes quando houver muitas acoes.
- **UX-006**: O formulario de criacao/edicao DEVE caber em um fluxo curto, preferencialmente em dialog lateral ou pagina dedicada com campos agrupados.
- **UX-007**: O valor total DEVE ser exibido em destaque no formulario antes do salvamento.
- **UX-008**: O resumo financeiro final DEVE permanecer facil de comparar, exibindo tres blocos: "Concluidos", "Cancelados" e "A realizar".

## Screen Specification

### Listagem de Agendamentos

**Header**:
- Titulo: "Agendamentos"
- Subtitulo curto com o periodo atual, por exemplo: "Hoje" ou "01/04/2026 a 30/04/2026"
- Acao primaria: "Novo Agendamento" para Admin e Tecnico

**Filtros**:
- Periodo: hoje, semana, mes, intervalo personalizado
- Status: todos, a realizar, concluidos, cancelados
- Cliente: autocomplete
- Busca livre: cliente, produto ou servico

**Colunas sugeridas**:

| Coluna | Conteudo |
|--------|----------|
| Inicio | Data e hora de inicio |
| Previsao | Hora/data prevista de termino |
| Cliente | Nome do cliente |
| Produto | Nome do produto associado |
| Servico | Nome do servico associado |
| Valor total | Produto + servico em BRL |
| Status | A realizar, Concluido ou Cancelado |
| Tags | Remarcado quando aplicavel |
| Acoes | Detalhar, remarcar, concluir, cancelar |

**Resumo no rodape**:

| Grupo | Regra de soma | Exibicao |
|-------|---------------|----------|
| Concluidos | Soma do valor total de agendamentos com status `Completed` | Quantidade + total em BRL |
| Cancelados | Soma do valor total de agendamentos com status `Canceled` | Quantidade + total em BRL |
| A realizar | Soma do valor total de agendamentos com status `Scheduled` | Quantidade + total em BRL |

### Formulario de Agendamento

**Campos obrigatorios**:

| Campo | Tipo | Validacoes |
|-------|------|------------|
| Cliente | Autocomplete | Obrigatorio |
| Produto | Autocomplete/select | Obrigatorio, produto ativo |
| Servico | Autocomplete/select | Obrigatorio, servico ativo |
| Inicio | DateTime | Obrigatorio |
| Previsao de termino | DateTime | Obrigatorio, posterior ao inicio |

**Campos recomendados**:

| Campo | Tipo | Observacao |
|-------|------|------------|
| Observacoes | Textarea | Opcional, para detalhes operacionais |
| Veiculo | Autocomplete/select | Opcional nesta versao; se usado, listar apenas veiculos do cliente |

**Calculo financeiro**:
- O preco do produto e do servico sao capturados no momento do agendamento.
- O valor total do agendamento e a soma do preco do produto e do preco do servico.
- O usuario visualiza o valor total calculado antes de salvar.

### Remarcacao

**Acao**: disponivel apenas para agendamentos "A realizar".

**Campos**:
- Nova data/hora de inicio
- Nova previsao de termino
- Motivo/observacao da remarcacao (opcional, recomendado)

**Resultado esperado**:
- Atualiza inicio e previsao de termino.
- Mantem status "A realizar".
- Marca o agendamento como remarcado.
- Registra historico de remarcacao com dados anteriores e novos.
- Exibe tag "Remarcado" na listagem e no detalhe.

### Cancelamento

**Acao**: disponivel apenas para agendamentos "A realizar".

**Comportamento**:
- Exibe dialog de confirmacao.
- Campo de motivo pode ser opcional na primeira versao, mas a estrutura deve permitir adiciona-lo depois.
- Altera status para "Cancelado".
- Remove acoes de concluir/remarcar/cancelar da linha.

### Conclusao

**Acao**: disponivel apenas para agendamentos "A realizar".

**Comportamento**:
- Pode ser confirmacao simples.
- Altera status para "Concluido".
- Valor passa a compor o total de concluidos.
- Remove acoes de concluir/remarcar/cancelar da linha.

## Key Entities

- **Agendamento**: Registro operacional com cliente, produto, servico, inicio, previsao de termino, valor total e status.
- **Cliente**: Pessoa fisica ou juridica atendida pela garagem.
- **Produto**: Item associado ao atendimento; seu preco compoe o valor total do agendamento.
- **Servico**: Servico a ser executado; seu preco compoe o valor total do agendamento.
- **Historico de Remarcacao**: Registro de alteracao de data/hora do agendamento, preservando inicio/previsao anteriores e novos valores.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Um usuario treinado consegue criar um novo agendamento em menos de 60 segundos usando cliente, produto e servico ja cadastrados.
- **SC-002**: Um usuario consegue identificar os agendamentos do dia e seus status em menos de 10 segundos apos abrir a tela.
- **SC-003**: O valor total de cada agendamento e exibido em 100% das linhas da listagem.
- **SC-004**: A tag "Remarcado" aparece em 100% dos agendamentos que tiveram pelo menos uma remarcacao.
- **SC-005**: O resumo financeiro reflete corretamente os filtros aplicados e separa concluidos, cancelados e a realizar.
- **SC-006**: A tela permite concluir, cancelar ou remarcar um agendamento "A realizar" sem navegar para fora da listagem.
- **SC-007**: Validacoes de horario impedem salvar ou remarcar agendamentos com previsao de termino anterior ou igual ao inicio.

## Assumptions

- Produto e servico sao entidades ja cadastradas no sistema e possuem preco.
- Nesta versao, cada agendamento possui um produto e um servico. Futuramente, o modelo pode evoluir para multiplos itens por agendamento.
- O valor total do agendamento e uma fotografia do momento da criacao; mudancas futuras no catalogo nao alteram agendamentos existentes.
- "A realizar" representa agendamentos ainda nao concluidos e nao cancelados, mesmo que estejam em data futura ou no mesmo dia.
- "Remarcado" e uma tag complementar, nao um status independente.
- Usuarios Financeiros podem consultar agenda e totais, mas nao podem criar nem alterar status.
- A tela sera usada principalmente em desktop, mas deve permanecer responsiva para tablets.
