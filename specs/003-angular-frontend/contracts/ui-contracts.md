# UI Contracts: GaragePro Angular Frontend

**Feature**: 003-angular-frontend
**Date**: 2026-04-28

Define os contratos de interface de cada tela: estado que exibe, ações disponíveis, componentes compartilhados utilizados e comportamento por role.

---

## Componentes Compartilhados (shared/)

### `ConfirmDialogComponent`

**Input via `MAT_DIALOG_DATA`**:
```typescript
{ title: string; message: string; confirmLabel: string; severity: 'warn' | 'danger' }
```
**Output**: `boolean` (true = confirmado, false = cancelado)

**Usos**: exclusão de usuário, cliente, veículo, produto, serviço, endereço.

---

### `EmptyStateComponent`

**Inputs**: `@Input() icon: string`, `@Input() message: string`
**Usos**: toda listagem com `data.length === 0 && !loading`

---

### `PageHeaderComponent`

**Inputs**: `@Input() title: string`, `@Input() subtitle?: string`
**Content projection**: `<ng-content select="[actions]">` para botões de ação no canto direito.

---

### `ServerErrorBannerComponent`

**Input**: `@Input() error: string | null`
**Comportamento**: exibe `MatCard` com ícone de erro e `error` quando não-nulo; oculto quando null.

---

### `HasRoleDirective` (structural)

**Selector**: `*appHasRole="roles"`
**Comportamento**: renderiza o elemento somente se `AuthService.currentUser()?.roles` contém ao menos uma role da lista.

---

## Login (`/login`)

**Componente**: `LoginPage`

**Estado**:
| Signal | Tipo | Descrição |
|--------|------|-----------|
| `loading` | `Signal<boolean>` | Submit em andamento |
| `formError` | `Signal<string \| null>` | Erro 401 da API |

**Form** (`loginForm: FormGroup`):
| Campo | Tipo | Validações |
|-------|------|------------|
| `email` | `FormControl<string>` | required, email |
| `password` | `FormControl<string>` | required, minLength(6) |

**Ações**:
| Ação | Condição | Resultado |
|------|----------|-----------|
| Submit | form válido + !loading | `AuthService.login()` → navigate(returnUrl \| '/clients') |
| Submit inválido | form inválido | Exibe erros inline nos campos |
| Login 401 | — | `formError` = "E-mail ou senha incorretos" |

---

## Shell Layout

**Componentes**: `ShellComponent`, `SidebarComponent`, `HeaderComponent`

**SidebarComponent** — itens renderizados por role:
| Item | Roles | Rota |
|------|-------|------|
| Clientes | Admin, Technician, Financial | `/clients` |
| Veículos | Admin, Technician | `/vehicles` |
| Produtos | Admin, Financial | `/products` |
| Serviços | Admin, Financial | `/services` |
| Usuários | Admin | `/users` |

**HeaderComponent**:
- Exibe nome do usuário logado
- Menu dropdown: "Sair" → `AuthService.logout()`
- Toggle de dark mode (ícone sol/lua)

---

## Listagens (padrão comum a todos os recursos)

**Estado**:
| Signal | Tipo | Descrição |
|--------|------|-----------|
| `data` | `Signal<T[]>` | Registros da página atual |
| `pagination` | `Signal<PaginationMeta \| null>` | Metadados de paginação |
| `loading` | `Signal<boolean>` | Carregamento em andamento |
| `pageNumber` | `Signal<number>` | Página atual (init: 1) |
| `pageSize` | `Signal<number>` | Itens por página (init: 20) |

**Estrutura visual**:
1. `PageHeaderComponent` com botão "Novo" (oculto para roles sem escrita)
2. `MatProgressBar` quando `loading()`
3. `MatTable` com colunas definidas por recurso
4. `MatPaginator` integrado a `pagination()`
5. `EmptyStateComponent` quando `data().length === 0 && !loading()`

---

## Clientes — Listagem (`/clients`)

**Colunas da tabela**: Nome, E-mail, Telefone, Nº de Veículos, Data de Cadastro, Ações

**Ações por linha** (conforme role):
| Ação | Roles com acesso |
|------|-----------------|
| Editar (ícone lápis) | Admin, Technician, Financial (leitura) |
| Excluir (ícone lixeira) | Admin, Technician |

---

## Clientes — Detalhe/Form (`/clients/new`, `/clients/:id/edit`)

**Estado adicional**:
| Signal | Tipo | Descrição |
|--------|------|-----------|
| `isNew` | `boolean` | Derivado da rota (sem `:id` = novo) |
| `saving` | `Signal<boolean>` | Save em andamento |
| `loadingDetail` | `Signal<boolean>` | Carregamento do detalhe |
| `readOnly` | `Signal<boolean>` | True para Financial |

