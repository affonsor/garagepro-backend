# Tasks: GaragePro Angular Frontend

**Input**: Design documents from `/specs/003-angular-frontend/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Não solicitados explicitamente — tarefas de teste unitário incluídas apenas nos serviços HTTP e guards (camadas críticas).

**Organização**: Tarefas agrupadas por User Story para implementação e teste independentes.

## Formato: `[ID] [P?] [Story] Descrição`

- **[P]**: Pode ser executado em paralelo (arquivos diferentes, sem dependências incompletas)
- **[Story]**: User Story correspondente (US1–US6)

---

## Phase 1: Setup (Scaffold do Projeto Angular)

**Objetivo**: Criar o projeto Angular 21 em `frontend/` com todas as ferramentas configuradas.

- [X] T001 Criar projeto Angular 21 Standalone em `frontend/` via `ng new garagepro-web --directory frontend --style scss --routing true --standalone true --strict true`
- [X] T002 Instalar Angular Material 3 via `ng add @angular/material` (tema customizado, tipografia global, animações) em `frontend/`
- [X] T003 [P] Configurar Jest em `frontend/jest.config.ts`, `frontend/tsconfig.spec.json` e `frontend/package.json` (scripts `test`, `test:watch`) com `jest-preset-angular` e `@angular-builders/jest`
- [X] T004 [P] Configurar ESLint + `@angular-eslint` em `frontend/.eslintrc.json` com regras: sem `any`, sem `console.log`, `@typescript-eslint/no-explicit-any: error`
- [X] T005 [P] Configurar Prettier em `frontend/.prettierrc` (singleQuote: true, semi: true, printWidth: 100)
- [X] T006 Criar estrutura de diretórios em `frontend/src/app/`: `core/auth/`, `core/http/`, `core/models/`, `shared/components/`, `shared/directives/`, `shared/pipes/`, `layout/`, `features/auth/`, `features/users/`, `features/clients/`, `features/vehicles/`, `features/products/`, `features/services/`

---

## Phase 2: Foundational (Pré-requisitos que bloqueiam todas as User Stories)

**Objetivo**: Infraestrutura central sem a qual nenhuma feature pode ser implementada.

**⚠️ CRÍTICO**: Nenhuma User Story pode começar antes desta fase estar completa.

### Models (core/models/)

- [X] T007 [P] Criar `frontend/src/app/core/models/pagination.model.ts` com `PageQuery`, `PaginationMeta`, `PaginatedResult<T>` conforme `data-model.md`
- [X] T008 [P] Criar `frontend/src/app/core/models/api-error.model.ts` com `ApiError { error: string; errors?: string[] }`
- [X] T009 [P] Criar `frontend/src/app/core/models/user.model.ts` com `UserRole`, `User`, `CreateUserInput`, `UpdateUserInput`, `AuthUser`, `LoginInput`, `LoginResponse`
- [X] T010 [P] Criar `frontend/src/app/core/models/client.model.ts` com `ClientSummary`, `ClientDetail`, `CreateClientInput`, `UpdateClientInput`
- [X] T011 [P] Criar `frontend/src/app/core/models/address.model.ts` com `AddressType`, `Address`, `CreateAddressInput`, `UpdateAddressInput`
- [X] T012 [P] Criar `frontend/src/app/core/models/vehicle.model.ts` com `VehicleSummary`, `VehicleDetail`, `ClientRef`, `VehicleTransfer`, `CreateVehicleInput`, `UpdateVehicleInput`, `TransferVehicleInput`, `TransferVehicleResponse`
- [X] T013 [P] Criar `frontend/src/app/core/models/product.model.ts` com `Product`, `CreateProductInput`, `UpdateProductInput`
- [X] T014 [P] Criar `frontend/src/app/core/models/service.model.ts` com `Service`, `CreateServiceInput`, `UpdateServiceInput`

### Tema e Ambiente

- [X] T015 [P] Criar tema Material 3 em `frontend/src/styles/theme.scss` com paleta primária `azure` e secundária `cyan` via `@use '@angular/material' as mat; @include mat.theme(...)` e suporte a dark mode via `prefers-color-scheme`
- [X] T016 [P] Configurar `frontend/src/environments/environment.ts` (`apiBaseUrl: 'https://localhost:44384/api'`) e `environment.prod.ts`

### Auth Infrastructure (core/auth/)

- [X] T017 Criar `frontend/src/app/core/auth/auth.service.ts` com `BehaviorSubject<AuthUser | null>`, Signal `currentUser`, `roles`, métodos `login()`, `logout()`, `hasRole()`, persistência em `localStorage['garagepro_token']`
- [X] T018 Criar `frontend/src/app/core/auth/jwt.interceptor.ts` como `HttpInterceptorFn` que lê `localStorage['garagepro_token']` e adiciona `Authorization: Bearer` em todos os requests exceto `/api/auth/login`
- [X] T019 Criar `frontend/src/app/core/auth/error.interceptor.ts` como `HttpInterceptorFn` que captura 401 → `AuthService.logout()` + `Router.navigate(['/login'], { queryParams: { returnUrl } })`
- [X] T020 Criar `frontend/src/app/core/auth/auth.guard.ts` como `CanMatchFn` que verifica token válido e redireciona para `/login?returnUrl=` se ausente/expirado
- [X] T021 Criar `frontend/src/app/core/auth/role.guard.ts` como `CanMatchFn` que verifica `route.data.roles` contra `AuthService.roles()` e redireciona para `/clients` se sem permissão

### App Config e Rotas

- [X] T022 Criar `frontend/src/app/app.config.ts` com `provideRouter(routes)`, `provideHttpClient(withInterceptors([jwtInterceptor, errorInterceptor]))`, `provideAnimationsAsync()`
- [X] T023 Criar `frontend/src/app/app.routes.ts` com rotas raiz: `/login` (eager), shell com `authGuard` e `loadChildren` para todas as 5 features; redirect `/` → `/clients`; wildcard → `/login` (conforme `contracts/routes.md`)
- [X] T024 Criar `frontend/src/app/app.component.ts` como Standalone Component mínimo com `<router-outlet>`

### Layout Shell

- [X] T025 Criar `frontend/src/app/layout/shell.component.ts` com `MatSidenav`, `MatToolbar` e `<router-outlet>` principal; integra `SidebarComponent` e `HeaderComponent`
- [X] T026 [P] Criar `frontend/src/app/layout/sidebar.component.ts` com lista de itens de navegação filtrada por `AuthService.roles()` (clientes, veículos, produtos, serviços, usuários) usando `MatNavList`
- [X] T027 [P] Criar `frontend/src/app/layout/header.component.ts` com nome do usuário logado, menu dropdown "Sair", toggle dark mode (ícone sol/lua → alterna atributo `data-theme` em `<html>` + persiste em `localStorage['garagepro_theme']`)

### Shared Components e Utilitários

- [X] T028 [P] Criar `frontend/src/app/shared/components/confirm-dialog/confirm-dialog.component.ts` com `MAT_DIALOG_DATA: { title, message, confirmLabel, severity }` e retorno `boolean` via `MatDialogRef`
- [X] T029 [P] Criar `frontend/src/app/shared/components/empty-state/empty-state.component.ts` com `@Input() icon` e `@Input() message` usando `MatIcon` e tipografia Material 3
- [X] T030 [P] Criar `frontend/src/app/shared/components/page-header/page-header.component.ts` com `@Input() title`, `@Input() subtitle?` e `<ng-content select="[actions]">`
- [X] T031 [P] Criar `frontend/src/app/shared/components/server-error-banner/server-error-banner.component.ts` com `@Input() error: string | null` que exibe `MatCard` de erro quando não-nulo
- [X] T032 [P] Criar `frontend/src/app/shared/directives/has-role.directive.ts` como `StructuralDirective` (`*appHasRole="['Admin']"`) que injeta `AuthService` e renderiza elemento condicionalmente por role
- [X] T033 [P] Criar `frontend/src/app/shared/pipes/document-mask.pipe.ts` como `PipeTransform` que aplica máscara CPF (`000.000.000-00`) ou CNPJ (`00.000.000/0000-00`) conforme comprimento do valor

**Checkpoint**: Infraestrutura completa. A aplicação deve inicializar, exibir `/login`, e redirecionar rotas protegidas para `/login`. Todas as User Stories podem começar.

---

## Phase 3: User Story 1 — Autenticação e Acesso (P1) 🎯 MVP

**Objetivo**: Usuário consegue fazer login, ser redirecionado conforme seu perfil e fazer logout com segurança.

**Teste Independente**: Acessar `http://localhost:4200`, ser redirecionado para `/login`, logar com credenciais válidas e ser enviado para `/clients`; logar com credenciais inválidas e ver mensagem de erro; fazer logout e ser redirecionado para `/login`.

