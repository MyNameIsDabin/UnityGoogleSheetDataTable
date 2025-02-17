using System;
using System.Data;
using System.IO;
using DataTables;
using TabbySheet;
using UnityEngine;
using UnityEditor;

[Serializable]
public class DataTableSettings : ScriptableObject
{
    public const string RuntimeBinaryPath = "DataTableBinary";
    
    public string GoogleSheetURL = "https://docs.google.com/spreadsheets/d/000000~~/edit#gid=000000000";
    public string CredentialJsonPath = "Assets/DataTable/Editor/credentials.json";
    public string DownloadDirectory = "Assets/DataTable/Plugins/ExcelSheets";
    public string ExportClassFileDirectory = "Assets/DataTable/Tables";
    public string ExportBinaryDirectory = "Assets/DataTable/Resources";
    public bool IsDebugMode;
    
    [HideInInspector]
    public ExcelSheetFileMeta DownloadedSheet;
    
    [HideInInspector] 
    public bool UseOptions = true;
    
    public enum GUIAlign
    {
        Left, 
        Middle, 
        Right
    }

    private Vector2 _scrollPosition;

    public class ExcelMetaAssigner : IExcelMetaAssigner
    {
        public ISheetCustomProperty Assign(System.Data.DataTable dataTable) => new SheetCustomProperty();
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
                                sheetInfo.CustomProperties = new SheetCustomProperty { IsIgnore = true };
                        }

                        if (GUILayout.Button("Check All", GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            foreach (var sheetInfo in DownloadedSheet.SheetInfos)
                                sheetInfo.CustomProperties = new SheetCustomProperty { IsIgnore = false };
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

                        if (!(sheetInfo.CustomProperties is SheetCustomProperty sheetCustomProperty)) 
                            continue;
                        
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        {
                            var isToggle = GUILayout.Toggle(!sheetCustomProperty.IsIgnore, "", GUILayout.Width(15));

                            if (!sheetCustomProperty.IsIgnore.Equals(!isToggle))
                                sheetCustomProperty.IsIgnore = !isToggle;
                            
                            EditorGUI.BeginDisabledGroup(sheetCustomProperty.IsIgnore);
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
                var downloadResult = GoogleSheet.DownloadExcelFile(
                    Application.companyName, 
                    CredentialJsonPath, 
                    DownloadDirectory, 
                    GoogleSheetURL, out var outputPath);
                
                if (downloadResult != GoogleSheet.DownloadResult.Success)
                    return;
                
                var sheetInfo = ExcelSheetFileMeta.LoadFromFile(outputPath, new ExcelMetaAssigner());

                if (sheetInfo != null)
                {
                    DownloadedSheet = sheetInfo;
                    AssetDatabase.SaveAssets();
                }
            }

            if (DownloadedSheet != null)
            {
                EditorGUI.BeginDisabledGroup(false);

                var generateHandler = new DataTableAssetGenerator.GenerateHandler
                {
                    Predicate = sheetInfo => sheetInfo.CustomProperties is SheetCustomProperty { IsIgnore: false },
                };
                
                if (GUILayout.Button("Create Class Files", GUILayout.Height(30)))
                {
                    DataTableAssetGenerator.GenerateClassesFromExcel(DownloadedSheet, ExportClassFileDirectory, generateHandler);
                }
                
                if (GUILayout.Button("Export Binary", GUILayout.Height(30)))
                {
                    DataTableAssetGenerator.GenerateBinaryFromExcel(DownloadedSheet, ExportBinaryDirectory, generateHandler);
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
