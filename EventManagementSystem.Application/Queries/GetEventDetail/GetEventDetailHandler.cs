using EventManagementSystem.Domain.Repositories;
using MediatR;

namespace EventManagementSystem.Application.Queries.GetEventDetail;

public class GetEventDetailHandler : IRequestHandler<GetEventDetailQuery, EventDetailDto?>
{
    private readonly IEventRepository _eventRepository;

    public GetEventDetailHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventDetailDto?> Handle(GetEventDetailQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);

        if (@event == null) return null;

        var now = DateTime.UtcNow;

        var categoriesDto = @event.Categories
            .Where(c => c.IsActive)
            .Select(c => {
                string status = "Available";

                if (c.RemainingQuota <= 0)
                    status = "Sold Out";
                else if (now < c.SalesStart)
                    status = "Coming Soon";
                else if (now > c.SalesEnd)
                    status = "Sales Closed";

                return new TicketCategoryDto(
                    c.Id,
                    c.Name,
                    c.Price.Amount,
                    c.RemainingQuota,
                    status
                );
            }).ToList();

        return new EventDetailDto(
            @event.Id,
            @event.Name,
            @event.Description,
            @event.StartDate,
            @event.Location,
            "Event Organizer",
            categoriesDto
        );
    }
}