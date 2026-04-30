# Mapa de Rotas: GaragePro Angular Frontend

**Feature**: 003-angular-frontend
**Date**: 2026-04-28

---

## Visão geral

```
/
├── login                           ← pública (sem authGuard)
└── (shell layout)                  ← authGuard: CanMatchFn
    ├── users                       ← roleGuard: ['Admin']
    │   ├── (list)                  ← /users
    │   ├── new                     ← /users/new
    │   └── :id/edit                ← /users/:id/edit
    ├── clients                     ← roleGuard: ['Admin','Technician','Financial']
    │   ├── (list)                  ← /clients
    │   ├── new                     ← /clients/new
    │   └── :id/edit                ← /clients/:id/edit
    ├── vehicles                    ← roleGuard: ['Admin','Technician']
    │   ├── (list)                  ← /vehicles
    │   ├── new                     ← /vehicles/new
    │   └── :id/edit                ← /vehicles/:id/edit
    ├── products                    ← roleGuard: ['Admin','Financial']
    │   ├── (list)                  ← /products
    │   ├── new                     ← /products/new
    │   └── :id/edit                ← /products/:id/edit
    └── services                    ← roleGuard: ['Admin','Financial']
        ├── (list)                  ← /services
        ├── new                     ← /services/new
        └── :id/edit                ← /services/:id/edit
```

Endereços não têm rotas próprias — são gerenciados na tela de detalhe do cliente (`/clients/:id/edit`).

Transferência de veículo é um `MatDialog` aberto a partir de `/vehicles/:id/edit` — não uma rota separada.

---

## Tabela de rotas

| Rota | Componente | Guards | Roles |
|------|-----------|--------|-------|
| `/login` | `LoginPage` | — | público |
| `/users` | `UsersListPage` | `authGuard`, `roleGuard` | Admin |
| `/users/new` | `UserFormPage` | `authGuard`, `roleGuard` | Admin |
| `/users/:id/edit` | `UserFormPage` | `authGuard`, `roleGuard` | Admin |
| `/clients` | `ClientsListPage` | `authGuard`, `roleGuard` | Admin, Technician, Financial |
| `/clients/new` | `ClientFormPage` | `authGuard`, `roleGuard` | Admin, Technician |
| `/clients/:id/edit` | `ClientFormPage` | `authGuard`, `roleGuard` | Admin, Technician, Financial* |
| `/vehicles` | `VehiclesListPage` | `authGuard`, `roleGuard` | Admin, Technician |
| `/vehicles/new` | `VehicleFormPage` | `authGuard`, `roleGuard` | Admin, Technician |
| `/vehicles/:id/edit` | `VehicleFormPage` | `authGuard`, `roleGuard` | Admin, Technician |
| `/products` | `ProductsListPage` | `authGuard`, `roleGuard` | Admin, Financial |
| `/products/new` | `ProductFormPage` | `authGuard`, `roleGuard` | Admin, Financial |
| `/products/:id/edit` | `ProductFormPage` | `authGuard`, `roleGuard` | Admin, Financial |
| `/services` | `ServicesListPage` | `authGuard`, `roleGuard` | Admin, Financial |
| `/services/new` | `ServiceFormPage` | `authGuard`, `roleGuard` | Admin, Financial |
| `/services/:id/edit` | `ServiceFormPage` | `authGuard`, `roleGuard` | Admin, Financial |

\* Financial pode **acessar** `/clients/:id/edit` em modo leitura (form desabilitado); não pode acessar `/clients/new`.

---

## Comportamento de guards

### `authGuard: CanMatchFn`

```
1. Lê token de localStorage['garagepro_token']
2. Se ausente ou expirado → redireciona para /login?returnUrl=<rota_tentada>
3. Se válido → permite navegação
```

### `roleGuard: CanMatchFn`

```
1. Lê roles do AuthService.currentUser()
2. Verifica interseção com route.data.roles
3. Se vazio → redireciona para primeira rota acessível ao role
4. Se com interseção → permite navegação
```

### `JwtInterceptor`

```
1. Pula requisições para /api/auth/login
2. Adiciona header: Authorization: Bearer <token>
```

### `ErrorInterceptor`

```
1. Captura HttpErrorResponse
2. Status 401 → AuthService.logout() → Router.navigate(['/login'], { queryParams: { returnUrl } })
3. Demais erros → throwError(err) para tratamento no componente
```

---

## Redirecionamentos padrão

| Situação | Destino |
|----------|---------|
| `/` (raiz) | `/clients` (todos) |
| Acesso negado por role | `/clients` (ou primeiro item permitido na sidebar) |
| Após login bem-sucedido | `returnUrl` ou `/clients` |
| Após logout | `/login` |
| Rota não encontrada (404) | `/login` (fallback) |

---

## Lazy loading — configuração `app.routes.ts`

```typescript
export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login/login.page').then(m => m.LoginPage) },
  {
    path: '',
    component: ShellComponent,
    canMatch: [authGuard],
    children: [
      { path: 'users', canMatch: [roleGuard], data: { roles: ['Admin'] },
        loadChildren: () => import('./features/users/users.routes').then(m => m.routes) },
      { path: 'clients', canMatch: [roleGuard], data: { roles: ['Admin','Technician','Financial'] },
        loadChildren: () => import('./features/clients/clients.routes').then(m => m.routes) },
      { path: 'vehicles', canMatch: [roleGuard], data: { roles: ['Admin','Technician'] },
        loadChildren: () => import('./features/vehicles/vehicles.routes').then(m => m.routes) },
      { path: 'products', canMatch: [roleGuard], data: { roles: ['Admin','Financial'] },
        loadChildren: () => import('./features/products/products.routes').then(m => m.routes) },
      { path: 'services', canMatch: [roleGuard], data: { roles: ['Admin','Financial'] },
        loadChildren: () => import('./features/services/services.routes').then(m => m.routes) },
      { path: '', redirectTo: 'clients', pathMatch: 'full' },
    ]
  },
  { path: '**', redirectTo: 'login' },
];
```
