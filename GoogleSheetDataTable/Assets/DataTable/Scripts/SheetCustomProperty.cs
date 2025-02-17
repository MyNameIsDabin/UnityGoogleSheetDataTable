using System;
using TabbySheet;

namespace DataTables
{
    [Serializable]
    public class SheetCustomProperty : ISheetCustomProperty
    {
        public bool IsIgnore { get; set; } 
    }
}