# Quickstart: Tela de Agendamento

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker (para PostgreSQL 17 via docker-compose)
- Angular CLI 21+

## Running the Project

```bash
# Start PostgreSQL
docker-compose up -d

# Apply migrations (from repo root)
cd src/GaragePro.Infrastructure
dotnet ef database update --startup-project ../GaragePro.API

# Run the API
cd ../../
dotnet run --project src/GaragePro.API

# Run the frontend (separate terminal)
cd frontend
npm install
ng serve
```

API: `https://localhost:5001`
Frontend: `http://localhost:4200`

## Adding the Appointments Migration

```bash
cd src/GaragePro.Infrastructure
dotnet ef migrations add AddAppointments --startup-project ../GaragePro.API
dotnet ef migrations add AddIsActiveToProductAndService --startup-project ../GaragePro.API
dotnet ef database update --startup-project ../GaragePro.API
```

## Running Tests

```bash
dotnet test tests/GaragePro.UnitTests/GaragePro.UnitTests.csproj
```

## Key Files for This Feature

### Backend

| Arquivo | Localização | Descrição |
|---------|-------------|-----------|
| `Appointment.cs` | `src/GaragePro.Core/Entities/` | Entidade de agendamento |
| `AppointmentRescheduleHistory.cs` | `src/GaragePro.Core/Entities/` | Histórico de remarcação |
| `AppointmentStatus.cs` | `src/GaragePro.Core/Enums/` | Enum de status |
| `IAppointmentRepository.cs` | `src/GaragePro.Core/Interfaces/Repositories/` | Interface do repositório |
| `AppointmentConfiguration.cs` | `src/GaragePro.Infrastructure/Data/Configurations/` | Config EF Core |
| `AppointmentRepository.cs` | `src/GaragePro.Infrastructure/Repositories/` | Implementação do repositório |
| `AppointmentsEndpoints.cs` | `src/GaragePro.API/Endpoints/` | Minimal API endpoints |
| `Features/Appointments/` | `src/GaragePro.Application/` | Commands, Queries, Handlers, Validators |

### Frontend

| Arquivo | Localização | Descrição |
|---------|-------------|-----------|
| `appointment.model.ts` | `frontend/src/app/core/models/` | Interfaces TypeScript |
| `appointments.service.ts` | `frontend/src/app/core/http/` | HTTP client service |
| `appointments.routes.ts` | `frontend/src/app/features/appointments/` | Lazy-loaded routes |
| `appointments-list.page.ts` | `frontend/src/app/features/appointments/list/` | Smart list page |
| `appointment-form.page.ts` | `frontend/src/app/features/appointments/detail/` | Create form |
| `reschedule-dialog.component.ts` | `frontend/src/app/features/appointments/detail/` | Reschedule dialog |
| `cancel-dialog.component.ts` | `frontend/src/app/features/appointments/detail/` | Cancel confirm dialog |

## Feature Branch

Branch: `006-scheduling-screen`

## Spec & Plan

- Spec: `specs/005-scheduling-screen/spec.md`
- Plan: `specs/005-scheduling-screen/plan.md`
- Data Model: `specs/005-scheduling-screen/data-model.md`
- API Contracts: `specs/005-scheduling-screen/contracts/api-appointments.md`
