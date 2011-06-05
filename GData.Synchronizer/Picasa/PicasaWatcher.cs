using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.GData.Client;
using Google.GData.Photos;
using Google.Picasa;
using log4net;

namespace GData.Synchronizer.Picasa
{
	public class PicasaWatcher : GDataWatcher<PicasaEntry>
	{
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly PicasaService _service;

		public PicasaWatcher(string applicationName, GDataCredentials credentials, string id)
			: base(id)
		{
		    _service = new PicasaService(applicationName)
                {
                    Credentials = credentials
                };

            RequestSettings requestSettings = new RequestSettings(applicationName, credentials);
            Request = new PicasaRequest(requestSettings);

		    ApplicationName = applicationName;
		}

        public PicasaWatcher(string applicationName, GDataCredentials credentials)
            : this(applicationName, credentials, "PicasaWatcher")
        {
            
        }

        public string ApplicationName { get; private set; }
        public PicasaRequest Request { get; private set; }

        public IEnumerable<PicasaEntry> RetrieveRecentPhotos(string username, int count)
        {
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Retrieving recent photos for username: {0} count: {1}.", username, count);
			}
			
            PhotoQuery query = new PhotoQuery(PicasaQuery.CreatePicasaUri(username))
                {
                    NumberToRetrieve = count
                };
            PicasaFeed feed = _service.Query(query);
            return feed.Entries.Cast<PicasaEntry>();
        }

        public IEnumerable<PicasaEntry> RetrieveRecentPhotos(int count)
        {
            return RetrieveRecentPhotos("default", count);
        }

        public IEnumerable<PicasaEntry> RetrieveAllPhotos(string username)
        {
            return RetrieveRecentPhotos(username, int.MaxValue);
        }

        public IEnumerable<PicasaEntry> RetrieveAllPhotos()
        {
            return RetrieveAllPhotos("default");
        }

	    public IEnumerable<PicasaEntry> RetrievePhotosInAlbum(string username, string album)
		{
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Retrieving all photos in album: {0} for username: {1}.", album, username);
			}
			
            string albumId = (from Album a in Request.GetAlbums().Entries
                             where a.Title == album
                             select a.Id).FirstOrDefault();

            if (String.IsNullOrEmpty(albumId))
            {
				if (Logger.IsDebugEnabled)
				{
                	Logger.DebugFormat("Unable to find the albumId of album: {0}.", album);
				}
				
				return new List<PicasaEntry>();
            }

            PhotoQuery query = new PhotoQuery(PicasaQuery.CreatePicasaUri(username, albumId));
            PicasaFeed feed = _service.Query(query);
            return feed.Entries.Cast<PicasaEntry>();
		}

        public IEnumerable<PicasaEntry> RetrievePhotosInAlbum(string album)
        {
            return RetrievePhotosInAlbum("default", album);
        }
	}
}

