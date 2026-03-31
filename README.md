# authentication-system

A full-stack authentication system (Backend + Frontend) built with ASP.NET Core.
Includes registration, login, email OTP verification, and OTP resend with Clean Architecture.

## Features
- Register / Login / Verify OTP / Resend OTP
- JWT Authentication
- Secure password hashing (`PasswordHasher`)
- OTP generation with expiration
- Rate limiting for OTP requests
- Validation + Error Handling + Logging
- MVC UI with Bootstrap

## Architecture
- `src/Domain` core entities
- `src/Application` use cases, services, and business logic
- `src/Infrastructure` persistence, JWT, OTP, email
- `src/Api` Web API
- `src/Web` MVC frontend

## Quick Start
1. Run the API:
   ```bash
   dotnet run --project src/Api
   ```
2. Run the MVC app:
   ```bash
   dotnet run --project src/Web
   ```

## Configuration
### Database (Local SQL Server)
Default connection string in `src/Api/appsettings.json`:
```json
"Default": "Server=(localdb)\\MSSQLLocalDB;Database=AuthenticationSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### JWT
`SigningKey` must be at least 32 bytes:
```json
"Jwt": {
  "Issuer": "AuthenticationSystem",
  "Audience": "AuthenticationSystem",
  "SigningKey": "dev-only-change-me-please-32bytes-minimum-key-123456"
}
```

### OTP
Fixed OTP code for development only:
```json
"Otp": {
  "CodeLength": 6,
  "ExpiryMinutes": 10,
  "MinResendIntervalSeconds": 60,
  "MaxPerHour": 5,
  "HashKey": "replace-with-otp-hash-key",
  "FixedCode": "123456"
}
```
> Remove `FixedCode` in production.

## Notes
- OTP is logged in the API console via `LoggingEmailSender` (dev only).
- The frontend stores the JWT in Session (display/demo purposes only).
