using System;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace TabbySheet
{
    public class GoogleSheet 
    {
        public enum DownloadResult
        {
            Success = 0,
            NotFoundCredentialFile = 1,
            Cancelled = 2,
            UnknownError = 3
        }
        
        private static readonly string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        
        public static DownloadResult DownloadExcelFile(
            string appName, 
            string credentialPath, 
            string downloadPath, 
            string googleSheetUrl, 
            out string outputPath)
        {
            UserCredential credential;
            outputPath = null;
            
            if (File.Exists(credentialPath) == false)
            {
                Logger.Log("For accessing the Google API, an OAuth Client JSON file is required. For more details, please refer to the README.md.");
                return DownloadResult.NotFoundCredentialFile;
            }

            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                Logger.Log("Google sign-in attempt.");

                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(30));
                
                try
                {
                    var authorizeTask = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { SheetsService.Scope.SpreadsheetsReadonly, DriveService.Scope.DriveReadonly },
                        appName,
                        cancellationToken.Token);
                    
                    authorizeTask.Wait(cancellationToken.Token);
                    credential = authorizeTask.Result;

                    if (cancellationToken.IsCancellationRequested)
                        return DownloadResult.Cancelled;
                }
                catch (Exception e)
                {
                    Logger.Log($"The permission request has been canceled. {e.Message}");
                    return DownloadResult.Cancelled;
                }
                
                Logger.Log("Google login successful.");
            }
            
            try
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = appName,
                };
                
                var sheetService = new SheetsService(serviceInitializer);
                var matches = Regex.Matches(googleSheetUrl, @"(https://docs.google.com/spreadsheets/d/)(.*)(\/.*)");
                var sheetId = matches[0].Groups[2].Value;
                var spreadSheet = sheetService.Spreadsheets.Get(sheetId).Execute();

                var driveService = new DriveService(serviceInitializer);
                var request = driveService.Files.Export(spreadSheet.SpreadsheetId, ExcelMimeType);

                using var memoryStream = new MemoryStream();
                request.Download(memoryStream);
                
                var fileName = driveService.Files.Get(spreadSheet.SpreadsheetId).Execute().Name;
                outputPath = Path.Combine(downloadPath, $"{fileName}.xlsx");
                using var file = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                memoryStream.WriteTo(file);
            }
            catch (Exception)
            {
                return DownloadResult.UnknownError;
            }

            return DownloadResult.Success;
        }
    }   
}
