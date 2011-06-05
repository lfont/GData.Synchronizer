using System;
using System.Linq;
using System.Reflection;
using log4net;
using System.Configuration;
using System.IO;
using PetaPoco;

namespace GData.Synchronizer.Storage
{
    public class SQLiteContext : IDataContext
    {
		private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private bool _disposed;
		private Database _database;
		
		static SQLiteContext()
        {
            string connectionString = GetConnectionString();
            string dataSource = GetDataSource(connectionString);
            if (String.IsNullOrEmpty(dataSource))
            {
                throw new ApplicationException("SQLite data source is not valid.");
            }
            
            if (!DatabaseExists(dataSource))
            {
                InitializeDatabase(connectionString);
            }
        }
		
		public SQLiteContext()
		{
			string connectionString = GetConnectionString();
			_database = GetDatabase(connectionString);
		}
		
		public static string GetConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["GData.Synchronizer.Storage"].ConnectionString;
		}
		
		private static Database GetDatabase(string connectionString)
		{
			return new Database(connectionString, "System.Data.SQLite");
		}
		
        private static string GetDataSource(string connectionString)
        {
            int dataSourceIndex = connectionString.IndexOf("Data Source=");
            if (dataSourceIndex > -1)
            {
                string dataSource = connectionString.Substring(dataSourceIndex + 12);
                int endOfDataSourceIndex = dataSource.IndexOfAny(new [] { ',', ';' });
                if (endOfDataSourceIndex < 0)
                {
                    endOfDataSourceIndex = dataSource.Length;
                }
                return dataSource.Substring(0, endOfDataSourceIndex);
            }

            return String.Empty;
        }
		
        private static bool DatabaseExists(string dataSource)
        {
            FileInfo fileInfo = new FileInfo(dataSource);
            return fileInfo.Exists;
        }
		
        public static void InitializeDatabase(string connectionString)
        {
            using (Database db = GetDatabase(connectionString))
            {
				if (Logger.IsInfoEnabled)
				{
					Logger.InfoFormat("Initializing database using connection: {0}.", connectionString);
				}
			
                db.Execute(@"CREATE TABLE TaskHistory (
	                            Id INTEGER PRIMARY KEY NOT NULL,
								TaskId VARCHAR (128) NOT NULL,
	                            Uri VARCHAR (2084) NOT NULL,
	                            Updated DATETIME NOT NULL,
	                            UNIQUE (TaskId, Uri)
	                        )");
					
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Creating index using connection: {0}.", connectionString);
				}
					
				db.Execute(@"CREATE INDEX main.TaskIdx ON TaskHistory (
								TaskId,
								Uri
						  )");
            }
        }

        public void Save(object poco)
        {
            _database.Save(poco);
        }

        public IQueryable<T> Query<T>()
		{
			return _database.Query<T>();
		}
		
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			
			if (disposing)
			{
				if (_database != null)
				{
					_database.Dispose();
					_database = null;
				}
			}
			
			_disposed = true;
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
    }
}