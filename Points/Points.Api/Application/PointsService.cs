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

                DeductPointsFromTransactionArray(transactions, userId, -transaction.Points);

                Repository.UpdatePointsTransactions(transactions);
            }
        }

        public IEnumerable<PointsTransaction> DeductPoints(string userId, int amount)
        {
            var balances = Repository.GetPointsTransactionsByUserId(userId)
                .OrderBy(b => b.TransactionDate).ToArray();

            var transactions = DeductPointsFromTransactionArray(balances, userId, amount);

            Repository.UpdatePointsTransactions(balances);

            return transactions;
        }

        /// <summary>
        /// Helper function to deduct points from a sorted array of PointsTransaction.
        /// </summary>
        /// <param name="balances">A sorted array of transactions from which to perform the delete operation on.</param>
        /// <param name="userId">The UserId associated with transactions to be deleted.</param>
        /// <param name="amount">The amount to deduct.</param>
        /// <returns>Returns a list of transactions that the deduction operation affected.</returns>
        private IEnumerable<PointsTransaction> DeductPointsFromTransactionArray(PointsTransaction[] balances, string userId, int amount)
        {
            var summaries = new List<PointsTransaction>();

            int balanceIndex = 0;
            while (amount > 0)
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
                    summaries.Add(transaction);
                    amount = 0;
                }
                else
                {
                    amount -= oldestBalance.Points;
                    transaction.Points = -oldestBalance.Points;
                    oldestBalance.Points = 0; // 0 balances should be preserved, per Unilever example 
                    summaries.Add(transaction);
                }
            }

            return summaries;
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
