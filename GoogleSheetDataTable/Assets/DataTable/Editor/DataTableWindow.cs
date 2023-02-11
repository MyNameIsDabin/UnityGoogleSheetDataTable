using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DataTableWindow : EditorWindow
{
    public static string EditorSettingFilePath = "Assets/DataTable/Resources/DataTableSettings.asset";
    public static DataTableSettings dataTableSettings;

    [MenuItem("Tools/DataTable #t")]
    static void ShowWindow() 
    {
        LoadSettings();

        var window = GetWindow(typeof(DataTableWindow));
        window.titleContent = new GUIContent("GoogleDataTable ");
        window.Show();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void OnScriptsReloaded()
    {
        LoadSettings();
    }
    
    private void OnGUI()
    {
        if (dataTableSettings != null)
            dataTableSettings.OnGUI();

        if (GUI.changed)
            SaveSettings();
    }

    private void OnDestroy()
    {
        SaveSettings();
    }

    private static void SaveSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath(EditorSettingFilePath, typeof(DataTableSettings)) as DataTableSettings;
        
        if (dataTableSettings == null)
        {
            if (settings == null)
            {
                dataTableSettings = CreateInstance<DataTableSettings>();
                AssetDatabase.CreateAsset(dataTableSettings, EditorSettingFilePath);   
            }
            else
            {
                dataTableSettings = settings;
            }
        }

        EditorUtility.SetDirty(dataTableSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void LoadSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath(EditorSettingFilePath, typeof(DataTableSettings)) as DataTableSettings;

        if (settings == null)
            SaveSettings();
        else
            dataTableSettings = settings;
    }
}
