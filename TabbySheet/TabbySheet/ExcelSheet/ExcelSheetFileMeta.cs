using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExcelDataReader;

namespace TabbySheet
{
    [Serializable]
    public class ExcelSheetFileMeta : ISheetFileMeta
    {
        public class SheetInfo : ISheetInfo
        {
            public string Name { get; set; }
            public int Rows { get; set; }
            public ISheetCustomProperty CustomProperties { get; set; }
        }
        
        public string FilePath { get; private set; }
        public List<ISheetInfo> SheetInfos { get; } = new ();

        public ExcelDataSetConfiguration ExcelDataSetConfiguration { get; private set; }
        
        public ISheetInfo GetSheetInfoOrNullByName(string sheetName) => SheetInfos.FirstOrDefault(x => x.Name == sheetName);
        
        public static ExcelSheetFileMeta LoadFromFile(string excelPath, IExcelMetaAssigner excelMetaAssigner = null)
        {
            var sheetFileMeta = new ExcelSheetFileMeta
            {
                FilePath = excelPath,
                ExcelDataSetConfiguration = CreateExcelDataSetConfiguration()
            };

            using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(sheetFileMeta.ExcelDataSetConfiguration);

            foreach (System.Data.DataTable table in result.Tables)
            {
                sheetFileMeta.SheetInfos.Add(new SheetInfo
                {
                    Name = table.TableName,
                    Rows = table.Rows.Count,
                    CustomProperties = excelMetaAssigner?.Assign(table)
                });
            }

            return sheetFileMeta;
        }
        
        private static ExcelDataSetConfiguration CreateExcelDataSetConfiguration()
        {
            return new ExcelDataSetConfiguration
            {
                UseColumnDataType = true,
                FilterSheet = (_, _) => true,
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                    ReadHeaderRow = rowReader => rowReader.Read(),
                    FilterRow = _ => true,
                    FilterColumn = (_, _) => true
                }
            };
        }
    }
}