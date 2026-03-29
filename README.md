# SehhaLink (Sehha360 Backend)

SehhaLink is a secure medical document management and healthcare platform built with .NET 9.0. It allows patients and doctors to securely store, share, and manage medical records using cloud storage (Supabase) and encrypted communication.

## 🚀 Tech Stack

- **Framework**: ASP.NET Core 9.0 (Web API)
- **Database**: PostgreSQL (Neon/Railway)
- **Storage**: Supabase Storage (S3-compatible)
- **Auth**: Entity Framework Core Identity + JWT
- **Deployment**: Docker + Railway

---

## 🛠️ Setup & Installation

### 1. Prerequisites
- .NET 9.0 SDK
- PostgreSQL instance
- Supabase account (for Storage)

### 2. Environment Variables
Create a `.env` file in the root directory with the following variables:

```text
CONNECTION_STRING="your-postgresql-connection-string"
SecurityKey="your-long-jwt-secret-key"
SUPABASE_URL="https://your-project-id.supabase.co"
SUPABASE_KEY="your-service-role-key"
SUPABASE_BUCKET="medical-documents"

# SMTP Settings (for OTP)
SMTP_HOST="smtp.gmail.com"
SMTP_PORT=587
SMTP_USERNAME="your-email@gmail.com"
SMTP_PASSWORD="your-app-password"
SMTP_FROM_EMAIL="your-email@gmail.com"
SMTP_FROM_NAME="SehhaLink"
```

### 3. Run Locally
```bash
dotnet restore
dotnet ef database update
dotnet run
```

---

## 📖 API Documentation

### Authentication (`/api/Auth`)

| Endpoint | Method | Description | Auth |
| :--- | :--- | :--- | :--- |
| `/register` | `POST` | Register a new user (Patient/Doctor) | No |
| `/login` | `POST` | Authenticate and receive a JWT token | No |
| `/forgot-password` | `POST` | Request an OTP via email for password reset | No |
| `/reset-password` | `POST` | Reset password using the received OTP | No |

### Document Management (`/api/Documents`)

| Endpoint | Method | Description | Auth |
| :--- | :--- | :--- | :--- |
| `/upload` | `POST` | Upload a file (PDF, JPEG, PNG, DICOM) - Max 20MB | Yes |
| `/{id}/url` | `GET` | Generate a 15-minute secure signed URL for a file | Yes |

### User Profile (`/api/User`)

| Endpoint | Method | Description | Auth |
| :--- | :--- | :--- | :--- |
| `/me` | `GET` | Get the current user's profile details | Yes |
| `/me` | `PATCH` | Update profile (name, phone, etc.) | Yes |
| `/deactivate` | `POST` | Soft-delete account (30-day recovery period) | Yes |
| `/me` | `DELETE` | Request immediate hard-delete of all data | Yes |

---

## 🔒 Security Features

- **JWT Authentication**: Secure stateless authentication for all protected routes.
- **OTP Verification**: Case-insensitive OTP validation for password resets.
- **Secure File Access**: Files are stored in private Supabase buckets; access is granted only via time-limited (15min) signed URLs.
- **Data Privacy**: Support for GDPR-compliant account deactivation and hard-deletion.

## 🐳 Deployment

The project is pre-configured for **Railway** using the included `Dockerfile`.

```bash
# Build & Publish command used by Docker
dotnet publish -c Release -o /app/publish
```
