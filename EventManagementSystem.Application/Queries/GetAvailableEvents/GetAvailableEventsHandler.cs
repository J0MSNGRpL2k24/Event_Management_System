using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Application.Queries.GetAvailableEvents
{
    public class GetAvailableEventsHandler : IRequestHandler<GetAvailableEventsQuery, List<EventSummaryDto>>
    {
        private readonly IEventRepository _repo;
        public GetAvailableEventsHandler(IEventRepository repo) => _repo = repo;

        public async Task<List<EventSummaryDto>> Handle(GetAvailableEventsQuery request, CancellationToken ct)
        {
            var events = await _repo.GetAllAsync();

            return events
                .Where(e => e.Status == EventStatus.Published) // AC: Only Published
                .Where(e => !request.FilterDate.HasValue || e.StartDate.Date == request.FilterDate.Value.Date) // Filter Date
                .Where(e => string.IsNullOrEmpty(request.FilterLocation) || e.Location.Contains(request.FilterLocation)) // Filter Loc
                .Select(e => new EventSummaryDto(
                    e.Id,
                    e.Name,
                    e.StartDate,
                    e.Location,
                    e.Categories.Any() ? e.Categories.Min(c => c.Price.Amount) : 0 // AC: Lowest Price
                )).ToList();
        }
    }
}
