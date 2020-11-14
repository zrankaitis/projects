using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Points.Application;
using Points.Application.Data;
using Points.Application.Exceptions;
using Points.Models;
using System;
using System.Linq;

namespace Points.Tests
{
    public class PointsServiceTests
    {
        private PointsService Service { get; set; }

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddTransient<IPointsTransactionRepository, PointsTransactionSqlRepository>();
            services.AddTransient<IDbContext, SQLiteDbContext>();
            services.AddTransient<IPointsService, PointsService>();

            var provider = services.BuildServiceProvider();

            Service = (PointsService)provider.GetRequiredService<IPointsService>();
            var repository = provider.GetRequiredService<IPointsTransactionRepository>();
            repository.CreateSchema();
        }

        [Test]
        public void Test_AddNegativePointsToPositiveBalance()
        {
            string userId = "user";

            // Add points to make a positive balance
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "DANNON",
                Points = 300,
                TransactionDate = new DateTime(2020, 10, 31, 10, 30, 0)
            });

            // Add negative points
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "DANNON",
                Points = -100,
                TransactionDate = new DateTime(2020, 10, 31, 10, 30, 0)
            });

            // Balance should be reduced
            var balance = Service.GetPointsSummaries(userId);
            Assert.AreEqual(balance.First().TotalPoints, 200);
        }

        [Test]
        public void Test_AddNegativePointsBeyondBalanceProducesError()
        {
            string userId = "user";

            // Add points to make a positive balance
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "DANNON",
                Points = 100,
                TransactionDate = new DateTime(2020, 10, 31, 10, 30, 0)
            });

            // Exceeding the current balance should produce an exception
            Assert.Throws(typeof(InsufficientBalanceException), AddNegativePointsDelegate);
        }

        private void AddNegativePointsDelegate()
        {
            string userId = "user";
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "DANNON",
                Points = -200,
                TransactionDate = new DateTime(2020, 10, 31, 10, 30, 0)
            });
        }

        [Test]
        public void Test_Example()
        {
            string userId = "user";

            // Add a series of points from varying companies
            Service.AddPoints(userId, new PointsTransaction() {
                UserId = userId,
                PayerName = "DANNON",
                Points = 300,
                TransactionDate = new DateTime(2020,10,31,10,30,0)
            });
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "UNILEVER",
                Points = 200,
                TransactionDate = new DateTime(2020, 10, 31, 11, 0, 0)
            });
            Service.AddPoints(userId, new PointsTransaction()
            {
                PayerName = "DANNON",
                Points = -200,
                TransactionDate = new DateTime(2020, 10, 31, 15, 0, 0)
            });
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "MILLER COORS",
                Points = 10000,
                TransactionDate = new DateTime(2020, 11, 1, 14, 0, 0)
            });
            Service.AddPoints(userId, new PointsTransaction()
            {
                UserId = userId,
                PayerName = "DANNON",
                Points = 1000,
                TransactionDate = new DateTime(2020, 11, 2, 14, 0, 0)
            });

            // Deduct 5,000 points
            var result = Service.DeletePoints(userId, 5000);

            var dannonTransaction = result.Where(t => t.PayerName == "DANNON").First();
            Assert.AreEqual(dannonTransaction.Points, -100);

            var unileverTranaction = result.Where(t => t.PayerName == "UNILEVER").First();
            Assert.AreEqual(unileverTranaction.Points, -200);

            var millerTransaction = result.Where(t => t.PayerName == "MILLER COORS").First();
            Assert.AreEqual(millerTransaction.Points, -4700);

            var summary = Service.GetPointsSummaries(userId);

            var dannonSummary = summary.Where(s => s.PayerName == "DANNON").First();
            Assert.AreEqual(dannonSummary.TotalPoints, 1000);

            var unileverSummary = summary.Where(s => s.PayerName == "UNILEVER").First();
            Assert.AreEqual(unileverSummary.TotalPoints, 0);

            var millerSummary = summary.Where(s => s.PayerName == "MILLER COORS").First();
            Assert.AreEqual(millerSummary.TotalPoints, 5300);
        }
    }
}