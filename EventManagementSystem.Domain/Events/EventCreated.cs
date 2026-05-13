using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Domain.Events
{
    public record EventCreated(Guid EventId, string Name) : IDomainEvent;
}
