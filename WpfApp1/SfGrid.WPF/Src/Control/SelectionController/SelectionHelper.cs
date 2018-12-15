#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using Syncfusion.Data.Extensions;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT
using Windows.UI.Core;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid.Helpers
{
#if !WinRT && !UNIVERSAL
    using System.Windows.Media;
    using System.Windows.Input;
    using System.Windows; 
    using System.Data; 
#endif
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
    using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml;
    using Windows.UI.Core;
    using Windows.Foundation;
    using Windows.Devices.Input;
#endif
    /// <summary>
    /// Represents an extension class that provides the methods for selection in SfDataGrid.
    /// </summary>
   public static class SelectionHelper
    {
        /// <summary>
        /// Determines whether the control key is pressed in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the control key is pressed; otherwise, <b>false</b>.
        /// </returns>
        public static bool CheckControlKeyPressed()
        {
#if UWP
            if (Window.Current.CoreWindow.GetAsyncKeyState(Key.Control).HasFlag(CoreVirtualKeyStates.Down))            
#else
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
#endif                
                return true;            
            else
                return false;
        }

        /// <summary>
        /// Determines whether the Tab key is pressed in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the Tab key is pressed; otherwise, <b>false</b>.
        /// </returns>
        public static bool CheckTabKeyPressed()
        {
#if UWP
            if (Window.Current.CoreWindow.GetAsyncKeyState(Key.Control).HasFlag(CoreVirtualKeyStates.Down))
#else
            if (Keyboard.IsKeyDown(Key.Tab))
#endif
                return true;
            else
                return false;
        }

#if UWP
        internal static ActivationTrigger ConvertPointerDeviceTypeToActivationTrigger(PointerDeviceType pointerDeviceType)
        {
            var activationTrigger = ActivationTrigger.Mouse;
            if (pointerDeviceType == PointerDeviceType.Pen)
                activationTrigger = ActivationTrigger.Pen;
            else if (pointerDeviceType == PointerDeviceType.Touch)
                activationTrigger = ActivationTrigger.Touch;
            return activationTrigger;
        }
#endif

        /// <summary>
        /// Determines whether the Shift key is pressed in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the Shift key is pressed; otherwise, <b>false</b>.
        /// </returns>
        public static bool CheckShiftKeyPressed()
        {
#if UWP
            if (Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift).HasFlag(CoreVirtualKeyStates.Down))
#else
            if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
#endif
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the Alt key is pressed in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the Alt key is pressed; otherwise, <b>false</b>.
        /// </returns>
        public static bool CheckAltKeyPressed()
        {
#if UWP
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.Menu).HasFlag(CoreVirtualKeyStates.Down));
#else
            return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
#endif
        }
        
        /// <summary>
        /// Gets the index of the row positioned at the end of next page that is not currently in view of SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the end row index of next page.
        /// </returns>
        public static int GetNextPageIndex(this SfDataGrid DataGrid)
        {
            var CurrentCellManager = DataGrid.SelectionController.CurrentCellManager;
            var rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;            
            //The HeaderLineCount includes the AddNewRow also, hence the FirstNavigatingRowIndex is used.
            //When pressing from AddNewRow with DetailsView expanded, the focus is updated to DetailsView
            if (rowIndex < DataGrid.GetFirstNavigatingRowIndex())
                rowIndex = 0;
            //WRT-5949 While pressing the pagedown key with Custom RowHeight, next page index will be calculated by using RowHeights.SetRange method.
            //Instead of using GetNextPage method. Since it returns the row index which has default row height.

            //WPF-20498 While navigate the datagrid using PageDown key out of view records are not come to view when using FooterRows and FrozonRows,  
            //returns the firstBodyVisibleIndex to GetNextPage method because ScrollPageSize calculated from FrozonRows.
            int nextPageIndex = 0;
            var visibleLines = DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            int firstBodyVisibleIndex = -1;
            if (visibleLines.FirstBodyVisibleIndex < visibleLines.Count)
                firstBodyVisibleIndex = visibleLines[visibleLines.FirstBodyVisibleIndex].LineIndex;
#if WPF                      
            var index = firstBodyVisibleIndex > rowIndex ? firstBodyVisibleIndex : rowIndex;
            nextPageIndex = DataGrid.CanQueryRowHeight() ? DataGrid.VisualContainer.SetRowHeights(rowIndex, System.Windows.Controls.ExpandDirection.Down) : DataGrid.VisualContainer.ScrollRows.GetNextPage(index);
#else       
            var index = firstBodyVisibleIndex > rowIndex ? firstBodyVisibleIndex : rowIndex;
            nextPageIndex = DataGrid.CanQueryRowHeight() ? DataGrid.VisualContainer.SetRowHeights(rowIndex, Key.Down) : DataGrid.VisualContainer.ScrollRows.GetNextPage(index);           
#endif

            //This Condition is while the CurrentRowColumnIndex is AddNewRow position Bottom, then it will Stays the selection in the AddNewRow while we pressing the PageDown Button
            if ((CurrentCellManager.IsAddNewRow || CurrentCellManager.IsFilterRow) && nextPageIndex > DataGrid.GetLastNavigatingRowIndex()
                && CurrentCellManager.CurrentRowColumnIndex.RowIndex > DataGrid.GetLastRowIndex())
                    return CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            //The LastNavigatingRowIndex returns the AddNewRowIndex hence the GetLastRowIndex is used.
             nextPageIndex = nextPageIndex <= DataGrid.GetLastRowIndex() ? nextPageIndex : DataGrid.GetLastRowIndex();         
            //WPF-18098 Here we have added the Condition to avoid the focus to set in the DetailsViewGrid.
            //If the focus is set to the DetailsViewGrid,the ParentGrid Cant be accessed.
            while (DataGrid.IsInDetailsViewIndex(nextPageIndex))
            {
                //GetLastNavigatingRowIndex will return with AddNewRowInex hence the GetLastRowIndex is used.
                if (nextPageIndex == DataGrid.GetLastRowIndex())
                {
                    //WPF-23996(Issue 7)- When we press the PageDown key if it’s in before the lastpage means it’s should 
                    //get the nextpageindex and return, but its gets the previousScrollIndexindex so this loop is 
                    //not terminates so only the deadlock occurs. So retrun the nextpageindex
                    do
                    {
                        nextPageIndex = DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(nextPageIndex);
                    } while (DataGrid.IsInDetailsViewIndex(nextPageIndex));
                    return nextPageIndex;
                }
                else
                    //Gets next row index when the nextPageIndex is the DetailsViewIndex
                    nextPageIndex = CurrentCellManager.GetNextRowIndex(nextPageIndex);
            }           
            return nextPageIndex;
        }

        /// <summary>
        /// Gets the index of the row positioned at the start of the previous page that is not currently in view of SfDataGrid.
        /// </summary>
        /// <returns>
        /// The start index of previous page.
        /// </returns>
        public static int GetPreviousPageIndex(this SfDataGrid DataGrid)
        {
            var CurrentCellManager = DataGrid.SelectionController.CurrentCellManager;
            var rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;

            //WPF-20498 While navigate the datagrid using PageUp key out of view records are not come to view when using FooterRows and FrozonRows,  
            //returns the lastBodyVisibleIndex to GetPreviousPage method  because ScrollPageSize calculated from FooterRows.
            int previousPageIndex = 0;
            int lastBodyVisibleIndex = -1;
            var visibleLines = DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            if (visibleLines.LastBodyVisibleIndex < visibleLines.Count)
                lastBodyVisibleIndex = visibleLines[visibleLines.LastBodyVisibleIndex].LineIndex;
#if WPF      
            var index = lastBodyVisibleIndex < rowIndex ? lastBodyVisibleIndex : rowIndex;
            previousPageIndex = DataGrid.CanQueryRowHeight() ? DataGrid.VisualContainer.SetRowHeights(rowIndex, System.Windows.Controls.ExpandDirection.Up) : DataGrid.VisualContainer.ScrollRows.GetPreviousPage(index);
#else
            var index = lastBodyVisibleIndex < rowIndex ? lastBodyVisibleIndex : rowIndex;
            previousPageIndex = DataGrid.CanQueryRowHeight() ? DataGrid.VisualContainer.SetRowHeights(rowIndex, Key.Up) : DataGrid.VisualContainer.ScrollRows.GetPreviousPage(index);
#endif
            //This Condition is while the CurrentRowColumnIndex is AddNewRow position Top, then it will Stays the selection in the AddNewRow while we pressing the PageUP Button
            //when navigating a DatGrid Using PageUp key, CurrentRowColumnIndex is AddNewRow position Top need to avoid selection moved to Body Rows based on GetFirstRowIndex()
            //instead of GetFirstNavigatingRowIndex()
            if ((CurrentCellManager.IsAddNewRow || CurrentCellManager.IsFilterRow) && previousPageIndex < DataGrid.GetFirstRowIndex()
                && CurrentCellManager.CurrentRowColumnIndex.RowIndex < DataGrid.GetFirstRowIndex())
                return CurrentCellManager.CurrentRowColumnIndex.RowIndex;  
            //The FirstNavigatingRowIndex will returns the AddNewRowIndex hence the GetFirstRowIndex is used.
            previousPageIndex = previousPageIndex < DataGrid.GetFirstRowIndex() ? DataGrid.GetFirstRowIndex() : previousPageIndex;
            //WPF-18098 Here we have added the Condition to avoid the focus to set in the DetailsViewGrid.
            //If the focus is set to the DetailsViewGrid,the ParentGrid Cant be accessed.
            while (DataGrid.IsInDetailsViewIndex(previousPageIndex) && previousPageIndex != DataGrid.GetFirstNavigatingRowIndex())
            {
                previousPageIndex = DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousPageIndex); 
            }
            //WPF - 18503 When we naviagte the DataGrid using Up key When the FrozenGroupHeader is true, returns the Outofview RowIndex
            //based on FrozenGroupHeader count, so add FrozenGroupHeader count to previousPageIndex.
            VisibleLineInfo lineInfo = DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(previousPageIndex);
            if (DataGrid.GridModel.HasGroup && DataGrid.AllowFrozenGroupHeaders && lineInfo != null && previousPageIndex != DataGrid.GetFirstRowIndex())
                previousPageIndex = previousPageIndex + DataGrid.View.GroupDescriptions.Count;
            return previousPageIndex;
        }

        /// <summary>
        /// Method which returns the Last row index in DataGrid. This method was used in Detail View.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        internal static int GetLastNavigatingRowIndex(this SfDataGrid DataGrid, SfDataGrid dataGrid)
        {
            int count;
            var CurrentCellManager = DataGrid.SelectionController.CurrentCellManager;
            var lastRowIndex = dataGrid.VisualContainer.RowCount - dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) - 1;
            for (var start = lastRowIndex; start >= 0; start--)
            {
                if (dataGrid.VisualContainer.RowHeights.GetHidden(start, out count)) continue;
                lastRowIndex = start;
                break;
            }
            return lastRowIndex;
        }
        
        /// <summary>
        /// Gets the next row info at the specified row index in SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get next row info.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of row to get next row info
        /// </param>
        /// <returns>
        /// Returns the next row info of the specified row index.
        /// </returns>
        public static GridRowInfo GetNextRowInfo(this SfDataGrid DataGrid, int rowIndex)
        {
            if (rowIndex < 0)
                return null;           

            //While Editing the LastRowIndex then here we can getting the LastRowIndex if there is no AddNewRowPosition at bottom.
            if (!DataGrid.IsAddNewIndex(rowIndex) && rowIndex == DataGrid.GetLastNavigatingRowIndex() &&
                rowIndex == DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
            {
                rowIndex = DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(rowIndex);
            }
           
            //The Below condition is Checked with while Editng the Record if it gets the View then we have to give the Selection.
            if (rowIndex > DataGrid.GetLastNavigatingRowIndex() || rowIndex < DataGrid.GetFirstNavigatingRowIndex())
                return null;

            var rowInfo = (DataGrid.SelectionController as GridBaseSelectionController).GetGridSelectedRow(rowIndex);   
            
            //rowInfo which have stored the Record for the Corresponding RowIndex By using the RowInfo.NodeEntry

            if (rowInfo.NodeEntry is NestedRecordEntry)
                rowInfo = DataGrid.GetNextRowInfo(DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(rowIndex));                
            else if (rowInfo.NodeEntry is SummaryRecordEntry)
            {
                var parent = ((SummaryRecordEntry)rowInfo.NodeEntry).Parent as Group;
                if (parent.ItemsCount == 1)
                    rowInfo = DataGrid.GetNextRowInfo(DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(rowIndex));                
            }
            return rowInfo;
        }

        /// <summary>
        /// Gets the previous row info at the specified row index of SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get previous row info.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of row to get previous row info.
        /// </param>
        /// <returns>
        /// Returns the previous row info of specified row index.
        /// </returns>
        public static GridRowInfo GetPreviousRowInfo(this SfDataGrid DataGrid, int rowIndex)
        {
            if (rowIndex < 0)
                throw new InvalidOperationException("Negative rowIndex in GetNextRecordEntry");

            if (!DataGrid.IsAddNewIndex(rowIndex) && rowIndex == DataGrid.GetFirstDataRowIndex() &&
                rowIndex == DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
            {
                rowIndex = DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(rowIndex);
            }

            //The Below condition is Checked with while Editng the Record if it gets the View then we have to give the Selection.
            if (rowIndex > DataGrid.GetLastNavigatingRowIndex() || rowIndex < DataGrid.GetFirstNavigatingRowIndex())
                return null;

            var rowInfo = (DataGrid.SelectionController as GridBaseSelectionController).GetGridSelectedRow(rowIndex);

            if ((rowInfo.NodeEntry is NestedRecordEntry))
                rowInfo = DataGrid.GetPreviousRowInfo(DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(rowIndex));
            else if (rowInfo.NodeEntry is Data.Group)
            {
                if (((Data.Group)rowInfo.NodeEntry).ItemsCount == 1)
                    rowInfo = DataGrid.GetPreviousRowInfo(DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(rowIndex));                             
            }            
            return rowInfo;
        }
       

       /// <summary>
       /// Method which helps to find the Previous RowIndex while the currentRowIndex is in DetailsViewIndex.
       /// </summary>
       /// <param name="DataGrid"></param>
       /// <param name="rowIndex"></param>
       /// <returns></returns>
       internal static int GetLastParentRowIndex(this SfDataGrid DataGrid,int rowIndex)
        {
               while(DataGrid.IsInDetailsViewIndex(rowIndex))
               {
                   rowIndex = DataGrid.SelectionController.CurrentCellManager.GetPreviousRowIndex(rowIndex);
               }
               return rowIndex;
        }
        
        internal static AddNewRowPosition GetAddNewRowPosition(this SfDataGrid DataGrid)
        {
            if (DataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop || DataGrid.AddNewRowPosition == AddNewRowPosition.Top)
                return AddNewRowPosition.Top;
            else if (DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                return AddNewRowPosition.Bottom;
            return AddNewRowPosition.None;
        }

        internal static FilterRowPosition GetFilterRowPosition(this SfDataGrid DataGrid)
        {
            if (DataGrid.FilterRowPosition == FilterRowPosition.FixedTop || DataGrid.FilterRowPosition == FilterRowPosition.Top)
                return FilterRowPosition.Top;
            else if (DataGrid.FilterRowPosition == FilterRowPosition.Bottom)
                return FilterRowPosition.Bottom;
            return FilterRowPosition.None;
        }

        /// <summary>
        /// Gets the index of the DataRow positioned at start of the SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get index of first DataRow.
        /// </param>
        /// <returns>
        /// Returns the index of first DataRow in SfDataGrid.If the record count is zero, return -1.
        /// </returns>
        public static int GetFirstDataRowIndex(this SfDataGrid DataGrid)
        {
            var topBodyCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);
            if (DataGrid.GetRecordsCount(false) == 0)
                return -1;

            int index = DataGrid.HeaderLineCount + topBodyCount;
            if (DataGrid.FilterRowPosition == FilterRowPosition.Top)
                index += 1;
            if (DataGrid.AddNewRowPosition == AddNewRowPosition.Top)
                index += 1;
            int count = 0;
            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }

        /// <summary>
        /// Gets the index of the row positioned at the start point of SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get the first row index.
        /// </param>
        /// <returns>
        /// Returns the first row index in SfDataGrid. If the record count is zero and AddNewRow position is None, return -1.
        /// </returns>
        public static int GetFirstRowIndex(this SfDataGrid DataGrid)
        {
            //While Removing all the rows and the AddNewRow is present in the view at this case it returned as -1. 
            //So Added the Condition to check the DataGridView.
            if (DataGrid.GetRecordsCount() == 0)
                return -1;

            int index = DataGrid.HasView ? DataGrid.HeaderLineCount : DataGrid.GetHeaderIndex() + 1;
            if (DataGrid.FilterRowPosition == FilterRowPosition.Top)
                index += 1;
            if (DataGrid.AddNewRowPosition == AddNewRowPosition.Top)
                index += 1;
            int count = 0;
            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }

       /// <summary>
       /// Gets the last row index than can be interact for selection.
       /// </summary>
       /// <param name="DataGrid"></param>
       /// <returns></returns>
       internal static int GetFirstNavigatingRowIndex(this SfDataGrid DataGrid)
       {
           //Wehn the record collection is empty the AddNewRow can't be edited when it is in Bottom because this method returns the index as -1. 
           //Hence the below code is added to return the exact AddNewRow index.
           bool isFilterRow = DataGrid.FilterRowPosition == FilterRowPosition.FixedTop || 
                (DataGrid.FilterRowPosition == FilterRowPosition.Top && DataGrid.AddNewRowPosition != AddNewRowPosition.FixedTop) ||
                (DataGrid.GetRecordsCount() == 0 && DataGrid.FilterRowPosition == FilterRowPosition.Bottom && DataGrid.AddNewRowPosition == AddNewRowPosition.None);

            bool isAddNewRow = !isFilterRow && (DataGrid.GetAddNewRowPosition() == AddNewRowPosition.Top ||
                (DataGrid.GetRecordsCount() == 0 && DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom));

            int index = -1;
            if (isAddNewRow)
                index = DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
            else
                index = isFilterRow ? DataGrid.GetFilterRowIndex() : DataGrid.GetFirstRowIndex();

           int count = 0;

           for (int start = index; start >= 0; start--)
           {
               if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                   return start;
           }
           return index;
       }

       /// <summary>
       /// Gets the index of the DataRow positioned at end of the SfDataGrid.
       /// </summary>
       /// <param name="DataGrid">
       /// The corresponding DataGrid to get the first row index.
       /// </param>
       /// <returns>
       /// Returns the first row index in SfDataGrid. If the record count is zero or AddNewRow positioned at bottom , return -1.
       /// </returns>
        public static int GetLastDataRowIndex(this SfDataGrid DataGrid)
        {
            var footerCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            var bottomBodyCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);
            if (DataGrid.GetRecordsCount(false) == 0)
                return -1;
            int count = 0;
            int index = DataGrid.VisualContainer.RowCount - (DataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + bottomBodyCount + footerCount + 1);
            if (DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                index -= 1;
            if (DataGrid.FilterRowPosition == FilterRowPosition.Bottom)
                index -= 1;
            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }

        /// <summary>
        /// Gets the index of the row positioned at end of the SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get index of first last.
        /// </param>
        /// <returns>
        /// Returns the index of last row in SfDataGrid. If the record count is zero or AddNewRow positioned at bottom , return -1.
        /// </returns>
        public static int GetLastRowIndex(this SfDataGrid DataGrid)
        {
            if (DataGrid.GetRecordsCount() == 0)
                return -1;
            var footerCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            int count = 0;
            int index = DataGrid.VisualContainer.RowCount - (DataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount + 1);
            if (DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                index -= 1;
            if (DataGrid.FilterRowPosition == FilterRowPosition.Bottom)
                index -= 1;
            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }

        /// <summary>
        /// Gets the last row index than can be interact for selection.
        /// </summary>
        /// <param name="DataGrid"></param>
        /// <returns></returns>
        internal static int GetLastNavigatingRowIndex(this SfDataGrid DataGrid)
        {
            int lastRowIndex = -1;
            int count = 0;
            int recordCount = DataGrid.GetRecordsCount();
            //Wehn the record collection is empty the AddNewRow can't be edited when it is in Top because this method returns the index as -1. 
            //Hence the below code is added to return the exact AddNewRow index.
            bool isFilterRow = DataGrid.FilterRowPosition == FilterRowPosition.Bottom ||
                               //WPF-28914 when add new position is in top then the last row will be add new row, hence the below AddNewRow.Top condition is added.
                               (recordCount == 0 &&
                                ((DataGrid.FilterRowPosition == FilterRowPosition.Top &&
                                  DataGrid.AddNewRowPosition != AddNewRowPosition.Bottom &&
                                  DataGrid.AddNewRowPosition != AddNewRowPosition.Top) ||
                                 (DataGrid.FilterRowPosition == FilterRowPosition.FixedTop &&
                                  DataGrid.AddNewRowPosition == AddNewRowPosition.None)));

            bool isAddNewRow = !isFilterRow && (DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ||
                                                (recordCount == 0 &&
                                                 (DataGrid.AddNewRowPosition == AddNewRowPosition.Top ||
                                                  DataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop)));

            if (isAddNewRow)
                lastRowIndex = DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
            else if (isFilterRow)
                lastRowIndex = DataGrid.GetFilterRowIndex();
            else
            {
                if (recordCount == 0)
                    return -1;
                var footerCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);


                lastRowIndex = DataGrid.VisualContainer.RowCount -
                               (DataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount + 1);
                //WPF-35600 need to break the loop while the items count is present.
                bool isChildPresent = false;
                while (DataGrid.IsInDetailsViewIndex(lastRowIndex) &&
                       !DataGrid.VisualContainer.RowHeights.GetHidden(lastRowIndex, out count))
                {
                    //WRT-7171 Need to avoid the calling of BringInToView to avoid the improper width calculation for the Details view datagrid.
                    //while the BringInToView is called, the column width calculation take place based upon the Record not available in the view.
                    //This make the improper column width arrangement. 
                    var lastRowIndexRecord = DataGrid.GetRecordAtRowIndex(lastRowIndex);
                    var definition = DataGrid.DetailsViewDefinition;
                    int _count = definition.Count - 1;
                    RecordEntry recordEntry = DataGrid.DetailsViewManager.GetDetailsViewRecord(lastRowIndex);
                    if (recordEntry.IsExpanded)
                    {
                        do
                        {
                            var sourceGrid = (definition[_count] as GridViewDefinition).DataGrid;
                            var items = DataGrid.DetailsViewManager.GetChildSource(lastRowIndexRecord, definition[_count].RelationalColumn);
#if UWP
                            if (items != null && (items.AsQueryable().Count() != 0 || sourceGrid.AddNewRowPosition != AddNewRowPosition.None || sourceGrid.FilterRowPosition != FilterRowPosition.None))
#else
                            if (items != null && ((DataGrid.View.IsLegacyDataTable ? ((System.Data.DataView)items).Count != 0 : items.AsQueryable().Count() != 0) || sourceGrid.AddNewRowPosition != AddNewRowPosition.None || sourceGrid.FilterRowPosition != FilterRowPosition.None))
#endif
                            {
                                isChildPresent = true;
                                break;
                            }
                            _count -= 1;
                            lastRowIndex = DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(lastRowIndex);
                        } while (_count >= 0);
                        if (isChildPresent)
                            break;
                    }
                    else
                         lastRowIndex = DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(lastRowIndex);
                }
            }

            for (int start = lastRowIndex; start >= 0; start--)
            {
                if (!DataGrid.VisualContainer.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return lastRowIndex;
        }
        
      /// <summary>
      /// Gets the index of currently selected Details View row .
      /// </summary>
      /// <param name="DataGrid">
      /// The corresponding DataGrid to get index of selected Details View row.
      /// </param>
      /// <returns>
      /// Returns the index of selected Details View row. If the selection is not found at Details View, return -1.
      /// </returns>
       public static int GetSelectedDetailsViewGridRowIndex(this SfDataGrid DataGrid)
        {
            if (DataGrid.SelectedDetailsViewGrid == null)
                return -1;
            var detailsViewDataRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == DataGrid.SelectedDetailsViewGrid);
            if (detailsViewDataRow != null)
                return detailsViewDataRow.RowIndex;
            return -1;
        }

        /// <summary>
        /// Method which helps to find whether the group is expanded or not.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        internal static bool CheckGroupExpanded(this SfDataGrid DataGrid, Group group)
        {
            if (group == null)
                throw new NullReferenceException("Group can't be null");

            if (group.IsTopLevelGroup)
                return true;

            return group.IsExpanded ? DataGrid.CheckGroupExpanded(group.Parent as Group) : false;
        }
        
        /// <summary>
        /// Gets the index of the first column corresponding to the specified flow direction.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding dataGrid to get the index of first column.
        /// </param>
        /// <param name="flowdirection">
        /// Corresponding direction to get the index of first column.
        /// </param>
        /// <returns>
        /// Returns the index of first column.
        /// </returns>
        public static int GetFirstColumnIndex(this SfDataGrid dataGrid, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return dataGrid.GetLastColumnIndex();

            int firstColumnIndex = dataGrid.Columns.IndexOf(dataGrid.Columns.FirstOrDefault(col => (!col.IsHidden && col.ActualWidth != 0d)));
            //CurrentCell is updated when clicking on RowHeader when there is no columns in view, hence the below condition is added.
            if (firstColumnIndex < 0)
                return firstColumnIndex;

            if (dataGrid.DetailsViewManager.HasDetailsView)
                firstColumnIndex += 1;

            firstColumnIndex += dataGrid.HasView ? dataGrid.View.GroupDescriptions.Count : 0;
            if (dataGrid.ShowRowHeader)
                firstColumnIndex += 1;
            return firstColumnIndex;
        }

        /// <summary>
        /// Gets the index of the last column corresponding to the specified flow direction.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding dataGrid to get the index of last column.
        /// </param>
        /// <param name="flowdirection">
        /// Corresponding direction to get the index of last column.
        /// </param>
        /// <returns>
        /// Returns the index of last column.
        /// </returns>
        public static int GetLastColumnIndex(this SfDataGrid dataGrid, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return dataGrid.GetFirstColumnIndex();

            int lastIndex = dataGrid.Columns.IndexOf(dataGrid.Columns.LastOrDefault(col => (!col.IsHidden && col.ActualWidth != 0d)));
            //CurrentCell is updated when clicking on RowHeader when there is no columns in view, hence the below condition is added.
            if (lastIndex < 0)
                return lastIndex;

            lastIndex += dataGrid.HasView ? dataGrid.View.GroupDescriptions.Count : 0;
            if (dataGrid.DetailsViewManager.HasDetailsView)
                lastIndex += 1;            

            if (dataGrid.ShowRowHeader)
                lastIndex += 1;
            return lastIndex;
        }

        /// <summary>
        /// Gets the number of records in SfDataGrid.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding dataGrid to get the records count.
        /// </param>
        /// <param name="checkUnBoundRows">
        /// Indicates whether UnBoundRows count is considered to get the records count.
        /// </param>
        /// <returns>
        /// Returns the number of records in SfDataGrid.
        /// </returns>
        public static int GetRecordsCount(this SfDataGrid DataGrid, bool checkUnBoundRows = true)
        {
            int index = 0;
            if (DataGrid.HasView)
                index = DataGrid.View.Records.Count;

            if(checkUnBoundRows)
            {
                var topBodyCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);
                var bottomBodyCount = DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);
                index += topBodyCount + bottomBodyCount;
            }
            return index;
        }

        /// <summary>
        /// Method which return the mouse position in DataGrid.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        internal static Point GetPointPosition(MouseEventArgs args, UIElement relativeTo)
        {
#if WinRT || UNIVERSAL
            return args.GetCurrentPoint(relativeTo).Position;
#else
            return args.GetPosition(relativeTo);
#endif
        }

        internal static int ResolveColumnIndex(this SfDataGrid DataGrid, RowColumnIndex rowColumnIndex)
        {
            var currentCellManager = DataGrid.SelectionController.CurrentCellManager;
            return currentCellManager.CurrentRowColumnIndex.ColumnIndex < currentCellManager.GetFirstCellIndex() ? currentCellManager.GetFirstCellIndex() : currentCellManager.CurrentRowColumnIndex.ColumnIndex;
        }

        internal static bool HasFilter(this SfDataGrid DataGrid)
        {
            if (!DataGrid.HasView)
                return false;
            bool hasFilter = false;
            if (DataGrid.View is QueryableCollectionView)
                hasFilter = (DataGrid.View as QueryableCollectionView).RowFilter != null;
            else if (DataGrid.View is Syncfusion.Data.PagedCollectionView)
                hasFilter = (DataGrid.View as Syncfusion.Data.PagedCollectionView).RowFilter != null;
#if WPF
            else if (DataGrid.View is DataTableCollectionView)
                hasFilter = (DataGrid.View as DataTableCollectionView).Filter != null;
#endif
            else if (DataGrid.View is VirtualizingCollectionView)
                hasFilter = (DataGrid.View as VirtualizingCollectionView).RowFilter != null;
            else
                hasFilter = DataGrid.View.Filter != null;
            return hasFilter;
        }
 
        /// <summary>
        /// Shows the selection background for the specified row index.
        /// </summary>
        /// <param name="DataGrid"> 
        /// The corresponding DataGrid to enable the selection background.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to enable the selection background.
        /// </param>
        /// <remarks>
        /// The selection background applied based on the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowSelectionBrush"/> property.
        /// </remarks>
        public static void ShowRowSelection(this SfDataGrid DataGrid, int rowIndex)
        {
            DataRowBase row = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null)
            {
                row.IsSelectedRow = true;
            }
        }

        /// <summary>
        /// Hides the selection background for the specified row index.
        /// </summary>
        /// <param name="DataGrid"> 
        /// The corresponding DataGrid to hide the selection background.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to hide the selection background.
        /// </param>      
        public static void HideRowSelection(this SfDataGrid DataGrid, int rowIndex)
        {
            DataRowBase row = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null)
            {
                row.IsSelectedRow = false;
            }
        }

        /// <summary>
        /// Shows the row focus border for the specified row index.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to enable the row focus border.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to enable row focus border.
        /// </param>
        public static void ShowRowFocusBorder(this SfDataGrid DataGrid, int rowIndex)
        {
            DataRowBase row = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null && !row.IsSelectedRow && ((row.RowType != RowType.DefaultRow && row.RowType != RowType.DetailsViewRow && row.RowType != RowType.AddNewRow && row.RowType != RowType.UnBoundRow && row.RowType != RowType.FilterRow && DataGrid.NavigationMode == NavigationMode.Cell) || DataGrid.NavigationMode == NavigationMode.Row))
            {
                row.IsFocusedRow = true;
            }
        }

        /// <summary>
        /// Hides the row focus border for the specified row index.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to hide the row focus border.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to hide row focus border.
        /// </param>
        public static void HideRowFocusBorder(this SfDataGrid DataGrid)
        {
            DataRowBase row = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsFocusedRow);
            if (row != null)
            {
                row.IsFocusedRow = false;
            }
        }
       
        /// <summary>
        /// Updates the visual state of the RowHeader based on the current cell or row changed.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to update the visual state of RowHeader.
        /// </param>
        public static void UpdateRowHeaderState(this SfDataGrid DataGrid)
        {
            if (DataGrid.RowGenerator.Items.All(row => row.RowType == RowType.HeaderRow))
                return;
            var currentRow = DataGrid.RowGenerator.Items.FirstOrDefault(row => row.IsCurrentRow);
            if (currentRow != null)
            {
                currentRow.IsCurrentRow = false;
                (currentRow as GridDataRow).ApplyRowHeaderVisualState();
            }

            if (DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0)
                return;

            currentRow = DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            if (currentRow != null)
            {
                currentRow.IsCurrentRow = true;
                (currentRow as GridDataRow).ApplyRowHeaderVisualState();
            }
            else if (DataGrid.SelectedDetailsViewGrid != null && DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= DataGrid.GetFirstNavigatingRowIndex())
            {
                currentRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == DataGrid.SelectedDetailsViewGrid);
                if (currentRow != null)
                {
                    currentRow.IsCurrentRow = true;
                    (currentRow as GridDataRow).ApplyRowHeaderVisualState();
                }
            }
        }

        /// <summary>
        /// Returns the GridColumn for the given RowColumnIndex
        /// </summary>
        /// <param name="columnIndex">Corresponding ColumnIndex Value</param>
        /// <remarks></remarks>
        internal static GridColumn GetGridColumn(this SfDataGrid DataGrid, int columnIndex)
        {
            var index = DataGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            return index >= 0 && index < DataGrid.Columns.Count ? DataGrid.Columns[index] : null;//This code modifed as AkunaCapital Suggested in incident 136657
        }
        
        /// <summary>
        /// Gets the DetailsViewDataGrid for the specified row index.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get the DetailsViewDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding rowIndex to get the DetailsViewDataGrid.
        /// </param>
        /// <returns>
        /// Returns the DetailsViewDataGrid for the specified row index. 
        /// The DetailsViewDataGrid return null ,if specified row index doesn't have the DetailsViewDataGrid or it is in collapsed state or the DetailsViewDataGrid is not reused. 
        /// </returns>
        public static DetailsViewDataGrid GetDetailsViewGrid(this SfDataGrid DataGrid, int rowIndex)
        {
            if (!DataGrid.IsInDetailsViewIndex(rowIndex))
                return null;
            DetailsViewDataRow detailsViewDataRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(item => item.RowIndex == rowIndex);
            if (detailsViewDataRow == null)
            {
                //Get the order of DetailsViewDataGrid             
                var detailsviewIndex = DataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);
                //Get the parentrecord of the DetailsViewGrid by passing DetailsViewDataRow index
                RecordEntry recordEntry = DataGrid.DetailsViewManager.GetDetailsViewRecord(rowIndex);
                if (recordEntry == null || recordEntry.ChildViews == null)
                    return null;
                        
                var relationName = DataGrid.DetailsViewDefinition[detailsviewIndex].RelationalColumn;
                var childView = recordEntry.ChildViews[relationName].View;
                //Get DetailsViewDataRow by comparing ChildView       
                detailsViewDataRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid.View == childView);
            }
            return detailsViewDataRow != null ? detailsViewDataRow.DetailsViewDataGrid : null;
        }

       internal static bool IsInSummarryRow(this SfDataGrid DataGrid, int rowIndex)
        {
            if (!DataGrid.GridModel.HasGroup)
                return false;

            var summary = DataGrid.View.TopLevelGroup.DisplayElements[
                                 DataGrid.ResolveToRecordIndex(rowIndex)];

            return summary is Group || summary is SummaryRecordEntry;
        }

        internal static DetailsViewDataGrid GetDetailsViewGridInView(this SfDataGrid DataGrid, int rowIndex)
        {
            if (!DataGrid.IsInDetailsViewIndex(rowIndex))
                return null;

            var detailsViewDataRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(item => item.RowIndex == rowIndex);
            //In the Key Navigation while pressing the Down key, If the DetailsViewGrid has no Record Means the Navigation directly gets to the DetailsViewGrid.
            //If it is out of View. so we can bring the detailsViewGrid into View and skip the Navigation getting into the DetailsViewGrid.
            if (detailsViewDataRow == null)
            {
                DataGrid.DetailsViewManager.BringIntoView(rowIndex);
                detailsViewDataRow = DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == rowIndex);
            }
            return detailsViewDataRow!=null ? detailsViewDataRow.DetailsViewDataGrid : null;

        }

       internal static SfDataGrid GetSelectedDetailsViewDataGrid(this SfDataGrid dataGrid)
       {
           if (dataGrid.SelectedDetailsViewGrid == null)
               return null;
           if (dataGrid.SelectedDetailsViewGrid.SelectedDetailsViewGrid != null)
               return dataGrid.SelectedDetailsViewGrid.GetSelectedDetailsViewDataGrid();
           return dataGrid.SelectedDetailsViewGrid;
       }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> for the given RowColumnIndex.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get DataColumnBase.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to get the DataColumnBase.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> for the specified rowcolumnindex.
        /// </returns>       
        public static DataColumnBase GetDataColumnBase(this SfDataGrid DataGrid, RowColumnIndex rowColumnIndex)
        {
            // The current row column index cannot be decided while key down operations. will be ensured in Ensure rows and Ensure columns.
            if (rowColumnIndex.RowIndex == -1 || rowColumnIndex.ColumnIndex == -1)
                return null;

            var resetRowColumnIndex = rowColumnIndex;
            var dataRow = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);            
            if (dataRow != null && DataGrid.CanQueryCoveredRange())
            {                
                //var currentRowColumnIndex = DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;                
                var coveredCellInfo = DataGrid.CoveredCells.GetCoveredCellInfo(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex);
                if (coveredCellInfo != null)
                {
                    var coveredColumn = dataRow.VisibleColumns.Find(column => column.ColumnIndex == coveredCellInfo.MappedRowColumnIndex.ColumnIndex);

                    if (coveredColumn != null)
                    {
                        DataColumnBase dataColumn = null;
                        // get the bottom row if its in view.               
                        dataRow = DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Top && item.IsEnsured && item.RowVisibility == Visibility.Visible);

                        if (dataRow != null)
                        {
                            var dataRowLineInfo = DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(dataRow.RowIndex);

                            if (dataRowLineInfo != null && dataRowLineInfo.IsClippedOrigin)
                                dataRow = DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top || item.RowIndex == coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, coveredColumn.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);
                        }

                        // get the previous bottom row when the bottom is out of view. while scroll towards down.
                        var nextTopRow = DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top) && coveredCellInfo.Contains(item.RowIndex, coveredColumn.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);

                        if (nextTopRow != null)
                        {
                            var previousBottomRowLineInfo = DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextTopRow.RowIndex);
                            if (previousBottomRowLineInfo != null && previousBottomRowLineInfo.IsClippedOrigin)
                                nextTopRow = DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > nextTopRow.RowIndex || item.RowIndex == coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, coveredColumn.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);
                        }

                        // get the index of that row to compare with covered cell bottom index.               
                        var bottomRowIndex = dataRow == null ? (nextTopRow != null ? nextTopRow.RowIndex : coveredColumn.RowIndex) : dataRow.RowIndex;

                        // get column from bottom row that was in view.
                        if (dataRow != null)
                        {
                            // get the left column .  
                            dataColumn = dataRow.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                                     spannedColumn.IsEnsured &&
                                                                     spannedColumn.ColumnVisibility == Visibility.Visible &&
                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                                     spannedColumn.GridColumn != null && !spannedColumn.GridColumn.IsHidden);

                            if (dataColumn != null)
                            {
                                var visibleLineInfo = dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex);

                                if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                                {
                                    // get the next left column while the left column has not in view or it is clipped.                             
                                    dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                     (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                     coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                     item.IsEnsured &&
                                                                                     item.ColumnVisibility == Visibility.Visible &&
                                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                     item.GridColumn != null && !item.GridColumn.IsHidden);
                                }
                            }
                            else
                            {
                                dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                     (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                     coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                     item.IsEnsured &&
                                                                                     item.ColumnVisibility == Visibility.Visible &&
                                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                     item.GridColumn != null && !item.GridColumn.IsHidden);

                                var visibleLineInfo = dataColumn != null ? dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex) : null;

                                if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                                {
                                    // get the next left column while the left column has not in view or it is clipped.                             
                                    dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                     (item.ColumnIndex > dataColumn.ColumnIndex || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                     coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                     item.IsEnsured &&
                                                                                     item.ColumnVisibility == Visibility.Visible &&
                                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                     item.GridColumn != null && !item.GridColumn.IsHidden);
                                }
                            }
                        }
                        // get column from previous bottom row when bottom row is not in view.
                        else if (nextTopRow != null)
                        {
                            // get the left column .  
                            dataColumn = nextTopRow.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                                     spannedColumn.IsEnsured &&
                                                                     spannedColumn.ColumnVisibility == Visibility.Visible &&
                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                                     spannedColumn.GridColumn != null && !spannedColumn.GridColumn.IsHidden);

                            if (dataColumn != null)
                            {
                                var visibleLineInfo = dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex);

                                if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                                {
                                    // get the next left column while the left column has not in view or it is clipped.                             
                                    dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                     (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                     coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                     item.IsEnsured &&
                                                                                     item.ColumnVisibility == Visibility.Visible &&
                                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                     item.GridColumn != null && !item.GridColumn.IsHidden);
                                }
                            }
                            else
                            {
                                dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                    (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                    coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                    item.IsEnsured &&
                                                                                    item.ColumnVisibility == Visibility.Visible &&
                                                                                    DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                    !item.GridColumn.IsHidden);

                                var visibleLineInfo = dataColumn != null ? dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex) : null;

                                if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                                {
                                    // get the next left column while the left column has not in view or it is clipped.                             
                                    dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                                     (item.ColumnIndex > dataColumn.ColumnIndex || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                                     coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                                     item.IsEnsured &&
                                                                                     item.ColumnVisibility == Visibility.Visible &&
                                                                                     DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                                     !item.GridColumn.IsHidden);
                                }

                            }
                        }

                        // get the index of the column to compare with covered cell left index.
                        var leftColumnIndex = dataColumn == null ? coveredColumn.ColumnIndex : dataColumn.ColumnIndex;

                        resetRowColumnIndex = new RowColumnIndex(bottomRowIndex, leftColumnIndex);

                        if (dataRow == null)
                            dataRow = DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => item.RowIndex == bottomRowIndex);
                    }
                }
            }

            if (dataRow != null && !(dataRow is DetailsViewDataRow))
            {
                DataColumnBase dataColumn = null;
                if (dataRow is SpannedDataRow)
                    dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column is SpannedDataColumn && (column.ColumnElement as GridCell).CanSelectCurrentCell());
                else
                    dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == resetRowColumnIndex.ColumnIndex);
