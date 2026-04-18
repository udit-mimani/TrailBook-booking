# TrailBook - Travel Booking Platform

A backend API for managing travel bookings with payment processing, refund policies, and admin visibility.

## Tech Stack
- **.NET 8.0 (LTS)** - Modern, cross-platform framework
- **PostgreSQL 16** - Reliable ACID-compliant database with strong concurrency support
- **Entity Framework Core 8** - Type-safe ORM with migration support

## 📋 Features

- ✅ Trip management and booking
- ✅ Payment webhook processing with idempotency
- ✅ Refund policy enforcement
- ✅ Concurrency control to prevent overbooking
- ✅ Admin metrics and analytics
- ✅ Background job for auto-expiring pending bookings

## 🏗️ Architecture

### Project Structure
```
TrailBook-booking/
├── TrailBook.Api/              # REST API layer with controllers
├── TrailBook.Domain/           # Domain models and business logic
├── TrailBook.Infrastructure/   # Data access and repositories
└── TrailBook.Tests/            # Unit and integration tests (tests will be added in future)
```

### Design Patterns
- **Repository Pattern** - Abstracts data access
- **Service Layer** - Encapsulates business logic
- **Background Services** - Handles periodic tasks (expiry)

## 🔧 Getting Started
### Prerequisites

- Docker & Docker Compose
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local development)
- [PostgreSQL 16](https://www.postgresql.org/download/) (Installed locally)
- [pgAdmin](https://www.pgadmin.org/download/) (Optional - for database management)

### Run with Docker
```bash
# Clone repository
git clone https://github.com/udit-mimani/TrailBook-booking.git
cd TrailBook-booking

# Start all services
docker-compose up -d

# Check logs
docker-compose logs -f api

# Access API
curl http://localhost:5000/api/trips
```

For Podman
```bash
# Initialize the Machine
podman machine init   # Run only the first time
podman machine start  # Run every time you want to use Podman

# Navigate to the project and start all services
podman compose up --build

# Common commands:
## Check Containers:
podman ps
## Check logs:
podman logs trailbook-api
```

The API will be available at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### Run Locally

#### Database Setup

- Connect to PostgreSQL using pgAdmin
- Create database and user
CREATE DATABASE trailbook_db;
CREATE USER trailbook_user WITH PASSWORD 'trailbook_password';
GRANT ALL PRIVILEGES ON DATABASE trailbook_db TO trailbook_user;

#### Application Setup
```bash
# Clone the repository
git clone https://github.com/udit-mimani/TrailBook-booking.git
cd TrailBook-booking

# Restore dependencies
dotnet restore

# Update database connection string in appsettings.json if needed
# Default: Host=localhost;Port=5432;Database=trailbook_db;Username=trailbook_user;Password=trailbook_password

# Apply database migrations
dotnet ef database update --project TrailBook.Infrastructure --startup-project TrailBook.Api

# Run the application
dotnet run --project TrailBook.Api

# Access Swagger UI for API documentation:
```

## 🔒 Concurrency & Data Consistency

### Preventing Overbooking

We use **pessimistic locking** with PostgreSQL row-level locks:
```csharp
// Acquire FOR UPDATE lock on trip row
var trip = await _tripRepository.GetByIdAsync(tripId, forUpdate: true);
// SQL: SELECT * FROM "Trips" WHERE "Id" = @id FOR UPDATE
```

This ensures only one booking can reserve seats at a time, preventing race conditions.

### Webhook Idempotency

Payment webhooks are deduplicated using unique `idempotencyKey`:

- Database unique constraint prevents duplicate processing
- Always returns 200 OK to payment provider
- Logs duplicate attempts for monitoring

### Auto-Expiry

Background service runs every minute to expire bookings:
- Bookings in `PENDING_PAYMENT` state older than 15 minutes
- Automatically transitions to `EXPIRED` state
- Releases reserved seats back to availability

## 💰 Refund Policy

**Formula:**
```
daysUntilTrip = trip.StartDate - Today
if (daysUntilTrip <= refundableUntilDaysBefore):
    refund = 0
else:
    refund = priceAtBooking × (1 - cancellationFeePercent / 100)
```

**Example:**
- Booking: $100
- Cancellation fee: 10%
- Cutoff: 7 days before trip
- Cancelled 10 days before → **Refund: $90**
- Cancelled 5 days before → **Refund: $0**

## 📚 API Documentation

### Trips

**List all published trips**
```http
GET /api/trips
```

**Get trip details**
```http
GET /api/trips/{tripId}
```

**Create trip** (Admin)
```http
POST /api/trips
Content-Type: application/json

{
  "title": "Paris City Tour",
  "destination": "Paris, France",
  "startDate": "2026-02-15T00:00:00Z",
  "endDate": "2026-02-18T00:00:00Z",
  "price": 100.00,
  "maxCapacity": 20,
  "refundPolicy": {
    "refundableUntilDaysBefore": 7,
    "cancellationFeePercent": 10
  }
}
```

### Bookings

**Book a trip**
```http
POST /api/trips/{tripId}/book
Content-Type: application/json

{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "numSeats": 2
}

Response: 200 OK
{
  "bookingId": "456e7890-e89b-12d3-a456-426614174001",
  "state": "PendingPayment",
  "priceAtBooking": 200.00,
  "expiresAt": "2026-01-25T10:30:00Z",
  "paymentUrl": "https://payment-provider.example.com/pay/..."
}
```

**Get booking status**
```http
GET /api/bookings/{bookingId}
```

**Cancel booking**
```http
POST /api/bookings/{bookingId}/cancel

Response: 200 OK
{
  "bookingId": "...",
  "state": "Cancelled",
  "refundAmount": 180.00,
  "cancelledAt": "2026-01-25T10:15:00Z"
}
```

### Payments

**Payment webhook** (Called by payment provider)
```http
POST /api/payments/webhook
Content-Type: application/json

{
  "bookingId": "456e7890-e89b-12d3-a456-426614174001",
  "status": "success",
  "idempotencyKey": "webhook-unique-key-123"
}
```

### Admin

**Get trip metrics**
```http
GET /api/admin/trips/{tripId}/metrics
```

**Get at-risk trips**
```http
GET /api/admin/trips/at-risk
```

## 🐛 Debugging & Issues Found

### Bug 1: Race Condition in Seat Reservation
**Symptom**: Overbooking when two users book simultaneously
**Fix**: Added `FOR UPDATE` row-level lock in repository

### Bug 2: Incorrect Refund Calculation
**Symptom**: Refunds calculated on current price instead of booking price
**Fix**: Use `PriceAtBooking` instead of `Trip.Price`

### Bug 3: Seats Not Released on Cancellation
**Symptom**: Available seats not incremented after cancellation
**Fix**: Added `trip.ReleaseSeats()` in cancellation flow

## 🚀 Future Enhancements

- [ ] Docker containerization for easier deployment
- [ ] CI/CD pipeline with GitHub Actions
- [ ] API authentication and authorization
- [ ] Email notifications for bookings and cancellations
- [ ] Payment provider integration (Stripe/PayPal)
- [ ] Advanced analytics dashboard

## 👤 Author

**Udit Mimani**
- GitHub: [@udit-mimani](https://github.com/udit-mimani)

## 📄 License

This project is for educational/interview purposes.
