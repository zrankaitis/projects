using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Models
{
    public class PointsTransaction
    {
        public string PayerName { get; set; }
        public int Points { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
