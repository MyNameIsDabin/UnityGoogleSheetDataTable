﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using ExcelDataReader;
#endif

public class ExcelSheetHelper : MonoBehaviour
{
    [Serializable]
    public class ExcelMeta
    {
        [Serializable]
        public class SheetInfo
        {
            public string Name;
            public int Rows;
            public bool IsIgnore;
        }

        public string FilePath;
        public string Name => Path.GetFileNameWithoutExtension(FilePath);
        public List<SheetInfo> SheetInfos = new List<ExcelMeta.SheetInfo>();

        public SheetInfo GetSheetInfoOrNullByName(string sheetName)
        {
            return SheetInfos.FirstOrDefault(x => x.Name == sheetName);
        }
    }
    
    private enum RowTypeOfIndex
    {
        Header,
        Name,
        Type,
        Option,
        DataBegin
    }

    public enum Option
    {
        UniqueKey
    }

    private static readonly string BinaryExportEncryptPassword = "BinaryPassword";
    private static readonly string[] SupportedExtensions = new[] { ".XLS", ".XLSX" };

#if UNITY_EDITOR
    public static ExcelMeta LoadExcelMeta(string excelPath)
    {
        if (!IsExcelFile(excelPath))
            return null;

        var sheetFileMeta = new ExcelMeta
        {
            FilePath = excelPath
        };

        using var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        
        var result = reader.AsDataSet(CreateExcelDataSetConfiguration());

        foreach (DataTable table in result.Tables)
        {
            sheetFileMeta.SheetInfos.Add(new ExcelMeta.SheetInfo
            {
                Name = table.TableName,
                Rows = table.Rows.Count,
                IsIgnore = false
            });
        }

        return sheetFileMeta;
    }
    
