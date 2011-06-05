using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GData.Synchronizer.Extensions;
using Google.GData.Client;

namespace GData.Synchronizer
{
	public sealed class GDataFile : IDisposable
	{
		private bool _disposed;
        private string _directoryName = String.Empty;

        public GDataFile(AbstractEntry entry, string name, Stream baseStream)
        {
			Entry = entry;
            Name = name.GetValidFileName();
            BaseStream = baseStream;
        }
		
		public AbstractEntry Entry { get; private set; }
        public string Name { get; private set; }
		public Stream BaseStream { get; private set; }
		public string MediaType { get; set; }
		
		public string DirectoryName
		{
			get { return _directoryName; }
			set { _directoryName = (value == null) ? null : value.GetValidPath (); }
		}
		
	    public string RelativeFullName
	    {
            get { return Path.Combine(DirectoryName, Name); }
	    }

		#region IDisposable implementation
		
		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
				
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			
			if (disposing && BaseStream != null)
			{
				BaseStream.Dispose();
				BaseStream = null;
			}
			
			_disposed = true;
		}
		
		#endregion
	}
}

