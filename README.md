# NotifyService

test random commit SHA a085d496676eec3ef1f0ad2da26ae2f7f4adc7944e09153a50c9002431a64f96

Toy notification system used to exercise a Kubernetes CI/CD pipeline. Logic is intentionally trivial. **All backing services are in-memory stand-ins** — nothing is shared across processes, and nothing talks to a database, broker, or cloud provider.

## Workloads

| Project | Role |
| --- | --- |
| `Notify.Core.Api` | Accepts `POST /api/notifications`, stores the payload, enqueues it, and serves `GET /api/notifications/{id}`. Health: `/health`, `/health/ready`. |
| `Notify.Public.Api` | Accepts `POST /api/callbacks/status` and serves `GET /api/notifications/{id}/status`. Optional `X-Api-Key` check against config `ApiKey` (skipped when empty). Same health endpoints. |
| `Notify.Worker` | Dequeues items, calls the sender, updates status. On `SIGTERM` it finishes the in-flight item, then exits. |

The in-memory queue does **not** cross process boundaries, so the worker cannot see items posted to Core. That is expected. The worker enqueues a synthetic notification every 30 seconds so deployed logs still show activity.

Each process logs config value `AppEnvironment` at startup.

## Interface → production mapping

| Interface | This repo | Production |
| --- | --- | --- |
| `INotificationStore` | `ConcurrentDictionary` | SQL database |
| `INotificationQueue` | `System.Threading.Channels.Channel<T>` | Message broker |
| `INotificationSender` | Console no-op | SMS / email provider |

## Run locally

From `src/` (SDK 8.0):

```bash
dotnet restore
dotnet run --project Notify.Core.Api --urls http://localhost:5080
dotnet run --project Notify.Public.Api --urls http://localhost:5081
dotnet run --project Notify.Worker
```

```bash
curl -X POST http://localhost:5080/api/notifications \
  -H 'Content-Type: application/json' \
  -d '{"recipient":"user@example.com","body":"hello"}'
```

Images expect **pre-published** output as the Docker build context (`dotnet publish -o <dir>`, then `docker build` that folder). Charts live under `charts/`; environment overrides are `charts/values/dv1` and `charts/values/qa1`.