- [X] T034 [US1] Completar `frontend/src/app/core/auth/auth.service.ts` adicionando método `login(input: LoginInput): Observable<LoginResponse>` que chama `POST /api/auth/login`, armazena token em `localStorage['garagepro_token']`, atualiza `currentUser` Signal e retorna `LoginResponse`
- [X] T035 [US1] Criar `frontend/src/app/features/auth/auth.routes.ts` com rota `/login` apontando para `LoginPage`
- [X] T036 [US1] Criar `frontend/src/app/features/auth/login/login.page.ts` como Standalone Component com `loginForm: FormGroup` (`email`: required+email, `password`: required+minLength(6)), Signal `loading`, Signal `formError`, submit chama `AuthService.login()` → navega para `returnUrl` ou `/clients` em sucesso, define `formError = 'E-mail ou senha incorretos'` em 401
- [X] T037 [US1] Criar template de `login.page.ts` com `MatCard`, dois `MatFormField` (email + password), botão submit desabilitado durante `loading()`, exibição de `formError` abaixo do form e exibição de erros inline de validação por campo

**Checkpoint**: US1 completa. Login funcional com feedback de erro e proteção de rotas.

---

## Phase 4: User Story 2 — Gestão de Clientes (P1)

**Objetivo**: Admin e Técnico gerenciam clientes (CRUD completo); Financeiro visualiza lista e detalhe em modo leitura.

