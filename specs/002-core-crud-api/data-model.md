# Data Model: GaragePro Core CRUD API

**Branch**: `002-core-crud-api` | **Date**: 2026-04-27

## Visão Geral do Domínio

```
User ──(roles)──> UserRole[]

Client ──1:N──> Address
       ──1:N──> Vehicle ──1:N──> VehicleTransferRecord
                                       |── FromClient (Client FK)
                                       └── ToClient  (Client FK)

Product   (catálogo independente)
Service   (catálogo independente)
```

---

## Entidades

### User

| Campo        | Tipo            | Restrições                                      |
|-------------|-----------------|------------------------------------------------|
| Id           | Guid            | PK, gerado automaticamente                      |
| Name         | string          | NOT NULL, max 200                               |
| Email        | string          | NOT NULL, max 256, UNIQUE (índice único)        |
| PasswordHash | string          | NOT NULL (BCrypt hash, opaco ao domínio)        |
| Roles        | List\<UserRole\> | NOT NULL, mínimo 1 role obrigatório (FR-002)    |
| CreatedAt    | DateTimeOffset  | NOT NULL, UTC, default: agora                   |
| UpdatedAt    | DateTimeOffset  | NOT NULL, UTC, atualizado em cada modificação   |

**Mapeamento EF Core**:
- `Roles` → coluna `roles text[]` no PostgreSQL via `HasConversion` (enum para string array)
- Índice único em `Email`

**Regras de negócio**:
- `Roles` não pode ser vazio — validado no FluentValidation do Command e verificado no Handler
- Email deve ser único — Handler verifica via `IUserRepository.ExistsByEmail(email)`

---

### Client

| Campo     | Tipo           | Restrições                                   |
|-----------|----------------|----------------------------------------------|
| Id        | Guid           | PK, gerado automaticamente                   |
| Name      | string         | NOT NULL, max 200                            |
| Email     | string?        | nullable, max 256                            |
| Phone     | string?        | nullable, max 30                             |
| Document  | string?        | nullable, max 20 (CPF/CNPJ sem formatação)   |
| Addresses | List\<Address\> | navegação 1:N, mínimo 1 exigido no Create   |
| Vehicles  | List\<Vehicle\> | navegação 1:N                               |
| CreatedAt | DateTimeOffset | NOT NULL, UTC                                |
| UpdatedAt | DateTimeOffset | NOT NULL, UTC                                |

**Regras de negócio**:
- Ao criar Client, pelo menos 1 Address deve ser fornecido (FR-006)
- Deleção de Client com Vehicles vinculados é rejeitada (preservação de histórico)

---

### Address

| Campo      | Tipo          | Restrições                                          |
|------------|---------------|-----------------------------------------------------|
| Id         | Guid          | PK, gerado automaticamente                          |
| ClientId   | Guid          | FK → Client, NOT NULL                              |
| Type       | AddressType   | enum NOT NULL (Residential, Billing, Other)         |
| Street     | string        | NOT NULL, max 200                                   |
| Number     | string        | NOT NULL, max 20                                    |
| Complement | string?       | nullable, max 100                                   |
| District   | string        | NOT NULL, max 100                                   |
| City       | string        | NOT NULL, max 100                                   |
| State      | string        | NOT NULL, max 2 (código UF: SP, RJ, MG…)           |
| ZipCode    | string        | NOT NULL, max 10                                    |
| CreatedAt  | DateTimeOffset | NOT NULL, UTC                                      |

**Mapeamento EF Core**:
- `OnDelete(DeleteBehavior.Cascade)` — endereços removidos com o cliente

---

### Vehicle

| Campo        | Tipo           | Restrições                                           |
|-------------|----------------|------------------------------------------------------|
| Id           | Guid           | PK, gerado automaticamente                           |
| ClientId     | Guid           | FK → Client, NOT NULL (proprietário atual)           |
| LicensePlate | string         | NOT NULL, max 10, UNIQUE (índice único) (FR-009)    |
| Make         | string         | NOT NULL, max 100 (fabricante: Honda, Toyota…)       |
| Model        | string         | NOT NULL, max 100                                    |
| Year         | int            | NOT NULL, entre 1900 e ano atual + 1                 |
| Color        | string?        | nullable, max 50                                     |
| VIN          | string?        | nullable, max 17 (chassis)                           |
| CreatedAt    | DateTimeOffset | NOT NULL, UTC                                        |
| UpdatedAt    | DateTimeOffset | NOT NULL, UTC                                        |

