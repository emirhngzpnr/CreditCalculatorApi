# Credit Calculator — Angular frontend

Angular 20 interface for the [Credit Calculator & Application Management project](../README.md), developed during my VakıfBank internship.

The application includes loan calculations, bank and campaign browsing, customer and credit application forms, account/profile pages, and admin screens for banks, campaigns, applications, and logs.

## Run locally

Complete the [local setup guide](../docs/SETUP.md) first, then run from this directory:

```bash
npm ci
npm start
```

The frontend runs at `http://localhost:4200`. API services reference `https://localhost:7152/api`; the backend must be running with matching HTTPS and CORS settings.

## Project structure

| Directory | Purpose |
| --- | --- |
| `src/app/pages` | User-facing pages, account flows, and profile views |
| `src/app/admin` | Admin screens |
| `src/app/services` | API clients and authentication interceptor |
| `src/app/guards` | Route guards |
| `src/app/models` | TypeScript API models |
| `src/app/layouts` | User and admin layouts |

## Build and test

```bash
npm run build
npm test
```

The test command runs the checked-in Jasmine/Karma specs. End-to-end testing is not configured in this project.

See the [main README](../README.md) for the architecture and internship background, or the [Türkçe README](../README.tr.md) for the Turkish overview.

