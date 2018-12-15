#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that maintains the collection of selected cells and its information.
    /// </summary>
    public class GridSelectedCellsCollection : IList<GridSelectedCellsInfo>
    {
        #region Fields
       
        List<GridSelectedCellsInfo> internalCellsList;

        #endregion

        #region Ctor

        /// <summary> 
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> class.        
        /// </summary>
        public GridSelectedCellsCollection()
        {
            internalCellsList = new List<GridSelectedCellsInfo>();
        }

        #endregion

        #region IList Members

        /// <summary> 
        /// Searches for the specified cell info and returns zero-based index of the first occurrence with in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/>. 
        /// </summary>
        /// <param name="cellInfo">
        /// The corresponding cell info that locate in the internal selected cells list.
        /// </param>
        /// <returns>
        /// Returns the zero-based index of the first occurrence of selected cell info , if its found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/>; otherwise, –1.
        /// </returns>
        public int IndexOf(GridCellInfo cellInfo)
        {
            if (cellInfo == null)
                throw new InvalidOperationException("GridCellInfo can't be null");

            if (cellInfo.IsDataRowCell)
                return internalCellsList.IndexOf(Find(cellInfo.RowData));

            return internalCellsList.IndexOf(Find(cellInfo.RowIndex));
        }

        /// <summary> 
        /// Searches for the specified selected cells info and returns zero-based index of the first occurrence with in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/>. 
        /// </summary>
        /// <param name="item">
        /// The selected cells info that locate in the internal selected cells list.
        /// </param>
        /// <returns>
        /// Returns the zero-based index of the first occurrence of selected cells info , if its found on the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/>; otherwise, –1.
        /// </returns>
        public int IndexOf(GridSelectedCellsInfo item)
        {
            if (item == null)
                throw new InvalidOperationException("GridSelectedCellsInfo can't be null");

            if (item.IsDataRow)
                return internalCellsList.IndexOf(Find(item.RowData));

            return internalCellsList.IndexOf(item);
        }

        /// <summary> 
        /// Inserts a specified cell info into the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// Zero-based index at which cell info should be inserted. 
        /// </param>
        /// <param name="cellInfo"> 
        /// The cell info to insert. 
        /// </param>
        public void Insert(int index, GridCellInfo cellInfo)
        {
            throw new NotImplementedException();           
        }

        /// <summary> 
        /// Inserts a new cells info into the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> at the specified index. 
        /// </summary>
        /// <param name="index">
        /// Zero-based index at which cells information should be inserted. . 
        /// </param>
        /// <param name="newItem"> 
        /// The new item to insert. 
        /// </param>
        public void Insert(int index, GridSelectedCellsInfo newItem)
        {
            throw new NotImplementedException();           
        }

        /// <summary> 
        /// Removes the cells info at specified index of the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/>. 
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= internalCellsList.Count) 
                throw new ArgumentOutOfRangeException();

            internalCellsList.RemoveAt(index);
        }

       /// <summary>
       /// Gets or sets the selected cells information at the specified index.
       /// </summary>
       /// <param name="index">
       /// The index for retrieving the selected cell info.
       /// </param>
       /// <returns>
       /// Returns the selected cells information at the specified index.
       /// </returns>
        public GridSelectedCellsInfo this[int index]
        {
            get
            {
                return internalCellsList[index];
            }
            set
            {
                internalCellsList[index] = value;
            }
        }

        /// <summary>
        /// Adds the specific cell info into the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellInfo"> 
        /// The cell info to add.
        /// </param>
        /// <exception cref="System.ArgumentException">Thrown when the cell info argument value is null.</exception>
        public void Add(GridCellInfo cellInfo)
        {
            if (cellInfo == null)
                throw new ArgumentException("GridCellInfo can't be null in Add");

            if (cellInfo.IsDataRowCell)
            {
                var selectedCellInfo = Find(cellInfo.RowData);
                if (selectedCellInfo != null)
                {
                    if (!selectedCellInfo.HasColumn(cellInfo.Column))
                        selectedCellInfo.ColumnCollection.Add(cellInfo.Column);
                }
                else
                {
                    selectedCellInfo = new GridSelectedCellsInfo() { RowData = cellInfo.RowData, NodeEntry = cellInfo.NodeEntry };
                    if(cellInfo.Column != null)
                        selectedCellInfo.ColumnCollection.Add(cellInfo.Column);
                    internalCellsList.Add(selectedCellInfo);
                }
            }
            else if (cellInfo.IsUnBoundRow)
            {
                var selectedCellInfo = Find(cellInfo.GridUnboundRowInfo);
                if (selectedCellInfo != null)
                {
                    if (!selectedCellInfo.HasColumn(cellInfo.Column))
                        selectedCellInfo.ColumnCollection.Add(cellInfo.Column);
                }
                else
                {
                    selectedCellInfo = new GridSelectedCellsInfo() { GridUnboundRowInfo = cellInfo.GridUnboundRowInfo, RowIndex = cellInfo.RowIndex };
                    if (cellInfo.Column != null)
                        selectedCellInfo.ColumnCollection.Add(cellInfo.Column);
                    internalCellsList.Add(selectedCellInfo);
                }
            }
            else
            {
                var selectedCellInfo = Find(cellInfo.RowIndex);
                if (selectedCellInfo == null)
                {
                    selectedCellInfo = new GridSelectedCellsInfo() { RowIndex = cellInfo.RowIndex, NodeEntry = cellInfo.NodeEntry, IsAddNewRow = cellInfo.IsAddNewRow, IsFilterRow = cellInfo.IsFilterRow };
                    if (cellInfo.Column != null)
                        selectedCellInfo.ColumnCollection.Add(cellInfo.Column);
                    internalCellsList.Add(selectedCellInfo);
                }
            }
        }

        /// <summary>
        /// Adds the specific cells info into the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellsInfo">
        /// The selected cells info to add. 
        /// </param>
        /// <exception cref="System.ArgumentException">Thrown when the cells info argument value is null.</exception>
        public void Add(GridSelectedCellsInfo cellsInfo)
        {
            if (cellsInfo == null)
                throw new ArgumentException("GridSelectedCellsInfo can't be null in Add");

            if (cellsInfo.IsDataRow || cellsInfo.IsUnBoundRow)
            {
                var selectedCellInfo = cellsInfo.IsDataRow ? Find(cellsInfo.RowData) : Find(cellsInfo.GridUnboundRowInfo);
                if (selectedCellInfo != null)
                {
                    cellsInfo.ColumnCollection.ForEach(column =>
                        {
                            if (!selectedCellInfo.HasColumn(column))
                                selectedCellInfo.ColumnCollection.Add(column);
                        });
                }
                else
                    internalCellsList.Add(cellsInfo);
            }
            else
            {
                if (!(internalCellsList.Any(item => item == cellsInfo)))
                    internalCellsList.Add(cellsInfo);
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        public void Clear()
        {
            internalCellsList.Clear();
        }

        /// <summary>
        /// Determines whether the specific cell info is in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellInfo">
        /// The cell info to locate in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the cell info is found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the cell info argument value is null.</exception>
        public bool Contains(GridCellInfo cellInfo)
        {
            if (cellInfo == null)
                throw new ArgumentException("GridCellInfo can't be null in Remove");

            if (cellInfo.IsDataRowCell)
            {
                var selectedCellInfo = Find(cellInfo.RowData);
                return selectedCellInfo != null && selectedCellInfo.HasColumn(cellInfo.Column);
            }
            if (cellInfo.IsAddNewRow || cellInfo.IsFilterRow)
            {
                var selectedCellInfo = Find(cellInfo.RowIndex);
                return selectedCellInfo != null && selectedCellInfo.HasColumn(cellInfo.Column);
            }

            if (cellInfo.IsUnBoundRow)
            {
                var selectedCellInfo = Find(cellInfo.GridUnboundRowInfo);
                return selectedCellInfo != null && selectedCellInfo.HasColumn(cellInfo.Column);
            }
            return Find(cellInfo.RowIndex) != null;
        }

        /// <summary>
        /// Determines whether the specific cells info contains in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellsInfo">
        /// The cells info to locate in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the cells info is found in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list; otherwise, <b>false</b>.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the cells info argument value is null.</exception>
        public bool Contains(GridSelectedCellsInfo cellsInfo)
        {
            if (cellsInfo == null)
                throw new ArgumentException("GridSelectedCellsInfo can't be null in Remove");
            
            if (cellsInfo.IsDataRow || cellsInfo.IsUnBoundRow || cellsInfo.IsAddNewRow || cellsInfo.IsFilterRow)
            {
                GridSelectedCellsInfo selectedCellInfo;
                if (cellsInfo.IsUnBoundRow)
                    selectedCellInfo = Find(cellsInfo.GridUnboundRowInfo);
                else
                    selectedCellInfo = cellsInfo.IsAddNewRow || cellsInfo.IsFilterRow ? Find(cellsInfo.RowIndex) : Find(cellsInfo.RowData);
                if (selectedCellInfo != null && selectedCellInfo.ColumnCollection.Count > 0)
                {
                    return cellsInfo.ColumnCollection.All(selectedCellInfo.HasColumn);
                }
                return false;
            }

            return Find(cellsInfo.RowIndex) != null;
        }

        /// <summary>
        /// Copies all the cells information to the specified destination array index.
        /// </summary>
        /// <param name="array">
        /// An One-dimensional array that is the destination of the elements copied from the current array.
        /// </param>
        /// <param name="arrayIndex">
        /// The index in array at which copying begins. 
        /// </param>
        public void CopyTo(GridSelectedCellsInfo[] array, int arrayIndex)
        {
            internalCellsList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of cells info contained in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <value>
        /// The number of elements contained in the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </value>
        public int Count
        {
            get { return internalCellsList.Count; }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list is read only.
        /// </summary>
        /// <value>
        /// True if the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list is read only; otherwise, <b>false</b>.
        /// </value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific cell info from the  <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellInfo">
        /// The cell info to remove from the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the item removed; otherwise, <b>false</b>. 
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the cell info argument value  is null.</exception>
        public bool Remove(GridCellInfo cellInfo)
        {
            if (cellInfo == null)
                throw new ArgumentException("GridCellInfo can't be null in Remove");

            var isItemRemoved = false;
            if (cellInfo.IsDataRowCell || cellInfo.IsUnBoundRow)
            {
                var selectedCellInfo = cellInfo.IsDataRowCell ? Find(cellInfo.RowData) : Find(cellInfo.GridUnboundRowInfo);
                if (selectedCellInfo != null)
                {
                    if (selectedCellInfo.ColumnCollection.Count > 0)
                    {
                        if (selectedCellInfo.HasColumn(cellInfo.Column))
                            isItemRemoved = selectedCellInfo.ColumnCollection.Remove(cellInfo.Column);
                    }
                    if (selectedCellInfo.ColumnCollection.Count <= 0)
                        isItemRemoved = this.internalCellsList.Remove(selectedCellInfo);
                }
            }
            else
                isItemRemoved = this.internalCellsList.Remove(Find(cellInfo.RowIndex));
            return isItemRemoved;
        }

        /// <summary>
        /// Removes the first occurrence of a specific cells info from the  <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <param name="cellsInfo">
        /// The cells info to remove from the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </param>
        /// <returns>
        /// <b>true</b> if the cells info removed; otherwise, <b>false</b>. 
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the cells info argument value is null.</exception>
        public bool Remove(GridSelectedCellsInfo cellsInfo)
        {
            if (cellsInfo == null)
                throw new ArgumentException("GridSelectedCellsInfo can't be null in Remove");

            var isItemRemoved = false;
            if (cellsInfo.IsDataRow || cellsInfo.IsUnBoundRow)
            {
                var selectedCellInfo = cellsInfo.IsDataRow ? Find(cellsInfo.RowData) : Find(cellsInfo.GridUnboundRowInfo);
                if (selectedCellInfo != null)
                {
                    if (cellsInfo.ColumnCollection.Count > 0)
                    {
                        int index = cellsInfo.ColumnCollection.Count - 1;
                        while(index>=0)
                        {
                            var column = cellsInfo.ColumnCollection[index];
                            if (selectedCellInfo.HasColumn(column))
                                isItemRemoved = selectedCellInfo.ColumnCollection.Remove(column);
                            index--;
                        }
                    }
                    if (selectedCellInfo.ColumnCollection.Count <= 0)
                        isItemRemoved = this.internalCellsList.Remove(selectedCellInfo);
                }
            }
            else
                this.internalCellsList.Remove(cellsInfo);
            return isItemRemoved;
        }

        /// <summary>
        /// Gets an enumerator that iterates through the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <returns>
        /// The enumerator for the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </returns>
        public IEnumerator<GridSelectedCellsInfo> GetEnumerator()
        {
            return internalCellsList.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator that iterates through the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> list.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> object that can be used to  iterate through the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsCollection"/> collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (internalCellsList as IEnumerable).GetEnumerator();
        }

        #endregion

        #region Helper Methods

        internal GridSelectedCellsInfo Find(object rowData)
        {
            if (internalCellsList.Count > 0 && rowData != null)
            {
                var selectedCell = internalCellsList.FirstOrDefault(item => item.IsDataRow && item.RowData.GetHashCode() == rowData.GetHashCode());
                if (selectedCell != null && selectedCell.RowData == rowData)
                    return selectedCell;
                else if (selectedCell != null)
                    return internalCellsList.FirstOrDefault(item => item.RowData == rowData);
            }
            return null;
        }

        internal GridSelectedCellsInfo Find(int rowIndex)
        {
            if(rowIndex < 0)
                throw new ArgumentException("rowIndex can't be less than zero");

            if (internalCellsList.Count > 0)
                return internalCellsList.FirstOrDefault(item => item.RowIndex == rowIndex);
            return null;
        }

        internal GridSelectedCellsInfo Find(GridUnBoundRow unBoundRow)
        {
            if (unBoundRow == null)
                throw new InvalidOperationException("UnBoundRow is null is not a valid operation in Find");

            if (internalCellsList.Count > 0)
                return internalCellsList.FirstOrDefault(item => item.GridUnboundRowInfo == unBoundRow);
            return null;
        }

        internal GridSelectedCellsInfo Find(NodeEntry nodeEntry)
        {
            if (nodeEntry == null)
                throw new ArgumentException("NodeEntry can't be null in Find");

            if (internalCellsList.Count > 0)
                return internalCellsList.FirstOrDefault(item => item.NodeEntry == nodeEntry);
            return null;
        }

        internal List<GridCellInfo> ConvertToGridCellInfoList(GridSelectedCellsInfo item = null)
        {
            var cellList = new List<GridCellInfo>();
            if (item == null)
            {
                if (this.internalCellsList.Count > 0)
                {
                    this.internalCellsList.ForEach(selectedCellsInfo =>
                    {
                        if (selectedCellsInfo.IsDataRow || selectedCellsInfo.IsUnBoundRow)
                        {
                            selectedCellsInfo.ColumnCollection.ForEach(column =>
                                {
                                    cellList.Add(new GridCellInfo(column, selectedCellsInfo.RowData,
                            selectedCellsInfo.NodeEntry, selectedCellsInfo.RowIndex, selectedCellsInfo.IsAddNewRow)
                                    {
                                        GridUnboundRowInfo = selectedCellsInfo.GridUnboundRowInfo
                                    });
                                });
                        }
                        else
                            cellList.Add(new GridCellInfo(selectedCellsInfo.ColumnCollection.Count > 0 ? selectedCellsInfo.ColumnCollection[0] : null, null, selectedCellsInfo.NodeEntry, selectedCellsInfo.RowIndex, selectedCellsInfo.IsAddNewRow) { IsFilterRow = selectedCellsInfo.IsFilterRow });
                    });
                }
            }
            else
            {
                if (item.IsDataRow || item.IsUnBoundRow)
                {
                    var selectedCellsInfo = this.Find(item.RowData);
                    selectedCellsInfo.ColumnCollection.ForEach(column =>
                    {
                        cellList.Add(new GridCellInfo(column, selectedCellsInfo.RowData,
                            selectedCellsInfo.NodeEntry, selectedCellsInfo.RowIndex, selectedCellsInfo.IsAddNewRow)
                        {
                            GridUnboundRowInfo = selectedCellsInfo.GridUnboundRowInfo
                        });
                    });
                }
                else
                {
                    var selectedCellsInfo = this.Find(item.RowIndex);
                    cellList.Add(new GridCellInfo(selectedCellsInfo.ColumnCollection.Count > 0 ? selectedCellsInfo.ColumnCollection[0] : null, null, selectedCellsInfo.NodeEntry, selectedCellsInfo.RowIndex, selectedCellsInfo.IsAddNewRow) { IsFilterRow = selectedCellsInfo.IsFilterRow });
                }
            }
            return cellList;
        }

        #endregion
    }
}