**Form principal** (`clientForm: FormGroup`):
| Campo | Validações |
|-------|------------|
| `name` | required, maxLength(200) |
| `email` | email, maxLength(256) |
| `phone` | maxLength(20) |
| `document` | maxLength(20), máscara CPF/CNPJ |

**Seção de Endereços** (`AddressesSectionComponent`) — somente no edit (não no new):
- Exibe lista de endereços existentes
- Botão "Adicionar Endereço" (Admin + Technician)
- Por endereço: botões Editar e Excluir (ambos Admin + Technician)
- Formulário de endereço inlined ou via `MatDialog`:

| Campo | Validações |
|-------|------------|
| `type` | required, enum AddressType |
| `street` | required, maxLength(200) |
| `number` | required, maxLength(20) |
| `complement` | maxLength(100) |
| `district` | required, maxLength(100) |
| `city` | required, maxLength(100) |
| `state` | required, minLength(2), maxLength(2) |
| `zipCode` | required, maxLength(9) |

**Seção de Veículos** (somente no edit, leitura):
- Lista os veículos do cliente com placa, marca, modelo e ano
- Link para `/vehicles/:id/edit`
- Sem ações de CRUD aqui (gerenciado em `/vehicles`)

**Ações**:
| Ação | Condição |
|------|----------|
| Salvar | form válido + !saving + !readOnly |
| Cancelar | sempre → volta para `/clients` |

---

## Veículos — Listagem (`/vehicles`)

**Filtro opcional**: seletor de cliente (carrega `ClientSummary[]` via `ClientsService.list()`) — limpa com "×"

**Colunas**: Placa, Marca, Modelo, Ano, Cor, Proprietário Atual, Ações

**Ações por linha**: Editar, Excluir (Admin + Technician apenas)

---

## Veículos — Detalhe/Form (`/vehicles/new`, `/vehicles/:id/edit`)

**Form principal** (`vehicleForm: FormGroup`):
| Campo | Validações |
|-------|------------|
| `clientId` | required (seleção de cliente via autocomplete) |
| `licensePlate` | required, maxLength(10) — desabilitado no edit |
| `make` | required, maxLength(100) |
| `model` | required, maxLength(100) |
| `year` | required, min(1900), max(currentYear+1) |
| `color` | required, maxLength(50) |
| `vin` | maxLength(17) |

**Seção Transferência** (somente no edit, roles Admin + Technician):
- Botão "Transferir Veículo" → abre `TransferDialogComponent`

**`TransferDialogComponent`**:
- Campo `toClientId`: autocomplete de clientes (excluindo proprietário atual)
- Campo `notes`: textarea
- Validação: `toClientId` obrigatório
- Após sucesso: fecha dialog + recarrega detalhe do veículo + SnackBar "Transferência realizada"

**Seção Histórico de Transferências** (somente no edit):
- Tabela: De, Para, Data, Observações
- Exibida mesmo que vazia (estado vazio com mensagem)

---

## Usuários — Listagem (`/users`)

**Colunas**: Nome, E-mail, Perfis, Data de Cadastro, Ações

**Ações por linha**: Editar, Excluir

---

## Usuários — Detalhe/Form (`/users/new`, `/users/:id/edit`)

**Form** (`userForm: FormGroup`):
| Campo | Validações |
|-------|------------|
| `name` | required, maxLength(200) |
| `email` | required, email, maxLength(256) |
| `password` | required (somente no new), minLength(8) |
| `roles` | required, min 1 item selecionado (checkboxes: Admin, Technician, Financial) |

---

## Produtos — Listagem (`/products`)

**Colunas**: Nome, Descrição, Preço, Data de Cadastro, Ações

**Ações por linha**: Editar, Excluir

---

## Produtos — Detalhe/Form (`/products/new`, `/products/:id/edit`)

**Form** (`productForm: FormGroup`):
| Campo | Validações |
|-------|------------|
| `name` | required, maxLength(200) |
| `description` | maxLength(500) |
| `price` | required, min(0), padrão monetário BRL |

---

## Serviços — Listagem e Form (`/services`)

**Idêntico aos Produtos** — mesmas colunas, mesmo form, mesma lógica.

---

## Padrões de feedback (todos os forms/listagens)

| Evento | Comportamento |
|--------|---------------|
| Operação 2xx | `MatSnackBar` 3s: "Salvo com sucesso" / "Removido com sucesso" |
| Erro 400 (campo) | `setErrors({ apiError: mensagem })` no controle + erro inline |
| Erro 400 (negócio) | `ServerErrorBannerComponent` exibe `error` da resposta |
| Erro 409 (conflito) | `setErrors({ apiError: 'Já existe um registro com esse valor' })` no campo relevante |
| Erro 404 | SnackBar 6s: "Registro não encontrado" + navigate para listagem |
| Erro 500 | SnackBar 6s: "Erro interno. Tente novamente." |
| Carregamento | `MatProgressSpinner` ou `MatProgressBar` visível; botões desabilitados |