#if WPF
                if (dataColumn != null && DataGrid.useDrawing)               
                    dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();               
#endif
                return dataColumn;
            }
            return null;
        }

        /// <summary>
        /// Determines whether the corresponding cell in a column is focusible or not.
        /// </summary>
        /// <param name="DataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to check whether the cells in a column is focusible or not. 
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the cells in a column is focusible; otherwise, <b>false</b>.
        /// </returns>
        public static bool AllowFocus(this SfDataGrid DataGrid, RowColumnIndex rowColumnIndex)
        {
            var gridColumn = DataGrid.GetGridColumn(rowColumnIndex.ColumnIndex);
            if (gridColumn != null)
            {
                //Added the code to check whether the column width is 0 which is missed in previous codding.
                return DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) ||
                       DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex)
                    ? !gridColumn.IsHidden && gridColumn.ActualWidth != 0d
                    : gridColumn.ActualWidth != 0d && gridColumn.AllowFocus && !gridColumn.IsHidden;
            }
            return false;
        }

        /// <summary>
        /// Gets the record for the specified row index.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get the record.
        /// </param>
        /// <param name="index">
        /// The corresponding row index to get the record.
        /// </param>
        /// <returns>
        /// The data item corresponding to the specified row index. Return the <b>null</b> , if the specified row index is of AddNewRow .
        /// </returns>
        public static object GetRecordAtRowIndex(this SfDataGrid DataGrid, int index)
        {
            if (DataGrid.IsAddNewIndex(index))
            {
                return DataGrid.View.IsAddingNew ? DataGrid.View.CurrentAddItem : null;
            }
            else
            {
                if (DataGrid.GridModel.HasGroup)
                {
                    var newObj = DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.ResolveToRecordIndex(index)];
                    if (newObj is RecordEntry)
                    {
                        return ((RecordEntry)newObj).Data;
                    }
                    else if (newObj is NestedRecordEntry)
                    {
                        return ((RecordEntry)newObj.Parent).Data;
                    }
                }
                else
                {
                    var resolvedIndex = DataGrid.ResolveToRecordIndex(index);
                    var recordsCount = DataGrid.GetRecordsCount(false);
                    return (recordsCount > 0 && resolvedIndex >= 0 && resolvedIndex < recordsCount)
                               ? DataGrid.View.Records[resolvedIndex].Data
                               : null;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the record for the specified row index.
        /// </summary>
        /// <param name="DataGrid">
        /// The corresponding DataGrid to get the record.
        /// </param>
        /// <param name="index">
        /// The corresponding row index to get the record.
        /// </param>
        /// <returns>
        /// The data item corresponding to the specified row index. Return <b>null</b> , if the row index is of AddNewRow .
        /// </returns>
        public static NodeEntry GetRecordEntryAtRowIndex(this SfDataGrid DataGrid, int index)
        {
            if (DataGrid.IsAddNewIndex(index))
                return null;
            else
            {
                if (DataGrid.GridModel.HasGroup)
                {
                    var newObj = DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.ResolveToRecordIndex(index)];
                    return newObj;
                }
                else
                {
                    var resolvedIndex = DataGrid.ResolveToRecordIndex(index);
                    var recordsCount = DataGrid.GetRecordsCount(false);
                    return (recordsCount > 0 && resolvedIndex >= 0 && resolvedIndex < recordsCount)
                               ? DataGrid.View.Records[resolvedIndex]
                               : null;
                }
            }
        }

       /// <summary>
       /// Gets the top level parent grid.
       /// </summary>
       /// <param name="DataGrid">
       /// The corresponding DataGrid to get the top level parent grid.
       /// </param>
       /// <returns>
       /// Returns the top level parent grid.
       /// </returns>
       public static SfDataGrid GetTopLevelParentDataGrid(this SfDataGrid DataGrid)
       {
           SfDataGrid parentGrid = DataGrid;
           while (parentGrid.NotifyListener != null)
               parentGrid = parentGrid.NotifyListener.GetParentDataGrid();
           return parentGrid;
       }
	   
       /// <summary>
       /// Gets the immediate parent DataGrid of the specified DetailsViewDataGrid.
       /// </summary>
       /// <param name="DataGrid">
       /// The corresponding DetailsViewDataGrid to get its parent grid.</param>
       /// <returns>
       /// Returns the corresponding parent grid .
       /// </returns>
       public static SfDataGrid GetParentDataGrid(this SfDataGrid dataGrid)
       {
           if (dataGrid is DetailsViewDataGrid && dataGrid.NotifyListener != null)
               return dataGrid.NotifyListener.GetParentDataGrid();
           return null;           
       }

       /// <summary>
       /// Gets the SfDataGrid that has current cell.
       /// </summary>
       /// <param name="dataGrid">
       /// The SfDataGrid.
       /// </param>
       /// <returns> 
       /// The SfDataGrid that has current cell.
       /// </returns>
       public static SfDataGrid GetDataGrid(this SfDataGrid dataGrid)
       {
           if (dataGrid == null)
               return dataGrid;

           if (dataGrid.SelectionController.CurrentCellManager.HasCurrentCell)
               return dataGrid;
           else
               return GetDataGrid(dataGrid.SelectedDetailsViewGrid);
       }
      
        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> for the specified rowcolumnindex.
        /// </summary>
        /// <param name="DataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to get the cell info.
        /// </param>
        /// <returns>
        /// Returns <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> for the specified rowcolumnindex.
        /// </returns>
        public static GridCellInfo GetGridCellInfo(this SfDataGrid DataGrid, RowColumnIndex rowColumnIndex)
        {
            var column = DataGrid.GetGridColumn(rowColumnIndex.ColumnIndex);
            if (DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                return new GridCellInfo(column, rowColumnIndex.RowIndex, true);
            if (DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
                return new GridCellInfo(column, true, rowColumnIndex.RowIndex);
            var nodeEntry = DataGrid.GetNodeEntry(rowColumnIndex.RowIndex);
            //Returns GridCellInfo when the rowColumnIndex is in GridUnboundRow
            if (nodeEntry == null && DataGrid.IsUnBoundRow(rowColumnIndex.RowIndex))
            {
                var unBoundRow = DataGrid.GetUnBoundRow(rowColumnIndex.RowIndex);
                if (unBoundRow != null)
                    return new GridCellInfo(column, rowColumnIndex.RowIndex, unBoundRow);
                return null;
            }
            
            var data = DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            if (data != null && !DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                return new GridCellInfo(column, data, nodeEntry);

            else if (nodeEntry != null)
                return new GridCellInfo(column, null, nodeEntry, rowColumnIndex.RowIndex);
            return null;
        }

        public static NodeEntry GetNodeEntry(this SfDataGrid DataGrid, int rowIndex)
        {
            if(rowIndex == -1 || DataGrid.IsAddNewIndex(rowIndex))
                return null;
            
            //Node entry value has been set while it is Expanded so here checked with nodeEntry is Expanded or Not.
            //Because while changing the position of the Record If we click on the DetailsViewIndex the childView set as null.
            if (DataGrid.GridModel.HasGroup)
                return DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.ResolveToRecordIndex(rowIndex)];
            else
            {
                int recordIndex = DataGrid.ResolveToRecordIndex(rowIndex);
                if (recordIndex < 0)
                    return null;
                RecordEntry nodeEntry = DataGrid.View.GetRecordAt(recordIndex) as RecordEntry;
                //Exception throws While Navigating through keys in the HideEmptyGridViewDefinition.Hence we have return the nodeEntry value through relational column.
                if (DataGrid.IsInDetailsViewIndex(rowIndex) && nodeEntry != null && nodeEntry.IsExpanded) 
                {
                    var detailsviewIndex = DataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);
                    var relationalColumnName = DataGrid.DetailsViewDefinition[detailsviewIndex].RelationalColumn;
                    return nodeEntry.ChildViews[relationalColumnName];
                }
                return nodeEntry;
            }
        }
    }
}
