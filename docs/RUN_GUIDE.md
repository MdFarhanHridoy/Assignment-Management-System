# Run Guide — Start, Stop & Restart the App Locally

> **Scope:** Practical commands for running, stopping, and restarting the backend
> (.NET 8 API) and frontend (Next.js 14) on a developer machine. Focused on the
> two operational gotchas that cause the most friction:
>
> 1. **Run in `Development`** — bare `dotnet run` defaults to `Production`, which
>    uses an empty connection string and disables Swagger + DB auto-migration.
> 2. **Stop before rebuild** — the API locks its output DLLs; rebuilding while it
>    runs fails with file-lock errors. Always stop, rebuild, then restart.
>
> Source docs: [`PROJECT_STRUCTURE.md`](./PROJECT_STRUCTURE.md) §5 (build & run),
> [`AUTH_MODEL.md`](./AUTH_MODEL.md) §2 (config/env keys). Shell examples below are
> **Windows PowerShell 5.1** (the project's default shell); cross-platform notes
> are given where relevant.

---

## 1. Ports & Processes

| Service | Port | Process name (Windows) |
|---|---|---|
| Backend API (ASP.NET Core 8) | `5000` | `AssignmentManagement.Api` |
| Frontend (Next.js 14) | `3000` | `node` (via `npm run dev`) |
| PostgreSQL | `5432` | `postgres` |

- API base URL: `http://localhost:5000` (Swagger UI at `/swagger` in Development).
- Frontend URL: `http://localhost:3000` (calls the API at `NEXT_PUBLIC_API_URL`).

---

## 2. Prerequisites (one-time)

1. **PostgreSQL 14+** running locally on `5432` with a `postgres/postgres` superuser
   (or update the connection string in `appsettings.Development.json`).
2. **.NET 8 SDK** (`dotnet --version` must report 8.x).
3. **Node.js 18+** for the frontend (`node --version`).

The connection string used in Development lives in
`server/src/AssignmentManagement.Api/appsettings.Development.json`:

```
Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=postgres
```

> **Note:** `appsettings.json` (the Production/base file) intentionally has an
> **empty** `ConnectionStrings:DefaultConnection`. This is why running without
> `ASPNETCORE_ENVIRONMENT=Development` fails at runtime (DB errors / 500s). Always
> set the Development environment for local work (see §3).

Migrations + demo seeding are applied **automatically on startup** when running in
Development (`DbInitializationService` hosted service, gated by `IsDevelopment()`).
You do **not** need to run `dotnet ef database update` manually for local dev.

---

## 3. Start the backend (Development) — REQUIRED env var

Always set `ASPNETCORE_ENVIRONMENT=Development` for local work. From `server/`:

```powershell
# PowerShell (sets env var for the current session, then runs)
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/AssignmentManagement.Api
```

A single-line version (handy for scripted/background runs):

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --project src/AssignmentManagement.Api
```

Cross-platform (bash / zsh):

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/AssignmentManagement.Api
```

Expected startup output confirms the right environment and port:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

> ⚠️ If you see **`Hosting environment: Production`**, the API will use the empty
> connection string, login/API calls will return **500**, and **Swagger will be
> unavailable**. Stop it and re-run with `ASPNETCORE_ENVIRONMENT=Development`.

---

## 4. Verify the backend is up

In a separate terminal (Development exposes Swagger):

```powershell
# Swagger spec (lists all routes, e.g. /api/admin/enrollments/{id})
Invoke-WebRequest -Uri "http://localhost:5000/swagger/v1/swagger.json" -UseBasicParsing

# Smoke test: log in as the demo Admin
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
  -Method Post -ContentType "application/json" `
  -Body '{"email":"admin@example.com","password":"admin@123"}'
```

Demo credentials (seeded): `admin@example.com` / `admin@123`,
`teacher@example.com` / `teacher@123`, `student@example.com` / `student@123`.

---

## 5. Stop the backend

The API holds locks on its output DLLs, so you **must** stop it before rebuilding
or restarting (see §6). Any one of these works:

**Option A — in the terminal where it is running:**
```
Ctrl+C
```

**Option B — stop by process name (PowerShell, any terminal):**
```powershell
Get-Process -Name AssignmentManagement.Api -ErrorAction SilentlyContinue | Stop-Process -Force
```

**Option C — stop by PID (if you know it, e.g. 16748):**
```powershell
Stop-Process -Id 16748 -Force
```

**Option D — find the PID first, then stop:**
```powershell
Get-Process -Name AssignmentManagement.Api | Select-Object Id, ProcessName, StartTime
# then:  Stop-Process -Id <pid> -Force
```

Confirm it stopped:
```powershell
if (Get-Process -Name AssignmentManagement.Api -ErrorAction SilentlyContinue) { "still running" } else { "stopped" }
```

---

## 6. Restart after a code change (the important workflow)

`dotnet run` builds once and runs; it does **not** hot-reload controller/service
changes by default. After editing backend code, restart so the new binaries load:

```powershell
# 1) Stop the running API (releases the DLL locks)
Get-Process -Name AssignmentManagement.Api -ErrorAction SilentlyContinue | Stop-Process -Force

# 2) Rebuild (from server/) — this FAILS with MSB3027/MSB3021 file-lock errors if the API is still running
dotnet build AssignmentManagement.sln

# 3) Re-run in Development
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/AssignmentManagement.Api
```

> ⚠️ **Symptom of forgetting to stop:** `dotnet build` fails with errors like
> `MSB3027: Could not copy "...AssignmentManagement.Application.dll" ... The file
> is locked by: "AssignmentManagement.Api (PID)"`. This is **not** a code error —
> it means the API is still running. Stop it (§5) and rebuild. A successful build
> reports `Build succeeded. 0 Error(s)`.

> **Tip — auto-reload (optional):** use `dotnet watch` to rebuild & restart on file
> changes automatically:
> ```powershell
> $env:ASPNETCORE_ENVIRONMENT='Development'
> dotnet watch run --project src/AssignmentManagement.Api
> ```
> With `watch`, you usually don't need to manually stop/rebuild after edits.

---

## 7. Start the frontend

From `client/` (separate terminal):

```powershell
npm install        # first time only
npm run dev        # http://localhost:3000
```

The frontend calls the API at `NEXT_PUBLIC_API_URL`
(`client/.env.example` → `http://localhost:5000`). Copy it to `.env.local` if you
need to override. The frontend does **not** need to be restarted when only backend
code changes.

---

## 8. Common issues

| Symptom | Cause | Fix |
|---|---|---|
| API login returns **500** | Running in `Production` → empty connection string | Set `$env:ASPNETCORE_ENVIRONMENT='Development'` and restart (§3/§6) |
| `/swagger` returns **404** | Swagger is Dev-gated | Run in `Development` (§3) |
| `dotnet build` fails with **MSB3027/MSB3021** "file is locked by AssignmentManagement.Api" | API still running; DLLs locked | Stop the API (§5), then rebuild |
| `dotnet run` fails with **"Failed to bind to address http://127.0.0.1:5000: address already in use"** | Another process holds port `5000` (usually a previous API instance still running, e.g. from another terminal or a background run) | Find & stop it: `Get-NetTCPConnection -LocalPort 5000 -State Listen` → note `OwningProcess` → `Stop-Process -Id <pid> -Force`. Shortcut: `Get-Process AssignmentManagement.Api \| Stop-Process -Force` |
| Edits don't take effect after `dotnet run` | `dotnet run` doesn't hot-reload | Stop + rebuild + restart (§6), or use `dotnet watch` |
| **401** on every API call | Token missing/expired (2 h lifetime) | Re-login via `/login` |
| Frontend can't reach API (CORS/network) | API not running, or wrong `NEXT_PUBLIC_API_URL` | Start API (§3); verify `client/.env.local` |

---

## 9. Quick reference (cheat sheet)

```powershell
# --- Backend (from server/) ---
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/AssignmentManagement.Api            # start
# Ctrl+C                                                      # stop (in its terminal)
Get-Process AssignmentManagement.Api | Stop-Process -Force   # stop (any terminal)
dotnet build AssignmentManagement.sln                         # rebuild (stop first!)

# --- Frontend (from client/) ---
npm install
npm run dev                                                   # http://localhost:3000
```
