using System;
using DataTables;
using TabbySheet;

[Serializable]
public class ExcelSheetInfo : ISheetInfo<CustomSheetProperty>
{
    public string Name { get; set; }
    public int Rows { get; set; }
    public CustomSheetProperty CustomProperties { get; set; }
}