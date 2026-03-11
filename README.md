# Hissatak Integration Services

Backend services for payment processing, WhatsApp notifications, and media uploads — built as part of the [hissatak.online](https://hissatak.online) platform.

## What's inside

Three standalone services wired together under a clean architecture setup:

- **Vodafone Cash payments** via Paymob's wallet API
- **WhatsApp notifications** via Meta's Cloud API
- **Image/file uploads** via Cloudinary

Nothing fancy, no over-engineering. Each service has its own interface, sandbox mode for local development, and logs everything to SQLite.

---

## Stack

- .NET 8 / ASP.NET Core
- Entity Framework Core + SQLite
- Cloudinary .NET SDK
- Paymob Wallet API
- WhatsApp Cloud API (Meta)
- Swagger UI

---

## Project structure

```
├── Domain/
│   └── Entities/          # PaymentRecord, WhatsAppLog
├── Application/
│   ├── DTOs/              # Request/Response models + Settings classes
│   └── Interfaces/        # Service contracts
├── Infrastructure/
│   ├── Data/              # AppDbContext (SQLite)
│   └── Services/          # Actual implementations
└── API/
    └── Controllers/       # Thin controllers, no logic
```

The controllers do nothing except call the service and return the result. All logic lives in the service layer.

---

1. Clone and restore
bashgit clone https://github.com/mohameddsalah03/moalimy.git

**2. Fill in your credentials**

Rename `appsettings.example.json` to `appsettings.json` and add your keys:

```json
{
  "Paymob": {
    "ApiKey": "your_paymob_api_key",
    "WalletIntegrationId": "your_integration_id",
    "IsSandbox": "true"
  },
  "WhatsApp": {
    "AccessToken": "your_meta_access_token",
    "PhoneNumberId": "your_phone_number_id",
    "IsSandbox": "true"
  },
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret",
    "IsSandbox": "true"
  }
}
```

**3. Run**

```bash
dotnet run --project Moalimi.API
```

Swagger will be at `https://localhost:7046/swagger`

The SQLite database (`moalimi.db`) gets created automatically on first run.

---

## Sandbox mode

Every service has an `IsSandbox` flag. When it's `true`:

- **Payment** — skips Paymob entirely, returns a fake transaction ID. Any OTP except `000000` succeeds.
- **WhatsApp** — logs the message to console instead of sending it. Still saves to `WhatsAppLogs` table.
- **Cloudinary** — returns a placeholder URL without touching your Cloudinary account.

Good for local development without burning through API quota or dealing with expired tokens.

---

## API endpoints

### Payment

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/payment/initiate` | Start a Vodafone Cash payment |
| POST | `/api/payment/verify-otp` | Confirm OTP to complete payment |

**Initiate request:**
```json
{
  "msisdnNumber": "01012345678",
  "amount": 200.00,
  "description": "باقة أساسية"
}
```

**Verify OTP request:**
```json
{
  "paymentId": 1,
  "transactionId": "SANDBOX-20260311212955",
  "otpCode": "123456"
}
```

---

### WhatsApp

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/whatsapp/send` | Send a free-text message |
| POST | `/api/whatsapp/payment-confirmation` | Payment confirmed notification |
| POST | `/api/whatsapp/session-reminder` | Upcoming session reminder |
| POST | `/api/whatsapp/gift` | Gift notification |

---

### Uploads

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/upload/avatar/{userId}` | Upload profile picture |
| POST | `/api/upload/certificate/{userId}` | Upload certificate file |

---

## Where credentials come from

**Paymob** — [accept.paymob.com](https://accept.paymob.com) → Settings → Account Info → API Key. Create a mobile wallet integration to get the `WalletIntegrationId`.

**WhatsApp** — [developers.facebook.com](https://developers.facebook.com) → your app → WhatsApp → API Setup. For a non-expiring token, generate one through a System User in Business Settings.

**Cloudinary** — [cloudinary.com](https://cloudinary.com) → Dashboard. Cloud name, API key, and API secret are on the main screen after signup. Free tier is more than enough.

---

## Notes

- Payment flow is two steps: initiate → get OTP on phone → verify. Paymob handles the OTP delivery.
- WhatsApp tokens from the developer console expire every 24 hours. Use a System User token for anything beyond local testing.
- SQLite is fine for this use case. If load increases, swapping to Postgres is just a connection string and one NuGet package change since EF Core abstracts it.
- The `WhatsAppLogs` table keeps a record of every message attempt, success or failure. Useful for debugging without digging through logs.

---

## License

MIT
