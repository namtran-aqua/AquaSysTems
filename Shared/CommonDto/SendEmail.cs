using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaSolution.Shared.CommonDto
{
    public class SendEmailDto
    {
        public Guid RequestId { get; set; }
        public string? ToAdress { get; set; }
        public string Subject { get; set; } = "System Notification";

        public StringBuilder? Body { get; set; }
    }
}
