# Feature Specification: GaragePro Core CRUD API

**Feature Branch**: `002-core-crud-api`  
**Created**: 2026-04-27  
**Status**: Draft  
**Input**: User description: "@.specify/especificacoes/spec_v2.md"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - User Authentication & Role-Based Access (Priority: P1)

A system administrator registers users (mechanics, financial staff, admins) and assigns them one or more roles. Each user authenticates with their credentials to receive an access token. The system enforces role-based access so that users can only perform operations permitted by their role(s).

**Why this priority**: Security and access control are the foundation of any multi-user system. Without authenticated access, none of the other features can be safely exposed.

**Independent Test**: Independently testable by creating a user with specific roles, authenticating, and verifying that protected resources are accessible or denied based on role.

**Acceptance Scenarios**:

1. **Given** an admin is logged in, **When** they create a new user with the "technician" role, **Then** the user is saved and can log in with the assigned role
2. **Given** a user with the "financial" role, **When** they attempt to access an admin-only resource, **Then** the system denies the request with an appropriate error response
3. **Given** a registered user, **When** they submit valid credentials, **Then** they receive a time-limited access token
4. **Given** a user with multiple roles (e.g., admin + technician), **When** they access resources restricted to either role, **Then** access is granted

---

### User Story 2 - Client Registration & Management (Priority: P1)

Staff registers clients who bring their vehicles for service. Each client can have multiple contact addresses (residential, billing, etc.) and multiple vehicles linked to their profile. Staff can update or remove client records as needed.

**Why this priority**: Clients are the central entity of a garage management system. All service work is tied to a client and their vehicle.

**Independent Test**: Independently testable by creating a client, adding multiple addresses and vehicles, and verifying all data is correctly stored and retrievable.

**Acceptance Scenarios**:

1. **Given** a staff member is logged in, **When** they create a new client with basic contact information, **Then** the client is saved and appears in the client list
2. **Given** an existing client, **When** a staff member adds a second address, **Then** both addresses are linked to the client and retrievable independently
3. **Given** an existing client, **When** a staff member registers a vehicle under that client, **Then** the vehicle is linked to the client and visible in the client's vehicle list
4. **Given** an existing client, **When** a staff member updates or removes the client's information, **Then** changes are persisted correctly

---

### User Story 3 - Vehicle Registration & Transfer (Priority: P2)

A vehicle is registered under a client. If the vehicle changes ownership, it can be transferred to another existing client while preserving the full service history associated with the vehicle.

**Why this priority**: Vehicle transfer is unique to the domain and ensures service history continuity, which is a key differentiator for workshop management.

**Independent Test**: Independently testable by registering a vehicle under Client A, transferring it to Client B, and verifying that the service history is preserved and the vehicle now appears under Client B.

**Acceptance Scenarios**:

1. **Given** a vehicle registered under Client A, **When** a staff member initiates a transfer to Client B, **Then** the vehicle is now listed under Client B
2. **Given** a transferred vehicle, **When** a staff member views the vehicle's history, **Then** all previous records (under Client A) remain visible
3. **Given** a vehicle transfer attempt to a non-existent client, **When** the operation is submitted, **Then** the system rejects the transfer with a clear error message
4. **Given** a completed transfer, **When** a staff member reviews the transfer log, **Then** the previous and new owner, and the transfer date, are recorded

---

### User Story 4 - Product Catalog Management (Priority: P2)

Staff manages a catalog of products (parts, materials) used in vehicle servicing. Products can be created, viewed, updated, and removed from the catalog.

**Why this priority**: A product catalog is a prerequisite for quoting and billing parts used in services.

**Independent Test**: Independently testable by creating, listing, updating, and deleting products in the catalog.

**Acceptance Scenarios**:

1. **Given** a staff member is logged in, **When** they create a product with a name, description, and price, **Then** the product appears in the catalog
2. **Given** an existing product, **When** its price is updated, **Then** the updated price is reflected in the catalog
3. **Given** an existing product, **When** it is deleted, **Then** it no longer appears in the catalog

---

### User Story 5 - Service Catalog Management (Priority: P2)

Staff manages a catalog of services offered by the garage (e.g., oil change, brake inspection). Services can be created, viewed, updated, and removed.

**Why this priority**: The service catalog defines the billable services the garage offers and is a prerequisite for work order functionality in future versions.

**Independent Test**: Independently testable by creating, listing, updating, and deleting services in the catalog.

**Acceptance Scenarios**:

