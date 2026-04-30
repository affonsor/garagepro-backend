# API Contract: Appointments

Base path: `/api/appointments`
Auth: JWT Bearer — todos os endpoints exigem autenticação.

---

## GET /api/appointments

Lista agendamentos com filtros, paginação e resumo financeiro.

**Authorization**: todos os roles (Admin, Technician, Financial)

**Query Parameters**:

| Parâmetro | Tipo | Obrigatório | Notas |
|-----------|------|-------------|-------|
| `startDate` | `DateOnly` | Não | Filtra `StartAt >= startDate` |
| `endDate` | `DateOnly` | Não | Filtra `StartAt <= endDate` |
| `status` | `AppointmentStatus?` | Não | Scheduled / Completed / Canceled |
| `clientId` | `Guid?` | Não | Filtra pelo cliente |
| `search` | `string?` | Não | Busca parcial em nome do cliente, produto ou serviço |
| `pageNumber` | `int` | Não | Default: 1 |
| `pageSize` | `int` | Não | Default: 20 |

**Response 200 OK** — `AppointmentListResponse`:

```json
{
  "data": {
    "data": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "clientName": "Maria Silva",
        "productName": "Óleo 5W30",
        "serviceName": "Troca de óleo",
        "startAt": "2026-04-28T09:00:00-03:00",
        "expectedEndAt": "2026-04-28T10:00:00-03:00",
        "status": "Scheduled",
        "isRescheduled": true,
        "totalValue": 250.00,
        "notes": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 21,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "summary": {
    "scheduledCount": 11,
    "scheduledTotal": 4150.00,
    "completedCount": 8,
    "completedTotal": 3200.00,
    "canceledCount": 2,
    "canceledTotal": 540.00
  }
}
```

---

## GET /api/appointments/{id}

Retorna o detalhe completo de um agendamento, incluindo histórico de remarcações.

**Authorization**: todos os roles

**Path Parameters**: `id: Guid`

**Response 200 OK** — `AppointmentDetailResponse`:

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "clientId": "...",
  "clientName": "Maria Silva",
  "productId": "...",
  "productName": "Óleo 5W30",
  "serviceId": "...",
  "serviceName": "Troca de óleo",
  "startAt": "2026-04-28T09:00:00-03:00",
  "expectedEndAt": "2026-04-28T10:00:00-03:00",
  "status": "Scheduled",
  "isRescheduled": true,
  "rescheduleCount": 1,
  "productValueSnapshot": 80.00,
  "serviceValueSnapshot": 170.00,
  "totalValue": 250.00,
  "notes": "Cliente solicitou troca do filtro também.",
  "createdAt": "2026-04-25T14:00:00-03:00",
  "updatedAt": "2026-04-27T09:00:00-03:00",
  "rescheduleHistory": [
    {
      "id": "...",
      "previousStartAt": "2026-04-26T09:00:00-03:00",
      "previousExpectedEndAt": "2026-04-26T10:00:00-03:00",
      "newStartAt": "2026-04-28T09:00:00-03:00",
      "newExpectedEndAt": "2026-04-28T10:00:00-03:00",
      "reason": "Cliente pediu para adiar.",
      "changedByUserName": "João Técnico",
      "changedAt": "2026-04-27T09:00:00-03:00"
    }
  ]
}
```

**Response 404 Not Found**: `{ "error": "Agendamento não encontrado." }`

---

## POST /api/appointments

Cria um novo agendamento com status `Scheduled`.

**Authorization**: Admin, Technician (`TechnicianOrAdmin`)

**Request Body** — `CreateAppointmentCommand`:

```json
{
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "serviceId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "startAt": "2026-05-10T09:00:00-03:00",
  "expectedEndAt": "2026-05-10T10:00:00-03:00",
  "notes": "Observações opcionais."
}
```

**Response 201 Created**: `{ "id": "..." }`

**Response 400 Bad Request** (validação):
```json
{ "error": "Validation failed", "errors": ["ExpectedEndAt deve ser posterior a StartAt."] }
```

---

## POST /api/appointments/{id}/complete

Conclui um agendamento.

**Authorization**: Admin, Technician (`TechnicianOrAdmin`)

**Path Parameters**: `id: Guid`

**Request Body**: vazio `{}`

**Response 200 OK**: `{ "id": "..." }`

**Response 400 Bad Request**: `{ "error": "Apenas agendamentos 'A realizar' podem ser concluídos." }`

**Response 404 Not Found**: `{ "error": "Agendamento não encontrado." }`

**Response 409 Conflict**: `{ "error": "O agendamento foi modificado por outro usuário. Recarregue e tente novamente." }`

---

## POST /api/appointments/{id}/cancel

Cancela um agendamento.

**Authorization**: Admin, Technician (`TechnicianOrAdmin`)

**Path Parameters**: `id: Guid`

**Request Body**: vazio `{}`

**Response 200 OK**: `{ "id": "..." }`

**Response 400 Bad Request**: `{ "error": "Apenas agendamentos 'A realizar' podem ser cancelados." }`

**Response 404 Not Found**: `{ "error": "Agendamento não encontrado." }`

**Response 409 Conflict**: `{ "error": "O agendamento foi modificado por outro usuário. Recarregue e tente novamente." }`

---

## POST /api/appointments/{id}/reschedule

Remarca um agendamento para nova data/horário.

**Authorization**: Admin, Technician (`TechnicianOrAdmin`)

**Path Parameters**: `id: Guid`

**Request Body** — `RescheduleAppointmentCommand`:

```json
{
  "newStartAt": "2026-05-15T10:00:00-03:00",
  "newExpectedEndAt": "2026-05-15T11:00:00-03:00",
  "reason": "Cliente pediu para adiar."
}
```

**Response 200 OK**: `{ "id": "..." }`

**Response 400 Bad Request**:
```json
{ "error": "Validation failed", "errors": ["NewStartAt deve ser no futuro."] }
```

**Response 404 Not Found**: `{ "error": "Agendamento não encontrado." }`

**Response 409 Conflict**: `{ "error": "O agendamento foi modificado por outro usuário. Recarregue e tente novamente." }`

---

## Authorization Policies

| Policy | Roles |
|--------|-------|
| (default) — `RequireAuthorization()` | Admin, Technician, Financial |
| `TechnicianOrAdmin` | Admin, Technician |

Usuário Financial pode apenas leitura (GET). O controle é feito no endpoint via `RequireAuthorization("TechnicianOrAdmin")` nas mutações.
