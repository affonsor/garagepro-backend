# Quickstart: Endereço na Criação de Clientes

**Feature**: 004-fix-user-address-create
**Date**: 2026-04-28

---

## Pré-requisitos

- Backend rodando em `https://localhost:44384` (via `dotnet run` ou Docker)
- Frontend rodando em `http://localhost:4200` (`ng serve` dentro de `frontend/`)
- Credencial de teste com permissão de criação: `admin@garagepro.com` / `Admin@123`

---

## Fluxo de teste — Criação de cliente COM endereço

1. Logar com `admin@garagepro.com`
2. Navegar para **Clientes → Novo Cliente**
3. Verificar que a seção "Endereços" está visível com empty state "Nenhum endereço adicionado"
4. Tentar clicar em **Salvar** sem adicionar endereço → botão deve estar desabilitado
5. Clicar em **+ Adicionar Endereço**
6. Preencher o formulário do dialog:
   - Tipo: Residencial
   - Rua: Rua das Flores
   - Número: 123
   - Bairro: Centro
   - Cidade: São Paulo
   - UF: SP
   - CEP: 01310-100
7. Clicar em **Salvar** no dialog → endereço aparece listado na seção
8. Preencher dados do cliente: Nome: "Cliente Teste" (mínimo necessário)
9. Clicar em **Salvar** → cliente criado com sucesso, redirecionado para lista
10. Abrir o cliente criado → verificar que o endereço está listado na seção de endereços

---

## Fluxo de teste — Tentativa de criar sem endereço

1. Navegar para **Clientes → Novo Cliente**
2. Preencher apenas o nome do cliente
3. Verificar que o botão **Salvar** está desabilitado enquanto não houver endereços
4. Adicionar 1 endereço → botão **Salvar** passa a estar habilitado

---

## Fluxo de teste — Múltiplos endereços na criação

1. Navegar para **Clientes → Novo Cliente**
2. Adicionar 2 endereços via "+ Adicionar Endereço"
3. Tentar excluir um → `ConfirmDialog` → confirmar → endereço removido
4. Tentar excluir o último endereço restante → bloqueado com snackbar 6s

---

## Fluxo de teste — Edição de endereço pendente

1. Navegar para **Clientes → Novo Cliente**
2. Adicionar 1 endereço com cidade "São Paulo"
3. Clicar em **Editar** no endereço pendente → dialog abre pré-preenchido
4. Alterar cidade para "Campinas" e salvar → endereço atualizado na lista

---

## Fluxo de regressão — Edição de cliente existente

1. Abrir um cliente existente em `/clients/:id/edit`
2. Verificar que a seção de endereços continua funcionando como antes
3. Adicionar, editar e remover endereços → comportamento inalterado

---

## Arquivos modificados

| Arquivo | Tipo de mudança |
|---|---|
| `frontend/src/app/features/clients/detail/client-form.page.ts` | Principal: adiciona create-mode address management |
| `frontend/src/app/features/clients/detail/address-form-dialog.component.ts` | Ajuste: suporta modo draft (clientId null → fecha com dados sem chamar API) |
