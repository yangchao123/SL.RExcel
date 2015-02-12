﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SL.RExcel.MHT
{
    public class MHTRow : IRow
    {
        public static readonly Regex TdReg = new Regex("<td.+?</td>", RegexOptions.IgnoreCase);

        public MHTRow()
        {
            Cells = new Dictionary<uint, ICell>();
        }

        public IDictionary<uint, ICell> Cells { get; private set; }

        public bool IsEmpty()
        {
            return Cells.Count == 0;
        }

        public ICell GetCell(uint index)
        {
            ICell result = null;
            Cells.TryGetValue(index, out result);
            return result;
        }

        public void AddCells(string row)
        {
            uint index = Cells.Keys.IsEmpty() ? 0 : Cells.Keys.Max() + 1;
            foreach (Match cellx in TdReg.Matches(row))
            {
                if (!cellx.Success) continue;
                var cell = new MHTCell(index, cellx.Value);
                index = cell.Index;
                Cells.Add(index, cell);
                index++;
                index = SetMergerAcross(index, cell);
            }
        }

        private uint SetMergerAcross(uint index, MHTCell cell)
        {
            if (cell.ColSpan > 0)
            {
                for (int i = 1; i <= cell.ColSpan; i++)
                {
                    Cells.Add(index, new MHTCell(index, cell.Value));
                    index++;
                }
            }
            return index;
        }

        public IEnumerable<MHTCell> GetMergeDownCell()
        {
            return Cells.Values.Select(i => (MHTCell)i).Where(i => i.RowSpan > 0);
        }

        internal void AddCell(MHTCell cell)
        {
            if (!Cells.ContainsKey(cell.Index))
                Cells.Add(cell.Index, cell);
        }
    }
}