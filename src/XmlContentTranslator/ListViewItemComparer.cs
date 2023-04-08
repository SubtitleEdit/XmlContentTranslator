using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace XmlContentTranslator
{
    public class ListViewItemComparer : IComparer
    {
        public static Dictionary<ListViewItem, int> NoSortOrder = new Dictionary<ListViewItem, int>();

        public int ColumnIndex { get; set; }

        public SortOrder SortDirection { get; set; }

        public TypeCode ColumnType { get; set; }

        public ListViewItemComparer()
        {
            SortDirection = SortOrder.None;
        }

        public int Compare(object x, object y)
        {
            var lviX = x as ListViewItem;
            var lviY = y as ListViewItem;

            if (SortDirection == SortOrder.None)
            {
                return decimal.Compare(NoSortOrder[lviX], NoSortOrder[lviY]);
            }

            if (lviX == null && lviY == null)
            {
                return CompareResult(0);
            }

            if (lviX == null)
            {
                return CompareResult(-1); ;
            }

            if (lviY == null)
            {
                return CompareResult(1);
            }

            switch (ColumnType)
            {
                case TypeCode.Decimal:
                    decimal xD = 0;
                    if (decimal.TryParse(lviX.SubItems[ColumnIndex].Text, out var dx))
                    {
                        xD = dx;
                    }

                    decimal yD = 0;
                    if (decimal.TryParse(lviY.SubItems[ColumnIndex].Text, out var dy))
                    {
                        yD = dy;
                    }

                    return CompareResult(decimal.Compare(xD, yD));

                default:
                    return CompareResult(string.Compare(
                        lviX.SubItems[ColumnIndex].Text,
                        lviY.SubItems[ColumnIndex].Text));
            }
        }

        private int CompareResult(int result)
        {
            if (SortDirection == SortOrder.Descending)
            {
                return -result;
            }

            return result;
        }
    }
}
