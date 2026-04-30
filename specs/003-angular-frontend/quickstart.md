# Quickstart: GaragePro Angular Frontend

**Feature**: 003-angular-frontend
**Date**: 2026-04-28

---

## Pré-requisitos

| Ferramenta | Versão mínima | Verificação |
|------------|---------------|-------------|
| Node.js | 22 LTS | `node --version` |
| npm | 10 | `npm --version` |
| Angular CLI | 21 | `ng version` |
| Docker (opcional) | 24 | backend via `docker-compose` |

---

## Inicializar o projeto Angular

A pasta `frontend/` ainda não existe — o primeiro passo é criá-la com Angular CLI:

```bash
# na raiz do repositório
ng new garagepro-web \
  --directory frontend \
  --style scss \
  --routing true \
  --standalone true \
  --strict true \
  --package-manager npm

cd frontend
```

Instalar Angular Material 3:

```bash
ng add @angular/material
# escolher: tema customizado, tipografia global, animações habilitadas
```

Instalar dependências adicionais:

```bash
npm install --save-dev jest jest-preset-angular @angular-builders/jest @types/jest
npm install --save-dev eslint @angular-eslint/eslint-plugin prettier
```

---

## Variáveis de ambiente

Editar `frontend/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:44384/api',
};
```

Para produção (`environment.prod.ts`), substituir com URL do servidor.

---

## Subir o backend (pré-requisito do frontend)

Na raiz do repositório:

```bash
docker-compose up -d
```

Ou via .NET CLI:

```bash
dotnet run --project src/GaragePro.API
```

A API ficará disponível em `http://localhost:44384`. Swagger em `https://localhost:44384/swagger`.

---

## Rodar o frontend em desenvolvimento

```bash
cd frontend
ng serve
# abre em http://localhost:4200
```

---

## Credenciais de teste (seed do banco)

| E-mail | Senha | Role |
|--------|-------|------|
| `admin@garagepro.com` | `Admin@123` | Admin |
| `tech@garagepro.com` | `Tech@123` | Technician |
| `fin@garagepro.com` | `Fin@123` | Financial |

> As credenciais acima dependem do seed de dados do backend. Verificar `docker-compose.yml` ou migration seed se os logins falharem.

---

## Rodar os testes unitários

```bash
cd frontend
npm test
# ou em modo watch:
npm test -- --watch
```

---

## Build de produção

```bash
cd frontend
ng build --configuration production
# output em frontend/dist/garagepro-web/
```

---

## Estrutura de diretórios após scaffold

```
frontend/
├── src/
│   ├── main.ts                     ← bootstrapApplication()
│   ├── index.html
│   ├── styles.scss
│   ├── styles/
│   │   └── theme.scss              ← Material 3 tema
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── app/
│       ├── app.component.ts
│       ├── app.config.ts           ← providers globais
│       ├── app.routes.ts           ← rotas lazy
│       ├── core/
│       │   ├── auth/               ← service, guards, interceptors
│       │   ├── http/               ← serviços por recurso
│       │   └── models/             ← interfaces TypeScript
│       ├── shared/
│       │   ├── components/         ← confirm-dialog, empty-state, etc.
│       │   ├── directives/
│       │   └── pipes/
│       ├── layout/                 ← shell, sidebar, header
│       └── features/
│           ├── auth/login/
│           ├── users/
│           ├── clients/
│           ├── vehicles/
│           ├── products/
│           └── services/
├── angular.json
├── tsconfig.json
├── jest.config.ts
├── .eslintrc.json
└── .prettierrc
```

---

## Problemas comuns

**CORS ao chamar o backend local**

Verificar se a API está configurada com `app.UseCors(...)` para `http://localhost:4200`. Em development o `appsettings.Development.json` deve ter:

```json
"AllowedOrigins": ["http://localhost:4200"]
```

**Token expirado ao recarregar a página**

O interceptor redireciona para `/login` automaticamente. Logar novamente — token JWT tem duração definida no backend (`expiresAt` da resposta de login).

**`ng serve` na porta 4200 já em uso**

```bash
ng serve --port 4201
```

Atualizar a origem permitida no backend e a configuração de CORS correspondente.
