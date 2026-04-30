# Research: GaragePro Core CRUD API

**Branch**: `002-core-crud-api` | **Date**: 2026-04-27

## Sumário Executivo

Todas as decisões técnicas foram resolvidas. A stack é totalmente determinada pela especificação; pesquisa focou em configuração ótima de cada componente para .NET 10 + C# 14.

---

## Decisão 1 — Documentação OpenAPI / Swagger

**Decisão**: Swashbuckle.AspNetCore 7.x

**Justificativa**: Swashbuckle 7 suporta .NET 9/10, fornece Swagger UI nativa, e tem suporte maduro para anotações XML, exemplos de schema, definições de segurança JWT, e decoradores `[ProducesResponseType]`. A alternativa `Microsoft.AspNetCore.OpenApi` + Scalar é mais simples mas menos madura para documentação rica com security schemes e exemplos inline.

**Como usar**:
- Habilitar comentários XML no `.csproj` da API: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- Anotar endpoints com `WithSummary()`, `WithDescription()`, `Produces<T>()` via Minimal API extensions
- Registrar security definition para Bearer JWT na configuração do Swashbuckle
- UI disponível em `/swagger` (dev apenas)

**Alternativas consideradas**: `Microsoft.AspNetCore.OpenApi` + Scalar (descartado por maturidade inferior para anotações ricas); NSwag (descartado por complexidade de configuração desnecessária).

---

## Decisão 2 — Minimal APIs vs Controllers

**Decisão**: Minimal APIs com extension methods por recurso

**Justificativa**: .NET 10 favorece Minimal APIs; integração com MediatR via `mediator.Send()` é direta. Endpoints agrupados por `RouteGroupBuilder` mantêm organização sem overhead de controllers. Suporte nativo a `WithOpenApi()`, `WithTags()`, `WithSummary()` para documentação Swagger.

**Padrão de agrupamento**:
```csharp
// Program.cs
app.MapGroup("/api").MapAuthEndpoints()
                    .MapUsersEndpoints()
                    .MapClientsEndpoints()
                    ...

// UsersEndpoints.cs
static class UsersEndpoints {
    static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group) {
        var users = group.MapGroup("/users").RequireAuthorization().WithTags("Users");
        users.MapGet("/", GetAll);
        users.MapGet("/{id:guid}", GetById);
        ...
        return group;
    }
}
```

**Alternativas consideradas**: Controllers MVC (descartados — desnecessário para API sem views).

---

## Decisão 3 — Result Pattern

**Decisão**: Implementação própria de `Result<T>` no Application/Common

**Justificativa**: A Constituição exige `Result<T>` em todos os Handlers. Implementação própria mantém zero dependências externas no Core e Application. Estrutura mínima:

```csharp
// Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public IEnumerable<string> Errors { get; }

    public static Result<T> Success(T data) => ...;
    public static Result<T> Failure(string error) => ...;
    public static Result<T> ValidationFailure(IEnumerable<string> errors) => ...;
    public static Result<T> NotFound(string error) => ...;
}
```

**Alternativas consideradas**: FluentResults (descartado — dependência desnecessária); OneOf (descartado — curva de aprendizado e complexidade).

---

## Decisão 4 — Paginação

**Decisão**: Cursor-free offset pagination com `PageNumber` (1-based) e `PageSize`

**Padrões**:
- Default `PageSize`: 20
- Máximo `PageSize`: 100
- Query params: `?pageNumber=1&pageSize=20`
- Resposta encapsulada em `PaginatedResult<T>`:

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

**Justificativa**: Offset pagination é suficiente para o volume esperado (garagem pequena/média). Cursor pagination seria overengineering para este contexto.

---

## Decisão 5 — JWT Configuration

**Decisão**: JWT Bearer com claims customizados (userId, email, roles)

**Configuração**:
- Access token expiry: 60 minutos (configurável via `appsettings.json`)
- Algoritmo: HS256 com chave simétrica (RS256 deferido para versão futura)
- Claims: `sub` (userId), `email`, `role[]` (múltiplos roles como array)
- Refresh token: fora de escopo nesta versão

```json
// appsettings.json
"Jwt": {
  "Secret": "...",
  "Issuer": "garagepro-api",
  "Audience": "garagepro-clients",
  "ExpiryMinutes": 60
}
```

**Authorization policies**:
```csharp
// Roles via policy
builder.Services.AddAuthorization(opts => {
    opts.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    opts.AddPolicy("TechnicianOrAdmin", p => p.RequireRole("Admin", "Technician"));
    opts.AddPolicy("FinancialOrAdmin", p => p.RequireRole("Admin", "Financial"));
});
```

---

## Decisão 6 — BCrypt Configuration

**Decisão**: BCrypt.Net-Next 4 com work factor 12

**Justificativa**: Work factor 12 fornece equilíbrio entre segurança e performance (< 300ms por hash em hardware moderno). Adequado para autenticação sem impacto perceptível ao usuário.

```csharp
// Hash: BCrypt.HashPassword(plaintext, workFactor: 12)
// Verify: BCrypt.Verify(plaintext, hash)
```

---

## Decisão 7 — EF Core + PostgreSQL

**Decisão**: EF Core 9 com Npgsql 9, Code First, Migrations

**Configurações**:
- Todas as entidades configuradas via `IEntityTypeConfiguration<T>` (Fluent API — sem DataAnnotations)
- Conversor de `List<UserRole>` (enum flags) para coluna `text[]` no PostgreSQL
- Índices únicos: `Vehicle.LicensePlate`, `User.Email`
- `DateTimeOffset` para todos os campos de data (timezone-aware)
- `decimal(18,2)` para preços de Product e Service

**Naming convention**: snake_case no banco (via `UseSnakeCaseNamingConvention()` do EFCore.NamingConventions) para compatibilidade com PostgreSQL.

**Alternativas consideradas**: Dapper (descartado — EF Core exigido pela stack); EF Core 10 preview (descartado — estabilidade preferida para produção).

---

## Decisão 8 — MediatR Pipeline

**Decisão**: Pipeline com `ValidationBehavior` como único behavior registrado nesta versão

**Ordem do pipeline**: `ValidationBehavior → Handler`

**Registro**:
```csharp
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

---

## Decisão 9 — Deleção de Cliente com Veículos Vinculados

**Decisão**: Rejeitar deleção de cliente que possui veículos ativos

**Justificativa**: Deleção em cascata perderia histórico de transferências. O Handler de `DeleteClient` verifica `IVehicleRepository.HasVehiclesByClientId(clientId)` e retorna `Result.Failure("Client has linked vehicles")` se verdadeiro.

**Edge case - endereços**: Cascade delete em endereços é seguro (Address pertence exclusivamente ao Client).

---

## Decisão 10 — C# 14 Features em Uso

| Feature | Onde usar |
|---------|-----------|
| Primary constructors | Handlers, Repositories, Services (injeção de dependência) |
| Collection expressions | Inicialização de listas em testes e em `Result.Errors` |

**Exemplo primary constructor em Handler**:
```csharp
public sealed class CreateClientHandler(IClientRepository repository) 
    : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(...) { ... }
}
```
