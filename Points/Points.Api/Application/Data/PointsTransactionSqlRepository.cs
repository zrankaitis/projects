using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Points.Application.Data
{
    public class PointsTransactionSqlRepository : IPointsTransactionRepository
    {
        protected IDbContext DbContext { get; set; }

        public PointsTransactionSqlRepository(IDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public void CreateSchema()
        {
            // Set up initial DB migration so table schema is in place
            string sql = @"CREATE TABLE PointsTransactions (
                        Id INTEGER PRIMARY KEY,
                        UserId nvarchar(255) NOT NULL,
	                    PayerName nvarchar(255) NOT NULL,
	                    Points Int NOT NULL,
	                    TransactionDate DateTime NOT NULL
                    );";

            DbContext.GetConnection().Execute(sql);
        }

        public IEnumerable<PointsTransaction> GetPointsTransactionsByUserId(string userId)
        {
            string sql = "SELECT * FROM PointsTransactions WHERE UserId = @UserId";

            var transactions = DbContext.GetConnection().Query<PointsTransaction>(sql, new { UserId = userId });

            return transactions;
        }

        public PointsTransaction GetPointsTransactionById(int id)
        {
            string sql = "SELECT * FROM PointsTransactions WHERE Id = @Id";

            var transaction = DbContext.GetConnection().Query<PointsTransaction>(sql, new { Id = id }).FirstOrDefault();

            return transaction;
        }

        public void InsertPointsTransaction(PointsTransaction transaction)
        {
            DbContext.GetConnection().Insert(transaction);
        }

        public void InsertPointsTransactions(IEnumerable<PointsTransaction> transactions)
        {
            DbContext.GetConnection().Insert(transactions);
        }

        public void UpdatePointsTransactions(IEnumerable<PointsTransaction> balances)
        {
            DbContext.GetConnection().Update(balances);
        }

        public void UpdatePointsTransaction(PointsTransaction balance)
        {
            DbContext.GetConnection().Update(balance);
        }
    }
}
