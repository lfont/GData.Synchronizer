using System;
using System.Linq;
using System.Reflection;
using log4net;

namespace GData.Synchronizer.Storage
{
    public class TaskHistoryRepository : ITaskHistoryRepository
    {
		private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        private static IDataContext GetContext()
        {
            return new SQLiteContext();
        }

        public void SaveHistory(TaskHistory taskHistory)
        {
			try
			{
				using (IDataContext context = GetContext())
        		{
                    context.Save(taskHistory);
				}
			}
			catch (Exception ex)
			{
				if (Logger.IsErrorEnabled)
				{
					Logger.Error("SaveHistory failed.", ex);
				}
			}
        }

        public TaskHistory GetHistory(string taskId, string uri)
        {
			try
			{
				using (IDataContext context = GetContext())
            	{
					return (from t in context.Query<TaskHistory>()
                            where t.TaskId == taskId && t.Uri == uri
                            select t).FirstOrDefault();
            	}
			}
			catch (Exception ex)
			{
				if (Logger.IsErrorEnabled)
				{
					Logger.Error("GetHistory failed.", ex);
				}
				
				return null;
			}
        }
    }
}
