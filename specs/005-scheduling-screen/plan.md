# Implementation Plan: Tela de Agendamento

**Branch**: `006-scheduling-screen` | **Date**: 2026-04-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/005-scheduling-screen/spec.md`

## Summary

Implementar a tela de agendamento do GaragePro — a tela de maior uso operacional do sistema. O backend requer novas entidades (`Appointment`, `AppointmentRescheduleHistory`), um repositório com suporte a filtros e agregação de resumo financeiro, e cinco operações CQRS (criar, listar, concluir, cancelar, remarcar). O frontend requer uma feature standalone Angular 21 com listagem filtrada, formulário de criação, e dialogs inline para concluir/cancelar/remarcar — tudo sem sair da tela principal.

## Technical Context

**Language/Version**: C# 13 / .NET 10.0 (backend) + TypeScript / Angular 21 (frontend)
**Primary Dependencies**: MediatR 12, FluentValidation 11, EF Core 10, JWT Bearer (backend) | Angular Material 3, Reactive Forms, Signals (frontend)
**Storage**: PostgreSQL 17 via EF Core 10 + Npgsql com snake_case naming convention (padrão do projeto via `UseNpgsql` + `UseSnakeCaseNamingConvention`). Nova migration `AddAppointments` e `AddIsActiveToProductAndService`.
**Testing**: xUnit + Moq + FluentAssertions + Bogus (unitários) — padrão já estabelecido no projeto.
**Target Platform**: Web — desktop-primary, responsivo para tablets (viewport mínimo 360px).
**Project Type**: Web application (REST API backend + Angular SPA frontend).
**Performance Goals**: Lista de agendamentos carrega em < 2s para até 200 registros no período selecionado; mutações (criar/concluir/cancelar/remarcar) concluem em < 1s.
**Constraints**: Controle de acesso por role (Admin/Technician escrita; Financial leitura). Concorrência otimista via `xmin` do PostgreSQL (Npgsql `UseXminAsConcurrencyToken`).
**Scale/Scope**: ~50 agendamentos/dia esperados inicialmente; filtro padrão por período (semana/mês).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Status | Notas |
|-----------|--------|-------|
| I. Clean Architecture | ✅ PASS | `Appointment` e `AppointmentRescheduleHistory` em Core. Repository interface em Core. Implementação em Infrastructure. Endpoints em API. Zero lógica de negócio fora de Application/Core. |
| II. CQRS com MediatR | ✅ PASS | `GetAppointmentsQuery`, `GetAppointmentByIdQuery` (leitura). `CreateAppointmentCommand`, `CompleteAppointmentCommand`, `CancelAppointmentCommand`, `RescheduleAppointmentCommand` (escrita). Um Handler por operação. |
| III. Result Pattern | ✅ PASS | Todos os Handlers retornam `Result<T>`. Conflito de concorrência → `Result.Failure`. Transição inválida → `Result.Failure`. |
| IV. ValidationBehavior | ✅ PASS | `CreateAppointmentValidator` e `RescheduleAppointmentValidator` implementam `IValidator<TRequest>`. Regras de estado (status atual) tratadas no Handler, não no Validator. |
| V. Repository Pattern | ✅ PASS | `IAppointmentRepository` define interface em Core com métodos para filtro, agregação de summary e operações por ID. Implementação concreta em Infrastructure. |
| VI. Testes Unitários | ✅ PASS | Handlers de Command e Query cobertas com mocks via Moq. Validators testados diretamente. Nomenclatura `Handle_Should{Result}_When{Condition}`. |
| VII. Frontend Angular 21 | ✅ PASS | Standalone components. Signals para estado local. Lazy loading via `appointments.routes.ts`. `AppointmentsService` em `core/http/`. Reactive Forms. Angular Material 3. |

**Resultado: Todos os gates passam. Sem violações justificadas.**

## Project Structure

### Documentation (this feature)

```text
specs/005-scheduling-screen/
├── spec.md                        # Especificação funcional
├── plan.md                        # Este arquivo
├── research.md                    # Decisões de design e pesquisa (Phase 0)
├── data-model.md                  # Modelo de dados (Phase 1)
├── quickstart.md                  # Guia de desenvolvimento (Phase 1)
├── contracts/
│   └── api-appointments.md        # Contratos da API (Phase 1)
├── checklists/
│   └── requirements.md            # Checklist de qualidade da spec
└── tasks.md                       # Gerado pelo /speckit-tasks (NÃO criado aqui)
```

### Source Code — Backend

```text
src/
├── GaragePro.Core/
│   ├── Entities/
│   │   ├── Appointment.cs                          # NOVO
│   │   └── AppointmentRescheduleHistory.cs         # NOVO
│   ├── Enums/
│   │   └── AppointmentStatus.cs                    # NOVO
│   └── Interfaces/Repositories/
│       └── IAppointmentRepository.cs               # NOVO
│
├── GaragePro.Application/
│   └── Features/
│       └── Appointments/
│           ├── AppointmentResponses.cs             # NOVO (DTOs de resposta)
│           ├── Create/
│           │   ├── CreateAppointmentCommand.cs     # NOVO
│           │   ├── CreateAppointmentValidator.cs   # NOVO
│           │   └── CreateAppointmentHandler.cs     # NOVO
│           ├── GetAll/
│           │   ├── GetAppointmentsQuery.cs         # NOVO
│           │   └── GetAppointmentsHandler.cs       # NOVO
│           ├── GetById/
│           │   ├── GetAppointmentByIdQuery.cs      # NOVO
│           │   └── GetAppointmentByIdHandler.cs    # NOVO
│           ├── Complete/
│           │   ├── CompleteAppointmentCommand.cs   # NOVO
│           │   └── CompleteAppointmentHandler.cs   # NOVO
│           ├── Cancel/
│           │   ├── CancelAppointmentCommand.cs     # NOVO
│           │   └── CancelAppointmentHandler.cs     # NOVO
│           └── Reschedule/
│               ├── RescheduleAppointmentCommand.cs # NOVO
│               ├── RescheduleAppointmentValidator.cs # NOVO
│               └── RescheduleAppointmentHandler.cs # NOVO
│
├── GaragePro.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs                         # MODIFICADO (+ DbSets)
│   │   ├── Configurations/
│   │   │   ├── AppointmentConfiguration.cs         # NOVO
│   │   │   └── AppointmentRescheduleHistoryConfiguration.cs # NOVO
│   │   └── Migrations/
│   │       └── [timestamp]_AddAppointments.cs      # NOVO (via dotnet-ef)
│   └── Repositories/
│       └── AppointmentRepository.cs                # NOVO
│
└── GaragePro.API/
    └── Endpoints/
        └── AppointmentsEndpoints.cs                # NOVO

