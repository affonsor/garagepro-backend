Crie a API, nessa versao teremos alguns cruds
Usuario - Pode ter mais de uma role (admin, tecnico, financeiro)
Cliente - obs: pode ter mais de um endereco, pode ter mais de um veiculo
Veiculo - estara vinculado a um cliente, pode ser transferido para outro cliente (para ter o historico de servicos pelo veiculo) 
Produto
Serviço

**Stack obrigatória:**

- .NET 10, C# 14 (primary constructors, collection expressions)
- ASP.NET Core 10 Web API
- MediatR (CQRS)
- FluentValidation
- Entity Framework Core + Postgres
- JWT Bearer + BCrypt.Net-Next