using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using GData.Synchronizer.Tasks;
using Google.GData.Client;
using Google.GData.Photos;
using Google.Picasa;
using log4net;

namespace GData.Synchronizer.Picasa
{
    public class PicasaDownloadTask : DownloadTask<PicasaEntry>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PicasaDownloadTask(PicasaRequest request)
            : base("PicasaDownloadTask")
        {
            Request = request;
        }

        public PicasaRequest Request { get; private set; }

        public static string GetMediaType(Photo photo)
        {
            switch (Path.GetExtension(photo.Title))
            {
                case ".gif":
                    return MediaTypeNames.Image.Gif;
                case ".tiff":
                    return MediaTypeNames.Image.Tiff;
                default:
                    return MediaTypeNames.Image.Jpeg;
            }
        }

        public Album GetAlbum(string albumId)
        {
            return (from Album a in Request.GetAlbums().Entries
                    where a.Id == albumId
                    select a).FirstOrDefault();
        }

        protected override GDataFile Download(PicasaEntry entry)
        {
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Downloading photo: {0}.", entry.Title.Text);
			}
			
            Feed<Photo> feed = Request.Get<Photo>(new Uri(entry.SelfUri.Content));
            Photo photo = feed.Entries.FirstOrDefault();
            Stream stream = Request.Download(photo);

            GDataFile file = new GDataFile(entry, photo.Title, stream)
                {
                    MediaType = GetMediaType(photo)
                };
            
            Album album = GetAlbum(photo.AlbumId);
            if (album != null)
            {
                file.DirectoryName = album.Title;
            }

            return file;
        }
    }
}
