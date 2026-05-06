# Event Ticketing & Booking System WEEK 8

## 1. Project Overview
This project is an Event Ticketing & Booking System developed using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The system allows Event Organizers to manage events, Customers to book tickets, Gate Officers to validate check-ins, and System Admins to handle refunds.

*   **Technology Stack:** .NET 8 (C#)
*   **Database:** PostgreSQL (to be implemented)
*   **Architecture:** Clean Architecture with CQRS (Command Query Responsibility Segregation)

## 2. Project Structure (Clean Architecture)
The solution is divided into four main layers to ensure strict separation of concerns, as seen in the source code:

1.  **`EventManagementSystem.Domain`**: The core of the system. Contains `Entities` (Aggregates), `ValueObjects`, `Events`, and `Repositories` (Interfaces only). It has *zero* external dependencies.
2.  **`EventManagementSystem.Application`**: Contains the use cases categorized into `Commands` and `Queries` (CQRS), Data Transfer Objects (`DTOs`), and service `Interfaces`.
3.  **`EventManagementSystem.Infrastructure`**: Contains implementations for database access in `Persistence` (PostgreSQL) and external integrations in `ExternalServices`.
4.  **`EventManagementSystem.Presentation`**: The Web API layer containing `Controllers` to handle HTTP requests.

## 3. Ubiquitous Language Glossary
To ensure clear communication within the problem domain, we use the following terms and their definitions:

| Term | Meaning | System Representation |
| :--- | :--- | :--- |
| **Event** | An activity organized by an Event Organizer and attended by customers. | Aggregate Root |
| **Event Organizer** | A user who creates and manages events. | Actor / User Entity |
| **Customer** | A user who books and purchases tickets. | Actor / User Entity |
| **Gate Officer** | A user who validates tickets during event check-in. | Actor / User Entity |
| **Ticket Category** | A type of ticket, such as Regular, VIP, or Early Bird. | Entity (within Event) |
| **Quota** | The maximum number of tickets available in a ticket category. | Property / Value Object |
| **Booking** | A temporary reservation before payment is completed. | Aggregate Root |
| **Pending Payment** | A booking status indicating that payment has not been completed. | Enumeration (Status) |
| **Paid** | A booking status indicating that payment has been completed. | Enumeration (Status) |
| **Expired** | A booking status indicating that the payment deadline has passed. | Enumeration (Status) |
| **Ticket** | Proof of attendance generated after a booking is paid. | Entity (within Booking) |
| **Ticket Code** | A unique code used to identify and validate a ticket. | Value Object / Identifier |
| **Check-in** | The process of validating a ticket at the venue. | Domain Event / Process |
| **Refund** | The process of returning money to a customer. | Aggregate Root / Entity |
| **Money** | A value object representing an amount and currency. | Value Object |
| **Sales Period** | The period during which a ticket category can be purchased. | Value Object (Date Range) |
| **Payment Deadline** | The deadline for completing payment after a booking is created. | Property / Value Object |
## 4. Initial Domain Model & Business Rules
The following core business rules (invariants) are enforced within the Domain Layer to ensure system integrity:

*   **Event & Ticket Logic:** 
    *   Event end date must be after its start date, and capacity must be > 0.
    *   Total quota of all ticket categories must not exceed the event's maximum capacity.
    *   Ticket prices must be ≥ 0 and sales periods must end before the event starts.
*   **Booking Constraints:** 
    *   Customers are limited to one active booking per event.
    *   Booking quantity must be > 0 and cannot exceed the remaining category quota.
*   **Payment & Expiry:** 
    *   Payment amounts must exactly match the total booking price to confirm tickets.
    *   Unpaid bookings automatically expire and release reserved quota after 15 minutes.
*   **Refund & Check-in:** 
    *   Refunds are prohibited if any ticket in the booking has already been checked-in.
    *   Check-in is only permitted during the allowed event time window and for active tickets.

## 5. Domain Events
The system utilizes Domain Events to decouple side effects and support an Event-Driven approach:

*   **Event Management:** `EventCreated`, `EventPublished`, `EventCancelled`.
*   **Ticketing:** `TicketCategoryCreated`, `TicketCategoryDisabled`, `TicketCheckedIn`.
*   **Booking:** `TicketReserved`, `BookingPaid`, `BookingExpired`.
*   **Refunds:** `RefundRequested`, `RefundApproved`, `RefundRejected`, `RefundPaidOut`.
