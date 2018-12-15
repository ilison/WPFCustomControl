#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Data.Extensions;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that contains the information about the particular row.
    /// </summary>
    public class GridRowInfo : IEquatable<GridRowInfo>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> class for data row.
        /// </summary>
        /// <param name="rowIndex">
        /// Corresponding index of the selected row. 
        /// </param>
        /// <param name="rowData"> 
        /// Contains the corresponding data item of the selected row.
        /// </param>
        /// <param name="nodeEntry">
        /// Contains corresponding the node entry of the selected row. 
        /// </param>
        /// <param name="isAddNewRow">
        /// (Optional)Indicates whether the selected row is AddNewRow. 
        /// </param>       
        public GridRowInfo(int rowIndex, object rowData, NodeEntry nodeEntry, bool isAddNewRow = false)
        {
            this.RowData = rowData;
            this.NodeEntry = nodeEntry;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = isAddNewRow;
            this.IsFilterRow = false;
            this.GridUnboundRowInfo = null;
        }
       
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> class for UnBoundRow.
        /// </summary>
        /// <param name="rowIndex">
        /// Corresponding index of the UnBoundRow info.
        /// </param>
        /// <param name="unBoundRow">
        /// Contains the data item of corresponding UnBoundRow.
        /// </param>
        public GridRowInfo(int rowIndex, GridUnBoundRow unBoundRow)
        {
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = false;
            this.IsFilterRow = false;
            this.GridUnboundRowInfo = unBoundRow;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> using the specified parameter for summary row.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding index of the row info.
        /// </param>
        /// <param name="isAddNewRow">
        /// Indicates whether the corresponding row is AddNewRow.
        /// </param>
        public GridRowInfo(int rowIndex, bool isAddNewRow)
        {
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = isAddNewRow;
            this.IsFilterRow = false;
            this.GridUnboundRowInfo = null;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> using the specified parameter for Filter row.
        /// </summary>
        /// <param name="isFilterRow">
        /// Indicates whether the corresponding row is FilterRow.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of the row info.
        /// </param>
        public GridRowInfo(bool isFilterRow, int rowIndex)
        {
            this.RowData = null;
            this.NodeEntry = null;
            this.RowIndex = rowIndex;
            this.IsAddNewRow = false;
            this.IsFilterRow = isFilterRow;
            this.GridUnboundRowInfo = null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data item for the corresponding selected row.
        /// </summary>
        /// <value>
        /// The data item for the row.
        /// </value>
        public object RowData { get; private set; }

        /// <summary>
        /// Gets or sets a value that determines whether the selected row is DataRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the corresponding row is DataRow; otherwise, <b>false</b>.
        /// </value>
        public bool IsDataRow
        {
            get { return RowData != null && !(NodeEntry is NestedRecordEntry); }
        }
        
        /// <summary>
        /// Gets or sets a value that determines whether the selected row is AddNewRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the corresponding row is AddNewRow; otherwise, <b>false</b>.
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
        /// Gets or sets a value that determines whether the selected row is UnBoundRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the row is UnBoundRow; otherwise, <b>false</b>.
        /// </value>
        public bool IsUnBoundRow
        {
            get { return this.GridUnboundRowInfo != null; }
        }

        /// <summary>
        /// Gets or sets the index of the corresponding selected row.
        /// </summary>
        /// <value>
        /// The zero-based index of the corresponding selected row.
        /// </value>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.Data.NodeEntry"/> information of the group rows.
        /// </summary>
        /// <value>
        /// Returns the corresponding <see cref="Syncfusion.Data.NodeEntry"/> information , if the selection found at group rows ; otherwise , the node entry is <b>null</b> .
        /// </value>
        public NodeEntry NodeEntry { get; private set; }

        /// <summary>
        /// Gets or sets an UnBoundRow information when the selection is in unbound rows.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.Data.GridUnBoundRow"/> information , if the selection fount at unbound rows ; otherwise , the unbound row info is <b>null</b> .
        /// </value>
        public GridUnBoundRow GridUnboundRowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value that indicates whether the item is removed from <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the item is removed; otherwise, <b>false</b>.
        /// </value>
        public bool IsDirty { get; set; }

        #endregion
        /// <summary>
        /// Returns the hash code for the <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> instance.
        /// </summary>
        /// <returns>
        /// Returns the hash code
        /// </returns>
        public override int GetHashCode()
        {
            var rowDataHashCode = RowData == null ? 0 : RowData.GetHashCode();
            return rowDataHashCode ^ RowIndex;
        }

        #region IEquatable<GridRowInfo> Members
        /// <summary>
        /// Compares whether the current row info is equal to another row info of the same type.
        /// </summary>
        /// <param name="other">
        /// Another row info to compare with the current row info.
        /// </param>
        /// <returns>
        /// <b>true</b> if the current row info is equal to the other row info; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(GridRowInfo other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;

            if (Object.ReferenceEquals(this, other))
                return true;

            if (this.IsDataRow != other.IsDataRow)
                return false;

            if (this.IsDataRow)
                return this.RowData == other.RowData;

            if (this.IsAddNewRow)
                return other.IsAddNewRow;

            if (this.IsFilterRow)
                return other.IsFilterRow;

            if (this.IsUnBoundRow)
                return this.GridUnboundRowInfo == other.GridUnboundRowInfo;

            return this.NodeEntry == other.NodeEntry;
        }

        #endregion

    }
    /// <summary>
    /// Represents a class that maintains the collection of selected rows and its information.
    /// </summary>
    public class GridSelectedRowsCollection : List<GridRowInfo>
    {
        #region IList Members

        /// <summary>
        /// Determines whether the specific row info is in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowInfo">
        /// The row info to locate in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row info is found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        public new bool Contains(GridRowInfo rowInfo)
        {
            if (rowInfo.IsUnBoundRow)
                return Find(rowInfo.GridUnboundRowInfo) != null;
            if (rowInfo.IsAddNewRow || rowInfo.IsFilterRow)
                return Find(rowInfo.RowIndex) != null;
            return Find(rowInfo.NodeEntry) != null;
        }

        /// <summary>
        /// Determines whether the row info is at the specified row index of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowIndex">
        /// The index that contains the row info at specified index in <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row info is found at specified index; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(int rowIndex)
        {
            return Find(rowIndex) != null;
        }

        /// <summary>
        /// Determines whether the specific row data is in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowInfo">
        /// The row data to locate in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row data is found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        public bool ContainsObject(object rowData)
        {
            return FindRowData(rowData) != null;
        }

        /// <summary>
        /// Determines whether the specific node entry is in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowInfo">
        /// The node entry to locate in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the node entry is found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedRowsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(NodeEntry nodeEntry)
        {
            return Find(nodeEntry) != null;
        }

        #endregion

        #region Helper Methods

        internal GridRowInfo Find(GridUnBoundRow unBoundRow)
        {
            if (unBoundRow == null)
                throw new InvalidOperationException("UnBoundRow is null is not a valid operation in Find");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.GridUnboundRowInfo == unBoundRow) : null;
        }

        internal GridRowInfo Find(NodeEntry nodeEntry)
        {
            if(nodeEntry == null)
                throw  new InvalidOperationException("NodeEntry is null is not a valid operation in Find");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.NodeEntry == nodeEntry) : null;
        }

        internal GridRowInfo Find(int rowIndex)
        {
            if(rowIndex <= 0)
                throw new InvalidOperationException("RowIndex should be greater than zero in Find method");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.RowIndex == rowIndex) : null;
        }

        internal GridRowInfo FindRowData(object rowData)
        {
            if (rowData == null)
                throw new InvalidOperationException("RowData is null in Find is not valid operation");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.RowData == rowData) : null;
        }

        internal GridRowInfo Find(GridRowInfo rowInfo)
        {
            if (rowInfo == null)
                throw new InvalidOperationException("GridRowInfo is null in Find is not valid operation");

            return Count > 0 ? this.FirstOrDefault(item => item.NodeEntry == rowInfo.NodeEntry) : null;
        }

        internal List<int> GetRowIndexes()
        {
            return this.Select(rowinfo => rowinfo.RowIndex).ToList();
        }

        #endregion
    }

}

    