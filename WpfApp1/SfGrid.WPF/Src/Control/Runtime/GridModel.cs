#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using System.Collections;
using System.Reflection;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.RowFilter;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if WinRT || UNIVERSAL
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.ObjectModel;
using PagedCollectionView = Syncfusion.Data.PagedCollectionView;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using System.Collections.ObjectModel;

#endif
    /// <summary>
    /// Represents a wrapper class for SfDataGrid control to handle the collection and view related operations.
    /// </summary>
    /// <remarks>
    /// GridModel class listens to the collection changes in a SfDataGrid control and responds to them.
    /// It updates the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.View"/> in response to the collection changes.
    /// </remarks>
    [ClassReference(IsReviewed = false)]
    public class GridModel : IDisposable
    {
        #region Fields

        internal SfDataGrid dataGrid;
        private bool isSuspended = false;
        private bool isGroupDescriptionChanged = false;
        private bool isdisposed = false;
        private bool isGroupColumnChanged = false;
        private bool filterSuspend = false;
        internal bool isFilterApplied = false;
        internal GridAddNewRowController addNewRowController;

        #endregion

        #region Properties

        private ICollectionViewAdv View
        {
            get { return this.dataGrid.View; }
        }

        public bool IsInSort { get; internal set; }
        private bool isSortDescriptionChanged { get; set; }
        private bool isSortColumnChanged { get; set; }

        internal bool HasGroup
        {
            get
            {
                if (this.View != null)
                    return this.View.GroupDescriptions.Count > 0;
                return false;
            }
        }

        internal bool FilterSuspend
        {
            get { return filterSuspend; }
            set { filterSuspend = value; }
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> which controls AddNewRow operations in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/>.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        public GridAddNewRowController AddNewRowController 
        {
            get { return addNewRowController; }
            set { addNewRowController = value; }
        }

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridModel(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
            AddNewRowController = new GridAddNewRowController(dataGrid);
        }

        #endregion

        #region WireEvents

        internal void WireEvents()
        {
            if (View != null)
            {
                var sortNotifyCollectionChanged = this.dataGrid.View.SortDescriptions as INotifyCollectionChanged;
                sortNotifyCollectionChanged.CollectionChanged += OnSortDescriptionsChanged;
                (View as CollectionViewAdv).CollectionChanged += OnRecordCollectionChanged;
                (View as CollectionViewAdv).TopLevelGroupCollectionChanged += OnTopLevelGroupCollectionChanged;
                if (this.View.GroupDescriptions != null)
                    this.View.GroupDescriptions.CollectionChanged += OnGroupDescriptionChanged;
                this.View.RecordPropertyChanged += OnRecordPropertyChanged;
                View.SourceCollectionChanged += OnSourceCollectionChanged;
                View.PropertyChanged += OnViewPropertyChanged;

                if (View is Syncfusion.Data.PagedCollectionView)
                {
                    (View as Syncfusion.Data.PagedCollectionView).PageChanged += OnPageChanged;
                }
                View.CurrentChanged += OnViewCurrentChanged;
            }
            if (dataGrid != null)
            {
                if (this.dataGrid.SortColumnDescriptions != null)
                    this.dataGrid.SortColumnDescriptions.CollectionChanged += OnSortColumnsChanged;
                if (this.dataGrid.GroupColumnDescriptions != null)
                    this.dataGrid.GroupColumnDescriptions.CollectionChanged += OnGroupColumnDescriptionsChanged;
                if (this.dataGrid.GroupSummaryRows != null)
                    this.dataGrid.GroupSummaryRows.CollectionChanged += OnSummaryRowsChanged;
                if (this.dataGrid.StackedHeaderRows != null)
                    this.dataGrid.StackedHeaderRows.CollectionChanged += OnStackedHeaderRowsChanged;
                if (this.dataGrid.TableSummaryRows != null)
                    this.dataGrid.TableSummaryRows.CollectionChanged += OnTableSummaryRowsChanged;
                if (this.dataGrid.UnBoundRows != null)
                    this.dataGrid.UnBoundRows.CollectionChanged += OnUnBoundRowsChanged;
                if (this.dataGrid.SortComparers != null)
                    this.dataGrid.SortComparers.CollectionChanged += SortComparers_CollectionChanged;
                if (this.dataGrid.Columns != null)
                    this.dataGrid.Columns.ForEach(x => WireColumnDescriptor(x));
                this.InitializeGridTableSummaryRow();
            }
        }

        /// <summary>
        /// Method which helps to Syncronnize the CurrentItem between view and DataGrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#if !UWP
        void OnViewCurrentChanged(object sender, EventArgs e)
#else
        void OnViewCurrentChanged(object sender, object e)
#endif
        {
            if (this.View.CurrentItem != null)
            {
                var record = this.View.CurrentItem as RecordEntry;
                if (record != null)
                    this.dataGrid.CurrentItem = record.Data;
                else
                    this.dataGrid.CurrentItem = null;
            }
            else
                this.dataGrid.CurrentItem = null;
        }

        private void OnViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemProperties")
            {
                if (this.dataGrid.AutoGenerateColumns && this.dataGrid.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None && this.dataGrid.VisualContainer != null)
                {
                    this.dataGrid.GenerateGridColumns();
                    if (this.dataGrid.Columns.Count != this.dataGrid.VisualContainer.ColumnCount)
                        this.dataGrid.UpdateColumnCount(false);
                    //WPF-20213 - While adding columns in ondemandpaging, we have need to refresh the column sizer once it is created above.
                    this.dataGrid.GridColumnSizer.Refresh();
                    UpdateHeaderCells();
                }
            }
            else if(e.PropertyName == "TableSummary")
            {
                this.UpdateTableSummaries();
            }
        }

        void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.ResetEditItem(args);
            this.dataGrid.SelectionController.HandleCollectionChanged(args, CollectionChangedReason.SourceCollectionChanged);
            if(args.Action == NotifyCollectionChangedAction.Reset)
            {
                //WPF - 33610 ScrollBar value is not set to minimum when changing the ItemsSource
                this.dataGrid.VisualContainer.ResetScrollBars();
#if WPF
                //WPF-33987  while clear the ItemSource , Column sizer is refreshed from RowHeights_LineHiddenChanged method of GridColumnAutoSizer class,
                //previous arrange width must be reset to update the ColumnSizer in Measureoverrride
                this.dataGrid.VisualContainer.previousArrangeWidth = 0.0;
#endif
            }
        }

        private void ResetEditItem(NotifyCollectionChangedEventArgs args)
        {
            if (this.View.IsEditingItem)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (args.OldItems.Contains(View.CurrentEditItem))
                        this.EndEdit(true);
                }
                else if (args.Action == NotifyCollectionChangedAction.Move)
                {
                    if (args.NewItems[0] == View.CurrentEditItem)
                        this.EndEdit();
                }
                else if (args.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (args.NewItems.Contains(View.CurrentEditItem))
                        this.EndEdit();
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                    this.EndEdit();
            }
        }

        internal void EndEdit(bool canResetBinding = false)
        {
            //this.dataGrid.VisualContainer.SuspendManipulationScroll = false;
            var collectionview = this.View as CollectionViewAdv;

            if (collectionview == null || !collectionview.IsEditingItem) return;
            var datarow = this.dataGrid.RowGenerator.Items.FirstOrDefault(x => x.IsEditing && x.RowData.Equals(View.CurrentEditItem));
            this.dataGrid.ValidationHelper.ResetValidations(true);
            if (datarow != null)
            {
                var column = datarow.VisibleColumns.FirstOrDefault(col => col.IsEditing);
                if (column != null && column.Renderer != null && column.Renderer.IsEditable)
                {
                    column.Renderer.EndEdit(column, datarow.RowData, canResetBinding);
                    column.IsEditing = false;
                }
                datarow.IsEditing = false;
            }
            collectionview.EndEdit();
        }

        private void OnPageChanged(object sender, PageChangedEventArgs args)
        {
            this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Paging, null));
            RefreshView();
        }

        /// <summary>
        /// Event Hooks whenever the Sort comparer's Collection changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void SortComparers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.View == null)
                return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var comparer in e.NewItems)
                        this.View.SortComparers.Add(comparer as SortComparer);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var comparer in e.OldItems)
                        this.View.SortComparers.Remove(comparer as SortComparer);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.View.SortComparers.Clear();
                    break;
            }
        }


        /// <summary>
        /// Event Hooks whenever collection changed while sorting is present
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnRecordCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.HasGroup)
            {
                //Refreshing will be handled in OnTopLevelGroupCollectionChanged event
                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                {
                    this.UpdateTableSummaries();
                        return;
                }
            }

            this.ResetEditItem(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    {
                        var rowindex = this.dataGrid.ResolveToRowIndex(e.OldStartingIndex);
	                        UpdateDataRow(rowindex, 1, e.Action);
                        break;
                    }
                case NotifyCollectionChangedAction.Add:
                    {
                        var addedrowindex = this.dataGrid.ResolveToRowIndex(e.NewStartingIndex);                       
                        UpdateDataRow(addedrowindex, 1, e.Action);                                                
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        var args = e as NotifyCollectionChangedEventArgsExt;
                        var isProgrammatic = args == null ? false : args.IsProgrammatic;
                        this.dataGrid.RowGenerator.ForceUpdateBinding = true;
                        RefreshBatchUpdate(e.Action,isProgrammatic);                                                
                        this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();   
                        RefreshView();
                        this.dataGrid.DetailsViewManager.ResetExpandedLevel(this.dataGrid);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        //if (!this.HasGroup)
                        //{
                        var replaceIndex = this.dataGrid.ResolveToRowIndex(e.NewStartingIndex);
                        //Here i have added the Condition to setHidden for the DetailsViewDataRow 
                        //While replcing the parentDataRow when DetailsViewExpander cell is expanded.

                        var datarow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == replaceIndex);
                        if (datarow != null && datarow.IsExpanded && this.dataGrid.DetailsViewManager.HasDetailsView) 
                        {
                            for (int i = 1; i <= dataGrid.DetailsViewDefinition.Count; i++)
                            {
                                int ValueCount;
                                if (!this.dataGrid.VisualContainer.RowHeights.GetHidden(replaceIndex + i, out ValueCount))
                                    this.dataGrid.VisualContainer.RowHeights.SetHidden(replaceIndex + i, replaceIndex + i, true);
                            }
                        } 

                        this.UpdateDataRow(replaceIndex);
                        //if (this.dataGrid.RowGenerator.Items.Any(row => row.RowIndex == replaceIndex))
                        //    UpdateDataRow(replaceIndex, true);
                        //else
                        //    RefreshView();
                        //}
                        //else
                        //    RefreshView();
                    }

                    break;
            }

            if (this.dataGrid is DetailsViewDataGrid && this.dataGrid.NotifyListener != null)
            {
                // WPF-19724 - to reset Extendedwidth
                this.dataGrid.DetailsViewManager.UpdateExtendedWidth();
                this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
            }

            this.dataGrid.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.RecordCollectionChanged);
        }

        private void OnTopLevelGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewStartingIndex < 0 && e.Action == NotifyCollectionChangedAction.Add)
            {
                this.dataGrid.SelectionController.HandleCollectionChanged(e,
                                                                          CollectionChangedReason
                                                                              .RecordCollectionChanged);
                return;
            }
            if (e.OldStartingIndex < 0 && e.Action == NotifyCollectionChangedAction.Remove)
                return;

            this.ResetEditItem(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var rowindex = this.dataGrid.ResolveToRowIndex(e.NewStartingIndex);
                        var count = 0;
                        IList<NodeEntry> recordList = null;
                        var recordStartIndex = 0;
                        var recordCount = 0;
                        //SL-2986:In SILVERLIGHT removed and added items are object types.
                        //So need to check whether e.NewItems[0] is List<NodeEntry> and get the
                        if (e.NewItems[0] is List<NodeEntry>)
                        {
                            recordList = e.NewItems[0] as List<NodeEntry>;
                            count = recordList.Count;
                        }
                        else
                            count = e.NewItems.Count;                           
                        if (this.dataGrid.DetailsViewManager.HasDetailsView)
                        {
                            if (recordList != null)
                              recordCount = recordList.Count(item => item is RecordEntry);
                            else 
                              recordCount = e.NewItems.OfType<RecordEntry>().Count();
                                
                      
                            count += recordCount*(this.dataGrid.DetailsViewDefinition.Count);
                            if(e.NewItems.OfType<RecordEntry>().Any())
                            recordStartIndex = rowindex+e.NewItems.IndexOf(e.NewItems.OfType<RecordEntry>().FirstOrDefault());
                        }
                        RefreshView(rowindex, count, e.Action, recordStartIndex,recordCount);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        var rowindex = this.dataGrid.ResolveToRowIndex(e.OldStartingIndex);
       
                        var count=0;
                        IList<NodeEntry> recordList = null;
                        //SL-2986:In SILVERLIGHT removed and added items are object types.
                        //So need to check whether e.OldItems[0] is List<NodeEntry> and get the
                        if (e.OldItems[0] is List<NodeEntry>)
                        {
                            recordList = e.OldItems[0] as List<NodeEntry>;
                            count = recordList.Count;
                        }
                        else
                            count = e.OldItems.Count;
                        var recordCount = 0;
                        if (this.dataGrid.DetailsViewManager.HasDetailsView)
                        {
                            if (recordList != null)
                                recordCount = recordList.Count(item => item is RecordEntry); 
                            else 
                            recordCount = e.OldItems.OfType<RecordEntry>().Count();
                            var nestedRecordCount = e.OldItems.OfType<NestedRecordEntry>().Count();
                            var expandedRecordCount = (nestedRecordCount/this.dataGrid.DetailsViewDefinition.Count);
                            count += (recordCount - expandedRecordCount)*this.dataGrid.DetailsViewDefinition.Count;
                        }
                        RefreshView(rowindex, count, e.Action);
                        break;
                    }
            }
            // To refresh parent DataGrid
            if (this.dataGrid is DetailsViewDataGrid && this.dataGrid.NotifyListener != null)
            {
                this.dataGrid.DetailsViewManager.UpdateExtendedWidth();
                this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
            }
            this.dataGrid.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.RecordCollectionChanged);
        }

        internal void UnWireEvents(bool unwireGridEvents=true)
        {
            if (View != null)
            {
                if (this.dataGrid.View.SortDescriptions != null)
                {
                    var sortNotifyCollectionChanged = this.dataGrid.View.SortDescriptions as INotifyCollectionChanged;
                    sortNotifyCollectionChanged.CollectionChanged -= OnSortDescriptionsChanged;
                }
                (View as CollectionViewAdv).CollectionChanged -= OnRecordCollectionChanged;
                (View as CollectionViewAdv).TopLevelGroupCollectionChanged -= OnTopLevelGroupCollectionChanged;
                if (this.View.GroupDescriptions != null)
                    this.View.GroupDescriptions.CollectionChanged -= OnGroupDescriptionChanged;
                this.View.RecordPropertyChanged -= OnRecordPropertyChanged;
                View.SourceCollectionChanged -= OnSourceCollectionChanged;
                View.PropertyChanged -= OnViewPropertyChanged;

                if (View is Syncfusion.Data.PagedCollectionView)
                {
                    (View as Syncfusion.Data.PagedCollectionView).PageChanged -= OnPageChanged;
                }

                View.CurrentChanged -= OnViewCurrentChanged;
            }
            if (dataGrid != null && unwireGridEvents)
            {
                if (this.dataGrid.SortColumnDescriptions != null)
                    this.dataGrid.SortColumnDescriptions.CollectionChanged -= OnSortColumnsChanged;
                if (this.dataGrid.GroupColumnDescriptions != null)
                    this.dataGrid.GroupColumnDescriptions.CollectionChanged -= OnGroupColumnDescriptionsChanged;
                if (this.dataGrid.GroupSummaryRows != null)
                    this.dataGrid.GroupSummaryRows.CollectionChanged -= OnSummaryRowsChanged;
                if (this.dataGrid.StackedHeaderRows != null)
                    this.dataGrid.StackedHeaderRows.CollectionChanged -= OnStackedHeaderRowsChanged;
                if (this.dataGrid.TableSummaryRows != null)
                    this.dataGrid.TableSummaryRows.CollectionChanged -= OnTableSummaryRowsChanged;
                if (this.dataGrid.UnBoundRows != null)
                    this.dataGrid.UnBoundRows.CollectionChanged -= OnUnBoundRowsChanged;
                if (this.dataGrid.SortComparers != null)
                    this.dataGrid.SortComparers.CollectionChanged -= SortComparers_CollectionChanged;
            }
        }

        #endregion

        #region Sorting Codes

        #region Sorting Operation

        /// <summary>
        /// All Sorting Operation Done Here Except for TriState Condition
        /// </summary>
        /// <param name="column"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal void MakeSort(GridColumn column)
        {
            if (this.View == null)
                return;

            if (!this.dataGrid.CheckColumnNameinItemProperties(column) && !column.IsUnbound && !this.View.IsDynamicBound &&!this.View.IsXElementBound)
               return;

            if (column.MappingName == null)
            {
                throw new InvalidOperationException("Mapping Name necessary for Sorting,Grouping & Filtering");
            }
            var cancelScroll = false;
            if (this.View != null && column.AllowSorting)
            {
                CommitCurrentRow(this.dataGrid);
                var isInDeferRefresh = dataGrid.View.IsInDeferRefresh;
                IsInSort = true;
                var sortColumName = column.MappingName;
                var allowMultiSort = CheckControlKeyPressed();

                if (dataGrid.SortColumnDescriptions.Count > 0 && allowMultiSort)
                {
                    var sortedColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                    if (sortedColumn == default(SortColumnDescription))
                    {
                        var newSortColumn = new SortColumnDescription { ColumnName = sortColumName, SortDirection = ListSortDirection.Ascending };
                        var sortDescription = new SortDescription(sortColumName, ListSortDirection.Ascending);
                        if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { newSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add, out cancelScroll))
                        {
                            BeginInit();
                            this.dataGrid.SortColumnDescriptions.Add(newSortColumn);
                            this.dataGrid.View.SortDescriptions.Add(sortDescription);
                            this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { newSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add);
                        }
                    }
                    else
                    {
                        if (sortedColumn.SortDirection == ListSortDirection.Descending && this.dataGrid.AllowTriStateSorting)
                        {
                            var removedSortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                            var removedSortDescription = this.dataGrid.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == sortColumName);
                            if (removedSortColumn != null)
                            {
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>(), new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Remove, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Remove(removedSortColumn);
                                    if (removedSortDescription != default(SortDescription))
                                        this.dataGrid.View.SortDescriptions.Remove(removedSortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>(), new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Remove);
                                }
                            }
                        }
                        else
                        {
                            sortedColumn.SortDirection = sortedColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                            var sortDescription = new SortDescription(sortColumName, sortedColumn.SortDirection);
                            var removedSortDescription = this.dataGrid.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == sortColumName);
                            var removedSortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortedColumn.ColumnName);
                            if (this.RaiseSortColumnsChanging(new List<SortColumnDescription> { sortedColumn }, new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Replace, out cancelScroll))
                            {
                                BeginInit();
                                this.dataGrid.SortColumnDescriptions.Remove(removedSortColumn);
                                this.dataGrid.SortColumnDescriptions.Add(sortedColumn);
                                if (removedSortDescription != default(SortDescription))
                                    this.dataGrid.View.SortDescriptions.Remove(removedSortDescription);
                                this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                this.RaiseSortColumnsChanged(new List<SortColumnDescription> { sortedColumn }, new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Replace);
                            }
                        }
                    }
                }
                else
                {
                    var currentSortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                    if (this.dataGrid.SortColumnDescriptions.Count > 0 && currentSortColumn != default(SortColumnDescription))
                    {
                        if (currentSortColumn.SortDirection == ListSortDirection.Descending && this.dataGrid.AllowTriStateSorting)
                        {
                            if (!this.HasGroup)
                            {
                                var sortColumnsClone = this.dataGrid.SortColumnDescriptions.ToList();
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>(), sortColumnsClone, NotifyCollectionChangedAction.Remove, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Clear();
                                    this.dataGrid.View.SortDescriptions.Clear();
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>(), sortColumnsClone, NotifyCollectionChangedAction.Remove);
                                }
                            }
                            else
                            {
                                var remainingSortColumns = this.GetSortColumnsNotInGroupColumns();
                                var remainingSortDescriptions = this.GetSortDescriptionNotInGroupDescription();
                                var removedSortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == currentSortColumn.ColumnName);
                                var removedSortDescription = this.dataGrid.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == currentSortColumn.ColumnName);
                                var sortDescription = new SortDescription();
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription> { currentSortColumn },
                                    new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace, out cancelScroll))
                                {
                                    BeginInit();
                                    if (this.dataGrid.GroupColumnDescriptions.Any(col => col.ColumnName.Equals(currentSortColumn.ColumnName)))
                                    {
                                        this.dataGrid.SortColumnDescriptions.Remove(removedSortColumn);
                                        currentSortColumn.SortDirection = currentSortColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                                        sortDescription.PropertyName = currentSortColumn.ColumnName;
                                        sortDescription.Direction = currentSortColumn.SortDirection;
                                        this.dataGrid.SortColumnDescriptions.Add(currentSortColumn);
                                    }
                                    else
                                    {
                                        foreach (var item in remainingSortColumns)
                                            this.dataGrid.SortColumnDescriptions.Remove(item);
                                    }

                                    //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                                    if (this.View.GroupDescriptions.Any(col => (col as ColumnGroupDescription).PropertyName.Equals(sortDescription.PropertyName)))
                                    {
                                        this.dataGrid.View.SortDescriptions.Remove(removedSortDescription);
                                        this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    }
                                    else
                                    {
                                        foreach (var item in remainingSortDescriptions)
                                        {
                                            if (removedSortDescription != default(SortDescription))
                                                this.View.SortDescriptions.Remove(item);
                                        }
                                    }
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace);
                                }
                            }
                        }
                        else
                        {
                            currentSortColumn.SortDirection = currentSortColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                            var sortDescription = new SortDescription(sortColumName, currentSortColumn.SortDirection);
                            // clear it before adding the current sort column
                            if (!this.HasGroup)
                            {
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Clear();
                                    this.dataGrid.SortColumnDescriptions.Add(currentSortColumn);
                                    this.dataGrid.View.SortDescriptions.Clear();
                                    this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace);
                                }
                            }
                            else
                            {
                                var remainingSortColumns = this.GetSortColumnsNotInGroupColumns();
                                var remainingSortDescriptions = this.GetSortDescriptionNotInGroupDescription();
                                var removedSortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == currentSortColumn.ColumnName);
                                var removedSortDescription = this.dataGrid.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == currentSortColumn.ColumnName);

                                if (removedSortColumn != default(SortColumnDescription))
                                {
                                    if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace, out cancelScroll))
                                    {
                                        BeginInit();
                                        if (this.dataGrid.GroupColumnDescriptions.Any(col => col.ColumnName.Equals(currentSortColumn.ColumnName)))
                                        {
                                            this.dataGrid.SortColumnDescriptions.Remove(removedSortColumn);
                                        }
                                        else
                                        {
                                            foreach (var item in remainingSortColumns)
                                            {
                                                this.dataGrid.SortColumnDescriptions.Remove(item);
                                            }
                                        }
                                        this.dataGrid.SortColumnDescriptions.Add(currentSortColumn);
                                        //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                                        if (this.View.GroupDescriptions.Any(col => (col as ColumnGroupDescription).PropertyName.Equals(sortDescription.PropertyName)))
                                        {
                                            this.View.SortDescriptions.Remove(removedSortDescription);
                                        }
                                        else
                                        {
                                            foreach (var item in remainingSortDescriptions)
                                            {
                                                if (removedSortDescription != default(SortDescription))
                                                    this.View.SortDescriptions.Remove(item);
                                            }
                                        }
                                        this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                        this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var sortColumn = new SortColumnDescription()
                        {
                            ColumnName = sortColumName,
                            SortDirection = ListSortDirection.Ascending
                        };

                        var sortDescription = new SortDescription(sortColumName, ListSortDirection.Ascending);

                        if (!this.HasGroup)
                        {
                            if (this.dataGrid.SortColumnDescriptions.Count > 0)
                            {
                                var sortColumnsClone = this.dataGrid.SortColumnDescriptions.ToList();
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, sortColumnsClone, NotifyCollectionChangedAction.Add, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Clear();
                                    this.dataGrid.SortColumnDescriptions.Add(sortColumn);
                                    this.dataGrid.View.SortDescriptions.Clear();
                                    this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, sortColumnsClone, NotifyCollectionChangedAction.Add);
                                }
                            }
                            else
                            {
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Add(sortColumn);
                                    this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add);
                                }
                            }
                        }
                        else
                        {
                            var sortColumnClone = new List<SortColumnDescription>();
                            this.GetSortColumnsNotInGroupColumns().ForEach(sortColumnClone.Add);
                            if (sortColumnClone.Any())
                            {
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, sortColumnClone, NotifyCollectionChangedAction.Add, out cancelScroll))
                                {
                                    BeginInit();
                                    foreach (var removecolumns in sortColumnClone)
                                    {
                                        this.dataGrid.SortColumnDescriptions.Remove(removecolumns);
                                        var sortDesc = this.dataGrid.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == removecolumns.ColumnName);
                                        if (sortDesc != default(SortDescription))
                                            this.dataGrid.View.SortDescriptions.Remove(sortDesc);
                                    }
                                    this.dataGrid.SortColumnDescriptions.Add(sortColumn);
                                    this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, sortColumnClone, NotifyCollectionChangedAction.Add);
                                }
                            }
                            else
                            {
                                if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add, out cancelScroll))
                                {
                                    BeginInit();
                                    this.dataGrid.SortColumnDescriptions.Add(sortColumn);
                                    this.dataGrid.View.SortDescriptions.Add(sortDescription);
                                    this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add);
                                }
                            }
                        }
                    }
                }
                RefreshAfterSorting(column, cancelScroll, isInDeferRefresh);
                IsInSort = false;
            }
        }

        /// <summary>
        /// Commit the Current editing row (DataRow and AddNewRow)
        /// </summary>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="cancommit">decide to commit the row or not</param>
        internal void CommitCurrentRow(SfDataGrid dataGrid, bool cancommit = true)
        {
            if (dataGrid == null)
                return;

            //Checks if the dataGrid has CurrentCell in its NestedGrid and calls EndEdit.
            var currentDetailsViewGrid = (dataGrid.SelectionController is GridBaseSelectionController) ? (dataGrid.SelectionController as GridBaseSelectionController).GetCurrentDetailsViewGrid(dataGrid) : null;
            if (currentDetailsViewGrid != null)
                CommitCurrentRow(currentDetailsViewGrid);

            var cmanager = dataGrid.SelectionController.CurrentCellManager;
            if (!cmanager.HasCurrentCell)
                return;
            cmanager.EndEdit(cancommit);
            // Selection and state should not be reset in AddNewRow. So false is passed
            if (cmanager.IsAddNewRow && dataGrid.View.IsAddingNew)
                dataGrid.GridModel.AddNewRowController.CommitAddNew(false);
        }

        /// <summary>
        /// If the view is not in DeferRefresh, need to call BeginInit method
        /// </summary>
        private void BeginInit()
        {   
            if (!dataGrid.View.IsInDeferRefresh)
                this.View.BeginInit(false);
        }

        internal void MakeSort(GridColumn column, ListSortDirection sortDirection)
        {
            if (this.View == null)
                return;

            if (!this.dataGrid.CheckColumnNameinItemProperties(column) && !column.IsUnbound && !this.View.IsDynamicBound&&!this.View.IsXElementBound)
                return;

            CommitCurrentRow(this.dataGrid);

            bool cancelScroll = false;
            IsInSort = true;
            var isInDeferRefresh = dataGrid.View.IsInDeferRefresh;  
            var action = NotifyCollectionChangedAction.Add;
            var sortColumnsClone = this.dataGrid.SortColumnDescriptions.ToList();
            
            if (this.dataGrid.GroupColumnDescriptions.Count > 0 && this.dataGrid.SortColumnDescriptions.Count > 0)
            {
                if (dataGrid.GroupColumnDescriptions.Any(col => col.ColumnName.Equals(column.MappingName)))
                {
                    BeginInit();
                    dataGrid.SortColumnDescriptions.Remove(dataGrid.SortColumnDescriptions.FirstOrDefault(col => col.ColumnName.Equals(column.MappingName)));
                    View.SortDescriptions.Remove(View.SortDescriptions.FirstOrDefault(col => col.PropertyName.Equals(column.MappingName)));
                    action = NotifyCollectionChangedAction.Replace;
                }
                else
                {
                    if (dataGrid.SortColumnDescriptions.Any(col => col.ColumnName.Equals(column.MappingName)))
                        action = NotifyCollectionChangedAction.Replace;
                    var sortedColumns = this.GetSortColumnsNotInGroupColumns();
                    BeginInit();
                    sortedColumns.ForEach(x => this.dataGrid.SortColumnDescriptions.Remove(x));
                    var sortDescriptions = this.GetSortDescriptionNotInGroupDescription();
                    sortDescriptions.ForEach(x => this.View.SortDescriptions.Remove(x));
                }
            }
            else
            {
                BeginInit();
                this.dataGrid.SortColumnDescriptions.Clear();
                this.View.SortDescriptions.Clear();
            }
            var newSortColumn = new SortColumnDescription() { ColumnName = column.MappingName, SortDirection = sortDirection };
            var newSortDescription = new SortDescription() { PropertyName = column.MappingName, Direction = sortDirection };
            if (this.RaiseSortColumnsChanging(new List<SortColumnDescription>() { newSortColumn }, sortColumnsClone, action, out cancelScroll))
            {
                BeginInit();
                this.dataGrid.SortColumnDescriptions.Add(newSortColumn);
                this.View.SortDescriptions.Add(newSortDescription);
                this.RaiseSortColumnsChanged(new List<SortColumnDescription>() { newSortColumn }, sortColumnsClone, action);
            }
    
            RefreshAfterSorting(column, cancelScroll, isInDeferRefresh);            
            IsInSort = false;
        }
        /// <summary>
        /// Refresh Header cells, update group drop area item, reset ExpandedDetailsView and call after sorting
        /// </summary>
        /// <param name="column">GridColumn</param>
        /// <param name="cancelScroll">cancelScroll</param>
        /// <param name="isInDeferRefresh">isInDeferRefresh</param>        
        private void RefreshAfterSorting(GridColumn column, bool cancelScroll, bool isInDeferRefresh)
        {
            //WPF-17813 - If the view is in not in defer refresh, no need to call EndInit
            if (!this.View.IsInDeferRefresh)
                return;

            // If the BeginInit is called from MakeSort method, need to call EndInit here
            if (!isInDeferRefresh)
                this.View.EndInit();
            //Reset Expanded detailsview after Endinit the View.
            this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();

            this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Sorting,
        new SortColumnChangedHandle()
        {
            ScrollToCurrentItem = !cancelScroll,
            IsProgrammatic = false
        }));

            if (this.dataGrid.GroupDropArea != null)
                this.dataGrid.GroupDropArea.UpdateGroupDropItemSortIcon(column);

            UpdateHeaderCells();
        }

        #endregion

        #region Sorting Event

        private bool RaiseSortColumnsChanging(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action, out bool cancelScroll)
        {
            return this.dataGrid.RaiseSortColumnsChanging(addedColumns, removedColumns, action, out cancelScroll);
        }

        private void RaiseSortColumnsChanged(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action)
        {
            this.dataGrid.RaiseSortColumnsChanged(addedColumns, removedColumns, action);
        }

        internal void ScrollToCurrentItem()
        {
#if WinRT || UNIVERSAL
            if (this.dataGrid.View.CurrentItem != null && (this.dataGrid.View.CurrentItem as RecordEntry).Data != null)
#else
            if (this.dataGrid.View.CurrentItem != null)
#endif
            {
                var currentRowIndex = this.dataGrid.ResolveToRowIndex((this.dataGrid.View.CurrentItem as RecordEntry).Data);
                if (currentRowIndex < 0)
                    currentRowIndex = 0;
                this.dataGrid.VisualContainer.ScrollRows.ScrollInView(currentRowIndex);
            }
        }

        #endregion

        #region Refresh DataRows
        /// <summary>
        /// Refreshes the DataRow in view when the grid operations is performed.
        /// </summary>
        public void RefreshDataRow()
        {
            if (this.dataGrid.VisualContainer == null)
                return;
            this.dataGrid.RowGenerator.Items.ForEach(ResetRowIndex);
            if (this.dataGrid.CoveredCells.Count != 0)
                this.dataGrid.CoveredCells.Clear();
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        internal void RefreshDataRowForGroup(int fromRowIndex, bool invalidateMeasure)
        {
            if (this.dataGrid.VisualContainer == null) return;

            this.dataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowIndex == fromRowIndex)
                    row.isDataRowDirty = true;
                if (row.RowIndex > fromRowIndex)
                    ResetRowIndex(row);
            });
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }
        /// <summary>
        /// Refreshes the data row from the specified row index.
        /// </summary>
        /// <param name="fromRowIndex">
        /// The 
        /// </param>
        /// <remarks></remarks>
        /// <summary>
        /// Refreshes the data row from the specified row index.
        /// </summary>
        /// <param name="fromRowIndex">
        /// The corresponding row index to refresh.
        /// </param>
        /// <param name="invalidateMeasure">
        /// Indicates to invalidate the measurement.
        /// </param>
        public void RefreshDataRow(int fromRowIndex, bool invalidateMeasure)
        {
            if (this.dataGrid.VisualContainer == null) return;

            this.dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (row.RowIndex >= fromRowIndex)
                        ResetRowIndex(row);
                });
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Reseting row index except header
        /// </summary>
        /// <param name="dr"></param>
        /// <remarks></remarks>
        private void ResetRowIndex(DataRowBase dr)
        {
            //WPF-18585 while reusing existing rows need to reset row index except header row.
            if (dr.RowType != RowType.HeaderRow)
            {
                dr.RowIndex = -1;
                if(this.dataGrid.CanQueryCoveredRange())
                    ResetMergedRow(dr);
            }
        }
        
        /// <summary>
        /// Resetting column index while replacing the column
        /// </summary>
        /// <param name="oldColumnIndex"></param>
        internal void ResetColumnIndex(int oldColumnIndex)
        {
            this.dataGrid.RowGenerator.Items.ForEach
                (item =>
                {
                    item.VisibleColumns.ForEach(col =>
                    {
                        if ((col.ColumnIndex == oldColumnIndex))
                            col.ColumnIndex = -1;
                    });
                });
        }

        private void ResetMergedRow(DataRowBase dr)
        {
            this.dataGrid.MergedCellManager.ResetCoveredRows(dr);
        }

        public void UpdateDataRow(int index)
        {
            if (this.dataGrid.VisualContainer == null)
                return;

            var datarow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == index);
            if (datarow != null)
            {
                datarow.RowIndex = -1;
                //this.dataGrid.VisualContainer.UpdateScrollBars();
                this.dataGrid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        private void UpdateDataRow(int index, int count, NotifyCollectionChangedAction action)
        {
            if (this.dataGrid.VisualContainer == null)
                return;
            var level = count;
            var isQueryCoveredRangeEventWired = this.dataGrid.CanQueryCoveredRange();

            if (this.dataGrid.DetailsViewManager.HasDetailsView)
                level += this.dataGrid.DetailsViewDefinition.Count;

            if (action == NotifyCollectionChangedAction.Add)
            {
                //No need to check whole rows in RowGenerator hence added the Where condition to get the RowData 
                //which is greater than the added index.
                var rowItems = this.dataGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex >= index).OrderBy(item => item.RowIndex);
                rowItems.ForEach(row =>
                {
                    row.SuspendUpdateStyle();
                    bool isRemovableRow = row.RowType != RowType.HeaderRow && row.RowType != RowType.TableSummaryCoveredRow && row.RowType != RowType.TableSummaryRow;

                    if (row.RowType != RowType.HeaderRow)
                    {
                        row.RowIndex += level;
                        row.OnRowIndexChanged();
                        if (isQueryCoveredRangeEventWired)
                            ResetMergedRow(row);
                    }
                        //This code is to set -1 for Caption and Group summary rows. 
                        //This code will not be used, because in grouping OnTopLevelGroupCollectionChanged mehod only excuted.
                        //if (row.RowType != RowType.DefaultRow && isRemovableRow)
                        //    row.RowIndex = -1;

                        row.ResumeUpdateStyle();
                });
            }
            else if (action == NotifyCollectionChangedAction.Remove)
            {
                var datarow =
                        this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == index && row.RowType != RowType.AddNewRow);

                if (datarow != null)
                {
                    var rowIndex = datarow.RowIndex;
                    datarow.RowIndex = -1;

                    if (this.dataGrid.DetailsViewManager.HasDetailsView && datarow.IsExpanded)
                    {
                        for (int i = rowIndex + 1; i < rowIndex + level; i++)
                        {
                            var row = this.dataGrid.RowGenerator.Items.FirstOrDefault(dr => dr.RowIndex == i);
                            if (row != null)
                                row.RowIndex = -1;
                        }
                    }
                }

                //No need to check whole rows in RowGenerator hence added the Where condition to get the RowData 
                //which is greater than the removed index.
                var rowItems = this.dataGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex > index).OrderBy(item => item.RowIndex);
                rowItems.ForEach(row =>
                {
                    row.SuspendUpdateStyle();
                    bool isRemovableRow = row.RowType != RowType.HeaderRow && row.RowType != RowType.TableSummaryCoveredRow && row.RowType != RowType.TableSummaryRow;

                    if (row.RowType != RowType.HeaderRow)
                    {
                        row.RowIndex -= level;
                        row.OnRowIndexChanged();
                        if (isQueryCoveredRangeEventWired)
                            ResetMergedRow(row);
                    }
                        //This code is to set -1 for Caption and Group summary rows. 
                        //This code will not be used, because in grouping OnTopLevelGroupCollectionChanged mehod only excuted.
                        //if (row.RowType != RowType.DefaultRow && isRemovableRow)
                        //    row.RowIndex = -1;

                        row.ResumeUpdateStyle();
                });
            }
            else
                throw new NotImplementedException("UpdateDataRow is not implemented for" + action.ToString());
            UpdateView(index, count, action);
        }

        /// <summary>
        /// Refreshes the view.
        /// </summary>
        /// <param name="needToUpdateRowCount">
        /// Indicates whether the row count is updated while updating view.
        /// </param>
        public void RefreshView(bool needToUpdateRowCount = true)
        {
            if (this.View == null || this.dataGrid.VisualContainer == null) 
                return;

            this.dataGrid.RowGenerator.Items.Where(row => row.RowType == RowType.TableSummaryCoveredRow || row.RowType == RowType.TableSummaryRow).ForEach(dr => UpdateBindingTableSummary(dr));
            this.RefreshDataRow();
            if (needToUpdateRowCount)
                this.dataGrid.UpdateRowCount();
            this.dataGrid.ResetUnBoundRowIndex();
            this.dataGrid.VisualContainer.UpdateScrollBars();
        }

        /// <summary>
        /// Refreshes the view for the specified row index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to refresh view.
        /// </param>
        /// <param name="count">
        /// The corresponding line count.
        /// </param>
        /// <param name="action">
        /// Contains the corresponding collection changed action to update the view.
        /// </param>
        /// <param name="recordIndex">
        /// The corresponding record index to update the view.
        /// </param>
        /// <param name="recordCount">
        /// The corresponding record count.
        /// </param>
        /// <exception cref="System.NotImplementedException">Thrown when the RefreshView method called other than Add or Remove action.
        /// </exception>
        private void RefreshView(int rowIndex, int count, NotifyCollectionChangedAction action, int recordIndex = 0, int recordCount = 0)
        {
            if (this.View == null || this.dataGrid.VisualContainer == null)
                return;

            if (action == NotifyCollectionChangedAction.Add)
                this.dataGrid.InsertLine(rowIndex, count, recordIndex, recordCount);
            else if (action == NotifyCollectionChangedAction.Remove)
                this.dataGrid.RemoveLine(rowIndex, count);
            else
                throw new NotImplementedException("RefreshView not implemented for" + action.ToString());
            if(this.dataGrid.CanQueryRowHeight())
                this.dataGrid.VisualContainer.UpdateRegion();
            this.dataGrid.RowGenerator.Items.Where(row => row.RowType == RowType.TableSummaryCoveredRow || row.RowType == RowType.TableSummaryRow).ForEach(dr => UpdateBindingTableSummary(dr));            
            this.RefreshDataRow();
            //Updates the frozen rows and footer rows count when source collection changed. 
            //It will throws exception when records count is less than the freeze pane count.
            this.dataGrid.UpdateFreezePaneRows();
            this.dataGrid.VisualContainer.UpdateScrollBars();
        }

        private void UpdateView(int rowIndex, int count, NotifyCollectionChangedAction action)
        {
            if (this.View == null || this.dataGrid.VisualContainer == null) 
                return;

            if (action == NotifyCollectionChangedAction.Add)
                this.dataGrid.InsertLine(rowIndex, count);
            else if(action == NotifyCollectionChangedAction.Remove)
			{
                this.dataGrid.RemoveLine(rowIndex, count);
            }
            else
                throw new NotImplementedException("RefreshView not implemented for" + action.ToString());
            if (this.dataGrid.CanQueryRowHeight())
            this.dataGrid.VisualContainer.RowHeightManager.UpdateBody(rowIndex - 1, count, action);
            if (this.dataGrid.CanQueryCoveredRange())
                this.dataGrid.CoveredCells.Clear();
            this.dataGrid.RowGenerator.Items.Where(row => row.RowType == RowType.TableSummaryCoveredRow || row.RowType == RowType.TableSummaryRow).ForEach(dr => UpdateBindingTableSummary(dr));
            //Updates the frozen rows and footer rows count when source collection changed. 
            //It will throws exception when records count is less than the freeze pane count.
            this.dataGrid.UpdateFreezePaneRows();
            this.dataGrid.VisualContainer.UpdateScrollBars();
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        #endregion

        #region Numbering / Ordering the sorted columns

        /// <summary>
        /// Helps to make numbering for sorted column
        /// </summary>
        /// <remarks></remarks>
        internal void ShowSortNumbers()
        {
            var rowGeneratorItems = this.dataGrid.RowGenerator.Items;
            if (rowGeneratorItems == null || !rowGeneratorItems.Any()) return;
            var sortNumber = 1;
            var headerIndex = this.dataGrid.GetHeaderIndex();
            foreach (var item in View.SortDescriptions)
            {
                var sortColumn = rowGeneratorItems[headerIndex].VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == item.PropertyName);
                if (sortColumn != null)
                {
                    var currentCell = sortColumn.ColumnElement as GridHeaderCellControl;
                    //sort number is visible only when  sortdescription count is greater than 1
                    if (currentCell!=null)
                    {
                        if (View.SortDescriptions.Count>1)
                        {
                                currentCell.SortNumber = sortNumber.ToString();
                                currentCell.SortNumberVisibility = Visibility.Visible;
                        }
                        else
                        {
                            currentCell.SortNumberVisibility = Visibility.Collapsed;
                        }
                    }
                }
                sortNumber += 1;
            }
        }

        /// <summary>
        /// Collapse the sort number visibility when ShowSortNumber property sets false
        /// </summary>
        /// <remarks></remarks>
        internal void CollapseSortNumber()
        {
            var rowGeneratorItems = this.dataGrid.RowGenerator.Items;
            if (rowGeneratorItems == null) return;
            var headerIndex = this.dataGrid.GetHeaderIndex();
            foreach (var visibleColumn in rowGeneratorItems[headerIndex].VisibleColumns)
            {
                var visibleCell = visibleColumn.ColumnElement as GridHeaderCellControl;
                if (visibleCell != null)
                    visibleCell.SortNumberVisibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Changing the sort_icon Visibility

        /// <summary>
        /// Change SortIcon Visibility On Collection changed
        /// </summary>
        /// <param name="column"></param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks>Icon Visibility Changed according the collection change</remarks>
        internal void ChangeSortIconVisibility(GridColumn column, object oldItem, NotifyCollectionChangedAction action)
        {
            if (this.dataGrid == null) return;
            if (this.dataGrid.RowGenerator.Items.Count <= 0) return;
            var headerIndex = this.dataGrid.GetHeaderIndex();
            var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == headerIndex);
            if (dataRow == null)
            {
                if (this.dataGrid.GroupDropArea != null && column != null)
                    this.dataGrid.GroupDropArea.UpdateGroupDropItemSortIcon(column);

                return;
            }
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                   {
                        var sortDescription =
                            View.SortDescriptions.FirstOrDefault(item => item.PropertyName == column.MappingName);
                        var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == sortDescription.PropertyName);
                        if (dataColumn != null)
                        {
                            var currentCell = dataColumn.ColumnElement as GridHeaderCellControl;
                            if (currentCell != null)
                            {
                                currentCell.SortDirection = sortDescription.Direction;
                                if (this.View.SortDescriptions.Count > 1)
                                {
                                    var sortNumber = this.View.SortDescriptions.IndexOf(sortDescription) + 1;
                                    currentCell.SortNumber = sortNumber.ToString();
                                }
                                else
                                    currentCell.SortNumberVisibility = Visibility.Collapsed;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        string columnName;
                        if (oldItem is SortColumnDescription)
                            columnName = (oldItem as SortColumnDescription).ColumnName;
                        else
                            columnName = ((SortDescription)oldItem).PropertyName;
                        var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == columnName);
                        if (dataColumn != null)
                        {
                            var currentCell = dataColumn.ColumnElement as GridHeaderCellControl;
                            if (currentCell != null)
                            {
                                currentCell.SortDirection = null;
                                currentCell.SortNumberVisibility = Visibility.Collapsed;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var sortDescription in View.SortDescriptions)
                    {
                        var dataColumn = dataRow.VisibleColumns.SingleOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == sortDescription.PropertyName);
                        if (dataColumn != null)
                        {
                            var currentCell = dataColumn.ColumnElement as GridHeaderCellControl;
                            if (currentCell != null)
                            {
                                currentCell.SortDirection = null;
                                currentCell.SortNumberVisibility = Visibility.Collapsed;
                            }
                        }
                    }
                    foreach (var sortColumn in this.dataGrid.SortColumnDescriptions)
                    {
                        var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == sortColumn.ColumnName);
                        if (dataColumn != null)
                        {
                            var currentCell = dataColumn.ColumnElement as GridHeaderCellControl;
                            if (currentCell != null)
                            {
                                currentCell.SortDirection = null;
                                currentCell.SortNumberVisibility = Visibility.Collapsed;
                            }
                        }
                    }
                    break;
            }
            if (this.dataGrid.GroupDropArea != null && column != null)
                this.dataGrid.GroupDropArea.UpdateGroupDropItemSortIcon(column);
        }

        #endregion

        #region CollectionChanged Events

        private void OnSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GridColumn column = null;          
           
            if (IsInSort || isSortColumnChanged)
            {
                return;
            }
            bool isIndeferRefresh = false;
            // WPF-18235 - Need to refresh view while applying sorting if the grid has grouping.                
            isIndeferRefresh = this.dataGrid.View.IsInDeferRefresh;
            BeginInit();

            isSortDescriptionChanged = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SortDescription sortItem in e.NewItems)
                    {
                        if (this.View.SortDescriptions.Count(desc => desc.PropertyName == sortItem.PropertyName) > 1)
                        {
                            throw new InvalidOperationException("SortDescription already exist in View.SortDescriptions");
                        }
                        var hasSortItem = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortItem.PropertyName) != null;
                        if (!hasSortItem)
                        {
                            this.dataGrid.SortColumnDescriptions.Insert(View.SortDescriptions.IndexOf(sortItem), new SortColumnDescription() { ColumnName = sortItem.PropertyName, SortDirection = sortItem.Direction });
                            column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == sortItem.PropertyName);
                            ChangeSortIconVisibility(column, null, e.Action);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SortDescription sortItem in e.OldItems)
                    {
                        var modelSortItem = this.dataGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortItem.PropertyName);
                        if (modelSortItem != null)
                        {
                            this.dataGrid.SortColumnDescriptions.Remove(modelSortItem);
                            column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == sortItem.PropertyName);
                            if (e.OldItems != null)
                                ChangeSortIconVisibility(column, e.OldItems[0], e.Action);
                            else
                                ChangeSortIconVisibility(column, null, e.Action);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ChangeSortIconVisibility(column, null, e.Action);
                    this.dataGrid.SortColumnDescriptions.Clear();
                    break;
            }

            isSortDescriptionChanged = false;
            // WPF-18235 - Need to refresh view while applying sorting if the grid has grouping.           
            if (this.dataGrid.View.IsInDeferRefresh && !isIndeferRefresh)
                this.dataGrid.View.EndInit();

            //when removing the grouping/sorting, sort number has been updated 
            if (this.dataGrid.ShowSortNumbers && this.dataGrid.View.SortDescriptions.Any())
                ShowSortNumbers();

            if (this.dataGrid.View.IsInDeferRefresh)
                return;
        
            // Nested lines and cached row index needs to be reset while refreshing by data operations.
            this.dataGrid.DetailsViewManager.ResetExpandedDetailsView(); 

            if (!isSuspended)
            {
                this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Sorting,
                 new SortColumnChangedHandle()
                 {
                     ScrollToCurrentItem = true
                 }));
            }           
        }

        internal void OnSortColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.dataGrid == null) return;

            if (this.dataGrid.NotifyListener != null && !this.dataGrid.suspendNotification)
            {
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.SortColumnDescriptions, e, datagrid => datagrid.SortColumnDescriptions, this.dataGrid, typeof(SortColumnDescription));
            }

            GridColumn column = null;
            if (IsInSort || isSortDescriptionChanged || View == null)
            {
                return;
            }
            bool isIndeferRefresh = false;
            // WPF-18235 - Need to refresh view while applying sorting if the grid has grouping.           
            isIndeferRefresh = this.dataGrid.View.IsInDeferRefresh;
            BeginInit();
            isSortColumnChanged = true;

            CommitCurrentRow(this.dataGrid);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SortColumnDescription newItem in e.NewItems)
                    {
                        if (this.dataGrid.SortColumnDescriptions.Count(col => col.ColumnName == newItem.ColumnName) > 1)
                        {
                            throw new InvalidOperationException("SortColumnDescription already exist in DataGrid.SortColumnDescriptions");
                        }
                        var desc = this.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == newItem.ColumnName);
                        if (desc != default(SortDescription))
                            return;
                        var index = this.dataGrid.SortColumnDescriptions.IndexOf(newItem);
                        this.View.SortDescriptions.Insert(index, new SortDescription { PropertyName = newItem.ColumnName, Direction = newItem.SortDirection });
                        column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == newItem.ColumnName);
                        ChangeSortIconVisibility(column, null, e.Action);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SortColumnDescription item in e.OldItems)
                    {
                        var sortDesc = this.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == item.ColumnName);
                        if (sortDesc != default(SortDescription))
                        {
                            this.View.SortDescriptions.Remove(sortDesc);
                        }
                        column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == item.ColumnName);
                        if (e.OldItems != null)
                            ChangeSortIconVisibility(column, e.OldItems[0], e.Action);
                        else
                            ChangeSortIconVisibility(column, null, e.Action);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ChangeSortIconVisibility(column, null, e.Action);
                    this.View.SortDescriptions.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldItem = e.OldItems[0] as SortColumnDescription;
                    var replaceItem = e.NewItems[0] as SortColumnDescription;
                    if (this.View.SortDescriptions.Count(col => col.PropertyName == replaceItem.ColumnName) >= 1)
                    {
                        return;
                    }
                    var sortitemDesc = this.View.SortDescriptions.FirstOrDefault(s => s.PropertyName == oldItem.ColumnName);
                    if (sortitemDesc != default(SortDescription))
                    {
                        this.View.SortDescriptions[e.OldStartingIndex] = new SortDescription { PropertyName = replaceItem.ColumnName, Direction = replaceItem.SortDirection };
                        column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == oldItem.ColumnName);
                        ChangeSortIconVisibility(column, oldItem, NotifyCollectionChangedAction.Remove);
                        column = this.dataGrid.Columns.FirstOrDefault(x => x.MappingName == replaceItem.ColumnName);
                        ChangeSortIconVisibility(column, null, NotifyCollectionChangedAction.Add);
                    }
                    break;
            }
           
            isSortColumnChanged = false;
            // WPF-18235 - Need to refresh view while applying sorting if the grid has grouping.           
            if (this.dataGrid.View.IsInDeferRefresh && !isIndeferRefresh)
                this.dataGrid.View.EndInit();

            if (this.dataGrid.View.IsInDeferRefresh)
                return;

            // Nested lines and cached row index needs to be reset while refreshing by data operations.
            this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();  

            if (this.dataGrid.ShowSortNumbers && this.dataGrid.SortColumnDescriptions.Count > 1)
                ShowSortNumbers();

            //WPF-18782 The sort column change operation has been called after the EndInit operation.
            this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Sorting,
           new SortColumnChangedHandle()
           {
               ScrollToCurrentItem = true,
               IsProgrammatic = true,
           }));
		    //WPF-24641 - Changing SortColumns progamatically, Need to refresh Headers
            UpdateHeaderCells();
        }

        #endregion

        #endregion

        #region Grouping Codes

        #region Internal Methods

        /// <summary>
        /// Method which is helps to made the group by passing the column name
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="converter"></param>
        /// <param name="comparer"></param>
        /// <remarks></remarks>
        internal void GroupBy(string columnName, IValueConverter converter, IComparer<object> comparer)
        {
            if (this.View == null)
                return;

            var gridColumn = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);

            if (!this.dataGrid.CheckColumnNameinItemProperties(gridColumn) && !gridColumn.IsUnbound && !this.View.IsDynamicBound && !this.View.IsXElementBound && !gridColumn.UseBindingValue)
                return;

            if (this.CheckForExistingGroupDescription(columnName))
                return;

            CommitCurrentRow(this.dataGrid);

            if (this.dataGrid.VisualContainer == null)
            {
                //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
                this.dataGrid.GroupColumnDescriptions.Add(new GroupColumnDescription() { ColumnName = columnName, Converter = converter, Comparer = comparer });
                return;
            }

            this.Suspend();

            if (this.dataGrid.GroupColumnDescriptions.All(col => col.ColumnName != columnName))
            {
                this.View.BeginInit(false);
                //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
                var groupcolumndesc = new GroupColumnDescription() { ColumnName = columnName, Converter = converter, Comparer = comparer };
                this.dataGrid.GroupColumnDescriptions.Add(groupcolumndesc);
                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                this.View.GroupDescriptions.Add(new ColumnGroupDescription(groupcolumndesc.ColumnName, groupcolumndesc.Converter, groupcolumndesc.Comparer) { SortGroupRecords = groupcolumndesc.SortGroupRecords });                
                var sortColumn = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == columnName);
                if (sortColumn == default(SortDescription))
                    this.View.SortDescriptions.Add(new SortDescription(columnName, ListSortDirection.Ascending));

                this.AddGroupDropItem(columnName);
                
                this.View.EndInit();                

                RefreshBatchUpdate(NotifyCollectionChangedAction.Add);
                SetIsHidden(NotifyCollectionChangedAction.Add, null, columnName);
            }
            this.Resume();
        }

        /// <summary>
        /// Add the initial Groupdescription to GropDropArea
        /// </summary>
        /// <remarks></remarks>
        internal void InitializeGrouping()
        {
            if (this.View == null) return;
            this.View.GroupDescriptions.ForEach(desc =>
                {
                    //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                    var gridcolumn =
                        this.dataGrid.Columns.FirstOrDefault(
                            col => col.MappingName == (desc as ColumnGroupDescription).PropertyName);
                    if (gridcolumn != null && !this.dataGrid.ShowColumnWhenGrouped)
                        gridcolumn.IsHidden = true;
                });

            if (this.dataGrid.GroupDropArea != null)
            {
                if (this.View == null) return;
                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                this.View.GroupDescriptions.ForEach(
                    desc => this.AddGroupDropItem((desc as ColumnGroupDescription).PropertyName));
            }
        }

        /// <summary>
        /// Method which is helps to made the group by passing the column name
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="converter"></param>
        /// <param name="comparer"></param>
        /// <remarks></remarks>
        internal void GroupBy(string columnName, int insertAt, IValueConverter converter, IComparer<object> comparer)
        {
            if (this.View == null)
                return;

            var gridColumn = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);

            if (!this.dataGrid.CheckColumnNameinItemProperties(gridColumn) && !gridColumn.IsUnbound &&!this.View.IsDynamicBound&&!this.View.IsXElementBound)
                return;

            if (this.CheckForExistingGroupDescription(columnName))
                return;

            CommitCurrentRow(this.dataGrid);

            if (this.dataGrid.VisualContainer == null)
            {
                //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
                this.dataGrid.GroupColumnDescriptions.Insert(insertAt, new GroupColumnDescription() { ColumnName = columnName, Converter = converter, Comparer = comparer });
                return;
            }

            this.Suspend();

            if (this.dataGrid.GroupColumnDescriptions.All(col => col.ColumnName != columnName))
            {
                this.View.BeginInit(false);
                //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
                var groupcolumndesc = new GroupColumnDescription() { ColumnName = columnName, Converter = converter, Comparer = comparer };
                this.dataGrid.GroupColumnDescriptions.Insert(insertAt, groupcolumndesc);
                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                this.View.GroupDescriptions.Insert(insertAt, new ColumnGroupDescription(groupcolumndesc.ColumnName, groupcolumndesc.Converter, groupcolumndesc.Comparer) { SortGroupRecords = groupcolumndesc.SortGroupRecords});
                var sortColumn = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == columnName);
                if (sortColumn == default(SortDescription))
                    this.View.SortDescriptions.Add(new SortDescription(columnName, ListSortDirection.Ascending));

                this.AddGroupDropItem(columnName);
                this.View.EndInit();                

                RefreshBatchUpdate(NotifyCollectionChangedAction.Add);
                SetIsHidden(NotifyCollectionChangedAction.Add, null, columnName);
            }
            this.Resume();
        }

        /// <summary>
        /// Method which is helps to remove the grouping by passing the corresponding column name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <remarks></remarks>
        internal void RemoveGroup(string columnName)
        {
            this.Suspend();
            if (this.View.GroupDescriptions.Count > 0 && this.dataGrid.GroupColumnDescriptions.Any(col => col.ColumnName == columnName))
            {
                var groupColumn = this.dataGrid.GroupColumnDescriptions.FirstOrDefault(col => col.ColumnName == columnName);
                this.dataGrid.GroupColumnDescriptions.Remove(groupColumn);
                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                var groupDesc = this.View.GroupDescriptions.FirstOrDefault(desc => ((ColumnGroupDescription)desc).PropertyName == columnName);
                var sortDesc = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == columnName);

                this.View.BeginInit(false);
                          
                // When cell is in edit mode, EndEdit should not be called. So false is passed.
                CommitCurrentRow(this.dataGrid, false);

                this.View.GroupDescriptions.Remove(groupDesc);
                this.View.SortDescriptions.Remove(sortDesc);

                this.RemoveGroupDropItem(columnName);
                this.View.EndInit();                
                RefreshBatchUpdate(NotifyCollectionChangedAction.Remove);
                SetIsHidden(NotifyCollectionChangedAction.Remove, null, columnName);
            }
            this.Resume();
        }

        /// <summary>
        /// Method which helps to expand all the groups in Specific level
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="groupLevel"></param>
        /// <remarks></remarks>
        internal void ExpandGroupsAtLevel(List<Group> groups, int groupLevel)
        {
            foreach (var group in groups)
            {
                if (group.Level <= groupLevel)
                {
                    group.IsExpanded = true;
                }
                if (!group.IsBottomLevel)
                    this.ExpandGroupsAtLevel(group.Groups, groupLevel);
            }
        }

        /// <summary>
        /// Method which helps to collapse all the groups in Specific level
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="groupLevel"></param>
        /// <remarks></remarks>
        internal void CollapseGroupsAtLevel(List<Group> groups, int groupLevel)
        {
            foreach (var group in groups)
            {
                if (group.Level == groupLevel)
                {
                    group.IsExpanded = false;
                }
                if (!group.IsBottomLevel)
                    this.CollapseGroupsAtLevel(group.Groups, groupLevel);
            }
        }

        /// <summary>
        /// Method which helps to expand the specific group by paasing the corresponding group
        /// </summary>
        /// <param name="group"></param>
        /// <returns>Returns whether this expanding action is succeeded or failed</returns>
        internal bool ExpandGroup(Group group)
        {
            if (group == null)
                return group.IsExpanded;
            //WRT-4503 RaiseGroupExpandingEvent will be called only if group is not expanded already
            if (!group.IsExpanded)
            {
                if (!this.dataGrid.RaiseGroupExpandingEvent(new GroupChangingEventArgs(this.dataGrid) { Group = group }))
                {
                    var groupModel = this.View.TopLevelGroup;
                    var insertedItems = groupModel.ExpandGroup(group);
                    var startIndex = this.dataGrid.ResolveStartIndexOfGroup(group) + 1;
                    var lineSizeCollection = this.dataGrid.VisualContainer.RowHeights as LineSizeCollection;
                    lineSizeCollection.SuspendUpdates();
                    this.dataGrid.VisualContainer.InsertRows(startIndex, insertedItems);
                    if (this.dataGrid.CanQueryRowHeight())
                    {
                        this.dataGrid.VisualContainer.RowHeightManager.UpdateBody(startIndex - 1, insertedItems, NotifyCollectionChangedAction.Add);
                        this.dataGrid.VisualContainer.RowHeights.SetRange(startIndex, this.dataGrid.VisualContainer.RowCount - 1, this.dataGrid.RowHeight);
                    }
                    if (this.dataGrid.CanQueryCoveredRange())
                        this.dataGrid.CoveredCells.RemoveRowRange(0, startIndex - 1);

                    lineSizeCollection.ResumeUpdates();

                    if (this.dataGrid.DetailsViewManager.HasDetailsView)
                    {
                        this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                            {
                                if (row.CatchedRowIndex >= startIndex + insertedItems)
                                {
                                    if (row.CatchedRowIndex != -1)
                                        row.CatchedRowIndex += insertedItems;
                                }
                            });
                    }

                    this.dataGrid.SelectionController.HandleGroupExpandCollapse(startIndex, insertedItems, true);
                    this.dataGrid.VisualContainer.ScrollRows.MarkDirty();

                    if (this.dataGrid.DetailsViewManager.HasDetailsView)
                        RestExpandedState(group);

                    this.dataGrid.UpdateFreezePaneRows();
                    this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                    //WPF-32955 - We have to refresh the ParentDataGrid when Expand the Group in DetailsViewDataGrid.
                    if (this.dataGrid.NotifyListener != null)
                        this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
                    this.dataGrid.RaiseGroupExpandedEvent(new GroupChangedEventArgs(this.dataGrid) { Group = group });
                }
                return group.IsExpanded;
            }
            return group.IsExpanded;
        }
        
        private void RestExpandedState(Group group)
        {
            if (!group.IsBottomLevel)
            {
                foreach (var childGroup in group.Groups)
                {
                    RestExpandedState(childGroup);
                }
            }
            else
            {
                if (group.IsExpanded)
                {
                    var lineSizeCollection = this.dataGrid.VisualContainer.RowHeights as LineSizeCollection;
                    int startIdx = this.dataGrid.ResolveStartIndexOfGroup(group) + 1;
                    var endIndex = startIdx + group.GetRecordCount();
                    lineSizeCollection.SetHiddenIntervalWithState(startIdx, endIndex, this.dataGrid.DetailsViewManager.GetHiddenPattern());
                    foreach (var record in group.Records)
                    {
                        if (record.IsExpanded)
                        {
                            foreach (var viewDefinition in this.dataGrid.DetailsViewDefinition)
                            {
                                startIdx++;
                                lineSizeCollection.SetHidden(startIdx, startIdx, false);
                            }
                            //Skipping record and move to next DetailsView
                            startIdx += 1;
                        }
                        else
                        {
                            //Skipping record and move to next DetailsView
                            startIdx += (this.dataGrid.DetailsViewDefinition.Count + 1);
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Method which helps to collapse the specific group by paasing the corresponding group
        /// </summary>
        /// <param name="group"></param>
       /// <returns> Returns whether this collapsing action is succeeded or failed</returns>
        internal bool CollapseGroup(Group group)
        {
            if (group == null)
                return !group.IsExpanded;
            //WRT-4503 RaiseGroupCollapsingEvent will be called only if group is expanded already
            if (group.IsExpanded)
            {
                if (!this.dataGrid.RaiseGroupCollapsingEvent(new GroupChangingEventArgs(this.dataGrid) { Group = group }))
                {
                    var groupModel = this.View.TopLevelGroup;
                    int collapsedItems = 0;
                    groupModel.ComputeCount(group, ref collapsedItems);
                    var startIndex = this.dataGrid.ResolveStartIndexOfGroup(group) + 1;
                    groupModel.CollapseGroup(group);
                    var lineSizeCollection = this.dataGrid.VisualContainer.RowHeights as LineSizeCollection;
                    lineSizeCollection.SuspendUpdates();
                    this.dataGrid.VisualContainer.RemoveRows(startIndex, collapsedItems);
                    if (this.dataGrid.CanQueryRowHeight())
                    {
                        this.dataGrid.VisualContainer.RowHeightManager.UpdateBody(this.dataGrid.ResolveStartIndexOfGroup(group) - 1, collapsedItems, NotifyCollectionChangedAction.Remove);
                        //WPF-19668 already  removed row in visual continer so need to decrease one in index value
                        this.dataGrid.VisualContainer.RowHeights.SetRange(startIndex - 1, this.dataGrid.VisualContainer.RowCount - 1, this.dataGrid.RowHeight);
                    }

                    if (this.dataGrid.CanQueryCoveredRange())
                        this.dataGrid.CoveredCells.RemoveRowRange(0, startIndex - 1);

                    lineSizeCollection.ResumeUpdates();

                    if (this.dataGrid.DetailsViewManager.HasDetailsView)
                    {
                        this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                        {
                            if (row.CatchedRowIndex > startIndex + collapsedItems)
                            {
                                if (row.CatchedRowIndex != -1)
                                    row.CatchedRowIndex -= collapsedItems;
                            }
                        });
                    }

                    this.dataGrid.UpdateFreezePaneRows();
                    this.dataGrid.SelectionController.HandleGroupExpandCollapse(startIndex, collapsedItems, false);
                    this.dataGrid.VisualContainer.ScrollRows.MarkDirty();
                    this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                    //WPF-32955 - We have to refresh the ParentDataGrid when Collapsed the Group in DetailsViewDataGrid.
                    if (this.dataGrid.NotifyListener != null)
                        this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
                    this.dataGrid.RaiseGroupCollapsedEvent(new GroupChangedEventArgs(this.dataGrid) { Group = group });
                }
                return !group.IsExpanded;
            }
            return !group.IsExpanded;
        }

        /// <summary>
        /// Method which helps to reset the column count and column indexes when group added or removed
        /// </summary>
        /// <remarks></remarks>
        internal void ResetColumns(bool isBatchUpdate = false)
        {
            // WPF-18333  While clearing the columns,no need to reset columns if GroupColumnDescriptions is changed.
            // Below code is added to skip this. but freeze pane columns should be updated
            if (this.dataGrid.isInColumnReset)
            {
                this.dataGrid.UpdateFreezePaneColumns();
                return;
            }
            var columnCount = this.dataGrid.Columns.Count;
            columnCount += this.dataGrid.ShowRowHeader ? 1 : 0;
            columnCount += this.dataGrid.DetailsViewManager.HasDetailsView ? 1 : 0;

            if (this.View!=null && this.View.TopLevelGroup != null)
                columnCount += this.View.TopLevelGroup.GetMaxLevel();

            var increasedIndexValue = columnCount - this.dataGrid.VisualContainer.ColumnCount;
            // WPF-32367 - Issue due to unwanted columns update.
            if (increasedIndexValue != 0)
            {
                var insertAtColumnIndex = isBatchUpdate ?
                    (this.dataGrid.showRowHeader ? 1 : 0) :
                    (this.dataGrid.View.GroupDescriptions.Count > 0 ? this.dataGrid.View.GroupDescriptions.Count - 1 : 0) + (this.dataGrid.showRowHeader ? 1 : 0);

                var startColindex = this.dataGrid.showRowHeader ? 1 : 0;
                var indentcolcount = this.View.GroupDescriptions.Count;

                this.dataGrid.RowGenerator.Items.ForEach(row =>
                    {
                        if ((row.RowType != RowType.TableSummaryCoveredRow && row.RowType != RowType.TableSummaryRow) && (row.RowType == RowType.HeaderRow && row.RowIndex < this.dataGrid.GetHeaderIndex()))
                        {
                            this.dataGrid.RowGenerator.UpdateStackedheaderCoveredRow(row as SpannedDataRow, increasedIndexValue);
                        }
                    });

                (this.dataGrid.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();

                if (increasedIndexValue > 0)
                {
                    this.dataGrid.VisualContainer.InsertColumns(insertAtColumnIndex, increasedIndexValue);

                    if (isBatchUpdate)
                    {
                        if (indentcolcount != 0)
                        {
                            for (int i = startColindex; i <= indentcolcount; i++)
                            {
                                if (i < this.dataGrid.VisualContainer.ColumnCount)
                                    this.dataGrid.VisualContainer.ColumnWidths[i] = dataGrid.IndentColumnWidth;
                            }
                        }
                    }
                    else
                        this.dataGrid.VisualContainer.ColumnWidths[(this.dataGrid.View.GroupDescriptions.Count > 0 ? this.dataGrid.View.GroupDescriptions.Count - 1 : 0) + (this.dataGrid.showRowHeader ? 1 : 0)] = dataGrid.IndentColumnWidth;
                }
                else
                {
                    var indentcolIndex = this.dataGrid.showRowHeader ? this.dataGrid.View.GroupDescriptions.Count + 1 : this.dataGrid.View.GroupDescriptions.Count;
                    this.dataGrid.VisualContainer.RemoveColumns(indentcolIndex, Math.Abs(increasedIndexValue));
                    if (dataGrid.DetailsViewManager.HasDetailsView)
                        indentcolcount++;

                    if (indentcolcount != 0)
                    {
                        for (int i = startColindex; i <= indentcolcount; i++)
                        {
                            if (i < this.dataGrid.VisualContainer.ColumnCount)
                            {
                                if (dataGrid.DetailsViewManager.HasDetailsView && i == indentcolcount)
                                    this.dataGrid.VisualContainer.ColumnWidths[i] = dataGrid.ExpanderColumnWidth;
                                else
                                    this.dataGrid.VisualContainer.ColumnWidths[i] = dataGrid.IndentColumnWidth;
                            }
                        }
                    }
                }
            }
            // Need to reset the hidden state to calculate the column sizer correctly after refresh view when RowsLineSizeColelctionChanged.
            (this.dataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResetHiddenState();

            this.dataGrid.UpdateFreezePaneColumns();

            (this.dataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();

            // Resset the extended with of the column when we reset the columns before refresh the  column sizer.
            if (this.dataGrid.DetailsViewManager.HasDetailsView && this.View != null)
            {
				if (!this.View.Records.Any(record => record.IsExpanded))
                {
                    this.dataGrid.DetailsViewManager.UpdateExtendedWidth();
                }
            }
            this.dataGrid.GridColumnSizer.Refresh();
        }      


        internal void RefreshBatchUpdate(NotifyCollectionChangedAction action, bool isProgrammatic = true)
        {
            // isProgrammatic will be true when BeginInit is called by application. 
            // && condition is added to reset the respective updates even-though IsInDeferRefresh is true. 
            if (!isProgrammatic)                               
                return;            
            this.ResetColumns(action == NotifyCollectionChangedAction.Reset);            
            // After resetting columns, need to reset ExpandedDetailsView
            this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();     
            //WPF-23162 The below code changes made because when we getting the LastRowIndex it will return the wrong value.
            //Based on the LastRowIndex when we resolve that index it will return as -1.
            this.dataGrid.UpdateRowCountAndScrollBars();
        }

        private void SetIsHidden(NotifyCollectionChangedAction action, List<string> groupedColumns, string columnName = null)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);

            // Reset the IsHidden property for columns while removing from GroupDropArea.
            // Also Reset the IsHidden property while programmatically adding or removing a column in GroupColumnDescriptions collection.            
            if (this.dataGrid.ShowColumnWhenGrouped && (column == null || !column.IsHidden))
                return;

            if (string.IsNullOrEmpty(columnName))
            {
                foreach (var item in groupedColumns)
                {                    
                    var groupedColumn = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == item);
                    if (groupedColumn != null)
                    {
                        if (action == NotifyCollectionChangedAction.Add)
                            groupedColumn.IsHidden = true;
                        else
                        {
                            // WPF - 35754 - procedure : group a column then hide the column manually. Now, ungroup the column.
                            // In this case no need to change the IsHidden state as false when column width is zero.
                            // Because, the hidden column goes to invisible in datagrid when ShowColumnWhenGrouped property as true.
                            if (groupedColumn.Width != 0)
                                groupedColumn.IsHidden = false;
                        }
                    }
                }
            }
            else
            {               
               if (column != null)
                {
                    if (action == NotifyCollectionChangedAction.Add)
                        column.IsHidden = true;
                   
                    else
                    {
                        // WPF - 35754 - procedure : group a column then hide the column manually. Now, ungroup the column.
                        // In this case no need to change the IsHidden state as false when column width is zero.
                        // Because, the hidden column goes to invisible in datagrid when ShowColumnWhenGrouped property as true.
                        if (column.Width != 0)
                            column.IsHidden = false;
                    }
                }
            }
        }
        /// <summary>
        /// Method helps to suspand all the collection change update when doing grouping operatrions 
        /// </summary>
        /// <remarks></remarks>
        internal void Suspend()
        {
            isSuspended = true;
        }

        /// <summary>
        /// Method helps to suspand all the collection change update when doing grouping operatrions 
        /// </summary>
        /// <remarks></remarks>
        internal void Resume()
        {
            isSuspended = false;
        }

        internal void AddGroupDropItem(string columnName)
        {
            if (this.dataGrid.GroupDropArea != null)
            {
                var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);
                var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(col => col.ColumnName == columnName);
                // Creating dummy column to add the group drop area item.
                if (column == null)
                    column = new GridTextColumn() { MappingName = columnName };                    
                this.dataGrid.GroupDropArea.AddGroupDropAreaItem(column, sortColumn != null ? sortColumn.SortDirection : ListSortDirection.Ascending, false);
            }
        }

        internal void AddGroupDropItem(string columnName, int insertAt)
        {
            if (this.dataGrid.GroupDropArea != null)
            {
                var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);
                var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(col => col.ColumnName == columnName);
                if (column != null)
                    this.dataGrid.GroupDropArea.AddGroupDropAreaItem(column, sortColumn != null ? sortColumn.SortDirection : ListSortDirection.Ascending, insertAt, false);
            }
        }

        internal void RemoveGroupDropItem(string columnName)
        {
            if (this.dataGrid.GroupDropArea != null)
            {
                var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == columnName);
                if (column != null)
                    this.dataGrid.GroupDropArea.RemoveGroupDropItem(column);
            }
        }

        internal void RemoveAllGroupDropItems()
        {
            if (this.dataGrid.GroupDropArea != null)
                this.dataGrid.GroupDropArea.RemoveAllGroupDropItems();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method which helps to get the SortColumn which are not in the GroupedColumns
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        internal IEnumerable<SortColumnDescription> GetSortColumnsNotInGroupColumns()
        {
            var unAvailableColumns = new List<SortColumnDescription>();
            foreach (var sortColumn in this.dataGrid.SortColumnDescriptions)
            {
                if (this.dataGrid.GroupColumnDescriptions.All(grpColumn => grpColumn.ColumnName != sortColumn.ColumnName))
                    unAvailableColumns.Add(sortColumn);
            }
            return unAvailableColumns;
        }

        /// <summary>
        /// Method which helps to get the unavailable sort description which property names is not in group description
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        internal IEnumerable<SortDescription> GetSortDescriptionNotInGroupDescription()
        {
            var unAvailableSortDesc = new List<SortDescription>();
            foreach (var sortDesc in this.View.SortDescriptions)
            {
                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                if (this.View.GroupDescriptions.All(groupDesc => ((ColumnGroupDescription)groupDesc).PropertyName != sortDesc.PropertyName))
                    unAvailableSortDesc.Add(sortDesc);
            }
            return unAvailableSortDesc;
        }

        /// <summary>
        /// Method which helps to get the unavailable sort column description in group column description
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        internal IEnumerable<SortColumnDescription> GetSortColumnDescriptionNotInGroupColumnDescription()
        {
            var unAvailableSortDesc = new List<SortColumnDescription>();
            var groupColumnDescriptionsCopy = this.dataGrid.groupColumnDescriptionsCopy;
            // If groupColumnDescriptionsCopy is null, need to get groupColumnDescriptionsCopy from SourceDataGrid
            if (groupColumnDescriptionsCopy == null)
            {
                if (this.dataGrid.NotifyListener != null)
                    groupColumnDescriptionsCopy = this.dataGrid.NotifyListener.SourceDataGrid.groupColumnDescriptionsCopy;
            }
            foreach (var sortDesc in this.dataGrid.SortColumnDescriptions)
            {   
                if (groupColumnDescriptionsCopy.All(groupDesc => (groupDesc.ColumnName != sortDesc.ColumnName)))
                    unAvailableSortDesc.Add(sortDesc);
            }
            return unAvailableSortDesc;
        }

        /// <summary>
        /// Check whether the GroupDescription is already present in Groupdescriptions or not
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CheckForExistingGroupDescription(string columnName)
        {
            //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
            return this.View.GroupDescriptions.Cast<ColumnGroupDescription>().Any(group => group.PropertyName == columnName);
        }

        /// <summary>
        /// Check whethe the corresponding column name already present in GropColumns  or not.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CheckForExistingGroupColumn(string columnName)
        {
            return this.dataGrid.GroupColumnDescriptions.Any(col => col.ColumnName == columnName);
        }

        #endregion

        #region Event Helper Methods

        /// <summary>
        /// Method which helps to update the view when  change the GroupColumn collection
        /// </summary>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnGroupColumnDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool isChanged = false;

            if (this.dataGrid.NotifyListener != null) 
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.GroupColumnDescriptions, e, datagrid => datagrid.GroupColumnDescriptions, this.dataGrid, typeof(GroupColumnDescription));

            // For SourceDataGrid, View is null and if there is any DetailsViewDataGrid with view is null, we need to update SortcolumnDescriptions separately from GroupColumnDescriptions
            // for other grids, SortcolumnDescriptions will be updated when its view Group and Sort Description is changed
            if ((this.dataGrid.NotifyListener != null && this.dataGrid.IsSourceDataGrid) || (this.dataGrid is DetailsViewDataGrid && this.dataGrid.View == null))
            {
                this.dataGrid.suspendNotification = true;
                ChangeSourceDataGridSortColumnDescriptions(e);
                this.dataGrid.suspendNotification = false;
            }

            if (isSuspended || isGroupDescriptionChanged || this.View == null)
                return;

            CommitCurrentRow(this.dataGrid);
            this.isGroupColumnChanged = true;
            Suspend();
            var groupedColumns = new List<string>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var column in e.NewItems)
                        {
                            var groupColumn = column as GroupColumnDescription;
                            if (this.dataGrid.GroupColumnDescriptions.Count(col => col.ColumnName == groupColumn.ColumnName) > 1)
                            {
                                throw new InvalidOperationException("GroupColumnDescription already exist in DataGrid.GroupColumnDescriptions");
                            }
                            if (this.CheckForExistingGroupDescription(groupColumn.ColumnName))
                                continue;

                            var gridColumn = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == groupColumn.ColumnName);
                            if (View != null && (this.dataGrid.CheckColumnNameinItemProperties(gridColumn) || gridColumn.IsUnbound || this.View.IsDynamicBound||this.View.IsXElementBound))
                            {
                                this.View.BeginInit(false);
                                //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                                var groupDescription = new ColumnGroupDescription(groupColumn.ColumnName, groupColumn.Converter, groupColumn.Comparer) { SortGroupRecords = groupColumn.SortGroupRecords };
                                this.View.GroupDescriptions.Insert(e.NewStartingIndex,groupDescription );
                                var sortColumn = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == groupColumn.ColumnName);
                                if (sortColumn == default(SortDescription))
                                {
                                    var insertindex = e.NewStartingIndex > this.View.SortDescriptions.Count ? this.View.SortDescriptions.Count : e.NewStartingIndex;
                                    this.View.SortDescriptions.Insert(insertindex, new SortDescription(groupColumn.ColumnName, ListSortDirection.Ascending));
                                }

                                this.AddGroupDropItem(groupColumn.ColumnName, e.NewStartingIndex);
                                groupedColumns.Add(groupDescription.PropertyName);
                                this.AddGroupDropItem(groupColumn.ColumnName, e.NewStartingIndex);
                                isChanged = true;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        this.View.BeginInit(false);
                        foreach (var column in e.OldItems)
                        {
                            var groupColumn = column as GroupColumnDescription;
                            //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                            var groupDesc = this.View.GroupDescriptions.FirstOrDefault(desc => ((ColumnGroupDescription)desc).PropertyName == groupColumn.ColumnName);
                            var sortDesc = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == groupColumn.ColumnName);
                            groupedColumns.Add((groupDesc as ColumnGroupDescription).PropertyName);
                            this.View.GroupDescriptions.Remove(groupDesc);
                            if (sortDesc != default(SortDescription))
                                this.View.SortDescriptions.Remove(sortDesc);
                            this.RemoveGroupDropItem(groupColumn.ColumnName);                            
                        }
                        isChanged = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.View.BeginInit(false);
                        var sortDescs = this.GetSortDescriptionNotInGroupDescription();
                        groupedColumns = this.View.GroupDescriptions.Select(o => (o as ColumnGroupDescription).PropertyName).ToList();
                        this.View.GroupDescriptions.Clear();
                        this.View.SortDescriptions.Clear();
                        foreach (var desc in sortDescs)
                            this.View.SortDescriptions.Add(desc);
                        this.RemoveAllGroupDropItems();                       
                        isChanged = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        this.View.BeginInit(false);
                        this.dataGrid.View.GroupDescriptions.Move(e.OldStartingIndex, e.NewStartingIndex);
                        isChanged = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldgroupedColumn = e.OldItems[0] as GroupColumnDescription;
                    var replaceColumn = e.NewItems[0] as GroupColumnDescription;
                    if (this.dataGrid.Columns.Any(col => col.MappingName == replaceColumn.ColumnName) && oldgroupedColumn.ColumnName != replaceColumn.ColumnName)
                    {
                        this.View.BeginInit(false);
                        //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                        this.View.GroupDescriptions[e.OldStartingIndex] = new ColumnGroupDescription(replaceColumn.ColumnName, replaceColumn.Converter, replaceColumn.Comparer) { SortGroupRecords = replaceColumn.SortGroupRecords};
                        this.View.SortDescriptions[e.OldStartingIndex] = new SortDescription(replaceColumn.ColumnName, ListSortDirection.Ascending);
                        this.RemoveGroupDropItem(oldgroupedColumn.ColumnName);
                        this.AddGroupDropItem(replaceColumn.ColumnName, e.NewStartingIndex);
                        isChanged = true;
                    }
                    break;
            }

            Resume();
            this.isGroupColumnChanged = false;

            if (isChanged)
            {                
                this.View.EndInit();                

                if (this.View.IsInDeferRefresh)
                {
                    this.dataGrid.GridColumnSizer.Suspend();
                    SetIsHidden(e.Action, groupedColumns);
                    this.dataGrid.GridColumnSizer.Resume();
                    return;
                }

                if(dataGrid.VisualContainer != null)
                {
                    if (e.Action != NotifyCollectionChangedAction.Move && e.Action != NotifyCollectionChangedAction.Replace)
                    {
                        RefreshBatchUpdate(e.Action);
                        SetIsHidden(e.Action, groupedColumns);
                    }
                    dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, new GridGroupingEventArgs(e) { IsProgrammatic = true }));                    
                }
            }
        }
        
        /// <summary>
        /// Update SortColumnDescriptions based on GroupColumnDescriptions
        /// </summary>
        /// <param name="e">GroupColumnDescriptions NotifyCollectionChangedEventArgs</param>
        private void ChangeSourceDataGridSortColumnDescriptions(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var column in e.NewItems)
                    {
                        var groupColumn = column as GroupColumnDescription;
                        if (this.dataGrid.groupColumnDescriptionsCopy!=null)
                        this.dataGrid.groupColumnDescriptionsCopy.Add(groupColumn);
                        var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == groupColumn.ColumnName);
                        if (sortColumn == default(SortColumnDescription))
                            this.dataGrid.SortColumnDescriptions.Insert(e.NewStartingIndex, new SortColumnDescription() { ColumnName = groupColumn.ColumnName, SortDirection = ListSortDirection.Ascending });
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var column in e.OldItems)
                    {
                        var groupColumn = column as GroupColumnDescription;
                        if (this.dataGrid.groupColumnDescriptionsCopy != null)
                        this.dataGrid.groupColumnDescriptionsCopy.Remove(groupColumn);
                        var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == groupColumn.ColumnName);
                        if (sortColumn != default(SortColumnDescription))
                            this.dataGrid.SortColumnDescriptions.Remove(sortColumn);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldgroupedColumn = e.OldItems[0] as GroupColumnDescription;
                    var replaceColumn = e.NewItems[0] as GroupColumnDescription;
                    if (this.dataGrid.groupColumnDescriptionsCopy != null)
                    this.dataGrid.groupColumnDescriptionsCopy[e.OldStartingIndex] = replaceColumn;
                    this.dataGrid.SortColumnDescriptions[e.OldStartingIndex] = new SortColumnDescription() { ColumnName = replaceColumn.ColumnName, SortDirection = ListSortDirection.Ascending };
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var sortDescs = this.GetSortColumnDescriptionNotInGroupColumnDescription();
                    this.dataGrid.SortColumnDescriptions.Clear();
                    foreach (var desc in sortDescs)
                        this.dataGrid.SortColumnDescriptions.Add(desc);
                    if (this.dataGrid.groupColumnDescriptionsCopy != null)
                    this.dataGrid.groupColumnDescriptionsCopy.Clear();
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (this.dataGrid.groupColumnDescriptionsCopy != null)
                    this.dataGrid.groupColumnDescriptionsCopy.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
            }             
        }

        /// <summary>
        /// Method which helps to update the view when  change the GroupDescription collection
        /// </summary>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnGroupDescriptionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool isChanged = false;
            if (isSuspended || isGroupColumnChanged)
                return;
            this.isGroupDescriptionChanged = true;
            Suspend();

            CommitCurrentRow(this.dataGrid);
            var groupedColumns = new List<string>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                        foreach (ColumnGroupDescription description in e.NewItems)
                        {
                            //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                            if (this.View.GroupDescriptions.Count(desc => ((ColumnGroupDescription)desc).PropertyName == description.PropertyName) > 1)
                            {
                                throw new InvalidOperationException("GroupDescription already exist in View.GroupDescriptions");
                            }
                            if (this.CheckForExistingGroupColumn(description.PropertyName))
                                continue;

                            var gridColumn = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == description.PropertyName);

                            if (View != null && (this.dataGrid.CheckColumnNameinItemProperties(gridColumn) || gridColumn.IsUnbound || this.View.IsDynamicBound || this.View.IsXElementBound))
                            {
                                View.BeginInit(false);
                                var groupDesc = new GroupColumnDescription() { ColumnName = description.PropertyName, Converter = description.Converter, Comparer = description.Comparer };
                                this.dataGrid.GroupColumnDescriptions.Add(groupDesc);
                                groupedColumns.Add(groupDesc.ColumnName);
                                this.AddGroupDropItem(description.PropertyName);
                                if (this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == description.PropertyName) == default(SortDescription))
                                    this.View.SortDescriptions.Add(new SortDescription(description.PropertyName, ListSortDirection.Ascending));                               
                                isChanged = true;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    View.BeginInit(false);
                    //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.
                    foreach (ColumnGroupDescription description in e.OldItems)
                    {
                        var groupColumn = this.dataGrid.GroupColumnDescriptions.First(col => col.ColumnName == description.PropertyName);
                        this.dataGrid.GroupColumnDescriptions.Remove(groupColumn);
                        groupedColumns.Add(groupColumn.ColumnName);
                        this.RemoveGroupDropItem(description.PropertyName);
                        var sortDescription = this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == description.PropertyName);
                        if (sortDescription != default(SortDescription))
                            this.View.SortDescriptions.Remove(sortDescription);                      
                        isChanged = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var sortDescs = this.GetSortDescriptionNotInGroupDescription();
                    groupedColumns = this.dataGrid.GroupColumnDescriptions.Select(o => o.ColumnName).ToList();
                    this.View.BeginInit(false);
                    this.dataGrid.GroupColumnDescriptions.Clear();
                    this.View.SortDescriptions.Clear();
                    foreach (var desc in sortDescs)
                        this.View.SortDescriptions.Add(desc);
                    this.RemoveAllGroupDropItems();                  
                    isChanged = true;
                    break;
            }

            Resume();
            this.isGroupDescriptionChanged = false;

            if (isChanged)
            {                
                View.EndInit();

                if (this.View.IsInDeferRefresh)
                {
                    this.dataGrid.GridColumnSizer.Suspend();
                    SetIsHidden(e.Action, groupedColumns);
                    this.dataGrid.GridColumnSizer.Resume();
                    return;
                }

                if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    RefreshBatchUpdate(e.Action);
                    SetIsHidden(e.Action, groupedColumns);
                }
                this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, new GridGroupingEventArgs(e)));                
            }          
        }

        #endregion

        #endregion

        #region Summaries Codes

        /// <summary>
        /// Method which helps to update the TableSummary Values when  change the Record Property Change
        /// </summary>
        /// <param name="e">An <see cref="T:System.ComponentModel.PropertyChangedEventArgs">PropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataRowBase = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowData == sender);
            var liveUpdatMode = dataGrid.LiveDataUpdateMode;
            if (liveUpdatMode.HasFlag(LiveDataUpdateMode.AllowChildViewUpdate) && this.dataGrid.DetailsViewManager.HasDetailsView)
            {
                if (this.dataGrid.DetailsViewDefinition.Any(x => x.RelationalColumn == e.PropertyName))
                {
                    var recordindex = this.dataGrid.View.Records.IndexOfRecord(sender);
                    var recordentry = this.dataGrid.View.Records.GetRecord(recordindex);
                    int childviewindex = -1;
                    if (recordentry != null && recordentry.ChildViews != null)
                    {
                        //Below finding the index of ChildView (if we have more than one ChildView in same level)
                        if (recordentry.ChildViews != null)
                        {
                            childviewindex = recordentry.ChildViews.Keys.ToList().IndexOf(e.PropertyName);
                            if (childviewindex >= 0)
                                recordentry.ChildViews.Remove(e.PropertyName);
                        }
                        if (childviewindex != -1)
                        {
                            var rowindex = this.dataGrid.ResolveToRowIndex(recordindex);
                            var detailsviewdatarow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowindex + childviewindex + 1);
                            if (detailsviewdatarow != null)
                            {
                                if (detailsviewdatarow.IsEnsured || detailsviewdatarow.RowVisibility == Visibility.Visible)
                                    this.dataGrid.DetailsViewManager.UpdateDetailsViewDataRow(this.dataGrid.RowGenerator.Items, detailsviewdatarow.RowIndex);
                                else
                                    detailsviewdatarow.RowIndex = -1;
                            }
                        }
                    }
                    //Updating parent row expander state, when DetailsView is not expanded.
                    if (dataRowBase != null && childviewindex == -1)
                    {
                        dataGrid.UpdateDataRow(dataRowBase.RowIndex);
                    }
                }
            }
            dataGrid.UpdateUnboundColumn(dataRowBase, e.PropertyName);
            
