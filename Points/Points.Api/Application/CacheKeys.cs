using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application
{
    public static class CacheKeys
    {
        public static string PointsTransactionsByUserId {  get { return "GetPointsBalanceByUserId.{0}"; } }
    }
}
