# Tasks: Endereço na Criação de Clientes

**Input**: Design documents from `/specs/004-fix-user-address-create/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Não solicitados explicitamente — sem tarefas de teste unitário. Validação via fluxos do `quickstart.md`.

**Organização**: Tarefas agrupadas por fase de entrega. A fix é cirúrgica: 2 arquivos modificados, 2 User Stories.

## Formato: `[ID] [P?] [Story] Descrição`

- **[P]**: Pode ser executado em paralelo (arquivos diferentes, sem dependências incompletas)
- **[Story]**: User Story correspondente (US1–US2)

---

## Phase 1: Foundational (Pré-requisito compartilhado)

**Objetivo**: Adaptar o `AddressFormDialogComponent` para suportar o modo "draft" (sem `clientId`), bloqueando as user stories até estar completo.

**⚠️ CRÍTICO**: US1 depende desta fase — o dialog deve retornar dados localmente quando `clientId` é `null`, sem chamar a API.

- [X] T001 Modificar `frontend/src/app/features/clients/detail/address-form-dialog.component.ts`: tornar `clientId` opcional na interface `DialogData` (`clientId: string | null`); em `onSubmit()`, se `clientId === null`, fechar o dialog com o objeto `CreateAddressInput` diretamente (sem chamar `AddressesService`); se `clientId !== null`, manter o comportamento atual de chamar a API e fechar com `true`

**Checkpoint**: `AddressFormDialogComponent` funciona em dois modos — modo API (edit, clientId presente) e modo draft (create, clientId null). Nenhuma regressão na funcionalidade de edição de endereços existente.

---

## Phase 2: User Story 1 — Endereço no Cadastro de Novo Cliente (P1) 🎯 MVP

**Objetivo**: Ao criar um novo cliente, o usuário pode adicionar, editar e remover endereços pendentes antes de salvar. O formulário exige ao menos um endereço para permitir o envio.

**Teste Independente**: Abrir `/clients/new`, adicionar um endereço, preencher o nome do cliente e clicar em "Salvar" → cliente criado com endereço. Verificar que o botão "Salvar" fica desabilitado enquanto `draftAddresses` estiver vazio.

- [X] T002 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, adicionar Signals: `draftAddresses = signal<CreateAddressInput[]>([])` e `draftAddressError = signal<string | null>(null)`; importar `CreateAddressInput` de `address.model.ts`

- [X] T003 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, adicionar método `openAddAddressDialog()` que abre `AddressFormDialogComponent` com `{ clientId: null, address: null }`; ao fechar com dado não-nulo do tipo `CreateAddressInput`, executa `draftAddresses.update(arr => [...arr, result])`; importar `MatDialog` e `AddressFormDialogComponent`

- [X] T004 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, adicionar método `editDraftAddress(index: number)` que abre `AddressFormDialogComponent` pré-preenchido com `{ clientId: null, address: draftAddresses()[index] }` (adaptado para `Address`-like); ao confirmar, substitui o item no índice com `draftAddresses.update(arr => arr.map((a, i) => i === index ? result : a))`

- [X] T005 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, adicionar método `removeDraftAddress(index: number)`: se `draftAddresses().length === 1`, exibe `MatSnackBar` 6s ("Ao menos um endereço é obrigatório.") e retorna sem remover; se `length > 1`, abre `ConfirmDialogComponent` e, ao confirmar, executa `draftAddresses.update(arr => arr.filter((_, i) => i !== index))`; importar `ConfirmDialogComponent`

- [X] T006 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, no template, adicionar bloco `@if (isNew)` com seção de endereços pendentes: exibir `EmptyStateComponent` quando `draftAddresses().length === 0` com mensagem "Nenhum endereço adicionado. Ao menos 1 é necessário."; listar endereços com linha `Tipo • Logradouro, Nº — Cidade/UF` com botões "Editar" (`aria-label="Editar endereço"`) e "Excluir" (`aria-label="Excluir endereço"`); exibir `mat-error` com `draftAddressError()` quando não-nulo; botão `[P] + Adicionar Endereço` com `*appHasRole="['Admin', 'Technician']"`; importar `EmptyStateComponent` e `HasRoleDirective`

- [X] T007 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, atualizar o binding `[disabled]` do botão "Salvar" para: `saving() || clientForm.invalid || (isNew && draftAddresses().length === 0)`

- [X] T008 [US1] Em `frontend/src/app/features/clients/detail/client-form.page.ts`, no método `onSubmit()`, substituir `addresses: []` por `addresses: this.draftAddresses()`; adicionar validação antes do submit: se `isNew && draftAddresses().length === 0`, definir `draftAddressError.set('Adicione ao menos um endereço.')` e retornar sem enviar; limpar `draftAddressError.set(null)` ao iniciar um submit válido

**Checkpoint**: US1 completa. Criar cliente com endereço funciona de ponta a ponta. Botão desabilitado sem endereços. Erros de validação exibidos inline.

---

## Phase 3: User Story 2 — Edição de Endereços em Cliente Existente sem Regressão (P2)

**Objetivo**: Garantir que a funcionalidade de endereços no modo de edição (`/clients/:id/edit`) continua operando corretamente após as mudanças do T001 no `AddressFormDialogComponent`.

**Teste Independente**: Abrir cliente existente, adicionar novo endereço (dialog chama API via `AddressesService`), editar endereço, remover endereço com mais de 1 → todos os fluxos funcionam como antes da fix.

- [X] T009 [US2] Validar que `AddressesSectionComponent` em `frontend/src/app/features/clients/detail/addresses-section.component.ts` continua abrindo `AddressFormDialogComponent` com `{ clientId: client.id, address: null | Address }` — modo API — e que o fluxo de salvar/editar/excluir endereços de clientes existentes não foi afetado pela mudança do T001

- [ ] T010 [US2] Executar os fluxos de regressão do `quickstart.md` (seção "Fluxo de regressão — Edição de cliente existente"): abrir cliente existente, adicionar, editar e remover endereços e confirmar comportamento inalterado

**Checkpoint**: US2 completa. Nenhuma regressão. Modo edit preservado.

---

## Phase 4: Polish

**Objetivo**: Limpeza e validação final.

- [X] T011 [P] Executar `ng lint` em `frontend/` e corrigir quaisquer violações introduzidas pelas mudanças
- [X] T012 [P] Executar `ng build` em `frontend/` e confirmar zero erros de compilação TypeScript
- [ ] T013 Validar fluxo completo do `quickstart.md`: criar cliente com endereço, múltiplos endereços, remoção, edição de endereço pendente, regressão de edição

---

## Dependências e Ordem de Execução

### Dependências entre fases

- **Foundational (Phase 1)**: Sem dependências — iniciar imediatamente
- **US1 (Phase 2)**: Depende de T001 (Phase 1 completa)
- **US2 (Phase 3)**: Depende de T001 (Phase 1 completa); pode rodar em paralelo com US1
- **Polish (Phase 4)**: Depende de US1 e US2 completas

### Dependências dentro de US1

```
T001 (dialog draft mode)
  ↓
