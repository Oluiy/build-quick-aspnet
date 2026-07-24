# JWT Authentication

Answering yes to "Add JWT Authentication boilerplate?" wires up bearer-token authentication end to end, with a working example, not just the middleware.

## What's generated

- The `Microsoft.AspNetCore.Authentication.JwtBearer` package, added to the API project.
- `builder.Services.AddAuthentication(...).AddJwtBearer(...)` in `Program.cs`, configured to validate issuer, audience, lifetime, and signing key against your `Jwt:*` configuration.
- `app.UseAuthentication()` / `app.UseAuthorization()` in the middleware pipeline, in the correct order (after CORS, before endpoint mapping).
- A JWT bearer security scheme registered with Swashbuckle (`AddSecurityDefinition`/`AddSecurityRequirement`), so Swagger UI actually shows an **Authorize** button and lock icons instead of silently ignoring auth entirely. Without this, JWT still works against the API directly; Swagger UI just has no way to know about it or let you attach a token.
- A `Jwt` section in `appsettings.json` (`Issuer`, `Audience`, `ExpiryMinutes`) and a signing key in `appsettings.Development.json`; see [Getting Started](getting-started.md) for how the three `appsettings.*` files layer.
- Two sample endpoints in `Program.cs`:
  - `POST /api/auth/token?username=someone`: issues a signed JWT for the given username. This is a **demo token issuer**, not a real login flow (no password check, no user store); see below.
  - `GET /api/secure`: requires a valid bearer token (`[Authorize]`-equivalent via `.RequireAuthorization()`), returns a simple confirmation message.

Already generated a project without it? Run `BuildQuickPkg add jwt` from the project root; see [Adding a Feature Later](adding-features-later.md).

## Trying it out

With the API running:

```bash
# Get a token
curl -X POST "http://localhost:5200/api/auth/token?username=alice"
# → { "token": "eyJhbGciOi..." }

# Call the protected endpoint without a token
curl http://localhost:5200/api/secure
# → 401 Unauthorized

# Call it with the token
curl http://localhost:5200/api/secure -H "Authorization: Bearer eyJhbGciOi..."
# → { "message": "You are authenticated!" }
```

Or use Swagger UI (`http://localhost:5200/`): click **Authorize** in the top right, paste just the token (no `Bearer ` prefix), and it's attached to every request you make from the UI afterward.

Note the lock icon shows on every endpoint, including `/api/health`, which doesn't actually require a token; the security requirement is registered at the document level rather than per-endpoint, which is simpler and is what most JWT+Swagger setups do, at the cost of that one cosmetic overstatement. The API itself is unaffected: only endpoints with `.RequireAuthorization()` (like `/api/secure`) actually reject unauthenticated requests.

## Replacing the demo token endpoint

`POST /api/auth/token` exists to prove the wiring works end to end on generation; it issues a token for *any* username with no verification. Before this goes anywhere real, replace it with an actual login flow: validate credentials against your user store (likely backed by the `DbContext`, if you also picked Entity Framework Core), then issue the token only on success. The token-issuing code itself (claims, signing key, expiry) is a reasonable starting point; the missing piece is the credential check in front of it.

## Going to production

- `appsettings.Development.json` ships a **dev-only signing key**: long enough to pass validation, clearly labeled, and never meant to sign anything real.
- `appsettings.Production.json` leaves `Jwt:Key` blank. Supply a strong, random secret (32+ characters, e.g. `openssl rand -base64 32`) via the `Jwt__Key` environment variable or your platform's secret manager; never commit it.
- Rotate the signing key periodically, and keep `Issuer`/`Audience` matched to your actual deployed URLs, not `https://localhost`.
- If you need refresh tokens, roles/claims-based authorization, or external identity providers (Azure AD, Auth0, etc.), those are deliberately out of scope for the generated boilerplate; this gives you a working starting point, not a full auth system.
