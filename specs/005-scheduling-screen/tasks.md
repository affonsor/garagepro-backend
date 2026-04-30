# Tasks: Tela de Agendamento

**Input**: Design documents from `specs/005-scheduling-screen/`
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/ ✅

**Tests**: Incluídos — Constitution Principle VI exige cobertura de Handlers e Validators.

**Organization**: Tasks agrupadas por user story para implementação e teste independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode executar em paralelo (arquivos diferentes, sem dependências)
- **[Story]**: User story correspondente (US1–US5)
- Caminhos relativos à raiz do repositório

---

## Phase 1: Setup — Modificações em Entidades Existentes

**Purpose**: Adicionar `IsActive` a `Product` e `Service` e aplicar migration. Bloqueia US2 mas não US1/US3/US4 no backend.

- [X] T001 [P] Adicionar `public bool IsActive { get; set; } = true;` em `src/GaragePro.Core/Entities/Product.cs`
- [X] T002 [P] Adicionar `public bool IsActive { get; set; } = true;` em `src/GaragePro.Core/Entities/Service.cs`
- [X] T003 [P] Mapear `IsActive` com `builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true)` em `src/GaragePro.Infrastructure/Data/Configurations/ProductConfiguration.cs`
- [X] T004 [P] Mapear `IsActive` com `builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true)` em `src/GaragePro.Infrastructure/Data/Configurations/ServiceConfiguration.cs`
- [X] T005 Gerar e aplicar migration `AddIsActiveToProductAndService` executando `dotnet ef migrations add AddIsActiveToProductAndService --startup-project src/GaragePro.API --project src/GaragePro.Infrastructure` e depois `dotnet ef database update --startup-project src/GaragePro.API --project src/GaragePro.Infrastructure`

**Checkpoint**: `Product` e `Service` com `IsActive`. Migration aplicada no banco.

---

## Phase 2: Foundational — Domínio de Agendamentos

**Purpose**: Toda a infraestrutura de domínio — entidades, enum, interface do repositório, configurações EF Core, migration, implementação base do repositório e registro no DI. **Bloqueia todas as user stories.**

⚠️ **CRÍTICO**: Nenhuma user story pode começar até esta fase estar completa.

- [X] T006 [P] Criar `src/GaragePro.Core/Enums/AppointmentStatus.cs` com `public enum AppointmentStatus { Scheduled = 0, Completed = 1, Canceled = 2 }`
- [X] T007 [P] Criar `src/GaragePro.Core/Entities/Appointment.cs` com propriedades: `Guid Id`, `Guid ClientId`, `Guid ProductId`, `Guid ServiceId`, `DateTimeOffset StartAt`, `DateTimeOffset ExpectedEndAt`, `AppointmentStatus Status`, `bool IsRescheduled`, `int RescheduleCount`, `decimal ProductValueSnapshot`, `decimal ServiceValueSnapshot`, `decimal TotalValue`, `string? Notes`, `DateTimeOffset CreatedAt`, `DateTimeOffset UpdatedAt`; navegações: `Client Client`, `Product Product`, `Service Service`, `ICollection<AppointmentRescheduleHistory> RescheduleHistory`
- [X] T008 [P] Criar `src/GaragePro.Core/Entities/AppointmentRescheduleHistory.cs` com propriedades: `Guid Id`, `Guid AppointmentId`, `DateTimeOffset PreviousStartAt`, `DateTimeOffset PreviousExpectedEndAt`, `DateTimeOffset NewStartAt`, `DateTimeOffset NewExpectedEndAt`, `string? Reason`, `Guid ChangedByUserId`, `DateTimeOffset ChangedAt`; navegações: `Appointment Appointment`, `User ChangedBy`
- [X] T009 Criar record de retorno `public record AppointmentSummaryData(int ScheduledCount, decimal ScheduledTotal, int CompletedCount, decimal CompletedTotal, int CanceledCount, decimal CanceledTotal)` e interface `IAppointmentRepository` em `src/GaragePro.Core/Interfaces/Repositories/IAppointmentRepository.cs` com métodos: `Task<(IEnumerable<Appointment> Items, int TotalCount)> GetAllAsync(DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status, Guid? clientId, string? search, int pageNumber, int pageSize, CancellationToken ct)`; `Task<AppointmentSummaryData> GetSummaryAsync(DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status, Guid? clientId, string? search, CancellationToken ct)`; `Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct)`; `Task<Guid> AddAsync(Appointment appointment, CancellationToken ct)`; `Task UpdateAsync(Appointment appointment, CancellationToken ct)`
- [X] T010 Criar `src/GaragePro.Infrastructure/Data/Configurations/AppointmentConfiguration.cs` implementando `IEntityTypeConfiguration<Appointment>`: PK `Id`; FKs para `Client`, `Product`, `Service`; mapear todas as propriedades; índices em `StartAt`, `Status`, `ClientId`; configurar concorrência otimista com `.UseXminAsConcurrencyToken()` no entity builder do Npgsql
- [X] T011 Criar `src/GaragePro.Infrastructure/Data/Configurations/AppointmentRescheduleHistoryConfiguration.cs` implementando `IEntityTypeConfiguration<AppointmentRescheduleHistory>`: PK `Id`; FK para `Appointment` com cascade delete; FK para `User` via `ChangedByUserId`; mapear todas as propriedades
- [X] T012 Atualizar `src/GaragePro.Infrastructure/Data/AppDbContext.cs`: adicionar `public DbSet<Appointment> Appointments => Set<Appointment>();` e `public DbSet<AppointmentRescheduleHistory> AppointmentRescheduleHistories => Set<AppointmentRescheduleHistory>();`; registrar `ApplyConfiguration(new AppointmentConfiguration())` e `ApplyConfiguration(new AppointmentRescheduleHistoryConfiguration())` em `OnModelCreating`
- [X] T013 Criar `src/GaragePro.Infrastructure/Repositories/AppointmentRepository.cs` implementando `IAppointmentRepository`: injetar `AppDbContext`; implementar `GetByIdAsync` com `.Include(a => a.Client).Include(a => a.Product).Include(a => a.Service).Include(a => a.RescheduleHistory).ThenInclude(h => h.ChangedBy)`; implementar `AddAsync` (gerar Id via `Guid.NewGuid()`, `context.Add`, `SaveChangesAsync`); implementar `UpdateAsync` (`context.Update`, `SaveChangesAsync`); deixar `GetAllAsync` e `GetSummaryAsync` com filtragem mínima por data (implementação completa em US5)
- [X] T014 Registrar `services.AddScoped<IAppointmentRepository, AppointmentRepository>()` em `src/GaragePro.Infrastructure/DependencyInjection.cs`
- [X] T015 Gerar e aplicar migration `AddAppointments` executando `dotnet ef migrations add AddAppointments --startup-project src/GaragePro.API --project src/GaragePro.Infrastructure` e depois `dotnet ef database update --startup-project src/GaragePro.API --project src/GaragePro.Infrastructure`
- [X] T016 Criar skeleton de `src/GaragePro.API/Endpoints/AppointmentsEndpoints.cs` com `public static class AppointmentsEndpoints` e `public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder routes) { return routes; }` e registrar `app.MapAppointmentsEndpoints()` em `src/GaragePro.API/Program.cs` junto aos demais endpoints

