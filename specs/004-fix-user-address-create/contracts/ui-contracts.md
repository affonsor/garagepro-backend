# UI Contracts: Endereço na Criação de Clientes

**Feature**: 004-fix-user-address-create
**Date**: 2026-04-28

---

## Tela: Formulário de Novo Cliente (`/clients/new`)

### Diferença em relação ao estado atual

| Elemento | Estado atual (bugado) | Estado corrigido |
|---|---|---|
| Seção "Endereços" | Ausente em modo create | Visível em modo create |
| Botão "Adicionar Endereço" | Ausente em modo create | Visível em modo create |
| Botão "Salvar" | Habilitado sem endereço | Desabilitado se `draftAddresses().length === 0` |
| Payload enviado ao backend | `addresses: []` | `addresses: draftAddresses()` |

---

## Contrato: `ClientFormPage` — Modo Criação ("new")

### Entradas (estado inicial)

| Signal / Campo | Tipo | Valor inicial | Descrição |
|---|---|---|---|
| `isNew` | `boolean` | `true` (sem `:id` na URL) | Determina o modo |
| `draftAddresses` | `WritableSignal<CreateAddressInput[]>` | `[]` | Endereços pendentes |
| `draftAddressError` | `WritableSignal<string \| null>` | `null` | Erro de validação da seção |
| `clientForm` | `FormGroup` | Campos em branco | Dados do cliente |

### Seção de Endereços (visível em create mode)

```
┌─ Endereços ─────────────────────────────────────────────────────────┐
│                                                                      │
│  @if (draftAddresses().length === 0)                                 │
│    [Empty state: "Nenhum endereço adicionado. Ao menos 1 é          │
│     necessário para criar o cliente."]                               │
│                                                                      │
│  @for (addr of draftAddresses(); track idx)                          │
│    [Linha: Tipo • Logradouro, Nº — Cidade/UF]   [Editar] [Excluir] │
│                                                                      │
│  @if (draftAddressError())                                           │
│    <mat-error> draftAddressError() </mat-error>                     │
│                                                                      │
│  [+ Adicionar Endereço]  (sempre visível, visível para Admin/Tecnico)│
└──────────────────────────────────────────────────────────────────────┘
```

### Interações

| Ação do usuário | Condição | Resultado |
|---|---|---|
| Clica "Adicionar Endereço" | — | Abre `AddressFormDialogComponent` com dados nulos |
| Confirma dialog de endereço | Todos os campos obrigatórios preenchidos | `draftAddresses.update(arr => [...arr, addr])` |
| Cancela dialog de endereço | — | Nenhuma alteração em `draftAddresses` |
| Clica "Editar" em endereço pendente | — | Abre `AddressFormDialogComponent` pré-preenchido; ao confirmar, substitui o item no índice |
| Clica "Excluir" em endereço pendente (não é o único) | `draftAddresses().length > 1` | `ConfirmDialogComponent` → remove do array |
| Clica "Excluir" em endereço pendente (é o único) | `draftAddresses().length === 1` | `MatSnackBar` 6s: "Ao menos um endereço é obrigatório." — sem remoção |
| Clica "Salvar" do formulário do cliente | `draftAddresses().length === 0` | `draftAddressError.set('Adicione ao menos um endereço.')` — não envia |
| Clica "Salvar" do formulário do cliente | `clientForm.invalid` | Validação padrão do form — não envia |
| Clica "Salvar" do formulário do cliente | Form válido + `draftAddresses().length >= 1` | `POST /api/clients` com `addresses: draftAddresses()` |

### Botão "Salvar"

```
[disabled] = saving() || clientForm.invalid || (isNew && draftAddresses().length === 0)
```

---

## Contrato: `AddressFormDialogComponent` (reutilizado sem alteração)

O dialog já suporta ambos os modos via `MAT_DIALOG_DATA`:

- `{ address: null, clientId: null }` → modo criação (sem chamada à API)
- `{ address: Address, clientId: string }` → modo edição (chama `AddressesService`)

**Em create mode do cliente**: Abrir com `{ address: null, clientId: null }`.
O dialog retorna `CreateAddressInput | null` via `MatDialogRef.close(result)`.

> **Nota**: O `AddressFormDialogComponent` atual pode chamar `AddressesService.add()` ao fechar. É necessário verificar se ele faz a chamada API internamente ou apenas retorna os dados. Se chamar a API internamente, um pequeno ajuste será necessário para suportar o modo "apenas retorno de dados sem chamada API".

---

## Rota sem alteração

| Rota | Componente | Guard | Mudança |
|---|---|---|---|
| `/clients/new` | `ClientFormPage` | `roleGuard(['Admin','Technician','Financial'])` | Nenhuma mudança na rota |
| `/clients/:id/edit` | `ClientFormPage` | `roleGuard(['Admin','Technician','Financial'])` | Sem alteração |