**Teste Independente**: Criar cliente com endereço, buscar na listagem paginada, abrir detalhe, editar nome, tentar excluir cliente com veículo vinculado (ver erro), excluir cliente sem veículo.

- [X] T038 [US2] Criar `frontend/src/app/core/http/clients.service.ts` com métodos tipados: `list(query: PageQuery): Observable<PaginatedResult<ClientSummary>>`, `getById(id: string): Observable<ClientDetail>`, `create(input: CreateClientInput): Observable<{ id: string }>`, `update(id, input: UpdateClientInput): Observable<{ id: string }>`, `delete(id): Observable<void>`
- [X] T039 [US2] Criar `frontend/src/app/core/http/addresses.service.ts` com métodos: `add(clientId, input: CreateAddressInput): Observable<{ id: string }>`, `update(clientId, addressId, input: UpdateAddressInput): Observable<{ id: string }>`, `delete(clientId, addressId): Observable<void>`
- [X] T040 [US2] Criar `frontend/src/app/features/clients/clients.routes.ts` com rotas: `''` → `ClientsListPage` (lazy), `'new'` → `ClientFormPage`, `':id/edit'` → `ClientFormPage`; `roleGuard` com `['Admin','Technician','Financial']`
- [X] T041 [US2] Criar `frontend/src/app/features/clients/list/clients-list.page.ts` com Signals `data`, `pagination`, `loading`, `pageNumber`, `pageSize`; `effect()` chama `ClientsService.list()`; `MatTable` com colunas Nome/E-mail/Telefone/Nº Veículos/Data/Ações; `MatPaginator`; botão "Novo Cliente" com `*appHasRole="['Admin','Technician']"`; exclusão abre `ConfirmDialogComponent` → `ClientsService.delete()` → SnackBar
- [X] T042 [US2] Criar `frontend/src/app/features/clients/detail/client-form.page.ts` com `clientForm: FormGroup` (name required maxLength(200), email email maxLength(256), phone maxLength(20), document maxLength(20)); detecta modo new/edit via `ActivatedRoute`; em edit carrega `ClientsService.getById()` e preenche form; define `readOnly = authService.hasRole('Financial')` desabilitando form; submit chama create ou update conforme modo; erros 400 aplicados via `setErrors()`
- [X] T043 [US2] Adicionar seção de veículos vinculados em `client-form.page.ts` (edit only): lista `VehicleSummary[]` do `ClientDetail` com placa/marca/modelo/ano e link `[routerLink]="['/vehicles', v.id, 'edit']"`; sem ações CRUD aqui

**Checkpoint**: US2 completa. CRUD de clientes funcional com paginação, validação e controle de acesso por role.

