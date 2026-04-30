# API Reference: GaragePro Core CRUD API

**Branch**: `002-core-crud-api` | **Date**: 2026-04-27  
**Base URL**: `/api`  
**Auth**: JWT Bearer — todos os endpoints exceto `/auth/login` exigem `Authorization: Bearer <token>`

---

## Convenções Globais

### Paginação (endpoints de listagem)

**Query params**: `?pageNumber=1&pageSize=20` (pageSize máximo: 100)

**Response envelope**:
```json
{
  "data": [...],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 47,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Respostas de Erro

| HTTP | Estrutura |
|------|-----------|
| 400 Bad Request | `{ "error": "mensagem", "errors": ["campo: detalhe"] }` |
| 401 Unauthorized | `{ "error": "Unauthorized" }` |
| 403 Forbidden | `{ "error": "Forbidden" }` |
| 404 Not Found | `{ "error": "mensagem" }` |
| 409 Conflict | `{ "error": "mensagem de conflito" }` |
| 500 Internal | `{ "error": "Internal server error" }` |

---

## Auth

### POST /api/auth/login

Autentica um usuário e retorna um access token JWT.

**Acesso**: Público (sem autenticação)

**Request**:
```json
{
  "email": "admin@garagepro.com",
  "password": "senha123"
}
```

**Validações**:
- `email`: obrigatório, formato válido
- `password`: obrigatório, min 6 caracteres

**Response 200**:
```json
{
  "accessToken": "eyJ...",
  "expiresAt": "2026-04-27T15:00:00Z",
  "user": {
    "id": "uuid",
    "name": "Admin",
    "email": "admin@garagepro.com",
    "roles": ["Admin"]
  }
}
```

**Response 401**: Credenciais inválidas

---

## Users

**Acesso**: Admin apenas

### GET /api/users

Lista usuários paginados.

**Query**: `?pageNumber=1&pageSize=20`

**Response 200**: `PaginatedResult<UserResponse>`

```json
{
  "data": [
    {
      "id": "uuid",
      "name": "João Silva",
      "email": "joao@garagem.com",
      "roles": ["Technician"],
      "createdAt": "2026-04-27T10:00:00Z"
    }
  ],
  "pagination": { ... }
}
```

### GET /api/users/{id}

Busca usuário por ID.

**Response 200**: `UserResponse` (estrutura acima)  
**Response 404**: Usuário não encontrado

### POST /api/users

Cria novo usuário.

**Request**:
```json
{
  "name": "João Silva",
  "email": "joao@garagem.com",
  "password": "senha123",
  "roles": ["Technician"]
}
```

**Validações**:
- `name`: obrigatório, max 200 chars
- `email`: obrigatório, formato válido, max 256 chars, único
- `password`: obrigatório, min 8 chars
- `roles`: obrigatório, mínimo 1 role, valores válidos: `Admin`, `Technician`, `Financial`

**Response 201**: `{ "id": "uuid" }`  
**Response 400**: Erros de validação  
**Response 409**: Email já cadastrado

### PUT /api/users/{id}

Atualiza dados de um usuário.

**Request**:
```json
{
  "name": "João Santos",
  "email": "joao.santos@garagem.com",
  "roles": ["Technician", "Financial"]
}
```

**Validações**: Mesmas do POST exceto `password` (alteração de senha não suportada neste endpoint)

**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Usuário não encontrado  
**Response 409**: Email já em uso por outro usuário

### DELETE /api/users/{id}

Remove um usuário.

**Response 204**: Removido com sucesso  
**Response 404**: Usuário não encontrado

---

## Clients

**Acesso**:
- Admin: CRUD completo
- Technician: CRUD completo
- Financial: somente GET (leitura)

### GET /api/clients

Lista clientes paginados.

**Query**: `?pageNumber=1&pageSize=20`

**Response 200**: `PaginatedResult<ClientSummaryResponse>`

```json
{
  "data": [
    {
      "id": "uuid",
      "name": "Maria Oliveira",
      "email": "maria@email.com",
      "phone": "(11) 99999-9999",
      "vehicleCount": 2,
      "createdAt": "2026-04-27T10:00:00Z"
    }
  ],
  "pagination": { ... }
}
```

### GET /api/clients/{id}

Busca cliente com perfil completo (endereços + veículos).

**Response 200**: `ClientDetailResponse`

```json
{
  "id": "uuid",
  "name": "Maria Oliveira",
  "email": "maria@email.com",
  "phone": "(11) 99999-9999",
  "document": "12345678900",
  "addresses": [
    {
      "id": "uuid",
      "type": "Residential",
      "street": "Rua das Flores",
      "number": "100",
      "complement": "Apto 2",
      "district": "Centro",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01310-100"
    }
  ],
  "vehicles": [
    {
      "id": "uuid",
      "licensePlate": "ABC1D23",
      "make": "Honda",
      "model": "Civic",
      "year": 2020,
      "color": "Prata"
    }
  ],
  "createdAt": "2026-04-27T10:00:00Z",
  "updatedAt": "2026-04-27T10:00:00Z"
}
```

### POST /api/clients

Cria novo cliente com pelo menos um endereço.

**Request**:
```json
{
  "name": "Maria Oliveira",
  "email": "maria@email.com",
  "phone": "(11) 99999-9999",
  "document": "12345678900",
  "addresses": [
    {
      "type": "Residential",
      "street": "Rua das Flores",
      "number": "100",
      "complement": "Apto 2",
      "district": "Centro",
      "city": "São Paulo",
      "state": "SP",
      "zipCode": "01310-100"
    }
  ]
}
```

**Validações**:
- `name`: obrigatório, max 200 chars
- `addresses`: obrigatório, mínimo 1 item
- `addresses[].type`: valores válidos: `Residential`, `Billing`, `Other`
- `addresses[].street`, `.number`, `.district`, `.city`, `.state`, `.zipCode`: obrigatórios
- `addresses[].state`: exatamente 2 caracteres (código UF)

**Response 201**: `{ "id": "uuid" }`  
**Response 400**: Erros de validação (incluindo ausência de endereço)

### PUT /api/clients/{id}

Atualiza dados básicos do cliente (sem endereços — gerenciados separadamente).

**Request**:
```json
{
  "name": "Maria Silva Oliveira",
  "email": "maria.silva@email.com",
  "phone": "(11) 98888-8888",
  "document": "12345678900"
}
```

**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Cliente não encontrado

### DELETE /api/clients/{id}

Remove cliente. Rejeitado se houver veículos vinculados.

**Response 204**: Removido com sucesso  
**Response 404**: Cliente não encontrado  
**Response 400**: `{ "error": "Client has linked vehicles and cannot be deleted" }`

---

## Addresses

**Acesso**: Admin e Technician (CRUD); Financial (sem acesso)

### POST /api/clients/{clientId}/addresses

Adiciona endereço a um cliente existente.

**Request**:
```json
{
  "type": "Billing",
  "street": "Av. Paulista",
  "number": "1000",
  "complement": null,
  "district": "Bela Vista",
  "city": "São Paulo",
  "state": "SP",
  "zipCode": "01310-100"
}
```

**Response 201**: `{ "id": "uuid" }`  
**Response 404**: Cliente não encontrado

### PUT /api/clients/{clientId}/addresses/{addressId}

Atualiza um endereço existente.

**Request**: Mesma estrutura do POST  
**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Cliente ou endereço não encontrado

### DELETE /api/clients/{clientId}/addresses/{addressId}

Remove um endereço. Rejeitado se for o último endereço do cliente.

**Response 204**: Removido com sucesso  
**Response 404**: Cliente ou endereço não encontrado  
**Response 400**: `{ "error": "Client must have at least one address" }`

---

## Vehicles

**Acesso**:
- Admin: CRUD completo + Transfer
- Technician: CRUD completo + Transfer
- Financial: sem acesso

### GET /api/vehicles

Lista veículos paginados (com client name como referência).

**Query**: `?pageNumber=1&pageSize=20&clientId=uuid` (clientId opcional para filtrar por cliente)

**Response 200**: `PaginatedResult<VehicleSummaryResponse>`

```json
{
  "data": [
    {
      "id": "uuid",
      "licensePlate": "ABC1D23",
      "make": "Honda",
      "model": "Civic",
      "year": 2020,
      "color": "Prata",
      "currentOwner": { "id": "uuid", "name": "Maria Oliveira" }
    }
  ],
  "pagination": { ... }
}
```

### GET /api/vehicles/{id}

Busca veículo com histórico completo de transferências.

**Response 200**: `VehicleDetailResponse`

```json
{
  "id": "uuid",
  "licensePlate": "ABC1D23",
  "make": "Honda",
  "model": "Civic",
  "year": 2020,
  "color": "Prata",
  "vin": "1HGCM82633A004352",
  "currentOwner": { "id": "uuid", "name": "Maria Oliveira" },
  "transferHistory": [
    {
      "id": "uuid",
      "fromClient": { "id": "uuid", "name": "Carlos Santos" },
      "toClient": { "id": "uuid", "name": "Maria Oliveira" },
      "transferredAt": "2025-01-15T14:30:00Z",
      "notes": "Venda direta"
    }
  ],
  "createdAt": "2024-06-01T09:00:00Z",
  "updatedAt": "2025-01-15T14:30:00Z"
}
```

### POST /api/vehicles

Registra novo veículo vinculado a um cliente.

**Request**:
```json
{
  "clientId": "uuid",
  "licensePlate": "ABC1D23",
  "make": "Honda",
  "model": "Civic",
  "year": 2020,
  "color": "Prata",
  "vin": "1HGCM82633A004352"
}
```

**Validações**:
- `clientId`: obrigatório, cliente deve existir
- `licensePlate`: obrigatório, max 10, único no sistema
- `make`, `model`: obrigatórios, max 100
- `year`: obrigatório, entre 1900 e ano_atual + 1

**Response 201**: `{ "id": "uuid" }`  
**Response 400**: Erros de validação  
**Response 404**: Cliente não encontrado  
**Response 409**: Placa já cadastrada

### PUT /api/vehicles/{id}

Atualiza dados do veículo (exceto placa e clientId).

**Request**:
```json
{
  "make": "Honda",
  "model": "Civic EX",
  "year": 2020,
  "color": "Preto",
  "vin": "1HGCM82633A004352"
}
```

**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Veículo não encontrado

### DELETE /api/vehicles/{id}

Remove um veículo.

**Response 204**: Removido com sucesso  
**Response 404**: Veículo não encontrado

### POST /api/vehicles/{id}/transfer

Transfere veículo para outro cliente, preservando histórico.

**Request**:
```json
{
  "toClientId": "uuid",
  "notes": "Venda realizada em 27/04/2026"
}
```

**Validações**:
- `toClientId`: obrigatório, cliente destino deve existir, deve ser diferente do dono atual

**Response 200**: `{ "transferRecordId": "uuid", "transferredAt": "2026-04-27T..." }`  
**Response 404**: Veículo ou cliente destino não encontrado  
**Response 400**: `{ "error": "Target client is the current owner" }`

---

## Products

**Acesso**:
- Admin: CRUD completo
- Financial: CRUD completo
- Technician: sem acesso

### GET /api/products

Lista produtos paginados.

**Query**: `?pageNumber=1&pageSize=20`

**Response 200**: `PaginatedResult<ProductResponse>`

```json
{
  "data": [
    {
      "id": "uuid",
      "name": "Filtro de Óleo",
      "description": "Filtro de óleo para veículos leves",
      "price": 35.90,
      "createdAt": "2026-04-27T10:00:00Z"
    }
  ],
  "pagination": { ... }
}
```

### GET /api/products/{id}

Busca produto por ID.

**Response 200**: `ProductResponse` (estrutura acima)  
**Response 404**: Produto não encontrado

### POST /api/products

Cria novo produto.

**Request**:
```json
{
  "name": "Filtro de Óleo",
  "description": "Filtro de óleo para veículos leves",
  "price": 35.90
}
```

**Validações**:
- `name`: obrigatório, max 200 chars
- `price`: obrigatório, >= 0

**Response 201**: `{ "id": "uuid" }`  
**Response 400**: Erros de validação

### PUT /api/products/{id}

Atualiza produto.

**Request**: Mesma estrutura do POST  
**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Produto não encontrado

### DELETE /api/products/{id}

Remove produto.

**Response 204**: Removido com sucesso  
**Response 404**: Produto não encontrado

---

## Services

**Acesso**:
- Admin: CRUD completo
- Financial: CRUD completo
- Technician: sem acesso

### GET /api/services

Lista serviços paginados.

**Query**: `?pageNumber=1&pageSize=20`

**Response 200**: `PaginatedResult<ServiceResponse>`

```json
{
  "data": [
    {
      "id": "uuid",
      "name": "Troca de Óleo",
      "description": "Serviço completo de troca de óleo e filtro",
      "price": 89.90,
      "createdAt": "2026-04-27T10:00:00Z"
    }
  ],
  "pagination": { ... }
}
```

### GET /api/services/{id}

**Response 200**: `ServiceResponse` (estrutura acima)  
**Response 404**: Serviço não encontrado

### POST /api/services

**Request**:
```json
{
  "name": "Troca de Óleo",
  "description": "Serviço completo de troca de óleo e filtro",
  "price": 89.90
}
```

**Validações**: Mesmas do POST /products

**Response 201**: `{ "id": "uuid" }`

### PUT /api/services/{id}

**Request**: Mesma estrutura do POST  
**Response 200**: `{ "id": "uuid" }`  
**Response 404**: Serviço não encontrado

### DELETE /api/services/{id}

**Response 204**: Removido com sucesso  
**Response 404**: Serviço não encontrado

---

## Swagger / OpenAPI

A documentação interativa está disponível em:
- **Swagger UI**: `GET /swagger` (somente em Development)
- **OpenAPI JSON**: `GET /swagger/v1/swagger.json`

A especificação inclui:
- Security scheme `BearerAuth` (JWT)
- Tags por recurso (Auth, Users, Clients, Addresses, Vehicles, Products, Services)
- Exemplos de request/response em cada endpoint
- Descrições de campos via XML comments
- Códigos HTTP documentados via `Produces<T>()` e `ProducesResponseType`
