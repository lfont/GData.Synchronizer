using System.IO;
using System.Reflection;
using GData.Synchronizer.Extensions;
using log4net;

namespace GData.Synchronizer.Tasks
{
	public class CopyTask : PersistentTask
	{
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        public CopyTask(string destinationFullPath)
        {
			DestinationDirectory = new DirectoryInfo (destinationFullPath.GetValidPath ());
        	if (!DestinationDirectory.Exists) {
				DestinationDirectory.Create ();
			}
			
			PreserveHierarchy = true;
		}

        public DirectoryInfo DestinationDirectory { get; private set; }
        public bool PreserveHierarchy { get; set; }

		public virtual void Copy(GDataFile file)
		{
		    FileInfo fileInfo = PreserveHierarchy ?
                new FileInfo(Path.Combine(DestinationDirectory.FullName, file.RelativeFullName)) :
                new FileInfo(Path.Combine(DestinationDirectory.FullName, file.Name));

            if (!fileInfo.Directory.Exists)
            {
				if (Logger.IsDebugEnabled)
				{
                	Logger.DebugFormat("Creating destination directory: {0}.", fileInfo.DirectoryName);
				}
				
				fileInfo.Directory.Create();
            }
			
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Copying file: {0} to: {1}", file.Name, fileInfo.FullName);
			}
			
			using (FileStream fileStream = File.Create(fileInfo.FullName))
			{
				const int bufferSize = 2048;
				byte[] buffer = new byte[bufferSize];
				int count;
				
				do
				{
					count = file.BaseStream.Read(buffer, 0, bufferSize);
					fileStream.Write(buffer, 0, count);
				}
				while (count > 0);
			}
		}

        protected override void Execute(GDataFile item)
		{
			if (Logger.IsDebugEnabled)
			{
            	Logger.DebugFormat("Copying file: {0}.", item.Name);
			}
			
			Copy(item);
		}
	}
}