---

## Phase 5: User Story 3 — Gestão de Veículos e Transferência (P2)

**Objetivo**: Admin e Técnico gerenciam veículos (CRUD + transferência com histórico).

**Teste Independente**: Cadastrar veículo para um cliente, abrir detalhe com histórico vazio, transferir para outro cliente, verificar histórico atualizado, excluir veículo.

- [X] T044 [US3] Criar `frontend/src/app/core/http/vehicles.service.ts` com métodos: `list(query: PageQuery & { clientId?: string }): Observable<PaginatedResult<VehicleSummary>>`, `getById(id): Observable<VehicleDetail>`, `create(input: CreateVehicleInput): Observable<{ id: string }>`, `update(id, input: UpdateVehicleInput): Observable<{ id: string }>`, `delete(id): Observable<void>`, `transfer(id, input: TransferVehicleInput): Observable<TransferVehicleResponse>`
- [X] T045 [US3] Criar `frontend/src/app/features/vehicles/vehicles.routes.ts` com rotas: `''` → `VehiclesListPage`, `'new'` → `VehicleFormPage`, `':id/edit'` → `VehicleFormPage`; `roleGuard` com `['Admin','Technician']`
- [X] T046 [US3] Criar `frontend/src/app/features/vehicles/list/vehicles-list.page.ts` com filtro opcional por cliente (Signal `clientFilter`, `MatSelect` carregando `ClientsService.list({pageSize:100})`); colunas: Placa/Marca/Modelo/Ano/Cor/Proprietário/Ações; paginação e exclusão com `ConfirmDialogComponent`
- [X] T047 [US3] Criar `frontend/src/app/features/vehicles/detail/vehicle-form.page.ts` com `vehicleForm: FormGroup` (clientId required, licensePlate required maxLength(10) desabilitado em edit, make required maxLength(100), model required maxLength(100), year required min(1900) max(anoAtual+1), color required, vin); `MatAutocomplete` para clientId consumindo `ClientsService.list()`; seção histórico de transferências (`MatTable` com De/Para/Data/Observações + empty state)
- [X] T048 [US3] Criar `frontend/src/app/features/vehicles/transfer/transfer-dialog.component.ts` como `MatDialog` com `toClientId` (`MatAutocomplete` excluindo proprietário atual) e `notes` (textarea); submit chama `VehiclesService.transfer()`; fecha dialog em sucesso + SnackBar "Transferência realizada"; botão "Transferir Veículo" visível apenas para Admin+Technician no `vehicle-form.page.ts`

**Checkpoint**: US3 completa. Veículos com CRUD, filtro por cliente, transferência e histórico funcionais.

---

## Phase 6: User Story 4 — Gestão de Endereços (P2)

**Objetivo**: Admin e Técnico adicionam, editam e removem endereços no detalhe do cliente; regra "último endereço" tratada.

**Teste Independente**: Abrir detalhe de cliente existente, adicionar novo endereço, editar um endereço existente, tentar remover o único endereço (ver mensagem de bloqueio), remover endereço quando há mais de um.

- [X] T049 [US4] Criar `frontend/src/app/features/clients/detail/addresses-section.component.ts` como Standalone Component com `@Input() clientId`, `@Input() addresses: Address[]`, `@Output() changed = new EventEmitter<void>()`; lista endereços com tipo/logradouro/ações; botão "Adicionar Endereço" com `*appHasRole="['Admin','Technician']"`
- [X] T050 [US4] Adicionar `MatDialog` de formulário de endereço em `addresses-section.component.ts`: `addressForm: FormGroup` com todos os campos obrigatórios (type enum, street, number, complement optional, district, city, state minLength(2) maxLength(2), zipCode); submit chama `AddressesService.add()` ou `AddressesService.update()` conforme modo; emite `changed` para recarregar detalhe do cliente
- [X] T051 [US4] Implementar exclusão de endereço em `addresses-section.component.ts`: abre `ConfirmDialogComponent` → chama `AddressesService.delete()`; trata 400 "Client must have at least one address" exibindo `MatSnackBar` 6s com mensagem de negócio; emite `changed` em sucesso
- [X] T052 [US4] Integrar `AddressesSectionComponent` no `client-form.page.ts` (somente em modo edit): passa `clientId` e `addresses` do `ClientDetail`; em `(changed)` recarrega `ClientsService.getById()` e atualiza Signal

