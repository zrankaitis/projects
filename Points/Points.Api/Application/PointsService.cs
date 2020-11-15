using Points.Application.Data;
using Points.Application.Exceptions;
using Points.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application
{
    public class PointsService : IPointsService
    {
        private IPointsTransactionRepository Repository { get; set; }

        public PointsService(IPointsTransactionRepository repository)
        {
            Repository = repository;
        }

        public void AddPoints(string userId, PointsTransaction transaction)
        {
            if (transaction.Points == 0)
                return;

            if (transaction.Points > 0)
                Repository.InsertPointsTransaction(transaction);
            else
            {
                // Actually a deduction operation
                int totalDeduction = -transaction.Points;

                var transactions = Repository.GetPointsTransactionsByUserId(userId)
                    .Where(b => b.PayerName == transaction.PayerName)
                    .OrderBy(b => b.TransactionDate).ToArray();

                int balanceIndex = 0;
                while (totalDeduction > 0)
                {
                    // Potential for refactoring here as this is largely the same logic as DeletePoints()
                    if (balanceIndex >= transactions.Length)
                        throw new InsufficientBalanceException();

                    var oldestBalance = transactions[balanceIndex];

                    if (oldestBalance == null)
                        throw new InsufficientBalanceException();

                    if (oldestBalance.Points > totalDeduction)
                    {
                        oldestBalance.Points -= totalDeduction;
                        totalDeduction = 0;
                    }
                    else
                    {
                        totalDeduction -= oldestBalance.Points;
                        oldestBalance.Points = 0; // 0 balances should be preserved, per Unilever example 
                    }

                    balanceIndex += 1;
                }

                Repository.UpdatePointsTransactions(transactions);
            }
        }

        public IEnumerable<PointsTransaction> DeletePoints(string userId, int amount)
        {
            var balances = Repository.GetPointsTransactionsByUserId(userId)
                .OrderBy(b => b.TransactionDate).ToArray();
            
            var transactions = new List<PointsTransaction>();

            int balanceIndex = 0;
            while(amount > 0)
            {
                if (balanceIndex >= balances.Length)
                    throw new InsufficientBalanceException();

                var oldestBalance = balances[balanceIndex];

                if (oldestBalance == null)
                    throw new InsufficientBalanceException();

                balanceIndex += 1;

                var transaction = new PointsTransaction()
                {
                    PayerName = oldestBalance.PayerName,
                    UserId = userId,
                    TransactionDate = DateTime.UtcNow
                };

                if (oldestBalance.Points > amount)
                {
                    transaction.Points = -amount;
                    oldestBalance.Points -= amount;
                    transactions.Add(transaction);
                    amount = 0;
                } else
                {
                    amount -= oldestBalance.Points;
                    transaction.Points = -oldestBalance.Points;
                    oldestBalance.Points = 0; // 0 balances should be preserved, per Unilever example 
                    transactions.Add(transaction);
                }
            }

            Repository.UpdatePointsTransactions(balances);

            return transactions;
        }

        public IEnumerable<PointsSummary> GetPointsSummaries(string userId)
        {
            var transactions = Repository.GetPointsTransactionsByUserId(userId);

            if (transactions == null)
                transactions = new List<PointsTransaction>();

            // Points are stored in the persistence layer as PointsTransactions, 
            // but this endpoint needs to aggregate the existing transactions into 
            // a list of PointsSummary objects which only contain the data useful to users.
            var summaries = new Dictionary<string, PointsSummary>();
            foreach (var transaction in transactions)
            {
                if (summaries.ContainsKey(transaction.PayerName))
                {
                    summaries[transaction.PayerName].TotalPoints += transaction.Points;
                }
                else
                {
                    summaries[transaction.PayerName] = new PointsSummary()
                    {
                        PayerName = transaction.PayerName,
                        TotalPoints = transaction.Points
                    };
                }
            }

            return summaries.Values;
        }
    }
}
