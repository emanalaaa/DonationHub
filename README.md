# DonationHub

A modern, high-performance Donation and Campaign Management Web API built with **.NET 10**, **Entity Framework Core**, **Clean Architecture**, and **Domain-Driven Design (DDD)** principles.

---

## Table of Contents
1. [Overview](#overview)
2. [Solution Architecture](#solution-architecture)
3. [Prerequisites](#prerequisites)
4. [Setup & Configuration](#setup--configuration)
5. [User Secrets Configuration](#user-secrets-configuration)
6. [Database Migrations](#database-migrations)
7. [Running the Application](#running-the-application)
8. [API Documentation (Scalar & OpenAPI)](#api-documentation-scalar--openapi)
9. [Architecture & Design Decisions](#architecture--design-decisions)

---

## Overview

DonationHub provides a secure, auditable, and scalable platform for managing charitable donation campaigns, donor contributions, and administrative operations. The solution is built strictly on clean architecture guidelines and vertical slices for features, adhering to modern enterprise patterns.

### Key Capabilities
- **Domain-Driven Design**: Pure domain model featuring value objects (`Money`), domain entities, and explicit state invariants.
- **Identity & Authentication**: Custom JWT authentication and refresh token rotation with PBKDF2 password hashing (no dependency on bloated identity tables).
- **Audit & Soft Deletion**: Transparent soft deletion via EF Core `SaveChangesInterceptor` and global query filters.
- **CQRS Read/Write Separation**: Write operations guarded by Repository/Unit of Work; high-throughput reads optimized with direct projections and `AsNoTracking()`.
- **Modern API Documentation**: Built-in .NET 10 OpenAPI integrated natively with **Scalar** (`/scalar/v1`) and Bearer token authorization.

---

## Solution Architecture

The solution uses a 4-project structure placed directly at the repository root:

```text
DonationHub/
├── DonationHub.Domain/            # Pure domain layer (Entities, Value Objects, Enums, Interfaces)
├── DonationHub.Application/       # Application logic, CQRS vertical slices, DTOs, Handlers
├── DonationHub.Infrastructure/    # Persistence (EF Core, SQL Server), Identity, Interceptors
├── DonationHub.Api/               # ASP.NET Core Minimal API endpoints, Scalar docs, Program.cs
└── DonationHub.slnx               # Modern XML-based .NET Solution file
```

### Dependency Flow (Inside-Out)
- `Domain`: Has zero external dependencies or framework packages.
- `Application`: References only `Domain`. Contains application abstractions and feature slices.
- `Infrastructure`: References `Application` and `Domain`. Implements persistence, repositories, and services.
- `Api`: References `Infrastructure` and `Application`. Acts as the presentation layer and composition root.

---

## Prerequisites

Before running the project, ensure you have installed:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/) (SQL Server 2022, LocalDB, or Azure SQL)
- [.NET EF Core CLI Tools](https://learn.microsoft.com/ef/core/cli/dotnet):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Setup & Configuration

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-org/DonationHub.git
   cd DonationHub
   ```

2. **Restore dependencies & build:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Configure `appsettings.json`:**
   Inspect `DonationHub.Api/appsettings.json` for base settings:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DonationHubDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "Jwt": {
       "Issuer": "DonationHub",
       "Audience": "DonationHubClient",
       "ExpiryMinutes": 60,
       "RefreshTokenExpiryDays": 7
     },
     "SeedAdmin": {
       "Email": "admin@donationhub.org",
       "FullName": "System Administrator"
     },
     "Donation": {
       "MinAmount": 5.00
     }
   }
   ```

---

## User Secrets Configuration

To adhere to security best practices, sensitive values (such as database credentials, JWT secret keys, and seed passwords) must never be stored in plaintext configuration files. Use .NET User Secrets in development:

```bash
# Set connection string (if using custom SQL server instance)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=DonationHubDb;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True" --project DonationHub.Api

# Set the JWT Secret Key (MUST be at least 32 characters long)
dotnet user-secrets set "Jwt:SecretKey" "donationhub_super_secret_signing_key_2026_at_least_32_chars!" --project DonationHub.Api

# Set the Initial Seed Administrator Password
dotnet user-secrets set "SeedAdmin:Password" "Admin@Secure2026!" --project DonationHub.Api
```

---

## Database Migrations

Apply the database schema migrations to your SQL Server instance using `dotnet-ef`:

```bash
dotnet ef database update --project DonationHub.Infrastructure --startup-project DonationHub.Api
```

### Applied Migrations
1. `InitialCreate`: Core entities (Users, RefreshTokens, Categories, Campaigns, Donations, PaymentAttempts, ProcessedStripeEvents, IdempotencyRecords).
2. `AddSoftDeleteSupport`: Added `DeletedAtUtc` column and soft delete filtering to `Campaigns`.
3. `AddIdentityFieldsToUser`: Added `PasswordHash`, `Role`, and `IsActive` fields to `ApplicationUser`.
4. `UpdateCategoryIndexesAndLengths`: Unique indexes and constraints for `Category` (`NameAr`, `NameEn`).

---

## Running the Application

Start the API project:

```bash
dotnet run --project DonationHub.Api
```

On startup in `Development` mode, the application:
1. Validates strongly-typed options (`JwtOptions`, `SeedAdminOptions`, `DonationOptions`).
2. Runs the `DatabaseSeeder`, creating the initial Administrator account if it does not exist.
3. Exposes the OpenAPI specification and Scalar documentation.

---

## API Documentation (Scalar & OpenAPI)

The application utilizes .NET 10 built-in OpenAPI coupled with **Scalar** for an interactive, modern API client interface.

### Endpoints
- **Scalar UI**: `https://localhost:<port>/scalar/v1`
- **OpenAPI JSON Spec**: `https://localhost:<port>/openapi/v1.json`

> [!NOTE]
> OpenAPI and Scalar endpoints are guarded by `app.Environment.IsDevelopment()` and are only accessible in the development environment.

### Authorizing via JWT Bearer in Scalar
1. Open `https://localhost:<port>/scalar/v1` in your browser.
2. Under the **Auth** tag, call `POST /api/v1/auth/login` with your admin credentials:
   ```json
   {
     "email": "admin@donationhub.org",
     "password": "Admin@Secure2026!"
   }
   ```
3. Copy the `accessToken` from the response.
4. Click the **Authorize** button in the Scalar UI header/client.
5. Select **Bearer** authentication and paste your token.
6. All protected routes (e.g., `POST /api/v1/categories`, `POST /api/v1/admin/staff`, `GET /api/v1/auth/me`) are now unlocked for testing.

---

## Architecture & Design Decisions

### 1. CQRS Read/Write Separation Rationale
The application enforces a clear architectural boundary between commands (writes) and queries (reads):

```
                     ┌──────────────────────────────────────────────┐
                     │               HTTP Request                   │
                     └───────┬──────────────────────────────┬───────┘
                             │                              │
                    [Write / Command]                [Read / Query]
                             │                              │
                             ▼                              ▼
                 ┌───────────────────────┐      ┌───────────────────────┐
                 │  IGenericRepository<T>│      │     AppDbContext      │
                 │     & IUnitOfWork     │      │(Direct Projections)   │
                 └───────────┬───────────┘      └───────────┬───────────┘
                             │                              │
                     Change Tracking               .AsNoTracking()
                   Invariant Validation             Fast SQL Projection
                             │                              │
                             ▼                              ▼
                 ┌──────────────────────────────────────────────────────┐
                 │                 SQL Server Database                  │
                 └──────────────────────────────────────────────────────┘
```

#### Why Writes Use `GenericRepository` & `UnitOfWork`
- **Invariant Enforcement**: Write operations must maintain domain consistency and satisfy business rules inside Aggregate Roots before persisting state.
- **Change Tracker Integration**: The generic repository leverages EF Core's change tracking mechanism to detect state transitions safely.
- **Transactional Atomicity**: `IUnitOfWork.CompleteAsync()` ensures that multiple aggregate changes or related writes commit within a single, atomic database transaction.
- **Encapsulated Data Modification**: Restricting write operations to minimal primitives (`GetByIdAsync`, `AddAsync`, `Update`, `Delete`) prevents arbitrary modifications and guarantees reproducible state mutations.

#### Why Reads Bypass the Repository
- **Maximum Throughput & Zero Tracking Overhead**: Read queries directly use `AppDbContext` (or `IApplicationDbContext`) with `.AsNoTracking()`. This eliminates the memory allocation and CPU overhead of EF Core's Change Tracker.
- **Optimized SQL Projections**: Read handlers project directly into clean, purpose-built DTOs (`.Select(c => new CategoryDto(...))`), ensuring only the required SQL columns are selected over the network.
- **Avoidance of Repository Bloat**: In traditional architectures, generic repositories degrade into an anti-pattern as teams append dozens of specialized methods (`GetWithCampaignsAndDonationsAsync`, `GetActiveOnlyAsync`, etc.). Direct database querying cleanly isolates read queries to their respective vertical slices.

### 2. Vertical Slice Architecture
All business logic is organized into feature-driven vertical slices in `DonationHub.Application/Features/`:
- Each feature folder (e.g., `Categories/Create`, `Categories/GetAll`, `Auth/`) houses its own request, handler, and response contracts.
- High cohesion within a slice allows independent evolution, refactoring, and maintenance without side-effects across the application.

### 3. Soft Delete Interceptor & Global Query Filter
- `ISoftDeletable` entities (`Campaign`) maintain `IsDeleted` and `DateTime? DeletedAtUtc`.
- `SoftDeleteInterceptor` automatically intercepts `DbContext.SaveChanges` / `SaveChangesAsync`, converting hard `EntityState.Deleted` operations into soft delete updates with UTC timestamps.
- Global Query Filters defined in EF Core configurations automatically exclude deleted records from read operations across the entire application unless explicitly overridden with `.IgnoreQueryFilters()`.

