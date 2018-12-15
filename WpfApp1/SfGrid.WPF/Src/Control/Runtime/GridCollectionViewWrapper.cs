#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
#if WinRT || UNIVERSAL
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Globalization;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
#if WPF
using System.Data;
using System.Xml.Linq;
#endif
#endif


namespace Syncfusion.UI.Xaml.Grid
{

    public class GridQueryableCollectionViewWrapper : QueryableCollectionView
    {
        #region Fields
        protected SfDataGrid dataGrid;
        private bool isdisposed = false;
        #endregion

        #region ctor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GridQueryableCollectionViewWrapper(IEnumerable source, SfDataGrid grid)
            : base(source, grid.SourceType)
        {
            dataGrid = grid;
            propertyAccessProvider = this.CreateItemPropertiesProvider();
        }
        #endregion

        /// <summary>
        /// Set dataGrid in view
        /// </summary>
        /// <param name="grid">dataGrid</param>
        public override void AttachGridView(object grid)       
        {
            if (!(grid is SfDataGrid))
                throw new InvalidOperationException("Attached view is not type of SfDataGrid");

            dataGrid = grid as SfDataGrid;           
            if (this.IsDynamicBound)
                (propertyAccessProvider as GridDynamicPropertiesProvider).SetDataGrid(dataGrid);
            else if (this.IsXElementBound)
                (propertyAccessProvider as GridXElementAttributesProvider).SetDataGrid(dataGrid);
            else
                (propertyAccessProvider as GridItemPropertiesProvider).SetDataGrid(dataGrid); 
        }

        /// <summary>
        /// Clear Selection if the selected item is not in View 
        /// </summary>
        /// <param name="isProgrammatic">if View need to clear the selection in SfDataGrid, using View.RowFilter expression; set <b>true</b>. The default value is <b>false</b>.</param>