**Mapeamento EF Core**:
- `OnDelete(DeleteBehavior.Restrict)` no FK → Client (não cascatear)
- Índice único em `LicensePlate`

**Regras de negócio**:
- Placa única no sistema — Handler verifica antes de criar
- Transferência: atualiza `ClientId` + cria `VehicleTransferRecord` na mesma transação

---

### VehicleTransferRecord

| Campo          | Tipo           | Restrições                                  |
|----------------|----------------|---------------------------------------------|
| Id             | Guid           | PK, gerado automaticamente                  |
| VehicleId      | Guid           | FK → Vehicle, NOT NULL                      |
| FromClientId   | Guid           | FK → Client, NOT NULL (dono anterior)       |
| ToClientId     | Guid           | FK → Client, NOT NULL (novo dono)           |
| TransferredAt  | DateTimeOffset | NOT NULL, UTC (timestamp da transferência)  |
| Notes          | string?        | nullable, max 500                           |

**Mapeamento EF Core**:
- `OnDelete(DeleteBehavior.Restrict)` em todos os FKs (preservar histórico)
- Imutável após criação — sem Update endpoint

---

### Product

| Campo       | Tipo           | Restrições                  |
|-------------|----------------|-----------------------------|
| Id          | Guid           | PK, gerado automaticamente  |
| Name        | string         | NOT NULL, max 200           |
| Description | string?        | nullable, max 1000          |
| Price       | decimal        | NOT NULL, precision(18,2), >= 0 |
| CreatedAt   | DateTimeOffset | NOT NULL, UTC               |
| UpdatedAt   | DateTimeOffset | NOT NULL, UTC               |

---

### Service

| Campo       | Tipo           | Restrições                  |
|-------------|----------------|-----------------------------|
| Id          | Guid           | PK, gerado automaticamente  |
| Name        | string         | NOT NULL, max 200           |
| Description | string?        | nullable, max 1000          |
| Price       | decimal        | NOT NULL, precision(18,2), >= 0 |
| CreatedAt   | DateTimeOffset | NOT NULL, UTC               |
| UpdatedAt   | DateTimeOffset | NOT NULL, UTC               |

---

## Enums

### UserRole

```csharp
public enum UserRole { Admin, Technician, Financial }
```

### AddressType

```csharp
public enum AddressType { Residential, Billing, Other }
```

---

## Relacionamentos — Resumo

| Relação                             | Cardinalidade | Delete Behavior |
|-------------------------------------|:-------------:|:---------------:|
| Client → Address                    | 1:N           | Cascade         |
| Client → Vehicle                    | 1:N           | Restrict        |
| Vehicle → VehicleTransferRecord     | 1:N           | Restrict        |
| VehicleTransferRecord → FromClient  | N:1           | Restrict        |
| VehicleTransferRecord → ToClient    | N:1           | Restrict        |

---

## Matriz de Permissões por Role (FR-014)

| Recurso     | Admin | Technician     | Financial        |
|-------------|:-----:|:--------------:|:----------------:|
| Users       | CRUD  | —              | —                |
| Clients     | CRUD  | CRUD           | Read             |
| Addresses   | CRUD  | CRUD           | —                |
| Vehicles    | CRUD  | CRUD           | —                |
| Products    | CRUD  | —              | CRUD             |
| Services    | CRUD  | —              | CRUD             |

---

## Migrações EF Core

- Naming convention: snake_case (via `UseSnakeCaseNamingConvention`)
- Migrações criadas com: `dotnet ef migrations add <Nome> --project GaragePro.Infrastructure --startup-project GaragePro.API`
- Aplicação automática no startup (Development) ou via script SQL em produção

---

## Índices

| Tabela    | Coluna         | Tipo    |
|-----------|----------------|---------|
| users     | email          | UNIQUE  |
| vehicles  | license_plate  | UNIQUE  |
