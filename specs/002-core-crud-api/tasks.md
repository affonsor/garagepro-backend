# Tasks: GaragePro Core CRUD API

**Input**: Design documents from `specs/002-core-crud-api/`
**Branch**: `002-core-crud-api` | **Date**: 2026-04-27
**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md) | **Data Model**: [data-model.md](data-model.md) | **Contracts**: [contracts/api-reference.md](contracts/api-reference.md)

**Tests**: Not requested in this feature â€” no test tasks generated.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no shared dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1â€“US5)
- Exact file paths included in all descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create solution, projects, package references, and base configuration.

- [X] T001 Create GaragePro solution file at repository root: `dotnet new sln -n GaragePro`
- [X] T002 Create GaragePro.API Minimal API project: `dotnet new webapi -n GaragePro.API -o src/GaragePro.API --no-openapi`
- [X] T003 [P] Create GaragePro.Application class library: `dotnet new classlib -n GaragePro.Application -o src/GaragePro.Application`
- [X] T004 [P] Create GaragePro.Core class library: `dotnet new classlib -n GaragePro.Core -o src/GaragePro.Core`
- [X] T005 [P] Create GaragePro.Infrastructure class library: `dotnet new classlib -n GaragePro.Infrastructure -o src/GaragePro.Infrastructure`
- [X] T006 [P] Create GaragePro.UnitTests xUnit project: `dotnet new xunit -n GaragePro.UnitTests -o tests/GaragePro.UnitTests`
- [X] T007 Add all 5 projects to GaragePro.sln: `dotnet sln add src/GaragePro.API src/GaragePro.Application src/GaragePro.Core src/GaragePro.Infrastructure tests/GaragePro.UnitTests`
- [X] T008 Add project references: APIâ†’Application+Infrastructure; Applicationâ†’Core; Infrastructureâ†’Core; UnitTestsâ†’Application+Core
- [X] T009 Install NuGet packages for GaragePro.API: `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`
- [X] T010 [P] Install NuGet packages for GaragePro.Application: `MediatR`, `FluentValidation.DependencyInjectionExtensions`
- [X] T011 [P] Install NuGet packages for GaragePro.Infrastructure: `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.EntityFrameworkCore.Design`
- [X] T012 [P] Install NuGet packages for GaragePro.UnitTests: `Moq`, `FluentAssertions`, `Bogus`
- [X] T013 Configure `src/GaragePro.API/appsettings.json` and `appsettings.Development.json` with `ConnectionStrings.Default` (Npgsql) and `Jwt` (Secret, Issuer, Audience, ExpiryMinutes: 60) sections per quickstart.md

**Checkpoint**: Solution builds with all 5 projects and no errors.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before any user story can be implemented.

**âš ï¸ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T014 Create `Result<T>` with `Success(T)`, `Failure(string)`, `ValidationFailure(IEnumerable<string>)`, and `NotFound(string)` factory methods in `src/GaragePro.Application/Common/Result.cs`
- [X] T015 [P] Create `PaginatedResult<T>` with `Data`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage` in `src/GaragePro.Application/Common/PaginatedResult.cs`
- [X] T016 Create `ValidationBehavior<TRequest,TResponse>` implementing `IPipelineBehavior<TRequest,TResponse>`: run all `IValidator<TRequest>` via DI; aggregate failures; return `Result.ValidationFailure(errors)` before reaching handler in `src/GaragePro.Application/Behaviors/ValidationBehavior.cs`
- [X] T017 Create `AddApplication()` extension method registering `MediatR` (assembly scan of Application), `ValidationBehavior` as pipeline behavior, and `FluentValidation` validators (assembly scan) in `src/GaragePro.Application/DependencyInjection.cs`
- [X] T018 Create `AppDbContext : DbContext` with `UseSnakeCaseNamingConvention()` in `OnModelCreating`; `DbSet` properties will be added per story; pass `DbContextOptions` via primary constructor in `src/GaragePro.Infrastructure/Data/AppDbContext.cs`
- [X] T019 Create `AddInfrastructure(IConfiguration config)` extension method registering `AppDbContext` (Npgsql connection string from `config`) as shell to be filled per story in `src/GaragePro.Infrastructure/DependencyInjection.cs`
- [X] T020 Create `AuthExtensions.cs` containing `AddJwtBearerAuthentication(IConfiguration)` (HS256, validate issuer/audience/lifetime, read from `Jwt` config section) and `AddAuthorizationPolicies()` (three named policies: `AdminOnly` â†’ `RequireRole("Admin")`; `TechnicianOrAdmin` â†’ `RequireRole("Admin","Technician")`; `FinancialOrAdmin` â†’ `RequireRole("Admin","Financial")`) in `src/GaragePro.API/Extensions/AuthExtensions.cs`
- [X] T021 [P] Create `OpenApiExtensions.cs` with `AddSwaggerWithJwt()` (Swashbuckle 7 `AddSwaggerGen` + `BearerAuth` JWT security definition) and `UseSwaggerInDevelopment(IWebHostEnvironment)` (Swagger UI at `/swagger` only in Development) in `src/GaragePro.API/Extensions/OpenApiExtensions.cs`
- [X] T022 Create `GlobalExceptionHandler` implementing `IExceptionHandler`: catch unhandled exceptions, log via `ILogger`, return 500 with `{ "error": "Internal server error" }` JSON body in `src/GaragePro.API/GlobalExceptionHandler.cs`
- [X] T023 Wire `Program.cs`: call `AddApplication()`, `AddInfrastructure(config)`, `AddJwtBearerAuthentication(config)`, `AddAuthorizationPolicies()`, `AddSwaggerWithJwt()`, `AddExceptionHandler<GlobalExceptionHandler>`; configure pipeline: `UseExceptionHandler`, `UseAuthentication`, `UseAuthorization`, `app.MapGroup("/api")` (endpoint registration will be added per story), `UseSwaggerInDevelopment`; enable XML doc in `src/GaragePro.API/GaragePro.API.csproj` (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`)

