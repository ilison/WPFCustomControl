#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Data;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that contains the information about the particular selected cells in SfDataGrid.
    /// </summary>
    public class GridSelectedCellsInfo
    {
        #region Fields

        private object rowData;
        private List<GridColumn> columnCollection;

        #endregion

        #region Ctor      
        /// <summary>
        /// Initialize a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsInfo"/> class.
        /// </summary>
        public GridSelectedCellsInfo()
        {
            columnCollection = new List<GridColumn>();
            RowIndex = -1;
            IsAddNewRow = false;
            IsFilterRow = false;
            GridUnboundRowInfo = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the data item for the corresponding selected cells.
        /// </summary>
        /// <value>
        /// The data item for the corresponding selected cells.
        /// </value>
        public object RowData
        {
            get { return rowData; }
            set { rowData = value; }
        }

        /// <summary>
        /// Returns the collection of column that contains the selected cells.
        /// </summary>      
        public List<GridColumn> ColumnCollection
        {
            get { return columnCollection; }
        }

        private int rowIndex;

        /// <summary>
        /// Gets or sets the index of the corresponding selected summary row, UnBoundRow, DetailsView.
        /// </summary>
        /// <value>
        /// The zero-based index of the corresponding summary row, UnBoundRow, DetailsView.
        /// </value>
        public int RowIndex
        {
            get { return rowIndex; }
            set { rowIndex = value; }
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Data.NodeEntry"/> information of the selected cells.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.Data.NodeEntry"/> information.
        /// </value>
        public NodeEntry NodeEntry { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> information of the corresponding selected UnBoundRow cells.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> information of the corresponding selected UnBoundRow cells.
        /// </value>
        public GridUnBoundRow GridUnboundRowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines whether the DataRow that contains selected cells.
        /// </summary>
        /// <value>
        /// <b>true</b> if the DataRow that contains the selected cells; otherwise, <b>false</b>.
        /// </value>
        public bool IsDataRow
        {
            get { return RowIndex < 0; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the AddNewRow that contains the selected cells.
        /// </summary>
        /// <value>
        /// <b>true</b> if the AddNewRow that contains the selected cells; otherwise, <b>false</b>.
        /// </value>
        public bool IsAddNewRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this row index is FilterRow index.
        /// </summary>
        /// <value> True if the rowIndex is FilterRow, false if not. </value>
        public bool IsFilterRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the UnBoundRow that contains the selected cells.
        /// </summary>
        /// <value>
        /// <b>true</b> if the UnBoundRow that contains the selected cells; otherwise, <b>false</b>.
        /// </value>
        public bool IsUnBoundRow
        {
            get { return this.GridUnboundRowInfo != null; }
        }

        /// <summary>
        /// Gets a value that indicates whether the item is removed from <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the item is removed; otherwise, <b>false</b>.
        /// </value>
        public bool IsDirty { get; set; }

        #endregion

        /// <summary>
        /// Determines whether the given column is present in the selected column collection.
        /// </summary>
        /// <param name="column">
        /// The corresponding column.
        /// </param>
        /// <returns> 
        /// Returns<b>true</b> if column is present in the selected column selection; otherwise <b>false</b>. 
        /// </returns>
        public bool HasColumn(GridColumn column)
        {
            return ColumnCollection.Any(item => item == column);
        }
    }

    /// <summary>
    /// Represents a class contains information about the particular cell.
    /// </summary>
    public class GridCellInfo : IEquatable<GridCellInfo>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> class for data row.
        /// </summary>
        /// <param name="column">
        /// Corresponding column that contains the selected cell. 
        /// </param>
        /// <param name="rowData">
        /// The data item of the corresponding selected cell.
        /// </param>
        /// <param name="nodeEntry">
        /// The node entry of the corresponding selected cell. 
        /// </param>
        /// <param name="rowIndex">
        /// (Optional) zero-based index of the summary row. 
        /// </param>
        /// <param name="isAddnewrow">
        /// (Optional) Indicates whether the selected cell is in AddNewRow or not. 
        /// </param>
        public GridCellInfo(GridColumn column, object rowData, NodeEntry nodeEntry, int rowIndex = -1, bool isAddnewrow = false)
        {
            this.Column = column;
            this.RowData = rowData;
            this.NodeEntry = nodeEntry;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = isAddnewrow;
            this.IsFilterRow = false;
            this.GridUnboundRowInfo = null;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> class for UnBoundRow.
        /// </summary>
        /// <param name="column">
        /// Corresponding column of the selected cell in UnBoundRow. 
        /// </param>
        /// <param name="rowIndex">
        /// Corresponding index of the selected cell in UnBoundRow.
        /// </param>
        /// <param name="unBoundRow">
        /// Contains the data item of the selected cell in UnBoundRow.
        /// </param>
        public GridCellInfo(GridColumn column, int rowIndex, GridUnBoundRow unBoundRow)
        {
            this.Column = column;
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = false;
            this.IsFilterRow = false;
            this.GridUnboundRowInfo = unBoundRow;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> class for summary row.
        /// </summary>
        /// <param name="column">
        /// Corresponding column of the selected cell in summary row. 
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of summary row.
        /// </param>
        /// <param name="isAddNewRow">
        /// Indicates whether the corresponding row is AddNewRow.
        /// </param>       
        public GridCellInfo(GridColumn column, int rowIndex, bool isAddnewrow)
        {
            this.Column = column;
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = isAddnewrow;
            this.IsFilterRow = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> class for Filter row.
        /// </summary>
        /// <param name="column">
        /// Corresponding column of the selected cell in summary row. 
        /// </param>    
        /// <param name="isFilterRow">
        /// Indicates whether the corresponding row is FilterRow.
        /// </param>      
        /// <param name="rowIndex">
        /// The corresponding index of summary row.
        /// </param> 
        public GridCellInfo(GridColumn column, bool isFilterRow, int rowIndex)
        {
            this.Column = column;
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = false;
            this.IsFilterRow = isFilterRow;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the column that contains the selected cell.
        /// </summary>        
        public GridColumn Column { get; private set; }

        /// <summary>
        /// Gets the data item of the corresponding selected cell.
        /// </summary>        
        public object RowData { get; private set; }


        /// <summary>
        /// Gets a NodeEntry information of the selected cell.
        /// </summary>
        /// <value> Node Entry. </value>
        internal NodeEntry NodeEntry { get; private set; }
     
        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> information of the selected unbound row cell.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> information for the selected unbound row cell.
        /// </value>
        public GridUnBoundRow GridUnboundRowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value that indicates whether the corresponding selected cell is data row cell.
        /// </summary>
        /// <value>
        /// <b>true</b> if the selected cell is data row cell ; otherwise, <b>false</b>.
        /// </value>
        public bool IsDataRowCell
        {
            get { return RowIndex < 0; }
        }

        /// <summary>
        /// Gets or sets a value that determines whether the corresponding selected cell is in AddNewRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the corresponding selected cell is in AddNewRow; otherwise, <b>false</b>.
        /// </value>
        public bool IsAddNewRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this row index is FilterRow index.
        /// </summary>
        /// <value> True if the rowIndex is FilterRow, false if not. </value>
        public bool IsFilterRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines whether the corresponding selected cell is UnBoundRow cell.
        /// </summary>
        /// <value>
        /// <b>true</b> if the selected cell is UnBoundRow cell; otherwise, <b>false</b>.
        /// </value>
        public bool IsUnBoundRow
        {
            get { return this.GridUnboundRowInfo != null; }
        }

        /// <summary>
        /// Gets the corresponding row index of summary row, Details View, UnBoundRow.
        /// </summary>
        public int RowIndex { get; internal set; }

        #endregion    
        /// <summary>
        /// Returns the hash code for the <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> instance.
        /// </summary>
        /// <returns>
        /// Returns the hash code. 
        /// </returns>
        public override int GetHashCode()
        {
            int rowDataHashCode = this.RowData == null ? 0 : this.RowData.GetHashCode();
            return rowDataHashCode ^ this.RowIndex;
        }

        #region IEquatable<GridCellInfo> Members
        /// <summary>
        /// Compares whether the current cell info is equal to other cell info of the same type.
        /// </summary>
        /// <param name="other">
        /// The cell info to compare with the current cell info.
        /// </param>
        /// <returns>
        /// <b>true</b> if the current cell info is equal to the other cell info; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(GridCellInfo other)
        {
            if(Object.ReferenceEquals(other, null)) return false;
            
            if(Object.ReferenceEquals(this, other)) return true;

            if (this.IsDataRowCell != other.IsDataRowCell)
                return false;

            if (this.IsDataRowCell)
                return this.RowData == other.RowData && this.Column == other.Column;

            if (this.IsAddNewRow)
                return other.IsAddNewRow;

            if (this.IsFilterRow)
                return other.IsFilterRow;

            if (this.IsUnBoundRow)
                return this.GridUnboundRowInfo == other.GridUnboundRowInfo && this.Column == other.Column;

            return this.NodeEntry == other.NodeEntry;
        }

        #endregion
    }

    /*
    /// <summary>
    /// Represents a class that compares two GridCellInfo objects.
    /// </summary>
    public class EqualityComparer : IEqualityComparer<GridCellInfo>
    {
        #region IEqualityComparer<object> Members
        /// <summary>
        /// Compares the specified <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> instances are equal.
        /// </summary>
        /// <param name="x">
        /// The first object to compare
        /// </param>
        /// <param name="y">
        /// The second object to compare.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the both cell info are considered equal; otherwise, <b>false</b>. If the both cell info is <b>null</b> ,the method returns <b>true</b>.
        /// </returns>
        public bool Equals(GridCellInfo x, GridCellInfo y)
        {
            return x != null && y != null && x.Equals(y);
        }

        /// <summary>
        /// Returns the hash code for the specified cell info object.
        /// </summary>
        /// <param name="obj">
        /// The corresponding cell info to get the hash code.
        /// </param>
        /// <returns>
        /// Returns the hash code for the specified cell info object.
        /// </returns>
        public int GetHashCode(GridCellInfo obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }*/
}
