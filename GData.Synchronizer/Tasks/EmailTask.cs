using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using log4net;

namespace GData.Synchronizer.Tasks
{
    public class EmailTask : PersistentTask
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public EmailTask(NetworkCredential credentials, string to)
        {
            Credentials = credentials;
            To = to;
        }

        public NetworkCredential Credentials { get; private set; }
        public string To { get; private set; }

        public virtual void Send(GDataFile file)
        {
			if (Logger.IsInfoEnabled)
			{
            	Logger.InfoFormat("Sending file: {0} to: {1}", file.Name, To);
			}
			
            MailMessage message = new MailMessage(Credentials.UserName, To, file.Name, String.Empty);

            Attachment attachment = new Attachment(file.BaseStream, file.Name, file.MediaType);
            message.Attachments.Add(attachment);

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = Credentials;
            client.EnableSsl = true;
            client.Timeout = client.Timeout * 10;
            client.Send(message);
        }
		
        protected override void Execute(GDataFile item)
        {
			if (Logger.IsDebugEnabled)
			{
            	Logger.DebugFormat("Emailing file: {0}.", item.Name);
			}
			
        	Send(item);
        }
    }
}