#if WPF
            if (dataGrid.useDrawing && dataRowBase != null)            
                dataRowBase.WholeRowElement.ItemsPanel.InvalidateVisual();            
#endif
            if (dataRowBase != null && this.dataGrid.Columns.Any(col => col.GridValidationMode != GridValidationMode.None))            
            {         
                var dc = dataRowBase.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == e.PropertyName);
                //WPF-25806 - For template column alone validation will be handled here. for other columns validation will be handled in ValidationHelper.
                if (dc != null && (!dc.IsEditing || dc.GridColumn.IsTemplate))
                {
                    (dataRowBase as GridDataRow).ApplyRowHeaderVisualState();
                    this.dataGrid.ValidationHelper.ValidateColumn(dataRowBase.RowData, e.PropertyName, dc.ColumnElement as GridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }

            if ((liveUpdatMode.HasFlag(LiveDataUpdateMode.Default)))
                return;

            if (this.dataGrid.View.TableSummaryRows.Count > 0)
            {
                var tableSummaryRows = this.dataGrid.RowGenerator.Items.Where(item => (item.RowType == RowType.TableSummaryCoveredRow || item.RowType == RowType.TableSummaryRow));
                foreach (SpannedDataRow row in tableSummaryRows)
                {
                    var record = row.RowData as SummaryRecordEntry;
                    this.UpdateSummaryCells(row, record, e.PropertyName);
                }
            }
            
            if (!this.HasGroup || (liveUpdatMode.HasFlag(LiveDataUpdateMode.AllowDataShaping) || liveUpdatMode.HasFlag(LiveDataUpdateMode.AllowSummaryUpdate))
#if WPF
 || (this.View.SourceCollection is IBindingList)
#endif
)
            {
                var groupcoldesc = this.dataGrid.GroupColumnDescriptions.FirstOrDefault(col => col.ColumnName == e.PropertyName);
                //when grouping range of vaues using custom group, below condition allows to refresh summary when grouped column changed
                if (liveUpdatMode.HasFlag(LiveDataUpdateMode.AllowDataShaping) && groupcoldesc != default(GroupColumnDescription) && groupcoldesc.Converter == null)
                    return;

                if (this.dataGrid.View.SummaryRows.Count > 0)
                {
                    var groupSummaryRows = this.dataGrid.RowGenerator.Items.Where(item => (item.RowType == RowType.SummaryCoveredRow || item.RowType == RowType.SummaryRow) && item.RowVisibility == Visibility.Visible);
                    foreach (SpannedDataRow row in groupSummaryRows)
                    {
                        var record = row.RowData as SummaryRecordEntry;
                        this.UpdateSummaryCells(row, record, e.PropertyName);
                    }
                }

                if (this.dataGrid.View.CaptionSummaryRow != null)
                {
                    var captionSummaryRows = this.dataGrid.RowGenerator.Items.Where(item => (item.RowType == RowType.CaptionCoveredRow || item.RowType == RowType.CaptionRow) && item.RowVisibility == Visibility.Visible);
                    foreach (SpannedDataRow row in captionSummaryRows)
                    {
                        var record = (row.RowData as Group).SummaryDetails;
                        this.UpdateSummaryCells(row, record, e.PropertyName);
                    }
                }
            }
        }

        internal void UpdateUnBoundRows(int index, int count, NotifyCollectionChangedAction action)
        {
            if (this.dataGrid.VisualContainer == null)
                return;

            var datarow =
                this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == index);
            var level = count;

            if (datarow != null)
            {
                if (action == NotifyCollectionChangedAction.Add)
                {
                    var rowItems = this.dataGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex >= index).OrderBy(item => item.RowIndex);
                    rowItems.ForEach(row =>
                        {
                            row.SuspendUpdateStyle();
                            if (row.RowType != RowType.HeaderRow)
                            {
                                row.RowIndex += level;
                                row.OnRowIndexChanged();
                            }
                            row.ResumeUpdateStyle();
                        });

                    var unBoundRowsItems = this.dataGrid.UnBoundRows.Where(unBoundRowInfo => unBoundRowInfo.RowIndex >= index).OrderBy(item => item.RowIndex);
                    unBoundRowsItems.ForEach(row => row.RowIndex += level);
                }
                else if (action == NotifyCollectionChangedAction.Remove)
                {
                    var rowIndex = datarow.RowIndex;
                    datarow.RowIndex = -1;

                    if (this.dataGrid.DetailsViewManager.HasDetailsView && datarow.IsExpanded)
                    {
                        for (int i = rowIndex + 1; i < rowIndex + level; i++)
                        {
                            var row = this.dataGrid.RowGenerator.Items.FirstOrDefault(dr => dr.RowIndex == i);
                            if (row != null)
                                row.RowIndex = -1;
                        }
                    }
                    //No need to check whole rows in RowGenerator hence added the Where condition to get the RowData 
                    //which is greater than the removed index.
                    var rowItems = this.dataGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex > index).OrderBy(item => item.RowIndex);
                    rowItems.ForEach(row =>
                        {
                            row.SuspendUpdateStyle();
                            if (row.RowType != RowType.HeaderRow)
                            {
                                 if (row.RowType == RowType.UnBoundRow)
                                {
                                    this.dataGrid.UnBoundRows.ForEach(item =>
                                    {
                                        if (item.RowIndex == row.RowIndex)                                        
                                            item.RowIndex -= level;                                                                                    
                                    });
                                 }
                                row.RowIndex -= level;
                                row.OnRowIndexChanged();
                            }

                            row.ResumeUpdateStyle();
                        });
                }
                else
                    this.RefreshDataRow();    
            }                        
            else
            {
                // While add unboundrow to Top Below Summary/ Bottom above Summary - that is not in view, we  need to consider update the rowindex for all except Header row.
                this.RefreshDataRow();
            }
        }

        /// <summary>
        /// 处理StackedHeaderRows 集合改变属性
        /// Method to handle the collection Changed property of StackedHeaderRows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnStackedHeaderRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {           
            if (this.dataGrid.NotifyListener != null)
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.StackedHeaderRows, e, datagrid => datagrid.StackedHeaderRows, this.dataGrid, typeof(StackedHeaderRow));

            if (dataGrid.VisualContainer == null)
                return;          
            int count = 0;            
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //WPF-29825 When adding StackedHeader for DetailsViewgrid at runtime StackedColumns does not cloned properly,
                    //so clone the StackedColumns based on below code.
                    if (this.dataGrid.NotifyListener != null)
                    {
                        foreach (StackedHeaderRow stackedHeaderRow in this.dataGrid.StackedHeaderRows)
                        {
                            var sourceRow = this.dataGrid.NotifyListener.SourceDataGrid.StackedHeaderRows.ElementAt(this.dataGrid.StackedHeaderRows.IndexOf(stackedHeaderRow));
                            CloneHelper.CloneCollection(sourceRow.StackedColumns, stackedHeaderRow.StackedColumns, typeof(StackedColumn));
                        }
                    }                     
                    dataGrid.InitializeStackedColumnChildDelegate();                    
                    dataGrid.VisualContainer.InsertRows(0, e.NewItems.Count);
                    count = e.NewItems.Count;                    
                    break;
                case NotifyCollectionChangedAction.Remove:
                    dataGrid.RowGenerator.RemoveStackedHeader(e.OldStartingIndex);
                    foreach (StackedHeaderRow stackedHeaderRow in e.OldItems)
                    {
                        stackedHeaderRow.StackedColumns.ForEach(col =>
                        {
                            if (col.ChildColumnChanged != null)
                                col.ChildColumnChanged = null;
                        });

                        count++;
                    }                        
                    dataGrid.VisualContainer.RemoveRows(0, e.OldItems.Count);                    
                    break;       
                case NotifyCollectionChangedAction.Reset:
                    dataGrid.RowGenerator.RemoveStackedHeader();
                    //UWP-5190 while clear the rows from visualContainer we need to consider AddNewRowPosition as FixedTop
                    count = dataGrid.HeaderLineCount - (1 + ((dataGrid.AddNewRowPosition == AddNewRowPosition.Top || dataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop) ? 1 : 0) +
                            ((dataGrid.FilterRowPosition == FilterRowPosition.Top || dataGrid.FilterRowPosition == FilterRowPosition.FixedTop) ? 1 : 0) +
                            dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false) + dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top));
                    if (count == 0)
                        return;
                    dataGrid.VisualContainer.RemoveRows(0, count);   
                    break;                
            }

            // To update catched row index based on newly added/removed row count
            if (dataGrid.DetailsViewManager.HasDetailsView)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += count;
                    });
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                    this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= count;
                    });                
            }

            // Reset the Height when stacked header added.
            if (dataGrid.CanQueryRowHeight())
                dataGrid.VisualContainer.RowHeightManager.Reset();
            // Refresh the headerline count.
            dataGrid.RefreshHeaderLineCount();
            // Update FreezePanes            
            dataGrid.UpdateFreezePaneRows();

            // Reset the row index for Header row also for Stacked Header changes.
            this.dataGrid.RowGenerator.Items.ForEach(row =>
                {
                    if (row.RowType == RowType.HeaderRow)
                        row.RowIndex = -1;
                });        

            this.dataGrid.VisualContainer.UpdateScrollBars();
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();

            // Refresh the view.

            RefreshView(false);   
                        
            // Code to handle the selection                 
            dataGrid.SelectionController.HandleGridOperations(
                                              new GridOperationsHandlerArgs(GridOperation.StackedHeaderRow,
                                  new StackedHeaderCollectionChangedEventArgs(e.Action,count)));                            
            
            // To refresh parent DataGrid
            if (dataGrid is DetailsViewDataGrid && dataGrid.NotifyListener != null)
                this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
        }

        int GetLastIndex(UnBoundRowsPosition position, bool showBelowSummary)
        {
            var URow = this.dataGrid.UnBoundRows.Where(item => item.Position == position && item.ShowBelowSummary == showBelowSummary).ToList();
            if(URow.Count != 0)
            {
                if (position == UnBoundRowsPosition.Top)
                    return showBelowSummary ? ((URow.Count + this.dataGrid.headerLineCount) - 1) : URow.Count + this.dataGrid.StackedHeaderRows.Count;
                else
                    return showBelowSummary ? this.dataGrid.VisualContainer.RowCount : this.dataGrid.VisualContainer.RowCount -
                                                                                        ((this.dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)) +
                                                                                        (this.dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0) +
                                                                                        (this.dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom,true)) );
            }
            return  -1;
        }

        internal void OnUnBoundRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.dataGrid.NotifyListener != null)
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.UnBoundRows, e, datagrid => datagrid.UnBoundRows, this.dataGrid, typeof(GridUnBoundRow));

            if (dataGrid.VisualContainer == null)
                return;            

            int count = 0;
            int rowIndex = 0;
            UnBoundRowsPosition position = UnBoundRowsPosition.None;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (GridUnBoundRow unBoundRow in e.NewItems)
                    {
                        if (unBoundRow.Position == UnBoundRowsPosition.Top)
                        {
                            var lastUnBoundIndex = GetLastIndex(unBoundRow.Position, unBoundRow.ShowBelowSummary);
                            if (!unBoundRow.ShowBelowSummary)
                            {
                                var startIndex = dataGrid.StackedHeaderRows.Count + 1;
                                dataGrid.VisualContainer.InsertRows(startIndex, 1);
                                rowIndex = lastUnBoundIndex == -1 ? startIndex : lastUnBoundIndex;
                            }
                            else
                            {
                                this.dataGrid.VisualContainer.InsertRows(dataGrid.headerLineCount, 1);
                                rowIndex = lastUnBoundIndex == -1 ? dataGrid.HeaderLineCount + (dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0) +
                                    (dataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0) : lastUnBoundIndex;
                            }
                        }
                        else
                        {
                            var lastUnBoundIndex = GetLastIndex(unBoundRow.Position, unBoundRow.ShowBelowSummary);
                            if (!unBoundRow.ShowBelowSummary)
                            {
                                int bodyCount = ((this.dataGrid.VisualContainer.FooterRows - this.dataGrid.FooterRowsCount)
                                        + ((this.dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom) ? 1 : 0)
                                        + ((this.dataGrid.FilterRowPosition == FilterRowPosition.Bottom) ? 1 : 0));
                                rowIndex = lastUnBoundIndex == -1 ? this.dataGrid.VisualContainer.RowCount - bodyCount : lastUnBoundIndex;
                                this.dataGrid.VisualContainer.InsertRows(this.dataGrid.VisualContainer.RowCount - bodyCount, 1);
                            }
                            else
                            {
                                rowIndex = lastUnBoundIndex == -1
                                    ? this.dataGrid.VisualContainer.RowCount
                                    : lastUnBoundIndex;
                                this.dataGrid.VisualContainer.InsertRows(this.dataGrid.VisualContainer.RowCount, 1);
                            }
                        }
                        position = unBoundRow.Position;
                        count++;                          
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (GridUnBoundRow unBoundDataRow in e.OldItems)
                    {
                        rowIndex = unBoundDataRow.RowIndex;

                        //WPF-22165 - If UnBoundRow is not in view means, unBoundDataRow.RowIndex is -1 so we 
                        //have to reset the rowindex based on RemoveRow index.
                        if (unBoundDataRow.Position == UnBoundRowsPosition.Top)
                        {
                            var startIndex = dataGrid.GetHeaderIndex();
                            if (!unBoundDataRow.ShowBelowSummary)
                            {
                                this.dataGrid.VisualContainer.RemoveRows(startIndex, 1);
                                rowIndex = rowIndex == -1 ? startIndex : rowIndex;
                            }
                            else
                            {
                                this.dataGrid.VisualContainer.RemoveRows(dataGrid.headerLineCount, 1);
                                rowIndex = rowIndex == -1 ? dataGrid.headerLineCount : rowIndex;
                            }
                        }
                        else
                        {
                            if (unBoundDataRow.ShowBelowSummary)
                            {
                                this.dataGrid.VisualContainer.RemoveRows(this.dataGrid.VisualContainer.RowCount - 1, 1);
                                rowIndex = rowIndex == -1 ? this.dataGrid.VisualContainer.RowCount : rowIndex;
                            }
                            else
                            {
                                int bodyCount = (this.dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)) +
                                     (this.dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0) +
                                     (this.dataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0);
                                this.dataGrid.VisualContainer.RemoveRows(this.dataGrid.VisualContainer.RowCount - (bodyCount + 1), 1);
                                rowIndex = rowIndex == -1 ? this.dataGrid.VisualContainer.RowCount - (bodyCount + 1) : rowIndex;
                            }

                        }
                        position = unBoundDataRow.Position;
                        count++;
                    }
                }
                    break;
                case NotifyCollectionChangedAction.Reset:
                {
                    var topBodyCount = this.dataGrid.UnBoundRows.TopBodyUnboundRowCount;
                    var bottomBodyCount = this.dataGrid.UnBoundRows.BottomBodyUnboundRowCount;
                    var frozenCount = this.dataGrid.UnBoundRows.FrozenUnboundRowCount;
                    var footerCount = this.dataGrid.UnBoundRows.FooterUnboundRowCount;

                    count = topBodyCount + frozenCount;                
      
                    if(topBodyCount > 0)
                        this.dataGrid.VisualContainer.RemoveRows(dataGrid.GetHeaderIndex(), topBodyCount);
                    if(frozenCount > 0)
                        this.dataGrid.VisualContainer.RemoveRows(dataGrid.headerLineCount, frozenCount);
                    if(footerCount > 0)
                        this.dataGrid.VisualContainer.RemoveRows(this.dataGrid.VisualContainer.RowCount, footerCount);
                    if(bottomBodyCount > 0)
                        this.dataGrid.VisualContainer.RemoveRows(
                                   this.dataGrid.VisualContainer.RowCount -
                                   ((this.dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)) +
                                    (this.dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0)), bottomBodyCount);
                    
                    position= (topBodyCount > 0 || frozenCount > 0 ) ? UnBoundRowsPosition.Top : UnBoundRowsPosition.Bottom;
                    // UnboundRow's internal counts (Frozen, Footer, TopBody, BottomBody) will be recalculated here before the actual event ends.
                    this.dataGrid.UnBoundRows.ReCalculateUnboundRowsCount();
                }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.RefreshView();
                    break;
            }

            // To update catched row index based on newly added/removed row count
            //When the UnBoundRows collection is modified with Bottom UnBoundRow, then no need to update the cached row index, hence 
            //below condition is added.
            if (dataGrid.DetailsViewManager.HasDetailsView && position == UnBoundRowsPosition.Top && count > 0)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += count;
                    });
                else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                    this.dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= count;
                    });
            }

            if (this.dataGrid.CanQueryRowHeight())
                this.dataGrid.VisualContainer.RowHeightManager.Reset();

            if (this.dataGrid.CanQueryCoveredRange())            
                this.dataGrid.CoveredCells.Clear();            
            
            // When the items source is null and unbound rows added at run time, need to update the frozen and footer rows count to show the unbound rows in view.
            if (this.dataGrid.View == null)
            {                
                this.dataGrid.VisualContainer.FrozenRows = this.dataGrid.StackedHeaderRows.Count + 1 + this.dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
                this.dataGrid.VisualContainer.FooterRows = this.dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);                
            }
            else
            {
                this.dataGrid.RowGenerator.Items.Where(row => row.RowType == RowType.TableSummaryCoveredRow || row.RowType == RowType.TableSummaryRow).ForEach(dr => UpdateBindingTableSummary(dr));
                // Which updates the row index of all rows if unbound row added/removed.            
                this.UpdateUnBoundRows(rowIndex, e.Action == NotifyCollectionChangedAction.Add ? e.NewItems.Count : e.Action == NotifyCollectionChangedAction.Remove ? e.OldItems.Count : 0, e.Action);
                // Need to update the header line count , frozen and footer rows - since IsUnBoundRow calculated based on these count                
                this.dataGrid.RefreshHeaderLineCount();

                this.dataGrid.UpdateFreezePaneRows();
            }
            // Need to update the UnBoundRow index. Since, Based on this , GetUnBoundRow calculation has been done.                           
            this.dataGrid.RefreshUnBoundRows(e.Action == NotifyCollectionChangedAction.Remove);
            this.dataGrid.VisualContainer.UpdateScrollBars();
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();


            dataGrid.SelectionController.HandleGridOperations(
                new GridOperationsHandlerArgs(GridOperation.UnBoundDataRow,
                    new UnBoundDataRowCollectionChangedEventArgs(position, e.Action, count, rowIndex)));

            // To refresh parent DataGrid
            if (this.dataGrid is DetailsViewDataGrid && this.dataGrid.NotifyListener != null)
                this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
    }

    /// <summary>
        /// Method which helps to update the view when  change the Table Summary Rows collection
        /// </summary>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnTableSummaryRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.dataGrid.NotifyListener != null) 
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.TableSummaryRows, e, datagrid => datagrid.TableSummaryRows, this.dataGrid, typeof(GridSummaryRow));

            // Since view is null for SourceDataGrid, need to update TableSummary here
            if (this.dataGrid.IsSourceDataGrid)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ISummaryRow row in e.NewItems)
                    {
                        if (row is GridTableSummaryRow)
                            SetGridAndTableSummaryPositionChangedAction(row as GridTableSummaryRow);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (ISummaryRow row in e.OldItems)
                    {
                        if (row is GridTableSummaryRow)
                            (row as GridTableSummaryRow).Dispose();
                    }
                }
            }

            if (this.View == null)
                return;
            //Position is returned to SelectionController wrongly hence the below property is used to update the position.
            TableSummaryRowPosition position = TableSummaryRowPosition.Top;
            int count = 0;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ISummaryRow row in e.NewItems)
                        {
                            this.View.TableSummaryRows.Add(row);
                            if (row is GridTableSummaryRow)
                                SetGridAndTableSummaryPositionChangedAction(row as GridTableSummaryRow);
                            if (row is GridTableSummaryRow &&
                                (row as GridTableSummaryRow).Position == TableSummaryRowPosition.Top)
                            {

                                this.dataGrid.headerLineCount += 1;
                                this.dataGrid.VisualContainer.FrozenRows += 1;
                                this.dataGrid.VisualContainer.InsertRows(dataGrid.HeaderLineCount, 1);
                                position = (row as GridTableSummaryRow).Position;
                            }
                            else
                            {
                                this.dataGrid.VisualContainer.FooterRows += 1;
                                this.dataGrid.VisualContainer.InsertRows(dataGrid.VisualContainer.RowCount, 1);
                                position = TableSummaryRowPosition.Bottom;
                            }
                            count++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ISummaryRow row in e.OldItems)
                        {
                            this.View.TableSummaryRows.Remove(row);
                            var needRemoveRecord = this.View.Records.TableSummaries.FirstOrDefault(record => record.SummaryRow == row);
                            this.View.Records.TableSummaries.Remove(needRemoveRecord);
                            if (row is GridTableSummaryRow)
                                (row as GridTableSummaryRow).Dispose();
                            if (row is GridTableSummaryRow && (row as GridTableSummaryRow).Position == TableSummaryRowPosition.Top)
                            {                                
                                this.dataGrid.headerLineCount -= 1;
                                this.dataGrid.VisualContainer.FrozenRows -= 1;
                                dataGrid.VisualContainer.RemoveRows(dataGrid.HeaderLineCount, 1);
                                position = (row as GridTableSummaryRow).Position;
                            }
                            else
                            {
                                this.dataGrid.VisualContainer.FooterRows -= 1;
                                dataGrid.VisualContainer.RemoveRows(dataGrid.VisualContainer.RowCount - (dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)), 1);
                                position = TableSummaryRowPosition.Bottom;
                            }
                            count++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        count = dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top);
                        this.View.TableSummaryRows.Clear();
                        this.View.Records.TableSummaries.Clear();
                        this.dataGrid.UpdateFreezePaneRows();
                        //WPF-20151-Getting exception while use TableSummaryContextMenu  with postion on Top
                        //Need to update HeaderLineCount if the TableSummaryPosition at Top 
                        this.dataGrid.RefreshHeaderLineCount();
                        position = count > 0 ? TableSummaryRowPosition.Top : TableSummaryRowPosition.Bottom;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        this.View.TableSummaryRows[e.OldStartingIndex] = e.NewItems[0] as ISummaryRow;
                        this.View.Records.TableSummaries[e.OldStartingIndex].SummaryRow = e.NewItems[0] as ISummaryRow;                        
                    }
                    break;
            }

            // To refreh parent grid, when table summary is added to DetailsViewDataGrid
            if (this.dataGrid.NotifyListener != null)
            {
                this.dataGrid.DetailsViewManager.RefreshParentDataGrid(this.dataGrid);
            }

            // To update catched row index based on newly added/removed row count
            if (dataGrid.DetailsViewManager.HasDetailsView && count > 0 && position == TableSummaryRowPosition.Top)
            {
                //CatchedRowIndex is updated when adding TableSummary in bottom of the grid, hence the below condition is added.
                if (e.Action == NotifyCollectionChangedAction.Add)
                    dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += count;
                    });
                else if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                    dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= count;
                    });
            }

            // Since lines are already added or removed from LineSizeCollection, no need to update row count
            //In Reset, we have to update row count. In other case, row count updated by adding / inserting lines in visual container
            RefreshView(e.Action == NotifyCollectionChangedAction.Reset);
            dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.TableSummary, new TableSummaryPositionChangedEventArgs(position, e.Action, count)));
        }

        /// <summary>
        /// Method which helps to update the view when  change the SummaryRows collection
        /// </summary>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnSummaryRowsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.dataGrid.NotifyListener != null) 
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.dataGrid.GroupSummaryRows, e, datagrid => datagrid.GroupSummaryRows, this.dataGrid, typeof(GridSummaryRow));

            if (this.View == null)
            {
                return;
            }
            this.View.BeginInit(false);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ISummaryRow row in e.NewItems)
                            this.View.SummaryRows.Add(row);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ISummaryRow row in e.OldItems)
                            this.View.SummaryRows.Remove(row);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.View.SummaryRows.Clear();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        this.View.SummaryRows[e.OldStartingIndex] = e.NewItems[0] as ISummaryRow;
                    }
                    break;
            }

            this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();
            this.View.EndInit();
            this.RefreshView();
        }

        /// <summary>
        /// Method which helps to update the view when change the Caption Summary Row
        /// </summary>
        /// <param name="row"></param>
        /// <remarks></remarks>
        internal void OnCaptionSummaryRowChanged(GridSummaryRow row)
        {
            var captionRows = this.dataGrid.RowGenerator.Items.Where(item => item.RowType == RowType.CaptionCoveredRow || item.RowType == RowType.CaptionRow);
            if (!captionRows.Any()) 
                return;

            captionRows.ForEach(caption => caption.RowIndex = -1);
            this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        internal void UpdateTableSummaries()
        {
            var footerRows = this.dataGrid.RowGenerator.Items.Where(item => item.RowType == RowType.TableSummaryCoveredRow || item.RowType == RowType.TableSummaryRow);
            foreach (var row in footerRows)
            {
                foreach (var column in row.VisibleColumns)
                    column.UpdateBinding(row.RowData, false);
            }
        }

        internal void UpdateHeaderCells(bool updateCellStyle = true)
        {
            var headerRows = this.dataGrid.RowGenerator.Items.Where(item => item.RowType == RowType.HeaderRow && item.RowIndex == dataGrid.GetHeaderIndex());
            foreach (var headerRow in headerRows)
            {
                foreach (var column in headerRow.VisibleColumns)
                    column.UpdateBinding(headerRow.RowData, updateCellStyle);
            }
        }

        private void UpdateSummaryCells(DataRowBase row, SummaryRecordEntry record, string propertyName)
        {
            // WPF-25804 - While pasting the values in row, if group column value is changed, records will be removed and inserted based on new value.
            // While removing, old items will be disposed. so SummaryRow will be null.
            if (record == null || record.SummaryRow == null)
                return;

            //WPF-29540 Update the summary value for the UnBoundColumns
            if (this.dataGrid.HasUnboundColumns)
            {
                row.VisibleColumns.ForEach(col =>
                 {
                     if (col.GridColumn != null && col.GridColumn.IsUnbound)
                     {
                         var column = col.GridColumn as GridUnBoundColumn;
                         if (column.Expression.ToLower().Contains(propertyName.ToLower()) || column.Format.ToLower().Contains(propertyName.ToLower()))
                            col.UpdateCellStyle(row.RowData);
                     }
                 });
            }

            if (record.SummaryRow.SummaryColumns.Any(summaryColumn => summaryColumn.MappingName == propertyName))
            {
                if (record.SummaryRow.ShowSummaryInRow)
                {
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle(row.RowData));
                }
                else
                {
                    var colum = row.VisibleColumns.FirstOrDefault(column => column.GridColumn != null && column.GridColumn.MappingName == propertyName);
                    if (colum != null)
                        colum.UpdateCellStyle(row.RowData);
                }
            }
        }

        private void UpdateBindingTableSummary(DataRowBase dr)
        {
            if (dr.RowType == RowType.TableSummaryCoveredRow || dr.RowType == RowType.TableSummaryRow)
                dr.VisibleColumns.ForEach(col => col.UpdateBinding(dr.RowData, false));
        }

        internal void InitializeGridTableSummaryRow()
        {
            //WPF-20773 avoid Designer crash while binding tablesummary in xaml
            if (this.dataGrid != null && this.dataGrid.TableSummaryRows != null)
                this.dataGrid.TableSummaryRows.Where(row => row is GridTableSummaryRow).ForEach(tRow => SetGridAndTableSummaryPositionChangedAction(tRow as GridTableSummaryRow));
        }

        private void SetGridAndTableSummaryPositionChangedAction(GridTableSummaryRow row)
        {
            row.DataGrid = this.dataGrid;
            if (row.TableSummaryPositionChanged == null)
                row.TableSummaryPositionChanged = OnTableSummaryPositionChanged;
        }

        private void OnTableSummaryPositionChanged(GridSummaryRow summaryRow, TableSummaryRowPosition position)
        {
            // When view is null, header line count, Frozen rows and footer rows will be updated while setting itemssource
            if (this.dataGrid.View == null)
            {
                return;
            }
            var index = dataGrid.TableSummaryRows.IndexOf(summaryRow);
            
            var lineSizeCollection = dataGrid.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            if (position == TableSummaryRowPosition.Bottom)
            {
                dataGrid.headerLineCount -= 1;
                var tableSummaryRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowRegion == RowRegion.Header && (item.RowType == RowType.TableSummaryCoveredRow || item.RowType == RowType.TableSummaryRow));
                tableSummaryRow.RowIndex = -1;
                tableSummaryRow.RowRegion = RowRegion.Footer;
                dataGrid.VisualContainer.FrozenRows -= 1;
                dataGrid.VisualContainer.FooterRows += 1;
                // Need to remove table summary row from top and add in bottom to maintain row height
                dataGrid.VisualContainer.RemoveRows(dataGrid.HeaderLineCount, 1);              
                dataGrid.VisualContainer.InsertRows(dataGrid.VisualContainer.RowCount, 1);                

                if (tableSummaryRow.WholeRowElement is TableSummaryRowControl && this.dataGrid.VisualContainer.FooterRows == 1)
                    tableSummaryRow.WholeRowElement.RowBorderState = "FooterRow";
            }
            else
            {
                dataGrid.headerLineCount += 1;
                var tableSummaryRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowRegion == RowRegion.Footer && item.WholeRowElement is TableSummaryRowControl);
                tableSummaryRow.RowIndex = -1;
                tableSummaryRow.RowRegion = RowRegion.Header;
                dataGrid.VisualContainer.FooterRows -= 1;
                dataGrid.VisualContainer.FrozenRows += 1;
                // Need to remove table summary row from bottom and add in top to maintain row height
                dataGrid.VisualContainer.RemoveRows(dataGrid.VisualContainer.RowCount - (dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)), 1);
                dataGrid.VisualContainer.InsertRows(dataGrid.HeaderLineCount, 1);               

                if (tableSummaryRow.WholeRowElement is TableSummaryRowControl && tableSummaryRow.WholeRowElement.RowBorderState == "FooterRow")
                    tableSummaryRow.WholeRowElement.RowBorderState = "Normal";            
            }
            this.dataGrid.RefreshHeaderLineCount();
            this.dataGrid.VisualContainer.FrozenRows = this.dataGrid.HeaderLineCount;
            lineSizeCollection.ResumeUpdates();
            var addNewRow = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.AddNewRow);
            if (addNewRow != null)
                addNewRow.RowIndex = -1;
            this.dataGrid.RefreshUnBoundRows();

            // To update catched row index based on newly added/removed row count
            if (dataGrid.DetailsViewManager.HasDetailsView)
            {
                if (position == TableSummaryRowPosition.Top)
                    dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += 1;
                    });
                else if (position == TableSummaryRowPosition.Bottom)
                    dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= 1;
                    });
            }

            // Since lines are already added and removed from LineSizeCollection, no need to update row count
            RefreshView(false);
            dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.TableSummary, new TableSummaryPositionChangedEventArgs(position, NotifyCollectionChangedAction.Move, 1)));
        }

        #endregion

        #region Helper Methods

        private static bool CheckControlKeyPressed()
        {
#if UWP
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.Control).HasFlag(CoreVirtualKeyStates.Down));
#else
            return ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)));
