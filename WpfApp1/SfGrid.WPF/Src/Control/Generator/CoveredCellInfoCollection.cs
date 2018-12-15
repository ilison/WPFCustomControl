#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
using System.Windows.Data;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Class that handles the cell merging in SfDataGrid.
    /// </summary>
    public class MergedCellManager :IDisposable
    {
        /// Initialize the merged row.
        ///     1. IsSpannedRow.
        ///     2. IsSpannedColumn.
        ///     3. Get's merging range.
        ///     4. Added to CoeveredCell's collection based on range avialable in DataGrid.
        /// Update the merged row.
        ///     1. Reset IsSpannedRow.
        ///     2. Reset IsSpannedColumn
        ///     3. Reset range.
        ///     4. Remove Range.
        ///     5. Update mapped row column index of covered cells based on visible row and column.

        internal SfDataGrid dataGrid;
        bool canRaiseEvent = true;
        private bool isdisposed = false;
        /// <summary>
        /// decide that the event can be raised while end edit.
        /// </summary>
        internal bool CanRasieEvent
        {
            get { return canRaiseEvent; }
            set { canRaiseEvent = value; }
        }

        public MergedCellManager(SfDataGrid sfDataGrid)
        {
            dataGrid = sfDataGrid;
        }
        /// <summary>
        /// Raise event for merged row.
        /// </summary>
        /// <param name="dr"></param>
        internal void InitializeMergedRow(DataRowBase dr)
        {
            dr.VisibleColumns.ForEach(item =>
            {
                if (CanQueryColumn(item))
                    this.EnsureMergedCells(dr,item, dr.RowData);
            });
        }

        /// <summary>
        /// Update merged rows column element and its properties - IsSpannedColumn, IsSpannedRow.
        /// </summary>
        /// <param name="dr"></param>
        internal void UpdateMergedRow(DataRowBase dr)
        {
            dr.VisibleColumns.ForEach(item =>
            {
                // For Data manipulation - while add or remove rows. 
                var coveredCellInfo = this.dataGrid.CoveredCells.GetCoveredCellInfo(item);
                if (this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                    coveredCellInfo != null && coveredCellInfo.MappedRowColumnIndex.RowIndex >= item.RowIndex && coveredCellInfo.MappedRowColumnIndex.RowIndex <= coveredCellInfo.Bottom && 
                    item.ColumnIndex >= coveredCellInfo.MappedRowColumnIndex.ColumnIndex && item.ColumnIndex <= coveredCellInfo.Right &&
                    item.ColumnVisibility == Visibility.Collapsed &&
                    item.GridColumn != null && !item.GridColumn.IsHidden)
                    item.ColumnVisibility = Visibility.Visible;    // while scroll vertically - next to next meregd rows to change its column visibility.
                else
                {
                    // coveredinfo will be null while add new row above merged rows
                    if (coveredCellInfo == null)
                    {
                        if (item.IsSpannedColumn && this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null)
                        {
                            item.isSpannedColumn = false;
                            item.ColumnVisibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        // have covered range but not false under the above conditions while remove rows - while remove row between meregd range, default row moved up and used as merged row.
                        if (!item.IsSpannedColumn && (dr.RowType == RowType.DefaultRow || dr.RowType == RowType.UnBoundRow))
                            this.EnsureMergedCells(dr, item, dr.RowData);
                    }
                }
            });
        }


        /// <summary>
        /// Refresh the merged row within the range.
        /// </summary>
        /// <param name="dr"></param>
        internal void RefreshMergedRows(DataRowBase dr)
        {
            var coveredCellInfoCollection = this.dataGrid.CoveredCells.GetCoveredCellsRange(dr);

            if (coveredCellInfoCollection.Count == 0) return;
            
            foreach (var coveredRange in coveredCellInfoCollection)
            {
                var invalidateRows = this.dataGrid.RowGenerator.Items.FindAll(item => item.RowIndex <= coveredRange.Bottom && item.RowIndex >= coveredRange.Top && (item.RowType == RowType.DefaultRow || item.RowType == RowType.UnBoundRow));
                foreach (var invalidateRow in invalidateRows)
                {
                    if (invalidateRow.RowType == RowType.AddNewRow && (invalidateRow.RowIndex == coveredRange.Top || invalidateRow.RowIndex == coveredRange.Bottom))
                        throw new Exception(String.Format("AddNewRow cells cannot be merged {0}", coveredRange));

                    if (!invalidateRow.IsSpannedRow)
                        this.InitializeMergedRow(invalidateRow);

                    invalidateRow.VisibleColumns.ForEach(r =>
                    {                       
                        var detailsViewDefinitionCount = this.dataGrid.DetailsViewDefinition.Count;
                        if (detailsViewDefinitionCount != 0 && this.dataGrid.IsInDetailsViewIndex(invalidateRow.RowIndex + detailsViewDefinitionCount))
                        {
                            var throwException = false;
                            foreach (var viewDefinition in this.dataGrid.DetailsViewDefinition)
                            {
                                var itemSource = this.dataGrid.DetailsViewManager.GetChildSource(dr.RowData, viewDefinition.RelationalColumn);
                                if (DetailsViewHelper.GetChildSourceCount(itemSource) > 0)
                                {
                                    throwException = true;
                                    break;
                                }
                            }

                            if (invalidateRow.RowIndex != coveredRange.Bottom && (throwException || !dataGrid.HideEmptyGridViewDefinition))
                                throw new Exception(String.Format("Given range is not valid {0} with the details view row", coveredRange));
                        }

                        if (r.ColumnIndex >= coveredRange.Left && r.ColumnIndex <= coveredRange.Right && r.GridColumn != null && !r.GridColumn.IsHidden)
                        {
                            if (r.ColumnVisibility == Visibility.Collapsed)
                                r.ColumnVisibility = Visibility.Visible;
                        }
                    }
                    );

                    if (!this.dataGrid.IsInDetailsViewIndex(invalidateRow.RowIndex))
                    {
                        invalidateRow.WholeRowElement.ItemsPanel.InvalidateMeasure();
                        invalidateRow.WholeRowElement.ItemsPanel.InvalidateArrange();
#if WPF
                        if (this.dataGrid.useDrawing)
                            invalidateRow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                    }
                }
            }            
        }

        /// <summary>
        /// Raise query for the each column
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="dc"></param>
        internal void EnsureMergedCells(DataRowBase dr, DataColumnBase dc, object dataContext)
        {
            if (dc.GridColumn == null)
                return;

            var coveredCell = dataGrid.CoveredCells.GetCoveredCell(dc.RowIndex, dc.ColumnIndex, dc.GridColumn, dataContext);

            if (coveredCell == null) return;

            //Throws exception for invalid range with rows.            
            this.dataGrid.CoveredCells.ContainsRow(coveredCell);
            // Throws exception for invalid range with columns. 
            this.dataGrid.CoveredCells.ContainsColumn(dr, coveredCell);
            
            if (!dc.GridColumn.hasCellTemplate && 
            (dc.GridColumn.hasCellTemplateSelector || 
            (dc.GridColumn.IsTemplate && ((dc.GridColumn as GridTemplateColumn).hasEditTemplateSelector || dataGrid.hasCellTemplateSelector))))                

            // Column has cell template selector will not get merge.

            {
                this.dataGrid.CoveredCells.Remove(coveredCell);
                return;
            }

            // Raise exception for the invalid range of unbound row.
            if (dr.RowType == RowType.UnBoundRow)
            {
                var bottomIndex = coveredCell.Bottom;
                var topIndex = coveredCell.Top;
                RowRegion topRowRegion = dr.RowRegion;
                if (dataGrid.RowGenerator.Items.Find(row => row.RowIndex == topIndex) != null)
                    topRowRegion = dataGrid.RowGenerator.Items.Find(row => row.RowIndex == topIndex).RowRegion;
                RowRegion bottomRowRegion = dr.RowRegion;
                if (dataGrid.RowGenerator.Items.Find(row => row.RowIndex == bottomIndex) != null)
                    bottomRowRegion = dataGrid.RowGenerator.Items.Find(row => row.RowIndex == bottomIndex).RowRegion;
                if (!dataGrid.IsUnBoundRow(bottomIndex) || !dataGrid.IsUnBoundRow(topIndex) || topRowRegion != bottomRowRegion)
                {
                    throw new Exception(string.Format("Given range {0} is not valid", coveredCell));
                }
            }
            dr.isSpannedRow = true;
            dc.isSpannedColumn = true;

            // Reset the covered cell  range by bottom for Frozen rows.                                
            if (dataGrid.FrozenRowsCount > 0 && coveredCell.Top < dataGrid.VisualContainer.FrozenRows)
            {
                CoveredCellInfo newCoveredCell = null;
                dataGrid.CoveredCells.Remove(coveredCell);
                if (coveredCell.Top < dataGrid.VisualContainer.FrozenRows && coveredCell.Bottom >= dataGrid.VisualContainer.FrozenRows)
                {
                    newCoveredCell = new CoveredCellInfo(coveredCell.Left,
                                                        coveredCell.Right,
                                                        coveredCell.Top,
                                                        coveredCell.Bottom < dataGrid.VisualContainer.FrozenRows ? coveredCell.Bottom : dataGrid.VisualContainer.FrozenRows - 1);                        
                }
                else
                    newCoveredCell = coveredCell;

                dataGrid.CoveredCells.Add(newCoveredCell);

                this.UpdateMappedRowIndex(dr, dr.RowIndex);

                dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (newCoveredCell != null && row.RowIndex > newCoveredCell.Bottom && row.RowIndex <= coveredCell.Bottom)
                        dataGrid.MergedCellManager.ResetCoveredRows(row);
                }
                    );
            }

            //  Reset the covered cell range by top for footer rows.                                
            else if (dataGrid.FooterRowsCount > 0 && coveredCell.Bottom >= (this.dataGrid.VisualContainer.RowCount - this.dataGrid.VisualContainer.FooterRows)
                && coveredCell.Bottom < this.dataGrid.VisualContainer.RowCount)
            {
                CoveredCellInfo newCoveredCell = null;
                dataGrid.CoveredCells.Remove(coveredCell);
                if (coveredCell.Top < (dataGrid.VisualContainer.RowCount - dataGrid.VisualContainer.FooterRows))
                {
                    newCoveredCell = new CoveredCellInfo(coveredCell.Left,
                                                        coveredCell.Right,
                                                        coveredCell.Top < (dataGrid.VisualContainer.RowCount - dataGrid.VisualContainer.FooterRows) ?
                                                        (dataGrid.VisualContainer.RowCount - dataGrid.VisualContainer.FooterRows) : coveredCell.Top,
                                                        coveredCell.Bottom);                        
                }
                else
                    newCoveredCell = coveredCell;

                dataGrid.CoveredCells.Add(newCoveredCell);

                this.UpdateMappedRowIndex(dr, dr.RowIndex);

                dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (newCoveredCell != null && row.RowIndex < newCoveredCell.Top && row.RowIndex >= coveredCell.Top)
                        dataGrid.MergedCellManager.ResetCoveredRows(row);
                }
                    );
            }

            // Reset the covered cell range by right for frozen columns                
            if (dataGrid.FrozenColumnCount > 0 && dc.ColumnIndex < dataGrid.VisualContainer.FrozenColumns)
            {
                CoveredCellInfo newCoveredCell = null;
                dataGrid.CoveredCells.Remove(coveredCell);
                if (coveredCell.Left < dataGrid.VisualContainer.FrozenColumns && coveredCell.Right >= dataGrid.VisualContainer.FrozenColumns)
                {
                    newCoveredCell = new CoveredCellInfo(coveredCell.Left,
                                                            coveredCell.Right < dataGrid.VisualContainer.FrozenColumns ? coveredCell.Right : dataGrid.VisualContainer.FrozenColumns - 1,
                                                            coveredCell.Top,
                                                            coveredCell.Bottom);                        
                }
                else
                    newCoveredCell = coveredCell;

                dataGrid.CoveredCells.Add(newCoveredCell);
                    
                this.UpdateMappedRowIndex(dr, dr.RowIndex);

                dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (newCoveredCell != null && row.RowIndex >= coveredCell.Top && row.RowIndex <= coveredCell.Bottom)
                    {
                        row.VisibleColumns.ForEach(column =>
                        {
                            if (column.ColumnIndex > newCoveredCell.Right && column.ColumnIndex <= coveredCell.Right)
                            {
                                column.isSpannedColumn = false;
                                column.ColumnVisibility = Visibility.Visible;
                            }
                        });
                    }
                }
                );
            }
            // Reset the covered cell range by left for frozen columns
            else if (dataGrid.FooterColumnCount > 0 && coveredCell.Right >= (dataGrid.VisualContainer.ColumnCount - dataGrid.VisualContainer.FooterColumns) && coveredCell.Right < dataGrid.VisualContainer.ColumnCount)
            {
                CoveredCellInfo newCoveredCell = null;
                dataGrid.CoveredCells.Remove(coveredCell);
                if ((dataGrid.VisualContainer.ColumnCount - dataGrid.VisualContainer.FooterColumns) >= coveredCell.Left && coveredCell.Right <= dataGrid.VisualContainer.ColumnCount)
                {
                    newCoveredCell = new CoveredCellInfo(coveredCell.Left,
                                                        coveredCell.Right < (dataGrid.VisualContainer.ColumnCount - dataGrid.VisualContainer.FooterColumns) ?
                                                        coveredCell.Right : dataGrid.VisualContainer.ColumnCount - dataGrid.VisualContainer.FooterColumns,
                                                        coveredCell.Top, coveredCell.Bottom);                                                
                }
                else
                    newCoveredCell = coveredCell;

                dataGrid.CoveredCells.Add(newCoveredCell);

                this.UpdateMappedRowIndex(dr, dr.RowIndex);

                dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (newCoveredCell != null && row.RowIndex >= coveredCell.Top && row.RowIndex <= coveredCell.Bottom)
                    {
                        row.VisibleColumns.ForEach(column =>
                        {
                            if (column.ColumnIndex > newCoveredCell.Right && column.ColumnIndex <= coveredCell.Right)
                            {
                                column.isSpannedColumn = false;
                                column.ColumnVisibility = Visibility.Visible;
                            }
                        });
                    }
                }
                );
            }                  
        }

        /// <summary>
        ///  Update the mapped row index while scroll the punch of columns that has covered range has been removed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="actualStartIndex"></param>
        internal void UpdateMappedRowIndex(DataRowBase dr, int actualStartIndex)
        {
            var startRows = this.dataGrid.RowGenerator.Items.FindAll(item => item.RowIndex <= actualStartIndex && item.RowIndex > (this.dataGrid.HeaderLineCount - 1) && item.RowVisibility == Visibility.Visible);

            List<CoveredCellInfo> startRowCoveredRangeCollection = new List<CoveredCellInfo>();
            foreach (DataRowBase mergedRow in startRows)
                startRowCoveredRangeCollection.AddRange(this.dataGrid.CoveredCells.GetCoveredCellsRange(mergedRow));

            if (startRowCoveredRangeCollection.Count == 0) return;

            startRowCoveredRangeCollection.ForEach(coveredRange =>
            {
                if (coveredRange.ContainsRow(dr.RowIndex))
                {
                    var dataRow = this.dataGrid.RowGenerator.Items.Find(item => item.RowVisibility == Visibility.Visible && item.RowIndex == actualStartIndex && dr.RowIndex == actualStartIndex);
                    if (dataRow != null)
                    {
                        var visibleinfo = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(dataRow.RowIndex);
                        if (visibleinfo != null && visibleinfo.IsClippedOrigin)
                        {
                            var nextTopRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowVisibility == Visibility.Visible && item.RowIndex > actualStartIndex && item.RowIndex <= coveredRange.Bottom);
                            if (nextTopRow != null)
                                coveredRange.MappedRowColumnIndex = new RowColumnIndex(nextTopRow.RowIndex, coveredRange.MappedRowColumnIndex.ColumnIndex);
                            else // when we load the bottom as it also clipped.                                
                                coveredRange.MappedRowColumnIndex = new RowColumnIndex(actualStartIndex, coveredRange.MappedRowColumnIndex.ColumnIndex);
                        }
                        else
                            coveredRange.MappedRowColumnIndex = new RowColumnIndex(actualStartIndex, coveredRange.MappedRowColumnIndex.ColumnIndex);
                    }
                }

            });
        }

        /// <summary>
        /// Update the mapped row's column index while scroll the punch of columns that has covered range has been removed.
        /// </summary>
        /// <param name="dr"></param>
        internal void UpdateMappedColumnIndex(DataRowBase dr)
        {
            var coveredCellInfoCollection = this.dataGrid.CoveredCells.GetCoveredCellsRange(dr).OrderBy(coveredRange => coveredRange.Top);
            
            if (coveredCellInfoCollection.Count() == 0) return;

            coveredCellInfoCollection.ForEach(coveredRange =>
            {
                if (coveredRange.MappedRowColumnIndex.RowIndex == dr.RowIndex)
                {
                    foreach (var column in dr.VisibleColumns)
                    {
                        if (column.IsSpannedColumn && coveredRange.ContainsColumn(column.ColumnIndex))
                        {
                            if (column.ColumnVisibility == Visibility.Visible)
                            {
                                var visibleInfo = dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(column.ColumnIndex);
                                if (visibleInfo.IsClippedOrigin)
                                {
                                    coveredRange.MappedRowColumnIndex = new RowColumnIndex(coveredRange.MappedRowColumnIndex.RowIndex, column.ColumnIndex + 1 >= coveredRange.Right ? coveredRange.Right : column.ColumnIndex + 1);
                                    break;
                                }
                                else
                                {
                                    coveredRange.MappedRowColumnIndex = new RowColumnIndex(coveredRange.MappedRowColumnIndex.RowIndex, column.ColumnIndex);
                                    break;
                                }
                            }
                            else
                                continue;
                        }
                    }
                    if (dr.VisibleColumns.All(column => column.ColumnIndex != coveredRange.MappedRowColumnIndex.ColumnIndex))
                        coveredRange.MappedRowColumnIndex = new RowColumnIndex(coveredRange.MappedRowColumnIndex.RowIndex, -1);
                }
            });
        }

        /// <summary>
        /// Ensure that indent and expander, row header columns does not merge.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal bool CanQueryColumn(DataColumnBase column)
        {
            if (column.Renderer != null && column.Renderer != this.dataGrid.CellRenderers["RowHeader"] && !column.IsIndentColumn && !column.IsExpanderColumn)
                return true;

            return false;
        }


        /// <summary>
        /// Resets the merged row and column associated proeprties.
        /// </summary>
        /// <param name="dataRow"></param>
        internal void ResetCoveredRows(DataRowBase dataRow)
        {            
            dataRow.VisibleColumns.ForEach
            (item =>
            {
                if (!item.IsIndentColumn && !item.IsExpanderColumn)
                {                                        
                    if (this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null)
                        item.ColumnVisibility = Visibility.Visible;
                    
                    if (item.IsSpannedColumn && dataRow.IsSpannedRow)                                          
                        item.IsCurrentCell = false;
                    
                    item.IsSelectedColumn = false;
                    item.isSpannedColumn = false;
                }
            }
            );

            dataRow.isSpannedRow = false;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.MergedCellManager"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.MergedCellManager"/> class.
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
    /// Represents the collection of CoveredCellInfo.
    /// </summary>
    public class CoveredCellInfoCollection : List<CoveredCellInfo>
    {
        SfDataGrid dataGrid = null;
        // Parameter Ctr
        public CoveredCellInfoCollection(SfDataGrid grid)
        {
            dataGrid = grid;
        }        

        /// <summary>
        /// Get covered cell info through event.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="gridColumn"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        internal CoveredCellInfo GetCoveredCell(int rowIndex, int columnIndex, GridColumn gridColumn, object record)
        {
            if ((dataGrid.SelectionMode != GridSelectionMode.Single && dataGrid.SelectionMode != GridSelectionMode.None) || dataGrid.SelectionUnit == GridSelectionUnit.Row || dataGrid.SelectionUnit == GridSelectionUnit.Any || dataGrid.NavigationMode == NavigationMode.Row || dataGrid.AllowFrozenGroupHeaders)
                throw new NotSupportedException(string.Format("Merged Cell will not support with {0}, {1}, {2}, AllowFrozenGroupedHeaders {3}", dataGrid.SelectionMode,dataGrid.SelectionUnit, dataGrid.NavigationMode, dataGrid.AllowFrozenGroupHeaders));            

            CoveredCellInfo range = null;
            if (dataGrid.GridModel == null)
                return range;

            GetCoveredCellInfo(out range, rowIndex, columnIndex);


            if (range != null) 
                return range;

            GridQueryCoveredRangeEventArgs e = new GridQueryCoveredRangeEventArgs(new RowColumnIndex(rowIndex, columnIndex), gridColumn, record, dataGrid);
            dataGrid.RaiseQueryCoveredRange(e);
            range = e.Range;
            
            if(range != null && e.Handled)
            {
                if (range.Contains(rowIndex, columnIndex))
                {                                        
                    if(IsInRange(range))
                        throw new Exception(String.Format("Conflict detected when trying to save {0}", range));
                    range.MappedRowColumnIndex = new RowColumnIndex(range.Top,columnIndex);
                    this.dataGrid.coveredCells.Add(range);                    
                    return range;
                }
            }
            return null;
        }

        /// <summary>
        /// Removes the covered range form the DataGrid covered cells collection while scroll vertically
        /// </summary>
        /// <param name="actualStartIndex"></param>
        /// <param name="actualEndIndex"></param>
        internal void RemoveRowRange(int actualStartIndex, int actualEndIndex)
        {
            var startRows = this.dataGrid.RowGenerator.Items.FindAll(item => item.RowIndex < actualStartIndex && item.RowIndex > (this.dataGrid.VisualContainer.FrozenRows - 1));
            var EndRows = this.dataGrid.RowGenerator.Items.FindAll(item => item.RowIndex > actualEndIndex && item.RowIndex <= this.dataGrid.VisualContainer.RowCount);

            var startRowCoveredRangeCollection = new List<CoveredCellInfo>();
            foreach(DataRowBase dr in startRows)            
                startRowCoveredRangeCollection.AddRange(this.GetCoveredCellsRange(dr));

            var endRowCoeveredRangeCollection = new List<CoveredCellInfo>();
            foreach (DataRowBase dr in EndRows)
                endRowCoeveredRangeCollection.AddRange(this.GetCoveredCellsRange(dr));            

            // gets the covered cell range that were out of view in top.
            List<CoveredCellInfo> removedRanges = new List<CoveredCellInfo>();
            if(startRowCoveredRangeCollection.Count != 0)
            {
                foreach(CoveredCellInfo range in startRowCoveredRangeCollection)
                {
                    if(range.MappedRowColumnIndex.RowIndex < actualStartIndex && range.Bottom < actualStartIndex)                    
                       removedRanges.Add(range);
                }
            }            
            
            // gets the covered cell range that were out of view in bottom.
            if(endRowCoeveredRangeCollection.Count != 0)
            {
                foreach(CoveredCellInfo range in endRowCoeveredRangeCollection)
                {
                    if (range.MappedRowColumnIndex.RowIndex > actualEndIndex && range.Top > actualEndIndex)
                        removedRanges.Add(range);
                }
            }

            if (removedRanges.Count == 0)
                return;


            // Remove the ranges and reset the merged rows property.                        
            removedRanges.ForEach(range => 
                {
                    this.dataGrid.RowGenerator.Items.ForEach( item => 
                        {
                            if (item.RowIndex >= range.Top && item.RowIndex <= range.Bottom)
                            {
                                this.dataGrid.MergedCellManager.ResetCoveredRows(item);
                            }
                        });
                    this.dataGrid.CoveredCells.Remove(range);
                });            
        }

        /// <summary>
        /// Removes the covered range form the DataGrid covered cells collection while scroll horizontally
        /// </summary>
        /// <param name="actualStartIndex"></param>
        /// <param name="actualEndIndex"></param>
        internal void RemoveColumnRange(int actualStartIndex, int actualEndIndex)
        {
            List<CoveredCellInfo> removedRanges = new List<CoveredCellInfo>();

            this.dataGrid.CoveredCells.ForEach( range =>
            {
                // while scroll horizontally mapped column index will be set to -1 when the range is hidden fully.
                if (range.MappedRowColumnIndex.ColumnIndex == -1 && ((range.Left < actualStartIndex && range.Right < actualStartIndex)
                    || (range.Left > actualEndIndex && range .Right > actualEndIndex)))
                    removedRanges.Add(range);
            });

            if (removedRanges.Count == 0)
                return;
            
            removedRanges.ForEach(range =>
                {
                    this.dataGrid.CoveredCells.Remove(range);
                });            
        }

        /// <summary>
        /// Returns covered range based on row and column index
        /// </summary>
        /// <param name="range"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        internal void GetCoveredCellInfo(out CoveredCellInfo range, int rowIndex, int columnIndex)
        {
            range = null;
            foreach (CoveredCellInfo coveredCell in this.dataGrid.CoveredCells)
            {
                if (coveredCell == null)
                    continue;
                // returns the covered range for the row and column index
                if (coveredCell.Contains(rowIndex, columnIndex))
                {
                    range = coveredCell;
                    break;
                }             
            }            
        }        

        /// <summary>
        /// Determines whether the range is already present in the CoveredCells collection. 
        /// </summary>
        /// <param name="coveredCell">
        /// Specifies the range to get the presence from CoveredCells collection.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the range is already there in CoveredCells; otherwise, <b>false</b>.
        /// </returns>
        public bool IsInRange(CoveredCellInfo coveredCell)
        {
            foreach(CoveredCellInfo range in this.dataGrid.CoveredCells)
            {                
                // Reference - condition based on Rectangle clip check from MS.
                if (coveredCell.Top <= range.Bottom
                    && coveredCell.Bottom >= range.Top
                    && coveredCell.Right >= range.Left
                    && coveredCell.Left <= range.Right)
                {                    
                    return true;
                }
            }

            return false;
        }

        /// <summary>      
        /// Gets the CoveredCellInfo collection for the specified data row.
        /// </summary>
        /// <param name="dataRow">
        /// Specifies the corresponding data row to get the CoveredCellInfo collection.
        /// </param>
        /// <returns>
        ///  Returns the CoveredCellInfo collection for the specified data row
        /// </returns>        
        public List<CoveredCellInfo> GetCoveredCellsRange(DataRowBase dataRow)
        {            
            return dataGrid.CoveredCells.FindAll(item => item.MappedRowColumnIndex.RowIndex == dataRow.RowIndex || item.ContainsRow(dataRow.RowIndex));
        }

        /// <summary>
        /// Gets the list of DataColumnBase that contains covered cell info for the specified DataRow.
        /// </summary>
        /// <param name="dataRow">
        /// Specifies the corresponding DataRow to get the list of DataColumnBase.
        /// </param>
        /// <returns>
        /// Returns the collection of <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> for the specified DataRow.
        /// </returns>
        public List<DataColumnBase> GetCoveredColumns(DataRowBase dataRow)
        {
            List<DataColumnBase> coveredColumns = new List<DataColumnBase>();
            var coveredCellInfo = GetCoveredCellsRange(dataRow);

            if (coveredCellInfo.Count == 0)
                return coveredColumns;

            coveredCellInfo.ForEach(info =>
                {
                    coveredColumns.Add(dataRow.VisibleColumns.Find(column => info.ContainsColumn(column.ColumnIndex)));
                });
            return coveredColumns;
        }  

        /// <summary>
        /// Gets CoveredCellInfo for the specified DataColumnBase.
        /// </summary>
        /// <param name="dc">
        /// Specifies the corresponding DataColumnBase get the CoveredCellInfo.
        /// </param>
        /// <returns>
        /// Returns the range based on DataColumnBase row and column index.
        /// </returns>
        public CoveredCellInfo GetCoveredCellInfo(DataColumnBase dc)
        {
            return GetCoveredCellInfo(dc.RowIndex, dc.ColumnIndex);
        }

        /// <summary>
        /// Gets CoveredCellInfo for the specified row and column index.
        /// </summary>
        /// <param name="rowIndex">
        /// Specifies the row index.
        /// </param>
        /// <param name="columnIndex">
        /// Specifies the column index.
        /// </param>
        /// <returns>
        /// Returns the range based on row and column index.
        /// </returns>
        public CoveredCellInfo GetCoveredCellInfo(int rowIndex, int columnIndex)
        {
            foreach (CoveredCellInfo coveredCell in this.dataGrid.CoveredCells)
            {
                if (coveredCell.Contains(rowIndex, columnIndex))
                    return coveredCell;
            }

            return null;
        }

        /// <summary>
        /// Throws exception for the row other DataRow/UnBound Row that tries to span
        /// </summary>
        /// <param name="range"></param>
        internal void ContainsRow(CoveredCellInfo range)
        {
            var rows = dataGrid.RowGenerator.Items.Where(row => row.RowIndex >= range.Top && row.RowIndex <= range.Bottom);
            
            if (!(rows.Any()))
                return;            
            
            rows.ForEach(row =>
            {
                if (range.ContainsRow(row.RowIndex) && (row.RowType != RowType.DefaultRow && row.RowType != RowType.UnBoundRow || row.RowType == RowType.AddNewRow))
                    throw new Exception(string.Format("Given range {0} is not valid with the row type {1}", range, row.RowType));    

                // Throw exception when the invalid parent row merges.
                if (this.dataGrid.DetailsViewDefinition.Count > 0 && this.dataGrid.IsInDetailsViewIndex(row.RowIndex + this.dataGrid.DetailsViewDefinition.Count))
                {
                    foreach (var viewDefinition in this.dataGrid.DetailsViewDefinition)
                    {
                        var itemSource = this.dataGrid.DetailsViewManager.GetChildSource(row.RowData, viewDefinition.RelationalColumn);
                        if (DetailsViewHelper.GetChildSourceCount(itemSource) > 0)
                        {
                            throw new Exception(string.Format("Given range {0} is not valid with the details view row", range));  
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Throws exception for the column other dataColumn.
        /// </summary>
        /// <param name="dataRow"></param>
        /// <param name="range"></param>
        internal void ContainsColumn(DataRowBase dataRow, CoveredCellInfo range)
        {
            var columns = dataRow.VisibleColumns.Where(column => column.ColumnIndex >= range.Left && column.ColumnIndex <= range.Right);

            if (!(columns.Any()))
                return;

            columns.ForEach(column =>
            {
                if (range.ContainsColumn(column.ColumnIndex))
                {
                    if(!this.dataGrid.MergedCellManager.CanQueryColumn(column))
                        throw new Exception(String.Format("Given range is not valid {0} with the column type {1}", column, column.IndentColumnType.ToString()));                    
                }

            }
            );
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridQueryCoveredRangeEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridQueryCoveredRangeEventHandler(object sender, GridQueryCoveredRangeEventArgs e);

    /// <summary>    
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event.
    /// </summary>    
    public class GridQueryCoveredRangeEventArgs : GridHandledEventArgs
    {
        CoveredCellInfo range;
        RowColumnIndex cellRowColumnIndex;
        GridColumn column;
        object record;

        /// <overload>
        /// Initialize a new object.
        /// </overload>
        /// <summary>
        /// Initialize a new object.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        public GridQueryCoveredRangeEventArgs(RowColumnIndex cellRowColumnIndex,GridColumn column, object record, object originalSource): base(originalSource)
        {
            this.range = null;
            this.cellRowColumnIndex = cellRowColumnIndex;
            this.column = column;
            this.record = record;
        }

        /// <summary>
        /// Initialize a new object.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="range">A <see cref="CoveredCellInfo"/> that will receive the resulting range for the covered cell.</param>
        public GridQueryCoveredRangeEventArgs(RowColumnIndex cellRowColumnIndex, CoveredCellInfo range, GridColumn column, object record,object originalSource): base(originalSource)
        {
            this.range = range;
            this.cellRowColumnIndex = cellRowColumnIndex;
            this.column = column;
            this.record = record;
        }

        /// <summary>
        /// Gets or sets the RowColumnIndex of cell which triggers this event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the cell which triggers this event.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get { return cellRowColumnIndex; }
        }

        /// <summary>
        /// Gets the data object associated with the row which has the grid cell triggered this event.
        /// </summary>
        public object Record
        {
            get { return record; }
        }


        /// <summary>
        /// Gets the GridColumn of the cell triggers this event.
        /// </summary>
        public GridColumn GridColumn
        {
            get { return column; }
        }

        /// <summary>
        /// Gets or Sets the <see cref="CoveredCellInfo"/> range to merge the cells.
        /// </summary>
        /// <value>        
        /// A <see cref="CoveredCellInfo"/> that will receive the resulting range for the covered cell.
        /// </value>               
        public CoveredCellInfo Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
            }
        }
    }

    /// <summary>
    /// Helper class to provide provision for customer purpose.        
    /// </summary>
    public static class MergedCellHelper
    {
        /// <summary>
        /// Remove the range from CoveredCells.
        /// </summary>
        /// <param name="dataGrid">The SfDataGrid.</param>
        /// <param name="coveredCellRange">Specifies the range to remove from CoveredCells</param>
        public static void RemoveRange(this SfDataGrid dataGrid, CoveredCellInfo coveredCellRange)
        {
            var conatins = dataGrid.CoveredCells.Contains(coveredCellRange);

            if (!conatins)
                return;

            // Find range from the covered cells.
            var resetRange = dataGrid.CoveredCells.Find(range => coveredCellRange.Top >= range.Top && coveredCellRange.Bottom <= range.Bottom && coveredCellRange.Left >= range.Left && coveredCellRange.Right <= range.Right);
            if (resetRange == null)
                return;

            dataGrid.CoveredCells.Remove(resetRange);
            dataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowIndex >= resetRange.Top && row.RowIndex <= resetRange.Bottom)
                {
                    row.RowIndex = -1;
                    dataGrid.MergedCellManager.ResetCoveredRows(row);
                }
            });

            if (!(resetRange.Top == coveredCellRange.Top || resetRange.Bottom == coveredCellRange.Bottom))
                throw new Exception(string.Format("Given range {0} is not valid to remove from the range {1}", coveredCellRange, resetRange));

            if (resetRange.Top == coveredCellRange.Top && resetRange.Bottom == coveredCellRange.Bottom)
                return;

            resetRange = new CoveredCellInfo(resetRange.Left, resetRange.Right, resetRange.Top == coveredCellRange.Top ? coveredCellRange.Bottom : resetRange.Top, resetRange.Bottom == coveredCellRange.Bottom ? coveredCellRange.Top : resetRange.Bottom);
            resetRange.MappedRowColumnIndex = new RowColumnIndex(resetRange.Top, resetRange.Left);
            dataGrid.CoveredCells.Add(resetRange);
        }

        /// <summary>
        /// Add the range to CoveredCells.
        /// </summary>
        /// <param name="dataGrid">The SfDataGrid.</param>
        /// <param name="coveredCellRange">Specifies the range that will added to CoveredCells</param>
        public static void AddRange(this SfDataGrid dataGrid, CoveredCellInfo coveredCellRange)
        {
            if (dataGrid.CoveredCells.IsInRange(coveredCellRange))
                throw new Exception(String.Format("Conflict detected with when trying to save {0}", coveredCellRange));

            coveredCellRange.MappedRowColumnIndex = new RowColumnIndex(coveredCellRange.Top, coveredCellRange.Left);

            dataGrid.CoveredCells.Add(coveredCellRange);
            dataGrid.RowGenerator.Items.ForEach(row =>
            {
                if ((row.RowType == RowType.UnBoundRow || row.RowType == RowType.DefaultRow) && row.RowIndex >= coveredCellRange.Top && row.RowIndex <= coveredCellRange.Bottom)
                    dataGrid.MergedCellManager.InitializeMergedRow(row);
            });

        }

        /// <summary>
        /// Gets the conflicting range from existing CoveredCells collection.
        /// </summary>
        /// <param name="coveredCellInfo">Specifies the range that conflicting with existing covered ranges</param>
        /// <returns>Returns the covered cell range if the given range is conflicting any other existing range of CoveredCells. </returns>        
        public static CoveredCellInfo GetConflictRange(this SfDataGrid dataGrid,CoveredCellInfo coveredCellInfo)
        {
            foreach (CoveredCellInfo coveredCell in dataGrid.CoveredCells)
            {
                // Reference - condition based on Rectangle clip check from MS.
                if (coveredCell.Top <= coveredCellInfo.Bottom
                    && coveredCell.Bottom >= coveredCellInfo.Top
                    && coveredCell.Right >= coveredCellInfo.Left
                    && coveredCell.Left <= coveredCellInfo.Right)
                {
                    return coveredCell;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Decides whether to merge the adjacent parent rows based on child source of row data.
        /// </summary>
        /// <param name="dataGrid">The SfDataGrid.</param>
        /// <param name="rowData">Specifies the data object to find the child items source.</param>
        /// <returns>
        /// Returns <b>true</b> if the DetialsViewDataGrid  has items source; otherwise, <b>false</b>.
        /// </returns>
        public static bool CanMergeNextRows(this SfDataGrid dataGrid, object rowData)
        {
            if (!dataGrid.HideEmptyGridViewDefinition)
                return false;

            foreach (var viewDefinition in dataGrid.DetailsViewDefinition)
            {
                var itemSource = dataGrid.DetailsViewManager.GetChildSource(rowData, viewDefinition.RelationalColumn);
                if (DetailsViewHelper.GetChildSourceCount(itemSource) > 0)
                    return false;
            }
            return true;
        }
    }
    
}