    public static void ConvertExcelToClassFiles(ExcelMeta excelMeta, string exportPath)
    {   
        if (!IsExcelFile(excelMeta.FilePath))
            return;

        using (var stream = File.Open(excelMeta.FilePath, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            Debug.Log("Convert excel to class files");

            var result = reader.AsDataSet(CreateExcelDataSetConfiguration());

            foreach (DataTable table in result.Tables)
            {
                var sheetInfo = excelMeta.GetSheetInfoOrNullByName(table.TableName);

                if (sheetInfo == null || sheetInfo.IsIgnore)
                {
                    Debug.Log($"[Excel: {table.TableName}] is Ignored");
                    continue;
                }
                
                Debug.Log($"[Excel: {table.TableName}] (length : {table.Rows.Count})");
                
                if (table.TableName.StartsWith("#"))
                {
                    Debug.Log($"[Excel: {table.TableName}] is Ignored (table name is started with '#')");
                    continue;
                }

                var className = $"{table.TableName}Table";
                var generateFilePath = Path.Combine(exportPath, $"{className}.cs");
                var templatePath = Path.Combine("Assets", "DataTable", "DataTableTemplate.txt");

                using (var classStream = new StreamWriter(new FileStream(generateFilePath, FileMode.Create)))
                {
                    var textTemplate = File.ReadAllText(templatePath);

                    for (var row = 0; row < table.Rows.Count; row++)
                    {
                        if ((RowTypeOfIndex)row == RowTypeOfIndex.Name)
                        {
                            var fieldsBuilder = new StringBuilder();
                            var funcGetObjectDataBuilder = new StringBuilder();
                            var constructorBuilder = new StringBuilder();
                            var primaryKeyBuilder = new StringBuilder();
                            var primaryDictionaryBuilder = new StringBuilder();
                            var primaryConstructorBuilder = new StringBuilder();
                            var primaryFunctionsBuilder = new StringBuilder();

                            for (var column = 0; column < table.Columns.Count; column++)
                            {
                                var fieldName = table.Rows[row][column].ToString();
                                var fieldType = table.Rows[row + 1][column].ToString();
                                var fieldOption = table.Rows[row + 2][column].ToString();

                                if (string.IsNullOrEmpty(fieldName) || fieldName.StartsWith("#"))
                                    continue;

                                if (TryGetTypeFromString(fieldType, out var fieldTypeName, out var type))
                                {
                                    var fieldTypeLower = fieldTypeName;

                                    fieldsBuilder.Append($"\t\t\tpublic {fieldTypeLower} {fieldName} {{ get; set; }}\n");
                                    funcGetObjectDataBuilder.Append($"\t\t\t\tinfo.AddValue(\"{fieldName}\", {fieldName});\n");
                                    constructorBuilder.Append($"\t\t\t\t{fieldName} = ({fieldTypeLower})info.GetValue(\"{fieldName}\", typeof({fieldTypeLower}));\n");

                                    if (Enum.TryParse<Option>(fieldOption, out var option))
                                    {
                                        switch(option)
                                        {
                                            case Option.UniqueKey:
                                                {
                                                    primaryKeyBuilder.Append($"\t\t\t{fieldName},\n");

                                                    var dictionaryName = $"{ToCamelCase(fieldName)}ToData";
                                                    primaryDictionaryBuilder.Append($"\t\tprivate Dictionary<{fieldTypeLower}, Data> {dictionaryName} = new Dictionary<{fieldTypeLower}, Data>();\n");
                                                    primaryFunctionsBuilder.Append($"\t\tpublic Data GetDataBy{fieldName}({fieldTypeLower} {ToCamelCase(fieldName)})\n");
                                                    primaryFunctionsBuilder.Append("\t\t{\n");
                                                    primaryFunctionsBuilder.Append($"\t\t\treturn {dictionaryName}[{ToCamelCase(fieldName)}];\n");
                                                    primaryFunctionsBuilder.Append("\t\t}\n\n");

                                                    primaryConstructorBuilder.Append("\t\t\tforeach(var data in datas)\n");
                                                    primaryConstructorBuilder.Append($"\t\t\t\t{dictionaryName}.Add(data.{fieldName}, data);\n\n");
                                                }
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"This is not supported type : {fieldType}");
                                }
                            }

                            fieldsBuilder.Remove(fieldsBuilder.ToString().LastIndexOf('\n'), 1);
                            funcGetObjectDataBuilder.Remove(funcGetObjectDataBuilder.ToString().LastIndexOf('\n'), 1);
                            constructorBuilder.Remove(constructorBuilder.ToString().LastIndexOf('\n'), 1);
                            primaryKeyBuilder.Remove(primaryKeyBuilder.ToString().LastIndexOf(",\n"), 2);
                            primaryDictionaryBuilder.Remove(primaryDictionaryBuilder.ToString().LastIndexOf('\n'), 1);
                            primaryFunctionsBuilder.Remove(primaryFunctionsBuilder.ToString().LastIndexOf("\n\n"), 2);
                            primaryConstructorBuilder.Remove(primaryConstructorBuilder.ToString().LastIndexOf("\n\n"), 2);

                            textTemplate = textTemplate.Replace("@ClassName", className);
                            textTemplate = textTemplate.Replace("@EnumList", primaryKeyBuilder.ToString());
                            textTemplate = textTemplate.Replace("@PrimaryDictionary", primaryDictionaryBuilder.ToString());
                            textTemplate = textTemplate.Replace("@Fields", fieldsBuilder.ToString());
                            textTemplate = textTemplate.Replace("@DataConstructor", constructorBuilder.ToString());
                            textTemplate = textTemplate.Replace("@PrimaryConstructor", primaryConstructorBuilder.ToString());
                            textTemplate = textTemplate.Replace("@GetObjectData", funcGetObjectDataBuilder.ToString());
                            textTemplate = textTemplate.Replace("@PrimaryFunctions", primaryFunctionsBuilder.ToString());
                        }
                    }

                    classStream.Write(textTemplate);

                    Debug.Log($"Create class file : {generateFilePath}");
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif

#if UNITY_EDITOR
    public static void ExportBinaryFromExcel(ExcelMeta excelMeta, string exportedPath, bool isDebugMode)
    {
        if (!IsExcelFile(excelMeta.FilePath))
            return;

        AssetDatabase.Refresh();

        using (var stream = File.Open(excelMeta.FilePath, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            Debug.Log("Export Binary");

            var result = reader.AsDataSet(CreateExcelDataSetConfiguration());

            foreach (DataTable table in result.Tables)
            {
                var sheetInfo = excelMeta.GetSheetInfoOrNullByName(table.TableName);

                if (sheetInfo == null || sheetInfo.IsIgnore)
                {
                    Debug.Log($"[Excel: {table.TableName}] is Ignored");
                    continue;
                }
                
                Debug.Log($"[Excel: {table.TableName}] (length : {table.Rows.Count})");
                
                if (table.TableName.StartsWith("#"))
                {
                    Debug.Log($"[Excel: {table.TableName}] is Ignored (table name is started with '#')");
                    continue;
                }

                Debug.Log($"[Excel: {table.TableName}] (length : {table.Rows.Count})");

                var exportFilePath = Path.Combine(exportedPath, $"{table.TableName}.bytes");

                using (var binaryStream = new FileStream(exportFilePath, FileMode.OpenOrCreate))
                {
                    var type = Type.GetType($"DataTables.{table.TableName}Table+Data");

                    var serializer = new BinaryFormatter();

                    var datas = new List<ISerializable>(table.Rows.Count - (int)(RowTypeOfIndex.DataBegin - 1));

                    for (var row = (int)RowTypeOfIndex.DataBegin; row < table.Rows.Count; row++)
                    {
                        var instance = Activator.CreateInstance(type) as ISerializable;

                        for (var column = 0; column < table.Columns.Count; column++)
                        {
                            var cell = table.Rows[row][column];
                            var rowType = (RowTypeOfIndex)row;

                            var fieldName = table.Rows[(int)RowTypeOfIndex.Name][column].ToString();

                            if (string.IsNullOrEmpty(fieldName) || fieldName.StartsWith("#"))
                                continue;

                            if (rowType > RowTypeOfIndex.Type && TryGetTypeFromString(table.Rows[(int)RowTypeOfIndex.Type][column].ToString(), out var fieldTypeName, out var typeString))
                            {
                                if (string.IsNullOrWhiteSpace(cell.ToString()))
                                {
                                    if (cell.ToString().Length > 0)
                                        Debug.LogError($"{fieldName}'s {row + 1} line data is null or whitespace. You must be delete this column.");
                                    
                                    continue;
                                }
                                
                                var converter = TypeDescriptor.GetConverter(typeString);
                                var dataValue = converter.ConvertFrom(cell.ToString());
                                
                                if (isDebugMode)
                                    Debug.Log($"{fieldName} : {dataValue}, {instance.GetType()}, {instance.GetType().GetProperty(fieldName)}");
                                
                                var property = instance.GetType().GetProperty(fieldName);
                                property.SetValue(instance, dataValue);
                            }
                        }

                        datas.Add(instance);
                    }

                    serializer.Serialize(binaryStream, datas);
                }

                var bytes = File.ReadAllBytes(exportFilePath);
                var encrpyted = CryptoAES.Encrypt(bytes, EncryptUtil.GetMD5Hash(BinaryExportEncryptPassword));
                File.WriteAllBytes(exportFilePath, encrpyted);
            }
        }

        AssetDatabase.Refresh();
    }

    private static ExcelDataSetConfiguration CreateExcelDataSetConfiguration()
    {
        return new ExcelDataSetConfiguration()
        {
            UseColumnDataType = true,
            FilterSheet = (tableReader, sheetIndex) => true,
            ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = false,
                ReadHeaderRow = (rowReader) => {
                    rowReader.Read();
                },
                FilterRow = (rowReader) => true,
                FilterColumn = (rowReader, columnIndex) => true
            }
        };
    }
#endif
    private static string ToCamelCase(string text)
    {
        return Char.ToLowerInvariant(text[0]) + text.Substring(1);
    }

    public static List<T> LoadBinaryToList<T>(string filePath) where T : ISerializable
    {
        var binaryText = Resources.Load(filePath) as TextAsset;

        var decrpyted = CryptoAES.Decrypt(binaryText.bytes, EncryptUtil.GetMD5Hash(BinaryExportEncryptPassword));

        using (var binaryStream = new MemoryStream(decrpyted))
        {
            var desirializer = new BinaryFormatter();

            var datas = desirializer.Deserialize(binaryStream) as List<ISerializable>;

            return datas.OfType<T>().ToList();
        }
    }

    public static bool IsExcelFile(string filePath)
    {
        var fileExtension = Path.GetExtension(filePath).ToUpper();

        if (SupportedExtensions.All(ext => ext != fileExtension))
        {
            Debug.LogWarning("Only Support to (.xls, .xlsx) file.");
            return false;
        }

        return true;
    }

    private static bool TryGetTypeFromString(string typeString, out string fieldName, out Type type)
    {
        fieldName = typeString.ToLower();
        
        switch (fieldName)
        {
            case "ushort":
                type = typeof(ushort);
                return true;
            case "uint":
                type = typeof(uint);
                return true;
            case "bool":
                type = typeof(bool);
                return true;
            case "char":
                type = typeof(char);
                return true;
            case "int":
                type = typeof(int);
                return true;
            case "float":
                type = typeof(float);
                return true;
            case "double":
                type = typeof(double);
                return true;
            case "long":
                type = typeof(long);
                return true;
            case "string":
                type = typeof(string);
                return true;

            default:
            {
                var enumType = Type.GetType(typeString);

                if (enumType != null)
                {   
                    fieldName = typeString;
                    type = enumType;
                    return true;
                }

                type = null;
                return false;   
            }
        }
    }
}
