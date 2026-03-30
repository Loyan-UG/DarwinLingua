# Local PostgreSQL Setup

## Purpose

This runbook explains how to start the **shared-content PostgreSQL database locally** on Windows with Docker Desktop.

Use it when you want to prepare the local environment for the future `DarwinLingua.WebApi` and shared-content backend work.

This document works with:

- `tools/Server/Postgres/docker-compose.yml`
- `tools/Server/Postgres/.env.example`
- `tools/Server/Postgres/init/001-create-databases.sh`
- `tools/Server/Config/appsettings.WebApi.Development.example.json`

---

## 1. Prerequisites

You need:

- Docker Desktop installed and running
- WSL 2 enabled
- port `5432` free on your machine
- port `8080` free if you also start pgAdmin

Recommended:

- enough free disk space for Docker volumes
- a password manager for local secrets

---

## 2. Files to Prepare

Go to:

- `tools/Server/Postgres`

Then create a real env file:

- copy `.env.example` to `.env`

The real `.env` file should stay local and should not be committed.

---

## 3. Recommended Local Defaults

The provided templates assume:

- PostgreSQL database: `darwinlingua_shared`
- app user: `darwinlingua_app`
- migrations/import user: `darwinlingua_admin`
- PostgreSQL exposed on `localhost:5432`
- pgAdmin exposed on `localhost:8080`

These defaults are fine for local development.

---

## 4. Start the Database

From the repository root:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml up -d
```

This starts:

- PostgreSQL
- pgAdmin

To check status:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps
```

To stop:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml down
```

To stop and remove data:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml down -v
```

Use `down -v` only when you intentionally want to wipe the local database.

---

## 5. Verify the Database

You can verify PostgreSQL with:

```powershell
docker exec -it darwinlingua-postgres psql -U postgres -d darwinlingua_shared -c "\dt"
```

Or open pgAdmin:

- URL: `http://localhost:8080`
- login with the values from `.env`

Then register the PostgreSQL server with:

- host: `postgres`
- port: `5432`
- username: `postgres`
- password: the `DARWINLINGUA_POSTGRES_SUPER_PASSWORD` value from `.env`

Use `postgres` as the host inside pgAdmin because it runs inside the same Docker network.

---

## 6. Web API Connection String

For the future Web API host, the recommended local connection string shape is:

```text
Host=localhost;Port=5432;Database=darwinlingua_shared;Username=darwinlingua_app;Password=<your-password>
```

The example appsettings file is:

- `tools/Server/Config/appsettings.WebApi.Development.example.json`

Recommended split:

- `ServerContent` for read-only runtime API access
- `ServerContentAdmin` for migrations, import, and publishing workflows

When the real Web API project is created, copy those values into the actual `appsettings.Development.json` or local secrets.

---

## 7. Important Setup Notes

### 7.1 Pick Strong Passwords

Do not leave the template passwords unchanged.

At minimum, change:

- `DARWINLINGUA_POSTGRES_SUPER_PASSWORD`
- `DARWINLINGUA_POSTGRES_APP_PASSWORD`
- `DARWINLINGUA_POSTGRES_ADMIN_PASSWORD`
- `DARWINLINGUA_PGADMIN_PASSWORD`

### 7.2 Keep the Time Zone Consistent

The template uses:

- `Europe/Berlin`

Keep the database and app environments aligned to avoid confusing timestamps.

### 7.3 Do Not Reuse the Superuser in the App

The runtime Web API should use:

- `darwinlingua_app`

The import/migration path can use:

- `darwinlingua_admin`

This prevents accidental over-privilege. The provided init script keeps the app role read-only by default.

### 7.4 Watch Port Conflicts

If another PostgreSQL instance already uses `5432`, change the host-side port in:

- `tools/Server/Postgres/docker-compose.yml`

Then update the Web API connection string to match.

### 7.5 Back Up Volumes Before Destructive Resets

If you start using this DB for real shared-content work, do not casually run:

```powershell
docker compose ... down -v
```

That deletes the local volume.

---

## 8. What You Need To Do Outside This Repository

You need to do these steps yourself:

1. install Docker Desktop if it is not already installed
2. make sure WSL 2 integration is enabled
3. copy `tools/Server/Postgres/.env.example` to `tools/Server/Postgres/.env`
4. replace the template passwords
5. run the `docker compose ... up -d` command
6. verify that `localhost:5432` responds

Optional but recommended:

7. open pgAdmin and confirm the database is visible

---

## 9. Final Recommendation

Use the provided Docker-based PostgreSQL setup as the local shared-content backend baseline.

It is simple enough for local development, but structured enough to support:

- future Web API work
- server-side content import
- package publication
- multi-product shared infrastructure
