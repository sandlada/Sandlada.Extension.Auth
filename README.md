# Sandlada.Extension.Auth

A batteries-included ASP.NET Core authentication & session management library — usable as a standalone Web API **or** as a NuGet package in any .NET host project.

- **Cookie authentication** with server-side session tickets (instant session kick)
- **Email verification code** flows (registration, login, email rebind) — SMTP or custom sender
- **User & UserProfile CRUD** with role-based authorization (Administrator / Normal)
- **OpenID Connect** server (OpenIddict) — Authorization Code + PKCE, Refresh Token
- **GraphQL** endpoint (HotChocolate) alongside REST
- **Hexagonal architecture** — Domain / Application / Infrastructure / Api / Extension

---

## Project Structure

```
Sandlada.Extension.Auth.slnx
├── Sandlada.Extension.Auth.Domain/           # Aggregates, Value Objects, Repository interfaces, Domain Errors
├── Sandlada.Extension.Auth.Domain.Tests/     # Domain unit tests
├── Sandlada.Extension.Auth.Application/       # Commands, Queries, Handlers (MediatR), Service interfaces (ports)
├── Sandlada.Extension.Auth.Application.Tests/ # Application unit tests
├── Sandlada.Extension.Auth.Infrastructure/    # EF Core, repositories, SMTP sender, ticket store, migrations
├── Sandlada.Extension.Auth.Infrastructure.Tests/ # Infrastructure unit tests
├── Sandlada.Extension.Auth.Api/               # ASP.NET Core host, endpoints, GraphQL, OpenIddict config
├── Sandlada.Extension.Auth.Api.Tests/         # API integration tests (WebApplicationFactory)
├── Sandlada.Extension.Auth.Extension/         # NuGet package entry point — AddAuthExtension() / UseAuthExtension()
└── Sandlada.Extension.Auth.Extension.Tests/   # Integration tests from external consumer perspective
```

| Layer              | Responsibility                                                                                                                                                                    |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Domain**         | Aggregates (`User`, `UserProfile`), Value Objects (`EmailAddress`, `UserRole`, `UserStatus`, …), Repository interfaces, `Result`/`DomainError`                                    |
| **Application**    | CQRS via MediatR — `Command`/`Query` + `Handler` per operation. Service interfaces (`ISecretHashService`, `IRegistrationVerificationCodeSender`, `IApplicationUnitOfWork`). DTOs. |
| **Infrastructure** | EF Core `AuthDbContext`, repository implementations, `AuthSessionTicketStore` (server-side cookie store), SMTP verification code sender, `DevelopmentDatabaseInitializer`         |
| **Api**            | Minimal API endpoints (`/Api/Auth`, `/Api/User`, `/Api/UserProfile`, `/Api/OAuthClient`), OpenIddict endpoints, GraphQL, Swagger/OpenAPI                                          |
| **Extension**      | `AddAuthExtension()` / `UseAuthExtension()` — single-call integration for external host projects                                                                                  |

---