        public override void RefreshFilter(bool isProgrammatic)             
        {
            base.RefreshFilter();

            if (isProgrammatic)
                dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)
                {
                    IsProgrammatic = true
                }));           
        }

        #region Override Functions 

        /// <summary>
        /// Returns func for corresponding Dataoperation and Property Name.
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <param name="operation">Data operation</param>
        /// <param name="reflectionMode">Data Reflection mode</param>
        /// <returns>Func</returns>
        public override Func<string, object, object> GetFunc(string propertyName, DataOperation operation = DataOperation.Default, DataReflectionMode reflectionMode = DataReflectionMode.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column == null)
                return null;

            DataReflectionMode mode = operation == DataOperation.Grouping ? column.GroupMode : reflectionMode;

            if (this.propertyAccessProvider != null)
                this.propertyAccessProvider.OnBeginReflect();

            return this.GetValueFunc(column, operation, mode, column.useBindingValue);
        }

        /// <summary>
        /// Return ExpressionFunc for corresponding Dataoperation and Property Name.
        /// </summary>
        /// <param name="propertyName">propertyName</param>
        /// <param name="operation">Data operation</param>
        /// <param name="reflectionMode">Reflection Mode</param>
        /// <returns>ExpressionFunc</returns>
        public override System.Linq.Expressions.Expression<Func<string, object, object>> GetExpressionFunc(string propertyName, DataOperation operation = DataOperation.Default, DataReflectionMode reflectionMode = DataReflectionMode.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column == null)
                return null;

            DataReflectionMode mode = operation == DataOperation.Grouping ? column.GroupMode : reflectionMode;

            if (this.propertyAccessProvider != null)
                this.propertyAccessProvider.OnBeginReflect();
            return this.GetValueExpressionFunc(column, operation, mode, column.UseBindingValue);
        }

        public override Func<string, object, object> GetDisplayValueFunc(string propertyName, DataOperation operation = DataOperation.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column != null && column.IsUnbound)
                return this.GetUnboundFunc(dataGrid, column as GridUnBoundColumn);

            return base.GetDisplayValueFunc(propertyName, operation);
        }

        #endregion

        #region internal methods


        #endregion

        protected override ItemPropertiesProvider CreateItemPropertiesProvider()
        {
            if (this.IsDynamicBound)
            {
                return new GridDynamicPropertiesProvider(this, dataGrid);
            }

            if (this.IsXElementBound)
                return new GridXElementAttributesProvider(this, dataGrid);

            return new GridItemPropertiesProvider(this, dataGrid);
        }

        /// <summary>
        /// Checks whether the property change affects the UpboundCoulmn summary in corresponding SummaryRow.
        ///<param name="row">row</param>
        ///<param name="propertyName">propertyName</param>
        /// <returns>true if the property change affects UpboundCoulmn summary in corresponding SummaryRow; otherwise false</returns>
        protected override bool CanUpdateSummary(ISummaryRow row, string propertyName)
        {
            if (this.dataGrid.HasUnboundColumns)
            {
                var canupdate = dataGrid.Columns.FirstOrDefault(col => col.IsUnbound
                && ((col as GridUnBoundColumn).Expression.Contains(propertyName)
                || (col as GridUnBoundColumn).Format.Contains(propertyName))) != null;

                if (canupdate)
                    return true;
            }
            return base.CanUpdateSummary(row, propertyName);
        }
        protected override RecordsList CreateRecords()
        {
            if (dataGrid != null && dataGrid.EnableDataVirtualization 
#if WPF
                || this.IsIQueryable
#endif
                )
                return new VirtualRecordsList(this.ViewSource, this);
            return base.CreateRecords();
        }

        protected override TopLevelGroup CreateTopLevelGroup()
        {
            if (dataGrid != null && dataGrid.EnableDataVirtualization
#if WPF
                || this.IsIQueryable
#endif
                )
                return new GridVirtualizingTopLevelGroup(this.dataGrid, this);
            return new GridDataTopLevelGroup(this.dataGrid, this);            
        }

        protected override void RemoveRecord(object record)
        {
            base.RemoveRecord(record);
            this.dataGrid.SelectionController.HandleCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, record, 0), CollectionChangedReason.DataReorder);
        }

        /// <summary>
        /// Invokes to check whether source is need to execute in Parallel Query.
        /// </summary>
        /// <param name="operation">DataOperation</param>
        /// <returns>Returns true if source is execute in parallel query</returns>
        protected override bool CanExecuteParallel(DataOperation operation = DataOperation.Default)
        {
            if (!this.UsePLINQ)
                return this.UsePLINQ;

            if (operation == DataOperation.Filtering)
            {
                foreach (var r in this.FilterPredicates.Where(x => x.FilterPredicates.Count > 0).ToList())
                {
                    var column = this.dataGrid.Columns.Where(x => x.mappingName == r.MappingName).FirstOrDefault();
#if WPF
                    if (column.UseBindingValue || column is GridMaskColumn)
#else
                    if (column.UseBindingValue)
#endif
                        return false;
                }
            }
            return base.CanExecuteParallel(operation);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridQueryableCollectionViewWrapper"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                // Unwire View related events wired in GridModel
                if (this.dataGrid != null && this.dataGrid.GridModel != null)
                    dataGrid.GridModel.UnWireEvents(false);
               

                // Set itemssource as null
                if (this.dataGrid != null && this.dataGrid is DetailsViewDataGrid)
                {
                    //this.dataGrid.View = null;
                    this.dataGrid.ItemsSource = null;
                }
                DetachGridView();
                base.Dispose(isDisposing); 
            }
            isdisposed = true;
        }

        public override void DetachGridView()
        {
            this.dataGrid = null;
        }

        public override object GetDataGrid()
        {
            return this.dataGrid;
        }
    }

