using TabbySheet;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DataTableWindow : EditorWindow
{
    private static readonly string EditorSettingFilePath = "Assets/DataTable/Resources/DataTableSettings.asset";
    
    private static DataTableSettings _dataTableSettings;

    static DataTableWindow()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        if (Application.isPlaying)
            return;
        
        DataSheet.SetDataTableAssetLoadHandler(OnDataTableLoadHandlerEditor);
    }
    
    private static byte[] OnDataTableLoadHandlerEditor(string sheetName)
    {
        var asset = Resources.Load($"DataTableBinary/{sheetName}", typeof(TextAsset)) as TextAsset;
        return asset!.bytes;
    }
    
    [MenuItem("Tools/DataTable #t")]
    static void ShowWindow() 
    {
        LoadSettings();

        var window = GetWindow(typeof(DataTableWindow));
        window.titleContent = new GUIContent("GoogleDataTable");
        window.Show();
    }

    [MenuItem("Tools/Test")]
    static void Test()
    {
        foreach (var data in DataSheet.Load<FoodsTable>())
        {
            Debug.Log(data.Name);
        }
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        LoadSettings();
    }
    
    private void OnGUI()
    {
        if (_dataTableSettings == null)
        {
            LoadSettings();
            return;
        }
        
        _dataTableSettings.OnGUI();
            
        if (GUI.changed)
            SaveSettings();
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private static DataTableSettings DataTableSettings
    {
        get
        {
            var settings = AssetDatabase.LoadAssetAtPath(EditorSettingFilePath, typeof(DataTableSettings)) as DataTableSettings;

            if (settings != null) 
                return settings;
            
            var asset = CreateInstance<DataTableSettings>();
            AssetDatabase.CreateAsset(asset, EditorSettingFilePath);
            
            return asset;
        }
    }

    private static void SaveSettings()
    {
        EditorUtility.SetDirty(DataTableSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void LoadSettings()
    {
        _dataTableSettings = DataTableSettings;
        
        TabbySheet.Logger.SetLogAction(Debug.Log);
    }
}
