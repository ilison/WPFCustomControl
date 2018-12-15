#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.Data;
using System.Reflection;
using Syncfusion.Data.Helper;
using System.Collections;
using System.Windows;
using System.ComponentModel;
using Syncfusion.UI.Xaml.Grid;
using System.Reflection.Emit;
#if UWP
using Windows.ApplicationModel.DataTransfer;
using Key = Windows.System.VirtualKey;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /* TreeGridCutCopyPaste work flow
    * 
    * Copy - For Row
    *   1. OnCopyingClipBoardContent - Sorting the Copied records
    *   2. CopyRows - Splits the number of rows into single row
    *   3. CopyRow - Split the number of columns into single column
    *   4. CopyCell - Copy the particular CellValue
    * 
    * Cut - For Row
    *     For cut all the copy operations are processed.
    *     After processing, CutRows - Set the null or empty value to selected rows.
    * 
    * Paste - For Row
    *    1. PasteRowsToClipboard - Split the ClipboardConent to number of rows
    *    2. PasteRows - Paste the CopiedRows and add the selected records
    *    3. PasteRow - Split the Copied row into cells and removing the empty value
    *    4. PasteCell - Paste the Copied CellValue
    *    5. CommitValue - Commit the CopiedValue to particular cell
    *    6. GetPropertyType - Get the Type of Property
    *    7. CanConvertToType - Check if Copy value type is compatable to Paste Cell
    *  
    */
    /// <summary>
    /// This class is used to perform the cut, copy and paste operation for selected records in SfTreeGrid. 
    /// </summary>
    public class TreeGridCutCopyPaste : IDisposable
    {
        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> .
        /// </summary>
        protected SfTreeGrid TreeGrid;
        private bool isdisposed = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCutCopyPaste"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The instance of SfTreeGrid.
        /// </param>
        public TreeGridCutCopyPaste(SfTreeGrid treeGrid)
        {
            this.TreeGrid = treeGrid;
        }

        /// <summary>
        /// This method is called when the copy operation is initiated for the specified collection of records to clipboard for row selection.
        /// </summary>
        /// <param name="records">
        /// The collection of records to initialize the copy operation.
        /// </param>
        /// <param name="canCut">
        /// Indicates whether the cut operation is perform or not.
        /// </param>
        protected virtual void OnCopyingClipBoardContent(ObservableCollection<object> records, bool canCut)
        {
            GridCopyPasteEventArgs args = null;
            args = this.RaiseCopyContentEvent(new GridCopyPasteEventArgs(false, this.TreeGrid));
            if (args.Handled)
                return;
            var rowDict = new SortedDictionary<int, object>();
            for (int i = 0; i < records.Count; i++)
            {
                var rowindex = this.TreeGrid.ResolveToRowIndex(records[i]);
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
            if (canCut)
                CutRows(sortedRecords);
#if UWP
            Clipboard.SetContent(data);
#else
            Clipboard.SetDataObject(data);
#endif
        }

        /// <summary>
        /// This method is called when the cut operation is performed on row selection in SfTreeGrid. 
        /// </summary>
        /// <param name="selections">
        /// Contains the collection of selected records.
        /// </param>
        protected virtual void CutRows(ObservableCollection<object> selections)
        {
            selections.ToList().ForEach(item =>
            {
                foreach (var col in TreeGrid.Columns)
                    ClearCell(item, col);
            });
        }

        /// <summary>
        /// This method is called when the cut operation performed for the particular row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Contains the row data of the selected record to perform cut operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column of the selected record to perform cut operation.
        /// </param>
        protected virtual void ClearCell(object rowData, TreeGridColumn column)
        {
            if (this.TreeGrid.View == null) return;
            var provider = this.TreeGrid.View.GetPropertyAccessProvider();
#if UWP
            PropertyInfoCollection properyCollection = null;
            if (TreeGrid.View is TreeGridUnboundView)
                properyCollection = new PropertyInfoCollection(rowData.GetType());
            else
                properyCollection = TreeGrid.View.GetItemProperties();
#else
            PropertyDescriptorCollection properyCollection = null;
            if (TreeGrid.View is TreeGridUnboundView)
                properyCollection = TypeDescriptor.GetProperties(rowData);
            else
                properyCollection = TreeGrid.View.GetItemProperties();
#endif
            if (column.IsHidden || properyCollection == null)
                return;

            Type type = null;
            var cellvalue = provider.GetValue(rowData, column.MappingName);
#if WPF
            if (cellvalue == null || cellvalue == DBNull.Value)
#else
            if (cellvalue == null)
#endif
                return;
            //Getting type of the column using GetPropertyType().
            type = GetPropertyType(rowData, column);
            if (type == null)
                return;
#if UWP
            if (properyCollection.Find(column.MappingName, false) != null && !properyCollection.Find(column.MappingName, false).CanWrite)
#else
            if (properyCollection.Find(column.MappingName, false) != null && properyCollection.Find(column.MappingName, false).IsReadOnly)
#endif
                return;
            if (type.IsValueType())
                provider.SetValue(rowData, column.MappingName, Activator.CreateInstance(type));
            else if (type == typeof(string))
                provider.SetValue(rowData, column.MappingName, string.Empty);
        }

        /// <summary>
        /// This method is called when the copy operation is processed for the specified collection of selected records.
        /// </summary>
        /// <param name="records">
        /// The collection of record to process copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyRows(ObservableCollection<object> records, ref StringBuilder text)
        {
            CopyHeaderRow(text);

            for (int i = 0; i < records.Count; i++)
            {
                var rowtext = new StringBuilder();
                CopyRow(records[i], ref rowtext);
                text.Append(rowtext);
                if (i < records.Count - 1)
                    text.Append("\r\n");
            }
        }

        /// <summary>
        /// This method is called when the copy operation is performed for selected records, if you want to include the header row,
        /// you can copy the header row by setting <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.GridCopyOption"/> as IncludeHeaders.
        /// </summary>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyHeaderRow(StringBuilder text)
        {
            if (TreeGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHeaders))
            {
                for (int i = 0; i < TreeGrid.Columns.Count; i++)
                {
                    var copyargs = this.RaiseCopyTreeGridCellContentEvent(TreeGrid.Columns[i], null, TreeGrid.Columns[i].HeaderText);

                    if (!TreeGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHiddenColumn) && TreeGrid.Columns[i].IsHidden)
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
        }

        /// <summary>
        /// This method is called when the copy operation is performed for particular record.
        /// </summary>
        /// <param name="record">
        /// The corresponding node to process copy operation.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyRow(object record, ref StringBuilder text)
        {
            for (var i = 0; i < TreeGrid.Columns.Count; i++)
            {
                if (!TreeGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeHiddenColumn) && TreeGrid.Columns[i].IsHidden)
                    continue;

                CopyCell(record, TreeGrid.Columns[i], ref text);
            }
        }

        /// <summary>
        /// This method is used to copy the particular cell value for the specified record and column.
        /// </summary>
        /// <param name="record">
        /// The corresponding node to process copy operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column to copy the cell value.
        /// </param>
        /// <param name="text">
        /// The corresponding copied value is append to the reference parameter.
        /// </param>
        protected virtual void CopyCell(object record, TreeGridColumn column, ref StringBuilder text)
        {
            if (this.TreeGrid.View == null)
                return;
            object copyText = null;
            var provider = this.TreeGrid.View.GetPropertyAccessProvider();
            if (this.TreeGrid.GridCopyOption.HasFlag(GridCopyOption.IncludeFormat))
                copyText = provider.GetFormattedValue(record, column.MappingName);
            else
                copyText = provider.GetValue(record, column.MappingName);
            var copyargs = this.RaiseCopyTreeGridCellContentEvent(column, record, copyText);
            if (!copyargs.Handled)
            {
                if (text.Length != 0)
                    text.Append('\t');
                text.Append(copyargs.ClipBoardValue);
            }
        }

        /// <summary>
        /// This method is called when the clipboard content is copied and perform the paste operation for row selection. 
        /// </summary>
#if UWP
        public async void PasteRowsToClipboard()
#else
        protected virtual void PasteRowsToClipboard()
#endif
        {
            if (this.TreeGrid.SelectedItem == null)
                return;

            var args = this.RaisePasteContentEvent(new GridCopyPasteEventArgs(false, this.TreeGrid));

            if (args.Handled)
                return;

#if UWP
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
                if (clipBoardContent == null)
                    return;
                var copiedRecords = Regex.Split(clipBoardContent.ToString(), @"\r\n");
                //Considered only ExcludeHeader while pasting
                if (TreeGrid.GridPasteOption.HasFlag(GridPasteOption.ExcludeFirstLine))
                    copiedRecords = copiedRecords.Skip(1).ToArray();
                PasteRows(copiedRecords);
            }
        }

        /// <summary>
        /// This method is called when the paste operation is performed to each row of the clipboard copied rows in SfTreeGrid.
        /// </summary>
        /// <param name="clipboardrows">
        /// Contains the copied clipboard content to paste rows.
        /// </param>
        protected virtual void PasteRows(object clipboardrows)
        {
            var startIndex = 0;
            var selectedRecords = new List<object>();
            var copiedRecord = (string[])clipboardrows;
            var lastselectedindex = this.TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            int pressedindex = (this.TreeGrid.SelectionController as TreeGridRowSelectionController).PressedRowColumnIndex.RowIndex;
            int index = pressedindex < lastselectedindex ? pressedindex : lastselectedindex;

            if (this.TreeGrid.GetNodeAtRowIndex(index) == null)
                return;
            copiedRecord.ForEach(rec =>
            {
                if (!rec.Equals(string.Empty) && index + startIndex <= this.TreeGrid.GetLastDataRowIndex())
                {
                    var selrec = this.TreeGrid.GetNodeAtRowIndex(index + startIndex);
                    if (selrec != null)
                        selectedRecords.Add(selrec);
                    startIndex += 1;
                    while (this.TreeGrid.GetNodeAtRowIndex(index + startIndex) == null)
                    {
                        if (index + startIndex >= this.TreeGrid.GetLastDataRowIndex())
                            return;
                        startIndex++;
                    }
                }
            });

            for (int i = 0; i < copiedRecord.Count(); i++)
            {
                if (i < selectedRecords.Count())
                    PasteRow(copiedRecord[i], (selectedRecords[i] as TreeNode).Item);
            }
        }

        /// <summary>
        /// This method invoked when the copied row is performed to paste operation for the specified clipboard content and selected records.
        /// </summary>
        /// <param name="clipboardcontent">
        /// Contains the copied record to paste row.
        /// </param>
        /// <param name="selectedRecords">
        /// Contains the selected record to paste row.
        /// </param>
        protected virtual void PasteRow(object clipboardcontent, object selectedRecords)
        {
            if (this.TreeGrid.Columns.Count == 0)
                return;
            //Split the row into no of cell by using \t
            clipboardcontent = Regex.Split(clipboardcontent.ToString(), @"\t");
            var copyValue = (string[])clipboardcontent;

            //For Row selection
            int columnindex = 0;
            foreach (var column in TreeGrid.Columns)
            {
                if (TreeGrid.GridPasteOption.HasFlag(GridPasteOption.IncludeHiddenColumn))
                {
                    if (copyValue.Count() <= this.TreeGrid.Columns.IndexOf(column))
                        break;
                    PasteCell(selectedRecords, column, copyValue[columnindex]);
                    columnindex++;
                }
                else
                {
                    if (copyValue.Count() <= columnindex)
                        break;
                    if (!TreeGrid.Columns[TreeGrid.Columns.IndexOf(column)].IsHidden)
                    {
                        PasteCell(selectedRecords, column, copyValue[columnindex]);
                        columnindex++;
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to perform the paste operation for the copied cell value to selected record cell. 
        /// </summary>
        /// <param name="value">
        /// Contains copied cell value to paste.</param>
        /// <param name="column">
        /// Contains the corresponding column of the selected cell.</param>
        /// <param name="record">
        /// Contains the record of the selected cell.
        /// </param>
        protected virtual void PasteCell(object record, TreeGridColumn column, object value)
        {
            if (this.TreeGrid.View == null) return;
            var provider = this.TreeGrid.View.GetPropertyAccessProvider();
            object properyCollection;
            if (!(TreeGrid.View is TreeGridUnboundView))
                properyCollection = this.TreeGrid.View.GetItemProperties();
            else
            {
#if UWP
                PropertyInfoCollection typeInfos = new PropertyInfoCollection(record.GetType());
                properyCollection = typeInfos.GetItemPropertyInfo();
#else
                PropertyDescriptorCollection typeInfos = TypeDescriptor.GetProperties(record);
                properyCollection = typeInfos.GetItemPropertyInfo(); 
#endif
            }
            if (properyCollection == null)
                return;
            var pasteargs = this.RaisePasteGridCellContentEvent(column, record, value);
            value = pasteargs.ClipBoardValue;
            if (!pasteargs.Handled)
                CommitValue(record, column, value);
        }

        /// <summary>
        /// To commits the value for the specified row data, column and corresponding changed value.
        /// </summary>
        protected virtual void CommitValue(object rowData, TreeGridColumn column, object changedValue)
        {
            Type type = null;
            var provider = this.TreeGrid.View.GetPropertyAccessProvider();
            type = GetPropertyType(rowData, column);
            ////While giving dummy mappingname, its type becomes null. so we should check the type as null or not.
            if (type == null)
                return;
            var canconvert = CanConvertToType(changedValue, ref type);
            if (!canconvert && string.IsNullOrEmpty(changedValue.ToString()))
                return;
            if (!(canconvert || type == typeof(string) || type == typeof(object)))
                return;

            if (column.IsDropDown)
                CommitValueDropDownColumn(column, ref changedValue, type);
            else if (type == typeof(DateTimeOffset))
            {
                DateTimeOffset value;
                DateTimeOffset.TryParse(changedValue.ToString(), out value);
                provider.SetValue(rowData, column.MappingName, value);
                return;

            }
            var pasteValue = ValueConvert.ChangeType(changedValue, type, null);
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
        protected virtual Type GetPropertyType(object rowData, TreeGridColumn column)
        {
            if (this.TreeGrid.View == null) return null;
            //Get the Type of particular column
            var provider = TreeGrid.View.GetPropertyAccessProvider();
#if UWP
            PropertyInfoCollection typeInfos = null;
            if (TreeGrid.View is TreeGridUnboundView)
                typeInfos = new PropertyInfoCollection(rowData.GetType());
            else
                typeInfos = TreeGrid.View.GetItemProperties();
#else
            PropertyDescriptorCollection typeInfos = null;
            if (TreeGrid.View is TreeGridUnboundView)
                typeInfos = TypeDescriptor.GetProperties(rowData);
            else
                typeInfos = TreeGrid.View.GetItemProperties();
#endif
            var typeInfo = typeInfos.GetPropertyDescriptor(column.MappingName);
            if (typeInfo != null)
                return typeInfo.PropertyType;

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
#if !SyncfusionFramework4_0
            var method = type.GetTypeInfo().DeclaredMethods.Where(x => x.Name.Equals("TryParse"));
#else
            var method = type.GetType().GetMethods().Where(x=>x.Name.Equals("TryParse"));
#endif
            if (method.Count() == 0)
                return false;

            var methodinfo = method.FirstOrDefault();
            object[] args = { value.ToString(), null };
            return (bool)methodinfo.Invoke(null, args);
        }

        /// <summary>
        /// To commit the copied value for the dropdown column
        /// </summary>
        /// <param name="column">Contains particular column</param>
        /// <param name="changedValue">Conatins copied value</param>
        /// <param name="type">contains type of selected column</param>
        private void CommitValueDropDownColumn(TreeGridColumn column, ref object changedValue, Type type)
        {
            IEnumerable list = null;
            string displayMemberPath;
            string valueMemberPath;
            if (column is TreeGridComboBoxColumn)
                list = (column as TreeGridComboBoxColumn).ItemsSource;
            displayMemberPath = (column as TreeGridComboBoxColumn).DisplayMemberPath;
            valueMemberPath = (column as TreeGridComboBoxColumn).SelectedValuePath;
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
                    if (value.Equals(pdc.GetValue(enumerator.Current, valueMemberPath)))
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

        /// <summary>
        /// To raise the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyContent"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the event data.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteEventArgs RaiseCopyContentEvent(GridCopyPasteEventArgs args)
        {
            return TreeGrid.RaiseCopyContentEvent(new GridCopyPasteEventArgs(false, this.TreeGrid));
        }

        /// <summary>
        /// To raise the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.PasteContent"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the event data.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.TreeGrid.GridCopyPasteEventArgs"/>.
        /// </returns>
        protected virtual GridCopyPasteEventArgs RaisePasteContentEvent(GridCopyPasteEventArgs args)
        {
            return TreeGrid.RaisePasteContentEvent(new GridCopyPasteEventArgs(false, this.TreeGrid));
        }

        /// <summary>
        /// To raise the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.GridCopyOption"/> event in SfTreeGrid.
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
        /// Returns the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/>.
        /// </returns>
        protected virtual TreeGridCopyPasteCellEventArgs RaiseCopyTreeGridCellContentEvent(TreeGridColumn column, object rowData, object clipboardValue)
        {
            return TreeGrid.RaiseCopyTreeGridCellContentEvent(new TreeGridCopyPasteCellEventArgs(false, column, this.TreeGrid, rowData, clipboardValue));

        }

        /// <summary>
        /// To raise the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.GridPasteOption"/> event in SfTreeGrid.
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
        /// Returns the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/>.
        /// </returns>
        protected virtual TreeGridCopyPasteCellEventArgs RaisePasteGridCellContentEvent(TreeGridColumn column, object rowData, object clipboardValue)
        {
            return TreeGrid.RaisePasteTreeGridCellContentEvent(new TreeGridCopyPasteCellEventArgs(false, column, this.TreeGrid, rowData, clipboardValue));
        }

        /// <summary>
        /// Copy the selected rows from SfTreeGrid to clipboard.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+C</b> key is pressed.
        /// </remarks>
        public void Copy()
        {
            if (this.TreeGrid.GridCopyOption.HasFlag(GridCopyOption.None) || !this.TreeGrid.GridCopyOption.HasFlag(GridCopyOption.CopyData))
                return;
            this.OnCopyingClipBoardContent(TreeGrid.SelectedItems, false);
        }

        /// <summary>
        /// Copy the selected rows and sets the default or null or empty value.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+X</b> key is pressed.
        /// </remarks>
        public void Cut()
        {
            if (!this.TreeGrid.GridCopyOption.HasFlag(GridCopyOption.None) && this.TreeGrid.GridCopyOption.HasFlag(GridCopyOption.CutData))
                this.OnCopyingClipBoardContent(TreeGrid.SelectedItems, true);
        }

        /// <summary>
        /// Paste the clipboard copied content to the selected rows in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This method is invoked when the <b>Ctrl+V</b> key is pressed.
        /// </remarks>
        public void Paste()
        {
            if (!this.TreeGrid.GridPasteOption.HasFlag(GridPasteOption.None) && this.TreeGrid.GridPasteOption.HasFlag(GridPasteOption.PasteData))
                this.PasteRowsToClipboard();
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCutCopyPaste"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCutCopyPaste"/> class.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release all the resources. </param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
                this.TreeGrid = null;
            isdisposed = true;
        }
    }
}
