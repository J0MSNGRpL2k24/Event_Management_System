using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Application.Queries.GetEventDetail
{
    public record TicketCategoryDto(
      Guid Id,
      string Name,
      decimal Price,
      int RemainingQuota,
      string Status
  );
}
