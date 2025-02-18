using System;
using TabbySheet;

namespace DataTables
{
    [Serializable]
    public class CustomSheetProperty : ISheetCustomProperty
    {
        public bool IsIgnore { get; set; } 
    }
}