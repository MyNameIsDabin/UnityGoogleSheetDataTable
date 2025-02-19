namespace TabbySheet
{
    public interface ISheetInfo
    {
        public string Name { get; set; }
        public int Rows { get; set; }
    }
    
    public interface ISheetInfo<T> : ISheetInfo where T : ISheetCustomProperty
    {
        public T CustomProperties { get; set; }
    }
}