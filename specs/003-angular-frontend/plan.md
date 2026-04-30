# Implementation Plan: GaragePro Angular Frontend

**Branch**: `004-angular-frontend` | **Date**: 2026-04-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-angular-frontend/spec.md`

## Summary

Construção do frontend web do GaragePro consumindo a API REST documentada em [`specs/002-core-crud-api/contracts/api-reference.md`](../002-core-crud-api/contracts/api-reference.md). Aplicação **Angular 21** baseada em **Standalone Components** + **Signals**, com lazy loading por recurso, Reactive Forms, Angular Material 3 como design system, autenticação JWT via interceptor e controle de acesso por role (Admin / Technician / Financial) espelhando exatamente as permissões impostas pelo backend. O projeto frontend será adicionado em `frontend/` na raiz do repositório, isolado da solução .NET existente em `src/`.

## Technical Context

**Language/Version**: TypeScript 5.6 / Angular 21 (Standalone APIs)
**Primary Dependencies**: Angular 21 (`@angular/core`, `@angular/router`, `@angular/forms`, `@angular/common/http`), Angular Material 3 (`@angular/material`, `@angular/cdk`), RxJS 7.8
**Storage**: `localStorage` para token JWT (chave `garagepro_token`); estado de UI em Angular Signals
**Testing**: Jest 29 + `@angular-builders/jest` (testes unitários de componentes, serviços e guards)
**Target Platform**: Navegadores desktop modernos (Chrome 120+, Edge 120+, Firefox 120+, Safari 17+); viewport mínimo 360px
**Project Type**: Web application (frontend SPA consumindo backend REST existente)
**Performance Goals**: First Contentful Paint < 2s em conexão 4G; troca de rota lazy < 500ms; nenhuma listagem com mais de 20 itens carregada de uma vez (paginação server-side)
**Constraints**: Sem mobile nativo; sem PWA/offline; sem internacionalização (PT-BR fixo); sem alteração de senha (endpoint não exposto); sem consulta automática de CEP; consumo exclusivo da API REST — sem MCP no cliente
**Scale/Scope**: Aproximadamente 25 telas (login + 6 listagens + 6 detalhes/forms + transferência de veículo + endereços + confirmações), 7 serviços HTTP tipados, 3 guards/interceptors, 1 layout shell

## Constitution Check

*GATE: Verificado antes do Phase 0. Revalidado após Phase 1.*

### I. Clean Architecture — Separação de Camadas
✅ **APROVADO** — Princípio aplicável ao backend; o frontend respeita por construção pois consome apenas endpoints REST documentados sem acessar persistência.

### II. CQRS com MediatR
✅ **APROVADO** — Aplicável ao backend; o frontend consome os endpoints já segregados em Commands/Queries via HTTP.

### III. Result Pattern — Retorno Padronizado
✅ **APROVADO** — Frontend consome os payloads padronizados (`{ id }` para escrita, `{ data, pagination }` para leitura paginada, `{ error, errors[] }` para falhas) exatamente como definido pela API.

### IV. Pipeline de Validação via ValidationBehavior
✅ **APROVADO** — Validação client-side espelhará as regras dos validators do backend (FR/Validações em [`api-reference.md`](../002-core-crud-api/contracts/api-reference.md)). Erros 400 retornados pela API serão aplicados via `setErrors()` nos controles do `FormGroup`.

### V. Repository Pattern — Abstração de Persistência
✅ **APROVADO** — Aplicável ao backend.

### VI. Disciplina de Testes Unitários
✅ **APROVADO** — Os testes unitários do frontend seguem o mesmo princípio: serviços HTTP com `HttpTestingController`, guards/interceptors com mocks, componentes com `TestBed`. Sem chamadas reais à API. Convenção análoga: `should{Resultado}_when{Condicao}`.

### VII. Frontend — Angular 21 (Standalone Architecture)
✅ **APROVADO** — Arquitetura proposta cumpre integralmente:
- Standalone Components (sem NgModule)
- Estrutura `core/` (auth, http, models), `shared/`, `features/{recurso}/{routes,list,detail}`, `layout/`
- Signals para estado local; `BehaviorSubject` apenas em `AuthService` global
- Lazy loading em todas as features (`loadChildren`)
- Um serviço HTTP por recurso em `core/http/` (`AuthService`, `UsersService`, `ClientsService`, `AddressesService`, `VehiclesService`, `ProductsService`, `ServicesService`)
- `JwtInterceptor` adiciona Bearer token automaticamente; `ErrorInterceptor` trata 401 → logout + redirect
- Smart pages × dumb shared components
- Angular Material 3 com tema único em `src/styles/theme.scss`; sem cores hex fora do tema; tipografia da escala Material 3
- Reactive Forms exclusivos; sem template-driven
- `MatProgressSpinner` em ações async; `MatSnackBar` para feedback (3s sucesso / 6s erro); `MatDialog` para confirmação de DELETE; `MatPaginator` integrado à paginação da API; empty states em todas as listagens
- Token JWT em `localStorage` chave `garagepro_token`; `AuthGuard` em todas as rotas exceto `/login`; expiração → `/login?returnUrl=`
- Ações ocultas/desabilitadas conforme role (Admin/Technician/Financial)
- `strict: true` no `tsconfig`, ESLint + `@angular-eslint`, Prettier; sem `any`, sem `console.log`, sem `document.querySelector`
- Comentários em pt-br

**Resultado: Nenhuma violação detectada. Sem bloqueios.**

## Project Structure

### Documentation (this feature)

```text
specs/003-angular-frontend/
├── plan.md              ← Este arquivo (/speckit-plan)
├── research.md          ← Phase 0 output (/speckit-plan)
├── data-model.md        ← Phase 1 output (/speckit-plan)
├── quickstart.md        ← Phase 1 output (/speckit-plan)
├── contracts/
│   ├── routes.md        ← Mapa de rotas e guards
│   └── ui-contracts.md  ← Contratos de tela (inputs/outputs/estados)
├── checklists/
│   └── requirements.md
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
GaragePro.slnx
src/                                      ← Backend .NET 10 existente (não alterado)
├── GaragePro.API/
├── GaragePro.Application/
├── GaragePro.Core/
└── GaragePro.Infrastructure/

