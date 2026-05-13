using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Domain.ValueObjects;
using MediatR;

namespace EventManagementSystem.Application.Commands.CreateTicketCategory;

public class CreateTicketCategoryCommandHandler : IRequestHandler<CreateTicketCategoryCommand, Guid>
{
    private readonly IEventRepository _eventRepository;

    public CreateTicketCategoryCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(CreateTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null) throw new Exception("Event not found.");

        var price = new Money(request.PriceAmount);

        @event.AddTicketCategory(
            request.Name,
            price,
            request.Quota,
            request.SalesStart,
            request.SalesEnd
        );

        await _eventRepository.SaveAsync(@event);

        return @event.Categories.Last().Id;
    }
}