#if WPF
    public class GridDataTableCollectionViewWrapper : DataTableCollectionView
    {
        protected SfDataGrid datagrid;
        private bool isdisposed = false;

        #region ctor
        public GridDataTableCollectionViewWrapper(IEnumerable source, SfDataGrid grid)
            : base(source)
        {
            datagrid = grid;
            this.propertyAccessProvider = this.CreateItemPropertiesProvider();
        }
        #endregion

        /// <summary>
        /// Checks whether the property change affects the UpboundCoulmn summary in corresponding SummaryRow.
        ///<param name="row">row</param>
        ///<param name="propertyName">propertyName</param>
        /// <returns>true if the property change affects UpboundCoulmn summary in corresponding SummaryRow; otherwise false</returns>
        protected override bool CanUpdateSummary(ISummaryRow row, string propertyName)
        {
            if (this.datagrid.HasUnboundColumns)
            {
                var canupdate = datagrid.Columns.FirstOrDefault(col => col.IsUnbound
                && ((col as GridUnBoundColumn).Expression.Contains(propertyName)
                || (col as GridUnBoundColumn).Format.Contains(propertyName))) != null;

                if (canupdate)
                    return true;
            }
            return base.CanUpdateSummary(row, propertyName);
        }
        
        protected override RecordsList CreateRecords()
        {
            if (datagrid != null && datagrid.EnableDataVirtualization
#if WPF
                 || this.IsIQueryable
#endif
                )
                return new VirtualRecordsList(this.ViewSource, this);
            return base.CreateRecords();
        }

        protected override TopLevelGroup CreateTopLevelGroup()
        {
            if (datagrid != null && datagrid.EnableDataVirtualization
#if WPF
                || this.IsIQueryable
#endif
                )
                return new GridVirtualizingTopLevelGroup(this.datagrid, this);
            return new GridDataTopLevelGroup(this.datagrid, this);            
        }

        protected override ItemPropertiesProvider CreateItemPropertiesProvider()
        {
            return new GridItemPropertiesProvider(this, datagrid);
        }


        public override void AttachGridView(object dataGrid)
        {
            datagrid = dataGrid is SfDataGrid ? dataGrid as SfDataGrid : null;
        }

        /// <summary>
        /// Clear Selection if the selected item is not in View 
        /// </summary>
        /// <param name="isProgrammatic">if View need to clear the selection in SfDataGrid, using View.RowFilter expression; set <b>true</b>. The default value is <b>false</b>.</param>
        public override void RefreshFilter(bool isProgrammatic)
        {
            base.RefreshFilter();

            if (isProgrammatic)
                datagrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)
                {
                    IsProgrammatic = true
                }));            
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridDataTableCollectionViewWrapper"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                // Unwire View related events wired in GridModel
                if (this.datagrid != null && this.datagrid.GridModel != null)
                    datagrid.GridModel.UnWireEvents(false);
                base.Dispose(isDisposing);
                if (this.datagrid != null)
                {
                    this.datagrid.View = null;

                }
                DetachGridView();
            }
            isdisposed = true;
        }

        public override object GetDataGrid()
        {
            return this.datagrid;
        }

        public override void DetachGridView()
        {
            this.datagrid = null;
        }
    }
