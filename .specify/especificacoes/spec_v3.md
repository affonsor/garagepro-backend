VII. Frontend — Angular 21 (Standalone Architecture)
O projeto DEVE utilizar Angular 21 com arquitetura baseada em Standalone Components — NgModule é proibido em novos módulos.

Estrutura de Diretórios

src/
  app/
    core/
      auth/           ← guards, interceptors, serviço JWT
      http/           ← clientes HTTP tipados por recurso
      models/         ← interfaces TypeScript espelhando os contratos da API
    shared/
      components/     ← componentes reutilizáveis (botões, tabelas, modais)
      directives/     ← diretivas customizadas
      pipes/          ← pipes de formatação
    features/
      {recurso}/
        {recurso}.routes.ts     ← rotas lazy-loaded do recurso
        list/                   ← página de listagem
        detail/                 ← página de detalhe / form
    layout/           ← shell da aplicação (sidebar, header, footer)
Princípios Arquiteturais
Signals obrigatórios para estado local e derivado. BehaviorSubject é permitido apenas em serviços de escopo global com múltiplos subscribers assíncronos.
Lazy loading OBRIGATÓRIO para todas as rotas de feature. Nenhuma rota de recurso pode ser carregada no bundle inicial.
HTTP Clients tipados: cada recurso da API (/api/clients, /api/vehicles, etc.) DEVE ter um serviço dedicado em core/http/ com métodos retornando Observable<T> fortemente tipados.
Interceptor JWT: todo request DEVE receber o Authorization: Bearer <token> automaticamente via HttpInterceptor — nenhum serviço pode adicionar o header manualmente.
Smart × Dumb components: páginas (rotas) são smart — injetam serviços e gerenciam estado. Componentes reutilizáveis em shared/ são dumb — recebem @Input() e emitem @Output() apenas.
Design System — Material 3
Angular Material 3 é o design system obrigatório. Customizações visuais DEVEM ser feitas via @use '@angular/material' as mat e temas CSS custom properties — nunca sobrescrevendo seletores internos do Material.
Paleta: definir uma paleta de cores primária e secundária no tema global (src/styles/theme.scss). Cores literais (#hex) são proibidas fora do arquivo de tema.
Typography: usar a escala tipográfica do Material 3. Tamanhos de fonte ad-hoc são proibidos.
Responsividade: todos os layouts DEVEM funcionar em viewport mínimo de 360px. Usar CSS Grid e Flexbox — position: absolute para posicionamento de layout é proibido.
Dark mode: suporte DEVE ser implementado via prefers-color-scheme e toggle manual, ambos manipulando o tema Material.
Padrões de Formulários
Formulários DEVEM usar Reactive Forms (FormGroup, FormControl). Template-driven forms são proibidos.
Validações client-side DEVEM espelhar as validações do backend (campos obrigatórios, tamanho máximo, formato). Não substituem a validação da API.
Erros de validação da API (400 Bad Request) DEVEM ser exibidos inline nos campos correspondentes via setErrors() — não apenas em toasts globais.
Feedback e UX
Loading states: toda operação assíncrona DEVE exibir indicador visual (MatProgressSpinner ou skeleton screen). Botões de submit DEVEM ser desabilitados durante chamadas HTTP.
Toasts / Snackbars: usar MatSnackBar para confirmações de sucesso e erros inesperados. Duração padrão: 3s para sucesso, 6s para erro.
Confirmação de exclusão: toda ação destrutiva (DELETE) DEVE abrir um MatDialog de confirmação antes de disparar a requisição.
Paginação: listas DEVEM usar MatPaginator integrado ao sistema de paginação da API (pageNumber, pageSize, totalCount).
Empty states: toda listagem DEVE exibir uma mensagem ilustrada quando não há itens cadastrados.
Integração com MCP Server
O MCP Server expõe as operações da API como ferramentas para agentes de IA. O frontend DEVE consumir a API REST diretamente — nunca via MCP.
O MCP Server é infraestrutura de desenvolvimento e automação; o cliente Angular NÃO deve conhecer ou referenciar sua interface.
Autenticação
O token JWT DEVE ser armazenado em localStorage com chave garagepro_token.
AuthGuard DEVE proteger todas as rotas exceto /login. Expiração do token DEVE redirecionar para /login com o parâmetro ?returnUrl= preservado.
Ao receber 401 da API, o interceptor DEVE limpar o token e redirecionar para /login.
Controle de Acesso por Role
A interface DEVE ocultar / desabilitar ações para as quais o usuário autenticado não tem permissão, espelhando as regras da API:

Role	Users	Clients	Vehicles	Products	Services
Admin	CRUD	CRUD	CRUD+Transfer	CRUD	CRUD
Technician	—	CRUD	CRUD+Transfer	—	—
Financial	—	Read	—	CRUD	CRUD
Ocultar botões/ações não autororizados é UX; a segurança real é imposta pelo backend.

Padrões de Qualidade
Todo PR de frontend DEVE ser verificado contra estes princípios antes do merge.
Proibido: any explícito em TypeScript, console.log em código commitado, acesso direto ao DOM via document.querySelector.
Obrigatório: strictNullChecks ativo, lint com ESLint + @angular-eslint, formatação com Prettier.
Documentação e comentários de código em português (pt-br).