tests/
└── GaragePro.UnitTests/                  ← Backend tests existentes

frontend/                                 ← NOVO — projeto Angular 21
├── package.json
├── angular.json
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.spec.json
├── jest.config.ts
├── .eslintrc.json
├── .prettierrc
├── public/
│   └── favicon.ico
└── src/
    ├── main.ts                           ← bootstrapApplication(AppComponent, appConfig)
    ├── index.html
    ├── styles.scss                       ← imports globais
    ├── styles/
    │   └── theme.scss                    ← Material 3 tema único (paleta + tipografia)
    ├── environments/
    │   ├── environment.ts                ← API base URL dev (http://localhost:44384)
    │   └── environment.prod.ts
    └── app/
        ├── app.component.ts              ← shell mínimo (router-outlet)
        ├── app.config.ts                 ← provideRouter, provideHttpClient, providers globais
        ├── app.routes.ts                 ← rotas raiz (lazy loadChildren)
        ├── core/
        │   ├── auth/
        │   │   ├── auth.service.ts       ← login, logout, currentUser signal, hasRole()
        │   │   ├── auth.guard.ts         ← CanMatchFn protegendo rotas
        │   │   ├── role.guard.ts         ← CanMatchFn por role (data: { roles: [...] })
        │   │   ├── jwt.interceptor.ts    ← adiciona Authorization header
        │   │   └── error.interceptor.ts  ← trata 401 → logout + /login?returnUrl
        │   ├── http/
        │   │   ├── users.service.ts
        │   │   ├── clients.service.ts
        │   │   ├── addresses.service.ts
        │   │   ├── vehicles.service.ts
        │   │   ├── products.service.ts
        │   │   └── services.service.ts
        │   └── models/
        │       ├── pagination.model.ts   ← PaginatedResult<T>, PageQuery
        │       ├── api-error.model.ts
        │       ├── user.model.ts
        │       ├── client.model.ts
        │       ├── address.model.ts
        │       ├── vehicle.model.ts
        │       ├── product.model.ts
        │       └── service.model.ts
        ├── shared/
        │   ├── components/
        │   │   ├── confirm-dialog/       ← MatDialog padronizado para DELETE
        │   │   ├── empty-state/          ← ilustração + texto para listagens vazias
        │   │   ├── page-header/          ← título + ações
        │   │   └── server-error-banner/  ← exibe `error` global de respostas 500/400
        │   ├── directives/
        │   │   └── has-role.directive.ts ← *appHasRole="['Admin','Technician']"
        │   └── pipes/
        │       └── document-mask.pipe.ts ← máscara de CPF/CNPJ
        ├── layout/
        │   ├── shell.component.ts        ← sidebar + header + main outlet
        │   ├── sidebar.component.ts      ← navegação por role
        │   └── header.component.ts       ← user menu, logout, dark mode toggle
        └── features/
            ├── auth/
            │   ├── auth.routes.ts
            │   └── login/
            │       └── login.page.ts
            ├── users/
            │   ├── users.routes.ts
            │   ├── list/users-list.page.ts
            │   └── detail/user-form.page.ts
            ├── clients/
            │   ├── clients.routes.ts
            │   ├── list/clients-list.page.ts
            │   └── detail/
            │       ├── client-form.page.ts
            │       └── addresses-section.component.ts
            ├── vehicles/
            │   ├── vehicles.routes.ts
            │   ├── list/vehicles-list.page.ts
            │   ├── detail/vehicle-form.page.ts
            │   └── transfer/transfer-dialog.component.ts
            ├── products/
            │   ├── products.routes.ts
            │   ├── list/products-list.page.ts
            │   └── detail/product-form.page.ts
            └── services/
                ├── services.routes.ts
                ├── list/services-list.page.ts
                └── detail/service-form.page.ts
```

**Structure Decision**: Frontend Angular 21 isolado em `frontend/` na raiz, espelhando a separação clara entre solução backend (`src/` + `tests/`) e cliente web. Isso evita conflitos com a estrutura .NET, mantém os builds independentes, permite que cada projeto tenha seu próprio toolchain (npm + Angular CLI vs `dotnet`) e respeita a Constitution VII para Angular. A organização interna (`core/auth`, `core/http`, `core/models`, `shared/`, `features/{recurso}`, `layout/`) é exatamente a definida no princípio VII.

## Complexity Tracking

> Nenhuma violação da Constituição detectada. Seção não aplicável.