**Checkpoint**: Foundation is ready â€” `dotnet build` succeeds; all user story phases can now begin.

---

## Phase 3: User Story 1 â€” User Authentication & Role-Based Access (Priority: P1) ðŸŽ¯ MVP

**Goal**: Admins can create and manage system users with role assignments; any user can authenticate and receive a JWT; role-based access is enforced on protected endpoints.

**Independent Test**: POST `/api/auth/login` with valid credentials â†’ 200 + JWT; POST `/api/users` with Admin token â†’ 201; same request with Technician token â†’ 403.

- [X] T024 [P] [US1] Create `UserRole` enum (`Admin`, `Technician`, `Financial`) in `src/GaragePro.Core/Enums/UserRole.cs`
- [X] T025 [P] [US1] Create `User` entity (`Id Guid`, `Name string`, `Email string`, `PasswordHash string`, `Roles List<UserRole>`, `CreatedAt DateTimeOffset`, `UpdatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/User.cs`
- [X] T026 [P] [US1] Create `IAuthService` interface with `GenerateToken(User user) : string` and `HashPassword(string plain) : string` and `VerifyPassword(string plain, string hash) : bool` in `src/GaragePro.Core/Interfaces/Services/IAuthService.cs`
- [X] T027 [US1] Create `IUserRepository` interface with `GetByIdAsync`, `GetByEmailAsync`, `GetAllAsync(int pageNumber, int pageSize)`, `ExistsByEmailAsync(string email, Guid? excludeId)`, `CreateAsync`, `UpdateAsync`, `DeleteAsync` in `src/GaragePro.Core/Interfaces/Repositories/IUserRepository.cs`
- [X] T028 [P] [US1] Create `UserConfiguration : IEntityTypeConfiguration<User>` (Fluent API): `Name` max 200, `Email` max 256 with unique index, `PasswordHash` NOT NULL, `Roles` stored as `text[]` PostgreSQL column via value converter (`List<UserRole>` â†” `string[]`), `CreatedAt`/`UpdatedAt` as `DateTimeOffset`; add `DbSet<User> Users` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/UserConfiguration.cs`
- [X] T029 [P] [US1] Create `UserRepository : IUserRepository` (primary constructor injecting `AppDbContext`; implement all interface methods with EF Core async calls) in `src/GaragePro.Infrastructure/Repositories/UserRepository.cs`
- [X] T030 [US1] Create `AuthService : IAuthService` (primary constructor injecting `IConfiguration`; `GenerateToken` builds JWT with `sub`, `email`, `role[]` claims; `HashPassword` uses `BCrypt.HashPassword(workFactor:12)`; `VerifyPassword` uses `BCrypt.Verify`) in `src/GaragePro.Infrastructure/Services/AuthService.cs`
- [X] T031 [US1] Register `UserRepository` and `AuthService` in `AddInfrastructure()` in `src/GaragePro.Infrastructure/DependencyInjection.cs`; add `modelBuilder.ApplyConfiguration(new UserConfiguration())` call in `AppDbContext.OnModelCreating`
- [X] T032 [P] [US1] Create `Auth/Login` CQRS feature: `LoginCommand(string Email, string Password)`, `LoginValidator` (email format required, password required min 6), `LoginHandler` (fetch user by email via `IUserRepository.GetByEmailAsync`, verify password via `IAuthService.VerifyPassword`, generate JWT via `IAuthService.GenerateToken`, return `Result<LoginResponse>`) in `src/GaragePro.Application/Features/Auth/Login/`
- [X] T033 [P] [US1] Create `Users/Create` CQRS feature: `CreateUserCommand(string Name, string Email, string Password, List<UserRole> Roles)`, `CreateUserValidator` (Name max 200, Email format max 256, Password min 8, Roles non-empty with valid values), `CreateUserHandler` (check email uniqueness via `ExistsByEmailAsync`, hash password, create user, return `Result<Guid>`) in `src/GaragePro.Application/Features/Users/Create/`
- [X] T034 [P] [US1] Create `Users/GetById` CQRS feature: `GetUserByIdQuery(Guid Id)`, `GetUserByIdHandler` (fetch by id, return `Result<UserResponse>` with 404 if not found) in `src/GaragePro.Application/Features/Users/GetById/`
- [X] T035 [P] [US1] Create `Users/GetAll` CQRS feature: `GetAllUsersQuery(int PageNumber, int PageSize)`, `GetAllUsersHandler` (paginated query, return `Result<PaginatedResult<UserResponse>>`) in `src/GaragePro.Application/Features/Users/GetAll/`
- [X] T036 [P] [US1] Create `Users/Update` CQRS feature: `UpdateUserCommand(Guid Id, string Name, string Email, List<UserRole> Roles)`, `UpdateUserValidator` (same rules as Create excluding password), `UpdateUserHandler` (fetch, check email uniqueness excluding self, update fields + `UpdatedAt`, persist) in `src/GaragePro.Application/Features/Users/Update/`
- [X] T037 [P] [US1] Create `Users/Delete` CQRS feature: `DeleteUserCommand(Guid Id)`, `DeleteUserHandler` (fetch, hard delete, return `Result<bool>`) in `src/GaragePro.Application/Features/Users/Delete/`
- [X] T038 [US1] Create `AuthEndpoints.cs`: `MapGroup("/auth")` with `POST /login` (no `RequireAuthorization`; dispatch `LoginCommand`; return 200 with token or 401); register via `app.MapGroup("/api").MapAuthEndpoints()` in `Program.cs` in `src/GaragePro.API/Endpoints/AuthEndpoints.cs`
- [X] T039 [US1] Create `UsersEndpoints.cs`: `MapGroup("/users").RequireAuthorization("AdminOnly")`; `GET /` â†’ `GetAllUsersQuery`; `GET /{id:guid}` â†’ `GetUserByIdQuery`; `POST /` â†’ `CreateUserCommand` â†’ 201; `PUT /{id:guid}` â†’ `UpdateUserCommand`; `DELETE /{id:guid}` â†’ `DeleteUserCommand` â†’ 204; map all HTTP status codes per api-reference.md (400/404/409); register in `Program.cs` in `src/GaragePro.API/Endpoints/UsersEndpoints.cs`
- [X] T040 [US1] Create and apply initial EF Core migration for `users` table: `dotnet ef migrations add AddUsers --project src/GaragePro.Infrastructure --startup-project src/GaragePro.API && dotnet ef database update --project src/GaragePro.Infrastructure --startup-project src/GaragePro.API`

**Checkpoint**: User Story 1 is fully functional â€” login returns JWT, user CRUD works, role enforcement rejects non-Admin requests.

---

## Phase 4: User Story 2 â€” Client Registration & Management (Priority: P1)

**Goal**: Authenticated staff can create and manage clients with multiple addresses; a client's full profile (addresses + vehicles) is retrievable in a single call.

**Independent Test**: POST `/api/clients` with 1 address â†’ 201; GET `/api/clients/{id}` â†’ full profile with addresses array; DELETE `/api/clients/{id}` (no vehicles) â†’ 204.

- [X] T041 [P] [US2] Create `AddressType` enum (`Residential`, `Billing`, `Other`) in `src/GaragePro.Core/Enums/AddressType.cs`
- [X] T042 [P] [US2] Create `Address` entity (`Id Guid`, `ClientId Guid`, `Type AddressType`, `Street`, `Number`, `Complement?`, `District`, `City`, `State` max 2, `ZipCode`, `CreatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/Address.cs`
- [X] T043 [US2] Create `Client` entity (`Id Guid`, `Name`, `Email?`, `Phone?`, `Document?`, `List<Address> Addresses`, `List<Vehicle> Vehicles`, `CreatedAt DateTimeOffset`, `UpdatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/Client.cs`
- [X] T044 [US2] Create `IClientRepository` interface (`GetByIdAsync` with Addresses+Vehicles eager load, `GetAllAsync` paginated returning summary with vehicle count, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `HasVehiclesByClientIdAsync`) in `src/GaragePro.Core/Interfaces/Repositories/IClientRepository.cs`
- [X] T045 [P] [US2] Create `IAddressRepository` interface (`GetByIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CountByClientIdAsync`) in `src/GaragePro.Core/Interfaces/Repositories/IAddressRepository.cs`
- [X] T046 [P] [US2] Create `ClientConfiguration : IEntityTypeConfiguration<Client>` (Fluent API): field lengths, `Addresses` nav â†’ `HasMany().WithOne().HasForeignKey(a => a.ClientId).OnDelete(DeleteBehavior.Cascade)`, `Vehicles` nav â†’ `HasMany().WithOne().HasForeignKey(v => v.ClientId).OnDelete(DeleteBehavior.Restrict)`; add `DbSet<Client> Clients` to `AppDbContext`; apply config in `OnModelCreating` in `src/GaragePro.Infrastructure/Data/Configurations/ClientConfiguration.cs`
- [X] T047 [P] [US2] Create `AddressConfiguration : IEntityTypeConfiguration<Address>` (Fluent API): `Type` stored as string via value converter, field lengths, `State` max 2, `ClientId` FK `OnDelete(Cascade)`; add `DbSet<Address> Addresses` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/AddressConfiguration.cs`
- [X] T048 [P] [US2] Create `ClientRepository : IClientRepository` implementing all methods; `GetAllAsync` projects to summary DTO with `.Count()` for `VehicleCount`; `HasVehiclesByClientIdAsync` uses `.AnyAsync()` in `src/GaragePro.Infrastructure/Repositories/ClientRepository.cs`
- [X] T049 [P] [US2] Create `AddressRepository : IAddressRepository` implementing all methods; `CountByClientIdAsync` used to enforce minimum-1-address rule in `src/GaragePro.Infrastructure/Repositories/AddressRepository.cs`
- [X] T050 [US2] Register `ClientRepository` and `AddressRepository` in `AddInfrastructure()` in `src/GaragePro.Infrastructure/DependencyInjection.cs`
- [X] T051 [P] [US2] Create `Clients/Create` CQRS feature: `CreateClientCommand` (Name, Email?, Phone?, Document?, `List<CreateAddressDto> Addresses`), validator (Name required max 200, Addresses non-empty, each address field validated per data-model.md, State exactly 2 chars), handler (create client with embedded addresses in one call) in `src/GaragePro.Application/Features/Clients/Create/`
- [X] T052 [P] [US2] Create `Clients/GetById` CQRS feature: query+handler returning `Result<ClientDetailResponse>` (full profile: client fields + `List<AddressResponse>` + `List<VehicleSummaryResponse>`) in `src/GaragePro.Application/Features/Clients/GetById/`
- [X] T053 [P] [US2] Create `Clients/GetAll` CQRS feature: paginated query+handler returning `Result<PaginatedResult<ClientSummaryResponse>>` (id, name, email, phone, vehicleCount, createdAt) in `src/GaragePro.Application/Features/Clients/GetAll/`
- [X] T054 [P] [US2] Create `Clients/Update` CQRS feature: command (Name, Email?, Phone?, Document?), validator, handler (update basic fields only â€” addresses are managed via Addresses endpoints) in `src/GaragePro.Application/Features/Clients/Update/`
- [X] T055 [US2] Create `Clients/Delete` CQRS feature: command+handler checks `HasVehiclesByClientIdAsync`; if true returns `Result.Failure("Client has linked vehicles and cannot be deleted")` â†’ 400; otherwise hard deletes in `src/GaragePro.Application/Features/Clients/Delete/`
- [X] T056 [P] [US2] Create `Addresses/Add` CQRS feature: `AddAddressCommand(Guid ClientId, AddressType Type, ...)`, validator (fields required, State max 2), handler (verify client exists, add address) in `src/GaragePro.Application/Features/Addresses/Add/`
- [X] T057 [P] [US2] Create `Addresses/Update` CQRS feature: `UpdateAddressCommand(Guid ClientId, Guid AddressId, ...)`, handler (verify client+address exist, update) in `src/GaragePro.Application/Features/Addresses/Update/`
- [X] T058 [US2] Create `Addresses/Delete` CQRS feature: `DeleteAddressCommand(Guid ClientId, Guid AddressId)`, handler checks `CountByClientIdAsync` â€” if 1, returns `Result.Failure("Client must have at least one address")` â†’ 400 in `src/GaragePro.Application/Features/Addresses/Delete/`
- [X] T059 [US2] Create `ClientsEndpoints.cs`: `MapGroup("/clients")` with `RequireAuthorization("TechnicianOrAdmin")` for mutations and all authenticated for GET (Financial gets 403 on mutations via missing policy); implement endpoints per api-reference.md (GET `/`, GET `/{id}`, POST `/`, PUT `/{id}`, DELETE `/{id}`, POST `/{clientId}/addresses`, PUT `/{clientId}/addresses/{addressId}`, DELETE `/{clientId}/addresses/{addressId}`); register in `Program.cs` in `src/GaragePro.API/Endpoints/ClientsEndpoints.cs`
- [X] T060 [US2] Create and apply EF Core migration for `clients` and `addresses` tables: `dotnet ef migrations add AddClientsAndAddresses --project src/GaragePro.Infrastructure --startup-project src/GaragePro.API && dotnet ef database update ...`

**Checkpoint**: User Story 2 is fully functional â€” client CRUD works, multi-address support verified, deletion blocked when vehicles exist.

---

## Phase 5: User Story 3 â€” Vehicle Registration & Transfer (Priority: P2)

**Goal**: Staff can register vehicles under clients, update vehicle data, and transfer ownership to another client while preserving the full transfer history accessible via the vehicle detail endpoint.

**Independent Test**: POST `/api/vehicles` â†’ 201; POST `/api/vehicles/{id}/transfer` to new client â†’ 200; GET `/api/vehicles/{id}` â†’ `transferHistory` contains one entry with `fromClient` and `toClient`; attempt to transfer to same owner â†’ 400.

- [X] T061 [P] [US3] Create `Vehicle` entity (`Id Guid`, `ClientId Guid`, `LicensePlate string`, `Make string`, `Model string`, `Year int`, `Color?`, `VIN?`, `List<VehicleTransferRecord> TransferHistory`, `CreatedAt DateTimeOffset`, `UpdatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/Vehicle.cs`
- [X] T062 [P] [US3] Create `VehicleTransferRecord` entity (`Id Guid`, `VehicleId Guid`, `FromClientId Guid`, `ToClientId Guid`, `TransferredAt DateTimeOffset`, `Notes?`) in `src/GaragePro.Core/Entities/VehicleTransferRecord.cs`
- [X] T063 [US3] Create `IVehicleRepository` interface (`GetByIdAsync` with `TransferHistory` eager load, `GetAllAsync(int page, int size, Guid? clientId)`, `ExistsByLicensePlateAsync(string plate, Guid? excludeId)`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `TransferAsync(Guid vehicleId, Guid toClientId, string? notes)`) in `src/GaragePro.Core/Interfaces/Repositories/IVehicleRepository.cs`; update `IClientRepository` to expose `HasVehiclesByClientIdAsync` if not already done
- [X] T064 [P] [US3] Create `VehicleConfiguration : IEntityTypeConfiguration<Vehicle>`: unique index on `LicensePlate`, `Year` CHECK constraint 1900â€“now+1, FK to `Client` with `OnDelete(Restrict)`, nav to `TransferHistory`; add `DbSet<Vehicle> Vehicles` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/VehicleConfiguration.cs`
- [X] T065 [P] [US3] Create `VehicleTransferRecordConfiguration : IEntityTypeConfiguration<VehicleTransferRecord>`: `OnDelete(Restrict)` on all FKs (`VehicleId`, `FromClientId`, `ToClientId`); entity is immutable (no update endpoint); add `DbSet<VehicleTransferRecord> VehicleTransferRecords` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/VehicleTransferRecordConfiguration.cs`
- [X] T066 [US3] Create `VehicleRepository : IVehicleRepository`: `TransferAsync` updates `Vehicle.ClientId` and inserts `VehicleTransferRecord` in a single `BeginTransactionAsync` scope to guarantee atomicity in `src/GaragePro.Infrastructure/Repositories/VehicleRepository.cs`
- [X] T067 [US3] Register `VehicleRepository` in `AddInfrastructure()` in `src/GaragePro.Infrastructure/DependencyInjection.cs`; add `VehicleConfiguration` and `VehicleTransferRecordConfiguration` to `OnModelCreating`
- [X] T068 [P] [US3] Create `Vehicles/Create` CQRS feature: `CreateVehicleCommand(Guid ClientId, string LicensePlate, string Make, string Model, int Year, string? Color, string? VIN)`, validator (all required fields, Year range 1900â€“now+1, LicensePlate max 10), handler (verify `ClientId` exists, check `LicensePlate` unique, create) in `src/GaragePro.Application/Features/Vehicles/Create/`
- [X] T069 [P] [US3] Create `Vehicles/GetById` CQRS feature: query+handler returning `Result<VehicleDetailResponse>` (vehicle fields + `currentOwner` name + `transferHistory` list per api-reference.md) in `src/GaragePro.Application/Features/Vehicles/GetById/`
- [X] T070 [P] [US3] Create `Vehicles/GetAll` CQRS feature: `GetAllVehiclesQuery(int PageNumber, int PageSize, Guid? ClientId)`, handler filters by `ClientId` when provided, returns paginated `VehicleSummaryResponse` with `currentOwner` in `src/GaragePro.Application/Features/Vehicles/GetAll/`
- [X] T071 [P] [US3] Create `Vehicles/Update` CQRS feature: `UpdateVehicleCommand(Guid Id, string Make, string Model, int Year, string? Color, string? VIN)` â€” `LicensePlate` and `ClientId` are intentionally NOT updatable; handler fetches, updates fields + `UpdatedAt`, persists in `src/GaragePro.Application/Features/Vehicles/Update/`
- [X] T072 [P] [US3] Create `Vehicles/Delete` CQRS feature: command+handler hard deletes vehicle (no FK protection from transfers due to Restrict; ensure transfer records are deleted first or reject if history exists) in `src/GaragePro.Application/Features/Vehicles/Delete/`
- [X] T073 [US3] Create `Vehicles/Transfer` CQRS feature: `TransferVehicleCommand(Guid VehicleId, Guid ToClientId, string? Notes)`, validator (ToClientId required), handler (verify vehicle exists, verify `ToClientId` exists, reject if `ToClientId == Vehicle.ClientId`, call `IVehicleRepository.TransferAsync`) returning `Result<TransferResponse>` with `transferRecordId` + `transferredAt` in `src/GaragePro.Application/Features/Vehicles/Transfer/`
- [X] T074 [US3] Create `VehiclesEndpoints.cs`: `MapGroup("/vehicles").RequireAuthorization("TechnicianOrAdmin")`; `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `DELETE /{id}`, `POST /{id}/transfer`; map HTTP status codes per api-reference.md (404/400/409 for duplicate plate); register in `Program.cs` in `src/GaragePro.API/Endpoints/VehiclesEndpoints.cs`
- [X] T075 [US3] Create and apply EF Core migration for `vehicles` and `vehicle_transfer_records` tables: `dotnet ef migrations add AddVehicles --project src/GaragePro.Infrastructure --startup-project src/GaragePro.API && dotnet ef database update ...`

**Checkpoint**: User Story 3 is fully functional â€” vehicle CRUD works, transfer atomically updates owner + creates history record, history visible on GET vehicle detail.

---

## Phase 6: User Story 4 â€” Product Catalog Management (Priority: P2)

**Goal**: Admin and Financial users can manage the product catalog (create, read, update, delete). Technicians have no access.

**Independent Test**: POST `/api/products` with Financial token â†’ 201; GET `/api/products` with Technician token â†’ 403; PUT `/api/products/{id}` â†’ updated price reflected in GET.

- [X] T076 [P] [US4] Create `Product` entity (`Id Guid`, `Name string`, `Description?`, `Price decimal`, `CreatedAt DateTimeOffset`, `UpdatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/Product.cs`
- [X] T077 [US4] Create `IProductRepository` interface (`GetByIdAsync`, `GetAllAsync(int pageNumber, int pageSize)`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`) in `src/GaragePro.Core/Interfaces/Repositories/IProductRepository.cs`
- [X] T078 [P] [US4] Create `ProductConfiguration : IEntityTypeConfiguration<Product>`: `Name` max 200, `Description` max 1000 nullable, `Price` `HasPrecision(18,2)` with `CHECK Price >= 0`; add `DbSet<Product> Products` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/ProductConfiguration.cs`
- [X] T079 [P] [US4] Create `ProductRepository : IProductRepository` in `src/GaragePro.Infrastructure/Repositories/ProductRepository.cs`
- [X] T080 [US4] Register `ProductRepository` in `AddInfrastructure()` in `src/GaragePro.Infrastructure/DependencyInjection.cs`; add `ProductConfiguration` to `OnModelCreating`
- [X] T081 [P] [US4] Create `Products/Create` CQRS feature: `CreateProductCommand(string Name, string? Description, decimal Price)`, validator (Name required max 200, Price >= 0), handler (create, return `Result<Guid>`) in `src/GaragePro.Application/Features/Products/Create/`
- [X] T082 [P] [US4] Create `Products/GetById` CQRS feature: query+handler returning `Result<ProductResponse>` (404 if not found) in `src/GaragePro.Application/Features/Products/GetById/`
- [X] T083 [P] [US4] Create `Products/GetAll` CQRS feature: paginated query+handler returning `Result<PaginatedResult<ProductResponse>>` in `src/GaragePro.Application/Features/Products/GetAll/`
- [X] T084 [P] [US4] Create `Products/Update` CQRS feature: `UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price)`, same validator rules as Create, handler updates + `UpdatedAt` in `src/GaragePro.Application/Features/Products/Update/`
- [X] T085 [P] [US4] Create `Products/Delete` CQRS feature: command+handler hard deletes; return 404 if not found in `src/GaragePro.Application/Features/Products/Delete/`
- [X] T086 [US4] Create `ProductsEndpoints.cs`: `MapGroup("/products").RequireAuthorization("FinancialOrAdmin")` for POST/PUT/DELETE; GET endpoints require any authenticated user; map all HTTP status codes per api-reference.md; register in `Program.cs` in `src/GaragePro.API/Endpoints/ProductsEndpoints.cs`
- [X] T087 [US4] Create and apply EF Core migration for `products` table: `dotnet ef migrations add AddProducts --project src/GaragePro.Infrastructure --startup-project src/GaragePro.API && dotnet ef database update ...`

**Checkpoint**: User Story 4 is fully functional â€” product catalog CRUD works, Financial/Admin can mutate, Technician access is denied.

---

## Phase 7: User Story 5 â€” Service Catalog Management (Priority: P2)

**Goal**: Admin and Financial users can manage the service catalog (create, read, update, delete). Structure mirrors Product catalog.

**Independent Test**: POST `/api/services` with Financial token â†’ 201; GET `/api/services` returns paginated list; DELETE `/api/services/{id}` â†’ 204 and item no longer appears in listing.

- [X] T088 [P] [US5] Create `Service` entity (`Id Guid`, `Name string`, `Description?`, `Price decimal`, `CreatedAt DateTimeOffset`, `UpdatedAt DateTimeOffset`) in `src/GaragePro.Core/Entities/Service.cs`
- [X] T089 [US5] Create `IServiceRepository` interface (`GetByIdAsync`, `GetAllAsync(int pageNumber, int pageSize)`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`) in `src/GaragePro.Core/Interfaces/Repositories/IServiceRepository.cs`
- [X] T090 [P] [US5] Create `ServiceConfiguration : IEntityTypeConfiguration<Service>`: same field constraints as ProductConfiguration (`Name` max 200, `Description` max 1000, `Price` precision(18,2) >= 0); add `DbSet<Service> Services` to `AppDbContext` in `src/GaragePro.Infrastructure/Data/Configurations/ServiceConfiguration.cs`
- [X] T091 [P] [US5] Create `ServiceRepository : IServiceRepository` in `src/GaragePro.Infrastructure/Repositories/ServiceRepository.cs`
- [X] T092 [US5] Register `ServiceRepository` in `AddInfrastructure()` in `src/GaragePro.Infrastructure/DependencyInjection.cs`; add `ServiceConfiguration` to `OnModelCreating`
- [X] T093 [P] [US5] Create `Services/Create` CQRS feature (mirrors Products/Create): `CreateServiceCommand`, `CreateServiceValidator`, `CreateServiceHandler` in `src/GaragePro.Application/Features/Services/Create/`
- [X] T094 [P] [US5] Create `Services/GetById` CQRS feature: query+handler returning `Result<ServiceResponse>` in `src/GaragePro.Application/Features/Services/GetById/`
- [X] T095 [P] [US5] Create `Services/GetAll` CQRS feature: paginated query+handler in `src/GaragePro.Application/Features/Services/GetAll/`
- [X] T096 [P] [US5] Create `Services/Update` CQRS feature: command+validator+handler in `src/GaragePro.Application/Features/Services/Update/`
- [X] T097 [P] [US5] Create `Services/Delete` CQRS feature: command+handler in `src/GaragePro.Application/Features/Services/Delete/`
- [X] T098 [US5] Create `ServicesEndpoints.cs`: `MapGroup("/services").RequireAuthorization("FinancialOrAdmin")` for mutations; GET endpoints require any authenticated user; map status codes per api-reference.md; register in `Program.cs` in `src/GaragePro.API/Endpoints/ServicesEndpoints.cs`
- [X] T099 [US5] Create and apply EF Core migration for `services` table: `dotnet ef migrations add AddServices --project src/Garagepro.Infrastructure --startup-project src/GaragePro.API && dotnet ef database update ...`

