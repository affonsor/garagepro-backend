# Data Model: GaragePro Angular Frontend

**Feature**: 003-angular-frontend
**Date**: 2026-04-28
**Source**: [`api-reference.md`](../002-core-crud-api/contracts/api-reference.md)

Este arquivo define as interfaces TypeScript que o frontend mantém em `core/models/`. São espelhos dos contratos da API — sem lógica, sem anotações de framework.

---

## pagination.model.ts

```typescript
export interface PageQuery {
  pageNumber: number;
  pageSize: number;
}

export interface PaginationMeta {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginatedResult<T> {
  data: T[];
  pagination: PaginationMeta;
}
```

**Origem**: convenção global da API (`?pageNumber=1&pageSize=20`, envelope `{ data, pagination }`).

---

## api-error.model.ts

```typescript
export interface ApiError {
  error: string;
  errors?: string[];
}
```

**Origem**: estrutura de erro padronizada para 400, 401, 403, 404, 409, 500.

---

## user.model.ts

```typescript
export type UserRole = 'Admin' | 'Technician' | 'Financial';

export interface User {
  id: string;
  name: string;
  email: string;
  roles: UserRole[];
  createdAt: string;
}

export interface CreateUserInput {
  name: string;
  email: string;
  password: string;
  roles: UserRole[];
}

export interface UpdateUserInput {
  name: string;
  email: string;
  roles: UserRole[];
}

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  roles: UserRole[];
}

export interface LoginInput {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  user: AuthUser;
}
```

**Restrições**:
- `name`: max 200 chars
- `email`: formato válido, max 256 chars, único
- `password` (create only): min 8 chars
- `roles`: mínimo 1 valor, enum `UserRole`

---

## client.model.ts

```typescript
export interface ClientSummary {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  vehicleCount: number;
  createdAt: string;
}

export interface ClientDetail {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  document: string | null;
  addresses: Address[];
  vehicles: VehicleSummary[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateClientInput {
  name: string;
  email?: string;
  phone?: string;
  document?: string;
  addresses: CreateAddressInput[];
}

export interface UpdateClientInput {
  name: string;
  email?: string;
  phone?: string;
  document?: string;
}
```

**Restrições**:
- `name`: obrigatório, max 200 chars
- `addresses`: mínimo 1 item no create
- Exclusão bloqueada se `vehicleCount > 0`

---

## address.model.ts

```typescript
export type AddressType = 'Residential' | 'Billing' | 'Other';

export interface Address {
  id: string;
  type: AddressType;
  street: string;
  number: string;
  complement: string | null;
  district: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface CreateAddressInput {
  type: AddressType;
  street: string;
  number: string;
  complement?: string;
  district: string;
  city: string;
  state: string;
  zipCode: string;
}

export type UpdateAddressInput = CreateAddressInput;
```

**Restrições**:
- `type`: enum `AddressType`
- `street`, `number`, `district`, `city`, `state`, `zipCode`: obrigatórios
- `state`: exatamente 2 caracteres (UF)
- Exclusão bloqueada se for o último endereço do cliente

---

## vehicle.model.ts

```typescript
export interface VehicleSummary {
  id: string;
  licensePlate: string;
  make: string;
  model: string;
  year: number;
  color: string;
  currentOwner: ClientRef;
}

export interface VehicleDetail {
  id: string;
  licensePlate: string;
  make: string;
  model: string;
  year: number;
  color: string;
  vin: string | null;
  currentOwner: ClientRef;
  transferHistory: VehicleTransfer[];
  createdAt: string;
  updatedAt: string;
}

export interface ClientRef {
  id: string;
  name: string;
}

export interface VehicleTransfer {
  id: string;
  fromClient: ClientRef;
  toClient: ClientRef;
  transferredAt: string;
  notes: string | null;
}

export interface CreateVehicleInput {
  clientId: string;
  licensePlate: string;
  make: string;
  model: string;
  year: number;
  color: string;
  vin?: string;
}

export interface UpdateVehicleInput {
  make: string;
  model: string;
  year: number;
  color: string;
  vin?: string;
}

export interface TransferVehicleInput {
  toClientId: string;
  notes?: string;
}

export interface TransferVehicleResponse {
  transferRecordId: string;
  transferredAt: string;
}
```

**Restrições**:
- `licensePlate`: max 10, único no sistema
- `make`, `model`: obrigatórios, max 100
- `year`: entre 1900 e `currentYear + 1`
- `toClientId` na transferência: deve diferir do proprietário atual

---

## product.model.ts

```typescript
export interface Product {
  id: string;
  name: string;
  description: string | null;
  price: number;
  createdAt: string;
}

export interface CreateProductInput {
  name: string;
  description?: string;
  price: number;
}

export type UpdateProductInput = CreateProductInput;
```

**Restrições**:
- `name`: obrigatório, max 200 chars
- `price`: obrigatório, >= 0

---

## service.model.ts

```typescript
export interface Service {
  id: string;
  name: string;
  description: string | null;
  price: number;
  createdAt: string;
}

export interface CreateServiceInput {
  name: string;
  description?: string;
  price: number;
}

export type UpdateServiceInput = CreateServiceInput;
```

**Restrições**: idênticas às de `Product`.

---

## Relacionamentos

```
AuthUser (1) ←── JWT payload ─── localStorage
User    (N)
Client (N) ──< Address (1..N)
Client (1) ──< Vehicle (N)
Vehicle (1) ──< VehicleTransfer (N)
Product (N)
Service (N)
```

- Um `Client` deve ter **no mínimo 1** `Address`.
- Um `Vehicle` tem exatamente **1** `currentOwner` (Client), mas o histórico de `VehicleTransfer` pode ter vários registros.
- `Product` e `Service` são independentes — sem relação entre si ou com `Client`/`Vehicle` nesta versão.

---

## Estado de autenticação

```
Não autenticado ──login OK──> Autenticado
Autenticado ──401 recebido──> Não autenticado (token limpo)
Autenticado ──logout()──> Não autenticado
```

`AuthService` persiste o token em `localStorage['garagepro_token']` e expõe `currentUser: Signal<AuthUser | null>`. Quando `null`, `authGuard` redireciona para `/login`.
