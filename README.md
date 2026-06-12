# Week 13: Presentation Layer & API Integration

## Overview
This week, our focus shifted to the **Presentation Layer**, which serves as the entry point for external clients to interact with the Event Management System. We implemented a RESTful Web API using ASP.NET Core, strictly adhering to the principles of **Clean Architecture**.

The Presentation Layer contains absolutely no business logic or direct database access. Instead, it utilizes the **CQRS (Command and Query Responsibility Segregation)** pattern via **MediatR** to dispatch incoming HTTP requests as Commands or Queries to the Application Layer. This ensures that the Web API remains a thin, lightweight routing mechanism.

## Key Accomplishments & Features

### 1. RESTful API Controllers
We designed and implemented specific controllers to handle distinct aggregate roots and operations within the system:
* **`EventsController`**: Manages the complete event lifecycle (Create, Add Categories, Publish, Cancel) and serves queries for available events and detailed sales reports (US 1-7, US 19-20).
* **`BookingsController`**: Handles ticket reservations, calculates totals, and processes payment confirmations with strict 15-minute deadline validations (US 8-11).
* **`TicketsController`**: Manages ticket visibility for customers and facilitates secure, gate-level ticket scanning/check-in operations for gate officers (US 12-14).
* **`RefundsController`**: Provides endpoints for the end-to-end refund workflow, including customer requests, organizer approvals/rejections, and admin payout confirmations (US 15-18).

### 2. MediatR Integration & CQRS
* Completely decoupled the HTTP request handling from the application logic. Each controller endpoint simply constructs a Command (e.g., `ApproveRefundCommand`) or a Query (e.g., `ViewEventSalesReportQuery`) and sends it through the MediatR pipeline.
* Mapped MediatR handler responses directly to appropriate HTTP Status Codes (e.g., `200 OK`, `204 No Content`).

### 3. Data Transfer Objects (DTOs) & Route Management
* Implemented specific request DTOs (e.g., `RejectRefundDto`, `PayoutRefundDto`) to cleanly capture payload data from the HTTP Body without conflicting with URL path parameters (`{id}`).
* Designed clean, predictable, and resource-oriented URI paths (e.g., `POST /api/refunds/{id}/approve`, `GET /api/events/{id}/participants`).

### 4. Exception Handling
* Implemented structured `try-catch` blocks within the controllers to catch Domain-level exceptions (e.g., "The refund deadline has passed") and Application-level exceptions (e.g., "Event not found").
* Mapped these internal exceptions to appropriate standard HTTP responses (returning `400 Bad Request` or `404 Not Found` with clear error messages) to ensure a secure and informative client experience.

## Technologies Used
* **.NET 9.0**
* **ASP.NET Core Web API**
* **MediatR** (for CQRS implementation)
* **Swagger / OpenAPI** (for API documentation and testing)

## API DOCUMENTATION
* [Event Management System API Documentation.pdf](./docs/Event%20Management%20System%20API%20Documentation.pdf)

## How to Run & Test the API

1. Ensure your PostgreSQL database is running and migrations are up to date.
2. Build and run the Presentation layer:

```bash
cd EventManagementSystem.Presentation
dotnet run
