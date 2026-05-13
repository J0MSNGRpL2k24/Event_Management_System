namespace EventManagementSystem.Domain.Entities;

public enum BookingStatus
{
    PendingPayment,
    Paid,
    Expired,
    Refunded
}