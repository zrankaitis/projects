using Microsoft.Extensions.Caching.Memory;
using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application
{
    public class PointsRepository : IPointsRepository
    {
        private IMemoryCache Cache { get; set; }

        public PointsRepository(IMemoryCache cache)
        {
            Cache = cache;
        }

        public IEnumerable<PointsTransaction> GetPointsTransactionsByUserId(string userId)
        {
            string key = String.Format(CacheKeys.PointsTransactionsByUserId, userId);

            var points = (IEnumerable<PointsTransaction>)Cache.Get(key);

            return points;
        }

        public void UpdatePointsBalanceByUserId(string userId, IEnumerable<PointsTransaction> balances)
        {
            string key = String.Format(CacheKeys.PointsTransactionsByUserId, userId);
            Cache.Set(key, balances, DateTimeOffset.UtcNow.AddYears(100));
        }
    }
}
