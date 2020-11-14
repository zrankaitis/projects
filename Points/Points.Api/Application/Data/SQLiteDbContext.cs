using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Points.Application.Data
{
    public class SQLiteDbContext : IDbContext, IDisposable
    {
        private readonly IDbConnection connection;

        public SQLiteDbContext()
        {
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
        }

        public IDbConnection GetConnection()
        {
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