## Quick Start (Standalone)

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Smtp4dev](https://github.com/rnwood/smtp4dev) (for email verification in development)

### Clone & Run

```bash
git clone <repo-url>
cd Sandlada.Extension.Auth

# Restore and build
dotnet build Sandlada.Extension.Auth.slnx

# Run the API (standalone)
dotnet run --project Sandlada.Extension.Auth.Api
```

Open **https://localhost:5097/swagger** in your browser. The development profile creates a SQLite database (`auth.Development.db`) and seeds two users:

| Email               | Password | Role          | UniqueName |
| ------------------- | -------- | ------------- | ---------- |
| `admin@example.com` | `admin`  | Administrator | `admin`    |
| `user@example.com`  | `user`   | Normal        | `user`     |

### Development Email (Smtp4dev)

1. Download and start [Smtp4dev](https://github.com/rnwood/smtp4dev) (listens on port 25 by default).
2. `appsettings.Development.json` is pre-configured with `"Host": "localhost"`, `"Port": 25`, `"UseSsl": false`.
3. Verification codes appear in the Smtp4dev web UI at `http://localhost:5000`.

---

## Quick Start (NuGet Integration)

> The **`Sandlada.Extension.Auth.Extension`** project is the NuGet entry point. Reference it directly or via NuGet feed.

### 1. Install

```bash
dotnet add package Sandlada.Extension.Auth
```

Or reference the project directly during development:

```xml
<ProjectReference Include="../Sandlada.Extension.Auth.Extension/Sandlada.Extension.Auth.Extension.csproj" />
```

### 2. Minimal Integration

```csharp
// Program.cs in your host project
using Sandlada.Extension.Auth;

var builder = WebApplication.CreateBuilder(args);

// Register all auth services (Application + Infrastructure + Cookie Auth + Authorization)
builder.Services.AddAuthExtension(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline (DB init, Swagger, auth, endpoint mapping)
app.UseAuthExtension();

app.Run();
```

### 3. Custom Database Provider

Override the EF Core provider by configuring `AddInfrastructure` separately:

```csharp
using Sandlada.Extension.Auth.Application;
using Sandlada.Extension.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

// Use MySQL instead of SQLite
builder.Services.AddInfrastructure(builder.Configuration, options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// Register auth & authorization separately (skip the DB part already done above)
builder.Services.AddAuthExtensionCore(builder.Configuration);

var app = builder.Build();
app.UseAuthExtension();
app.Run();
```

### 4. Custom Email Sender

Implement `IRegistrationVerificationCodeSender` and register it **after** `AddAuthExtension()`:

```csharp
builder.Services.AddAuthExtension(builder.Configuration);

// Replace SMTP with your own API-based sender
builder.Services.AddSingleton<IRegistrationVerificationCodeSender, MyApiEmailSender>();
```

---

## Configuration Reference

All settings live in `appsettings.json` (or any `IConfiguration` source).

### ConnectionStrings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=auth.db"
  }
}
```

| Provider   | Example                                                                                         |
| ---------- | ----------------------------------------------------------------------------------------------- |
| SQLite     | `"Data Source=auth.db"`                                                                         |
| MySQL      | `"Server=localhost;Database=sandlada_auth;User=root;Password=…"`                                |
| PostgreSQL | `"Host=localhost;Database=sandlada_auth;Username=postgres;Password=…"`                          |
| SQL Server | `"Server=localhost;Database=sandlada_auth;Trusted_Connection=true;TrustServerCertificate=true"` |

When switching providers, also update the `.UseXxx()` call in `AddInfrastructure`.

### Auth

```json
{
  "Auth": {
    "Authority": "https://localhost:5097"
  }
}
```

Used by JWT Bearer token validation (standalone host only).

### CORS

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "http://localhost:4000",
      "https://localhost:5097"
    ]
  }
}
```

### Email / SMTP

```json
{
  "Email": {
    "Smtp": {
      "Enabled": true,
      "Host": "localhost",
      "Port": 25,
      "UseSsl": false,
      "UserName": "",
      "Password": "",
      "FromAddress": "noreply@localhost",
      "FromDisplayName": "Sandlada Auth",
      "TimeoutSeconds": 15
    }
  }
}
```

| Field                   | Description                                                     |
| ----------------------- | --------------------------------------------------------------- |
| `Enabled`               | `false` to skip all email sending (uses Noop)                   |
| `Host` / `Port`         | SMTP server address                                             |
| `UseSsl`                | `true` for SMTPS (port 465) or STARTTLS (port 587)              |
| `UserName` / `Password` | Optional — leave empty for unauthenticated SMTP (e.g. Smtp4dev) |
| `FromAddress`           | Required when `Enabled: true`                                   |
| `TimeoutSeconds`        | SMTP connect/send timeout                                       |

---

## API Reference

Base URL: `https://localhost:5097`

### Auth — `/Api/Auth`

