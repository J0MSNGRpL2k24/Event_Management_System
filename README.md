# WEEK 9 - Event Management System


### . Domain-Driven Design (DDD)
- **Aggregates:** `Event` and `Booking` serve as consistency boundaries.
- **Value Objects:** `Money` ensures financial integrity (non-negative constraints).
- **Domain Events:** Decouples side effects across the system.

---

## ✅ Implemented User Stories

As of this week, we have implemented **10 User Stories** covering the core domain logic:

### Event Management
- [x] [cite_start]**US-1: Create Event** – Capability to create events in a *Draft* status[cite: 64, 70].
- [x] [cite_start]**US-2: Publish Event** – Logic to transition status to *Published* with validation requiring at least one active ticket category[cite: 71, 74].
- [x] [cite_start]**US-3: Cancel Event** – Ability to cancel events, which automatically deactivates all associated ticket categories[cite: 81, 86].

### Ticket Category Management
- [x] [cite_start]**US-4: Create Ticket Category** – Support for defining ticket tiers (VIP, Regular, etc.) with specific quotas and sales periods[cite: 90, 93, 94].
- [x] [cite_start]**US-5: Disable Ticket Category** – Logic to stop sales for specific categories while maintaining data for historical purposes[cite: 100, 105].

### Event Browsing & Booking
- [x] [cite_start]**US-6: View Available Events** – Query logic to list only *Published* events with filtering options[cite: 108, 111].
- [x] [cite_start]**US-7: View Event Details** – Detailed query displaying event info and dynamic ticket status (e.g., *Sold Out*, *Coming Soon*)[cite: 115, 120, 122].
- [x] [cite_start]**US-8: Create Ticket Booking** – Implementation of the reservation aggregate with an automatic 15-minute payment deadline[cite: 124, 139].
- [x] [cite_start]**US-9: Calculate Booking Total Price** – Automated pricing logic including unit prices, quantities, and service fees through the `Money` Value Object[cite: 141, 144, 145].

### Ticket & Check-in Management
- [x] [cite_start]**US-13: Ticket Check-in** – Gate officer validation logic to ensure active tickets are only used once for event entry[cite: 175, 179, 180].

---

## 📢 Implemented Domain Events
[cite_start]The following domain events have been successfully implemented to trigger side effects[cite: 283]:
1. [cite_start]`EventCreated` [cite: 71]
2. [cite_start]`EventPublished` [cite: 80]
3. [cite_start]`EventCancelled` [cite: 88]
4. [cite_start]`TicketCategoryCreated` [cite: 99]
5. [cite_start]`TicketCategoryDisabled` [cite: 106]
6. [cite_start]`TicketReserved` [cite: 140]
7. [cite_start]`BookingPaid` [cite: 157]
8. [cite_start]`BookingExpired` [cite: 166]
9. [cite_start]`TicketCheckedIn` [cite: 184]

