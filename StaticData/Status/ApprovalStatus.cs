using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator.StaticData.Status
{
    public static class ApprovalStatus
    {
        public static readonly string Pending = "Pending";
        public static readonly string Billed = "Billed";
        public static readonly string Returned = "Returned";
    }
}
