using System.IO;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
#endif

public class GoogleSheetHelper
{
    private static readonly string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

#if UNITY_EDITOR
    public static bool DownloadToExcel(string credentialPath, string downloadPath, string googleSheetUrl, out string filePath)
    {
        UserCredential credential = null;
        filePath = null;
        
        var credentialFullPath = Path.Combine(Application.dataPath, credentialPath);

        if (File.Exists(credentialFullPath) == false)
        {
            Debug.LogError("You need OAuth client JSON file for Google API access. For more information, visit the link at <a href=\"https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable\">https://github.com/MyNameIsDabin/UnityGoogleSheetDataTable</a>.");
            return false;
        }

        using (var stream = new FileStream(credentialFullPath, FileMode.Open, FileAccess.Read))
        {
            Debug.Log("Requests Google OAuth access on the web.");

            var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(TimeSpan.FromSeconds(30));
            
            try
            {
                var authorizeTask = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { SheetsService.Scope.SpreadsheetsReadonly, DriveService.Scope.DriveReadonly },
                    Application.companyName,
                    cancellationToken.Token);
                
                authorizeTask.Wait(cancellationToken.Token);
                credential = authorizeTask.Result;

                if (cancellationToken.IsCancellationRequested)
                    return false;
            }
            catch (Exception e)
            {
                Debug.Log($"The permission request has been canceled. {e.Message}");
                filePath = null;
                return false;
            }
        }
        
        var serviceInitializer = new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = Application.productName,
        };

        var sheetService = new SheetsService(serviceInitializer);

        var matches = Regex.Matches(googleSheetUrl, @"(https://docs.google.com/spreadsheets/d/)(.*)(\/.*)");
        var sheetId = matches[0].Groups[2].Value;
        var spreadSheet = sheetService.Spreadsheets.Get(sheetId).Execute();

        var driveService = new DriveService(serviceInitializer);
        
        var request = driveService.Files.Export(spreadSheet.SpreadsheetId, ExcelMimeType);

        using (var memoryStream = new MemoryStream())
        {
            request.Download(memoryStream);
            
            var fileName = driveService.Files.Get(spreadSheet.SpreadsheetId).Execute().Name;
            filePath = Path.Combine(downloadPath, $"{fileName}.xlsx");
            
            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(file);
                Debug.Log($"Download Success : {file.Name}");
            }
        }

        return true;
    }
#endif
}
