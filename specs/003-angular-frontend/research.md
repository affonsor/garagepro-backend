# Research: GaragePro Angular Frontend

**Feature**: 003-angular-frontend
**Date**: 2026-04-28
**Status**: Phase 0 Complete

Este documento consolida as decisões técnicas tomadas para o frontend, baseadas na Constituição (princípio VII), na especificação da API existente e em melhores práticas do ecossistema Angular 21.

---

## 1. Bootstrap da aplicação Angular 21

**Decision**: Usar `bootstrapApplication()` com `app.config.ts` contendo `provideRouter`, `provideHttpClient(withInterceptors([...]))`, `provideAnimationsAsync()` e `provideAnimationsAsync('noop')` somente em testes.

**Rationale**: Angular 21 elimina `AppModule` em favor de Standalone APIs. `bootstrapApplication` é a forma recomendada e exigida pela Constitution VII (NgModule proibido).

**Alternatives considered**: NgModule legado — rejeitado pela constituição.

---

## 2. Roteamento e Lazy Loading

**Decision**: Rotas raiz em `app.routes.ts` apontam para arquivos `{recurso}.routes.ts` via `loadChildren: () => import('./features/{recurso}/{recurso}.routes').then(m => m.routes)`. Rotas filhas usam `loadComponent` para páginas individuais quando possível.

**Rationale**: Constituição exige lazy loading para todas as features. `loadChildren` mantém o bundle inicial enxuto e permite que cada feature evolua independentemente.

**Alternatives considered**: Eager loading — proibido pela constituição.

---

## 3. Gerenciamento de Estado

**Decision**:
- **Signals** (`signal`, `computed`, `effect`) para estado local de página/componente.
- **`AuthService`** mantém `BehaviorSubject<AuthUser | null>` interno + Signal exposto (`readonly currentUser = toSignal(...)`).
- Sem Redux/NgRx. Estado de listagens vive nos componentes smart e é recarregado em mudanças de paginação/filtro.

**Rationale**: Princípio VII obriga Signals para estado local; `BehaviorSubject` é permitido apenas em serviço global com múltiplos subscribers — `AuthService` se enquadra. Listagens server-side com paginação não justificam um store global.

**Alternatives considered**: NgRx — overkill para o escopo CRUD; SignalStore (`@ngrx/signals`) — não introduz benefício mensurável aqui.

---

## 4. HTTP Clients tipados por recurso

**Decision**: Um serviço por recurso em `core/http/`, expondo métodos retornando `Observable<T>` fortemente tipados sobre `HttpClient`.

Exemplo (assinatura):

```ts
// users.service.ts
list(query: PageQuery): Observable<PaginatedResult<User>>
getById(id: string): Observable<User>
create(input: CreateUserInput): Observable<{ id: string }>
update(id: string, input: UpdateUserInput): Observable<{ id: string }>
delete(id: string): Observable<void>
```

**Rationale**: Princípio VII obriga um serviço por recurso. `Observable<T>` é o contrato natural do `HttpClient`; converter para Signal acontece nos componentes via `toSignal()`.

**Alternatives considered**: Facade pattern envolvendo Signals — adiciona indireção sem ganho neste escopo.

---

## 5. Interceptores HTTP

**Decision**: Dois interceptores funcionais (`HttpInterceptorFn`):
- `jwtInterceptor`: lê `localStorage.getItem('garagepro_token')` e adiciona `Authorization: Bearer ...` quando presente. Pula a rota `/api/auth/login`.
- `errorInterceptor`: captura `HttpErrorResponse`. Em 401 → chama `AuthService.logout()` + `Router.navigate(['/login'], { queryParams: { returnUrl } })`. Demais erros são repassados para o componente decidir.

**Rationale**: Princípio VII exige interceptor JWT centralizado. Tratamento global de 401 evita repetição de lógica em cada serviço.

**Alternatives considered**: Class-based `HttpInterceptor` — válido, mas a forma funcional (`provideHttpClient(withInterceptors([...]))`) é a recomendada em Angular 21.

---

## 6. Guards de rota

**Decision**:
- `authGuard: CanMatchFn` — verifica se há token válido; redireciona para `/login?returnUrl=` caso contrário.
- `roleGuard: CanMatchFn` — recebe roles permitidas via `data: { roles: ['Admin', 'Technician'] }` na rota; nega acesso se o usuário não possui ao menos uma role.

**Rationale**: `CanMatchFn` é mais performático que `CanActivate` em rotas lazy — evita carregar o chunk se o usuário não tem acesso. Funções puras alinham com Standalone API.

