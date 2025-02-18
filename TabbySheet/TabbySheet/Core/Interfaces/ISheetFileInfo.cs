using System.Collections.ObjectModel;

namespace TabbySheet
{
    public interface ISheetFileMeta
    {
        public string FilePath { get; }
        public ObservableCollection<ISheetInfo> SheetInfos { get; }
        public ISheetInfo GetSheetInfoOrNullByName(string sheetName);
    }
}