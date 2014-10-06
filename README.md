adform-masterdata-dotnet-example
================================

This is an example of .net C# application, which explains how to consume MasterData file service. You need a working adform account and MasterData service enabled for you.

To automate MasterData download you will be required to perform these steps:
1.	Authenticate with Adform security and receive authorization ticket, which will be used for further interactions 
2.	Retrieve file list for your clients
3.	Download each file individually

More technical details on the steps:
1.	To Authenticate with Adform security you will need to: 
a.	Send HTTP POST request to http://inlb.app.adform.com:50054/v1/auth/login url with following application/json content:
{"Username":"user","Password":"password"}
b.	If you‘re credentials are valid, you will receive a JSON response:
{"Ticket":"ticket"}
c.	You will need this ticket for further interactions

2.	To retrieve MasterData file list you will need to:
a.	Send HTTP GET request to http://masterdata.adform.com:8652/list/xxx?render=json&authTicket=zzz 
b.	Where xxx is your MasterData id and zzz is authetication ticket your received in step #1
c.	If you have provided valid authorization ticket and you have right to access MasterData, you will receive a JSON response: 
{ "meta": { "path":"/download/xxx/meta.zip", "absolutePath":"http://masterdata.adform.com:8652/xxx/download/meta.zip", "size":"123", "created":"October 06, 2014 05:00:09" },
  "files": [ 
    { "path":"/download/xxx/TrackingPoints/file1.zip", "absolutePath":"http://masterdata.adform.com:8652/download/xxx/TrackingPoints/file1.zip", "size":"123", "created":"October 06, 2014 05:00:09" },
    { "path":"/download/xxx/TrackingPoints/file2.zip", "absolutePath":"http://masterdata.adform.com:8652/download/xxx/TrackingPoints/file2.zip", "size":"123", "created":"October 06, 2014 05:00:09" } 
  ] }
d.	MasterData meta file is a separate entry. It contains multiple files with meta information for your account. It is optional though and might be missing if service has just been enabled.
e.	Each file is represented by path – relative url to the file location, asolutePath – absolute url to file location, size – file size in bytes, create – date, when file was created.

3.	To download MasterData files you will need to:
a.	Send HTTP GET request to absolutePath of each file, that was listed in step #2. Please note, that you will need to attach autorization ticket to each absolutePath you received. 
b.	For example, whe you have a absolutePath: http://masterdata.adform.com:8652/download/xxx/TrackingPoints/file1.zip you will need to send request to http://masterdata.adform.com:8652/download/xxx/TrackingPoints/file1.zip?authTicket=zzz
c.	Open response stream and save contents to local file
