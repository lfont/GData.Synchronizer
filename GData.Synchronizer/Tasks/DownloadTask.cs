using System.IO;
using System.Reflection;
using Google.GData.Client;
using log4net;

namespace GData.Synchronizer.Tasks
{
	public abstract class DownloadTask<TAtomEntry> : TasksLauncher<TAtomEntry, GDataFile>
		where TAtomEntry : AbstractEntry
	{
		private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected DownloadTask(string id)
            : base(id)
        {
            
        }

		protected abstract GDataFile Download(TAtomEntry entry);
				
		private static Stream GetSeekableStream(Stream stream)
		{
			Logger.DebugFormat("Copying stream into memory.");
			
			if (stream.CanSeek)
			{
				stream.Position = 0;
			}
			
			MemoryStream memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			memoryStream.Position = 0;
			return memoryStream;
		}
				
		protected override GDataFile MapItem(TAtomEntry item)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Task: {0}, Item: {1} mapping item.", Id, item.Title.Text);
			}
			
			GDataFile file = Download(item);
			return CopyItem(file);
		}
		
		protected override GDataFile CopyItem (GDataFile item)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Task: {0}, Item: {1} copying item.", Id, item.Name);
			}
			
			return new GDataFile(item.Entry, item.Name, GetSeekableStream(item.BaseStream))
				{
					DirectoryName = item.DirectoryName,
					MediaType = item.MediaType
				};
		}
	}
}

