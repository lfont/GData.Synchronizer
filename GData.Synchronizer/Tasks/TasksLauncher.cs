using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace GData.Synchronizer.Tasks
{
	public abstract class TasksLauncher<TItem1, TItem2> : IGDataTask<TItem1>
	{
		private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly TaskCollection<TItem2> _tasks = new TaskCollection<TItem2>();
		private readonly TaskFactory _taskFactory = new TaskFactory();
	    private string _id;

        protected TasksLauncher(string id)
        {
            Id = id;
        }

	    public string Id
	    {
            get { return _id; }
            set
            {
                _id = value;
                _tasks.UnicityToken = _id;
            }
	    }

        public ICollection<IGDataTask<TItem2>> Tasks
        {
            get { return _tasks; }
        }

	    public Predicate<TItem1> Prerequisites { get; set; }
        public bool Parallelize { get; set; }
		
		protected abstract TItem2 MapItem (TItem1 item);
		protected abstract TItem2 CopyItem (TItem2 item);
		
		public void Do(TItem1 item, CancellationToken cancellationToken)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Item: {0} used for launching job's tasks.", item);
			}
			
			TItem2 mappedItem = MapItem(item);

			foreach (IGDataTask<TItem2> task in Tasks)
			{
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

				TItem2 itemCopy = CopyItem(mappedItem);

                if (task.Prerequisites == null || task.Prerequisites(itemCopy))
				{
					if (Logger.IsInfoEnabled)
					{
						Logger.InfoFormat("Item: {0} used for launching task: {1}.", item, task.Id);
					}
					
					if (Parallelize)
					{
						// The closure must not use a shared instance of the task.
						IGDataTask<TItem2> t = task;
						_taskFactory.StartNew(() => t.Do(itemCopy, cancellationToken));
					}
					else
					{
						task.Do(itemCopy, cancellationToken);
					}
				}
			}
		}
	}
}
