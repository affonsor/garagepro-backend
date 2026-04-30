# Data Model: Tela de Agendamento

## New Entities

### Appointment (Agendamento)

Registro operacional de um atendimento na garagem.

| Campo | Tipo C# | Obrigatório | Notas |
|-------|---------|-------------|-------|
| `Id` | `Guid` | Sim | PK |
| `ClientId` | `Guid` | Sim | FK → `Client.Id` |
| `ProductId` | `Guid` | Sim | FK → `Product.Id` |
| `ServiceId` | `Guid` | Sim | FK → `Service.Id` |
| `StartAt` | `DateTimeOffset` | Sim | Data/hora de início |
| `ExpectedEndAt` | `DateTimeOffset` | Sim | Previsão de término (> StartAt) |
| `Status` | `AppointmentStatus` | Sim | Enum: Scheduled, Completed, Canceled |
| `IsRescheduled` | `bool` | Sim | True se houve ao menos uma remarcação |
| `RescheduleCount` | `int` | Sim | Contador de remarcações (default 0) |
| `ProductValueSnapshot` | `decimal` | Sim | Preço do produto no momento da criação |
| `ServiceValueSnapshot` | `decimal` | Sim | Preço do serviço no momento da criação |
| `TotalValue` | `decimal` | Sim | ProductValueSnapshot + ServiceValueSnapshot |
| `Notes` | `string?` | Não | Observações internas |
| `CreatedAt` | `DateTimeOffset` | Sim | Gerado na criação |
| `UpdatedAt` | `DateTimeOffset` | Sim | Atualizado em toda alteração |

**Navegações**:
- `Client Client` (muitos para um)
- `Product Product` (muitos para um)
- `Service Service` (muitos para um)
- `ICollection<AppointmentRescheduleHistory> RescheduleHistory` (um para muitos)

---

### AppointmentRescheduleHistory (Histórico de Remarcação)

Registro imutável criado a cada remarcação de um agendamento.

| Campo | Tipo C# | Obrigatório | Notas |
|-------|---------|-------------|-------|
| `Id` | `Guid` | Sim | PK |
| `AppointmentId` | `Guid` | Sim | FK → `Appointment.Id` (CASCADE DELETE) |
| `PreviousStartAt` | `DateTimeOffset` | Sim | Início anterior |
| `PreviousExpectedEndAt` | `DateTimeOffset` | Sim | Previsão anterior |
| `NewStartAt` | `DateTimeOffset` | Sim | Novo início |
| `NewExpectedEndAt` | `DateTimeOffset` | Sim | Nova previsão |
| `Reason` | `string?` | Não | Motivo informado pelo usuário |
| `ChangedByUserId` | `Guid` | Sim | FK → `User.Id` |
| `ChangedAt` | `DateTimeOffset` | Sim | Momento da remarcação (UTC) |

**Navegações**:
- `Appointment Appointment` (muitos para um)
- `User ChangedBy` (muitos para um)

---

## Modified Entities

### Product (alteração)

Adição do campo `IsActive` para suportar seleção de apenas itens disponíveis.

| Campo adicionado | Tipo C# | Default | Notas |
|-----------------|---------|---------|-------|
| `IsActive` | `bool` | `true` | False = produto desativado, não exibido em selects |

### Service (alteração)

Adição do campo `IsActive` para suportar seleção de apenas itens disponíveis.

| Campo adicionado | Tipo C# | Default | Notas |
|-----------------|---------|---------|-------|
| `IsActive` | `bool` | `true` | False = serviço desativado, não exibido em selects |

---

## Enum

### AppointmentStatus

```csharp
public enum AppointmentStatus
{
    Scheduled = 0,   // A realizar
    Completed = 1,   // Concluído
    Canceled  = 2    // Cancelado
}
```

---

## State Transitions

```
[Criação] → Scheduled
Scheduled → Completed   (ação: concluir)
Scheduled → Canceled    (ação: cancelar)
Scheduled → Scheduled   (ação: remarcar — mantém status, atualiza datas)
Completed → (nenhuma transição permitida)
Canceled  → (nenhuma transição permitida)
```

---

## Validation Rules

### CreateAppointment
- `ClientId` obrigatório e deve existir
- `ProductId` obrigatório, deve existir e ser ativo (`IsActive = true`)
- `ServiceId` obrigatório, deve existir e ser ativo (`IsActive = true`)
- `StartAt` obrigatório
- `ExpectedEndAt` obrigatório e deve ser posterior a `StartAt`

### RescheduleAppointment
- `NewStartAt` obrigatório e deve ser no futuro (> `DateTimeOffset.UtcNow`)
- `NewExpectedEndAt` obrigatório e deve ser posterior a `NewStartAt`
- Status do agendamento deve ser `Scheduled`

### CompleteAppointment / CancelAppointment
- Status do agendamento deve ser `Scheduled`

---

## Relationships Diagram (texto)

```
Client ──< Appointment >── Product
                │
                │>── Service
                │
                └──< AppointmentRescheduleHistory >── User
```

---

## EF Core Configuration Notes

- Banco de dados: **PostgreSQL 17** via Npgsql com `UseSnakeCaseNamingConvention()` (todos os nomes de coluna serão snake_case automaticamente).
- `Appointment` terá `AppointmentConfiguration : IEntityTypeConfiguration<Appointment>`
- `AppointmentRescheduleHistory` terá `AppointmentRescheduleHistoryConfiguration`
- Concorrência otimista via **`xmin`** do PostgreSQL: configurar com `.UseXminAsConcurrencyToken()` no Fluent API de `Appointment` — nenhuma coluna extra necessária.
- Cascade delete em `AppointmentRescheduleHistory` quando `Appointment` for deletado.
- Indexar `Appointment.start_at`, `Appointment.status`, `Appointment.client_id` para performance das queries filtradas.
- Migration nova: `AddAppointments`
- Migration para `IsActive` em Product e Service: `AddIsActiveToProductAndService` (pode ser separada ou combinada).

---

## AppDbContext — DbSets adicionados

```csharp
public DbSet<Appointment> Appointments => Set<Appointment>();
public DbSet<AppointmentRescheduleHistory> AppointmentRescheduleHistories => Set<AppointmentRescheduleHistory>();
```
