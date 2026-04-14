# Copilot Instructions — Nexus Productivity Assistant (NPA)

> **Living document** — update this file as the project evolves so every AI-assisted coding session stays consistent.

---

## 1. Project Overview

| Field | Value |
|---|---|
| **Application Name** | Nexus Productivity Assistant (NPA) |
| **Purpose** | Web dashboard (monday.com-style) for tracking Projects, Risks, and Issues |
| **Organisation** | BUI (https://www.bui.co/) |
| **Framework** | .NET 10, Blazor Web App (Interactive Server + WebAssembly) |
| **Database** | Microsoft SQL Server (via Entity Framework Core) |
| **Hosting** | Azure Linux App Service — self-contained publish (`--self-contained true`) |
| **IDE** | Visual Studio 2026+ |

---

## 2. Solution Structure

```
NPA/                            ← Server project (ASP.NET Core host)
│   Components/
│   │   Layout/                 ← Shared layouts (MainLayout, NavMenu)
│   │   Pages/                  ← Server-rendered pages (Error, Home, etc.)
│   │   App.razor               ← Root component
│   │   Routes.razor
│   │   _Imports.razor
│   Data/                       ← EF Core DbContext, Migrations
│   Models/                     ← Domain / entity models
│   Services/                   ← Server-side business logic & data access
│   Controllers/                ← API controllers (if needed for WASM HTTP calls)
│   wwwroot/                    ← Static assets (CSS, JS, images)
│   Program.cs                  ← Host builder & middleware pipeline
│   NPA.csproj                  ← net10.0, Microsoft.NET.Sdk.Web

NPA.Client/                     ← Blazor WebAssembly client project
│   Pages/                      ← Interactive WASM pages
│   Components/                 ← Reusable client-side components
│   Services/                   ← Client-side service interfaces / HTTP clients
│   wwwroot/                    ← Client static assets
│   Program.cs                  ← WebAssemblyHostBuilder
│   NPA.Client.csproj           ← net10.0, Microsoft.NET.Sdk.BlazorWebAssembly

NPA.Shared/                     ← (Future) Shared DTOs, enums, constants
```

---

## 3. Branding & UI / UX — BUI Theme

The look and feel **must match** [https://www.bui.co/](https://www.bui.co/).

### 3.1 Colour Palette

| Token | Hex | Usage |
|---|---|---|
| `--bui-primary` | `#0D1B2A` | Dark navy — sidebars, headers, primary backgrounds |
| `--bui-primary-light` | `#1B2D45` | Slightly lighter navy — hover states, cards |
| `--bui-accent` | `#E8491D` | BUI orange — CTAs, active indicators, links |
| `--bui-accent-hover` | `#D03C14` | Darker orange — button hover |
| `--bui-success` | `#2ECC71` | Positive status (On Track, Resolved) |
| `--bui-warning` | `#F39C12` | Warning status (At Risk, In Progress) |
| `--bui-danger` | `#E74C3C` | Critical / High risk |
| `--bui-surface` | `#F4F6F9` | Page background / light surface |
| `--bui-card` | `#FFFFFF` | Card background |
| `--bui-text` | `#1E1E1E` | Primary body text |
| `--bui-text-muted` | `#6C757D` | Secondary / muted text |
| `--bui-text-on-dark` | `#FFFFFF` | Text on dark backgrounds |
| `--bui-border` | `#DEE2E6` | Subtle borders and dividers |

### 3.2 Typography

- **Font family**: `'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif`
- **Headings**: Bold, sentence case
- **Body text**: 14–16 px, regular weight
- Keep generous white-space; BUI site is clean and modern

### 3.3 Layout & Components (monday.com-inspired)

- **Sidebar navigation** (dark navy, collapsible) with grouped menu items
- **Top header bar** with breadcrumbs, user avatar, notification bell
- **Dashboard cards** (KPI widgets) at the top of each section
- **Board / table views** with colour-coded status pills
- **Kanban view** option for issues & risks
- **Detail slide-over panels** (right drawer) for editing items
- **Responsive** — must work on tablets (min 768px)

### 3.4 Fun Element — Floating Monkey 🐒

- A small animated monkey sprite that **floats across the screen** when the user navigates to a different tab/page.
- Use CSS `@keyframes` animation (translate left → right) with a `z-index` overlay.
- Duration ≈ 3 seconds, only plays once per navigation, then hides.
- Monkey image: place an SVG or PNG in `wwwroot/images/monkey.png`.
- Implement as a shared Blazor component (`FloatingMonkey.razor`) included in `MainLayout`.
- Listen to `NavigationManager.LocationChanged` to trigger the animation.

---

## 4. Domain Model

### 4.1 Core Entities

```
Project
├── ProjectId          (int, PK, identity)
├── Name               (string, required, max 200)
├── Description        (string, max 2000)
├── Status             (enum: NotStarted, InProgress, OnHold, Completed, Cancelled)
├── Priority           (enum: Low, Medium, High, Critical)
├── StartDate          (DateOnly)
├── TargetEndDate      (DateOnly)
├── ActualEndDate      (DateOnly?)
├── CreatedBy          (string → userId)
├── CreatedDate        (DateTimeOffset)
├── ModifiedDate       (DateTimeOffset)
└── AssignedUsers      (many-to-many → ProjectAssignment)

Risk
├── RiskId             (int, PK, identity)
├── ProjectId          (int, FK → Project)
├── Title              (string, required, max 300)
├── Description        (string, max 2000)
├── Likelihood         (enum: VeryLow, Low, Medium, High, VeryHigh)
├── Impact             (enum: VeryLow, Low, Medium, High, VeryHigh)
├── Status             (enum: Open, Mitigating, Closed, Accepted)
├── MitigationPlan     (string, max 4000)
├── Owner              (string → userId)
├── IdentifiedDate     (DateOnly)
├── CreatedDate        (DateTimeOffset)
└── ModifiedDate       (DateTimeOffset)

Issue
├── IssueId            (int, PK, identity)
├── ProjectId          (int, FK → Project)
├── Title              (string, required, max 300)
├── Description        (string, max 2000)
├── Severity           (enum: Low, Medium, High, Critical)
├── Status             (enum: Open, InProgress, Resolved, Closed)
├── AssignedTo         (string → userId)
├── ResolutionNotes    (string, max 4000)
├── ReportedDate       (DateOnly)
├── ResolvedDate       (DateOnly?)
├── CreatedDate        (DateTimeOffset)
└── ModifiedDate       (DateTimeOffset)

ProjectAssignment
├── ProjectId          (int, FK → Project)
├── UserId             (string, FK → ApplicationUser)
├── Role               (enum: StandardUser, ProjectManager)
└── AssignedDate       (DateTimeOffset)
```

### 4.2 Enums

Define all enums in `NPA.Shared/Enums/` (or `NPA/Models/Enums/` until the Shared project exists). Use `[JsonConverter(typeof(JsonStringEnumConverter))]` for API serialisation.

---

## 5. Authentication & Authorisation

### 5.1 Authentication Providers

| Provider | Who uses it | Package |
|---|---|---|
| **Microsoft Entra ID (Azure AD)** | All corporate / SSO users | `Microsoft.Identity.Web`, `Microsoft.Identity.Web.UI` |
| **ASP.NET Core Identity (local)** | Admin accounts only | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |

- The login page must offer **both** options: "Sign in with Microsoft" and a local username/password form.
- Entra ID config stored in `appsettings.json` under `AzureAd` section.
- Local admin accounts are seeded on first run (see §7 Seed Data).

### 5.2 Roles

| Role | Permissions |
|---|---|
| `Admin` | Full access — manage users, all projects, all settings, assign roles |
| `ProjectManager` | View **all** projects; assign Standard Users to projects; manage risks & issues for any project |
| `StandardUser` | View **only** projects assigned to them; create/edit risks & issues on assigned projects |

- Use ASP.NET Core `[Authorize(Roles = "...")]` for page/API protection.
- Apply `AuthorizeView` in Blazor components for UI-level gating.
- Role claims must be emitted by both Entra ID (via app roles) and local Identity.

### 5.3 Configuration Keys (appsettings.json)

```jsonc
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<your-tenant>.onmicrosoft.com",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "CallbackPath": "/signin-oidc"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=<server>;Database=NPA;User Id=<user>;Password=<pw>;TrustServerCertificate=True"
  }
}
```

---

## 6. Data Access

- **ORM**: Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`)
- **DbContext**: `NpaDbContext` in `NPA/Data/`
- **Migrations**: Code-first, stored in `NPA/Data/Migrations/`
- **Pattern**: Repository / Service layer in `NPA/Services/` — no direct DbContext calls from pages
- **Naming**: Table names = plural (`Projects`, `Risks`, `Issues`, `ProjectAssignments`)

---

## 7. Seed Data

On first migration / application start, seed:

1. **Roles**: `Admin`, `ProjectManager`, `StandardUser`
2. **Default admin account**: `admin@npa.local` / `NpaAdmin@2025!` (local Identity, role = Admin)
3. **Sample project** (optional, dev only): "NPA Onboarding" with 2 risks and 3 issues

---

## 8. Publishing & Deployment

### 8.1 Publish Profile (Linux App Service, self-contained)

```xml
<PropertyGroup>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <SelfContained>true</SelfContained>
  <PublishSingleFile>false</PublishSingleFile>
  <PublishTrimmed>false</PublishTrimmed>
</PropertyGroup>
```

- All assemblies must ship with the site (`--self-contained true`).
- Target runtime: `linux-x64`.
- Ensure `wwwroot` static assets are included.

### 8.2 CLI Publish Command

```bash
dotnet publish NPA/NPA.csproj -c Release -r linux-x64 --self-contained true -o ./publish
```

---

## 9. Coding Conventions

| Rule | Detail |
|---|---|
| **Language** | C# 13 / .NET 10 |
| **Nullable** | `enable` — no `null` without `?` |
| **Implicit usings** | `enable` |
| **File-scoped namespaces** | Yes |
| **Async** | All I/O-bound methods must be `async Task` / `async Task<T>` |
| **Naming** | PascalCase for public members, `_camelCase` for private fields |
| **Components** | One `.razor` file per component; use code-behind (`.razor.cs`) for >30 lines of logic |
| **CSS isolation** | Prefer `Component.razor.css` scoped files |
| **Error handling** | Use `try/catch` at service boundaries; log with `ILogger<T>` |
| **No comments unless** | They match existing style or explain complex logic |
| **Libraries** | Prefer existing packages; ask before adding new NuGet dependencies |

---

## 10. NuGet Packages (Planned)

| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider |
| `Microsoft.EntityFrameworkCore.Tools` | Migrations CLI |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Local Identity store |
| `Microsoft.Identity.Web` | Entra ID authentication |
| `Microsoft.Identity.Web.UI` | Entra ID sign-in UI |
| `Microsoft.AspNetCore.Components.WebAssembly` | (already present) WASM runtime |
| `Microsoft.AspNetCore.Components.WebAssembly.Server` | (already present) WASM server hosting |

---

## 11. Key Pages & Components Roadmap

| Page / Component | Location | Render Mode | Description |
|---|---|---|---|
| `Login.razor` | Server | Server | Dual sign-in (Entra ID + local) |
| `Dashboard.razor` | Client | WASM | KPI cards, recent activity, my projects |
| `ProjectList.razor` | Client | WASM | Board/table view of projects with filters |
| `ProjectDetail.razor` | Client | WASM | Single project — tabs for Risks & Issues |
| `RiskBoard.razor` | Client | WASM | Kanban/table for risks |
| `IssueBoard.razor` | Client | WASM | Kanban/table for issues |
| `UserManagement.razor` | Server | Server | Admin — manage users & roles |
| `ProjectAssignment.razor` | Server | Server | PM — assign users to projects |
| `FloatingMonkey.razor` | Shared Layout | — | Animated monkey on navigation |
| `StatusPill.razor` | Shared Component | — | Colour-coded status badge |
| `KpiCard.razor` | Shared Component | — | Dashboard metric card |
| `NavMenu.razor` | Layout | — | Dark sidebar navigation |
| `MainLayout.razor` | Layout | — | Overall page shell |

---

## 12. API Endpoints (Server → Client communication)

Since the WASM client needs data, expose minimal API or controller endpoints:

```
GET    /api/projects                    → list (filtered by role)
GET    /api/projects/{id}               → single project with risks & issues
POST   /api/projects                    → create
PUT    /api/projects/{id}               → update
DELETE /api/projects/{id}               → soft delete

GET    /api/projects/{id}/risks         → list risks
POST   /api/projects/{id}/risks         → create risk
PUT    /api/risks/{id}                  → update risk

GET    /api/projects/{id}/issues        → list issues
POST   /api/projects/{id}/issues        → create issue
PUT    /api/issues/{id}                 → update issue

GET    /api/users                       → admin: all users
POST   /api/projects/{id}/assign        → PM: assign user
DELETE /api/projects/{id}/assign/{uid}  → PM: unassign user
```

---

## 13. Questions to Resolve Before Coding

> **Answer these so Copilot can generate the most accurate code:**

1. **Entra ID Tenant** — Do you have the Tenant ID, Client ID, and App Registration ready, or should the code use placeholder values for now?
2. **SQL Server** — Is this Azure SQL, or an on-prem / Docker SQL Server? Connection string format?
3. **Email / Notifications** — Should the app send email notifications (e.g., when a risk is assigned), or is that a future phase?
4. **File Attachments** — Do projects/risks/issues need file upload support?
5. **Audit Trail** — Do you need a full audit log of who changed what and when?
6. **Soft Delete vs Hard Delete** — Should deleted records be hidden (soft) or permanently removed?
7. **Multi-tenancy** — Is this a single-org app (BUI only), or will multiple organisations use it?
8. **Monkey Asset** — Do you have a specific monkey image/SVG, or should I generate a placeholder?
9. **Deployment Pipeline** — Will you use GitHub Actions, Azure DevOps, or manual publish to deploy to the Linux App Service?
10. **Additional Dashboard Widgets** — Beyond project/risk/issue counts, do you want charts (burn-down, risk heat-map, etc.)?

---

## 14. Getting Started (Developer Workflow)

```bash
# 1. Restore packages
dotnet restore

# 2. Apply EF migrations (once database is configured)
dotnet ef database update --project NPA

# 3. Run locally
dotnet run --project NPA

# 4. Publish for Linux App Service
dotnet publish NPA/NPA.csproj -c Release -r linux-x64 --self-contained true -o ./publish
```

---

*Last updated: auto-generated by Copilot*