**Checkpoint**: User Story 5 is fully functional â€” service catalog CRUD works with identical authorization rules as products.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Dev experience, observability, and validation of the full quickstart flow.

- [X] T100 Create development admin seed: `SeedAdminUserAsync()` extension method in `src/GaragePro.API/Extensions/SeedExtensions.cs` that creates `admin@garagepro.com / Admin@1234` (BCrypt hashed) with `Admin` role if no users exist; invoke in `Program.cs` inside `if (app.Environment.IsDevelopment())`
- [X] T101 [P] Add `WithSummary()`, `WithDescription()`, and `Produces<T>()` / `ProducesValidationProblem()` decorators to all endpoint groups in all `*Endpoints.cs` files for Swagger UI documentation; confirm XML doc file is referenced in Swashbuckle config
- [X] T102 Validate complete quickstart.md flow end-to-end: start Docker PostgreSQL, run all migrations, `dotnet run`, open Swagger UI, authenticate as admin, exercise at least one CRUD operation per resource group, verify transfer history endpoint
- [X] T103 [P] Validate role enforcement matrix (FR-014): confirm Financial token â†’ 403 on `/api/users` and `/api/vehicles`; Technician token â†’ 403 on `/api/products` and `/api/services`; Admin token â†’ 200 on all resources

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies â€” start immediately
- **Foundational (Phase 2)**: Depends on Setup â€” **BLOCKS all user stories**
- **US1 (Phase 3)**: Depends on Foundational â€” first story; sets up auth for all others
- **US2 (Phase 4)**: Depends on Foundational; can start in parallel with US1 if staffed (Client entity does not depend on User)
- **US3 (Phase 5)**: Depends on Foundational + US2 (Vehicle references Client entity)
- **US4 (Phase 6)**: Depends on Foundational only â€” Product is independent
- **US5 (Phase 7)**: Depends on Foundational only â€” Service is independent; can run in parallel with US4
- **Polish (Phase 8)**: Depends on all user story phases