| Method | Path                                      | Auth          | Description                             |
| ------ | ----------------------------------------- | ------------- | --------------------------------------- |
| POST   | `/RequestRegistrationVerificationCode`    | Anonymous     | Send verification code to email         |
| POST   | `/Register`                               | Anonymous     | Register new user (signs in on success) |
| POST   | `/RequestEmailRebindVerificationCode`     | Cookie        | Send code to rebind email               |
| POST   | `/ConfirmEmailRebind`                     | Cookie        | Confirm email rebind                    |
| POST   | `/LoginByEmailAddressAndPassword`         | Anonymous     | Login with email + password             |
| POST   | `/LoginByUniqueNameAndPassword`           | Anonymous     | Login with unique name + password       |
| POST   | `/RequestLoginVerificationCode`           | Anonymous     | Send login verification code            |
| POST   | `/LoginByEmailAddressAndVerificationCode` | Anonymous     | Login with email + code                 |
| POST   | `/LoginByUniqueNameAndVerificationCode`   | Anonymous     | Login with unique name + code           |
| POST   | `/Logout`                                 | Cookie        | Sign out current session                |
| POST   | `/LogoutAllSessions`                      | Cookie        | Sign out ALL sessions for current user  |
| POST   | `/KickOneSession/{sessionId}`             | Administrator | Kick a specific session                 |
| POST   | `/KickManySessionsByUserId/{userId}`      | Administrator | Kick all sessions for a user            |

### User — `/Api/User`

| Method | Path                                   | Auth          | Description                          |
| ------ | -------------------------------------- | ------------- | ------------------------------------ |
| GET    | `/FindOneUserById/{userId}`            | Administrator | Get user by ID                       |
| GET    | `/FindOneCurrentUserStatus`            | Cookie        | Get current user's status            |
| PUT    | `/UpdateOneUser/{userId}`              | Administrator | Update user fields                   |
| PUT    | `/UpdateOneUserEmailVerified/{userId}` | Administrator | Set email verified flag              |
| PUT    | `/UpdateOneUserUserStatus/{userId}`    | Administrator | Set user status (Enabled/Disabled/…) |
| POST   | `/InsertOneUser`                       | Administrator | Create a new user                    |
| PUT    | `/InsertOrUpdateOneUser`               | Administrator | Upsert a user                        |
| DELETE | `/RemoveOneUser/{userId}`              | Administrator | Delete a user                        |

### UserProfile — `/Api/UserProfile`

| Method | Path                                             | Auth          | Description                   |
| ------ | ------------------------------------------------ | ------------- | ----------------------------- |
| GET    | `/FindOneCurrentUserProfile`                     | Cookie        | Get own profile               |
| GET    | `/FindOneUserProfileByUserId/{userId}`           | Administrator | Get any user's profile        |
| POST   | `/InsertOneCurrentUserProfile`                   | Cookie        | Create own profile            |
| PUT    | `/UpdateOneCurrentUserProfile`                   | Cookie        | Update own profile            |
| PUT    | `/InsertOrUpdateOneCurrentUserProfile`           | Cookie        | Upsert own profile            |
| DELETE | `/RemoveOneCurrentUserProfile`                   | Cookie        | Delete own profile            |
| POST   | `/ResetOneCurrentUserProfile`                    | Cookie        | Reset own profile to defaults |
| POST   | `/InsertOneUserProfileByUserId/{userId}`         | Administrator | Create profile for user       |
| PUT    | `/UpdateOneUserProfileByUserId/{userId}`         | Administrator | Update profile for user       |
| PUT    | `/InsertOrUpdateOneUserProfileByUserId/{userId}` | Administrator | Upsert profile for user       |
| DELETE | `/RemoveOneUserProfileByUserId/{userId}`         | Administrator | Delete profile for user       |
| POST   | `/ResetOneUserProfileByUserId/{userId}`          | Administrator | Reset profile for user        |

### OAuthClient — `/Api/OAuthClient`

