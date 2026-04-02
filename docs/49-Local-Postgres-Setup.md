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
- port `5050` free if you also start pgAdmin

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
- pgAdmin exposed on `localhost:5050`

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

You can also verify it with pgAdmin.

### 5.1 Option A: Use the pgAdmin Container From Docker Compose

This project already starts a **pgAdmin container** for you.

Open:

- URL: `http://localhost:5050`

Log in with:

- email: the `DARWINLINGUA_PGADMIN_EMAIL` value from `.env`
- password: the `DARWINLINGUA_PGADMIN_PASSWORD` value from `.env`

After login, click:

- `Add New Server`

In the **General** tab:

- Name: `DarwinLingua Local Postgres`

In the **Connection** tab use:

- Host name/address: `postgres`
- Port: `5432`
- Maintenance database: `postgres`
- Username: `postgres`
- Password: the `DARWINLINGUA_POSTGRES_SUPER_PASSWORD` value from `.env`

Then save.

Use `postgres` as the host inside pgAdmin because it runs inside the same Docker network.

### 5.2 Option B: Use pgAdmin Installed on Windows

If you are using **pgAdmin installed directly on Windows** instead of the Docker pgAdmin container, then `postgres` will usually **not** resolve.

In that case, click:

- `Add New Server`

In the **General** tab:

- Name: `DarwinLingua Local Postgres`

In the **Connection** tab use:

- Host name/address: `localhost`
  or `127.0.0.1`
- Port: `5432`
- Maintenance database: `postgres`
- Username: `postgres`
- Password: the `DARWINLINGUA_POSTGRES_SUPER_PASSWORD` value from `.env`

Then save.

### 5.3 Which Host Value Should You Use?

Use:

- `postgres` only when you are using the **pgAdmin container** opened through `http://localhost:5050`
- `localhost` or `127.0.0.1` when you are using **pgAdmin installed on Windows**

If you see an error like:

```text
failed to resolve host 'postgres': [Errno -2] Name does not resolve
```

that usually means you are **not** using the pgAdmin container from Docker Compose, or that the pgAdmin container is not running.

### 5.4 Quick Checks if Connection Fails

First confirm that both containers are running:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps
```

You should see at least:

- `darwinlingua-postgres`
- `darwinlingua-pgadmin`

Then verify PostgreSQL itself is reachable:

```powershell
docker exec -it darwinlingua-postgres psql -U postgres -d postgres -c "SELECT version();"
```

If that works, PostgreSQL is fine and the problem is usually just the **host name used in pgAdmin**.

### 5.5 If pgAdmin Still Cannot Resolve `postgres`

One common cause is that you have an **old pgAdmin container** running outside this Docker Compose stack.

In that case:

- PostgreSQL may be running on the Compose network `postgres_default`
- but pgAdmin may be running on plain Docker `bridge`

Then pgAdmin cannot resolve the hostname `postgres`, even though PostgreSQL itself is healthy.

Check the running containers:

```powershell
docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"
```

Then check the Compose stack:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps -a
```

You should see both:

- `darwinlingua-postgres`
- `darwinlingua-pgadmin`

If `darwinlingua-pgadmin` is missing from the Compose output, or if `docker compose up -d pgadmin` reports a container-name conflict, remove the old pgAdmin container and recreate the Compose one:

```powershell
docker rm -f darwinlingua-pgadmin
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml up -d pgadmin
```

After that, verify again:

```powershell
docker compose --env-file tools/Server/Postgres/.env -f tools/Server/Postgres/docker-compose.yml ps -a
```

Expected result:

- `darwinlingua-postgres` and `darwinlingua-pgadmin` are both listed
- both are on the same Compose stack
- the pgAdmin web UI at `http://localhost:5050` can use `postgres` as the host

If you want to confirm the Docker network explicitly:

```powershell
docker inspect darwinlingua-postgres
docker inspect darwinlingua-pgadmin
```

Both should show:

- `NetworkMode: postgres_default`

and both should be attached to:

- `postgres_default`

### 5.6 Recommended Choice

For this repository, the easiest setup is:

1. run the provided Docker Compose
2. open pgAdmin at `http://localhost:5050`
3. click `Add New Server`
4. use `postgres` as the host

If you prefer your own pgAdmin installation on Windows, use:

- host: `localhost`

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
