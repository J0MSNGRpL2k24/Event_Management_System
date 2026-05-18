# Progress Report - Week 10: Domain Unit Testing

## 📌 Overview
In Week 10, the primary development focus was directed towards implementing **Domain Unit Testing** using the **xUnit** framework and applying the **AAA (Arrange-Act-Assert)** pattern. All core business invariants and domain event triggers within the Domain layer have been comprehensively validated.

## 🧪 Testing Methodology

All domain unit tests in this project were implemented using the **xUnit** framework. To ensure high code quality, readability, and maintainability, the testing process strictly adhered to the following methodologies:

### 1. The AAA (Arrange-Act-Assert) Pattern
Every single test case is structured using the industry-standard AAA pattern, which divides the test method into three distinct sections:
* **Arrange:** This phase sets up the testing environment. It involves initializing objects, configuring dummy data (using `Guid.NewGuid()` and valid test values), and preparing the exact system state required for the scenario (e.g., creating an Event and a paid Booking before testing a Refund). Helper methods were heavily utilized here to keep the code DRY (Don't Repeat Yourself).
* **Act:** This phase executes the specific business logic or behavior being tested. It typically involves calling a single method on the aggregate root or entity (e.g., `booking.ConfirmPayment()` or `ticket.CheckIn()`).
* **Assert:** This phase verifies that the action produced the expected outcome. It checks if the entity's state changed correctly (e.g., status changed to `Paid`), if the correct exceptions were thrown for invalid operations (`Assert.Throws<Exception>`), and if the appropriate Domain Events were raised and recorded.

### 2. Preserving Domain Encapsulation
A core principle of Domain-Driven Design (DDD) is protecting business invariants by restricting unauthorized state modifications. To test these encapsulated entities without compromising their architecture:
* **Factory Methods:** Entities were instantiated using their official factory methods (e.g., `Event.Create(...)`, `Booking.Create(...)`) rather than direct constructors, ensuring all creation business rules were validated during the *Arrange* phase.
* **Reflection for Internal State:** In scenarios where the domain strictly hides its constructor (e.g., `internal Ticket(...)`) or restricts state changes (e.g., time-dependent properties like `StartDate` or `PaymentDeadline`), **C# Reflection** was strategically used. This allowed the test suite to dynamically instantiate hidden objects or manipulate private time states (simulating past deadlines) without opening up the production code to vulnerabilities.

### 📊 Test Results Summary
* **Total Test Cases:** 37
* **Status:** 🟩 37 Passed (0 Failed)
* **Aggregate Coverage:** `Event`, `TicketCategory`, `Booking`, `Ticket`, and `Refund`.

---

## 🛠️ Implementation Details & Test Coverage

### 1. Event Management (`EventTests.cs` - US 1, 2, 3)
The focus was to ensure data encapsulation via the Factory Method (`Event.Create`) functions correctly and maintains the entity's state transitions.
* **Validation Scenarios:**
  * Newly created Events automatically get the `Draft` status and raise the `EventCreated` domain event.
  * Maximum capacity cannot be less than or equal to zero.
  * End date cannot be earlier than the start date.
  * Publish constraints: The event must be in `Draft` status and have at least one active ticket category to be published.
  * Event cancellation (`Cancel()`) automatically deactivates all related ticket categories and raises `EventCancelled`.

### 2. Ticket Category Management (`TicketCategoryTests.cs` - US 4, 5)
Validated the integration of ticket category creation within the `Event` Aggregate Root boundary.
* **Validation Scenarios:**
  * Ticket category price cannot be negative, and quota must be greater than zero.
  * The ticket sales period must end before or exactly on the event start date.
  * The total accumulated quota of all ticket categories must not exceed the event's maximum capacity.
  * Disabling a category changes its status to inactive and raises `TicketCategoryDisabled`.

### 3. Booking & Payment Management (`BookingTests.cs` - US 8, 9, 10, 11)
Tested the booking lifecycle from initial creation and financial calculations to the payment expiration mechanism.
* **Validation Scenarios:**
  * A new booking automatically gets the `PendingPayment` status with a strict 15-minute payment deadline.
  * Total Price calculation using the `Money` Value Object via the formula: `(Unit Price × Quantity) + Service Fee`.
  * Ticket quantity must be greater than zero.
  * Payment mechanism (`ConfirmPayment()`) validates the exact amount, currency match, and payment deadline. Upon success, the status changes to `Paid`, automatically generating unique physical tickets.
  * Expiration logic (`Expire()`) was validated by simulating time travel using **Reflection** to forcibly bypass the 15-minute payment deadline.

### 4. Ticket & Check-in Management (`TicketTests.cs` - US 13, 14)
Validated Gate check-in functionalities and the authenticity of physical tickets on the event day.
* **Validation Scenarios:**
  * A valid, `Active` ticket can successfully be checked in on the event day.
  * Check-in is strictly rejected if the ticket has already been used (duplicate check-in prevention).
  * Check-in is rejected if the ticket belongs to a different Event ID (mismatched event).
  * Check-in is rejected if the event has been `Cancelled`.
  * Guarantees that the ticket status does not change if the check-in validation fails (atomic state guarantee).

### 5. Refund Management (`RefundTests.cs` - US 15, 16, 17, 18)
Tested complex logic regarding customer refunds based on cancellation policies and deadlines.
* **Validation Scenarios:**
  * A refund can only be requested for bookings with a `Paid` status.
  * A refund request is strictly rejected if any ticket within the booking has already been checked in.
  * Under normal conditions, refunds are bounded by the event's refund deadline (start date).
  * If the Event is cancelled by the organizer, the system automatically bypasses the deadline rule and permits the refund request.
  * The `Approve` workflow changes the refund status to `Approved` and cancels the related tickets.
  * The `Reject` workflow strictly requires a valid rejection reason.
  * The `Paid Out` workflow requires the input of a valid payment reference code.

---

## 📐 Architectural Notes: Command & Query Separation (CQRS)

During the implementation of these Domain-level Unit Tests, several User Stories were intentionally **excluded from domain testing**:
* **US 6:** View Available Events
* **US 7:** View Event Details
* **US 12:** View Purchased Tickets
* **US 19:** View Event Sales Report
* **US 20:** View Event Participants

**Theoretical Reasoning:** The features listed above are pure **Query (GET)** operations responsible for reading data from the infrastructure layer to present to the user. They do not alter the state or business invariants of any domain object. Based on **CQRS** principles, testing these read-only functionalities falls outside the scope of Domain Unit Testing and is better suited for the Application Layer using Integration Testing or Repository Mocking.

---

## ⚡ Advanced Testing Techniques Used
1. **Encapsulation Protection:** Because several entities utilize private constructors or internal modifiers to protect domain integrity, the test project employs dynamic instantiation using `Activator.CreateInstance` and `BindingFlags`. This allows testing without compromising the original code's encapsulation.
2. **Time & State Manipulation:** Simulating booking expirations and post-deadline refund requests was achieved instantaneously using **Reflection** to manipulate private property states (e.g., forcing `DateTime.UtcNow` to a past date) without needing to pause or delay the test thread execution.
