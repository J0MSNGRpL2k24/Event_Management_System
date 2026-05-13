using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace EventManagementSystem.Application.Commands.CreateEvent
{
    public record CreateEventCommand(
     Guid OrganizerId,
     string Name,
     string Description,
     DateTime StartDate,
     DateTime EndDate,
     string Location,
     int MaxCapacity
 ) : IRequest<Guid>;
}
