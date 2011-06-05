using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Documents;
using Google.GData.Client;
using Google.GData.Documents;
using log4net;

namespace GData.Synchronizer.Documents
{
	public class DocumentsWatcher : GDataWatcher<DocumentEntry>
	{
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DocumentsService _service;

		public DocumentsWatcher (string applicationName, GDataCredentials credentials, string id)
            : base(id)
		{
		    _service = new DocumentsService(applicationName)
		        {
		            Credentials = credentials
		        };

            RequestSettings requestSettings = new RequestSettings(applicationName, credentials);
			Request = new DocumentsRequest(requestSettings);

		    ApplicationName = applicationName;
		}

        public DocumentsWatcher(string applicationName, GDataCredentials credentials)
            : this(applicationName, credentials, "DocumentsWatcher")
        {
            
        }

        public string ApplicationName { get; private set; }
        public DocumentsRequest Request { get; private set; }
		
		public IEnumerable<DocumentEntry> RetrieveRecentsDocuments (int count)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.InfoFormat("Retrieving recent documents count: {0}.", count);
			}
			
			DocumentsListQuery query = new DocumentsListQuery { NumberToRetrieve = count };
			DocumentsFeed feed = _service.Query (query);
			return feed.Entries.Cast<DocumentEntry>();
		}

        public IEnumerable<DocumentEntry> RetrieveAllDocuments()
        {
            return RetrieveRecentsDocuments(int.MaxValue);
        }

	    public IEnumerable<DocumentEntry> RetrieveDocumentsInFolder (string folder)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.InfoFormat("Retrieving all documents in folder: {0}.", folder);
			}
			
            string folderId = (from Document d in Request.GetFolders().Entries
                               where d.Title == folder
                               select d.ResourceId).FirstOrDefault();

            if (String.IsNullOrEmpty(folderId))
            {
				if (Logger.IsWarnEnabled)
				{
                	Logger.WarnFormat("Unable to find the folderId of folder: {0}.", folder);
				}
				return new List<DocumentEntry>();
            }

            DocumentsListQuery query = new DocumentsListQuery(
                String.Format("https://docs.google.com/feeds/default/private/full/{0}/contents", folderId));
            DocumentsFeed feed = _service.Query(query);
            return feed.Entries.Cast<DocumentEntry>();
		}
	}
}

