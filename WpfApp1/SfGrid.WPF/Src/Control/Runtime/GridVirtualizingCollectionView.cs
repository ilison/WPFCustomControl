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
#endif
#endif

namespace Syncfusion.UI.Xaml.Grid
{

    public class GridVirtualizingCollectionView : VirtualizingCollectionView
    {
        #region Members

        protected SfDataGrid dataGrid = null;
        private bool isdisposed = false;
        #endregion

        #region Ctor

        public GridVirtualizingCollectionView(IEnumerable source)
            : base(source)
        {

        }

        public GridVirtualizingCollectionView()
            : base()
        {
        }

        #endregion

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
        /// Returns Expression func for corresponding Dataoperation and Property Name.
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
                return new GridDynamicPropertiesProvider(this, dataGrid);
            }
            if(this.IsXElementBound)
                return new GridXElementAttributesProvider(this, dataGrid);
            return new GridItemPropertiesProvider(this, dataGrid);
        }

        /// <summary>
        /// Checks whether the property changes affect the corresponding SummaryRow of UpboundCoulmn
        /// </summary>
        ///<param name="row">row</param>
        ///<param name="propertyName">propertyName</param>
        /// <returns>True if the corresponding propertyName is corresponding SummaryRow of UpboundCoulmn</returns>
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

protected sealed override TopLevelGroup CreateTopLevelGroup()
        {
            return new GridVirtualizingTopLevelGroup(this.dataGrid, this);
        }

        public override void AttachGridView(object dataGrid)
        {
            if (!(dataGrid is SfDataGrid))
                return;
            this.dataGrid = dataGrid as SfDataGrid;
            if (propertyAccessProvider is GridItemPropertiesProvider)
                (propertyAccessProvider as GridItemPropertiesProvider).SetDataGrid(this.dataGrid);
            else if (propertyAccessProvider is GridDynamicPropertiesProvider)
                (propertyAccessProvider as GridDynamicPropertiesProvider).SetDataGrid(this.dataGrid);
#if WPF
            if (this.IsIQueryable)
            {
                this.KeyValue = this.dataGrid.KeyColumn;
                this.dataGrid.isIQueryable = this.IsIQueryable;
            }
#endif
        }


        public override object GetDataGrid()
        {
            return this.dataGrid;
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
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridVirtualizingCollectionView"/> class.
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
            }
            isdisposed = true;
        }

        public override void DetachGridView()
        {
            this.dataGrid = null;
        }
    }

    public class GridVirtualizingTopLevelGroup : VirtualizingTopLevelGroup
    {
         protected SfDataGrid dataGrid;

         public GridVirtualizingTopLevelGroup(SfDataGrid grid, CollectionViewAdv collectionView)
            : base(collectionView)
        {
            dataGrid = grid;
        }

        public override void Invalidate(int index, int count)
        {
            var rowIndex = this.dataGrid.ResolveToRowIndex(index);
            for (var i = 0; i < count; i++)
            {
                this.dataGrid.GridModel.UpdateDataRow(rowIndex);
                rowIndex++;
            }
        }

        public override int RelationsCount
        {
            get
            {
                return this.dataGrid.DetailsViewDefinition != null ? this.dataGrid.DetailsViewDefinition.Count : 0;
            }
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ExceptionThrown"/> event.  
    /// </summary>
    /// <param name="sender">The sender that contains the SfDataGrid.</param>
    /// <param name="args">The <see cref="Syncfusion.UI.Xaml.Grid.ExceptionThrownEventArgs"/> that contains the event data.</param>
    public delegate void ExceptionThrownEventHandler(object sender, ExceptionThrownEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ExceptionThrown"/> event.
    /// </summary>
    public class ExceptionThrownEventArgs : EventArgs
    {
        public ExceptionThrownEventArgs()
        {

        }

        /// <summary>
        /// Gets a value that indicates the type of exception caught while fetching data from server.
        /// </summary>
        public Exception Exception { get; internal set; }
    }
}
