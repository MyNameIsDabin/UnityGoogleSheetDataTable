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
    public bool IsDebugMode;
    
    [HideInInspector]
    public ExcelSheetHelper.ExcelMeta DownloadedSheet;
    
    [HideInInspector] 
    public bool UseOptions = true;

    public static string ToPlatformPath(string path) => path.Replace(@"\", Path.DirectorySeparatorChar.ToString());
    
    public enum GUIAlign
    {
        Left, 
        Middle, 
        Right
    }

    private Vector2 _scrollPosition;
    
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

            if (DownloadedSheet != null && !string.IsNullOrEmpty(DownloadedSheet.FilePath))
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    richText = true,
                    fontSize = 12
                };

                GUILayout.Space(3);
                GUIAlignText(GUIAlign.Middle, $"<color=yellow>{Path.GetFileNameWithoutExtension(DownloadedSheet.FilePath)}</color> is loaded.", style);
                GUILayout.Space(3);
                
                GUILayout.BeginVertical(GUI.skin.window, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(80));
                    {
                        if (GUILayout.Button("UnCheck All", GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            foreach (var sheetInfo in DownloadedSheet.SheetInfos)
                                sheetInfo.IsIgnore = true;
                        }

                        if (GUILayout.Button("Check All", GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            foreach (var sheetInfo in DownloadedSheet.SheetInfos)
                                sheetInfo.IsIgnore = false;
                        }
                        
                        GUILayout.FlexibleSpace();
                        
                        var isToggle = GUILayout.Toggle(IsDebugMode, "Debug Mode", GUILayout.Width(15), GUILayout.Height(20));
                        
                        if (!IsDebugMode.Equals(isToggle))
                            IsDebugMode = isToggle;
                        
                        GUILayout.Label("Debug Mode", GUILayout.Height(18));
                    }
                    GUILayout.EndHorizontal();
                    
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
                    
                    foreach (var sheetInfo in DownloadedSheet.SheetInfos)
                    {
                        if (sheetInfo.Name.StartsWith("#"))
                            continue;
                        
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        {
                            var isToggle = GUILayout.Toggle(!sheetInfo.IsIgnore, "", GUILayout.Width(15));

                            if (!sheetInfo.IsIgnore.Equals(!isToggle))
                                sheetInfo.IsIgnore = !isToggle;
                            
                            EditorGUI.BeginDisabledGroup(sheetInfo.IsIgnore);
                            GUILayout.Label(sheetInfo.Name, GUILayout.ExpandWidth(true), GUILayout.Height(15));
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Download Google Sheet", GUILayout.Height(30)))
            {
                if (!GoogleSheetHelper.DownloadToExcel(
                        ToPlatformPath(CredentialJsonPath), 
                        ToPlatformPath(DownloadDirectory), GoogleSheetURL, out var excelPath))
                    return;

                if (string.IsNullOrEmpty(excelPath))
                    return;
                
                var sheetInfo = ExcelSheetHelper.LoadExcelMeta(excelPath);

                if (sheetInfo != null)
                {
                    DownloadedSheet = sheetInfo;
                    AssetDatabase.SaveAssets();
                }
            }

            if (DownloadedSheet != null)
            {
                EditorGUI.BeginDisabledGroup(false);
                
                if (GUILayout.Button("Create Table Class", GUILayout.Height(30)))
                {
                    ExcelSheetHelper.ConvertExcelToClassFiles(DownloadedSheet, ToPlatformPath(ExportClassFileDirectory));
                }
                
                if (GUILayout.Button("Export Binary", GUILayout.Height(30)))
                {
                    ExcelSheetHelper.ExportBinaryFromExcel(DownloadedSheet, Path.Combine(ToPlatformPath(ExportBinaryDirectory), RuntimeBinaryPath), IsDebugMode);
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
