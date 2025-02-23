using Godot;
using TabbySheet;

public partial class Demo : Node
{
	public override void _Ready()
	{
		DataSheet.SetDataTableAssetLoadHandler(OnDataTableLoadHandler);
		
		 foreach (var foodData in DataSheet.Load<FoodsTable>())
		 	GD.Print(foodData.Name);
	}
	
	private static byte[] OnDataTableLoadHandler(string sheetName)
	{
		GD.Print($"res://tableData/{sheetName}");
		var fileArray = FileAccess.GetFileAsBytes($"res://tableData/{sheetName}.bytes");
		return fileArray;
	}
}