T002 (signals no client-form)
  ↓
T003, T004, T005   ← podem rodar em paralelo (métodos independentes)
  ↓
T006 (template — usa os métodos acima)
  ↓
T007, T008   ← podem rodar em paralelo (botão e onSubmit independentes)
```

### Oportunidades de paralelismo

```
Phase 1:
  T001 (único arquivo — sequencial)

Phase 2 — após T001:
  T002 → T003 || T004 || T005 → T006 → T007 || T008

Phase 3 — após T001 (paralelo com Phase 2 se recursos disponíveis):
  T009 || T010

Phase 4 — após Phases 2 e 3:
  T011 || T012 → T013
```

---

## Estratégia de Implementação

### MVP (US1)

1. Completar Phase 1 (T001 — dialog draft mode)
2. Completar Phase 2 (T002–T008 — client-form create mode)
3. **PARAR e VALIDAR**: Criar cliente com endereço via `/clients/new`
4. Executar T011, T012, T013 (lint, build, quickstart)

### Entrega Completa

1. T001 → base
2. T002–T008 → US1 funcional → **validar MVP**
3. T009–T010 → US2 regressão verificada
4. T011–T013 → Polish → **entrega pronta**

---

## Notas

- **[P]** = arquivos diferentes ou métodos independentes, sem dependências entre si
- Fix é cirúrgica: apenas 2 arquivos fonte são modificados (`address-form-dialog.component.ts` e `client-form.page.ts`)
- O `AddressesSectionComponent` existente **não é modificado** — US2 é apenas validação de regressão
- Commitar após Phase 1 (T001) e novamente após Phase 2 (T008)
