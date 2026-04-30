# Implementation Plan: GaragePro Core CRUD API

**Branch**: `002-core-crud-api` | **Date**: 2026-04-27 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-core-crud-api/spec.md`

## Summary

Construção da API RESTful fundacional do GaragePro, cobrindo autenticação de usuários com controle de acesso baseado em papéis (RBAC), gerenciamento de clientes/endereços/veículos, transferência de veículos com log de auditoria, e catálogos de produtos e serviços. A arquitetura segue Clean Architecture com CQRS via MediatR, FluentValidation no pipeline, EF Core + PostgreSQL para persistência, JWT Bearer para autenticação e BCrypt para hash de senhas. Toda a API é documentada via Swagger (Swashbuckle.AspNetCore 7).

## Technical Context

**Language/Version**: C# 14 / .NET 10 (primary constructors, collection expressions)
**Primary Dependencies**: ASP.NET Core 10, MediatR 12, FluentValidation 11, EF Core 9, Swashbuckle.AspNetCore 7, BCrypt.Net-Next 4, Npgsql.EntityFrameworkCore.PostgreSQL 9
**Storage**: PostgreSQL 15+
**Testing**: xUnit 2, Moq 4, FluentAssertions 6, Bogus 35
**Target Platform**: Linux server (Docker-ready)
**Project Type**: Web Service (REST API)
**Performance Goals**: Todas as operações CRUD < 2 segundos em carga normal (SC-001)
**Constraints**: Hard delete apenas (soft delete fora de escopo); ordens de serviço/faturamento fora de escopo; único endpoint público é `/api/auth/login`
**Scale/Scope**: Garagem de pequeno/médio porte; single-tenant; sem requisitos de alta concorrência nesta versão

## Constitution Check

*GATE: Verificado antes do Phase 0. Revalidado após Phase 1.*

### I. Clean Architecture — Separação de Camadas
✅ **APROVADO** — Solução com 4 projetos reforça a hierarquia: `API → Application → Core ← Infrastructure`. Nenhum projeto viola dependências inversas.

### II. CQRS com MediatR
✅ **APROVADO** — Cada operação mapeia para um Command ou Query dedicado com um único Handler sob `Features/{Recurso}/{Acao}/`. Handlers de Command e Query nunca são compartilhados.

### III. Result Pattern — Retorno Padronizado
✅ **APROVADO** — Todos os Handlers retornam `Result<T>`. Endpoints verificam `result.IsSuccess` antes de mapear para HTTP status. `GlobalExceptionHandler` captura exceções não tratadas.

### IV. Pipeline de Validação via ValidationBehavior
✅ **APROVADO** — `ValidationBehavior<TRequest, TResponse>` registrado no pipeline do MediatR. Cada Command/Query que exige validação tem `IValidator<TRequest>` correspondente. Handlers não duplicam validações.

### V. Repository Pattern — Abstração de Persistência
✅ **APROVADO** — Interfaces de repositório definidas no Core; implementações concretas exclusivamente na Infrastructure. Handlers injetam interfaces — nunca `DbContext` ou EF Core diretamente.

### VI. Disciplina de Testes Unitários
✅ **APROVADO** — Projeto `GaragePro.UnitTests` referencia apenas Application + Core. Stack: xUnit, Moq, FluentAssertions, Bogus. Convenção `{Metodo}_Should{Resultado}_When{Condicao}`.

**Resultado: Nenhuma violação detectada. Sem bloqueios.**

## Project Structure

### Documentation (this feature)

```text
specs/002-core-crud-api/
├── plan.md              ← Este arquivo (/speckit-plan)
├── research.md          ← Phase 0 output (/speckit-plan)
├── data-model.md        ← Phase 1 output (/speckit-plan)
├── quickstart.md        ← Phase 1 output (/speckit-plan)
├── contracts/
│   └── api-reference.md ← Phase 1 output (/speckit-plan)
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
GaragePro.sln
src/
├── GaragePro.API/
│   ├── GaragePro.API.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Extensions/
│   │   ├── OpenApiExtensions.cs        ← Swashbuckle + JWT security definition
│   │   └── AuthExtensions.cs           ← JWT Bearer configuration
│   └── Endpoints/
│       ├── AuthEndpoints.cs
│       ├── UsersEndpoints.cs
│       ├── ClientsEndpoints.cs
│       ├── VehiclesEndpoints.cs
│       ├── ProductsEndpoints.cs
│       └── ServicesEndpoints.cs
├── GaragePro.Application/
│   ├── GaragePro.Application.csproj
│   ├── DependencyInjection.cs          ← AddApplication()
│   ├── Common/
│   │   ├── Result.cs
│   │   └── PaginatedResult.cs
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   └── Features/
│       ├── Auth/
│       │   └── Login/
│       │       ├── LoginCommand.cs
│       │       ├── LoginValidator.cs
│       │       └── LoginHandler.cs
│       ├── Users/
│       │   ├── Create/ {Command, Validator, Handler}
│       │   ├── GetById/ {Query, Handler}
│       │   ├── GetAll/ {Query, Handler}
│       │   ├── Update/ {Command, Validator, Handler}
│       │   └── Delete/ {Command, Handler}
│       ├── Clients/
│       │   ├── Create/ {Command, Validator, Handler}
│       │   ├── GetById/ {Query, Handler}
│       │   ├── GetAll/ {Query, Handler}
│       │   ├── Update/ {Command, Validator, Handler}
│       │   └── Delete/ {Command, Handler}
│       ├── Addresses/
│       │   ├── Add/ {Command, Validator, Handler}
│       │   ├── Update/ {Command, Validator, Handler}
│       │   └── Delete/ {Command, Handler}
│       ├── Vehicles/
│       │   ├── Create/ {Command, Validator, Handler}
│       │   ├── GetById/ {Query, Handler}
│       │   ├── GetAll/ {Query, Handler}
│       │   ├── Update/ {Command, Validator, Handler}
│       │   ├── Delete/ {Command, Handler}
│       │   └── Transfer/ {Command, Validator, Handler}
│       ├── Products/
│       │   ├── Create/ {Command, Validator, Handler}
│       │   ├── GetById/ {Query, Handler}
│       │   ├── GetAll/ {Query, Handler}
│       │   ├── Update/ {Command, Validator, Handler}
│       │   └── Delete/ {Command, Handler}
│       └── Services/
│           ├── Create/ {Command, Validator, Handler}
│           ├── GetById/ {Query, Handler}
│           ├── GetAll/ {Query, Handler}
│           ├── Update/ {Command, Validator, Handler}
│           └── Delete/ {Command, Handler}
├── GaragePro.Core/
│   ├── GaragePro.Core.csproj           ← sem dependências externas
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Client.cs
│   │   ├── Address.cs
│   │   ├── Vehicle.cs
│   │   ├── VehicleTransferRecord.cs
│   │   ├── Product.cs
│   │   └── Service.cs
│   ├── Enums/
│   │   ├── UserRole.cs
│   │   └── AddressType.cs
│   └── Interfaces/
│       ├── Repositories/
│       │   ├── IUserRepository.cs
│       │   ├── IClientRepository.cs
│       │   ├── IAddressRepository.cs
│       │   ├── IVehicleRepository.cs
│       │   ├── IProductRepository.cs
│       │   └── IServiceRepository.cs
│       └── Services/
│           └── IAuthService.cs
└── GaragePro.Infrastructure/
    ├── GaragePro.Infrastructure.csproj
    ├── DependencyInjection.cs           ← AddInfrastructure(config)
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Configurations/
    │       ├── UserConfiguration.cs
    │       ├── ClientConfiguration.cs
    │       ├── AddressConfiguration.cs
    │       ├── VehicleConfiguration.cs
    │       ├── VehicleTransferRecordConfiguration.cs
    │       ├── ProductConfiguration.cs
    │       └── ServiceConfiguration.cs
    ├── Repositories/
    │   ├── UserRepository.cs
    │   ├── ClientRepository.cs
    │   ├── AddressRepository.cs
    │   ├── VehicleRepository.cs
    │   ├── ProductRepository.cs
    │   └── ServiceRepository.cs
    └── Services/
        └── AuthService.cs              ← JWT generation + BCrypt verification

tests/
└── GaragePro.UnitTests/
    ├── GaragePro.UnitTests.csproj      ← refs: Application + Core only
    ├── Domain/                          ← entity rules, enums
    └── Handlers/                        ← handler logic tests
```

**Structure Decision**: Solução multi-projeto com Clean Architecture de 4 camadas. Sem frontend (API apenas). Projeto de testes separado referenciando somente Application + Core. Minimal APIs com agrupamento por recurso substituem Controllers para alinhamento com .NET 10.

## Complexity Tracking

> Nenhuma violação da Constituição detectada. Seção não aplicável.
