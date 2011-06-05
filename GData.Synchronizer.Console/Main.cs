using System;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using GData.Synchronizer.Documents;
using GData.Synchronizer.Extensions;
using GData.Synchronizer.Picasa;
using GData.Synchronizer.Storage;
using GData.Synchronizer.Tasks;
using Google.GData.Client;
using log4net;

namespace GData.Synchronizer.Console
{
	class MainClass
	{
		private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			
		public static void Main (string[] args)
		{
			// TODO: Put string in a config file.
			ServicePointManager.ServerCertificateValidationCallback = Validator;

		    const string applicationName = "lfont-GData.Synchronizer.Console-1";
			const string email = "<A valid Google Account email>";
			const string password = "<The password of the Google Account>";
            GDataCredentials gDataCredentials = new GDataCredentials(email, password);
            NetworkCredential networkCredential = new NetworkCredential(email, password);

            using (DocumentsWatcher documentsWatcher = new DocumentsWatcher(applicationName, gDataCredentials))
            using (PicasaWatcher picasaWatcher = new PicasaWatcher(applicationName, gDataCredentials))
            {
				TaskHistoryRepository historyRepository = new TaskHistoryRepository();

                #region Documents

                CopyTask documentCopyTask = new CopyTask("Output".Combine("Documents", "CopyTask"))
                {
                    HisotoryRepository = historyRepository,
                    Prerequisites = f => true
                };

                EmailTask documentEmailTask = new EmailTask(networkCredential, email)
                {
                    HisotoryRepository = historyRepository,
                    Prerequisites = f => true
                };

                DocumentDownloadTask documentDownloadTask = new DocumentDownloadTask(documentsWatcher.Request)
                {
                    Parallelize = true,
                    Prerequisites = e => true,
                    Tasks =
                            {
                                documentCopyTask,
                                documentEmailTask
                            }
                };

                documentsWatcher.Tasks.Add(documentDownloadTask);
                documentsWatcher.WatchInterval = TimeSpan.FromMinutes(30);

                Logger.Info("Starting Documents watcher.");
                documentsWatcher.StartWatch (documentsWatcher.RetrieveAllDocuments);

                #endregion

				#region Picasa

                CopyTask picasaCopyTask = new CopyTask("Output".Combine("Picasa", "CopyTask"))
                                                {
                                                    HisotoryRepository = historyRepository,
                                                    Prerequisites = f => true                         
                                                };

                EmailTask picasaEmailTask = new EmailTask(networkCredential, email)
                                                  {
                                                      HisotoryRepository = historyRepository,
                                                      Prerequisites = f => true
                                                  };

                PicasaDownloadTask picasaDownloadTask = new PicasaDownloadTask(picasaWatcher.Request)
                    {
                        Parallelize = true,
                        Prerequisites = e => true,
                        Tasks =
                            {
                                picasaCopyTask,
                                picasaEmailTask
                            }
                    };

                picasaWatcher.Tasks.Add(picasaDownloadTask);
                picasaWatcher.WatchInterval = TimeSpan.FromMinutes(5);
                
                Logger.Info("Starting Picasa watcher.");
                picasaWatcher.StartWatch(picasaWatcher.RetrieveAllPhotos);
				
                #endregion
				
                System.Console.ReadLine();
            }
			
			Logger.Info("All watchers have stopped successfuly.");
            System.Console.ReadLine();
		}
		
		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
