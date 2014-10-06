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
        private const string AdformMasterDataUrl = @"http://masterdata.adform.com:8652";

        private static int Main(string[] args)
        {
            if (args.Length == 4)
            {
                var listId = Int32.Parse(args[0]);
                var user = args[1];
                var password = args[2];
                var targetDirectory = args[3];

                Console.WriteLine("Authenticating with Adform security...");
                var ticket = GetTicket(user, password);
                if (String.IsNullOrEmpty(ticket))
                    throw new InvalidOperationException("Failed to retrieve authentication ticket");
                
                Console.WriteLine("Listing MasterData files...");
                var list = GetFileList(listId, ticket);

                Console.WriteLine("Downloading missing files...");
                DownloadMissingFiles(list, ticket, targetDirectory);

                Console.WriteLine("Finished.");
            }
            else
            {
                Console.WriteLine("Usage of the program:");
                Console.WriteLine("[executable] [client-division-id] [adform-user] [adform-password] [target-directory]");
                Console.WriteLine("For example:");
                Console.WriteLine("[executable] 123 user password c:/temp/md-123");
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
                return ReadObject<LoginResponseDTO>(stream, (int) response.ContentLength).Ticket;
        }

        private static FileListDTO GetFileList(int listId, string ticket)
        {
            var wr = WebRequest.Create(AdformMasterDataUrl + "/list/" + listId + "?render=json&authTicket=" + ticket);

            var response = (HttpWebResponse)wr.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Unexpected response status: " + response.StatusCode);

            using (var stream = response.GetResponseStream())
                return ReadObject<FileListDTO>(stream, (int)response.ContentLength);
        }

        private static void DownloadMissingFiles(FileListDTO list, string ticket, string targetDirectory)
        {
            // lets make sure target directory exists
            var target = new DirectoryInfo(targetDirectory);
            if (!target.Exists)
                target.Create();

            if (list.meta != null)
                DownloadFile(list.meta, ticket, target);

            if (list.files != null)
            {
                foreach (var file in list.files)
                    DownloadFile(file, ticket, target);
            }
        }

        private static void DownloadFile(FileDTO file, string ticket, DirectoryInfo targetDirectory)
        {
            var fileName = Path.GetFileName(file.absolutePath);
            if (fileName == null)
                throw new NullReferenceException("Invalid file name in path: " + file.absolutePath);

            // skip if we have already have it
            if (targetDirectory.GetFiles(fileName).Length != 0)
                return;

            var wr = WebRequest.Create(file.absolutePath + "?authTicket=" + ticket);
            var response = (HttpWebResponse)wr.GetResponse();
            
            using (var downloadStream = response.GetResponseStream())
            {
                if (downloadStream == null)
                    throw new NullReferenceException("Could not retrieve response stream");

                var targetFile = new FileInfo(targetDirectory.FullName + "\\" + fileName);
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
        
        class LoginResponseDTO
        {
            public string Ticket;
        }

        class FileListDTO
        {
            public FileDTO meta;
            public FileDTO[] files;
        }


        class FileDTO
        {
            public string path;
            public string absolutePath;
            public long size;
            public string created;
        }
    }
}
