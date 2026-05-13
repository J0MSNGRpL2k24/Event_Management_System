namespace EventManagementSystem.Application.Queries.GetEventDetail;

public record EventDetailDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    string Location,
    string OrganizerName,
    List<TicketCategoryDto> Categories
);

