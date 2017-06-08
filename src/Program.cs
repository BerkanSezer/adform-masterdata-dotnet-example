using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace DownloadAdformMasterData
{
    internal class Program
    {
        private const string AdformSecurityUrl = @"https://api.adform.com/Services/Security/Login";
        private const string AdformMasterDataUrl = @"https://api.adform.com/v1/buyer/masterdata";

        private static int Main(string[] args)
        {
            if (args.Length == 4)
            {
                var setupId = args[0];
                var user = args[1];
                var password = args[2];
                var targetDirectory = args[3];

                Console.WriteLine("Authenticating with Adform security...");

                var ticket = GetTicket(user, password);

                if (String.IsNullOrEmpty(ticket))
                    throw new InvalidOperationException("Failed to retrieve authentication ticket");
                
                Console.WriteLine("Listing MasterData files...");
                var list = GetFileList(setupId, ticket);

                Console.WriteLine("Downloading missing files...");
                DownloadMissingFiles(list, setupId, ticket, targetDirectory);

                Console.WriteLine("Finished.");
            }
            else
            {
                Console.WriteLine("Usage of the program:");
                Console.WriteLine("[executable] [setup-id] [adform-user] [adform-password] [target-directory]");
                Console.WriteLine("For example:");
                Console.WriteLine("[executable] 1997e40a-f0e6-44ac-b3b2-60c5d6fce1bd user password c:/temp/1997e40a-f0e6-44ac-b3b2-60c5d6fce1bd");
                return -1;
            }

            return 0;
        }

        private static String GetTicket(String user, String password)
        {
            var wr = WebRequest.Create(AdformSecurityUrl);

            wr.Method = "POST";
            wr.ContentType = "application/json";

            var loginRequest = new
            {
                Username = user,
                Password = password,
            };

            using (var stream = wr.GetRequestStream())
                WriteObject(stream, loginRequest);
            
            var response = (HttpWebResponse) wr.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Unexpected response status: " + response.StatusCode);

            using (var stream = response.GetResponseStream())
                return ReadObject<LoginResponseDto>(stream, (int) response.ContentLength).Ticket;
        }

        private static FileDto[] GetFileList(string setupId, string ticket)
        {
            var wr = WebRequest.CreateHttp(AdformMasterDataUrl + "/files/" + setupId);

            AddTicketCookie(wr, ticket);

            var response = (HttpWebResponse) wr.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Unexpected response status: " + response.StatusCode);

            using (var stream = response.GetResponseStream())
                return ReadObject<FileDto[]>(stream, (int) response.ContentLength);
        }

        private static void DownloadMissingFiles(FileDto[] list, string setupId, string ticket, string targetDirectory)
        {
            // lets make sure target directory exists
            var target = new DirectoryInfo(targetDirectory);

            if (!target.Exists)
                target.Create();

            if (list != null)
            {
                foreach (var file in list)
                    DownloadFile(file, setupId, ticket, target);
            }
        }

        private static void DownloadFile(FileDto file, string setupId, string ticket, DirectoryInfo targetDirectory)
        {
            var resourceUrl = AdformMasterDataUrl + "/download/" + setupId + "/" + file.id;

            // skip if we have already have it
            if (targetDirectory.GetFiles(file.name).Length != 0)
                return;

            var wr = WebRequest.CreateHttp(resourceUrl);

            AddTicketCookie(wr, ticket);

            var response = (HttpWebResponse) wr.GetResponse();
            
            using (var downloadStream = response.GetResponseStream())
            {
                if (downloadStream == null)
                    throw new NullReferenceException("Could not retrieve response stream");

                var targetFile = new FileInfo(targetDirectory.FullName + "\\" + file.name);

                try
                {
                    using (var fileStream = targetFile.Create())
                    {
                        int read;
                        var buffer = new byte[1024];
                        while ((read = downloadStream.Read(buffer, 0, buffer.Length)) > 0)
                            fileStream.Write(buffer, 0, read);
                    }
                }
                catch
                {
                    if (targetFile.Exists)
                        targetFile.Delete();
                    throw;
                }
            }

        }

        private static void WriteObject(Stream stream, object obj)
        {
            var json = new JavaScriptSerializer().Serialize(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static T ReadObject<T>(Stream stream, int length)
        {
            var bytes = new byte[length];

            var read = 0;
            while (read < length)
                read += stream.Read(bytes, read, length-read);

            var json = Encoding.UTF8.GetString(bytes);

            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(json);
        }

        private static void AddTicketCookie(HttpWebRequest wr, string ticket)
        {
            wr.CookieContainer = new CookieContainer();
            wr.CookieContainer.Add(new Cookie("authTicket", ticket, "/", ".adform.com"));
        }
        
        class LoginResponseDto
        {
            public string Ticket;
        }

        class FileDto
        {
            public string id;
            public string name;
            public string setup;
            public long size;
            public string createdAt;
        }
    }
}
