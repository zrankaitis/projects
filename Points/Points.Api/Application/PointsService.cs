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
        private IPointsRepository Repository { get; set; }

        public PointsService(IPointsRepository repository)
        {
            Repository = repository;
        }

        public void AddPoints(string userId, PointsTransaction transaction)
        {
            if (transaction.Points == 0)
                return;

            var balances = (IList<PointsTransaction>)Repository.GetPointsTransactionsByUserId(userId);

            if (balances == null)
                balances = new List<PointsTransaction>();

            if (transaction.Points > 0)
                balances.Add(transaction);
            else
            {
                int amountToDeduct = -transaction.Points;

                var payerBalances = balances.Where(b => b.PayerName == transaction.PayerName).OrderBy(b => b.TransactionDate).ToArray();

                int balanceIndex = 0;
                while(amountToDeduct > 0)
                {
                    // Potential for refactoring here as this is largely the same logic as DeletePoints()
                    if (balanceIndex >= payerBalances.Length)
                        throw new InsufficientBalanceException();
                    
                    var oldestBalance = balances[balanceIndex];

                    if (oldestBalance == null)
                        throw new InsufficientBalanceException();

                    balanceIndex += 1;

                    if (oldestBalance.Points > amountToDeduct)
                    {
                        oldestBalance.Points -= amountToDeduct;
                        amountToDeduct = 0;
                    } else
                    {
                        amountToDeduct -= oldestBalance.Points;
                        oldestBalance.Points = 0; // 0 balances should be preserved, per Unilever example 
                    }
                } 
            }

            Repository.UpdatePointsBalanceByUserId(userId, balances);
        }

        public IEnumerable<PointsTransaction> DeletePoints(string userId, int amount)
        {
            var balances = Repository.GetPointsTransactionsByUserId(userId).OrderBy(b => b.TransactionDate).ToArray();
            
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

            Repository.UpdatePointsBalanceByUserId(userId, balances.ToList());

            return transactions;
        }

        public IEnumerable<PointsSummary> GetPointsSummaries(string userId)
        {
            var balances = Repository.GetPointsTransactionsByUserId(userId);

            if (balances == null)
                balances = new List<PointsTransaction>();

            // Points are stored in the persistence layer as PointsTransactions, 
            // but this endpoint needs to aggregate the existing transactions into 
            // a list of PointsSummary objects which only contain the data useful to users.
            var summaries = new Dictionary<string, PointsSummary>();
            foreach (var balance in balances)
            {
                if (summaries.ContainsKey(balance.PayerName))
                {
                    summaries[balance.PayerName].TotalPoints += balance.Points;
                }
                else
                {
                    summaries[balance.PayerName] = new PointsSummary()
                    {
                        PayerName = balance.PayerName,
                        TotalPoints = balance.Points
                    };
                }
            }

            return summaries.Values;
        }
    }
}
