using System.IO;
using System.Threading;
using UnityEngine;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
#endif

public class GoogleSheetHelper
{
    private static readonly string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

#if UNITY_EDITOR
    public static void DownloadToExcel(string credentialPath, string downloadPath, string googleSheetUrl, out string fileName)
    {
        UserCredential credential;

        using (var stream = new FileStream($"{Path.Combine(Application.dataPath, credentialPath)}", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                new [] { SheetsService.Scope.SpreadsheetsReadonly, DriveService.Scope.DriveReadonly },
                Application.companyName,
                CancellationToken.None).Result;
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

        FilesResource.ExportRequest request = driveService.Files.Export(spreadSheet.SpreadsheetId, ExcelMimeType);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            request.Download(memoryStream);

            fileName = driveService.Files.Get(spreadSheet.SpreadsheetId).Execute().Name;

            var excelPath = Path.Combine(downloadPath, $"{fileName}.xlsx");
            
            using (FileStream file = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(file);

                Debug.Log($"Download Success : {file.Name}");
            }
        }
    }
#endif
}
