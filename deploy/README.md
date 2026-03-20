# Deploy

This folder contains baseline deployment assets for the SSD monorepo.

## Local Container Stack

```bash
docker compose -f deploy/docker-compose.yml up --build
```

## Production Direction

- Build and publish the API container from `deploy/Dockerfile.api`
- Run PostgreSQL as a managed service where possible
- Provide secrets through your deployment platform, not committed files
- Add mobile distribution pipelines separately from API deployment
