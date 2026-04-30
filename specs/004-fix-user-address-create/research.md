# Research: Endereço na Criação de Clientes

**Feature**: 004-fix-user-address-create
**Date**: 2026-04-28

---

## Decisão 1: Gerenciamento de endereços em modo de criação

**Decision**: Acumular endereços em memória via Signal `draftAddresses: WritableSignal<CreateAddressInput[]>` no `client-form.page.ts` durante o modo de criação. Ao submeter o formulário, o array é incluído no `CreateClientInput`.

**Rationale**: No modo de edição, o `client ID` já existe e cada operação de endereço (add/update/delete) é uma chamada individual à API via `AddressesService`. No modo de criação, não há `client ID` ainda, portanto não é possível usar `AddressesService` diretamente. A solução mais simples e alinhada com os padrões do projeto é gerenciar um array local de endereços pendentes e submetê-los todos em conjunto no `POST /api/clients`.

**Alternatives considered**:
- Criar o cliente sem endereço primeiro e depois adicionar o endereço: Rejeitado — o backend exige ao menos 1 endereço na criação (`CreateClientInput.addresses` tem validação de minItems:1). Forçaria duas chamadas HTTP e um estado inconsistente temporário.
- Criar um componente separado para endereços em create mode: Rejeitado — complexidade desnecessária. O `AddressFormDialogComponent` já existente pode ser reutilizado via `MatDialog`, e a listagem de endereços pendentes é simples o suficiente para ser inline no `client-form.page.ts`.

---

## Decisão 2: Reutilização de componentes existentes

**Decision**: Reutilizar `AddressFormDialogComponent` diretamente no `client-form.page.ts` em modo de criação (sem modificar o componente existente). O dialog retorna `CreateAddressInput | null` no close, e o resultado é adicionado ao `draftAddresses`.

**Rationale**: O `AddressFormDialogComponent` já recebe dados de entrada via `MAT_DIALOG_DATA` e retorna os dados preenchidos ao fechar. Ele pode ser reutilizado tanto para adicionar um novo endereço (dados nulos) quanto para editar um endereço pendente (dados pré-preenchidos) sem nenhuma modificação.

**Alternatives considered**:
- Modificar `AddressesSectionComponent` para suportar o modo "pendente" (draft): Rejeitado — introduz complexidade no componente compartilhado e viola o princípio de responsabilidade única. O componente atual é projetado para operar sobre um cliente já existente via API.

---

## Decisão 3: Validação de "ao menos 1 endereço obrigatório"

**Decision**: Adicionar validação customizada no `client-form.page.ts` que verifica `draftAddresses().length >= 1` antes de permitir o submit. Exibir mensagem de erro inline na seção de endereços se o usuário tentar salvar sem endereço.

**Rationale**: A validação de endereços obrigatórios não pode ser feita via `FormControl` padrão (já que os endereços são gerenciados fora do `FormGroup`). Uma validação explícita no método `onSubmit()` é a abordagem mais direta e legível.

**Alternatives considered**:
- Desabilitar o botão "Salvar" até que ao menos 1 endereço exista: Implementado em complemento à validação — o botão ficará desabilitado enquanto `draftAddresses().length === 0 || clientForm.invalid`.

---

## Conclusão

Nenhuma dependência nova é necessária. Nenhuma alteração no backend. A fix é isolada no `client-form.page.ts` com reutilização total dos componentes e serviços já existentes.
