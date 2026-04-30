# Quickstart: GaragePro Core CRUD API

**Branch**: `002-core-crud-api` | **Date**: 2026-04-27

## Pré-requisitos

- .NET 10 SDK
- Docker + Docker Compose (para PostgreSQL local)
- IDE: Visual Studio 2022 / Rider / VS Code com C# DevKit

---

## 1. Configurar Banco de Dados (PostgreSQL via Docker)

```bash
docker run --name garagepro-pg \
  -e POSTGRES_DB=garagepro \
  -e POSTGRES_USER=garagepro \
  -e POSTGRES_PASSWORD=garagepro123 \
  -p 5432:5432 \
  -d postgres:15
```

---

## 2. Criar a Solução

```bash
# Na raiz do repositório
dotnet new sln -n GaragePro

# Projetos
dotnet new webapi -n GaragePro.API -o src/GaragePro.API --no-openapi
dotnet new classlib -n GaragePro.Application -o src/GaragePro.Application
dotnet new classlib -n GaragePro.Core -o src/GaragePro.Core
dotnet new classlib -n GaragePro.Infrastructure -o src/GaragePro.Infrastructure
dotnet new xunit -n GaragePro.UnitTests -o tests/GaragePro.UnitTests

# Adicionar à solução
dotnet sln add src/GaragePro.API
dotnet sln add src/GaragePro.Application
dotnet sln add src/GaragePro.Core
dotnet sln add src/GaragePro.Infrastructure
dotnet sln add tests/GaragePro.UnitTests

# Referências entre projetos
dotnet add src/GaragePro.API reference src/GaragePro.Application src/GaragePro.Infrastructure
dotnet add src/GaragePro.Application reference src/GaragePro.Core
dotnet add src/GaragePro.Infrastructure reference src/GaragePro.Core
dotnet add tests/GaragePro.UnitTests reference src/GaragePro.Application src/GaragePro.Core
```

---

## 3. Instalar Pacotes NuGet

```bash
# GaragePro.API
dotnet add src/GaragePro.API package Swashbuckle.AspNetCore
dotnet add src/GaragePro.API package Microsoft.AspNetCore.Authentication.JwtBearer

# GaragePro.Application
dotnet add src/GaragePro.Application package MediatR
dotnet add src/GaragePro.Application package FluentValidation.DependencyInjectionExtensions

# GaragePro.Infrastructure
dotnet add src/GaragePro.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/GaragePro.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/GaragePro.Infrastructure package EFCore.NamingConventions
dotnet add src/GaragePro.Infrastructure package BCrypt.Net-Next
dotnet add src/GaragePro.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/GaragePro.Infrastructure package Microsoft.Extensions.Configuration.Abstractions

# GaragePro.UnitTests
dotnet add tests/GaragePro.UnitTests package Moq
dotnet add tests/GaragePro.UnitTests package FluentAssertions
dotnet add tests/GaragePro.UnitTests package Bogus
```

---

## 4. Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=garagepro;Username=garagepro;Password=garagepro123"
  },
  "Jwt": {
    "Secret": "SUBSTITUIR_POR_SEGREDO_DE_32_CARACTERES_MINIMO",
    "Issuer": "garagepro-api",
    "Audience": "garagepro-clients",
    "ExpiryMinutes": 60
  }
}
```

---

## 5. Executar Migrations

```bash
dotnet ef migrations add InitialCreate \
  --project src/GaragePro.Infrastructure \
  --startup-project src/GaragePro.API

dotnet ef database update \
  --project src/GaragePro.Infrastructure \
  --startup-project src/GaragePro.API
```

---

## 6. Executar a API

```bash
dotnet run --project src/GaragePro.API
```

**Swagger UI**: https://localhost:44384/swagger

---

## 7. Executar Testes

```bash
dotnet test tests/GaragePro.UnitTests
```

---

## Fluxo de Autenticação

```bash
# 1. Fazer login
curl -X POST https://localhost:44384/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@garagepro.com","password":"senha123"}'

# 2. Usar o token retornado
curl -X GET https://localhost:44384/api/clients \
  -H "Authorization: Bearer <TOKEN>"
```

---

## Seed de Dados (Desenvolvimento)

Adicione um seed no `Program.cs` para criar um usuário admin inicial em Development:

```csharp
if (app.Environment.IsDevelopment())
{
    await app.SeedAdminUserAsync(); // extensão no namespace GaragePro.API.Extensions
}
```

Admin padrão: `admin@garagepro.com` / `Admin@1234`
