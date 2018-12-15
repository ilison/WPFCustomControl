#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows.Input;
using System.Windows;
using System.Data;
using Syncfusion.Data.Helper;
#if WPF
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;
#else
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using System.Reflection;
#endif

namespace Syncfusion.UI.Xaml.Grid.Helpers
{
#if !WPF
    using Key = Windows.System.VirtualKey;
#endif
    /// <summary>
    /// Represents an extension class that provides helper methods for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
    /// </summary>
    public static class GridHelper
    {
#if WPF
        internal static bool CanAllowNull(this SfDataGrid datagrid, PropertyDescriptor propertyinfo)
#else
        internal static bool CanAllowNull(this SfDataGrid datagrid, PropertyInfo propertyinfo)
#endif
        {
#if WPF
            if (datagrid.View != null && datagrid.View.IsLegacyDataTable)
            {
                var table = datagrid.View is PagedCollectionView ?
                    (datagrid.View.SourceCollection as DataView).Table :
                    (datagrid.View as DataTableCollectionView).ViewSource.Table;
                if (table != null && table.Columns.Contains(propertyinfo.Name))
                    return table.Columns[propertyinfo.Name].AllowDBNull;
            }
#endif
            var nullablememberType = NullableHelperInternal.GetNullableType(propertyinfo.PropertyType);
            return propertyinfo.PropertyType.IsAssignableFrom(nullablememberType);
        }

#if WPF
        internal static bool GetHasError(DependencyObject obj)
        {
            if (VisualTreeHelper.GetChildrenCount(obj) == 0)
                return Validation.GetHasError(obj);
            else if (Validation.GetHasError(obj))
                return true;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var element = VisualTreeHelper.GetChild(obj, i) as DependencyObject;
                if (element != null)
                {
                    var haserror = Validation.GetHasError(element as DependencyObject);
                    if (haserror)
                        return true;
                    else
                    {
                        haserror = GetHasError(element);
                        if (haserror)
                            return true;                                    
                    }
                }
            }
            return false;
        }
#endif

