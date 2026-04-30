<!--
SYNC IMPACT REPORT
==================
Version change: 1.1.0 → 1.2.0
Updated based on .specify/especificacoes/spec_v3.md (seção Frontend Angular 21 adicionada)

Added sections:
- VII. Frontend — Angular 21 (Standalone Architecture) (novo princípio)

Modified sections:
- Padrões de Qualidade e Revisão (referência ao princípio VII adicionada)
- Governance (referência à spec_v3.md adicionada)

Templates:
- .specify/templates/spec-template.md    ✅ compatível — nenhuma alteração necessária
- .specify/templates/plan-template.md    ✅ compatível — Constitution Check genérico já presente
- .specify/templates/tasks-template.md   ✅ compatível — estrutura por fases alinhada

Deferred TODOs: nenhum
-->

# GaragePro API — Constituição

## Princípios Fundamentais

### I. Clean Architecture — Separação de Camadas

O projeto DEVE respeitar a hierarquia de dependência: **API → Application → Core ← Infrastructure**.

- **Core** não importa nenhum pacote externo. Contém Entities, Interfaces e Common.
- **Application** depende exclusivamente do Core. Implementa casos de uso via Features.
- **Infrastructure** implementa as interfaces definidas no Core. Nunca é referenciada pela Application diretamente.
- **API** orquestra tudo via injeção de dependência — não contém lógica de negócio.

Toda violação desta hierarquia DEVE ser bloqueada em code review.

### II. CQRS com MediatR

Operações DEVEM ser segregadas entre Commands (escrita) e Queries (leitura).

- Commands implementam `IRequest<Result<T>>` e modificam estado.
- Queries lêem estado sem efeitos colaterais.
- Cada Command ou Query DEVE ter exatamente um Handler dedicado.
- Handlers NUNCA são compartilhados entre Commands e Queries.
- A estrutura de diretórios DEVE seguir `Features/{Recurso}/{Acao}/`.

### III. Result Pattern — Retorno Padronizado

Todos os Handlers DEVEM retornar `Result<T>`. Nenhuma exceção de domínio pode
vazar além do `GlobalExceptionHandler`.

- Leitura sem resultado: `Results.NotFound(new { result.Error })`
- Escrita bem-sucedida: `Results.Created(...)` com `{ id = result.Data }`
- Falha de validação ou negócio: `Results.BadRequest(new { result.Error, result.Errors })`

Endpoints DEVEM verificar `result.IsSuccess` antes de mapear para o HTTP status code.

### IV. Pipeline de Validação via ValidationBehavior

Toda validação de entrada DEVE ocorrer via `ValidationBehavior<TRequest, TResponse>`
no pipeline do MediatR — nunca dentro do Handler manualmente.

- Cada Command ou Query que exige validação DEVE ter um `IValidator<TRequest>` registrado.
- O `ValidationBehavior` retorna `Result.ValidationFailure(errors)` automaticamente quando
  a validação falha, antes de chegar ao Handler.
- Handlers NUNCA duplicam regras de validação já cobertas pelo Validator.

### V. Repository Pattern — Abstração de Persistência

Repositórios DEVEM ser definidos como interfaces no Core e implementados exclusivamente
na Infrastructure.

- Handlers dependem apenas das interfaces — NUNCA de `DbContext` ou EF Core diretamente.
- Nenhuma query LINQ ou acesso a banco de dados pode aparecer na camada Application.
- Implementações concretas (`AppDbContext`, `Configurations/`, `Repositories/`) pertencem
  à Infrastructure.

### VI. Disciplina de Testes Unitários

Os testes DEVEM ser **unitários** — sem banco de dados, sem HTTP, sem serviços externos.

- Stack obrigatório: **xUnit** (framework), **Moq** (mocks), **FluentAssertions** (assertions),
  **Bogus** (geração de dados de teste).
- O que DEVE ser testado: lógica dos Handlers (Application), regras de domínio nas Entities (Core),
  validações do FluentValidation.
- O que NÃO é testado diretamente: EF Core / SQL Server, endpoints HTTP, serviços externos (email, etc.).
- Toda dependência de infraestrutura DEVE ser mockada com Moq — nunca instanciada concretamente.
- O projeto `GaragePro.UnitTests` DEVE referenciar apenas `GaragePro.Application` e `GaragePro.Core`
  — NUNCA `GaragePro.Infrastructure` ou `GaragePro.API`.
- Convenção de nomenclatura obrigatória: `{Metodo}_Should{Resultado}_When{Condicao}`.
  Exemplo: `Handle_ShouldReturnSuccess_WhenUserIsCreated`.
- Estrutura de diretórios do projeto de testes:

```
GaragePro.UnitTests/
  Domain/     ← testes de entidades e regras de negócio
  Handlers/   ← testes de handlers do Application
```

### VII. Frontend — Angular 21 (Standalone Architecture)

O frontend DEVE utilizar **Angular 21** com arquitetura baseada em **Standalone Components**.
NgModule é proibido em novos módulos ou features.

**Estrutura de diretórios obrigatória**:

```
src/app/
  core/
    auth/           ← guards, interceptors, serviço JWT
    http/           ← clientes HTTP tipados por recurso
    models/         ← interfaces TypeScript espelhando os contratos da API
  shared/
    components/     ← componentes reutilizáveis (dumb)
    directives/
    pipes/
  features/
    {recurso}/
      {recurso}.routes.ts   ← rotas lazy-loaded
      list/
      detail/
  layout/           ← shell da aplicação (sidebar, header, footer)
```