| Method | Path                            | Auth          | Description                 |
| ------ | ------------------------------- | ------------- | --------------------------- |
| POST   | `/InsertOne`                    | Administrator | Register a new OAuth client |
| GET    | `/FindOneByClientId/{clientId}` | Administrator | Get client by ID            |
| GET    | `/FindMany`                     | Administrator | List all OAuth clients      |
| POST   | `/RemoveOne/{id}`               | Administrator | Remove a client             |

### OpenID Connect — `/Connect/*`

| Method | Path                 | Auth                     | Description                            |
| ------ | -------------------- | ------------------------ | -------------------------------------- |
| GET    | `/Connect/Authorize` | Cookie (redirect)        | Authorization endpoint                 |
| POST   | `/Connect/Token`     | OAuth client credentials | Token endpoint (code → token, refresh) |
| GET    | `/Connect/UserInfo`  | Bearer token             | UserInfo endpoint                      |
| GET    | `/Connect/Logout`    | Cookie                   | End session                            |

### GraphQL — `/graphql`

Access the Banana Cake Pop IDE at `https://localhost:5097/graphql`. Supports:

- **Query**: `currentUser`, `currentUserProfile`, `currentUserStatus`, `userById`, `userProfileByUserId`, `oauthClients`
- **Mutation**: `register`, `loginByEmailAddressAndPassword`, `loginByUniqueNameAndPassword`, `requestLoginVerificationCode`, `loginByEmailAddressAndVerificationCode`, `loginByUniqueNameAndVerificationCode`, `requestRegistrationVerificationCode`, `requestEmailRebindVerificationCode`, `confirmEmailRebind`, `logout`

---

## Architecture

### Patterns

| Pattern                            | Where                                                                             | Why                                                                                 |
| ---------------------------------- | --------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| **Hexagonal / Clean Architecture** | Domain ← Application ← Infrastructure → Api                                       | Dependencies point inward; Domain has zero external deps                            |
| **DDD Aggregates**                 | `Domain/Aggregates/User.cs`, `UserProfile.cs`                                     | Business rules live inside aggregates; `From()` factory methods validate invariants |
| **Value Objects**                  | `Domain/ValueObjects/EmailAddress.cs`, `UserRole.cs`, …                           | Immutable, self-validating, `IParsable`, JSON/TypeConverter support                 |
| **Result Pattern**                 | `Domain/Commons/Result.cs`                                                        | No exceptions for domain failures — `IResult<T>` carries `DomainError`              |
| **CQRS (via MediatR)**             | `Application/{Auth,Users,UserProfiles,OAuthClient}/`                              | Each operation = one `Command`/`Query` + `Handler`                                  |
| **Repository Pattern**             | `Domain/Repositories/IUserRepository.cs` → `Infrastructure/.../UserRepository.cs` | Domain defines contract, Infrastructure implements                                  |
| **Server-side Cookie Sessions**    | `Infrastructure/Security/AuthSessionTicketStore.cs`                               | Cookies store only an opaque session ID; tickets live in DB → instant kick          |

### Naming Conventions

- **Find** (Read), **Insert** (Create), **Update** (Modify), **Remove** (Delete) — no "Get"/"Create"/"Delete"
- **FindOne** / **FindMany** for queries; **InsertOne** / **RemoveOne** for commands
- Abbreviations: `Smtp`, `Http`, `Api`, `Db` stay uppercase; longer acronyms (Authentication) use PascalCase
- Files: `CommandInsertOneUser.cs`, `QueryFindOneUserById.cs`, `FindOneUserByIdQueryHandler.cs`

---

## Development Guide

### Build

```bash
dotnet build Sandlada.Extension.Auth.slnx
```

### Run Tests

```bash
dotnet test Sandlada.Extension.Auth.slnx
```

### Database Migrations

After changing domain models, generate a migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project Sandlada.Extension.Auth.Infrastructure \
  --startup-project Sandlada.Extension.Auth.Api