**Checkpoint**: Projeto compila sem erros, migration aplicada, DI registrada, skeleton de endpoints presente.

---

## Phase 3: User Story 1 — Consultar Agendamentos do Período (P1) 🎯 MVP

**Goal**: Usuário abre a tela, visualiza listagem paginada com todos os dados essenciais e resumo financeiro por status.

**Independent Test**: Inserir agendamentos via seed com diferentes status e datas; acessar `GET /api/appointments` e confirmar que response inclui `data.data` com colunas corretas e `summary` com totais por status; abrir frontend em `/appointments` e verificar lista, tag "Remarcado" e rodapé financeiro.

- [X] T017 [P] [US1] Criar DTOs de resposta em `src/GaragePro.Application/Features/Appointments/AppointmentResponses.cs`: records `AppointmentSummaryResponse(Guid Id, string ClientName, string ProductName, string ServiceName, DateTimeOffset StartAt, DateTimeOffset ExpectedEndAt, AppointmentStatus Status, bool IsRescheduled, decimal TotalValue, string? Notes)`, `AppointmentSummaryDto(int ScheduledCount, decimal ScheduledTotal, int CompletedCount, decimal CompletedTotal, int CanceledCount, decimal CanceledTotal)`, `AppointmentListResponse(PaginatedResult<AppointmentSummaryResponse> Data, AppointmentSummaryDto Summary)`, `AppointmentRescheduleHistoryResponse(Guid Id, DateTimeOffset PreviousStartAt, DateTimeOffset PreviousExpectedEndAt, DateTimeOffset NewStartAt, DateTimeOffset NewExpectedEndAt, string? Reason, string ChangedByUserName, DateTimeOffset ChangedAt)`, `AppointmentDetailResponse(Guid Id, Guid ClientId, string ClientName, Guid ProductId, string ProductName, Guid ServiceId, string ServiceName, DateTimeOffset StartAt, DateTimeOffset ExpectedEndAt, AppointmentStatus Status, bool IsRescheduled, int RescheduleCount, decimal ProductValueSnapshot, decimal ServiceValueSnapshot, decimal TotalValue, string? Notes, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IEnumerable<AppointmentRescheduleHistoryResponse> RescheduleHistory)`
- [X] T018 [P] [US1] Implementar `GetAllAsync` em `src/GaragePro.Infrastructure/Repositories/AppointmentRepository.cs`: LINQ com `.Include(a => a.Client).Include(a => a.Product).Include(a => a.Service)`, filtro por `startDate` (`StartAt.Date >= startDate`) e `endDate` (`StartAt.Date <= endDate`), ordenação `.OrderBy(a => a.StartAt)`, paginação via `.Skip((pageNumber-1)*pageSize).Take(pageSize)`, retornar `(items, totalCount)` com `CountAsync` antes do Skip/Take
- [X] T019 [P] [US1] Implementar `GetSummaryAsync` em `src/GaragePro.Infrastructure/Repositories/AppointmentRepository.cs`: mesma base LINQ de T018 com filtros de data; calcular `AppointmentSummaryData` via três `Where(a => a.Status == X).SumAsync(a => a.TotalValue)` e `CountAsync` correspondentes (ou GroupBy se preferir single query)
- [X] T020 [P] [US1] Criar `src/GaragePro.Application/Features/Appointments/GetAll/GetAppointmentsQuery.cs`: `public record GetAppointmentsQuery(DateOnly? StartDate, DateOnly? EndDate, int PageNumber = 1, int PageSize = 20) : IRequest<Result<AppointmentListResponse>>`
- [X] T021 [US1] Criar `src/GaragePro.Application/Features/Appointments/GetAll/GetAppointmentsHandler.cs`: injetar `IAppointmentRepository`; chamar `GetAllAsync` e `GetSummaryAsync` com `StartDate`, `EndDate`, `Status = null`, `ClientId = null`, `Search = null`, `PageNumber`, `PageSize`; mapear `Appointment` para `AppointmentSummaryResponse`; construir `AppointmentListResponse` com `PaginatedResult` e `AppointmentSummaryDto`
- [X] T022 [P] [US1] Criar `src/GaragePro.Application/Features/Appointments/GetById/GetAppointmentByIdQuery.cs`: `public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDetailResponse>>`
- [X] T023 [P] [US1] Criar `src/GaragePro.Application/Features/Appointments/GetById/GetAppointmentByIdHandler.cs`: injetar `IAppointmentRepository`; chamar `GetByIdAsync`; retornar `Result.NotFound("Agendamento não encontrado.")` se null; mapear para `AppointmentDetailResponse` incluindo `RescheduleHistory` ordenado por `ChangedAt ASC`
- [X] T024 [US1] Adicionar endpoints `GET /api/appointments` e `GET /api/appointments/{id:guid}` em `src/GaragePro.API/Endpoints/AppointmentsEndpoints.cs` seguindo padrão de `ClientsEndpoints.cs`: `RequireAuthorization()` para ambos; query params `startDate`, `endDate`, `pageNumber`, `pageSize` no GET /; `Produces<AppointmentListResponse>` e `Produces<AppointmentDetailResponse>`; tratar `ResultStatus.NotFound` com `Results.NotFound`
- [X] T025 [P] [US1] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/GetAppointmentsHandlerTests.cs`: mock `IAppointmentRepository`; testes: `Handle_ShouldReturnPaginatedList_WhenAppointmentsExist`, `Handle_ShouldReturnEmptyList_WhenNoAppointmentsMatchPeriod`, `Handle_ShouldReturnCorrectSummary_WhenAppointmentsHaveMultipleStatuses`
- [X] T026 [P] [US1] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/GetAppointmentByIdHandlerTests.cs`: `Handle_ShouldReturnDetailResponse_WhenAppointmentExists`, `Handle_ShouldReturnNotFound_WhenAppointmentDoesNotExist`
- [X] T027 [P] [US1] Criar `frontend/src/app/core/models/appointment.model.ts` com interfaces: `AppointmentStatus` (enum: Scheduled = 'Scheduled', Completed = 'Completed', Canceled = 'Canceled'), `AppointmentSummary`, `AppointmentSummaryItem`, `AppointmentListResponse { data: PaginatedResult<AppointmentSummaryItem>; summary: AppointmentSummary }`, `AppointmentDetailResponse`, `AppointmentRescheduleHistoryItem`, `AppointmentFilterParams`, `CreateAppointmentCommand`, `RescheduleAppointmentCommand`
- [X] T028 [US1] Criar `frontend/src/app/core/http/appointments.service.ts`: `@Injectable({ providedIn: 'root' })`; injetar `HttpClient`; método `getAll(params: AppointmentFilterParams): Observable<AppointmentListResponse>` passando query params via `HttpParams`; método `getById(id: string): Observable<AppointmentDetailResponse>`
- [X] T029 [US1] Criar `frontend/src/app/features/appointments/appointments.routes.ts` com rotas lazy-loaded: `{ path: '', component: AppointmentsListPage }`, `{ path: 'new', component: AppointmentFormPage }`; registrar no roteamento principal adicionando `{ path: 'appointments', loadChildren: () => import('./features/appointments/appointments.routes').then(m => m.APPOINTMENTS_ROUTES) }` em `frontend/src/app/app.routes.ts`
- [X] T030 [US1] Criar `frontend/src/app/features/appointments/list/appointments-list.page.ts`: standalone component; injetar `AppointmentsService`; `signal<AppointmentListResponse | null>('appointments', null)`; `signal<boolean>('loading', false)`; `loadAppointments()` chamado no `OnInit` com período padrão (mês atual); `MatTable` com colunas: Início (DatePipe pt-BR), Término Previsto, Cliente, Produto, Serviço, Valor Total (CurrencyPipe BRL), Status (MatChip colorido por texto+cor: Scheduled=cinza, Completed=verde, Canceled=vermelho), Tags (MatChip secundário "Remarcado" quando `isRescheduled`), Ações; `MatPaginator` conectado; rodapé com 3 blocos de resumo financeiro (A realizar / Concluídos / Cancelados) via `CurrencyPipe` pt-BR; `MatProgressSpinner` durante `loading`; estado vazio com ícone e mensagem "Nenhum agendamento encontrado" quando `data.length === 0`; botão "Novo Agendamento" (`routerLink="/appointments/new"`) visível apenas para Admin e Technician

