namespace TabbySheet
{
    public interface IExcelMetaAssigner
    {
        public ISheetCustomProperty Assign(System.Data.DataTable dataTable);
    }
}