        /// <summary>
        /// Updates the data row for the given row index.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// Specifies the row index to update the data row in view.
        /// </param>       
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// this.dataGrid.UpdateDataRow(1);
        /// ]]></code>
        /// </example>
        public static void UpdateDataRow(this SfDataGrid dataGrid, int rowIndex)
        {
            dataGrid.GridModel.UpdateDataRow(rowIndex);
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> of SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var visualContainer = this.dataGrid.GetVisualContainer();
        /// ]]></code>
        /// </example>
        public static VisualContainer GetVisualContainer(this SfDataGrid dataGrid)
        {
            return dataGrid.VisualContainer;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper"/> of SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var validationHelper = this.dataGrid.GetValidationHelper();
        /// ]]></code>
        /// </example>
        public static ValidationHelper GetValidationHelper(this SfDataGrid dataGrid)
        {
            return dataGrid.ValidationHelper;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> of SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var addNewRowController = this.dataGrid.GetAddNewRowController();
        /// ]]></code>
        /// </example>
        public static GridAddNewRowController GetAddNewRowController(this SfDataGrid dataGrid)
        {
            return dataGrid.GridModel.AddNewRowController;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> of SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var gridModel = this.dataGrid.GetGridModel();
        /// ]]></code>
        /// </example>
        public static GridModel GetGridModel(this SfDataGrid dataGrid)
        {
            return dataGrid.GridModel;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator"/> of SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var rowGenerator = this.dataGrid.GetRowGenerator();
        /// ]]></code>
        /// </example>
        public static RowGenerator GetRowGenerator(this SfDataGrid dataGrid)
        {
            return dataGrid.RowGenerator;
        }

        /// <summary>
        /// Gets the DetailsViewDataGrid for the specified record index and relation column.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="recordIndex">
        /// The record index to get the DetailsViewDataGrid.
        /// </param>
        /// <param name="relationalColumn">
        /// The name of relationalColumn to get the DetailsViewDataGrid.
        /// </param>
        /// <returns>
        /// Returns the DetailsViewDataGrid for the specified record index and relation column.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var detailsViewDataGrid = this.dataGrid.GetDetailsViewGrid(0, "ProductDetails");
        /// ]]></code>
        /// </example>
        public static SfDataGrid GetDetailsViewGrid(this SfDataGrid dataGrid, int recordIndex, string relationalColumn)
        {
            if (dataGrid.View == null)
                return null;

            RecordEntry record;
            if (dataGrid.GridModel.HasGroup)
                record = dataGrid.View.TopLevelGroup.DisplayElements[recordIndex] as RecordEntry;
            else
                record = dataGrid.View.Records[recordIndex];

            if (record != null && record.IsExpanded)
            {
                int rowIndex = dataGrid.ResolveToRowIndex(recordIndex);
                var gridView = dataGrid.DetailsViewDefinition.FirstOrDefault(detsilsview => detsilsview.RelationalColumn == relationalColumn);
                var gridViewIndex = dataGrid.DetailsViewDefinition.IndexOf(gridView);
                var actualRowIndex = rowIndex + gridViewIndex + 1;
                var row = dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(detailsrow => detailsrow.RowIndex == actualRowIndex);
                if (row != null)
                    return row.DetailsViewDataGrid;
            }
            return null;
        }

        /// <summary>
        /// Gets the source DataGrid for the specified DetailsViewDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The DetailsViewDataGrid to get its source DataGrid.
        /// </param>
        /// <returns>
        /// The source DataGrid for the specified DetailsViewDataGrid.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        ///   var detailsViewDataGrid = this.dataGrid.GetDetailsViewGrid(0, "ProductDetails");
        ///   var sourceDataGrid = (detailsViewDataGrid as DetailsViewDataGrid).GetSourceDataGrid();
        /// ]]></code>
        /// </example>
        public static SfDataGrid GetSourceDataGrid(this DetailsViewDataGrid dataGrid)
        {
            return dataGrid.NotifyListener != null ? dataGrid.NotifyListener.SourceDataGrid : null;
        }

        /// <summary>
        /// Updates <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundColumn"/> for the specified record and property name.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="record">
        /// The corresponding record to update unbound column.
        /// </param>
        /// <param name="propertyName">
        /// The corresponding property name to update unbound column.
        /// </param>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        ///    this.dataGrid.UpdateUnboundColumn(this.dataGrid.CurrentItem, "Discount");
        /// ]]></code>
        /// </example>
        public static void UpdateUnboundColumn(this SfDataGrid dataGrid, object record, string propertyName)
        {
            if (!dataGrid.HasUnboundColumns)
                return;

            if (dataGrid.RowGenerator.Items.Any(
                    row => row.VisibleColumns.Any(col => col.GridColumn != null && col.GridColumn.IsUnbound)))
            {
                var dataRowBase = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowData == record);
                if (dataRowBase != null)
                {
                    dataRowBase.VisibleColumns.ForEach(col =>
                    {
                        if (col.GridColumn != null && col.GridColumn.IsUnbound)
                        {
                            var column = col.GridColumn as GridUnBoundColumn;
                            if (column.Expression.ToLower().Contains(propertyName.ToLower()) ||
                                column.Format.ToLower().Contains(propertyName.ToLower()))
                            {
                                col.UpdateBinding(dataRowBase.RowData);
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Updates the unbound columns corresponding to the specified record in SfDataGrid,
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="record">
        /// The corresponding record to update the unbound columns in SfDataGrid.
        /// </param>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        ///    this.dataGrid.UpdateUnboundColumn(this.dataGrid.CurrentItem);
        /// ]]></code>
        /// </example>
        public static void UpdateUnboundColumns(this SfDataGrid dataGrid, object record)
        {
            if (!dataGrid.HasUnboundColumns)
                return;

            if (dataGrid.RowGenerator.Items.Any(
                    row => row.VisibleColumns.Any(col => col.GridColumn != null && col.GridColumn.IsUnbound)))
            {
                var dataRowBase = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowData == record);
                if (dataRowBase != null)
                {
                    dataRowBase.VisibleColumns.ForEach(col =>
                    {
                        if (col.GridColumn != null && col.GridColumn.IsUnbound)
                            col.UpdateBinding(dataRowBase.RowData);
                    });
                }
            }
        }

        /// <summary>
        /// Updates the UnboundColumn of the specified <see cref="Syncfusion.UI.Xaml.Grid.DataRowBase"/> and property name in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="dataRowBase">
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.DataRowBase"/> to update the UnboundColumn.
        /// </param>
        /// <param name="propertyName">
        /// The corresponding property name to update UnboundColumn in SfDataGrid.
        /// </param>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        ///     var dataRowBase = this.dataGrid.GetRowGenerator().Items.FirstOrDefault(row => row.RowData == this.dataGrid.CurrentItem);
        ///     this.dataGrid.UpdateUnboundColumn(dataRowBase, "Discount");
        /// ]]></code>
        /// </example>
        public static void UpdateUnboundColumn(this SfDataGrid dataGrid, DataRowBase dataRowBase, string propertyName)
        {
            if (!dataGrid.HasUnboundColumns || dataRowBase == null)
                return;

            if (dataGrid.RowGenerator.Items.Any(
                        row => row.VisibleColumns.Any(col => col.GridColumn != null && col.GridColumn.IsUnbound)))
            {
                dataRowBase.VisibleColumns.ForEach(col =>
                {
                    if (col.GridColumn != null && col.GridColumn.IsUnbound)
                    {
                        var column = col.GridColumn as GridUnBoundColumn;
                        if (column.Expression.ToLower().Contains(propertyName.ToLower()) ||
                            column.Format.ToLower().Contains(propertyName.ToLower()))
                        {
                            col.UpdateBinding(dataRowBase.RowData);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Refreshes the column count, width in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        public static void RefreshColumns(this SfDataGrid dataGrid)
        {
            if (dataGrid.VisualContainer == null)
                return;
            // Update column count
            dataGrid.UpdateColumnCount(false);

            // Update Indent column widths
            dataGrid.UpdateIndentColumnWidths();

            // Freeze columns updated when adding and removing columns
            dataGrid.UpdateFreezePaneColumns();
            // Update the scroll bars
            dataGrid.VisualContainer.UpdateScrollBars();
            dataGrid.VisualContainer.NeedToRefreshColumn = true;
            dataGrid.VisualContainer.InvalidateMeasureInfo();

            if (dataGrid.VisualContainer.ColumnCount > 0)
            {
                // Refresh the StackedHeaders
                dataGrid.RowGenerator.RefreshStackedHeaders();
                // Refresh Column sizer
                dataGrid.GridColumnSizer.Refresh();
            }
            //To refresh detailsview data row while changing column collection at runtime             
            dataGrid.RowGenerator.LineSizeChanged();
        }
    }

    internal static class HelperExtensions
    {
#if !WPF
        internal static Point GetPosition(this PointerRoutedEventArgs e, UIElement relativeTo)
        {
            return e.GetCurrentPoint(relativeTo).Position;
        }
#endif

        internal static Rect GetControlRect(this FrameworkElement control, UIElement relativeTo)
        {
#if !WPF
            var point = control.TransformToVisual(relativeTo).TransformPoint(new Point(0, 0));
#else
            var point = control.TranslatePoint(new Point(0, 0), relativeTo);
#endif
            var rect = new Rect(point.X, point.Y, control.ActualWidth, control.ActualHeight);
            return rect;
        }
    }

    /// <summary>
    /// Provides classes that simplify programming by providing readymade solution 
    /// for certain functionalities of SfDataGrid.
    /// </summary>
    class NamespaceDoc
    {

    }
}