1. **Given** a staff member is logged in, **When** they create a service with a name, description, and price, **Then** the service appears in the catalog
2. **Given** an existing service, **When** its price is updated, **Then** the updated price is reflected
3. **Given** an existing service, **When** it is deleted, **Then** it no longer appears in the catalog

---

### Edge Cases

- What happens when a client with linked vehicles is deleted?
- How does the system handle a duplicate license plate when registering a vehicle?
- What happens when a transfer is attempted for a vehicle that does not exist?
- How does the system respond when a user with no roles attempts to access a protected resource?
- What happens when a product or service that may be referenced in future work orders is deleted? (deferred — work orders are out of scope for this version)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authorized users to create, read, update, and delete User accounts
- **FR-002**: System MUST support assigning one or more roles (admin, technician, financial) to each User; a user without any role MUST NOT be permitted
- **FR-003**: System MUST authenticate users via email and password and issue a time-limited access token upon successful authentication
- **FR-004**: System MUST enforce access control based on user roles on all protected operations
- **FR-005**: System MUST allow authorized users to create, read, update, and delete Client records
- **FR-006**: Each Client MUST support multiple addresses (e.g., residential, billing), with at least one address required
- **FR-007**: Each Client MUST support zero or more associated Vehicles
- **FR-008**: System MUST allow authorized users to create, read, update, and delete Vehicles
- **FR-009**: Vehicle license plates MUST be unique within the system
- **FR-010**: System MUST allow transferring a Vehicle to a different Client, preserving the full service history
- **FR-011**: System MUST record a transfer log entry each time a Vehicle is transferred, including previous owner, new owner, and timestamp
- **FR-012**: System MUST allow authorized users to create, read, update, and delete Products in the catalog
- **FR-013**: System MUST allow authorized users to create, read, update, and delete Services in the catalog
- **FR-014**: System MUST enforce the following role permission matrix:
  - **Admin**: full create/read/update/delete access to all entities (Users, Clients, Addresses, Vehicles, Products, Services)
  - **Technician**: full create/read/update/delete access to Clients, Addresses, and Vehicles; no access to Users, Products, or Services management
  - **Financial**: full create/read/update/delete access to Products and Services; read-only access to Clients; no access to Users or Vehicles management

### Key Entities *(include if feature involves data)*

- **User**: A system operator with credentials and one or more assigned roles
- **Role**: A permission group (admin, technician, financial) that defines what operations a user may perform
- **Client**: A customer who brings vehicles to the garage; has contact information and one or more addresses
- **Address**: A contact address linked to a Client; categorized by type (e.g., residential, billing)
- **Vehicle**: A vehicle linked to a Client; uniquely identified by license plate; carries a service history across ownership changes
- **VehicleTransferRecord**: An audit entry recording a Vehicle ownership change, including previous owner, new owner, and transfer date
- **Product**: A catalog item representing a part or material, with name, description, and price
- **Service**: A catalog item representing a billable workshop service, with name, description, and price

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All CRUD operations for Users, Clients, Vehicles, Products, and Services complete in under 2 seconds under normal operating conditions
- **SC-002**: A vehicle transfer correctly preserves 100% of associated history entries, accessible under the new client
- **SC-003**: All unauthorized access attempts are rejected with no data exposure
- **SC-004**: All API endpoints return structured, descriptive error messages for invalid or missing input, enabling consuming applications to surface meaningful feedback to users
- **SC-005**: A client's full profile, including all addresses and vehicles, is retrievable in a single request
- **SC-006**: Duplicate license plate registrations are rejected 100% of the time with a clear conflict message

## Clarifications

### Session 2026-04-27

- Q: Which operations each role (admin, technician, financial) is permitted to perform? → A: Admin: full access to all entities; Technician: full access to Clients and Vehicles; Financial: full access to Products and Services, read-only on Clients

## Assumptions

- This is the foundational version of the system; work orders, billing, and scheduling are out of scope for this release
- Product stock and inventory management are out of scope; Products represent a catalog only
- Service items represent a service catalog, not service execution or work orders
- All API consumers are authenticated client applications; no public or anonymous endpoints exist beyond the authentication endpoint
- Email is used as the unique login identifier for users
- Hard delete is used by default for all entities in this version; soft delete strategy is deferred to a future release
- Pagination is expected on list endpoints; specific page size defaults are deferred to the planning phase
- The technical stack is predetermined and must be respected during implementation: .NET 10 with C# 14, ASP.NET Core 10 Web API, MediatR (CQRS pattern), FluentValidation, Entity Framework Core with PostgreSQL, and JWT Bearer with BCrypt for password hashing
