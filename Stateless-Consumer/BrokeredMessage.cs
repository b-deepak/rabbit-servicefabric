using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless_Consumer
{
    public class BrokeredMessage
    {
        public Guid Id { get; set; }
        public object Payload { get; set; }
    }
}