### User Story Dependencies

- **US1** â†’ Foundational only. No story deps.
- **US2** â†’ Foundational only. No story deps (independent of US1 entities).
- **US3** â†’ Foundational + US2 (Client entity must exist before Vehicle entity can reference it).
- **US4** â†’ Foundational only. No story deps.
- **US5** â†’ Foundational only. No story deps.

### Within Each User Story

- Enums â†’ Entities â†’ Repository Interfaces â†’ (EF Configs || Repository Implementations) â†’ (Handlers) â†’ Endpoints â†’ Migration
- [P] tasks at the same layer can run simultaneously
- Handlers depend on repository interfaces (not implementations) â€” can be developed while infrastructure is in progress
- Migrations must run after all DbSets and configurations for that story are applied

### Parallel Opportunities

- T003â€“T006 (project creation), T009â€“T012 (NuGet installs): all [P] within Phase 1
- T014â€“T015 (`Result<T>` + `PaginatedResult<T>`), T020â€“T021 (API extensions): [P] within Phase 2
- US4 (T076â€“T087) and US5 (T088â€“T099) can be worked simultaneously by different developers after US2 is complete
- All CQRS feature folders within a story (Create/GetById/GetAll/Update/Delete) are [P] with each other

---

## Parallel Example: User Story 3

