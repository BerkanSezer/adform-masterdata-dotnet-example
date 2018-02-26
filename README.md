adform-masterdata-dotnet-example
================================
### General
This is an example of .net C# application, which explains how to consume Master Data file service.
For this application to work, you need:
  * Agency account with permission to access Adform External API (https://api.adform.com/)
  * Master Data service enabled 
  
Once the contract is signed Adform will enable Master Data service and send you the credentials. Please note, that accessing External API places additional requirements on your account security and password complexity.
Next steps:
 * Clone git repository or download zip file
 * Compile it and open it in command line
 * Provide 4 parameters: setupId, login, password, where to store files

Please contact Adform support, if you have any questions.

**NOTE:** You can also use WEB UI (https://www.adform.com/masterdata?setup={setupId}), where ```{setupId}``` is your unique Master Data setup identificator, to preview and download files using WEB browser.
  
### Instructions (adviced to read if you won`t use .net C# solution)
#### To automate Master Data download you will be required to perform these steps:
1. Authenticate with Adform security and receive authorization ticket, which will be used for further interactions 
2. Retrieve file list for your clients
3. Download each file individually
#### More technical details on the steps:
1. To Authenticate with Adform security you will need to:
  * Send HTTP **POST** request to https://api.adform.com/Services/Security/Login URL with following ```application/json``` content: 
      ```JSON
      {"Username":"user","Password":"password"}
      ```
  * If your credentials are valid, you will receive a JSON response: 
      ```JSON
      {"Ticket":"ticket"}
      ```
  * You will need this ticket for further interactions
2. To retrieve Master Data file list you will need to:
  * Send HTTP **GET** request to https://api.adform.com/v1/buyer/masterdata/files/{setupId} with cookie ```authTicket={ticketValue}```, where ```{setupId}``` is your unique Master Data setup identificator and ```{ticketValue}``` is the authentication ticket you received in **step #1**
  * If you have provided a valid authorization ticket and you have right to access Master Data, you will receive a JSON response with an available files list: 
    
      ```JSON
      [
    {
        "name": "Click_7777.csv.gz",
        "id": "Click_7777__csv__gz",
        "setup": "c8599080-49e7-4a3b-b5bb-1811f7ed514a",
        "size": 27946,
        "createdAt": "2017-06-05T11:00:30Z",
        "checksumMD5": "9e0e2fef866f6529a71eb9cc89001a77"
    },
    {
        "name": "meta.zip",
        "id": "meta__zip",
        "setup": "c8599080-49e7-4a3b-b5bb-1811f7ed514a",
        "size": 69311391,
        "createdAt": "2017-06-05T10:31:30Z",
        "checksumMD5": "c12eee8f4f55a7894ea28649d7ad78fd"
    },
    {
        "name": "Trackingpoint_8888.csv.gz",
        "id": "Trackingpoint_8888__csv__gz",
        "setup": "c8599080-49e7-4a3b-b5bb-1811f7ed514a",
        "size": 54669,
        "createdAt": "2017-06-05T10:03:20Z",
        "checksumMD5": "641d06bfbcec1ef5c50df06f70c4624a"
    }
    ]
    ```
  * The returned list contains the prepared log level data files and additional meta file. It contains multiple files with meta information for your account. It is optional though, and might be missing if service has just been enabled.
  * Each file is represented by: 
      * ```name``` – file name
      * ```id``` – unique file identificator
      * ```setup``` – master data setup id the file belongs to
      * ```size``` – file size in bytes
      * ```createdAt``` – date, when file was created
3. To download Master Data files you will need to:
  * Send HTTP **GET** request to https://api.adform.com/v1/buyer/masterdata/download/{setupId}/{fileId} with cookie ```authTicket={ticketValue}```, where ```{setupId}``` is your unique Master Data setup identificator, ```{fileId}``` is the unique file identificator, that was listed in **step #2** and ```{ticketValue}``` is the authentication ticket you received in **step #1**.
  * Open response stream and save content to local file.
