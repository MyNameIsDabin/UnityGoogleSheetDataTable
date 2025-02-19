namespace TabbySheet
{
    public interface IExcelMetaAssigner<T>
    {
        public T Assign(System.Data.DataTable dataTable);
    }
}