```text
# After T061+T062 (entities) and T063 (IVehicleRepository) complete:

Parallel batch 1 â€” infrastructure:
  T064  VehicleConfiguration
  T065  VehicleTransferRecordConfiguration
  T066  VehicleRepository           â† (depends on T063, not T064/T065)

Parallel batch 2 â€” application handlers (after T063):
  T068  Vehicles/Create
  T069  Vehicles/GetById
  T070  Vehicles/GetAll
  T071  Vehicles/Update
  T072  Vehicles/Delete
  T073  Vehicles/Transfer           â† (no parallel; most complex, review last)

Sequential â€” endpoint + migration:
  T074  VehiclesEndpoints.cs        â† (after all handlers)
  T075  EF migration AddVehicles    â† (after EF configs)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (**CRITICAL â€” blocks all stories**)
3. Complete Phase 3: User Story 1 (Auth + Users)
4. **STOP and VALIDATE**: POST `/api/auth/login` â†’ JWT; POST `/api/users` (Admin) â†’ 201; test policy enforcement
5. Demo: authenticated CRUD for users, role-based rejection

### Incremental Delivery

1. Setup + Foundational â†’ solution compiles, `Program.cs` boots
2. US1 â†’ auth works end-to-end â†’ minimal demo
3. US2 â†’ client + address CRUD â†’ first domain entity
4. US3 â†’ vehicle CRUD + transfer â†’ history preserves ownership lineage
5. US4 â†’ product catalog
6. US5 â†’ service catalog
7. Polish â†’ seed data, Swagger docs, role enforcement smoke tests

### Parallel Team Strategy

With 3 developers after Foundational is complete:
- **Dev A**: US1 (Auth + Users) â†’ then US3 (Vehicles, after US2 entities done)
- **Dev B**: US2 (Clients + Addresses) â†’ unblocks US3 for Dev A
- **Dev C**: US4 (Products) â†’ then US5 (Services, in parallel with US4 once independent)

---

## Notes

- [P] = different files, no incomplete-task dependencies â€” safe to run simultaneously
- [Story] label traces each task to its user story for independent delivery
- Each story should be independently completable and testable before the next begins
- EF migrations must be applied **after** the corresponding `DbSet` and `IEntityTypeConfiguration` are wired into `AppDbContext`
- `AddInfrastructure()` DI registrations and `AppDbContext.OnModelCreating` configs are updated incrementally per story â€” avoid merge conflicts by completing one story's infrastructure tasks before another developer modifies the same file
- Commit after each phase checkpoint at minimum
