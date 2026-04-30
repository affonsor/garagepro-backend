# Data Model: Endereço na Criação de Clientes

**Feature**: 004-fix-user-address-create
**Date**: 2026-04-28
**Source**: Modelos existentes em `frontend/src/app/core/models/`

Este documento descreve o estado de dados introduzido para gerenciar endereços pendentes durante a criação de um cliente. Todos os tipos já existem — nenhum novo modelo de domínio é necessário.

---

## Estado Local no `ClientFormPage` (create mode)

### `draftAddresses: WritableSignal<CreateAddressInput[]>`

Sinal local que acumula endereços pendentes enquanto o formulário está em modo de criação ("new"). Inicializado como array vazio `[]`.

```
draftAddresses
  └── CreateAddressInput[]
        ├── type: AddressType          (obrigatório: 'Residential' | 'Billing' | 'Other')
        ├── street: string             (obrigatório)
        ├── number: string             (obrigatório)
        ├── complement?: string        (opcional)
        ├── district: string           (obrigatório)
        ├── city: string               (obrigatório)
        ├── state: string              (obrigatório, exatamente 2 chars)
        └── zipCode: string            (obrigatório)
```

**Ciclo de vida**:
- Inicializado como `[]` ao abrir o formulário de criação
- Cresce ao usuário confirmar o formulário de endereço no dialog
- Decresce quando o usuário remove um endereço pendente
- Submetido via `CreateClientInput.addresses` no `POST /api/clients`
- Descartado após navegação de retorno

### `draftAddressError: WritableSignal<string | null>`

Sinal para exibir erro de validação inline ("Ao menos 1 endereço é obrigatório") quando o usuário tenta submeter o formulário sem endereços.

---

## Tipos Existentes Reutilizados (sem alteração)

### `CreateAddressInput` (address.model.ts)

```typescript
export interface CreateAddressInput {
  type: AddressType;     // 'Residential' | 'Billing' | 'Other'
  street: string;
  number: string;
  complement?: string;
  district: string;
  city: string;
  state: string;         // exatamente 2 chars (UF)
  zipCode: string;
}
```

### `CreateClientInput` (client.model.ts) — campo `addresses` passa a ser preenchido

```typescript
export interface CreateClientInput {
  name: string;
  email?: string;
  phone?: string;
  document?: string;
  addresses: CreateAddressInput[];   // ← antes era sempre [] (bug); agora receberá draftAddresses()
}
```

---

## Fluxo de Estado

```
Usuário abre "Novo Cliente"
  → draftAddresses = []
  → Formulário de cliente visível + seção de endereços com mensagem "Nenhum endereço"
  → Botão "Adicionar Endereço" disponível

Usuário clica "Adicionar Endereço"
  → AddressFormDialogComponent abre
  → Usuário preenche e confirma
  → draftAddresses.update(arr => [...arr, novoEndereco])

Usuário clica "Salvar"
  → Validação: clientForm.invalid? → bloquear
  → Validação: draftAddresses().length === 0? → exibir draftAddressError
  → POST /api/clients com { ...formValues, addresses: draftAddresses() }

Usuário confirma exclusão de endereço pendente (não o único)
  → draftAddresses.update(arr => arr.filter((_, i) => i !== index))
```