**Gerenciamento de estado**:
- **Signals** DEVEM ser usados para estado local e estado derivado.
- `BehaviorSubject` é permitido APENAS em serviços de escopo global com múltiplos
  subscribers assíncronos.

**Roteamento**:
- **Lazy loading é obrigatório** para todas as rotas de feature. Nenhuma rota de recurso
  pode ser incluída no bundle inicial.

**HTTP Clients**:
- Cada recurso da API (`/api/clients`, `/api/vehicles`, etc.) DEVE ter um serviço dedicado
  em `core/http/` com métodos retornando `Observable<T>` fortemente tipados.
- O `HttpInterceptor` JWT DEVE adicionar `Authorization: Bearer <token>` automaticamente
  em todos os requests — nenhum serviço pode adicionar o header manualmente.

**Smart × Dumb components**:
- Páginas (rotas) são **smart**: injetam serviços e gerenciam estado.
- Componentes em `shared/` são **dumb**: recebem `@Input()` e emitem `@Output()` apenas.

**Design System — Angular Material 3**:
- Angular Material 3 é o design system obrigatório. Customizações DEVEM ser feitas via
  `@use '@angular/material' as mat` e CSS custom properties — nunca sobrescrevendo
  seletores internos do Material.
- Cores literais (`#hex`) são proibidas fora do arquivo de tema (`src/styles/theme.scss`).
- Tamanhos de fonte ad-hoc são proibidos; usar a escala tipográfica do Material 3.
- Todos os layouts DEVEM funcionar em viewport mínimo de 360px (CSS Grid/Flexbox).
- `position: absolute` para posicionamento de layout é proibido.
- Dark mode DEVE ser implementado via `prefers-color-scheme` e toggle manual.

**Formulários**:
- DEVEM usar **Reactive Forms** (`FormGroup`, `FormControl`). Template-driven forms são
  proibidos.
- Validações client-side DEVEM espelhar as validações do backend.
- Erros de validação da API (400) DEVEM ser exibidos inline via `setErrors()` — não
  apenas em toasts globais.

**Feedback e UX**:
- Toda operação assíncrona DEVE exibir indicador visual (`MatProgressSpinner` ou
  skeleton screen). Botões de submit DEVEM ser desabilitados durante chamadas HTTP.
- Usar `MatSnackBar` para confirmações: 3s para sucesso, 6s para erro.
- Toda ação destrutiva (DELETE) DEVE abrir um `MatDialog` de confirmação.
- Listas DEVEM usar `MatPaginator` integrado ao sistema de paginação da API
  (`pageNumber`, `pageSize`, `totalCount`).
- Toda listagem DEVE exibir mensagem ilustrada quando não há itens cadastrados.

**Autenticação e Controle de Acesso**:
- O token JWT DEVE ser armazenado em `localStorage` com chave `garagepro_token`.
- `AuthGuard` DEVE proteger todas as rotas exceto `/login`. Expiração do token DEVE
  redirecionar para `/login?returnUrl=`.
- Ao receber `401` da API, o interceptor DEVE limpar o token e redirecionar para `/login`.
- A interface DEVE ocultar/desabilitar ações para as quais o usuário não tem permissão,
  espelhando as regras da API (Admin / Technician / Financial).

**Padrões de Qualidade**:
- Proibido: `any` explícito em TypeScript, `console.log` em código commitado,
  acesso direto ao DOM via `document.querySelector`.
- Obrigatório: `strictNullChecks` ativo, lint com ESLint + `@angular-eslint`,
  formatação com Prettier.

## Estrutura de Features e Injeção de Dependência

Toda feature DEVE seguir a estrutura de diretórios abaixo, com um arquivo por responsabilidade:

```
Features/
  {Recurso}/
    {Acao}/
      {Acao}Command.cs    ← dados de entrada (record)
      {Acao}Validator.cs  ← regras FluentValidation (quando aplicável)
      {Acao}Handler.cs    ← lógica do caso de uso
```

A composição da aplicação DEVE ser registrada exclusivamente via:

```csharp
builder.Services
    .AddApplication()                        // MediatR + FluentValidation + ValidationBehavior
    .AddInfrastructure(builder.Configuration); // EF Core + Repositories + AuthService + EmailService
```

Instanciação manual de serviços fora do contêiner de DI é proibida.

## Padrões de Qualidade e Revisão

- Todo PR DEVE ser verificado contra os sete princípios acima antes do merge.
- Violações da regra de dependência DEVEM ser justificadas por escrito e aprovadas
  explicitamente antes de serem aceitas.
- O `GlobalExceptionHandler` é a única barreira de exceções não tratadas; exceções
  de domínio DEVEM ser convertidas em `Result` antes de atingir a API.
- Documentação e comentários de código sempre em **português (pt-br)**.

## Governance

Esta constituição substitui todas as outras diretrizes de código e arquitetura do projeto.
Alterações exigem:

1. Atualização coordenada de `.specify/especificacoes/spec_v3.md` e deste arquivo.
2. Justificativa documentada para remoção ou redefinição de princípio (bump MAJOR).
3. Code review explícito de todos os arquivos afetados pela mudança.

**Política de versionamento**:
- MAJOR: remoção ou redefinição incompatível de princípio.
- MINOR: adição de novo princípio ou expansão material de seção.
- PATCH: clarificações, correções de redação, ajustes não semânticos.

Use `CLAUDE.md` para orientação de desenvolvimento em tempo de execução.

**Version**: 1.2.0 | **Ratified**: 2026-04-27 | **Last Amended**: 2026-04-28