**Checkpoint**: `GET /api/appointments` e `GET /api/appointments/{id}` funcionais. Frontend exibe listagem com colunas, tag "Remarcado", resumo financeiro e estado vazio.

---

## Phase 4: User Story 2 — Criar Novo Agendamento (P1)

**Goal**: Usuário cria agendamento com cliente, produto ativo e serviço ativo; sistema captura preços como snapshot e calcula total antes de salvar.

**Independent Test**: Via frontend, abrir formulário de novo agendamento, selecionar cliente/produto/serviço, informar datas válidas, verificar exibição do total calculado e salvar; confirmar agendamento aparece na listagem com status "A realizar" e valor correto.

- [X] T031 [P] [US2] Adicionar método `Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct)` à interface `src/GaragePro.Core/Interfaces/Repositories/IProductRepository.cs` e implementar em `src/GaragePro.Infrastructure/Repositories/ProductRepository.cs` com `.Where(p => p.IsActive).ToListAsync(ct)`
- [X] T032 [P] [US2] Adicionar método `Task<IEnumerable<Service>> GetActiveAsync(CancellationToken ct)` à interface `src/GaragePro.Core/Interfaces/Repositories/IServiceRepository.cs` e implementar em `src/GaragePro.Infrastructure/Repositories/ServiceRepository.cs` com `.Where(s => s.IsActive).ToListAsync(ct)`
- [X] T033 [P] [US2] Criar `src/GaragePro.Application/Features/Appointments/Create/CreateAppointmentCommand.cs`: `public record CreateAppointmentCommand(Guid ClientId, Guid ProductId, Guid ServiceId, DateTimeOffset StartAt, DateTimeOffset ExpectedEndAt, string? Notes) : IRequest<Result<Guid>>`
- [X] T034 [P] [US2] Criar `src/GaragePro.Application/Features/Appointments/Create/CreateAppointmentValidator.cs` com `AbstractValidator<CreateAppointmentCommand>`: `ClientId` not empty; `ProductId` not empty; `ServiceId` not empty; `StartAt` not empty; `ExpectedEndAt` must satisfy `x => x > command.StartAt` com mensagem "Previsão de término deve ser posterior ao início"
- [X] T035 [US2] Criar `src/GaragePro.Application/Features/Appointments/Create/CreateAppointmentHandler.cs`: injetar `IAppointmentRepository`, `IClientRepository`, `IProductRepository`, `IServiceRepository`; verificar `client = await clientRepository.GetByIdAsync(request.ClientId)` — `Result.Failure` se null; verificar `product = await productRepository.GetByIdAsync(request.ProductId)` e `product.IsActive` — `Result.Failure` se null ou inativo; verificar `service` analogamente; construir `new Appointment { Id = Guid.NewGuid(), ClientId = request.ClientId, ProductId = request.ProductId, ServiceId = request.ServiceId, StartAt = request.StartAt, ExpectedEndAt = request.ExpectedEndAt, Status = AppointmentStatus.Scheduled, IsRescheduled = false, RescheduleCount = 0, ProductValueSnapshot = product.Price, ServiceValueSnapshot = service.Price, TotalValue = product.Price + service.Price, Notes = request.Notes, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }`; chamar `appointmentRepository.AddAsync(appointment, ct)`; retornar `Result.Success(appointment.Id)`
- [X] T036 [US2] Adicionar endpoint `POST /api/appointments` em `src/GaragePro.API/Endpoints/AppointmentsEndpoints.cs`: `RequireAuthorization("TechnicianOrAdmin")`; receber `CreateAppointmentCommand` no body; retornar `Results.Created($"/api/appointments/{result.Value}", new IdResponse(result.Value))` em sucesso; `Results.BadRequest` em `ValidationFailure` ou `Failure`
- [X] T037 [P] [US2] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/CreateAppointmentHandlerTests.cs`: `Handle_ShouldCreateAppointment_WhenAllDataIsValid`, `Handle_ShouldSnapshotProductAndServicePrices_WhenCreated`, `Handle_ShouldSetStatusToScheduled_WhenCreated`, `Handle_ShouldReturnFailure_WhenClientDoesNotExist`, `Handle_ShouldReturnFailure_WhenProductIsInactive`, `Handle_ShouldReturnFailure_WhenServiceIsInactive`
- [X] T038 [US2] Atualizar `frontend/src/app/core/http/appointments.service.ts`: adicionar `create(command: CreateAppointmentCommand): Observable<{id: string}>`; adicionar `getActiveProducts(): Observable<Product[]>` (`GET /api/products?isActive=true` ou equivalente); adicionar `getActiveServices(): Observable<Service[]>`
- [X] T039 [US2] Criar `frontend/src/app/features/appointments/detail/appointment-form.page.ts`: standalone component com `ReactiveFormsModule`; `FormGroup` com controles: `clientId` (MatAutocomplete consumindo `ClientsService.getAll()`, exibindo nome, armazenando Id), `productId` (MatSelect de produtos ativos via `getActiveProducts()`), `serviceId` (MatSelect de serviços ativos via `getActiveServices()`), `startAt` (MatDatetimePicker, obrigatório), `expectedEndAt` (MatDatetimePicker, obrigatório), `notes` (MatInput textarea, opcional); Signal `totalValue` computado como `product.price + service.price` quando ambos selecionados — exibido em destaque com `CurrencyPipe` BRL antes do botão Salvar; validação client-side: `expectedEndAt > startAt` com `setErrors`; submit chama `AppointmentsService.create()`, exibe `MatSnackBar` de sucesso (3s) e navega para `/appointments`; botão desabilitado e spinner visível durante chamada HTTP; erros de API exibidos via `MatSnackBar` (6s)

**Checkpoint**: `POST /api/appointments` funcional com snapshot de preços. Formulário cria agendamento mostrando total calculado e retorna à lista.

---

## Phase 5: User Story 3 — Concluir ou Cancelar Agendamento (P1)

**Goal**: Usuário conclui ou cancela um agendamento "A realizar" diretamente da listagem. Concluir é direto; cancelar exige confirmação. Ações ficam indisponíveis para status final.

**Independent Test**: Na listagem, clicar em concluir em um agendamento "A realizar" → status muda para "Concluído" e valor vai para bloco "Concluídos" no rodapé. Clicar em cancelar → dialog de confirmação → confirmar → status muda para "Cancelado". Verificar que ações somem de agendamentos finais.

- [X] T040 [P] [US3] Criar `src/GaragePro.Application/Features/Appointments/Complete/CompleteAppointmentCommand.cs`: `public record CompleteAppointmentCommand(Guid Id) : IRequest<Result<Guid>>`
- [X] T041 [P] [US3] Criar `src/GaragePro.Application/Features/Appointments/Complete/CompleteAppointmentHandler.cs`: injetar `IAppointmentRepository`; `GetByIdAsync` — `NotFound` se null; verificar `appointment.Status == AppointmentStatus.Scheduled` — `Result.Failure("Apenas agendamentos 'A realizar' podem ser concluídos.")` caso contrário; alterar `appointment.Status = AppointmentStatus.Completed` e `appointment.UpdatedAt = DateTimeOffset.UtcNow`; `UpdateAsync`; capturar `DbUpdateConcurrencyException` retornando `Result.Failure("O agendamento foi modificado por outro usuário. Recarregue e tente novamente.")`; retornar `Result.Success(appointment.Id)`
- [X] T042 [P] [US3] Criar `src/GaragePro.Application/Features/Appointments/Cancel/CancelAppointmentCommand.cs`: `public record CancelAppointmentCommand(Guid Id) : IRequest<Result<Guid>>`
- [X] T043 [P] [US3] Criar `src/GaragePro.Application/Features/Appointments/Cancel/CancelAppointmentHandler.cs`: mesma lógica de `CompleteAppointmentHandler` (T041) mas define `appointment.Status = AppointmentStatus.Canceled` e mensagem de erro diferente
- [X] T044 [US3] Adicionar endpoints `POST /api/appointments/{id:guid}/complete` e `POST /api/appointments/{id:guid}/cancel` em `src/GaragePro.API/Endpoints/AppointmentsEndpoints.cs`: `RequireAuthorization("TechnicianOrAdmin")`; retornar `200 Ok` com `IdResponse` em sucesso; `400 BadRequest` para transição inválida; `404 NotFound`; `409 Conflict` quando `result.Error` contém mensagem de concorrência (verificar via `ResultStatus.Failure` e mapear para 409 se for erro de concorrência)
- [X] T045 [P] [US3] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/CompleteAppointmentHandlerTests.cs`: `Handle_ShouldCompleteAppointment_WhenStatusIsScheduled`, `Handle_ShouldReturnFailure_WhenStatusIsAlreadyCompleted`, `Handle_ShouldReturnFailure_WhenStatusIsCanceled`, `Handle_ShouldReturnFailure_WhenAppointmentNotFound`
- [X] T046 [P] [US3] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/CancelAppointmentHandlerTests.cs`: `Handle_ShouldCancelAppointment_WhenStatusIsScheduled`, `Handle_ShouldReturnFailure_WhenStatusIsAlreadyCompleted`, `Handle_ShouldReturnFailure_WhenStatusIsCanceled`, `Handle_ShouldReturnFailure_WhenAppointmentNotFound`
- [X] T047 [P] [US3] Atualizar `frontend/src/app/core/http/appointments.service.ts`: adicionar `complete(id: string): Observable<{id: string}>` (`POST /api/appointments/{id}/complete`) e `cancel(id: string): Observable<{id: string}>` (`POST /api/appointments/{id}/cancel`)
- [X] T048 [P] [US3] Criar `frontend/src/app/features/appointments/detail/cancel-dialog.component.ts`: standalone component com `MatDialogModule`; receber via `MAT_DIALOG_DATA` o nome do cliente e data do agendamento; exibir mensagem de confirmação; botão "Cancelar" fecha dialog (retorna false); botão "Confirmar cancelamento" fecha dialog (retorna true); nenhuma chamada HTTP dentro do dialog
- [X] T049 [US3] Atualizar `frontend/src/app/features/appointments/list/appointments-list.page.ts`: adicionar coluna de ações com `MatIconButton` + `matTooltip`; ícone concluir (check_circle) visível apenas quando `status === 'Scheduled'` — clique chama `complete(id)` diretamente e recarrega lista com snackBar sucesso (3s); ícone cancelar (cancel) visível apenas quando `status === 'Scheduled'` — clique abre `CancelDialogComponent`; se dialog retornar true, chama `cancel(id)` e recarrega lista; tratar HTTP 409 exibindo snackBar "O agendamento foi modificado por outro usuário. Recarregue a página." (6s); desabilitar botões durante chamada em andamento

**Checkpoint**: `POST /complete` e `POST /cancel` funcionais com transições de estado e concorrência otimista. Frontend conclui/cancela inline com confirmação para cancelamento.

---

## Phase 6: User Story 4 — Remarcar Agendamento (P1)

**Goal**: Usuário remarca um agendamento "A realizar" para nova data, mantendo cliente/produto/serviço/valor. Tag "Remarcado" aparece na listagem. Histórico registra datas anteriores e novas.

**Independent Test**: Na listagem, clicar em remarcar um agendamento "A realizar" → dialog com nova data/hora → confirmar → agendamento aparece com tag "Remarcado"; em `GET /api/appointments/{id}` verificar `isRescheduled: true` e `rescheduleHistory` com registro de alteração.

- [X] T050 [P] [US4] Criar `src/GaragePro.Application/Features/Appointments/Reschedule/RescheduleAppointmentCommand.cs`: `public record RescheduleAppointmentCommand(Guid Id, DateTimeOffset NewStartAt, DateTimeOffset NewExpectedEndAt, string? Reason) : IRequest<Result<Guid>>`
- [X] T051 [P] [US4] Criar `src/GaragePro.Application/Features/Appointments/Reschedule/RescheduleAppointmentValidator.cs` com `AbstractValidator<RescheduleAppointmentCommand>`: `NewStartAt` must satisfy `x => x > DateTimeOffset.UtcNow` com mensagem "A nova data de início deve ser no futuro"; `NewExpectedEndAt` must satisfy `x => x > command.NewStartAt` com mensagem "Previsão de término deve ser posterior ao início"
- [X] T052 [US4] Criar `src/GaragePro.Application/Features/Appointments/Reschedule/RescheduleAppointmentHandler.cs`: injetar `IAppointmentRepository`; `GetByIdAsync` — `NotFound` se null; verificar `Status == Scheduled` — `Result.Failure` caso contrário; criar `new AppointmentRescheduleHistory { Id = Guid.NewGuid(), AppointmentId = appointment.Id, PreviousStartAt = appointment.StartAt, PreviousExpectedEndAt = appointment.ExpectedEndAt, NewStartAt = request.NewStartAt, NewExpectedEndAt = request.NewExpectedEndAt, Reason = request.Reason, ChangedByUserId = /* obter do contexto HTTP via IHttpContextAccessor ou parâmetro no command */, ChangedAt = DateTimeOffset.UtcNow }`; atualizar `appointment.StartAt = request.NewStartAt`, `appointment.ExpectedEndAt = request.NewExpectedEndAt`, `appointment.IsRescheduled = true`, `appointment.RescheduleCount++`, `appointment.UpdatedAt = DateTimeOffset.UtcNow`; adicionar history em `appointment.RescheduleHistory.Add(history)`; `UpdateAsync`; capturar `DbUpdateConcurrencyException`; retornar `Result.Success(appointment.Id)`
- [X] T053 [US4] Adicionar endpoint `POST /api/appointments/{id:guid}/reschedule` em `src/GaragePro.API/Endpoints/AppointmentsEndpoints.cs`: `RequireAuthorization("TechnicianOrAdmin")`; receber `RescheduleAppointmentCommand` no body; fazer merge de `id` via `command with { Id = id }`; retornar `200 Ok`, `400`, `404`, `409`
- [X] T054 [P] [US4] Criar testes em `tests/GaragePro.UnitTests/Handlers/Appointments/RescheduleAppointmentHandlerTests.cs`: `Handle_ShouldRescheduleAppointment_WhenStatusIsScheduled`, `Handle_ShouldSetIsRescheduledTrue_WhenRescheduled`, `Handle_ShouldIncrementRescheduleCount_WhenRescheduledMultipleTimes`, `Handle_ShouldCreateHistoryRecord_WhenRescheduled`, `Handle_ShouldReturnFailure_WhenStatusIsNotScheduled`, `Handle_ShouldReturnFailure_WhenAppointmentNotFound`
- [X] T055 [P] [US4] Atualizar `frontend/src/app/core/http/appointments.service.ts`: adicionar `reschedule(id: string, command: RescheduleAppointmentCommand): Observable<{id: string}>` (`POST /api/appointments/{id}/reschedule`)
- [X] T056 [US4] Criar `frontend/src/app/features/appointments/detail/reschedule-dialog.component.ts`: standalone component com `MatDialogModule` e `ReactiveFormsModule`; receber agendamento atual via `MAT_DIALOG_DATA` (clientName, startAt atual); `FormGroup` com `newStartAt` (MatDatetimePicker, obrigatório, futuro), `newExpectedEndAt` (MatDatetimePicker, obrigatório, > newStartAt), `reason` (MatInput textarea, opcional); validação client-side espelha backend; botão "Confirmar remarcação" chama `AppointmentsService.reschedule()`, exibe snackBar sucesso (3s) e fecha dialog retornando true; botão "Cancelar" fecha sem ação; spinner visível durante chamada; erros via snackBar (6s)
- [X] T057 [US4] Atualizar `frontend/src/app/features/appointments/list/appointments-list.page.ts`: adicionar ícone remarcar (event_repeat) na coluna de ações, visível apenas quando `status === 'Scheduled'`; clique abre `RescheduleDialogComponent`; se retornar true, recarrega lista; garantir que `MatChip` "Remarcado" é exibido com estilo secundário (menor opacidade ou cor cinza-azulada) sem competir com chip de status principal

**Checkpoint**: `POST /reschedule` funcional com histórico e `IsRescheduled = true`. Dialog de remarcação funcional; tag "Remarcado" aparece após operação.

---

## Phase 7: User Story 5 — Filtrar, Buscar e Trabalhar Rápido (P2)

**Goal**: Usuário filtra agendamentos por período, status, cliente e busca textual. Lista e resumo financeiro refletem exatamente os registros filtrados.

**Independent Test**: Aplicar filtro de status "Concluído" → lista exibe apenas concluídos → rodapé mostra 0 para A realizar e Cancelados e apenas total de Concluídos. Buscar pelo nome de um cliente → apenas agendamentos desse cliente aparecem.

- [X] T058 [P] [US5] Atualizar `src/GaragePro.Application/Features/Appointments/GetAll/GetAppointmentsQuery.cs`: adicionar parâmetros `AppointmentStatus? Status`, `Guid? ClientId`, `string? Search` ao record
- [X] T059 [US5] Atualizar `src/GaragePro.Application/Features/Appointments/GetAll/GetAppointmentsHandler.cs` para passar `Status`, `ClientId`, `Search` ao chamar `GetAllAsync` e `GetSummaryAsync`
- [X] T060 [US5] Atualizar `GetAllAsync` e `GetSummaryAsync` em `src/GaragePro.Infrastructure/Repositories/AppointmentRepository.cs`: adicionar filtros `if (status.HasValue) query = query.Where(a => a.Status == status.Value)`; `if (clientId.HasValue) query = query.Where(a => a.ClientId == clientId.Value)`; `if (!string.IsNullOrWhiteSpace(search)) query = query.Where(a => a.Client.Name.Contains(search) || a.Product.Name.Contains(search) || a.Service.Name.Contains(search))`; aplicar os mesmos filtros em `GetSummaryAsync`
- [X] T061 [US5] Atualizar `frontend/src/app/features/appointments/list/appointments-list.page.ts`: adicionar barra de filtros acima da tabela com: `MatDateRangePicker` (Início e Fim de período com opções rápidas Hoje/Semana/Mês/Personalizado via MatButtonToggle), `MatSelect` de status (opções: Todos/A realizar/Concluído/Cancelado), `MatAutocomplete` de cliente (busca por nome), `MatInput` de busca livre com debounce 300ms via `fromEvent`; armazenar filtros em `signal<AppointmentFilterParams>`; `effect` que chama `loadAppointments()` ao mudar filtros; botão "Limpar filtros" visível quando algum filtro estiver ativo; atualizar `AppointmentsService.getAll()` para aceitar e passar todos os parâmetros de filtro como query params

**Checkpoint**: Filtros de período, status, cliente e busca funcionam. Resumo financeiro reflete exatamente os registros filtrados.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Cobertura de testes de validators, UX final, controle de acesso e build limpo.

- [X] T062 [P] Criar `tests/GaragePro.UnitTests/Handlers/Appointments/CreateAppointmentValidatorTests.cs`: `Validate_ShouldFail_WhenExpectedEndAtIsBeforeStartAt`, `Validate_ShouldFail_WhenClientIdIsEmpty`, `Validate_ShouldPass_WhenAllFieldsAreValid`
- [X] T063 [P] Criar `tests/GaragePro.UnitTests/Handlers/Appointments/RescheduleAppointmentValidatorTests.cs`: `Validate_ShouldFail_WhenNewStartAtIsInThePast`, `Validate_ShouldFail_WhenNewExpectedEndAtIsBeforeNewStartAt`, `Validate_ShouldPass_WhenAllFieldsAreValid`
- [X] T064 [P] Garantir acessibilidade nos chips de status em `frontend/src/app/features/appointments/list/appointments-list.page.ts`: cada `MatChip` deve ter texto legível (não apenas cor) — "A realizar", "Concluído", "Cancelado" — usando classes de tema Material 3 e nenhum `#hex` literal; adicionar `aria-label` nos ícones de ação
- [X] T065 [P] Verificar que `frontend/src/app/features/appointments/list/appointments-list.page.ts` oculta/desabilita botão "Novo Agendamento" e ícones de ação (concluir/cancelar/remarcar) para usuários com role `Financial`, usando o serviço de autenticação existente no projeto para leitura do role do token JWT
- [X] T066 Executar `dotnet test tests/GaragePro.UnitTests/GaragePro.UnitTests.csproj` e corrigir falhas; executar `ng build --configuration production` dentro de `frontend/` e corrigir erros de compilação TypeScript; verificar fluxo completo seguindo `specs/005-scheduling-screen/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1**: Sem dependências — iniciar imediatamente. T001–T004 em paralelo.
- **Phase 2**: Depende de Phase 1 completa. T006–T009 em paralelo; T010–T012 após T006–T009; T013 após T010–T012; T014–T016 após T013.
- **Phase 3 (US1)**: Depende de Phase 2 completa.
- **Phase 4 (US2)**: Depende de Phase 2 completa; requer T031–T032 antes de T035.
- **Phase 5 (US3)**: Depende de Phase 2 completa; frontend depende de Phase 3 (precisa da lista).
- **Phase 6 (US4)**: Depende de Phase 2 completa; frontend depende de Phase 3 (precisa da lista).
- **Phase 7 (US5)**: Depende de Phase 3 completa (estende `GetAppointmentsQuery`).
- **Phase 8**: Após todas as user stories.

### User Story Dependencies

- **US1 (P1)**: Sem dependência de outras stories.
- **US2 (P1)**: Sem dependência de US1 no backend; frontend usa a lista de US1.
- **US3 (P1)**: Sem dependência de US1/US2 no backend; frontend usa a lista de US1.
- **US4 (P1)**: Sem dependência de US1/US2/US3 no backend; frontend usa a lista de US1.
- **US5 (P2)**: Estende US1 — iniciar após US1 completa.

### Within Each User Story

- Backend: Responses/DTOs → Repository methods → Command/Query → Handler → Endpoint → Testes
- Frontend: Models → Service → Component
- Handlers e seus testes devem ser entregues no mesmo incremento

### Parallel Opportunities

| Fase | Tasks em paralelo |
|------|-------------------|
| Phase 1 | T001, T002, T003, T004 |
| Phase 2 (início) | T006, T007, T008 |
| Phase 3 (início) | T017, T018, T019, T020, T022, T023, T025, T026, T027 |
| Phase 4 (início) | T031, T032, T033, T034, T037 |
| Phase 5 (início) | T040, T042, T045, T046, T047, T048 |
| Phase 6 (início) | T050, T051, T054, T055 |
| Phase 8 | T062, T063, T064, T065 |

---

## Parallel Example: User Story 5 (Complete/Cancel)

```
# Em paralelo (arquivos independentes):
T040: CompleteAppointmentCommand.cs
T042: CancelAppointmentCommand.cs
T045: CompleteAppointmentHandlerTests.cs
T046: CancelAppointmentHandlerTests.cs
T047: appointments.service.ts (complete + cancel)
T048: cancel-dialog.component.ts