#endif
        }

        #endregion

        #region Filters

        internal void InitialFiltering()
        {
            if (this.View == null)
                return;

            foreach (var column in this.dataGrid.Columns.Where(c => c.FilterPredicates.Any()))
            {
                if (!this.dataGrid.CheckColumnNameinItemProperties(column) && !column.IsUnbound && !this.View.IsDynamicBound && !this.View.IsXElementBound && !column.UseBindingValue)
                    column.FilterPredicates.Clear();
            }

            if (this.View != null && this.dataGrid.Columns.Any(column => column.FilterPredicates.Any()))
            {
                ApplyFilter();
            }

#if WPF
            if (this.dataGrid.SearchHelper.AllowFiltering)
            {
                this.dataGrid.View.Filter = this.dataGrid.SearchHelper.FilterRecords;
                this.dataGrid.View.RefreshFilter();
            }
#endif
        }

        internal void OnGridColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            foreach (GridColumn column in e.NewItems)
            {
                WireColumnDescriptor(column);
                column.mappingName = column.MappingName;
            }
        }

        /// <summary>
        /// Invokes to UnWire Column events
        /// </summary>
        /// <param name="column">GridColumn</param>
        internal void UnWireColumnDescriptor(GridColumn column)
        {
            column.FilterPredicates.CollectionChanged -= OnFiltersCollectionChanged;
            column.SetGrid(null);
            column.ColumnPropertyChanged = null;
        }

        internal void WireColumnDescriptor(GridColumn column)
        {
#if DEBUG
            if (string.IsNullOrEmpty(column.MappingName))
                throw new InvalidOperationException("MappingName is neccessary for defining the columns");
#endif
            if (column.DataGrid != null) return;
            column.FilterPredicates.CollectionChanged -= OnFiltersCollectionChanged;
            column.FilterPredicates.CollectionChanged += OnFiltersCollectionChanged;
            column.SetGrid(this.dataGrid);
        }

        private void OnFiltersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(x => x.FilterPredicates.Equals(sender));
            if (column == null)
                return;

            if (this.dataGrid.NotifyListener != null && !this.dataGrid.suspendNotification)
                this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(column.FilterPredicates, e, datagrid => datagrid.Columns != null ? datagrid.Columns.FirstOrDefault(x => x.MappingName == column.MappingName).FilterPredicates : null, this.dataGrid, typeof(FilterPredicate));

            //WPF-25318 - When we apply the filter in DetailsViewDataGrid means SelectionEvent is raised for ParentDataGrid,
            //So check the DataGrid is not SourceDataGrid.
            if (this.dataGrid.IsSourceDataGrid)
                return;
            if (!FilterSuspend)
            {
                CommitCurrentRow(this.dataGrid, !this.dataGrid.SelectionController.CurrentCellManager.IsFilterRow);
                this.ApplyFilter(column);
                //WRT-5960 - When we maintain the Selection in a row and apply the filter for that row by adding FilterPredicates,
                //the selection is not moves to first row, because we didn't process the selection while applying the filter 
                //using filter predicates, so selection is processed after applying the Filter.
               
                this.dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering,
          new GridFilteringEventArgs(false)
         {
             IsProgrammatic = true
         }));
                if (this.dataGrid.RowGenerator.Items.Count > 0)
                {
                    var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.dataGrid.GetHeaderIndex());
                    if (dataRow == null)
                        return;
                    var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == column.MappingName && x.GridColumn.Equals(column));
                    if (dataColumn != null)
                    {
                        var header = dataColumn.ColumnElement as GridHeaderCellControl;
                        header.ApplyFilterToggleButtonVisualState();
                    }
                }
            }
        }

        internal void ApplyFilter(GridColumnBase column = null)
        {
            var filterPredicates = this.dataGrid.Columns.OfType<IFilterDefinition, GridColumn>();
#if WPF
            if (this.dataGrid.View != null && this.dataGrid.isIQueryable)
                (this.dataGrid.View as CollectionViewAdv).ApplyFilters(filterPredicates, column.mappingName);
#endif
            if (filterPredicates.Count > 0 && this.View != null)
                this.View.FilterPredicates = filterPredicates;
            ApplyFilterRowText(column as GridColumn);
        }

        /// <summary>
        /// Apply the FilterRowText based on FilterPredicates in Column
        /// </summary>
        /// <param name="column">Corresponding column</param>
        internal void ApplyFilterRowText(GridColumn gridColumn)
        {
            //WPF-31262 - Return when the grid is not loaded, view or column is null.
            if (!dataGrid.IsLoaded || dataGrid.View == null || gridColumn == null)
            {
                isFilterApplied = true;
                return;
            }
            var filterPredicates = this.dataGrid.Columns.OfType<IFilterDefinition, GridColumn>();
            //WPF-31262 - Set the FilterRowText while applying filter programmatically when FilteredFrom is FilterRow.
            if (gridColumn.FilteredFrom == FilteredFrom.FilterRow && gridColumn.FilterPredicates.Count > 0)
            {
                var filterpredicate = filterPredicates.FirstOrDefault(pre => pre.MappingName == gridColumn.mappingName);
                var filtertext = GetFilterText(filterpredicate.FilterPredicates, gridColumn);
                gridColumn.SetFilterRowText(filtertext);
                gridColumn.FilterRowCondition = FilterRowHelpers.GetFilterRowCondition(filterpredicate.FilterPredicates.FirstOrDefault().FilterType);
            }
        }

        /// <summary>
        /// Get the FilterText based on filter predicates with corresponding format
        /// </summary>
        /// <param name="FilterValues">FilterPredicates of that column</param>
        /// <returns>Returns the filter row text of the corresponding column.</returns>
        internal string GetFilterText(ObservableCollection<FilterPredicate> filterPredicates, GridColumn column)
        {
            string _filterText = string.Empty;
            if (filterPredicates.Count() > 0)
                _filterText = column.GetFormatedFilterText(filterPredicates.FirstOrDefault().FilterValue);

            return _filterText;
        }

        public void FilterColumn(GridColumn column, List<FilterPredicate> filterPredicates)
        {
            if (column == null)
                return;

            if (filterPredicates == null)
            {
                this.dataGrid.View.FilterPredicates = GetFilters();
                return;
            }

            var args = new GridFilterEventArgs(column, filterPredicates.Count == 0 ? null : filterPredicates, this.dataGrid);
            if (!dataGrid.RaiseFilterChanging(args))
            {
                FilterSuspend = true;
                this.ClearFilters(column);
                if (filterPredicates.Count > 0)
                    filterPredicates.ForEach(x => column.FilterPredicates.Add(x));

                if (!this.dataGrid.View.IsInDeferRefresh)
                    this.dataGrid.DetailsViewManager.ResetExpandedDetailsView();

                if (this.dataGrid.RowGenerator.Items.Count > 0)
                {
                    var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.dataGrid.GetHeaderIndex());
                    var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == column.MappingName && x.GridColumn.Equals(column));
                    if (dataColumn != null)
                    {
                        var header = dataColumn.ColumnElement as GridHeaderCellControl;
                        header.ApplyFilterToggleButtonVisualState();
                    }
                }
                FilterSuspend = false;
            }

            var filters = GetFilters();

