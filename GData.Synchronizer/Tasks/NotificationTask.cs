using System.Reflection;
using Google.GData.Client;
using log4net;

namespace GData.Synchronizer.Tasks
{
    public class NotificationTask<TAtomEntry> : TasksLauncher<TAtomEntry, TAtomEntry>
        where TAtomEntry : AbstractEntry
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NotificationTask()
            : base("NotificationTask")
        {
            
        }

		protected override TAtomEntry MapItem (TAtomEntry item)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Task: {0}, Item: {1} mapping item.", Id, item.Title.Text);
			}
			
			return item;
		}
		
		protected override TAtomEntry CopyItem (TAtomEntry item)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Task: {0}, Item: {1} copying item.", Id, item.Title.Text);
			}
			
			return item;
		}
    }
}