**Alternatives considered**: `CanActivate` legado — funciona mas baixa o chunk antes de validar; rejeitado.

---

## 7. Forms e validação

**Decision**: **Reactive Forms** (`FormGroup`, `FormControl`, `FormBuilder`) exclusivamente. Validações client-side espelham os validators do backend (ver [`api-reference.md`](../002-core-crud-api/contracts/api-reference.md)):

| Campo | Regra |
|-------|-------|
| `email` | `Validators.required`, `Validators.email`, `Validators.maxLength(256)` |
| `password` (create) | `Validators.required`, `Validators.minLength(8)` |
| `name` | `Validators.required`, `Validators.maxLength(200)` |
| `roles` | `Validators.required`, custom `minLength(1)` em array |
| `state` (UF) | `Validators.required`, `Validators.minLength(2)`, `Validators.maxLength(2)` |
| `licensePlate` | `Validators.required`, `Validators.maxLength(10)` |
| `year` | `Validators.required`, `Validators.min(1900)`, `Validators.max(currentYear+1)` |
| `price` | `Validators.required`, `Validators.min(0)` |

Erros 400 da API (`{ error, errors: ["campo: detalhe"] }`) são parseados e aplicados via `formGroup.get(campo)?.setErrors({ apiError: detalhe })`.

**Rationale**: Princípio VII proíbe template-driven forms e exige espelhamento de validações + exibição inline de erros 400.

**Alternatives considered**: Template-driven — proibido; Angular Signal Forms — ainda em fase developer preview na 21, mantemos Reactive Forms para estabilidade.

---

## 8. Design System — Angular Material 3

**Decision**: Tema único definido em `src/styles/theme.scss` usando `@use '@angular/material' as mat;` + `mat.theme(...)`. Paleta: primária `azure`, secundária `cyan`. Tipografia: escala padrão Material 3. Densidade: `0` (default).

Componentes Material adotados:
- `MatToolbar`, `MatSidenav` — layout shell
- `MatTable` + `MatPaginator` + `MatSort` — listagens
- `MatFormField`, `MatInput`, `MatSelect`, `MatDatepicker` — forms
- `MatButton`, `MatIcon`, `MatMenu` — ações
- `MatDialog` — confirmações
- `MatSnackBar` — feedback
- `MatProgressSpinner`, `MatProgressBar` — loading

**Rationale**: Constituição obriga Material 3. Customização via `mat.theme` + CSS custom properties evita sobrescrever seletores internos.

**Alternatives considered**: Tailwind, shadcn (presente em `package.json` mas inadequado — é React) — rejeitados pela constituição.

---

## 9. Dark mode

**Decision**: Implementação dupla: (a) detecção automática via `@media (prefers-color-scheme: dark)` aplicando `light-dark()` nos tokens Material 3; (b) toggle manual no `HeaderComponent` que define `data-theme="dark|light"` no `<html>` e persiste em `localStorage` chave `garagepro_theme`. Override de `prefers-color-scheme` quando há preferência manual salva.

**Rationale**: Princípio VII exige `prefers-color-scheme` + toggle manual.

**Alternatives considered**: Apenas auto — não atende constituição; apenas manual — não atende constituição.

---

## 10. Testes unitários

**Decision**: **Jest** via `@angular-builders/jest`. Cobertura mínima recomendada: serviços HTTP (mocks de `HttpTestingController`), guards/interceptors (funções puras testáveis isoladamente), validações de Reactive Forms, lógica de seleção de role na sidebar.

**Rationale**: Jest é mais rápido que Karma, watch mode robusto, suporta `--findRelatedTests`. Convenção `should{Resultado}_when{Condicao}` alinha com a do backend (princípio VI).

**Alternatives considered**: Karma + Jasmine (default Angular) — ainda funciona mas Jest é tendência consolidada e mais leve para CI.

---

## 11. Estrutura de listagens com paginação

**Decision**: Cada `*-list.page.ts` mantém Signals `pageNumber`, `pageSize`, `data`, `pagination`, `loading`. Inicializa `pageNumber=1`, `pageSize=20`. `MatPaginator` emite eventos que atualizam Signals e disparam `effect()` que chama o serviço HTTP. Filtros (ex: `clientId` em vehicles) também são Signals que entram no `effect`.

Empty state: quando `data().length === 0 && !loading()`, renderiza `<app-empty-state>`.

**Rationale**: Modelo unificado entre as 6 listagens. Signal-driven evita gerenciar subscriptions manualmente.

