# Research: Tela de Agendamento

## Technology Stack

**Decision**: .NET 10.0 / C# 13 backend + Angular 21 frontend — already established in the project.
**Rationale**: No research needed; the stack is fixed.
**Alternatives considered**: N/A.

---

## Data Snapshot Strategy (Price Preservation)

**Decision**: Capture `ProductValueSnapshot` and `ServiceValueSnapshot` as `decimal` fields on `Appointment` at creation time; compute `TotalValue = ProductValueSnapshot + ServiceValueSnapshot` and persist it.
**Rationale**: Product and Service prices may change after an appointment is created. The spec explicitly requires that the total value be frozen at creation time and never updated by catalog changes. Storing snapshots is the simplest pattern that makes the appointment self-contained.
**Alternatives considered**:
- Event sourcing a price history table — over-engineering for this scope.
- Recomputing at display time from a price-history table — complex and fragile for financial summaries.

---

## Concurrency Control

**Decision**: Use EF Core optimistic concurrency via PostgreSQL's built-in `xmin` system column (configured via Npgsql's `UseXminAsConcurrencyToken()`). No extra column is needed on the table. The handler catches `DbUpdateConcurrencyException` and returns `Result.Failure` with a user-friendly message.
**Rationale**: Two technicians may view and concurrently try to complete/cancel the same appointment. Optimistic concurrency via `xmin` is idiomatic for Npgsql/PostgreSQL, zero-overhead (no extra column), and appropriate for the expected low-collision rate of a garage management system.
**Alternatives considered**:
- `byte[] RowVersion` column (SQL Server pattern) — not idiomatic for PostgreSQL; Npgsql's `xmin` approach is the correct equivalent.
- Pessimistic locking (`SELECT FOR UPDATE`) — unnecessary overhead; EF Core abstractions make this cumbersome.
- No concurrency control — last writer wins; could cause data integrity issues for status transitions.

---

## Status Transition Rules

**Decision**: State machine enforced in the domain handler (not in FluentValidation) because it involves reading current state from the database.

| Current Status | Allowed Transitions |
|----------------|---------------------|
| Scheduled      | → Completed, → Canceled, → Scheduled (reschedule) |
| Completed      | (none) |
| Canceled       | (none) |

Handler returns `Result.Failure("Transição de status inválida.")` for illegal transitions.

---

## Rescheduling to Past Dates

**Decision**: The validator MUST reject a reschedule if the new `StartAt` is in the past (before `DateTimeOffset.UtcNow`).
**Rationale**: Rescheduling implies the appointment will occur in the future. The spec's edge case ("remarcar para uma data no passado") is resolved by preventing it — the screen shows a clear validation error.
**Alternatives considered**: Allow past dates for historical correction — rejected because the spec's intent is operational scheduling, not backdating.

---

## Product/Service Deactivation Edge Case

**Decision**: Once an appointment is created, its values are snapshots; the linked `ProductId`/`ServiceId` are foreign keys but the UI does not re-validate them on complete/cancel. When listing appointments, the product and service names are fetched via JOIN even if they are later deactivated — deactivation does not remove or null the name.
**Rationale**: The appointment is a historical operational record. Deactivating a product should not affect existing appointments. The spec's "fotografia do momento da criação" principle covers this.
**Alternatives considered**: Denormalize product/service names into the appointment row — simplifies queries but creates data duplication drift; rejected in favour of JOIN with name resolution at query time.

---

## Product and Service Active Filter

**Decision**: Add an `IsActive` boolean flag to `Product` and `Service` entities. When creating or rescheduling an appointment, only active products and services can be selected (enforced by the validator querying the repository).
**Rationale**: The spec requires "apenas itens ativos/disponíveis" when selecting product and service. Currently neither entity has an `IsActive` field. This field must be added as part of this feature's scope.
**Alternatives considered**: Soft-delete pattern via `DeletedAt` — adds complexity for filtering; a simple `IsActive` flag is sufficient and aligns with how the frontend will autocomplete items.

---

## Financial Summary Architecture

**Decision**: The `GET /api/appointments` endpoint returns a custom `AppointmentListResponse` wrapping `PaginatedResult<AppointmentSummaryResponse>` with an additional `Summary` property of type `AppointmentSummary`. The summary is computed in the query handler via a separate database aggregation call (not in-memory on the full dataset).
**Rationale**: The summary must reflect the current filters (date range, status, client). Computing it in the handler via SQL `SUM`/`COUNT` with the same filter predicates is more efficient than loading all rows for aggregation.
**Alternatives considered**: Compute summary from the paginated page only — incorrect; the summary must span ALL matching records, not just the current page.

---

## Reschedule History

**Decision**: `AppointmentRescheduleHistory` is a child table of `Appointment`. Each reschedule operation creates one history record capturing previous and new dates, optional reason, the acting user ID, and timestamp.
**Rationale**: The spec requires showing the original date/time when viewing appointment detail. A history table is the correct relational approach.
**Alternatives considered**: JSON column on `Appointment` — simpler but non-queryable and not future-proof.

---

## Frontend State Management

**Decision**: Use Angular Signals for the appointments list state (filter values, selected status, loading flag). A dedicated `AppointmentsService` in `core/http/` handles HTTP communication.
**Rationale**: Signals are the mandated approach for local/derived state per the constitution. The appointments page is a single self-contained route.
**Alternatives considered**: NgRx — over-engineering for a single feature page.

---

## Inline Actions vs. Navigation

**Decision**: Complete and Cancel actions are performed inline via dialogs (MatDialog) from the list page. Reschedule opens a side dialog/bottom sheet with the new date/time form. Create opens the appointment form as a full page (`/appointments/new`) or dialog — **dialog is preferred** to keep the user on the list.
**Rationale**: The spec states "sem navegar para fora da listagem" for complete/cancel/reschedule (SC-006). A full-page form for create is also acceptable but a dialog is more consistent with the inline action pattern.
**Alternatives considered**: Navigate to `/appointments/{id}/edit` for all actions — violates SC-006.

---

## Resolved NEEDS CLARIFICATION Items

No `[NEEDS CLARIFICATION]` markers were present in the spec. All edge cases above are resolved via the decisions documented here.
