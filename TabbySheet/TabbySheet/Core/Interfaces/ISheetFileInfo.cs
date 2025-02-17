using System.Collections.Generic;

namespace TabbySheet
{
    public interface ISheetFileMeta
    {
        public string FilePath { get; }
        public List<ISheetInfo> SheetInfos { get; }
        public ISheetInfo GetSheetInfoOrNullByName(string sheetName);
    }
}