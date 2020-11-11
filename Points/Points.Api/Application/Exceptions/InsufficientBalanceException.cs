using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException() : base("User does not have a large enough balance.") { }
    }
}
