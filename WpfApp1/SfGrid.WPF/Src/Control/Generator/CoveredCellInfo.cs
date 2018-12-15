#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Defines the merge range by Top, Bottom, Left and Right. 
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class CoveredCellInfo : IComparable<CoveredCellInfo>, IComparable
    {
        #region Fields

        int row;
        int left;
        int right;
        int top;
        int bottom;
        string name;
        int rowSpan;
        RowColumnIndex mappedRowColumnIndex = new RowColumnIndex(-1, -1);        
      

        #endregion

        #region Property
        
        /// <summary>
        /// Gets or sets the RowColumnIndex of the GridCell that spanned the range in view.
        /// </summary>
        public RowColumnIndex MappedRowColumnIndex
        {
            get { return mappedRowColumnIndex; }
            set { mappedRowColumnIndex = value; }
        }       

        /// <summary>
        /// Gets Row index for the coveredCell.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int Row
        {
            get { return row; }
        }

        /// <summary>
        /// Gets Left index for the cell.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int Left
        {
            get { return left; }
        }

        /// <summary>
        /// Gets Right index for the cell.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int Right
        {
            get { return right; }
        }


        /// <summary>
        /// Gets the top index for the cell.
        /// </summary>
        public int Top
        {
            get { return top; }
        }

        /// <summary>
        /// Gets the bottom index for the cell.
        /// </summary>
        public int Bottom
        {
            get { return bottom; }
        }

        /// <summary>
        /// Gets Width for the cell.
        /// </summary>
        /// <value></value>
        /// <remarks>difference between left and right index of the cell</remarks>
        public int Width
        {
            get { return Right - Left + 1; }
        }

        /// <summary>
        /// Gets Height for the cell.
        /// </summary>
        public int Height
        {
            get { return Bottom - Top + 1; }
        }

        /// <summary>
        /// Gets Name of the StackedColumn corresponding to the Cell.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets or sets RowSpan for the cell.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int RowSpan
        {
            get { return rowSpan; }
            internal set { rowSpan = value; }
        }
        #endregion

        #region Ctor

        public CoveredCellInfo(int left,int right,int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public CoveredCellInfo(int row, int left, int right,int top,int bottom)
        {
            this.row = row;
            this.left = Math.Min(left, right);
            this.right = Math.Max(left, right);
            this.top = Math.Min(top, bottom);
            this.bottom = Math.Max(top, bottom);
        }

        public CoveredCellInfo(string name, int left, int right,int top, int bottom)
        {
            this.name = name;
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        #endregion

        #region override methods

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CoveredCellInfo;
            if (other == null)
                return false;

            return row == other.row
                && left == other.left
                && right == other.right
                && top == other.Top
                && bottom == other.Bottom;
        }

        public override string ToString()
        {
            return String.Format("{0} ( Left = {1} Right = {2} Top = {3} Bottom = {4})", GetType().Name, Left, Right,Top , Bottom);
        }

        #endregion

        #region IComparable

        public int CompareTo(CoveredCellInfo other)
        {
            int cmp = other.row - row;
            if (cmp == 0)
            {
                cmp = other.left - left;
                if (cmp == 0)
                {
                    cmp = other.right - right;
                }
            }
            return cmp;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((CoveredCellInfo)obj);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines the specified cell is inside span.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public bool Contains(int rowIndex, int columnIndex)
        {
            return rowIndex >= top && rowIndex <= bottom
                && columnIndex >= left && columnIndex <= right;
        }

        /// <summary>
        /// Determines the specified row is inside span.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public bool ContainsRow(int rowIndex)
        {
            return rowIndex >= top && rowIndex <= bottom;
        }

        /// <summary>
        /// Determines the specified column is inside range.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public bool ContainsColumn(int columnIndex)
        {
            return columnIndex >= left && columnIndex <= right;
        }

        #endregion
    }
}
