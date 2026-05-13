using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Application.Queries.GetAvailableEvents
{
    public record EventSummaryDto(
     Guid Id,
     string Name,
     DateTime Date,
     string Location,
     decimal LowestTicketPrice
 );
}
