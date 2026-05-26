# Car App

A playground for learning and trying out C#, .NET, and TypeScript features. The app itself is a car management system with a .NET 10 / Cosmos DB backend and a Next.js 16 / TypeScript / Tailwind frontend.

## Requirements

Docker Desktop.

## Running locally

The easiest way is Docker Compose. It spins up the Cosmos DB emulator, the backend, and the frontend in one command.

```bash
docker compose up --build
```

The app will be at `http://localhost:3000`. The backend API is at `http://localhost:5292`.

The backend automatically creates the `carsapp` database and `cars`/`users` containers in the emulator on startup.

To reset the emulator data:

```bash
docker compose down -v && docker compose up
```

### Running without Docker

Start the backend:

```bash
cd backend/src
dotnet run
```

Start the frontend:

```bash
cd frontend
npm install
npm run dev
```

The frontend reads `NEXT_PUBLIC_BACKEND_URL` from `.env.local` to know where the backend is. Make sure that file exists:

```
BACKEND_URL=http://localhost:5292
NEXT_PUBLIC_BACKEND_URL=http://localhost:5292
```

For local dev without Docker, the backend reads the JWT signing key from .NET User Secrets. You can use either a symmetric secret (quickest) or a certificate:

```bash
cd backend
dotnet user-secrets set "Jwt:Secret" "your-secret-here-min-32-chars" --project src/Cars.csproj
```

## Tests

```bash
cd backend
dotnet test
```

## Environment variables

**Backend (set via Docker Compose or Container App):**

| Variable | Description |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `CosmosDB__CosmosAccountOptions__AccountEndpoint` | Cosmos DB endpoint |
| `CosmosDB__CosmosAccountOptions__UseManagedIdentity` | Use managed identity in production |
| `CosmosDB__CosmosAccountOptions__RunningInContainer` | Enables Docker networking workaround for emulator |
| `JWT_CERT_BASE64` | Base64-encoded PFX certificate for JWT signing (set in `.env` for Docker Compose, Container App secret in production) |
| `Cors__AllowedOrigins__0` | Allowed frontend origin |

**Frontend:**

| Variable | Where | Description |
|---|---|---|
| `BACKEND_URL` | Runtime | Backend URL used by Server Components (Docker internal or localhost) |
| `NEXT_PUBLIC_BACKEND_URL` | Build time | Backend URL baked into the client bundle |

`NEXT_PUBLIC_BACKEND_URL` is embedded into the JavaScript bundle at build time, not at runtime. This means if the backend URL changes, the frontend needs to be rebuilt. The alternative is routing all browser API calls through Next.js API routes (a proxy pattern), which keeps the backend URL server-side only.

## Deployment

The app deploys to Azure Container Apps via GitHub Actions. The workflow builds both Docker images, pushes them to Azure Container Registry, and updates the Container Apps on every push to `main`.

A separate CI workflow runs the backend build and tests on every push and pull request.
