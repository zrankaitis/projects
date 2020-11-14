using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application.Data
{
    public interface IPointsTransactionRepository
    {
        void CreateSchema();
        IEnumerable<PointsTransaction> GetPointsTransactionsByUserId(string userId);
        PointsTransaction GetPointsTransactionById(int id);
        void InsertPointsTransaction(PointsTransaction transaction);
        void InsertPointsTransactions(IEnumerable<PointsTransaction> transactions);
        void UpdatePointsTransactions(IEnumerable<PointsTransaction> transactions);
        void UpdatePointsTransaction(PointsTransaction transaction);
    }
}
