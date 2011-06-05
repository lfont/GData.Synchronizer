using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using GData.Synchronizer.Tasks;
using Google.Documents;
using Google.GData.Client;
using Google.GData.Documents;
using log4net;

namespace GData.Synchronizer.Documents
{
    public class DocumentDownloadTask : DownloadTask<DocumentEntry>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentDownloadTask(DocumentsRequest request)
            : base("DocumentDownloadTask")
        {
            Request = request;
        }

        public DocumentsRequest Request { get; private set; }

        protected override GDataFile Download(DocumentEntry entry)
        {
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Downloading document: {0}.", entry.Title.Text);
			}
			
            Feed<Document> feed = Request.Get<Document>(new Uri(entry.SelfUri.Content));
            Document document = feed.Entries.FirstOrDefault();
            string fileName;
            string exportFormat;

            if (entry.IsPDF)
            {
                fileName = entry.Title.Text;
                exportFormat = String.Empty;
            }
            else
            {
                int dotIndex = entry.Title.Text.LastIndexOf('.');
                fileName = ((dotIndex > 0) ?
                    entry.Title.Text.Substring(0, dotIndex) :
                    entry.Title.Text) + ".pdf";
                exportFormat = Document.DownloadType.pdf.ToString();
            }

            Stream stream = Request.Download(document, exportFormat);
            GDataFile file = new GDataFile (entry, fileName, stream)
                {
                    MediaType = MediaTypeNames.Application.Pdf
                };
			
            if (entry.ParentFolders.Count > 0)
            {
                file.DirectoryName = entry.ParentFolders[0].Title;
            }
            
            return file;
        }
    }
}