# Após T040 concluído:
T041: CompleteAppointmentHandler.cs

# Após T042 concluído:
T043: CancelAppointmentHandler.cs

# Após T041 e T043 concluídos:
T044: AppointmentsEndpoints.cs (adicionar /complete e /cancel)

# Após T047 e T048 e T044 concluídos:
T049: appointments-list.page.ts (ações concluir/cancelar)
```

---

## Implementation Strategy

### MVP (User Story 1 apenas)

1. Completar Phase 1 → Phase 2
2. Completar Phase 3 (US1 — Listagem)
3. **PARAR E VALIDAR**: `GET /api/appointments` retorna lista e resumo; frontend exibe tabela, resumo financeiro e estado vazio
4. Demo/deploy se aprovado

### Entrega Incremental

| Etapa | Fase | Entrega |
|-------|------|---------|
| 1 | Phase 1 + 2 | Infraestrutura pronta |
| 2 | Phase 3 (US1) | Lista funcional → **Demo** |
| 3 | Phase 4 (US2) | Criar agendamentos → Demo |
| 4 | Phase 5 (US3) | Concluir/Cancelar → Demo |
| 5 | Phase 6 (US4) | Remarcar → Demo |
| 6 | Phase 7 (US5) | Filtros completos → Demo |
| 7 | Phase 8 | Produto final |

---

## Task Summary

| Fase | Tasks | US |
|------|-------|----|
| Phase 1 (Setup) | T001–T005 | — |
| Phase 2 (Foundational) | T006–T016 | — |
| Phase 3 (US1) | T017–T030 | US1 |
| Phase 4 (US2) | T031–T039 | US2 |
| Phase 5 (US3) | T040–T049 | US3 |
| Phase 6 (US4) | T050–T057 | US4 |
| Phase 7 (US5) | T058–T061 | US5 |
| Phase 8 (Polish) | T062–T066 | — |
| **Total** | **66 tasks** | |

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências entre si — podem ser executadas em paralelo
- [Story] label obrigatório em todas as tasks de fases de user story (Phase 3–7)
- Concorrência otimista via `xmin` do PostgreSQL — nenhuma coluna extra necessária
- snake_case de colunas é tratado automaticamente pelo `UseSnakeCaseNamingConvention()` — propriedades C# permanecem PascalCase
- Commitar após cada checkpoint de fase
- Parar em cada checkpoint para validar a story independentemente antes de avançar