**Alternatives considered**: `MatTableDataSource` client-side — incompatível com paginação server-side da API.

---

## 12. Confirmação de exclusão e feedback

**Decision**:
- `ConfirmDialogComponent` em `shared/components/confirm-dialog/` — recebe `{ title, message, confirmLabel, severity }` via `MAT_DIALOG_DATA`. Retorna `boolean`.
- Após DELETE bem-sucedido: `MatSnackBar.open('Removido com sucesso', 'Fechar', { duration: 3000 })`.
- Após DELETE 400 (regra de negócio): `MatSnackBar.open(error.error, 'Fechar', { duration: 6000, panelClass: 'snackbar-error' })`.

**Rationale**: Princípio VII exige `MatDialog` para destruição e `MatSnackBar` 3s/6s.

**Alternatives considered**: `confirm()` nativo — proibido.

---

## 13. Controle de acesso por role na UI

**Decision**:
- `AuthService.roles: Signal<UserRole[]>` derivado do user.
- `*appHasRole="['Admin','Technician']"` (StructuralDirective) — esconde elementos quando o usuário não tem nenhuma das roles listadas.
- `roleGuard` aplicado em `loadChildren` para evitar carregar chunks de features inacessíveis.
- Sidebar: itens montados a partir de uma lista que cada um declara `roles: UserRole[]`. Filtra antes de renderizar.

Mapa role → acesso (espelho do backend):

| Recurso     | Admin | Technician | Financial |
|-------------|-------|------------|-----------|
| Users       | CRUD  | —          | —         |
| Clients     | CRUD  | CRUD       | Read      |
| Addresses   | CRUD  | CRUD       | —         |
| Vehicles    | CRUD+T| CRUD+T     | —         |
| Products    | CRUD  | —          | CRUD      |
| Services    | CRUD  | —          | CRUD      |

**Rationale**: Princípio VII exige espelhar permissões do backend; `roleGuard` em rotas lazy é o método mais eficiente.

**Alternatives considered**: Validar role apenas no template — funciona mas baixa chunks desnecessários; rejeitado.

---

## 14. Endpoint base e ambientes

**Decision**: `environment.ts` define `apiBaseUrl: 'https://localhost:44384/api'` para desenvolvimento; `environment.prod.ts` lê de `process.env`/build replacement (a definir em deploy). Todos os serviços HTTP usam `${environment.apiBaseUrl}/{recurso}`.

**Rationale**: Padrão Angular consolidado; centraliza configuração.

**Alternatives considered**: `provideAppInitializer()` carregando JSON externo — overkill para single-tenant local-first.

---

## 15. Documento e máscara de CPF/CNPJ

**Decision**: Pipe `documentMaskPipe` em `shared/pipes/` aplica máscara ao exibir (`123.456.789-00` ou `12.345.678/0001-90`). Para input, usar Diretiva `[appDocumentMask]` em `shared/directives/` que usa `ControlValueAccessor` para gravar dígitos puros no `FormControl` e exibir mascarado.

**Rationale**: API armazena documento como dígitos crus (string). UX exige máscara visual.

**Alternatives considered**: `ngx-mask` — dependência extra; preferimos minimalismo aqui.

---

## 16. Acessibilidade

**Decision**: Confiar nos componentes Material (todos WAI-ARIA). Garantir labels em todos os `MatFormField`. Botões de ação com `aria-label` quando exibirem só ícone. Skip-link no shell para pular ao conteúdo. Foco visível preservado (sem `outline: none`).

**Rationale**: Material já cobre 90% dos requisitos de acessibilidade; o resto é conduta no template.

---

## Resumo de decisões

| Tópico | Decisão |
|--------|---------|
| Framework | Angular 21 Standalone + Signals |
| UI | Angular Material 3, tema único + dark mode |
| Forms | Reactive Forms |
| HTTP | `HttpClient` + interceptores funcionais (JWT, Erro) |
| Guards | `CanMatchFn` (auth + role) |
| Estado | Signals (local) + `BehaviorSubject` (AuthService global) |
| Lazy loading | Obrigatório, em todas as features |
| Testes | Jest + `HttpTestingController` |
| Auth storage | `localStorage['garagepro_token']` |
| Estrutura | `frontend/` na raiz, isolado de `src/` (.NET) |
| Idioma | PT-BR fixo |
| Plataforma alvo | Desktop modernos, viewport mínimo 360px |

Todas as `NEEDS CLARIFICATION` resolvidas. Pronto para Phase 1.
