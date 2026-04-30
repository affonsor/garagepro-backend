Idioma
A documentacao deve ser gerada sempre em portugues (pt-br)

Arquitetura

O GaragePro segue os princípios da **Clean Architecture**, com separação clara de responsabilidades entre camadas.

Camadas
Core (GaragePro.Core)
A camada mais interna. Não depende de nenhum pacote externo.
- Entities: Objetos de domínio com regras de negócio encapsuladas (Exemplo: "BaseEntity", "User", "Cliente", "Produto", etc).
- Interfaces: Contratos para repositórios ("IProdutoRepository", "IUserRepository", etc.) e serviços ("IAuthService", "IEmailService").
- Common: Constantes compartilhadas como "Roles" ("Admin", "User").
Application (LibraryManager.Application)
Casos de uso da aplicação. Depende apenas do Core.

Application (GaragePro.Application)
Casos de uso da aplicação. Depende apenas do Core.
Features: Cada feature fica em Features/{Recurso}/{Acao}/ com seus arquivos de Command/Query, Handler e Validator.
Common: Result<T> (padrão de retorno), PagedResult<T> (paginação), `ValidationBehavior` (pipeline de validação).
Registra MediatR, FluentValidation e ValidationBehavior via AddApplication().

Infrastructure (GaragePro.Infrastructure)
Implementações de infraestrutura. Depende do Core.

Persistence: AppDbContext (EF Core), Configurations/ (mapeamentos Fluent API), Repositories/ (implementações dos repositórios).
Auth: AuthService — geração de token JWT, hash e verificação de senha com BCrypt.
Email: SmtpEmailService — envio de e-mail via MailKit/SMTP.
Registra todos os serviços via AddInfrastructure().

API (GaragePro.API)
Ponto de entrada da aplicação. Depende de Application e Infrastructure.

Controllers: Organizados em Controllers/ por recurso (UserController, AuthController, ProdutoController, etc). Registrados via AddControllers() e MapControllers().
Middlewares: GlobalExceptionHandler — captura exceções não tratadas e retorna respostas padronizadas.
Program.cs: Composição da aplicação (DI, autenticação JWT, Swagger, CORS).

Regra de Dependência
API → Application → Core ← Infrastructure
Core não importa nada externo.
Infrastructure implementa as interfaces definidas no Core.
Application usa as interfaces do Core, sem saber da implementação.
API orquestra tudo via injeção de dependência.

Padrões Utilizados
CQRS com MediatR
Operações de escrita usam IRequest<Result<T>> implementadas como Commands. Leituras usam Queries. Ambos têm um Handler dedicado.
```
Features/
  User/
    CreateUser/
      CreateUserCommand.cs   ← record com os dados de entrada
      CreateUserValidator.cs ← regras de validação (FluentValidation)
      CreateUserHandler.cs   ← lógica do caso de uso
    GetAllUser/
      GetAllUsersQuery.cs
      GetAllUsersHandler.cs
```

Repository Pattern
Os repositórios são definidos como interfaces no Core e implementados na Infrastructure. Os handlers dependem apenas das interfaces — nunca do EF Core diretamente.


### Result Pattern

Todos os handlers retornam `Result<T>`. Os endpoints verificam `result.IsSuccess` antes de mapear para o HTTP status code:

```csharp
// Leitura — não encontrado
return result.IsSuccess ? Results.Ok(result.Data) : Results.NotFound(new { result.Error });

// Escrita — criação
return result.IsSuccess
    ? Results.Created($"/api/users/{result.Data}", new { id = result.Data })
    : Results.BadRequest(new { result.Error, result.Errors });
```

### ValidationBehavior (Pipeline)

O `ValidationBehavior<TRequest, TResponse>` intercepta toda chamada ao MediatR antes de chegar ao Handler. Se existir um `IValidator<TRequest>` registrado, ele valida e retorna `Result.ValidationFailure(errors)` automaticamente — os handlers não precisam validar manualmente.

## Módulos de Injeção de Dependência

```csharp
// Program.cs
builder.Services
    .AddApplication()      // MediatR + FluentValidation + ValidationBehavior
    .AddInfrastructure(builder.Configuration); // EF Core + Repositories + AuthService + EmailService
```

## Módulos de Injeção de Dependência

```csharp
// Program.cs
builder.Services
    .AddApplication()      // MediatR + FluentValidation + ValidationBehavior
    .AddInfrastructure(builder.Configuration); // EF Core + Repositories + AuthService + EmailService
```

# Testes

Guia para rodar e escrever testes no GaragePro.

## Como Rodar

```bash
# Rodar todos os testes
dotnet test

# Rodar com output detalhado
dotnet test --verbosity normal

# Rodar apenas o projeto de testes
dotnet test GaragePro.UnitTests

# Rodar um teste específico pelo nome
dotnet test --filter "FullyQualifiedName~CreateUserHandler"
```

## Stack de Testes

| Biblioteca | Papel |
|---|---|
| **xUnit** | Framework de testes |
| **Moq** | Mocks de interfaces (repositórios, serviços) |
| **FluentAssertions** | Assertions legíveis (`result.Should().BeTrue()`) |
| **Bogus** | Geração de dados de teste realistas (fake data) |

## Escopo dos Testes

| O que é testado | O que não é testado |
|---|---|
| Lógica dos Handlers (Application) | EF Core / SQL Server (Infrastructure) |
| Regras de domínio nas Entities | Endpoints HTTP (API) |
| Validações do FluentValidation | Serviços externos (email, etc.) |

Os testes são **unitários** — sem banco de dados, sem HTTP. Toda dependência de infraestrutura é mockada com Moq.

## Estrutura de um Teste de Handler

```csharp
public class CreateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly CreateUserHandler _handler;

    public CreateUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new CreateUserHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var command = new CreateUserCommand("Clean Code", "Robert Martin", "978-0132350884", 2008, null);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await new GetUserByIdHandler(_userRepositoryMock.Object)
            .Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}
```

## Uso do Bogus para Dados de Teste

```csharp
// Gerar um livro fake com dados realistas
var faker = new Faker<User>()
    .CustomInstantiator(f => new User(
        f.Lorem.Words(3).Aggregate((a, b) => $"{a} {b}"),
        f.Name.FullName(),
        f.Random.Replace("###-#-##-######-#"),
        f.Random.Int(1950, 2023),
        null));

var user = faker.Generate();
```

## Convenção de Nomenclatura dos Testes

```
{Metodo}_Should{Resultado}_When{Condicao}
```

Exemplos:
- `Handle_ShouldReturnSuccess_WhenUserIsCreated`
- `Handle_ShouldReturnFailure_WhenUserNotFound`
- `Handle_ShouldReturnValidationFailure_WhenTitleIsEmpty`

## Onde Ficam os Testes

```
LibraryManager.UnitTests/
  Domain/          ← testes de entidades e regras de negócio
  Handlers/        ← testes de handlers do Application
    Users/
    Users/
    Loans/
    Reviews/
```

> **Atenção:** O projeto `LibraryManager.UnitTests` referencia apenas `LibraryManager.Application` e `LibraryManager.Core` — nunca `LibraryManager.Infrastructure` ou `LibraryManager.API`.