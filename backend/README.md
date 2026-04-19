# PetMarketplace — Backend API

RESTful Web API for the PetMarketplace platform, built with **ASP.NET Core 8** following **Clean Architecture**. Serves both the customer frontend and the admin panel.

**Live API:** https://pet-market-place.onrender.com  
**Swagger Docs:** https://pet-market-place.onrender.com/swagger

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Language | C# |
| Database | PostgreSQL (via Supabase) |
| ORM | Entity Framework Core 8 + Npgsql |
| Auth | JWT Bearer tokens |
| Password Hashing | BCrypt.Net-Next |
| Image Storage | Cloudinary |
| API Docs | Swagger / Swashbuckle |
| Containerisation | Docker |

---

## Architecture

Clean Architecture with four layers:

```
PetMarketplace.sln
├── PetMarketplace.Server/         # Web API layer
│   ├── Controllers/               # HTTP endpoints
│   ├── Middleware/                # Global exception handler
│   ├── Extensions/                # Service registration helpers
│   ├── Program.cs
│   └── appsettings.json
├── PetMarketplace.Application/    # Business logic
│   ├── Services/                  # Application services
│   ├── Interfaces/                # Service contracts
│   └── DTOs/                      # Request / response models
├── PetMarketplace.Infrastructure/ # Data & external services
│   ├── Persistence/               # AppDbContext, EF config
│   ├── Migrations/                # EF Core migrations
│   └── Services/                  # Cloudinary, email, etc.
└── PetMarketplace.Core/           # Domain
    ├── Entities/                  # Domain models
    └── Enums/                     # UserRole, ListingStatus, Species, Gender
```

---

## Domain Enums

| Enum | Values |
|---|---|
| `UserRole` | `Buyer`, `Seller`, `Admin` |
| `ListingStatus` | `Draft`, `PendingApproval`, `Active`, `Rejected`, `Sold` |
| `Species` | `Dog`, `Cat`, `Bird`, `Fish`, `Rabbit`, `Other` |
| `Gender` | `Male`, `Female` |

---

## API Endpoints

All endpoints return a standard envelope:

```json
{
  "success": true,
  "data": {},
  "message": "Optional message",
  "errors": []
}
```

### Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | — | Register as Buyer or Seller |
| POST | `/api/auth/login` | — | Login, returns JWT |
| GET | `/api/auth/me` | Bearer | Get current user profile |

### Listings — `/api/listings`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/listings` | — | Browse active listings (filterable) |
| GET | `/api/listings/{id}` | — | Get listing detail |
| GET | `/api/listings/my-listings` | Seller | Get seller's own listings |
| POST | `/api/listings` | Seller | Create listing (Draft) |
| PUT | `/api/listings/{id}` | Seller | Update listing |
| DELETE | `/api/listings/{id}` | Seller/Admin | Delete listing |
| POST | `/api/listings/{id}/submit` | Seller | Submit for approval |
| POST | `/api/listings/{id}/images` | Seller | Upload image (multipart/form-data) |
| DELETE | `/api/listings/{id}/images/{imageId}` | Seller | Delete image |
| PUT | `/api/listings/{id}/images/{imageId}/set-main` | Seller | Set main image |

**Listing filters (query params):** `species`, `breed`, `gender`, `city`, `minPrice`, `maxPrice`, `isVaccinated`, `sortBy` (`Newest` / `PriceLow` / `PriceHigh` / `MostViewed`), `pageNumber`, `pageSize`

### Favorites — `/api/favorites`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/favorites` | Buyer | Get user's favorites (paged) |
| POST | `/api/favorites/{listingId}` | Buyer | Add to favorites |
| DELETE | `/api/favorites/{listingId}` | Buyer | Remove from favorites |
| GET | `/api/favorites/{listingId}/check` | Buyer | Check if listing is favorited |

### Inquiries — `/api/inquiries`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/inquiries/listings/{listingId}` | Buyer | Start an inquiry |
| GET | `/api/inquiries/my-inquiries` | Buyer | Get buyer's inquiries |
| GET | `/api/inquiries/listings/{listingId}` | Seller | Get all inquiries for a listing |
| GET | `/api/inquiries/{inquiryId}` | Bearer | Get inquiry details |
| POST | `/api/inquiries/{inquiryId}/messages` | Bearer | Send a message |

### Reviews — `/api/reviews`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/reviews/sellers/{sellerId}` | — | Get seller's reviews |
| POST | `/api/reviews/sellers/{sellerId}` | Buyer | Leave a review (rating + comment) |

### Notifications — `/api/notifications`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/notifications` | Bearer | Get user notifications |
| GET | `/api/notifications/unread-count` | Bearer | Get unread count |
| PUT | `/api/notifications/{id}/read` | Bearer | Mark one as read |
| PUT | `/api/notifications/read-all` | Bearer | Mark all as read |

### Admin — `/api/admin` *(Admin role required)*

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/admin/dashboard` | Platform stats overview |
| GET | `/api/admin/listings` | All listings (filter by status) |
| GET | `/api/admin/listings/pending` | Listings awaiting approval |
| POST | `/api/admin/listings/{id}/approve` | Approve a listing |
| POST | `/api/admin/listings/{id}/reject` | Reject with reason |
| GET | `/api/admin/users` | All users (filter by role) |
| POST | `/api/admin/users/{id}/verify-seller` | Verify a seller |
| POST | `/api/admin/users/{id}/ban` | Ban a user |
| POST | `/api/admin/users/{id}/unban` | Unban a user |

---

## Available Commands

| Command | Description |
|---|---|
| `dotnet build` | Build the solution |
| `dotnet run --project PetMarketplace.Server/...` | Run the API |
| `dotnet ef migrations add <Name> ...` | Add a migration |
| `dotnet ef database update ...` | Apply migrations |
| `dotnet test` | Run tests |

---

## Deployment

The API is deployed on **Render** using Docker. All configuration values are set as environment variables in the Render dashboard. Health check endpoint: `GET /health`.