#endif

    public class GridPagedCollectionViewWrapper : Syncfusion.Data.PagedCollectionView
    {
        #region Fields
        protected SfDataGrid dataGrid;
        new ItemPropertiesProvider propertyAccessProvider;
        private bool isdisposed = false;
        #endregion

        #region Ctor

        public GridPagedCollectionViewWrapper()
            : base()
        {

        }

        public GridPagedCollectionViewWrapper(IEnumerable sender)
            : base(sender)
        {
        }

        #endregion

        #region Override Functions

        public override bool MoveToPage(int pageIndex)
        {
            if (dataGrid != null)
            {
                var needToSkip = true;
                if (dataGrid.SelectionController.CurrentCellManager.CurrentCell != null && dataGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                    needToSkip = dataGrid.SelectionController.CurrentCellManager.RaiseValidationAndEndEdit();
                else if (dataGrid.SelectedDetailsViewGrid != null)
                    needToSkip = NestedGridValidation(dataGrid);
                if (needToSkip)
                    return base.MoveToPage(pageIndex);
                else
                    return false;
            }
            else
                return base.MoveToPage(pageIndex);
        }

        internal bool NestedGridValidation(SfDataGrid sfdatagrid)
        {
            SfDataGrid nestedDataGrid = null;
            if (sfdatagrid.SelectedDetailsViewGrid != null)
                nestedDataGrid = sfdatagrid.SelectedDetailsViewGrid as SfDataGrid;
            if (nestedDataGrid != null && nestedDataGrid.SelectedDetailsViewGrid != null)
                return NestedGridValidation(nestedDataGrid);
            if (nestedDataGrid != null && nestedDataGrid.SelectionController.CurrentCellManager.CurrentCell != null && nestedDataGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                return nestedDataGrid.SelectionController.CurrentCellManager.RaiseValidationAndEndEdit();
            return true;
        }

        /// <summary>
        /// Return func for corresponding Dataoperation and Property Name.
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        /// <param name="operation">Data operation</param>
        /// <param name="reflectionMode">Data Reflection mode</param>
        /// <returns>Func</returns>
        public override Func<string, object, object> GetFunc(string propertyName, DataOperation operation = DataOperation.Default, DataReflectionMode reflectionMode = DataReflectionMode.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column == null)
                return null;

            DataReflectionMode mode = operation == DataOperation.Grouping ? column.GroupMode : reflectionMode;

            if (this.propertyAccessProvider != null)
                this.propertyAccessProvider.OnBeginReflect();
            return this.GetValueFunc(column, operation, mode, column.useBindingValue);

        }

        /// <summary>
        /// Return ExpressionFunc for corresponding Dataoperation and Property Name.
        /// </summary>
        /// <param name="propertyName">propertyName</param>
        /// <param name="operation">Data operation</param>
        /// <param name="reflectionMode">Reflection Mode</param>
        /// <returns>ExpressionFunc</returns>
        public override System.Linq.Expressions.Expression<Func<string, object, object>> GetExpressionFunc(string propertyName, DataOperation operation = DataOperation.Default, DataReflectionMode reflectionMode = DataReflectionMode.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column == null)
                return null;

            DataReflectionMode mode = operation == DataOperation.Grouping ? column.GroupMode : reflectionMode;

            if (this.propertyAccessProvider != null)
                this.propertyAccessProvider.OnBeginReflect();
            return this.GetValueExpressionFunc(column, operation, mode, column.useBindingValue);
        }

        public override Func<string, object, object> GetDisplayValueFunc(string propertyName, DataOperation operation = DataOperation.Default)
        {
            var column = this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == propertyName);

            if (column != null && column.IsUnbound)
                     return this.GetUnboundFunc(dataGrid, column as GridUnBoundColumn);

            return base.GetDisplayValueFunc(propertyName, operation);
        }

        #endregion

        protected override ItemPropertiesProvider CreateItemPropertiesProvider()
        {
            if (this.IsDynamicBound)
            {
                propertyAccessProvider = new GridDynamicPropertiesProvider(this, dataGrid);
                return propertyAccessProvider;
            }
            if (this.IsXElementBound)
                propertyAccessProvider = new GridXElementAttributesProvider(this, dataGrid);
            else
                propertyAccessProvider = new GridItemPropertiesProvider(this, dataGrid);
            return propertyAccessProvider;
        }

        protected override void RaiseExceptionThrownEvent(Exception e)
        {
#if WPF
            if (this.dataGrid == null || !this.dataGrid.CanRaiseExceptionThrownEvent())
                return;

            this.dataGrid.RaiseExceptionThrownEvent(new ExceptionThrownEventArgs() { Exception = e });
#endif
        }
        /// <summary>
        /// Clear Selection if the selected item is not in View 
        /// </summary>
        /// <param name="isProgrammatic">if View need to clear the selection in SfDataGrid, using View.RowFilter expression; set <b>true</b>. The default value is <b>false</b>.</param>
        public override void RefreshFilter(bool isProgrammatic)
        {
            base.RefreshFilter();    
                
            if (isProgrammatic)
                dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)
                {
                    IsProgrammatic = true
                }));
        }

        /// <summary>
        /// Checks whether the property change affects the UpboundCoulmn summary in corresponding SummaryRow.
        ///<param name="row">row</param>
        ///<param name="propertyName">propertyName</param>
        /// <returns>true if the property change affects UpboundCoulmn summary in corresponding SummaryRow; otherwise false</returns>
        protected override bool CanUpdateSummary(ISummaryRow row, string propertyName)
        {
            if (this.dataGrid.HasUnboundColumns)
            {
                var canupdate = dataGrid.Columns.FirstOrDefault(col => col.IsUnbound
                && ((col as GridUnBoundColumn).Expression.Contains(propertyName)
                || (col as GridUnBoundColumn).Format.Contains(propertyName))) != null;

                if (canupdate)
                    return true;
            }
            return base.CanUpdateSummary(row, propertyName);
        }
        
        protected override RecordsList CreateRecords()
        {
            if (dataGrid != null && dataGrid.EnableDataVirtualization
#if WPF
                || this.IsIQueryable
#endif
                )
                return new VirtualRecordsList(this.GetQueryableSource(), this);
            return base.CreateRecords();
        }

        protected override TopLevelGroup CreateTopLevelGroup()
        {
            if (dataGrid != null && dataGrid.EnableDataVirtualization 
#if WPF
                || this.IsIQueryable
#endif
                )
                return new GridVirtualizingTopLevelGroup(dataGrid, this);
            return new GridDataTopLevelGroup(dataGrid, this);
        }

        protected override void RemoveRecord(object record)
        {
            if (this.dataGrid != null)
            {
                this.dataGrid.SelectionController.HandleCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, record, 0), CollectionChangedReason.DataReorder);
            }
        }

        #region Public Method

        /// <summary>
        /// When view is reused, need to update the grid
        /// </summary>        
        public override void AttachGridView(object dataGrid)     
        {
            if (!(dataGrid is SfDataGrid))
                return;
            this.dataGrid = dataGrid as SfDataGrid;

            if (propertyAccessProvider is GridItemPropertiesProvider)
                (propertyAccessProvider as GridItemPropertiesProvider).SetDataGrid(this.dataGrid);

            else if (propertyAccessProvider is GridDynamicPropertiesProvider)
                (propertyAccessProvider as GridDynamicPropertiesProvider).SetDataGrid(this.dataGrid);

            else if (propertyAccessProvider is GridXElementAttributesProvider)
                (propertyAccessProvider as GridXElementAttributesProvider).SetDataGrid(this.dataGrid);

#if WPF
            if (this.IsIQueryable)
            {
                this.KeyValue = this.dataGrid.KeyColumn;
                this.dataGrid.isIQueryable = this.IsIQueryable;
            }
#endif
        }

        #endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridPagedCollectionViewWrapper"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                // Unwire View related events wired in GridModel
                if (this.dataGrid != null && this.dataGrid.GridModel != null)
                    dataGrid.GridModel.UnWireEvents(false);
                base.Dispose(isDisposing);
                if (this.dataGrid != null)
                {
                    this.dataGrid.View = null;
                }
                DetachGridView();
                if (propertyAccessProvider != null)
                {
                    propertyAccessProvider.Dispose();
                    propertyAccessProvider = null;
                }
            }
            isdisposed = true;
        }

        public override object GetDataGrid()
        {
            return this.dataGrid;
        }

        public override void DetachGridView()
        {
            this.dataGrid = null;
        }

    }
}
