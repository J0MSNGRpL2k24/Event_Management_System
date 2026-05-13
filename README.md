# Event Ticketing & Booking System WEEk 8

- **Aggregates:** `Event` and `Booking` serve as consistency boundaries.
- **Value Objects:** `Money` ensures financial integrity (non-negative constraints).
- **Domain Events:** Decouples side effects across the system.

---

## ✅ Implemented User Stories

As of this week, we have implemented **10 User Stories** covering the core domain logic:

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

## The rest of the user stories will be implemented in the next week, along with additional domain events and integration tests to ensure system robustness. 

## 5. Domain Events
The system utilizes Domain Events to decouple side effects and support an Event-Driven approach:

*   **Event Management:** `EventCreated`, `EventPublished`, `EventCancelled`.
*   **Ticketing:** `TicketCategoryCreated`, `TicketCategoryDisabled`, `TicketCheckedIn`.
*   **Booking:** `TicketReserved`, `BookingPaid`, `BookingExpired`.
*   **Refunds:** `RefundRequested`, `RefundApproved`, `RefundRejected`, `RefundPaidOut`.
