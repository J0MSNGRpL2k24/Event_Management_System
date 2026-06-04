# Week 12: Infrastructure Layer Implementation (Data Persistence)

## Overview
This week, our focus was entirely on establishing the **Infrastructure Layer** for the Event Management System. We integrated **Entity Framework Core (EF Core)** with **PostgreSQL** to handle data persistence, ensuring that all business rules and invariants defined within the Domain Layer remain isolated and uncompromised.

By adhering to the principles of **Clean Architecture** and **Domain-Driven Design (DDD)**, the Infrastructure layer solely implements the repository interfaces defined in the Domain layer, acting as an adapter rather than dictating the business logic.

## Key Accomplishments & Features

### 1. Database Context & Fluent API Configuration
We successfully configured the `AppDbContext` to map our rich Domain Models into relational database tables using the Fluent API.
* **Value Object Mapping:** Mapped the `Money` Value Object using `.OwnsOne()`, splitting it into `Amount` and `Currency` columns across relevant tables (`Events`, `TicketCategories`, `Bookings`, `Refunds`).
* **Enum Conversions:** Configured enums (e.g., `BookingStatus`, `TicketStatus`, `RefundStatus`) to be stored as readable strings in the database using `.HasConversion<string>()`.
* **Private Constructors for EF Core:** Added parameterless private constructors to entities (e.g., `TicketCategory`) to allow EF Core to materialize objects from the database without breaking domain encapsulation.
* **Ignoring Domain Events:** Explicitly ignored `DomainEvents` in the EF Core model builder (`.Ignore(e => e.DomainEvents)`) to prevent them from being mapped as database columns.

### 2. Repository Implementations
We implemented the concrete repositories inside the `Infrastructure/Repositories` directory, strictly adhering to the contracts defined by the Domain's `IRepository` interfaces.
* **`EventRepository`**: Handles fetching and persisting Events along with their child entities (`TicketCategories`).
* **`BookingRepository`**: Manages the persistence of Customer Bookings. Leverages EF Core's `.Include(b => b.Tickets)` to eagerly load related tickets. Implemented a specific query for batch processing (`MarkBookingsForRefundAsync`).
* **`TicketRepository`**: Optimized for Gate Officers by implementing `GetByCodeAsync` for rapid Check-In operations, supported by a unique index constraint on the `TicketCode`.
* **`RefundRepository`**: Manages the state of refund requests related to cancelled events or customer claims.

### 3. Leveraging EF Core Change Tracking
Instead of creating separate repositories for child entities (like `TicketCategory` or `Ticket`), we utilized EF Core's built-in **Change Tracking**. 
When the Application layer executes behaviors on the Aggregate Root (e.g., `booking.ConfirmPayment()` which generates new `Ticket` entities), simply calling `_bookingRepository.SaveAsync(booking)` commands EF Core to automatically track the new children and execute the necessary `INSERT` and `UPDATE` statements atomically.

### 4. Code-First Database Migrations
Successfully generated the initial database schema reflecting the complete Domain Model.
* Generated migration: `CompleteDomainSchema`
* Connected the `Presentation` (API) layer to the `Infrastructure` layer via Dependency Injection in `Program.cs`.

## Technologies Used
* **.NET 9.0**
* **Entity Framework Core 9.0** (`Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Relational`)
* **Npgsql** (PostgreSQL Provider for EF Core)

## How to Run Migrations
To apply the migrations and update your local PostgreSQL database, ensure your database connection string is properly set in the `EventManagementSystem.Presentation/appsettings.json` file, then run:

```bash
# To add a new migration (if domain models change)
dotnet ef migrations add <MigrationName> --project EventManagementSystem.Infrastructure --startup-project EventManagementSystem.Presentation

# To update the database schema
dotnet ef database update --project EventManagementSystem.Infrastructure --startup-project EventManagementSystem.Presentation
