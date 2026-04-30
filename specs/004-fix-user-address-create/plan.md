# Implementation Plan: Endereço na Criação de Clientes

**Branch**: `005-fix-user-address-create` | **Date**: 2026-04-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-fix-user-address-create/spec.md`

## Summary

Correção do formulário de criação de clientes (`client-form.page.ts`) para exibir e gerenciar endereços em modo de criação ("new"), espelhando o comportamento já existente em modo de edição ("edit"). A principal diferença técnica é que, no modo de criação, os endereços são acumulados em memória (array local de `CreateAddressInput`) e enviados junto com o corpo da requisição `POST /api/clients`, enquanto no modo de edição os endereços são persistidos individualmente via `AddressesService`. Nenhuma alteração no backend é necessária — o endpoint `POST /api/clients` já aceita um array de endereços.

## Technical Context

**Language/Version**: TypeScript 5.6 / Angular 21 (Standalone APIs)
**Primary Dependencies**: Angular Material 3, RxJS 7.8, Reactive Forms
**Storage**: Estado de criação de endereços em memória (Signal `draftAddresses`); persistência via `POST /api/clients` ao salvar
**Testing**: Jest 29 + `jest-preset-angular` (já configurado)
**Target Platform**: Navegadores desktop modernos (mesma plataforma do frontend existente)
**Project Type**: Bugfix em SPA Angular existente (frontend GaragePro)
**Performance Goals**: Sem degradação de performance — operação puramente local até o submit
**Constraints**: Sem acesso ao backend durante a criação de endereços (sem client ID disponível); ao menos 1 endereço obrigatório no submit
**Scale/Scope**: 1 componente modificado (`client-form.page.ts`), 1 componente reutilizado (`AddressesSectionComponent` ou dialog diretamente), lógica de modo create vs. edit expandida

## Constitution Check

*GATE: Verificado antes do Phase 0 research. Revalidado após Phase 1 design.*

### I. Clean Architecture — Separação de Camadas
✅ **APROVADO** — Nenhuma alteração no backend. O frontend consome exclusivamente a API REST existente sem bypasses de camada.

### II. CQRS com MediatR
✅ **APROVADO** — Aplicável ao backend; nenhuma mudança nos Commands/Queries existentes.

### III. Result Pattern — Retorno Padronizado
✅ **APROVADO** — O frontend já consome `{ error, errors[] }` do endpoint `POST /api/clients`. Erros de validação do backend (400) continuarão sendo exibidos via `setErrors()`.

### IV. Pipeline de Validação via ValidationBehavior
✅ **APROVADO** — Validação client-side de "ao menos 1 endereço" é adicionada ao formulário. As regras de campo do endereço espelham os validators do backend já existentes.

### V. Repository Pattern — Abstração de Persistência
✅ **APROVADO** — Nenhuma alteração de repositórios. O `ClientsService` e `AddressesService` já abstraem o acesso à API.

### VI. Disciplina de Testes Unitários
✅ **APROVADO** — Testes unitários do `client-form.page.ts` (se existentes) precisarão ser atualizados para cobrir o fluxo de criação com endereços. Sem testes de banco de dados ou HTTP real.

### VII. Frontend — Angular 21 (Standalone Architecture)
✅ **APROVADO** — Todas as restrições do princípio VII são respeitadas:
- Standalone Components sem NgModule ✓
- Signals para `draftAddresses` (estado local de endereços em criação) ✓
- Reactive Forms para validação do formulário de cliente ✓
- `MatDialog` para o formulário de endereço ✓
- `MatSnackBar` para feedback (3s sucesso / 6s erro) ✓
- Sem `any`, sem `console.log`, `strict: true` ✓
- Reutilização de `AddressFormDialogComponent` já existente ✓

**Resultado: Nenhuma violação detectada. Sem bloqueios.**

## Project Structure

### Documentation (this feature)

```text
specs/004-fix-user-address-create/
├── plan.md              ← Este arquivo (/speckit-plan)
├── research.md          ← Phase 0 output (/speckit-plan)
├── data-model.md        ← Phase 1 output (/speckit-plan)
├── quickstart.md        ← Phase 1 output (/speckit-plan)
├── contracts/
│   └── ui-contracts.md  ← Contrato de tela do formulário de criação
├── checklists/
│   └── requirements.md
└── tasks.md             ← Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
frontend/src/app/features/clients/
├── clients.routes.ts           ← sem alteração
├── list/
│   └── clients-list.page.ts    ← sem alteração
└── detail/
    ├── client-form.page.ts     ← MODIFICADO: adicionar suporte a endereços em create mode
    ├── addresses-section.component.ts  ← sem alteração (já suporta edição)
    └── address-form-dialog.component.ts ← sem alteração (reutilizado em create mode)

frontend/src/app/core/
├── http/
│   ├── clients.service.ts      ← sem alteração
│   └── addresses.service.ts    ← sem alteração
└── models/
    ├── client.model.ts         ← possível adição de tipo auxiliar local
    └── address.model.ts        ← sem alteração
```

**Structure Decision**: Bugfix isolado no `client-form.page.ts`. Todos os componentes de suporte (`AddressFormDialogComponent`, `AddressesSectionComponent`, `AddressesService`) já existem e são reutilizados sem modificação. A lógica nova é introduzida exclusivamente no formulário de criação de clientes.

## Complexity Tracking

> Nenhuma violação da Constituição detectada. Seção não aplicável.
