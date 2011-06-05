using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GData.Synchronizer.Tasks;
using Google.GData.Client;
using log4net;

namespace GData.Synchronizer
{
	public abstract class GDataWatcher<TAtomEntry> : IDisposable
		where TAtomEntry : AbstractEntry
	{
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private bool _disposed;
	    private string _id;
        private readonly TaskFactory _watchTaskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
		private readonly List<Tuple<Task, CancellationTokenSource>> _watchTasks = new List<Tuple<Task, CancellationTokenSource>>();
        private readonly TaskCollection<TAtomEntry> _tasks = new TaskCollection<TAtomEntry>();

        protected GDataWatcher(string id)
        {
            Id = id;
			WatchInterval = TimeSpan.FromMinutes(30);	
		}
        
	    public string Id
	    {
            get { return _id; }
            protected set
            {
                _id = value;
                _tasks.UnicityToken = _id;
            }
	    }

        public ICollection<IGDataTask<TAtomEntry>> Tasks
        {
            get { return _tasks; }
        }

        public TimeSpan WatchInterval { get; set; }

        private static IEnumerable<TAtomEntry> GetLatestEntries(IEnumerable<TAtomEntry> previousEntries,
            IEnumerable<TAtomEntry> actualEntries)
		{
			if (Logger.IsInfoEnabled)
			{
		    	Logger.InfoFormat("Getting the latest entries of type: {0}.", actualEntries);
			}
			
            return from aEntry in actualEntries
                   let intersectionEntry = (from TAtomEntry pEntry in previousEntries
											where aEntry.SelfUri.Content == pEntry.SelfUri.Content
											select pEntry).FirstOrDefault() where intersectionEntry == null select aEntry;
		}

        private void Watch(Func<IEnumerable<TAtomEntry>> retriever, CancellationToken cancellationToken)
        {
			Stopwatch debugWatch = new Stopwatch();
			Stopwatch watch = new Stopwatch ();
            IEnumerable<TAtomEntry> previousEntries = new List<TAtomEntry>();

            while (!cancellationToken.IsCancellationRequested)
			{
				if (watch.ElapsedMilliseconds == 0 || watch.Elapsed >= WatchInterval)
				{
					if (Logger.IsInfoEnabled)
					{
                    	Logger.InfoFormat("Retriever: {0} is checking for new data.", retriever);
					}
					
					if (Logger.IsDebugEnabled)
					{
						debugWatch.Start();
					}
					
                    IEnumerable<TAtomEntry> entries = retriever();
                    foreach (TAtomEntry entry in GetLatestEntries(previousEntries, entries))
					{
						if (cancellationToken.IsCancellationRequested)
                    	{
                        	cancellationToken.ThrowIfCancellationRequested();
                    	}
						
						foreach (IGDataTask<TAtomEntry> task in Tasks)
						{
							// TODO: parallelize
                            if (task.Prerequisites == null || task.Prerequisites(entry))
                            {
                                task.Do(entry, cancellationToken);
                            }
							
							if (cancellationToken.IsCancellationRequested)
                        	{
                            	cancellationToken.ThrowIfCancellationRequested();
                        	}
						}
					}
					
					if (Logger.IsDebugEnabled)
					{
						debugWatch.Stop();
						Logger.DebugFormat("Tasks done in: {0}.", debugWatch.ElapsedMilliseconds);						
					}
					
					previousEntries = entries;
					watch.Restart();
					
					if (Logger.IsInfoEnabled)
					{
                    	Logger.InfoFormat("Retriver: {0} is waiting for the next checking time.", retriever);
					}
				}
				
				Thread.Sleep (1000);
			}
		}

        /// <summary>
        /// Start watching data returned by the given retriever.
        /// </summary>
        /// <param name="retriever">A source of data.</param>
        /// <returns>A token that can be use to cancel the task.</returns>
        public CancellationTokenSource StartWatch(Func<IEnumerable<TAtomEntry>> retriever)
		{
			if (Logger.IsInfoEnabled)
			{
		    	Logger.InfoFormat("Starting a watcher for: {0}.", retriever);
			}

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			Task task = _watchTaskFactory.StartNew(() => Watch(retriever, cancellationTokenSource.Token), cancellationTokenSource.Token);
			_watchTasks.Add(new Tuple<Task, CancellationTokenSource>(task, cancellationTokenSource));
			return cancellationTokenSource;
		}
	
		#region IDisposable implementation
		
        public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize(this);
		}
		
		protected void Dispose (bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			
			if (disposing)
			{
				if (Logger.IsDebugEnabled)
				{
                	Logger.DebugFormat("Disposing watcher: {0}.", this);
				}
				
                foreach (Tuple<Task, CancellationTokenSource> watchTask in _watchTasks)
                {
					if (Logger.IsInfoEnabled)
					{
                    	Logger.InfoFormat("Stopping task: {0}.", watchTask.Item1.Id);
					}
					
					watchTask.Item2.Cancel();

                    try
                    {
                        watchTask.Item1.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        // TODO: Handle exception correctly.
                        if (ae.InnerException is TaskCanceledException)
                        {
                            Logger.InfoFormat("Task: {0} has stopped.", watchTask.Item1.Id);
                        }
                        else
                        {
                            throw;
                        }
                    }
					
                    watchTask.Item1.Dispose();
					watchTask.Item2.Dispose();
                }

                _watchTasks.Clear();
			}
			
			_disposed = true;
		}
		
        #endregion
	}
}