**Checkpoint**: US4 completa. Endereços gerenciáveis no detalhe do cliente com proteção da regra de negócio.

---

## Phase 7: User Story 5 — Gestão de Produtos e Serviços (P2)

**Objetivo**: Admin e Financeiro gerenciam catálogo de produtos e serviços (CRUD completo).

**Teste Independente**: Criar produto com nome e preço, listar, editar preço, excluir; repetir para serviço.

- [X] T053 [P] [US5] Criar `frontend/src/app/core/http/products.service.ts` com métodos: `list(query: PageQuery): Observable<PaginatedResult<Product>>`, `getById(id): Observable<Product>`, `create(input: CreateProductInput): Observable<{ id: string }>`, `update(id, input: UpdateProductInput): Observable<{ id: string }>`, `delete(id): Observable<void>`
- [X] T054 [P] [US5] Criar `frontend/src/app/core/http/services.service.ts` com mesma interface de `products.service.ts` adaptada para `Service`/`CreateServiceInput`/`UpdateServiceInput`
- [X] T055 [P] [US5] Criar `frontend/src/app/features/products/products.routes.ts` e `frontend/src/app/features/services/services.routes.ts` com rotas `''` (list), `'new'` (form), `':id/edit'` (form); `roleGuard` com `['Admin','Financial']`
- [X] T056 [P] [US5] Criar `frontend/src/app/features/products/list/products-list.page.ts` e `frontend/src/app/features/services/list/services-list.page.ts` com `MatTable` (colunas: Nome/Descrição/Preço/Data/Ações), paginação, exclusão com `ConfirmDialogComponent` e empty state
- [X] T057 [US5] Criar `frontend/src/app/features/products/detail/product-form.page.ts` e `frontend/src/app/features/services/detail/service-form.page.ts` com `FormGroup` (name required maxLength(200), description maxLength(500), price required min(0)); modos new/edit; SnackBar de feedback; erros inline 400

**Checkpoint**: US5 completa. Produtos e serviços com CRUD paginado funcionais.

---

## Phase 8: User Story 6 — Gestão de Usuários (P3)

**Objetivo**: Admin gerencia usuários do sistema (CRUD); outros perfis sem acesso.

**Teste Independente**: Logar como Admin, criar usuário com role Technician, listar, editar roles, excluir.

- [X] T058 [US6] Criar `frontend/src/app/core/http/users.service.ts` com métodos: `list(query: PageQuery): Observable<PaginatedResult<User>>`, `getById(id): Observable<User>`, `create(input: CreateUserInput): Observable<{ id: string }>`, `update(id, input: UpdateUserInput): Observable<{ id: string }>`, `delete(id): Observable<void>`
- [X] T059 [US6] Criar `frontend/src/app/features/users/users.routes.ts` com rotas `''` (list), `'new'` (form), `':id/edit'` (form); `roleGuard` com `['Admin']`
- [X] T060 [US6] Criar `frontend/src/app/features/users/list/users-list.page.ts` com `MatTable` (colunas: Nome/E-mail/Perfis/Data/Ações), paginação, exclusão com `ConfirmDialogComponent` e empty state
- [X] T061 [US6] Criar `frontend/src/app/features/users/detail/user-form.page.ts` com `FormGroup` (name required maxLength(200), email required email maxLength(256), password required minLength(8) somente em new via `Validators.required` condicional, roles checkboxes Admin/Technician/Financial com validação de ao menos 1 selecionado); modos new/edit; trata 409 (e-mail duplicado) com `setErrors()` no campo email

**Checkpoint**: US6 completa. Todas as 6 User Stories implementadas e funcionais.

---

## Phase 9: Polish e Preocupações Transversais

**Objetivo**: Finalização de UX, acessibilidade e validação do quickstart.

