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
using System.Linq;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.Data.Helper;
using System.Text;
using System.Reflection;
#if UWP
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
#endif
using System.Windows;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace Syncfusion.UI.Xaml.Grid
{
    /* GridCutCopyPaste work flow
     * 
     * Copy - For Row
     *   1. CopyTextToClipBoard - Sorting the Copied records
     *   2. CopyRows - Splits the number of rows into single row
     *   3. CopyRow - Split the number of columns into single column
     *   4. CopyCell - Copy the Particular CellValue
     *   
     * Copy - For Cell
     *   1. CopyTextToClipBoard - Copy the SelectedCells
     *   2. CopyCells - Sorting the SelectedRows and ColumnCollection
     *   3. CopyCellRow - Split the number of Cells into single cell.
     *   4. CopyCell - Copy the Particular CellValue
     *  
     * Paste - For Row
     *    1. PasteTextToRow - Split the ClipboardConent to number of rows
     *    2. PasteToRows - Paste the CopiedRows and add the selected records
     *    3. PasteToRow - Split the Copied row into cells and removing the empty value
     *    4. PasteToCell - Paste the Copied CellValue
     *    5. CommitValue - Commit the CopiedValue to particular cell
     *    6. GetPropertyType - Get the Type of Property
     *    7. CanConvertToType - Check if Copy value type is compatable to Paste Cell
     *   
     * Paste - For Cell
     *     1. PasteTextToCell - Split the ClipboardConent to number of rows 
     *     2. PasteToCells - Paste CopiedCells and add selectedcells
     *     3. PasteToRow - Split the Copied row into cells and removing the empty value
     *     4. PasteToCell - Paste the Copied CellValue
     *    
     * Cut - Row and Cell
     *     For cut all the copy operations are processed.
     *     After processing, ClearCellsByCut - Set the null or empty value to particular cell
     */
    /// <summary>
    /// Represents the clipboard operations in SfDataGrid.
    /// </summary>
    public class GridCutCopyPaste : IGridCopyPaste, IDisposable
    {
        #region Fields

        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> .
        /// </summary>
        protected SfDataGrid dataGrid;
        private bool isdisposed = false;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridCutCopyPaste"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The instance of SfDataGrid.
        /// </param>
        public GridCutCopyPaste(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        #endregion

        #region Copy
       
        /// <summary>
        /// Invoked when the copy operation is initiated for the specified collection of records to clipboard for row selection.
        /// </summary>
        /// <param name="records">
        /// The collection of records to initialize copy operation.
        /// </param>
        /// <param name="cut">
        /// Indicates whether the cut operation is performed.
        /// </param>
        protected virtual void CopyTextToClipBoard(ObservableCollection<object> records, bool cut)
        {
            GridCopyPasteEventArgs args = null;
            args = this.RaiseCopyContentEvent(new GridCopyPasteEventArgs(false, this.dataGrid));
            if (args.Handled)
                return;

            var rowDict = new SortedDictionary<int, object>();
            for (int i = 0; i < records.Count; i++)
            {
                var rowindex = this.dataGrid.ResolveToRowIndex(records[i]);
                if (rowindex >= 0)
                rowDict.Add(rowindex, records[i]);
            }

            var text = new StringBuilder();
            var sortedRecords = (rowDict.Values).ToObservableCollection();
            CopyRows(sortedRecords, ref text);

#if UWP
            var data = new DataPackage();
#else
            var data = new DataObject();
#endif
            if (text.Length > 0)
                data.SetText(text.ToString());

            if (cut)
                ClearCellsByCut(sortedRecords);

#if UWP
            Clipboard.SetContent(data);
#else
            Clipboard.SetDataObject(data);
#endif
        }
        
        /// <summary>
        /// Invoked when the copy operation is processed for the specified collection records.
        /// </summary>
        /// <param name="records">
        /// The collection of record to process copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyRows(ObservableCollection<object> records, ref StringBuilder text)
        {
            //Copy if Header if GridCopyOption is IncludeHeaders
            if (dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHeaders))
            {
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    var copyargs = this.RaiseCopyGridCellContentEvent(dataGrid.Columns[i], null, dataGrid.Columns[i].HeaderText);

                    if (!dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHiddenColumn) && dataGrid.Columns[i].IsHidden)
                        continue;

                    if (!copyargs.Handled)
                    {
                        if (text.Length != 0)
                            text.Append("\t");
                        text = text.Append(copyargs.ClipBoardValue);
                    }
                }
                text.Append("\r\n");
            }

            //Copy Rows Value
            for (int i = 0; i < records.Count; i++)
            {
                StringBuilder rowtext = new StringBuilder();
                CopyRow(records[i], ref rowtext);
                text.Append(rowtext);
                if (i < records.Count - 1)
                    text.Append("\r\n");
            }
        }
       
        /// <summary>
        /// Invoked when the copy operation is performed for the particular record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to process copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyRow(object record, ref StringBuilder text)
        {
            //Copy ParticularRowValue
            //WPF-24165 - When we have copy the records by uisng CopyRowsToClipboard while set SelectionUnit as Cell,
            //leftMostColumnIndex is not set because this is set while processing the CopyCells method when processing 
            //cell selection, So we have to set leftMostColumnIndex as zero in copyrow method.
            leftMostColumnIndex = 0;
            //WRT-6031 - When we hide first column and did not set GridCopyOption as IncludeHiddenColumn,
            //we skip the hidden column while copying, but we set the leftMostColumnIndex as 0, so '\t' is 
            //append to copy text before adding the second column value, so one empty value is copied before 
            //second column value. So reset the leftMostColumnIndex as first visible column in datagrid.
            if (!dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHiddenColumn))
            {
                while (leftMostColumnIndex < dataGrid.Columns.Count && dataGrid.Columns[leftMostColumnIndex].IsHidden)
                    leftMostColumnIndex++;
            }
            for (var i = 0; i < dataGrid.Columns.Count; i++)
            {
                if (!dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHiddenColumn) && dataGrid.Columns[i].IsHidden)
                    continue;
                CopyCell(record, dataGrid.Columns[i], ref text);
            }
        }

        /// <summary>
        /// Copies the particular cell value for the specified column and record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to copy the cell.        
        /// </param>
        /// <param name="column">
        /// The corresponding column to copy the cell.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyCell(object record, GridColumn column, ref StringBuilder text)
        {
            //CopyParticularCellValue
            if (this.dataGrid.View == null)
                return;

            object copyText = null;

            if (column.IsUnbound)
            {
                var unboundValue = this.dataGrid.GetUnBoundCellValue(column, record);
                copyText = unboundValue != null ? unboundValue.ToString() : string.Empty;
            }
            else
            {
                if (this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeFormat))
                    copyText = this.dataGrid.View.GetPropertyAccessProvider().GetFormattedValue(record, column.MappingName);
                else
                    copyText = this.dataGrid.View.GetPropertyAccessProvider().GetValue(record, column.MappingName);
            }
            var copyargs = this.RaiseCopyGridCellContentEvent(column, record, copyText);
            if (!copyargs.Handled)
            {
                //WPF-24165 - SelectionUnit is Cell condition is removed because the leftMostColumnIndex is set in
                //both Row and Cell selection process.
                if (this.dataGrid.Columns[leftMostColumnIndex] != column || text.Length != 0)
                    text.Append('\t');
                
                text.Append(copyargs.ClipBoardValue);
            }
        }

        /// <summary>
        /// Gets or sets the left most selected column index of the grid while copying with cell selection.
        /// </summary>
        protected int leftMostColumnIndex = -1;

        /// <summary>
        /// Gets or sets the right most selected column index of the grid while copying with cell selection.
        /// </summary>
        protected int rightMostColumnIndex = -1;

        /// <summary>
        /// Invoked when the copy operation is initiated for the collection of selected cells to clipboard for cell selection.
        /// </summary>
        /// <param name="selectedCells">
        /// The collection of selected cells to initialize copy operation.
        /// </param>
        /// <param name="cut">
        /// Indicates whether the cut operation is performed.
        /// </param>
        protected virtual void CopyTextToClipBoard(GridSelectedCellsCollection selectedCells, bool cut)
        {
            GridCopyPasteEventArgs args = null;
            args = this.RaiseCopyContentEvent(new GridCopyPasteEventArgs(false, this.dataGrid));
            if (args.Handled)
                return;

            var dataCell = selectedCells.FirstOrDefault(cell => cell.IsDataRow);
            if (dataCell == null)
                return;
   
            var text = new StringBuilder();
            CopyCells(selectedCells, text);

#if UWP
            var data = new DataPackage();
#else
            var data = new DataObject();
#endif
          
            if (text.Length > 0)
                data.SetText(text.ToString());

            if (cut)
            {
                var selectedcells = selectedCells.ToObservableCollection<object>();
                ClearCellsByCut(selectedcells);
            }
#if UWP
            Clipboard.SetContent(data);
#else
            Clipboard.SetDataObject(data);
#endif
        }
        
        /// <summary>
        /// Invoked when the copy operation is processed for the specified collection cells.
        /// </summary>
        /// <param name="selectedCells">
        /// The collection of selected cells to perform copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the StringBuilder parameter.
        /// </param>
        protected virtual void CopyCells(GridSelectedCellsCollection selectedCells, StringBuilder text)
        {
            var rowDict = new SortedDictionary<int, object>();
            var columnDict = new SortedDictionary<int, object>();
            leftMostColumnIndex = -1;
            rightMostColumnIndex = -1;

            for (int i = 0; i < selectedCells.Count; i++)
            {
                int rowindex = this.dataGrid.ResolveToRowIndex(selectedCells[i].RowData);
                for (int j = 0; j < selectedCells[i].ColumnCollection.Count; j++)
                {
                    var column = selectedCells[i].ColumnCollection[j];
                    int colIndex = this.dataGrid.Columns.IndexOf(column);
                    //WRT-6031 - When we select the first column and hide first column,
                    //we skip the hidden column while copying, but we set the leftMostColumnIndex as 0 because 
                    //that column is added in the selectedcells, so '\t' is append to copy text before adding 
                    //the second column value, so one empty value is copied before 
                    //second column value. So reset the leftMostColumnIndex as first visible column in datagrid.
                    if (!dataGrid.Columns[colIndex].IsHidden)
                        columnDict.Add(colIndex, selectedCells[i].ColumnCollection[j]);
                }

                var selectedCellInfo = new GridSelectedCellsInfo() { RowData = selectedCells[i].RowData };
                //WRT-6031- When we select the cell and hide that and try to copy means that cell is not added in 
                //columnDict, so check columnDict count before reset leftMostColumnIndex and rightMostColumnIndex
                if (columnDict.Count > 0)
                {
                    var templeftmostindex = (columnDict.Keys).ElementAt(0);
                    var temprightmostindex = (columnDict.Keys).ElementAt(columnDict.Count - 1);

                    if (leftMostColumnIndex > templeftmostindex || leftMostColumnIndex == -1)
                        leftMostColumnIndex = templeftmostindex;

                    if (rightMostColumnIndex < temprightmostindex || rightMostColumnIndex == -1)
                        rightMostColumnIndex = temprightmostindex;
                    (columnDict.Values).ForEach(item => selectedCellInfo.ColumnCollection.Add(item as GridColumn));
                }
                if (rowindex >= 0)
                    rowDict.Add(rowindex, selectedCellInfo);
                columnDict.Clear();
            }

            // WRT -6031 - If the leftmostindex is less than 0 means there is no cell is selected in the view.
            // So no need to process the copy operation.
            if (leftMostColumnIndex < 0)
                return;
            var selectedcell = rowDict.Values.Cast<GridSelectedCellsInfo>().ToList();

            if (dataGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHeaders))
            {
                for (int i = leftMostColumnIndex; i <= rightMostColumnIndex; i++)
                {
                    var copyargs = this.RaiseCopyGridCellContentEvent(dataGrid.Columns[i], null, dataGrid.Columns[i].HeaderText);

                    if (dataGrid.Columns[i].IsHidden)
                        continue;

                    if (!copyargs.Handled)
                    {
                        if (text.Length != 0)
                            text.Append('\t');
                        text = text.Append(copyargs.ClipBoardValue);
                    }
                }
                text.Append("\r\n");
            }

            for (int i = 0; i < selectedcell.Count; i++)
            {
                StringBuilder rowtext = new StringBuilder();
                CopyCellRow(selectedcell[i], ref rowtext);
                text.Append(rowtext);
                if (i < selectedcell.Count - 1)
                    text.Append("\r\n");
            }
        }

        /// <summary>
        /// Invoked when the copy operation is performed for the particular selected cell.
        /// </summary>
        /// <param name="row">
        /// The corresponding row to process copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyCellRow(GridSelectedCellsInfo row, ref StringBuilder text)
        {
            for (int i = leftMostColumnIndex; i <= rightMostColumnIndex; i++)
            {
                if (dataGrid.Columns[i].IsHidden)
                    continue;
                if (row.ColumnCollection.Contains(dataGrid.Columns[i]))
                    CopyCell(row.RowData, dataGrid.Columns[i], ref text);
                else if (i != leftMostColumnIndex && i < rightMostColumnIndex)
                    text.Append('\t');
            }
        }

        /// <summary>
        /// Invoked when the cut operation performed on rows or cells in SfDataGrid. 
        /// </summary>
        /// <param name="selections">
        /// Contains the collection of selected records or cells based on the selection unit.
        /// </param>
        protected virtual void ClearCellsByCut(ObservableCollection<object> selections)
        {
            if (dataGrid.SelectionUnit == GridSelectionUnit.Row)
            {
                selections.ToList().ForEach(item =>
                {
                    foreach (var col in dataGrid.Columns)
                        CutRowCell(item, col);
                });
            }
            else
            {
                var selectedcell = selections.Cast<GridSelectedCellsInfo>().ToList();
                int count = selectedcell.Count;
                for (int i = 0; i < count; i++)
                {
                    var columns = selectedcell[i].ColumnCollection.ToList();
                    foreach (var col in columns)
                        CutRowCell(selectedcell[i].RowData, col);
                }
            }
        }

        /// <summary>
        /// Invoked when the cut operation performed for the specified the row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Contains the row data of the selected record to perform cut operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column of the selected record to perform cut operation.
        /// </param>
        protected virtual void CutRowCell(object rowData, GridColumn column)
        {
            if(this.dataGrid.View == null) return;
            var provider = this.dataGrid.View.GetPropertyAccessProvider();
            var properyCollection = this.dataGrid.View.GetItemProperties();
            if (column.IsHidden || column.IsUnbound || properyCollection==null)
                return;

            Type type = null;

            var cellvalue = provider.GetValue(rowData, column.MappingName);

#if WPF
            if (cellvalue == null || cellvalue == DBNull.Value)
#else
            if(cellvalue == null)
#endif
                return;

            //Getting type of the column using GetPropertyType().
            type = GetPropertyType(rowData, column);
            if (type == null)
                return;

#if !WPF
            if (properyCollection.Find(column.MappingName, false) != null && !properyCollection.Find(column.MappingName, false).CanWrite)
#else
            if (properyCollection.Find(column.MappingName, false) != null && properyCollection.Find(column.MappingName, false).IsReadOnly)
#endif
                return;

#if WinRT || UNIVERSAL
            if (type.IsValueType())
#else
            if (type.IsValueType)
#endif
                provider.SetValue(rowData, column.MappingName, Activator.CreateInstance(type));
            else if (type == typeof(string))
                provider.SetValue(rowData, column.MappingName, string.Empty);
        }

        #endregion

        #region RaiseEvents
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyContent"/> event in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the event data.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteEventArgs RaiseCopyContentEvent(GridCopyPasteEventArgs args)
        {
            return dataGrid.RaiseCopyContentEvent(new GridCopyPasteEventArgs(false,this.dataGrid));
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CopyGridCellContent"/> event in SfDataGrid.
        /// </summary>
        /// <param name="column">
        /// The corresponding column of the cell content.
        /// </param>
        /// <param name="rowData">
        /// The corresponding row data of the cell content.
        /// </param>
        /// <param name="clipboardValue">
        /// The corresponding clipboard value that is going to be copied.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteCellEventArgs RaiseCopyGridCellContentEvent(GridColumn column, object rowData, object clipboardValue)
        {
            return dataGrid.RaiseCopyGridCellContentEvent(new GridCopyPasteCellEventArgs(false , column,this.dataGrid, rowData, clipboardValue));
            
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridPasteContent"/> event in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the event data.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteEventArgs RaisePasteContentEvent(GridCopyPasteEventArgs args)
        {
            return dataGrid.RaisePasteContentEvent(new GridCopyPasteEventArgs(false, this.dataGrid));
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.PasteGridCellContent"/> event in SfDataGrid.
        /// </summary>
        /// <param name="column">
        /// The corresponding column of the cell content.
        /// </param>
        /// <param name="rowData">
        /// The corresponding row data of the cell content.
        /// </param>
        /// <param name="clipboardValue">
        /// The corresponding clipboard value that is going to be pasted.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteCellEventArgs RaisePasteGridCellContentEvent(GridColumn column, object rowData, object clipboardValue)
        {
            return dataGrid.RaisePasteGridCellContentEvent(new GridCopyPasteCellEventArgs(false, column,this.dataGrid, rowData, clipboardValue));
        }
        #endregion

        #region Paste

        /// <summary>
        /// Invoked when the clipboard content is splitted into number of rows to perform paste operation for row selection. 
        /// </summary>
#if WinRT || UNIVERSAL
        public async void PasteTextToRow()
#else
        protected virtual void PasteTextToRow()
#endif
        {
            if (this.dataGrid.SelectedItem == null)
                return;

            var args = this.RaisePasteContentEvent(new GridCopyPasteEventArgs(false, this.dataGrid));

            if (args.Handled)
                return;

#if WinRT || UNIVERSAL
            DataPackageView dataPackage = null;
            dataPackage = Clipboard.GetContent();
            DataPackage package = new DataPackage();
            if (dataPackage.AvailableFormats.Count <= 0)
                return;
            package.SetText(await dataPackage.GetTextAsync());
#else
            IDataObject dataObject = null;
            dataObject = Clipboard.GetDataObject();
#endif


#if UWP
            if (dataPackage.Contains(StandardDataFormats.Text))
            {
               var clipBoardContent = await dataPackage.GetTextAsync();
#else
            if (dataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                var clipBoardContent = dataObject.GetData(DataFormats.UnicodeText) as string;
#endif
                if(clipBoardContent == null)
                    return;

                // Split the ClipboardConent to number of rows 
                var copiedRecords = Regex.Split(clipBoardContent.ToString(), @"\r\n");
                //Considered only ExcludeFirstLine while pasting
                if (dataGrid.GridPasteOption.HasFlag(GridPasteOption.ExcludeFirstLine))
                    copiedRecords = copiedRecords.Skip(1).ToArray();
                PasteToRows(copiedRecords);
            }
        }

        /// <summary>
        /// Invoked when the paste operation is performed to each row of the clipboard copied rows to SfDataGrid.
        /// </summary>
        /// <param name="clipboardrows">
        /// Contains the copied clipboard content to paste rows.
        /// </param>
        protected virtual void PasteToRows(object clipboardrows)
        {
            var startIndex = 0;
            var selectedRecords = new List<object>();
            var copiedRecord = (string[])clipboardrows;
            //WPF-25047 Paste behaves wrong when we have shift selection and drag selection from BottomToTop
            //so get the pasted startindex based on PressedRowIndex and CurrentRowIndex
            var lastselectedindex = this.dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            int pressedindex = (this.dataGrid.SelectionController as GridSelectionController).PressedRowColumnIndex.RowIndex;
            int index = pressedindex < lastselectedindex ? pressedindex : lastselectedindex;
            //WPF-25968 When index is not a datarow we have skip the paste operation.
            if (this.dataGrid.GetRecordAtRowIndex(index) == null)
                return;
            copiedRecord.ForEach(rec =>
            {
                if (!rec.Equals(string.Empty) && index + startIndex <= this.dataGrid.GetLastDataRowIndex())
                {
                    var selrec = this.dataGrid.GetRecordAtRowIndex(index + startIndex);

                    //WPF-18634 -When selection is in CaptionSummary/GroupSummary/TableSummary row(other than Record row)
                    //GetRecordAtRowIndex will return null. So added null check

                    if (selrec != null)
                        selectedRecords.Add(selrec);
                    startIndex += this.dataGrid.DetailsViewManager.HasDetailsView ? this.dataGrid.DetailsViewDefinition.Count + 1 : 1;
                    while (this.dataGrid.GetRecordAtRowIndex(index + startIndex) == null)
                    {
                        if (index + startIndex >= this.dataGrid.GetLastDataRowIndex())
                            return;
                        startIndex++;
                    }
                }
            });

            for (int i = 0; i < copiedRecord.Count(); i++)
            {
                if (i < selectedRecords.Count())
                    PasteToRow(copiedRecord[i], selectedRecords[i]);
            }
        }

        /// <summary>
        /// Invoked when the copied row is splitted into cells to perform paste operation for the specified clipboard content and selected records.
        /// </summary>
        /// <param name="clipboardcontent">
        /// Contains the copied record to paste row.
        /// </param>
        /// <param name="selectedRecords">
        /// Contains the selected record to paste row.
        /// </param>
        protected virtual void PasteToRow(object clipboardcontent, object selectedRecords)
        {
            if (this.dataGrid.Columns.Count == 0)
                return;
            //Split the row into no of cell by using \t
            clipboardcontent = Regex.Split(clipboardcontent.ToString(), @"\t");

            var copyValue = (string[])clipboardcontent;
  
            //For Row selection
            if (dataGrid.SelectionUnit == GridSelectionUnit.Row)
            {
                int columnindex = 0;
                foreach (var column in dataGrid.Columns)
                {
                    if (dataGrid.GridPasteOption.HasFlag(GridPasteOption.IncludeHiddenColumn))
                    {
                        if (copyValue.Count() <= this.dataGrid.Columns.IndexOf(column))
                            break;
                        PasteToCell(selectedRecords, column, copyValue[columnindex]);
                        columnindex++;
                    }
                    else
                    {
                        if (copyValue.Count() <= columnindex)
                            break;
                        if (!dataGrid.Columns[dataGrid.Columns.IndexOf(column)].IsHidden)
                        {
                            PasteToCell(selectedRecords, column, copyValue[columnindex]);
                            columnindex++;
                        }
                    }
                }
            }
            //For Cell and Any selection
            else
            {
                //Removing the empty values 
                copyValue = copyValue.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                int cellcount = copyValue.Count();
                var selectionContoller = this.dataGrid.SelectionController as GridCellSelectionController;
                //WPF-25047 lastSelectedIndex of SelectionController is maintained only for Drag selection, 
                //because the value is not pasted correctly when shift selection,so get the lastselectedindex by using CurrentRowColumnIndex
                var lastselectedindex = selectionContoller.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                var pressedindex = selectionContoller.PressedRowColumnIndex.ColumnIndex;
                var pastecolumnindex = pressedindex < lastselectedindex ? pressedindex : lastselectedindex;

                int columnindex = 0;
                var columnStartIndex =
                        this.dataGrid.ResolveToGridVisibleColumnIndex(pastecolumnindex);              
                for (int i = columnStartIndex; i < cellcount + columnStartIndex; i++)
                {
                    if (dataGrid.GridPasteOption.HasFlag(GridPasteOption.IncludeHiddenColumn))
                    {
                        if (dataGrid.Columns.Count <= i)
                            break;
                        PasteToCell(selectedRecords, dataGrid.Columns[i], copyValue[columnindex]);
                        columnindex++;
                    }
                    else
                    {
                        if (dataGrid.Columns.Count <= i)
                            break;
                        if (!dataGrid.Columns[i].IsHidden)
                        {
                            PasteToCell(selectedRecords, dataGrid.Columns[i], copyValue[columnindex]);
                            columnindex++;
                        }
                        else
                            cellcount++;
                    }
                }
            }
        }

        /// <summary>
        /// Pastes the copied cell value to selected cell. 
        /// </summary>
        /// <param name="value">
        /// Contains copied cell value to paste.</param>
        /// <param name="column">
        /// Contains the corresponding column of the selected cell.</param>
        /// <param name="record">
        /// Contains the record of the selected cell.
        /// </param>
        protected virtual void PasteToCell(object record, GridColumn column, object value)
        {
            if(this.dataGrid.View == null) return;
            var provider = this.dataGrid.View.GetPropertyAccessProvider();
            var properyCollection = this.dataGrid.View.GetItemProperties();
            if (properyCollection == null)
                return;
            if (column.IsUnbound)
                CommitValue(record, column, provider, value);

            else
            {
                if (!this.dataGrid.View.IsDynamicBound)
                {
#if !WPF
               if (properyCollection.Find(column.MappingName, false) != null && 
                    !properyCollection.Find(column.MappingName, false).CanWrite)
#else
                    if (properyCollection.Find(column.MappingName, false) != null &&
                          properyCollection.Find(column.MappingName, false).IsReadOnly)
#endif
                        return;
                }
                var pasteargs = this.RaisePasteGridCellContentEvent(column, record, value);
                value = pasteargs.ClipBoardValue;
                if (!pasteargs.Handled)
                    CommitValue(record, column, provider, value);
            }
        }

        /// <summary>
        /// Invoked when the clipboard content is splitted into number of cells to perform paste operation for cell selection.        
        /// </summary>
#if WinRT || UNIVERSAL
        public async void PasteTextToCell()  
#else
        protected virtual void PasteTextToCell()
#endif
        {
            var args =
               this.RaisePasteContentEvent(new GridCopyPasteEventArgs(false, this.dataGrid));

            if (args.Handled)
                return;

            var selectionContoller = this.dataGrid.SelectionController as GridCellSelectionController;
            var selectedCells = selectionContoller.SelectedCells as GridSelectedCellsCollection;
            var dataCell = selectedCells.FirstOrDefault(cell => cell.IsDataRow);
            if (dataCell == null)
                return;

#if WinRT || UNIVERSAL
            DataPackageView dataPackage = null;
            dataPackage = Clipboard.GetContent();
            DataPackage package = new DataPackage();
            if (dataPackage.AvailableFormats.Count <= 0)
                return;
           package.SetText(await dataPackage.GetTextAsync());
#else
            IDataObject dataObject = null;
            dataObject = Clipboard.GetDataObject();
#endif

#if WinRT || UNIVERSAL
             if (dataPackage.Contains(StandardDataFormats.Text))
             {
                var clipBoardContent = await dataPackage.GetTextAsync();
#else
            if (dataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                var clipBoardContent = dataObject.GetData(DataFormats.UnicodeText) as string;
#endif
                // Split the ClipboardConent to number of rows 
                var copiedRecords = Regex.Split(clipBoardContent, @"\r\n");
                //Considered only ExcludeFirstLine while pasting
                if (dataGrid.GridPasteOption.HasFlag(GridPasteOption.ExcludeFirstLine))
                    copiedRecords = copiedRecords.Skip(1).ToArray();
                PasteToCells(copiedRecords);
            }
        }
        /// <summary>
        /// Invoked when the paste operation is performed to each cell of the clipboard copied cells to selected cells in SfDataGrid.
        /// </summary>
        /// <param name="clipboardrows">
        /// Contains the copied clipboard content to paste cells.
        /// </param>
        protected virtual void PasteToCells(object clipboardrows)
        {
            var selectionContoller = this.dataGrid.SelectionController as GridCellSelectionController;
            //The value of lastSelectedIndex of SelectionController is maintained only for Drag selection, 
            //So get the lastselectedindex by using CurrentRowColumnIndex
            var lastselectedindex = selectionContoller.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            int pressedindex = selectionContoller.PressedRowColumnIndex.RowIndex;

            //If we did not select any cell. But that time we try to paste means we need to skip that paste operation 
            //so we need to check that pressed index is available in the SelectedCells or not.
            var cellinfo = this.dataGrid.GetGridCellInfo(selectionContoller.PressedRowColumnIndex);
            if (this.dataGrid.SelectionController.SelectedCells.Contains(cellinfo))
            {
                var pasteRowIndex = 0;
                if (lastselectedindex < 0)
                    pasteRowIndex = pressedindex;
                else
                    pasteRowIndex = pressedindex < lastselectedindex ? pressedindex : lastselectedindex;
                //WPF-25968 When index is not a datarow we have skip the paste operation.
                if (this.dataGrid.GetRecordAtRowIndex(pasteRowIndex) == null)
                    return;
                var selectedRecords = new List<object>();
                var startIndex = 0;

                string[] record = (string[])clipboardrows;

                //Getting the SelectedRecords
                record.ForEach(rec =>
                {
                    if (pasteRowIndex + startIndex <= this.dataGrid.GetLastDataRowIndex())
                    {
                        var selrec = this.dataGrid.GetRecordAtRowIndex(pasteRowIndex + startIndex);
                        //WPF-23344 -When selection is in CaptionSummary/GroupSummary/TableSummary row(other than Record row)
                        //GetRecordAtRowIndex will return null. So added null check
                        while (selrec == null)
                        {
                            if (pasteRowIndex + startIndex > this.dataGrid.GetLastDataRowIndex())
                                return;
                            startIndex++;
                            selrec = this.dataGrid.GetRecordAtRowIndex(pasteRowIndex + startIndex);
                        }
                        selectedRecords.Add(selrec);
                        startIndex += this.dataGrid.DetailsViewManager.HasDetailsView ? this.dataGrid.DetailsViewDefinition.Count + 1 : 1;
                    }
                });

                record = record.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                for (int i = 0; i < record.Count(); i++)
                {
                    if (i < selectedRecords.Count())
                        PasteToRow(record[i], selectedRecords[i]);
                }
            }
        }
        
        /// <summary>
        /// Commits the value for the specified row data, column and corresponding changed value.
        /// </summary>
        /// <param name="rowData">
        /// The corresponding row data to commit value.
        /// </param>
        /// <param name="column">
        /// The corresponding column to commit value.
        /// </param>
        /// <param name="provider">
        /// The corresponding provider to commit value.</param>
        /// <param name="changedValue">
        /// The corresponding changed value to commit.
        /// </param>
        protected virtual void CommitValue(object rowData, GridColumn column, IPropertyAccessProvider provider, object changedValue)
        {
            if (column.IsUnbound)
            {
                this.dataGrid.RaiseQueryUnboundValue(UnBoundActions.PasteData, changedValue, column, rowData);
                return;
            }

            Type type = GetPropertyType(rowData, column);

            //While giving dummy mappingname, its type becomes null. so we should check the type as null or not.
            if (type == null)
                return;
            var canconvert = CanConvertToType(changedValue, ref type);

            if (!canconvert && string.IsNullOrEmpty(changedValue.ToString()))
            {
                return;
            }

            if (!(canconvert || type == typeof(string) || type == typeof(object)))
                return;

            if (column.IsDropDown)
            {
                CommitValueDropDownColumn(column, ref changedValue, type);
            }
#if !WinRT && !UNIVERSAL
            else if (column is GridTimeSpanColumn)
            {
                TimeSpan value;
                TimeSpan.TryParse(changedValue.ToString(), out value);
                provider.SetValue(rowData, column.MappingName, value);
                return;
            }
#endif
            //WPF-31462 DateTimeOffset type is not convert the value from string to DateTime using 
            //Convert.ChangeType method, need to try parse the type.
            else if (type == typeof(DateTimeOffset))
            {
                DateTimeOffset value;
                DateTimeOffset.TryParse(changedValue.ToString(), out value);
                provider.SetValue(rowData, column.MappingName, value);
                return;
            }
            var pasteValue = Convert.ChangeType(changedValue, type);
            provider.SetValue(rowData, column.MappingName, pasteValue);
        }

        /// <summary>
        /// Gets the corresponding property type for the specified row data and column.
        /// </summary>
        /// <param name="column"> 
        /// The corresponding column to get property type.
        /// </param>
        /// <param name="rowData">
        /// The corresponding row data to get property type.
        /// </param>
        /// <returns>
        /// Returns the corresponding property type.
        /// </returns>
        protected virtual Type GetPropertyType(object rowData, GridColumn column)
        {
            if (this.dataGrid.View == null) return null;
            //Get the Type of particular column
            var provider = this.dataGrid.View.GetPropertyAccessProvider();            

            //WPF-20175 - if the collection type is dynamic, no need to get the PropertyDescriptor. it always return null for dynamic types.
            if (!this.dataGrid.View.IsDynamicBound)
            {
#if WPF
                PropertyDescriptorCollection typeInfos = dataGrid.View.GetItemProperties();
#else
                PropertyInfoCollection typeInfos = dataGrid.View.GetItemProperties();
#endif
                var typeInfo = typeInfos.GetPropertyDescriptor(column.MappingName);
                if (typeInfo != null)
                    return typeInfo.PropertyType;
            }

            var cellvalue = provider.GetValue(rowData, column.MappingName);
            return cellvalue != null ? cellvalue.GetType() : null;
        }

        /// <summary>
        /// Determines whether the copied cell value type is compatible with type of paste cell content.
        /// </summary>
        /// <param name="value">
        /// The corresponding value can be compatible with paste cell type. </param>
        /// <param name="type">
        /// Contains type of selected column.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the copied value type is compatible with paste cell value type; otherwise, <b>false</b>.
        /// </returns>
        protected bool CanConvertToType(object value, ref Type type)
        {
            if (NullableHelperInternal.IsNullableType(type))
                type = NullableHelperInternal.GetUnderlyingType(type);

#if WinRT || UNIVERSAL
            var method = type.GetTypeInfo().DeclaredMethods.Where(x => x.Name.Equals("TryParse"));
#else
            var method = type.GetMethods().Where(x => x.Name.Equals("TryParse"));
#endif

            if (method.Count() == 0)
                return false;

            var methodinfo = method.FirstOrDefault();
            object[] args = { value.ToString(), null };
            return (bool)methodinfo.Invoke(null, args);
        }

        /// <summary>
        /// CommitValue for DropDownColumn
        /// </summary>
        /// <param name="column">Contains particular column</param>
        /// <param name="changedValue">Conatins copied value</param>
        /// <param name="type">contains type of selected column</param>
        private void CommitValueDropDownColumn(GridColumn column, ref object changedValue, Type type)
        {
            IEnumerable list = null;
            string displayMemberPath;
            string valueMemberPath;
            if (column is GridComboBoxColumn)
            {
                var comboColumn = column as GridComboBoxColumn;
                list = comboColumn.ItemsSource;
                displayMemberPath = comboColumn.DisplayMemberPath;
                valueMemberPath = comboColumn.SelectedValuePath;
            }
            else
            {
                var dropdown = column as GridMultiColumnDropDownList;
                list = dropdown.ItemsSource as IEnumerable;
                displayMemberPath = dropdown.DisplayMember;
                valueMemberPath = dropdown.ValueMember;
            }
            if (list == null)
                return;
            var enumerator = list.GetEnumerator();
            var value = Convert.ChangeType(changedValue, type);
#if WPF
            PropertyDescriptorCollection pdc = null;
#else
            PropertyInfoCollection pdc = null;
#endif
            if (!string.IsNullOrEmpty(valueMemberPath))
            {
                while (enumerator.MoveNext())
                {
                    if (pdc == null)
                    {
#if WPF
                        pdc = TypeDescriptor.GetProperties(enumerator.Current.GetType());
#else
                        pdc = new PropertyInfoCollection(enumerator.Current.GetType());
#endif
                    }
                    if (value.Equals(pdc.GetValue(enumerator.Current,
                        valueMemberPath)))
                        break;
                }
            }
            else if (string.IsNullOrEmpty(displayMemberPath))
            {
                while (enumerator.MoveNext())
                {
                    if (value.Equals(enumerator.Current))
                        break;
                }
            }
        }

        #endregion

        #region IGridCopyPaste Implementations
        /// <summary>
        /// Copies the selected rows or cells from SfDataGrid to clipboard.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+C</b> key is pressed.
        /// </remarks>
        public void Copy()
        {
            if (this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.None)
                || !this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.CopyData))
                return;
            
            if (this.dataGrid.SelectionUnit == GridSelectionUnit.Row)
                this.CopyTextToClipBoard(this.dataGrid.SelectedItems, false);
            else
            {
                var selectedCells = this.dataGrid.SelectionController.SelectedCells;
                this.CopyTextToClipBoard(selectedCells, false);
            }
        }
        
        /// <summary>
        /// Copies the selected rows or cells and sets the default or null or empty value.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+X</b> key is pressed.
        /// </remarks>
        public void Cut()
        {
            if (!this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.None) && this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.CutData))
            {
                if (this.dataGrid.SelectionUnit == GridSelectionUnit.Row)
                    this.CopyTextToClipBoard(this.dataGrid.SelectedItems, true);
                else
                {
                    var selectedCells = this.dataGrid.SelectionController.SelectedCells;
                    this.CopyTextToClipBoard(selectedCells,true);
                }
            }
        }

        /// <summary>
        /// Pastes the clipboard copied content to the selected rows or cells in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+V</b> key is pressed.
        /// </remarks>
        public void Paste()
        {
            if (!this.dataGrid.GridPasteOption.HasFlag(GridPasteOption.None) && this.dataGrid.GridPasteOption.HasFlag(GridPasteOption.PasteData))
            {
                if (this.dataGrid.SelectionUnit == GridSelectionUnit.Row)
                    this.PasteTextToRow();
                else
                    this.PasteTextToCell();
            }
        }

        /// <summary>
        /// Copies the rows to clipboard for the specified start and end of the record index.
        /// </summary>
        /// <param name="startRecordIndex">
        /// The start index of the record to copy rows to clipboard.
        /// </param>
        /// <param name="endRecordIndex">
        /// The end index of the record to copy rows to clipboard.
        /// </param>
        public void CopyRowsToClipboard(int startRecordIndex, int endRecordIndex)
        {            
            if(this.dataGrid.View == null) return;
            if ((!this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.None))
                && (this.dataGrid.GridCopyOption.HasFlag(GridCopyOption.CopyData)) && ((startRecordIndex <= endRecordIndex && startRecordIndex >= 0 && endRecordIndex >= 0)))
            {
                var Copiedrecords = new ObservableCollection<object>();
                for (int i = startRecordIndex; i <= endRecordIndex; i++)
                {
                    var record = this.dataGrid.View.GetRecordAt(i);
                    if (record != null)
                        Copiedrecords.Add(record.Data);
                }
                this.CopyTextToClipBoard(Copiedrecords,false);
            }
        }

        #endregion
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridCutCopyPaste"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridCutCopyPaste"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
                this.dataGrid = null;
            isdisposed = true;
        }
    }
    /// <summary>
    /// Provides the functionality of clipboard operations in SfDataGrid.
    /// </summary>
    public interface IGridCopyPaste
    {
        /// <summary>
        /// Copies the selected rows or cells from SfDataGrid to clipboard.
        /// </summary>        
        void Copy();

        /// <summary>
        /// Copies the selected rows or cells and sets the default or null or empty value.
        /// </summary>
        void Cut();

        /// <summary>
        /// Pastes the clipboard copied content to the selected rows or cells in SfDataGrid.
        /// </summary>
        void Paste();

        /// <summary>
        /// Copies the rows to clipboard for the specified start and end of the record index.
        /// </summary>
        /// <param name="startRecordIndex">
        /// The start index of the record to copy rows to clipboard.
        /// </param>
        /// <param name="endRecordIndex">
        /// The end index of the record to copy rows to clipboard.
        /// </param>
        void CopyRowsToClipboard(int startRecordIndex, int endRecordIndex);
    }
}
