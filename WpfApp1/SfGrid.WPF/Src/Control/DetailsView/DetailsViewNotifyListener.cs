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
using Syncfusion.Data;
#if !WinRT && !UNIVERSAL
using System.Windows;
using Syncfusion.Data.Extensions;
#else
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Interface used to hold the information related to the Details View Notify Listener and suspend, resume methods. 
    /// </summary>
    public interface IDetailsViewNotifier
    {
        IDetailsViewNotifyListener NotifyListener { get; }
        bool IsListenerSuspended { get; }
        void SetNotifierListener(IDetailsViewNotifyListener notifyListener);
        void SuspendNotifyListener();
        void ResumeNotifyListener();
    }

    /// <summary>
    /// Interface used to listen the dependency property changed and collection changed and pass the information to SourceDataGrid or ClonedDataGrid
    /// </summary>
    public interface IDetailsViewNotifyListener
    {
        SfDataGrid SourceDataGrid { get; }
        SfDataGrid GetParentDataGrid();
        void NotifyPropertyChanged(object source, string propertyName, DependencyPropertyChangedEventArgs e, Func<SfDataGrid, object> target, IDetailsViewNotifier notifier, Type ownerType);
        void NotifyCollectionChanged(object source, NotifyCollectionChangedEventArgs e, Func<SfDataGrid, object> target, IDetailsViewNotifier notifier, Type baseType);
        void NotifyCollectionPropertyChanged(IDetailsViewNotifier notifier, Type baseType);
        void EnsureCollection<T, S>(T source, Func<SfDataGrid, T> target, Func<S, S, bool> predicate, IDetailsViewNotifier notifier) where T : IList<S>;
    }

    /// <summary>
    /// Interface used to get and set the information while arranging the DetailsViewDataGrid
    /// </summary>
    internal interface IDetailsViewInfo
    {
        double GetExtendedWidth();
        void SetClipRect(Rect rect);
        void SetHorizontalOffset(double offset);
        void SetVerticalOffset(double offset);
    }

    /// <summary>
    /// class used to listen the property changed and collection changed in the DetailsViewDataGrid and SourceDataGrid, then send the notification to others.
    /// </summary>
    public class DetailsViewNotifyListener : IDetailsViewNotifyListener, IDisposable
    {
        private bool isdisposed = false;

        /// <summary>
        /// ClonedDataGrid is the list of DetailsViewDataGrid which is used to apply the changes(property/collection changed) in one DetailsViewDataGrid to others
        /// </summary>
        internal List<SfDataGrid> ClonedDataGrid;
      
        /// <summary>
        /// _parentDataGrid - used in Selection, Navigation and Refreshing
        /// </summary>
        internal SfDataGrid _parentDataGrid;

        /// <summary>
        /// SourceDataGrid - SfDataGrid placed in the GridViewDefinition
        /// </summary>
        public SfDataGrid SourceDataGrid { get; private set; }

        public DetailsViewNotifyListener(SfDataGrid sourceDataGrid, SfDataGrid parentDataGrid)
        {
            SourceDataGrid = sourceDataGrid; 
            _parentDataGrid = parentDataGrid;
            // Need to initialize DetailsViewGrid and wire events
            SourceDataGrid.ForceInitializeDetailsViewGrid();
            // Set NotifierListener for SourceDataGrid
            (SourceDataGrid as IDetailsViewNotifier).SetNotifierListener(this);
            ClonedDataGrid = new List<SfDataGrid>();
        }

        public DetailsViewNotifyListener(SfDataGrid sourceDataGrid)
        {
            SourceDataGrid = sourceDataGrid; 
        }

        /// <summary>
        /// Method which creates DetailsViewDataGrid and Copy Properties from SourceDataGrid to DetailsViewDataGrid
        /// </summary>
        /// <param name="sourceDataGrid">Source DataGrid</param>
        /// <returns>DetailsViewDataGrid</returns>
        public DetailsViewDataGrid CopyPropertiesFromSourceDataGrid(SfDataGrid sourceDataGrid)
        {
            var destinationDataGrid = new DetailsViewDataGrid();
            CopyPropertiesFromSourceDataGrid(sourceDataGrid, destinationDataGrid);
            destinationDataGrid.InitializeDetailsViewDataGrid();
            ClonedDataGrid.Add(destinationDataGrid);
            // Set NotifyListener for DetailsViewDataGrid
            (destinationDataGrid as IDetailsViewNotifier).SetNotifierListener(new DetailsViewNotifyListener(sourceDataGrid));
            return destinationDataGrid;
        }

        /// <summary>
        /// Copy Properties from SourceDataGrid to DetailsViewDataGrid
        /// </summary>
        /// <param name="sourceDataGrid">Source DataGrid</param>
        /// <param name="destinationDataGrid">DetailsViewDataGrid</param>
        /// <param name="needToClearProperties">whether need to clear DetailsViewDataGrid properties before assigning new properties from Source DataGrid</param>
        internal void CopyPropertiesFromSourceDataGrid(SfDataGrid sourceDataGrid, DetailsViewDataGrid destinationDataGrid, bool needToClearProperties = false)
        {
            if (needToClearProperties)
                ClearDetailsViewDataGridProperties(destinationDataGrid);
            // Copying the properties from sourceDataGrid to DetailsViewDataGrid
            CloneHelper.CloneProperties(sourceDataGrid, destinationDataGrid, typeof(SfGridBase));
            CloneHelper.CloneCollection(sourceDataGrid.Columns, destinationDataGrid.Columns, typeof(GridColumnBase));

            // Need to clone GroupColumnDescriptions first. Else, SortColumnDescriptions will be changed when GroupColumnDescriptions is changed
            CloneHelper.CloneCollection(sourceDataGrid.GroupColumnDescriptions, destinationDataGrid.GroupColumnDescriptions, typeof(GroupColumnDescription));
            CloneHelper.CloneCollection(sourceDataGrid.SortColumnDescriptions, destinationDataGrid.SortColumnDescriptions, typeof(SortColumnDescription));
            CloneHelper.CloneCollection(sourceDataGrid.GroupSummaryRows, destinationDataGrid.GroupSummaryRows, typeof(GridSummaryRow));
            CloneHelper.CloneCollection(sourceDataGrid.TableSummaryRows, destinationDataGrid.TableSummaryRows, typeof(GridSummaryRow));
            CloneHelper.CloneCollection(sourceDataGrid.UnBoundRows, destinationDataGrid.UnBoundRows, typeof(GridUnBoundRow));
            CloneHelper.CloneCollection(sourceDataGrid.CoveredCells, destinationDataGrid.CoveredCells, typeof(CoveredCellInfoCollection));
            // UWP-4752 Need to clone SortComparer from sourceDataGrid to DetailsViewDataGrid
            CloneHelper.CloneCollection(sourceDataGrid.SortComparers,destinationDataGrid.SortComparers,typeof(SortComparer));
            CloneHelper.CloneCollection(sourceDataGrid.StackedHeaderRows, destinationDataGrid.StackedHeaderRows, typeof(StackedHeaderRow));
            CloneHelper.CloneCollection(sourceDataGrid.DetailsViewDefinition, destinationDataGrid.DetailsViewDefinition, typeof(ViewDefinition));

#if WPF
            destinationDataGrid.SearchHelper.SearchText = sourceDataGrid.SearchHelper.SearchText;
            destinationDataGrid.SearchHelper.AllowFiltering = sourceDataGrid.SearchHelper.AllowFiltering;
            destinationDataGrid.SearchHelper.SearchType = sourceDataGrid.SearchHelper.SearchType;
            destinationDataGrid.SearchHelper.CanHighlightSearchText = sourceDataGrid.SearchHelper.CanHighlightSearchText;
#endif

            // Copying the columns from sourceDataGrid to DetailsViewDataGrid
            foreach (var targetColumn in destinationDataGrid.Columns)
            {
                var sourceColumn = sourceDataGrid.Columns.FirstOrDefault(x => x.MappingName == targetColumn.MappingName);
                if (sourceColumn != null)
                    CloneHelper.CloneCollection(sourceColumn.FilterPredicates, targetColumn.FilterPredicates, typeof(FilterPredicate));
            }
            // Copying the StackedHeaderRows from sourceDataGrid to DetailsViewDataGrid
            foreach (var targetColumn in destinationDataGrid.StackedHeaderRows)
            {
                var sourceColumn = sourceDataGrid.StackedHeaderRows.ElementAt(destinationDataGrid.StackedHeaderRows.IndexOf(targetColumn));
                if (sourceColumn != null)
                    CloneHelper.CloneCollection(sourceColumn.StackedColumns, targetColumn.StackedColumns, typeof(StackedColumn));
            }

            //The following properties should be same for Parent grid and DetailsViewDataGrid.
            // Suspend sourceDataGrid notification since it will set properties for all DetailsViewDataGrids
            (sourceDataGrid as IDetailsViewNotifier).SuspendNotifyListener();
            sourceDataGrid.AllowSelectionOnPointerPressed = destinationDataGrid.AllowSelectionOnPointerPressed = _parentDataGrid.AllowSelectionOnPointerPressed;
            sourceDataGrid.SelectionMode = destinationDataGrid.SelectionMode = _parentDataGrid.SelectionMode;
            sourceDataGrid.SelectionUnit = destinationDataGrid.SelectionUnit = _parentDataGrid.SelectionUnit;
            sourceDataGrid.NavigationMode = destinationDataGrid.NavigationMode = _parentDataGrid.NavigationMode;
            sourceDataGrid.DetailsViewPadding = destinationDataGrid.DetailsViewPadding = _parentDataGrid.DetailsViewPadding;
            sourceDataGrid.ReuseRowsOnItemssourceChange = destinationDataGrid.ReuseRowsOnItemssourceChange = _parentDataGrid.ReuseRowsOnItemssourceChange;
#if WPF
            sourceDataGrid.UseDrawing = destinationDataGrid.UseDrawing = _parentDataGrid.UseDrawing;
#endif
            // Resume sourceDataGrid notification
            (sourceDataGrid as IDetailsViewNotifier).ResumeNotifyListener();
        }

        /// <summary>
        /// Clear DetailsViewDataGrid properties
        /// </summary>
        /// <param name="detailsViewDataGrid">DetailsViewDataGrid</param>
        private void ClearDetailsViewDataGridProperties(SfDataGrid detailsViewDataGrid)
        {
            if (detailsViewDataGrid.Columns != null)
                detailsViewDataGrid.Columns.Clear();
            if (detailsViewDataGrid.StackedHeaderRows != null)
                detailsViewDataGrid.StackedHeaderRows.Clear();
            if (detailsViewDataGrid.UnBoundRows != null)
                detailsViewDataGrid.UnBoundRows.Clear();
            if (detailsViewDataGrid.DetailsViewDefinition != null)
                detailsViewDataGrid.DetailsViewDefinition.Clear();
            if (detailsViewDataGrid.TableSummaryRows != null)
                detailsViewDataGrid.TableSummaryRows.Clear();

            if (detailsViewDataGrid.SortComparers != null)
                detailsViewDataGrid.SortComparers.Clear();
            detailsViewDataGrid.CaptionSummaryRow = null;
            if (detailsViewDataGrid.View == null)
                return;
            if (detailsViewDataGrid.View.SortDescriptions != null && detailsViewDataGrid.View.SortDescriptions.Any())
                detailsViewDataGrid.View.SortDescriptions.Clear();
            if (detailsViewDataGrid.View.FilterPredicates != null && detailsViewDataGrid.View.FilterPredicates.Any())
                detailsViewDataGrid.View.FilterPredicates.Clear();
            if (detailsViewDataGrid.View.GroupDescriptions != null && detailsViewDataGrid.View.GroupDescriptions.Any())
                detailsViewDataGrid.View.GroupDescriptions.Clear();
            if (detailsViewDataGrid.View.SortComparers != null && detailsViewDataGrid.View.SortComparers.Any())
                detailsViewDataGrid.View.SortComparers.Clear();
            if (detailsViewDataGrid.View.Filter != null)
                detailsViewDataGrid.View.Filter = null;
            if (detailsViewDataGrid.View.GroupComparer != null)
                detailsViewDataGrid.View.GroupComparer = null;
            if (detailsViewDataGrid.View.CaptionSummaryRow != null)
                detailsViewDataGrid.View.CaptionSummaryRow = null;
            if (detailsViewDataGrid.View.SummaryRows != null && detailsViewDataGrid.View.SummaryRows.Any())
                detailsViewDataGrid.View.SummaryRows.Clear();
            if (detailsViewDataGrid.View.TableSummaryRows != null && detailsViewDataGrid.View.TableSummaryRows.Any())
                detailsViewDataGrid.View.TableSummaryRows.Clear();
        }

        public SfDataGrid GetParentDataGrid()
        {
            return _parentDataGrid;
        }

        /// <summary>
        /// When property is chnaged
        /// If it is SourceDataGrid then we will change the property to all the cloned child grids(DetailsViewDataGrids). If it is cloned child grid(DetailsViewDataGrid), then we will change the property to the SourceDataGrid, it will change the property to other cloned child grid using ClonedDataGrid list.
        /// </summary>       
        public void NotifyPropertyChanged(object source, string propertyName, DependencyPropertyChangedEventArgs e, Func<SfDataGrid, object> target, IDetailsViewNotifier notifier, Type ownerType)
        {
            if (notifier.IsListenerSuspended) return;
#if WinRT ||UNIVERSAL
            if (!ownerType.GetTypeInfo().IsAssignableFrom(source.GetType().GetTypeInfo())) return;
#else
            if (!ownerType.IsInstanceOfType(source)) return;
#endif
            var propertyDescriptor = CloneHelper.GetCloneableProperty(source.GetType(), ownerType, propertyName);
            if (propertyDescriptor == null) return;
            notifier.SuspendNotifyListener();
            // If the notifier is SourceDataGrid, we should not set the properties to SourceDataGrid
            if (this.SourceDataGrid != notifier)
            {
                var rootNotifier = SourceDataGrid as IDetailsViewNotifier;
                if (!rootNotifier.IsListenerSuspended)
                {
                    var targetElement = target(SourceDataGrid);
#if WPF
                    propertyDescriptor.SetValue(targetElement, e.NewValue);
#else
                    propertyDescriptor.SetValue(targetElement, e.NewValue, null);
#endif
                }
            }
            else
            {
                // Based on the SourceDataGrid property, other DetailsViewDataGrids property is set
                foreach (var clonedDataGrid in ClonedDataGrid)
                {
                    var clonedNotifier = clonedDataGrid as IDetailsViewNotifier;
                    if (clonedNotifier != null && clonedNotifier.IsListenerSuspended) continue;
                    var clonedTargetElement = target(clonedDataGrid);
#if WPF
                    propertyDescriptor.SetValue(clonedTargetElement, e.NewValue);
#else
                    propertyDescriptor.SetValue(clonedTargetElement, e.NewValue, null);
#endif
                }
            }
            notifier.ResumeNotifyListener();
        }

        /// <summary>
        /// When SortColumnDescriptions, GroupColumnDescriptions are changed, need to update this for other DetailsViewDataGrids
        /// </summary>
        /// <param name="notifier">DataGrid(SourceDataGrid/ DetailsViewDataGrid) which notfifies the changes</param>
        /// <param name="baseType">collection type</param>
        public void NotifyCollectionPropertyChanged(IDetailsViewNotifier notifier, Type baseType)
        {
            if (notifier.IsListenerSuspended) return;
            notifier.SuspendNotifyListener();

            // Based on the SourceDataGrid property, other DetailsViewDataGrids collection is updated
            foreach (var clonedDataGrid in ClonedDataGrid)
            {
                var clonedNotifier = clonedDataGrid as IDetailsViewNotifier;
                if (clonedNotifier != null && clonedNotifier.IsListenerSuspended) continue;
                var sourceDataGrid = notifier as SfDataGrid;
                if (baseType.Equals(typeof(GroupColumnDescriptions)))
                    CloneHelper.EnsureCollection<GroupColumnDescriptions, GroupColumnDescription>(sourceDataGrid.GroupColumnDescriptions, clonedDataGrid.GroupColumnDescriptions, (target, source) => target.ColumnName == source.ColumnName);
                if (baseType.Equals(typeof(SortColumnDescriptions)))
                    CloneHelper.EnsureCollection<SortColumnDescriptions, SortColumnDescription>(sourceDataGrid.SortColumnDescriptions, clonedDataGrid.SortColumnDescriptions, (target, source) => target.ColumnName == source.ColumnName && target.SortDirection == source.SortDirection);
                if (baseType.Equals(typeof(StackedHeaderRows)))
                {
                    CloneHelper.EnsureCollection<StackedHeaderRows, StackedHeaderRow>(sourceDataGrid.StackedHeaderRows, clonedDataGrid.StackedHeaderRows, (target, source) => target.Name == source.Name);
                    // Copying the StackedHeaderRows from sourceDataGrid to DetailsViewDataGrid
                    foreach (var targetColumn in clonedDataGrid.StackedHeaderRows)
                    {
                        var sourceColumn = sourceDataGrid.StackedHeaderRows.ElementAt(clonedDataGrid.StackedHeaderRows.IndexOf(targetColumn));
                        if (sourceColumn != null)
                            CloneHelper.CloneCollection(sourceColumn.StackedColumns, targetColumn.StackedColumns, typeof(StackedColumn));
                    }
                }
                if (baseType.Equals(typeof(UnBoundRows)))
                    CloneHelper.EnsureCollection<UnBoundRows, GridUnBoundRow>(sourceDataGrid.UnBoundRows, clonedDataGrid.UnBoundRows, (target, source) => target.Position == source.Position && target.ShowBelowSummary == source.ShowBelowSummary);
            }
            notifier.ResumeNotifyListener();
        }

        /// <summary>
        /// When collection is chnaged
        /// If it is SourceDataGrid then we will update the collection to all the cloned child grids(DetailsViewDataGrids). If it is cloned child grid(DetailsViewDataGrid), then we will change the property to the SourceDataGrid, it will change the property to other cloned child grid(DetailsViewDataGrids) using ClonedDataGrid list.
        /// </summary>    
        public void NotifyCollectionChanged(object source, NotifyCollectionChangedEventArgs e, Func<SfDataGrid, object> target, IDetailsViewNotifier notifier, Type baseType)
        {
            if (notifier.IsListenerSuspended) return;
            notifier.SuspendNotifyListener();
            if (this.SourceDataGrid != notifier)
            {
                var rootNotifier = SourceDataGrid as IDetailsViewNotifier;
                if (!rootNotifier.IsListenerSuspended)
                {
                    var targetElement = target(SourceDataGrid);
                    ProcessCollectionChanged(e, (IList)source, (IList)targetElement, baseType);
                }
            }
            else
            {
                foreach (var clonedDataGrid in ClonedDataGrid)
                {
                    var clonedNotifier = clonedDataGrid as IDetailsViewNotifier;
                    if (clonedNotifier != null && clonedNotifier.IsListenerSuspended) continue;
                    var clonedTargetElement = target(clonedDataGrid);
                    ProcessCollectionChanged(e, (IList)source, (IList)clonedTargetElement, baseType);
                }
            }
            notifier.ResumeNotifyListener();
        }


        public void EnsureCollection<T, S>(T source, Func<SfDataGrid, T> target, Func<S, S, bool> predicate, IDetailsViewNotifier notifier) where T : IList<S>
        {
            if (this.SourceDataGrid == notifier) return;
            var targetList = target(this.SourceDataGrid);
            if (source.Count != targetList.Count)
            {
                targetList.Clear();
                CloneHelper.CloneCollection((IList)source, this.SourceDataGrid.Columns, typeof(S));
                return;
            }
            if (!source.Any(column => targetList.All(col => predicate(col, column))))
                return;
            targetList.Clear();
            CloneHelper.CloneCollection((IList)source, this.SourceDataGrid.Columns, typeof(S));
        }

        /// <summary>
        /// Copying the collection changes to other DetailsViewDataGrids/ SourceDataGrid
        /// </summary>   
        public static void ProcessCollectionChanged(NotifyCollectionChangedEventArgs e, IList source, IList target, Type baseType)
        {
            if (target == null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            var clonedNewItem = CloneHelper.CreateClonedInstance(newItem, baseType);
                            var index = source.IndexOf(newItem);
                            // WPF-21377 (Issue 3) - while cloning columns, need to copy filter predicates also
                            if (newItem is GridColumn && (newItem as GridColumn).FilterPredicates != null && (newItem as GridColumn).FilterPredicates.Any())
                                CloneHelper.CloneCollection((newItem as GridColumn).FilterPredicates, (clonedNewItem as GridColumn).FilterPredicates, typeof(FilterPredicate));
                            target.Insert(index, clonedNewItem);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var oldItem in e.OldItems)
                            target.RemoveAt(e.OldStartingIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var obj1 = target[e.OldStartingIndex];
                        var obj2 = target[e.NewStartingIndex];
                        var num = (e.NewStartingIndex > e.OldStartingIndex) ? 1 : 0;
                        target.Remove(obj1);
                        target.Insert(target.IndexOf(obj2) + num, obj1);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        target.Clear();
                        CloneHelper.CloneCollection(source, target, baseType);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        target[e.OldStartingIndex] = e.NewItems[0];
                    }
                    break;
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewNotifyListener"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewNotifyListener"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                SourceDataGrid = null;
                if (ClonedDataGrid == null) return;
                ClonedDataGrid.Clear();
                ClonedDataGrid = null;
            }
            isdisposed = true;
        }
    }
}