- [X] T062 [P] Implementar dark mode completo: adicionar `prefers-color-scheme` media query em `frontend/src/styles/theme.scss` e garantir que toggle manual do `header.component.ts` funciona em conjunto (override via `[data-theme="dark"]` no `html`)
- [X] T063 [P] Garantir empty states em todas as 6 listagens (`clients-list`, `vehicles-list`, `products-list`, `services-list`, `users-list` e histórico de transferências em `vehicle-form`): adicionar `EmptyStateComponent` com ícone e mensagem contextual
- [X] T064 [P] Adicionar `skip-link` em `frontend/src/index.html` e `aria-label` em todos os botões de ação com ícone nas listagens (editar/excluir/transferir) para acessibilidade WAI-ARIA
- [X] T065 [P] Adicionar testes unitários de `AuthService` em `frontend/src/app/core/auth/auth.service.spec.ts` (métodos `login`, `logout`, persistência de token) usando `HttpTestingController`; testes de `authGuard` e `roleGuard` como funções puras
- [X] T066 Executar `ng lint` e `npm test` sem erros; corrigir quaisquer violations de ESLint ou falhas de teste antes de encerrar
- [ ] T067 Validar fluxo do `quickstart.md`: subir backend via Docker, rodar `ng serve`, logar com cada uma das 3 credenciais de teste e verificar que menus e permissões estão corretos por role

---

## Dependências e Ordem de Execução

### Dependências entre fases

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: Depende de Setup — **bloqueia todas as User Stories**
- **US1 (Phase 3)**: Depende de Foundational — sem dependências de outras stories
- **US2 (Phase 4)**: Depende de Foundational — sem dependências de outras stories
- **US3 (Phase 5)**: Depende de Foundational; referencia `ClientsService` (criado em US2) — pode ser iniciada após US2
- **US4 (Phase 6)**: Depende de US2 (`ClientFormPage` e `AddressesService` devem existir)
- **US5 (Phase 7)**: Depende apenas de Foundational — paralela com US2/US3/US4
- **US6 (Phase 8)**: Depende apenas de Foundational — paralela com as demais
- **Polish (Phase 9)**: Depende de todas as stories desejadas

### Dependências dentro de cada story

- Models (T007–T014) → Services (T038, T044, etc.) → Pages/Components
- `AuthService` (T017) → Interceptors (T018, T019) → Guards (T020, T021) → Shell (T025–T027)
- `ConfirmDialogComponent` (T028) → qualquer página com exclusão
- `AddressesService` (T039) deve existir antes de `AddressesSectionComponent` (T049)

---

## Oportunidades de Paralelismo

### Phase 2 — Foundational

```
Executar em paralelo:
  T007-T014   (todos os models — arquivos independentes)
  T015-T016   (tema + ambientes — arquivos independentes)

Sequencial após models:
  T017 → T018 → T019 → T020 → T021   (auth chain)
  T022 → T023 → T024                 (app config chain)

Paralelo entre si (após T022-T024):
  T025-T027   (shell components)
  T028-T033   (shared components + directives + pipes)
```

### Phase 7 — US5 Produtos + Serviços

```
Executar em paralelo:
  T053  (products.service.ts)
  T054  (services.service.ts)
  T055  (routes de ambos)
  T056  (list pages de ambos)

Sequencial:
  T057  (form pages — após routes e services)
```

### Após Foundational — Stories paralelas (com equipe)

```
Dev A: US1 + US2 (P1 em sequência)
Dev B: US5 + US6 (P2/P3 independentes de US2)
Dev A (continuação): US3 → US4 (após US2)
```

---

## Estratégia de Implementação

### MVP (apenas US1)

1. Completar Phase 1 (Setup)
2. Completar Phase 2 (Foundational)
3. Completar Phase 3 (US1 — Login)
4. **PARAR e VALIDAR**: Aplicação inicializa, login funciona, rotas protegidas redirecionam corretamente
5. Deploy/demo se aprovado

### Entrega Incremental

1. Setup + Foundational → base pronta
2. US1 → login funcional → **demo MVP**
3. US2 → clientes com CRUD → **demo incremento 1**
4. US3 + US4 → veículos + endereços → **demo incremento 2**
5. US5 → produtos e serviços → **demo incremento 3**
6. US6 → gestão de usuários → **demo completo**
7. Polish → produção

---

## Notas

- **[P]** = arquivos diferentes, sem dependências entre si — podem rodar em paralelo
- **[Story]** = rastreabilidade para a User Story correspondente
- Cada User Story é independentemente testável após sua phase
- Em erros de build após cada task: executar `ng build` e corrigir antes de prosseguir
- Commitar após cada phase ou group lógico de tasks
- Tasks T007–T014 (models) devem ser as primeiras a serem revisadas pois são a base tipada de tudo
