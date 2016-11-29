using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingService2
{
    public class BrokeredMessage
    {
        public Guid Id { get; set; }
        public object Payload { get; set; }
    }
}
