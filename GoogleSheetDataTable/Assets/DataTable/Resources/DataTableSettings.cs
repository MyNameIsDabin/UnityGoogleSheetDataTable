using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DataTableSettings : ScriptableObject
{
    public const string RuntimeBinaryPath = "DataTableBinary";
    
    public string GoogleSheetURL = @"https://docs.google.com/spreadsheets/d/000000~~/edit#gid=000000000";
    public string CredentialJsonPath = @"DataTable\credentials.json";
    public string DownloadDirectory = @"Assets\DataTable\Plugins\ExcelSheets";
    public string ExportClassFileDirectory = @"Assets\DataTable\Tables";
    public string ExportBinaryDirectory = @"Assets\DataTable\Resources";
    public string SheetName = string.Empty;
    
    [HideInInspector] public bool UseOptions = true;
    [HideInInspector] public TableProcessStep Step = TableProcessStep.Ready;
    
    public static string ToPlatformPath(string path) => path.Replace(@"\", Path.DirectorySeparatorChar.ToString());

    public enum TableProcessStep
    {
        Ready,
        DownloadSheet,
        CreateSingleton,
        ExportBinaries
    }
    
    public enum GUIAlign
    {
        Left, 
        Middle, 
        Right
    }
    
    public void OnGUI()
    {
#if UNITY_EDITOR
        GUILayout.BeginVertical("HelpBox");
        {
            GUILayout.BeginVertical();
            
            GUILayout.Space(8);
            GUIAlignText(GUIAlign.Middle,"Google Sheet Settings", EditorStyles.boldLabel);
            GUILayout.Space(8);
            
            var useOptions = EditorGUI.BeginFoldoutHeaderGroup(new Rect(14, 14, 44, 100), UseOptions, "Edit");

            if (useOptions != UseOptions)
            {
                UseOptions = useOptions;
                GUI.FocusControl("0");
            }
            
            EditorGUI.BeginDisabledGroup(!UseOptions);

            GoogleSheetURL = EditorGUILayout.TextField("GoogleSheet URL", GoogleSheetURL);
                
            CredentialJsonPath = EditorGUILayout.TextField("Credential.json Path", CredentialJsonPath);
            
            EditorGUI.EndDisabledGroup();

            if (UseOptions)
            {
                DownloadDirectory = EditorGUILayout.TextField("Excel Download Directory", DownloadDirectory);

                ExportClassFileDirectory = EditorGUILayout.TextField("Created Class Directory", ExportClassFileDirectory);
                
                ExportBinaryDirectory = EditorGUILayout.TextField("Export Binary Directory", ExportBinaryDirectory);
            }
            
            GUILayout.EndVertical();
            
            EditorGUI.EndFoldoutHeaderGroup();

            if (!string.IsNullOrEmpty(SheetName))
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    richText = true
                };
                style.fontSize = 12;

                GUILayout.Space(3);
                GUIAlignText(GUIAlign.Middle, $"<color=yellow>{SheetName}</color> is loaded.", style);
                GUILayout.Space(3);
            }

            if (GUILayout.Button("Download Google Sheet", GUILayout.Height(30)))
            {
                GoogleSheetHelper.DownloadToExcel(ToPlatformPath(CredentialJsonPath), ToPlatformPath(DownloadDirectory),
                    GoogleSheetURL, out var fileName);

                SheetName = fileName;
                
                Step = TableProcessStep.DownloadSheet;
            }

            if (!string.IsNullOrEmpty(SheetName))
            {
                EditorGUI.BeginDisabledGroup(Step < TableProcessStep.DownloadSheet);
                
                if (GUILayout.Button("Create Table Class", GUILayout.Height(30)))
                {
                    ExcelSheetHelper.ConvertExcelToClassFiles(
                        Path.Combine(ToPlatformPath(DownloadDirectory), $"{SheetName}.xlsx"),
                        ToPlatformPath(ExportClassFileDirectory));

                    Step = TableProcessStep.CreateSingleton;
                }
                
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup(Step < TableProcessStep.CreateSingleton);
                
                if (GUILayout.Button("Export Binary", GUILayout.Height(30)))
                {
                    ExcelSheetHelper.ExportBinaryFromExcel(
                        Path.Combine(ToPlatformPath(DownloadDirectory), $"{SheetName}.xlsx"),
                        Path.Combine(ToPlatformPath(ExportBinaryDirectory), RuntimeBinaryPath));
                    
                    Step = TableProcessStep.ExportBinaries;
                }
                
                EditorGUI.EndDisabledGroup();
            }
        }
        GUILayout.EndVertical();
        
        void GUIAlignText(GUIAlign guiAlign, string text, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            if (guiAlign == GUIAlign.Middle || guiAlign == GUIAlign.Right)
                GUILayout.FlexibleSpace();
            GUILayout.Label(text, style);
            if (guiAlign == GUIAlign.Middle || guiAlign == GUIAlign.Left)
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
#endif
    }
}
