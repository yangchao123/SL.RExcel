﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SL.RExcel.XLSX
{
    public class XLSXWorkBook : IWorkBook
    {
        public IWorksheet[] Worksheets { get; private set; }

        public int Count
        {
            get { return Worksheets.Length; }
        }

        public XLSXWorkBook(Stream stream)
        {
            UnZipper unzip;
            unzip = new UnZipper(stream);
            var fileNames = unzip.GetFileNamesInZip().ToList();
            Worksheets = GetSheetList(unzip, fileNames);
        }

        private IWorksheet[] GetSheetList(UnZipper unzip, List<string> fileNames)
        {
            var sharedStrings = GetSharedStrings(unzip, fileNames);
            List<XLSXSheet> result = new List<XLSXSheet>();
            if (fileNames.Contains(XLSXCommon.Workbook))
            {
                var workbook = unzip.GetXLSXPart(XLSXCommon.Workbook);
                foreach (var item in workbook.Descendants(XLSXCommon.ExcelNamespace + XLSXCommon.XML_Sheet))
                {
                    result.Add(new XLSXSheet(item.Attribute(XLSXCommon.XML_Name).Value,
                        item.Attribute(XLSXCommon.XML_SheetId).Value, unzip, sharedStrings, fileNames));
                }
            }
            return result.ToArray();
        }

        private static Dictionary<string, string> GetSharedStrings(UnZipper unzip, List<string> fileNames)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (fileNames.Contains(XLSXCommon.SharedStrings))
            {
                var sharedStrings = unzip.GetXLSXPart(XLSXCommon.SharedStrings);
                int count = 0;
                var si = XLSXCommon.ExcelNamespace + XLSXCommon.XML_SI;
                var t = XLSXCommon.ExcelNamespace + XLSXCommon.XML_T;
                foreach (var item in sharedStrings.Descendants(si))
                {
                    var items = item.Descendants(t);
                    if (!items.IsEmpty())
                    {
                        dic.Add(count.ToString(), string.Join("", items.Select(i => i.Value)));
                        count++;
                    }
                }
            }
            return dic;
        }

        public IWorksheet GetSheetByName(string name)
        {
            return Worksheets.FirstOrDefault(i => i.Name == name);
        }

        public IWorksheet GetSheetByIndex(int index)
        {
            return Worksheets[index];
        }

        public IEnumerable<string> GetAllSheetNames()
        {
            return Worksheets.Select(i => i.Name);
        }
    }
}