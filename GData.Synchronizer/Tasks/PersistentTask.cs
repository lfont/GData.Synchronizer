using System;
using System.Reflection;
using System.Threading;
using GData.Synchronizer.Storage;
using log4net;

namespace GData.Synchronizer.Tasks
{
    public abstract class PersistentTask : IGDataTask<GDataFile>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        public string Id { get; set; }
		public Predicate<GDataFile> Prerequisites { get; set; }		
		public ITaskHistoryRepository HisotoryRepository { get; set; }
		
        protected abstract void Execute(GDataFile item);

        public void Do(GDataFile item, CancellationToken cancellationToken)
        {
            TaskHistory history = null;

            if (HisotoryRepository != null)
            {
                history = HisotoryRepository.GetHistory(Id, item.Entry.SelfUri.Content);
                if (history != null && history.Updated == item.Entry.Updated)
                {
					if (Logger.IsDebugEnabled)
					{
                    	Logger.DebugFormat("Task: {0}, Item: {1} already processed.", Id, item.Entry.SelfUri.Content);
					}
					
					return;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            Execute(item);

            if (HisotoryRepository != null)
            {
                if (history == null)
                {
					if (Logger.IsDebugEnabled)
					{
                    	Logger.DebugFormat("Task: {0}, Item: {1} adding history.", Id, item.Entry.SelfUri.Content);
					}
						
					history = new TaskHistory
	                    {
							TaskId = Id,
	                        Updated = item.Entry.Updated,
	                        Uri = item.Entry.SelfUri.Content
	                    };
                    HisotoryRepository.SaveHistory(history);
                }
                else
                {
					if (Logger.IsDebugEnabled)
					{
                    	Logger.DebugFormat("Task: {0}, Item: {1} updating history.", Id, item.Entry.SelfUri.Content);
					}
						
					history.Updated = item.Entry.Updated;
                    HisotoryRepository.SaveHistory(history);
                }
            }
        }
    }
}
