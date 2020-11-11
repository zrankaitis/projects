using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application
{
    public interface IPointsRepository
    {
        IEnumerable<PointsTransaction> GetPointsTransactionsByUserId(string userId);
        void UpdatePointsBalanceByUserId(string userId, IEnumerable<PointsTransaction> balances);
    }
}
