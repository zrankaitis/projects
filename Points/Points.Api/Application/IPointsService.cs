using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application
{
    public interface IPointsService
    {
        IEnumerable<PointsSummary> GetPointsSummaries(string userId);
        void AddPoints(string userId, PointsTransaction transaction);
        IEnumerable<PointsTransaction> DeletePoints(string userId, int amount);
    }
}