```

In development, the database is recreated from migrations on every start (`DevelopmentDatabaseInitializer`). In production, pending migrations are applied automatically (`dbContext.Database.MigrateAsync()`).

### Run the API

```bash
dotnet run --project Sandlada.Extension.Auth.Api/Sandlada.Extension.Auth.Api.csproj
```

Or from the `Sandlada.Extension.Auth.Api` directory:

```bash
dotnet run
```

---

## Customization

### Swap the Database

1. Add the EF Core provider NuGet package to your host project (e.g. `Pomelo.EntityFrameworkCore.MySql`, `Npgsql.EntityFrameworkCore.PostgreSQL`).
2. Call `AddInfrastructure` with a custom `Action<DbContextOptionsBuilder>`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration, options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Replace the Email Sender

Implement `IRegistrationVerificationCodeSender` and register it after `AddAuthExtension`:

```csharp
public sealed class SendGridVerificationCodeSender : IRegistrationVerificationCodeSender
{
    public async Task SendAsync(
        EmailAddress emailAddress,
        string verificationCode,
        VerificationCodePurpose purpose,
        CancellationToken cancellationToken)
    {
        // Call SendGrid / Mailgun / custom API here
    }
}

// In Program.cs
builder.Services.AddSingleton<IRegistrationVerificationCodeSender, SendGridVerificationCodeSender>();
```

### Add Custom Claims

Override the claims added during sign-in by providing a custom `AuthCookieHelper.SignInAsync`-equivalent in your host project, or add claims via `OnTokenValidated` / `OnTicketReceived` events.

---

## Production Deployment

### HTTPS

The extension enables `app.UseHttpsRedirection()`. Ensure your production environment provides a valid TLS certificate (reverse proxy, cloud load balancer, or ASP.NET Core Kestrel with certificate).

### Database Migrations

In non-development environments, `UseAuthExtension()` automatically runs pending EF Core migrations:

```csharp
// AuthExtension.cs — executed when IHostEnvironment.IsDevelopment() == false
await dbContext.Database.MigrateAsync(cancellationToken);
```

No manual migration step is needed at deploy time. For zero-downtime deployments, review your provider's migration locking behavior.

### OpenIddict Certificates

**Development**: `AddDevelopmentEncryptionCertificate()` / `AddDevelopmentSigningCertificate()` generate ephemeral self-signed certificates. These **MUST NOT** be used in production — they change on every restart and will break token validation across deployments.

**Production**: Register real X.509 certificates:

```csharp
// Program.cs in your host project
using System.Security.Cryptography.X509Certificates;

builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        // ... other server options ...

        // Option A: Load from PFX file
        options.AddEncryptionCertificate(
            new X509Certificate2("/path/to/encryption.pfx", "password"));
        options.AddSigningCertificate(
            new X509Certificate2("/path/to/signing.pfx", "password"));

        // Option B: Load from Windows Certificate Store
        // using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        // store.Open(OpenFlags.ReadOnly);
        // var cert = store.Certificates.Find(X509FindType.FindByThumbprint, "<thumbprint>", false)[0];
        // options.AddSigningCertificate(cert);
    });
```

> ⚠️ Store PFX passwords securely — use Azure Key Vault, AWS Secrets Manager, or environment variables. Never commit certificates to source control.

### Logging

Default log level: `Information`. Set `"Microsoft.AspNetCore": "Warning"` to reduce noise. In production, wire a structured logging sink (Serilog, OpenTelemetry, Application Insights).

---

## Tech Stack

- **.NET 10** / ASP.NET Core
- **EF Core** — SQLite (dev), swappable to MySQL / PostgreSQL / SQL Server
- **MediatR** — CQRS command/query dispatch
- **OpenIddict** — OpenID Connect server
- **HotChocolate** — GraphQL
- **MailKit / MimeKit** — SMTP email
- **Swashbuckle** — Swagger / OpenAPI
- **xUnit** — Testing

---

## License

MIT — see [LICENSE](LICENSE).

