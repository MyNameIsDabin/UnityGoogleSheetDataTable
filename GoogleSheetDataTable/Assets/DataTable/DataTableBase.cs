using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;

namespace DataTables
{
    public partial class DataTable<TTable, TData> : IEnumerable<TData> where TData : ISerializable where TTable : new()
    {
        protected static TTable instance;

        public static TTable Instance
        {
            get
            {
                if (instance == null)
                    instance = new TTable();

                return instance;
            }
        }

        public string ColumnName;

        public Type DataType;

        protected List<TData> datas = new List<TData>();

        public TData this[int index] => datas[index];

        public int Count => datas.Count;

        public IEnumerator<TData> GetEnumerator() => datas.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => datas.GetEnumerator();

        public DataTable() => LoadBinary();

        public virtual void LoadBinary()
        {
            datas = ExcelSheetHelper.LoadBinaryToList<TData>(
                Path.Combine(DataTableSettings.ToPlatformPath(DataTableSettings.RuntimeBinaryPath), $"{GetType().Name.Replace("Table", "")}"));
        }

        public TData GetRandom()
        {
            return datas[UnityEngine.Random.Range(0, datas.Count - 1)];
        }

        public TData[] GetShuffledData(int count, int? seed = null)
        {
            var random = new System.Random(seed ?? Time.frameCount);
            
            var randomIndexes = Enumerable.Range(0, datas.Count).ToArray();

            var shuffled = randomIndexes.OrderBy(x => random.Next());
            
            return shuffled.Take(Mathf.Min(count, datas.Count)).Select(i => datas[i]).ToArray();
        }
    }
}