#if WPF
            if (this.dataGrid.isIQueryable)
                (this.dataGrid.View as CollectionViewAdv).ApplyFilters(filters, column.mappingName);
#endif

            this.dataGrid.View.FilterPredicates = filters;
            if (!args.Handled)
                dataGrid.RaiseFilterChanged(args);
        }

        internal ObservableCollection<IFilterDefinition> GetFilters()
        {
            return this.dataGrid.Columns.OfType<IFilterDefinition, GridColumn>();
        }

        internal void ClearFilters(GridColumn column)
        {
            if (column.FilterPredicates.Count > 0)
            {
                if (!FilterSuspend && this.dataGrid.RaiseFilterChanging(new GridFilterEventArgs(column, null, this.dataGrid)))
                    return;
                column.FilterRowText = null;
                column.FilterPredicates.Clear();
                if (!FilterSuspend)
                {
                    //WPF-37891 Raise the ColumnPropertyChanged event for reinitialize the edit element while clear the filter.
                    if (column.ColumnPropertyChanged != null)
                        column.ColumnPropertyChanged(column, "FilterRowText");
                    this.dataGrid.RaiseFilterChanged(new GridFilterEventArgs(column, null, this.dataGrid));
                }
            }
        }
#endregion

        #region Dispose Method
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            //UnwireEvents called from DataGrid UnWireEvents method. no need to call here
            //this.UnWireEvents();
            if (isDisposing)
            {
                if (this.dataGrid != null)
                {
                    if (this.dataGrid.GroupColumnDescriptions != null)
                    {
                        this.dataGrid.GroupColumnDescriptions.Clear();
                        this.dataGrid.ClearValue(SfDataGrid.GroupColumnDescriptionsProperty);
                    }

                    if (this.dataGrid.SortColumnDescriptions != null)
                    {
                        this.dataGrid.SortColumnDescriptions.Clear();
                        this.dataGrid.ClearValue(SfDataGrid.SortColumnDescriptionsProperty);
                    }
                    if (this.dataGrid.SortComparers != null)
                    {
                        this.dataGrid.SortComparers.Clear();
                        this.dataGrid.ClearValue(SfDataGrid.SortComparersProperty);
                    }
                }
                if (AddNewRowController != null)
                {
                    AddNewRowController.Dispose();
                    AddNewRowController = null;
                }
                this.dataGrid = null;
            }
            isdisposed = true;
        }

        #endregion
    }
}