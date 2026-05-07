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

<img width="519" height="580" alt="image" src="https://github.com/user-attachments/assets/eb2c42f9-2c27-464e-ae76-3a787a92de92" />


    

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
## 4. Business Rules
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

## 5. Domain Model Draft

This section outlines the tactical DDD patterns used to model the business logic within the `EventManagementSystem.Domain` layer.

### 5.1 Aggregates & Entities
Aggregates serve as consistency boundaries for our business rules.

* **Event [Aggregate Root]**
    * `id`: EventId
    * `organizerId`: UserId
    * `name`: string
    * `description`: string
    * `startDate`: Date
    * `endDate`: Date
    * `location`: string
    * `maxCapacity`: int
    * `status`: EventStatus (Draft, Published, Cancelled, Completed)
    * `categories`: TicketCategory[]

* **TicketCategory [Entity]**
    * `id`: CategoryId
    * `eventId`: EventId
    * `name`: string
    * `price`: Money
    * `quota`: int
    * `remainingQuota`: int
    * `salesStart`: Date
    * `salesEnd`: Date 
    * `isActive`: bool

* **Booking [Aggregate Root]**
    * `id`: BookingId
    * `customerId`: UserId
    * `eventId`: EventId
    * `categoryId`: CategoryId
    * `quantity`: int 
    * `totalPrice`: Money 
    * `status`: BookingStatus (PendingPayment, Paid, Expired, Refunded) 
    * `paymentDeadline`: DateTime 
    * `tickets`: Ticket[]

* **Ticket [Entity]**
    * `id`: TicketId
    * `bookingId`: BookingId
    * `code`: TicketCode
    * `status`: TicketStatus (Active, CheckedIn, Cancelled) 
    * `checkedInAt`: DateTime? 

* **Refund [Aggregate Root]**
    * `id`: RefundId
    * `bookingId`: BookingId
    * `customerId`: UserId
    * `amount`: Money
    * `status`: RefundStatus (Requested, Approved, Rejected, PaidOut) 
    * `reason`: string?
    * `rejectionReason`: string?
    * `paymentReference`: string?
    * `requestedAt`: DateTime

### 5.2 Value Objects
Value objects are defined by their attributes and have no identity.

* **Money**
    * `amount`: Decimal
    * `currency`: string
    * *Methods*: `add(Money)`, `multiply(int)`, `isNegative()`.
* **TicketCode**
    * `value`: string (UUID)
    * *Methods*: `generate()`, `equals(other)`.
* **EventId / BookingId**
    * `value`: UUID
    * *Methods*: `equals(other)`.

### 5.3 Domain Events
Events are triggered by state changes within the domain.

* **Event Management**: `EventCreated`, `EventPublished`, `EventCancelled`.
* **Ticketing**: `TicketCategoryCreated`, `TicketCategoryDisabled`, `TicketCheckedIn`.
* **Booking**: `TicketReserved`, `BookingPaid`, `BookingExpired`.
* **Refunds**: `RefundRequested`, `RefundApproved`, `RefundRejected`, `RefundPaidOut`.

### 5.4 Repository Interfaces
These interfaces reside in the Domain layer and are implemented in the Infrastructure layer.

* **IEventRepository**: `findById(id)`, `findPublished()`, `save(event)`.
* **IBookingRepository**: `findById(id)`, `findByCustomerAndEvent()`, `findPendingExpired()`, `save(booking)`.
* **ITicketRepository**: `findByCode(code)`, `save(ticket)`.
* **IRefundRepository**: `findById(id)`, `findByBooking(bookingId)`, `save(refund)`.
