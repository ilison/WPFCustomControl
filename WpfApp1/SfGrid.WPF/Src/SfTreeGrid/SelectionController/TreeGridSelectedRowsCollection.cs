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

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents a class that contains the information about the particular row.
    /// </summary>
    public class TreeGridRowInfo : IEquatable<TreeGridRowInfo>, IDisposable
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> class for data row.
        /// </summary>
        /// <param name="rowIndex">
        /// Corresponding index of the selected row. 
        /// </param>
        /// <param name="rowData"> 
        /// Contains the corresponding data item of the selected row.
        /// </param>          
        public TreeGridRowInfo(int rowIndex, object rowData)
        {
            this.RowData = rowData;
            this.RowIndex = rowIndex;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> class for data row.
        /// </summary>
        /// <param name="rowIndex">
        /// Corresponding index of the selected row. 
        /// </param>
        /// <param name="rowData"> 
        /// Contains the corresponding data item of the selected row.
        /// </param>          
        /// <param name="treeNode"> 
        /// Contains the corresponding node of the selected row.
        /// </param> 
        internal TreeGridRowInfo(int rowIndex, object rowData, TreeNode treeNode)
        {
            this.RowData = rowData;
            this.RowIndex = rowIndex;
            Node = treeNode;
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
        /// Gets the node for the corresponding selected row.
        /// </summary>       
        internal TreeNode Node { get; private set; }

        /// <summary>
        /// Gets or sets the index of the corresponding selected row.
        /// </summary>
        /// <value>
        /// The zero-based index of the corresponding selected row.
        /// </value>
        public int RowIndex { get; set; }


        /// <summary>
        /// Gets a value that indicates whether the item is removed from <see cref="Syncfusion.UI.Xaml.Grid.TreeGrid.TreeGridRowInfo"/>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the item is removed; otherwise, <b>false</b>.
        /// </value>
        public bool IsDirty { get; set; }

        #endregion
        /// <summary>
        /// Returns the hash code for the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> instance.
        /// </summary>
        /// <returns>
        /// Returns the hash code
        /// </returns>
        public override int GetHashCode()
        {
            var rowDataHashCode = RowData == null ? 0 : RowData.GetHashCode();
            return rowDataHashCode ^ RowIndex;
        }

        #region IEquatable<TreeGridRowInfo> Members
        /// <summary>
        /// Compares whether the current row info is equal to another row info of the same type.
        /// </summary>
        /// <param name="other">
        /// Another row info to compare with the current row info.
        /// </param>
        /// <returns>
        /// <b>true</b> if the current row info is equal to the other row info; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(TreeGridRowInfo other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;

            if (Object.ReferenceEquals(this, other))
                return true;

            return this.RowData == other.RowData;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool isdisposed = false;

        /// <summary>
        /// Releases all resources used by <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                this.Node = null;
                this.RowData = null;
            }
            isdisposed = true;
        }

        #endregion

    }
    /// <summary>
    /// Represents a class that maintains the collection of selected rows and its information.
    /// </summary>
    public class TreeGridSelectedRowsCollection : List<TreeGridRowInfo>
    {
        #region IList Members

        /// <summary>
        /// Determines whether the specific row info is in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowInfo">
        /// The row info to locate in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row info is found in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        public new bool Contains(TreeGridRowInfo rowInfo)
        {
            return FindRowData(rowInfo.RowData) != null;
        }

        /// <summary>
        /// Clears the TreeGridRowInfo collection.
        /// </summary>
        public new void Clear()
        {
            foreach (var row in this)
                row.Dispose();
            base.Clear();
        }
        /// <summary>
        /// Determines whether the row info is at the specified row index of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowIndex">
        /// The index that contains the row info at specified index in <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row info is found at specified index; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(int rowIndex)
        {
            return Find(rowIndex) != null;
        }

        /// <summary>
        /// Determines whether the specific row data is in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </summary>
        /// <param name="rowInfo">
        /// The row data to locate in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the row data is found in the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridSelectedRowsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        public bool ContainsObject(object rowData)
        {            
            return FindRowData(rowData) != null;
        }

        #endregion

        #region Helper Methods

        internal TreeGridRowInfo Find(int rowIndex)
        {
            if (rowIndex <= 0)
                throw new InvalidOperationException("RowIndex should be greater than zero in Find method");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.RowIndex == rowIndex) : null;
        }

        internal TreeGridRowInfo FindRowData(object rowData)
        {
            if (rowData == null)
                throw new InvalidOperationException("RowData is null in Find is not valid operation");

            return Count > 0 ? this.FirstOrDefault(rowInfo => rowInfo.RowData == rowData) : null;
        }
        internal List<int> GetRowIndexes()
        {
            return this.Select(rowinfo => rowinfo.RowIndex).ToList();
        }

        #endregion
    }

}