tests/
└── GaragePro.UnitTests/
    └── Handlers/
        └── Appointments/
            ├── CreateAppointmentHandlerTests.cs    # NOVO
            ├── GetAppointmentsHandlerTests.cs      # NOVO
            ├── CompleteAppointmentHandlerTests.cs  # NOVO
            ├── CancelAppointmentHandlerTests.cs    # NOVO
            └── RescheduleAppointmentHandlerTests.cs # NOVO
```

**Entidades existentes modificadas**:
- `Product.cs` — adicionar `IsActive: bool = true`
- `Service.cs` — adicionar `IsActive: bool = true`
- `ProductConfiguration.cs` / `ServiceConfiguration.cs` — mapear `IsActive`
- Migration separada: `AddIsActiveToProductAndService`

### Source Code — Frontend

```text
frontend/src/app/
├── core/
│   ├── models/
│   │   └── appointment.model.ts                    # NOVO
│   └── http/
│       └── appointments.service.ts                 # NOVO
│
└── features/
    └── appointments/
        ├── appointments.routes.ts                  # NOVO (lazy-loaded)
        ├── list/
        │   └── appointments-list.page.ts           # NOVO (smart component)
        └── detail/
            ├── appointment-form.page.ts            # NOVO (create form)
            ├── reschedule-dialog.component.ts      # NOVO (dialog)
            └── cancel-dialog.component.ts          # NOVO (dialog de confirmação)
```

**Rota registrada no app**: `{ path: 'appointments', loadChildren: () => import('./features/appointments/appointments.routes') }`

**Structure Decision**: Fullstack web application — backend em Clean Architecture com CQRS, frontend em Angular 21 Standalone. Ambos seguem padrões estabelecidos na constitution e refletidos nos demais recursos do projeto (clients, products, services, vehicles).

## Complexity Tracking

> Sem violações da constituição. Seção não aplicável.
