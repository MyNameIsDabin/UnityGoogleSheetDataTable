namespace TabbySheet
{
    public interface ISheetInfo
    {
        public string Name { get; }
        public int Rows { get; }
        public ISheetCustomProperty CustomProperties { get; set; }
    }
}