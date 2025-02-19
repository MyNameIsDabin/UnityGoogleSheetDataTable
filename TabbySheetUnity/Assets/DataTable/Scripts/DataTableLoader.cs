using TabbySheet;
using UnityEngine;

public static class TabbyDataSheet
{
    private static DataSheet.DataTableAssetLoadHandler AssetLoadHandler => OnDataTableLoadHandler;

    public static void Init()
    {
        DataSheet.SetDataTableAssetLoadHandler(AssetLoadHandler);
    }
    
    private static byte[] OnDataTableLoadHandler(string sheetName)
    {
        var asset = Resources.Load($"DataTableBinary/{sheetName}", typeof(TextAsset)) as TextAsset;
        return asset!.bytes;
    }
}