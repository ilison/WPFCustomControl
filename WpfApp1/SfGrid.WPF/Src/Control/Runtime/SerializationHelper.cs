#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Linq;
using System.Reflection;
using System.Windows;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using System.Collections.Specialized;
using System.Collections.Generic;
#if UWP
using Windows.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
using System.IO;
#else
using System.Windows.Media;
using Syncfusion.Windows.Shared;
using System.Windows.Controls;
using Syncfusion.Windows.Tools.Controls;
using System.IO;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a controller that is used to perform serialize and deserialize operations in SfDataGrid using DataContractSerializer.
    /// </summary>
    public class SerializationController
    {
        /// <summary>
        /// Gets the reference to the SfDataGrid control.
        /// </summary>
        public SfDataGrid Datagrid { get; internal set; }
        private bool isdisposed = false;
        private List<GridColumn> cachedColumns;


        //#if !SILVERLIGHT
        //        public static object CreateSerializableInstance(object source, Type targettype)
        //        {
        //            var target = Activator.CreateInstance(targettype);
        //#if WinRT || UNIVERSAL
        //            var sourceproperty = source.GetType().GetRuntimeProperties();
        //            var targetproperty = target.GetType().GetRuntimeProperties();
        //#else
        //            var sourceproperty = source.GetType().GetProperties();
        //            var targetproperty = target.GetType().GetProperties();
        //#endif

        //            foreach (var targetpropertyInfo in targetproperty)
        //            {
        //                if (!targetpropertyInfo.CanWrite) continue;
        //                var sourcepropertyInfo = sourceproperty.FirstOrDefault(property => property.Name.Equals(targetpropertyInfo.Name));
        //                if (sourcepropertyInfo != null && sourcepropertyInfo.CanWrite)
        //                {
        //#if WinRT || UNIVERSAL
        //                    if (sourcepropertyInfo.PropertyType.GetTypeInfo().IsSerializable)
        //                    {
        //                        var value = sourcepropertyInfo.GetValue(source);
        //                        if (typeof (IList).GetTypeInfo().IsAssignableFrom(sourcepropertyInfo.PropertyType.GetTypeInfo()))
        //                        {
        //                            var list = value as IList;
        //                            var result = Activator.CreateInstance(targetpropertyInfo.PropertyType);
        //                            var addmethodinfo = typeof(IList).GetTypeInfo().DeclaredMethods.FirstOrDefault(method => method.Name.Equals("Add"));
        //#else
        //                    if (sourcepropertyInfo.PropertyType.IsSerializable)
        //                    {
        //                        var value = sourcepropertyInfo.GetValue(source, null);
        //                        if (typeof(IList).IsAssignableFrom(sourcepropertyInfo.PropertyType))
        //                        {
        //                            var list = value as IList;
        //                            var result = Activator.CreateInstance(targetpropertyInfo.PropertyType);
        //                            var addmethodinfo = typeof(IList).GetMethods().FirstOrDefault(method => method.Name.Equals("Add"));
        //#endif
        //                            foreach (var items in list)
        //                            {
        //                                var type = Type.GetType(string.Format("Syncfusion.UI.Xaml.Grid.Serializable{0}", items.GetType().Name)) ?? items.GetType();
        //                                var serializableitem = CreateSerializableInstance(items, type);
        //                                addmethodinfo.Invoke(result, new[] { serializableitem });
        //                            }
        //                            targetpropertyInfo.SetValue(target, result, null);
        //                        }
        //                        else
        //                            targetpropertyInfo.SetValue(target, value, null);
        //                    }
        //                    else
        //                    {
        //                        var value = sourcepropertyInfo.GetValue(source, null);
        //                        if (value == null)
        //                        {
        //                            targetpropertyInfo.SetValue(target, null, null);
        //                            continue;
        //                        }
        //                        var result = CreateSerializableInstance(value, targetpropertyInfo.PropertyType);
        //#if WinRT || UNIVERSAL
        //                        if (result != null && (typeof (IList).GetTypeInfo().IsAssignableFrom(sourcepropertyInfo.PropertyType.GetTypeInfo())))
        //                        {
        //                            var list = value as IList;
        //                            var addmethodinfo = typeof(IList).GetTypeInfo().DeclaredMethods.FirstOrDefault(method => method.Name.Equals("Add"));
        //#else
        //                        if (result != null && (typeof(IList).IsAssignableFrom(sourcepropertyInfo.PropertyType)))
        //                        {
        //                            var list = value as IList;
        //                            var addmethodinfo = typeof(IList).GetMethods().FirstOrDefault(method => method.Name.Equals("Add"));
        //#endif
        //                            foreach (var items in list)
        //                            {
        //                                var type = Type.GetType(string.Format("Syncfusion.UI.Xaml.Grid.Serializable{0}", items.GetType().Name));
        //                                var serializableitem = CreateSerializableInstance(items, type);
        //                                addmethodinfo.Invoke(result, new[] { serializableitem });
        //                            }
        //                        }
        //                        targetpropertyInfo.SetValue(target, result, null);
        //                    }
        //                }
        //            }
        //            return target;
        //        }

        //                public static void UpdateValueFromDeSerializableInstance(object source, object target)
        //                {
        //#if WinRT || UNIVERSAL
        //                    var sourceproperty = source.GetType().GetRuntimeProperties();
        //                    var targetproperty = target.GetType().GetRuntimeProperties();
        //#else
        //                    var sourceproperty = source.GetType().GetProperties();
        //                    var targetproperty = target.GetType().GetProperties();
        //#endif

        //                    foreach (var sourcepropertyInfo in sourceproperty)
        //                    {
        //                        var targetpropertyInfo = targetproperty.FirstOrDefault(property => property.Name == sourcepropertyInfo.Name);
        //                        if (!targetpropertyInfo.CanWrite) continue;
        //#if WinRT || UNIVERSAL
        //                        if (targetpropertyInfo.PropertyType.GetTypeInfo().IsSerializable)
        //                        {
        //                            var value = sourcepropertyInfo.GetValue(source);
        //                            if (typeof (IList).GetTypeInfo().IsAssignableFrom(targetpropertyInfo.PropertyType.GetTypeInfo()))
        //                            {
        //                                var list = value as IList;
        //                                var result = Activator.CreateInstance(targetpropertyInfo.PropertyType);
        //                                var addmethodinfo = typeof (IList).GetTypeInfo().DeclaredMethods.FirstOrDefault(method => method.Name.Equals("Add"));
        //#else
        //                        if (targetpropertyInfo.PropertyType.IsSerializable)
        //                        {
        //                            var value = sourcepropertyInfo.GetValue(source, null);
        //                            if (typeof(IList).IsAssignableFrom(targetpropertyInfo.PropertyType))
        //                            {
        //                                var list = value as IList;
        //                                var result = Activator.CreateInstance(targetpropertyInfo.PropertyType);
        //                                var addmethodinfo = typeof(IList).GetMethods().FirstOrDefault(method => method.Name.Equals("Add"));

        //#endif
        //                                foreach (var items in list)
        //                                {
        //                                    var name = items.GetType().FullName.Replace("Serializable", "");
        //                                    var newitem = Activator.CreateInstance(Type.GetType(name));
        //                                    UpdateValueFromDeSerializableInstance(items, newitem);
        //                                    addmethodinfo.Invoke(result, new[] { newitem });
        //                                }
        //                                targetpropertyInfo.SetValue(target, result, null);
        //                            }
        //                            else
        //                                targetpropertyInfo.SetValue(target, value, null);
        //                        }
        //                        else
        //                        {
        //                            var value = sourcepropertyInfo.GetValue(source, null);
        //                            if (value == null)
        //                            {
        //                                targetpropertyInfo.SetValue(target, null, null);
        //                                continue;
        //                            }
        //                            var result = Activator.CreateInstance(targetpropertyInfo.PropertyType);
        //                            UpdateValueFromDeSerializableInstance(value, result);
        //#if WinRT || UNIVERSAL
        //                            if (result != null && (typeof(IList).GetTypeInfo().IsAssignableFrom(targetpropertyInfo.PropertyType.GetTypeInfo())))
        //                            {                        
        //                                var list = value as IList;
        //                                var addmethodinfo = typeof(IList).GetTypeInfo().DeclaredMethods.FirstOrDefault(method => method.Name.Equals("Add"));
        //#else
        //                            if (result != null && (typeof(IList).IsAssignableFrom(targetpropertyInfo.PropertyType)))
        //                            {
        //                                var list = value as IList;
        //                                var addmethodinfo = typeof(IList).GetMethods().FirstOrDefault(method => method.Name.Equals("Add"));

        //#endif
        //                                foreach (var items in list)
        //                                {
        //                                    var name = items.GetType().FullName.Replace("Serializable", "");
        //                                    var newitem = Activator.CreateInstance(Type.GetType(name));
        //                                    UpdateValueFromDeSerializableInstance(items, newitem);
        //                                    addmethodinfo.Invoke(result, new[] { newitem });
        //                                }
        //                            }
        //                            targetpropertyInfo.SetValue(target, result, null);
        //                        }
        //                    }
        //                }
        //#endif

        #region Construtor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/> class.
        /// </summary>
        /// <param name="grid">
        /// The SfDataGrid.
        /// </param>
        public SerializationController(SfDataGrid grid)
        {
            this.Datagrid = grid;
        }
        #endregion

        #region Serialization

        #region Public Virtual Methods
        /// <summary>
        /// Serializes the SfDataGrid to the given XML stream based on the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/>.
        /// </summary>
        /// <param name="stream">
        /// Specifies stream used to write XML document file.
        /// </param>
        /// <param name="serializeOptions">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> to decide the type of operations such as sorting ,filtering ,and etc to be serialized.
        /// </param>
        public virtual void Serialize(Stream stream, SerializationOptions serializeOptions)
        {
            SerializableGridColumn.DataGrid = this.Datagrid;
            var serializablegrid = StoreGridSettings(serializeOptions);
            var serializer = new DataContractSerializer(typeof(SerializableDataGrid));
            serializer.WriteObject(stream, serializablegrid);
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
                this.Datagrid = null;
            isdisposed = true;
        }

        /// <summary>
        /// Gets the known column types to serialize and deserialize the columns in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the corresponding column type.
        /// </returns>
        /// <remarks>
        ///  If you want to serialize the custom columns, its types should be added in the returned Type Array.
        /// </remarks>
        public virtual Type[] KnownTypes()
        {
            return new Type[] { typeof(SerializableGridTextColumn),typeof(SerializableGridUnBoundColumn),typeof(SerializableGridTemplateColumn),typeof(SerializableGridComboBoxColumn),typeof(SerializableGridCheckBoxColumn),typeof(SerializableGridHyperlinkColumn), typeof(SerializableGridNumericColumn),typeof(SerializableGridDateTimeColumn),
                typeof(SerializableGridColumn),typeof(SerializableGridEditorColumn),typeof(SerializableGridMaskColumn),
                typeof(SerializableGridMultiColumnDropDownList),
#if WPF
            typeof(SerializableGridTimeSpanColumn), typeof(SerializableGridCurrencyColumn), typeof(SerializableGridPercentageColumn),
#else
            typeof(SerializableGridUpDownColumn)
#endif
            };
        }

        /// <summary>
        /// Stores the SfDataGrid settings with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> to <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/>.
        /// </summary>
        /// <param name="serializeOptions">
        /// Specifies the serialization options to store grid settings.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with grid settings.
        /// </returns>
        protected virtual SerializableDataGrid StoreGridSettings(SerializationOptions serializeOptions)
        {
            var serializableDataGrid = GetSerializableDataGrid();
            StoreGridProperties(serializableDataGrid);
            serializableDataGrid.Columns = StoreGridColumns(this.Datagrid, serializeOptions);
            serializableDataGrid.FilterSettings = StoreFilterPredicates(this.Datagrid, serializeOptions);
            serializableDataGrid.SortColumnDescriptions = StoreSortColumnDescriptions(this.Datagrid, serializeOptions);
            serializableDataGrid.GroupColumnDescriptions = StoreGroupColumnDescriptions(this.Datagrid, serializeOptions);
            StoreGridGroupSummaryRows(serializableDataGrid, this.Datagrid.GroupSummaryRows, serializeOptions);

            if (Datagrid.CaptionSummaryRow != null)
                StoreGridCaptionSummaryRow(serializableDataGrid, Datagrid.CaptionSummaryRow, serializeOptions);

            StoreGridTableSummaryRows(serializableDataGrid, this.Datagrid.TableSummaryRows, serializeOptions);
            StoreGridStackedHeaderRow(serializableDataGrid, this.Datagrid.StackedHeaderRows, serializeOptions);
            StoreGridUnBoundRows(serializableDataGrid, this.Datagrid.UnBoundRows, serializeOptions);
            StoreDetailsViewDefinition(serializableDataGrid, this.Datagrid.DetailsViewDefinition, serializeOptions);

            return serializableDataGrid;
        }

        /// <summary>
        /// Gets the new instance of <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> while serializing the SfDataGrid.
        /// </summary>
        /// <returns>
        /// The new instance of <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/>.
        /// </returns>
        /// <remarks>
        /// If you need to serialize the custom SfDataGrid properties, you should add these properties to custom SerializableDataGrid class.
        /// </remarks>
        protected virtual SerializableDataGrid GetSerializableDataGrid()
        {
            return new SerializableDataGrid();
        }      

        /// <summary>
        /// Stores the SfDataGrid properties during serialization process.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> to store SfDataGrid properties settings.
        /// </param>
        protected virtual void StoreGridProperties(SerializableDataGrid serializableDataGrid)
        {
            serializableDataGrid.AllowDraggingColumns = Datagrid.AllowDraggingColumns;
            serializableDataGrid.AllowEditing = Datagrid.AllowEditing;
            serializableDataGrid.AllowFiltering = Datagrid.AllowFiltering;
            serializableDataGrid.CurrentCellBorderThickness = Datagrid.CurrentCellBorderThickness;
            serializableDataGrid.EditTrigger = Datagrid.EditTrigger;
            serializableDataGrid.AllowFrozenGroupHeaders = Datagrid.AllowFrozenGroupHeaders;
            serializableDataGrid.AllowGrouping = Datagrid.AllowGrouping;
            serializableDataGrid.AllowResizingColumns = Datagrid.AllowResizingColumns;
            serializableDataGrid.AllowResizingHiddenColumns = Datagrid.AllowResizingHiddenColumns;
            serializableDataGrid.AllowSelectionOnPointerPressed = Datagrid.AllowSelectionOnPointerPressed;
            serializableDataGrid.AllowSort = Datagrid.AllowSorting;
            serializableDataGrid.AllowTriStateSorting = Datagrid.AllowTriStateSorting;
            serializableDataGrid.AutoExpandGroups = Datagrid.AutoExpandGroups;
            serializableDataGrid.AutoGenerateColumns = Datagrid.AutoGenerateColumns;
            serializableDataGrid.AutoGenerateColumnsMode = Datagrid.AutoGenerateColumnsMode;
            serializableDataGrid.AutoGenerateRelations = Datagrid.AutoGenerateRelations;
            serializableDataGrid.ColumnSizer = Datagrid.ColumnSizer;
            serializableDataGrid.FrozenColumnCount = Datagrid.FrozenColumnCount;
            serializableDataGrid.FrozenRowsCount = Datagrid.FrozenRowsCount;
            serializableDataGrid.FooterColumnCount = Datagrid.FooterColumnCount;
            serializableDataGrid.FooterRowsCount = Datagrid.FooterRowsCount;
            serializableDataGrid.GroupCaptionTextFormat = Datagrid.GroupCaptionTextFormat;
            serializableDataGrid.GroupDropAreaText = Datagrid.GroupDropAreaText;
            serializableDataGrid.HeaderRowHeight = Datagrid.HeaderRowHeight;
            serializableDataGrid.IsGroupDropAreaExpanded = Datagrid.IsGroupDropAreaExpanded;
            serializableDataGrid.LiveDataUpdateMode = Datagrid.LiveDataUpdateMode;
            serializableDataGrid.RowHeight = Datagrid.RowHeight;
            serializableDataGrid.SelectionMode = Datagrid.SelectionMode;
            serializableDataGrid.SelectionUnit = Datagrid.SelectionUnit;
            serializableDataGrid.ShowColumnWhenGrouped = Datagrid.ShowColumnWhenGrouped;
            serializableDataGrid.ShowGroupDropArea = Datagrid.ShowGroupDropArea;
            serializableDataGrid.ShowSortNumbers = Datagrid.ShowSortNumbers;
            serializableDataGrid.SortClickAction = Datagrid.SortClickAction;
            serializableDataGrid.Columns = new SerializableColumns();
            serializableDataGrid.SortColumnDescriptions = new SerializableSortColumnDescriptions();
            serializableDataGrid.GroupColumnDescriptions = new SerializableGroupColumnDescriptions();
            serializableDataGrid.GroupSummaryRows = new SerializableGridSummaryRows();
            serializableDataGrid.TableSummaryRows = new SerializableTableGridSummaryRows();
            serializableDataGrid.StackedHeaderRows = new SerializableStackedHeaderRows();
            serializableDataGrid.UnBoundRows = new SerializableGridUnBoundRows();
            serializableDataGrid.AddNewRowPosition = Datagrid.AddNewRowPosition;
            serializableDataGrid.FilterRowPosition = Datagrid.FilterRowPosition;
            serializableDataGrid.AllowDeleting = Datagrid.AllowDeleting;
            serializableDataGrid.AllowRowHoverHighlighting = Datagrid.AllowRowHoverHighlighting;
            serializableDataGrid.DataFetchSize = Datagrid.DataFetchSize;
            serializableDataGrid.GridValidationMode = Datagrid.GridValidationMode;
            serializableDataGrid.NavigationMode = Datagrid.NavigationMode;
            serializableDataGrid.ShowRowHeader = Datagrid.ShowRowHeader;
            serializableDataGrid.EditorSelectionBehavior = Datagrid.EditorSelectionBehavior;
            serializableDataGrid.UsePLINQ = Datagrid.UsePLINQ;
            serializableDataGrid.ShowBusyIndicator = Datagrid.ShowBusyIndicator;
            serializableDataGrid.DetailsViewPadding = Datagrid.DetailsViewPadding;
            serializableDataGrid.ReuseRowsOnItemssourceChange = Datagrid.ReuseRowsOnItemssourceChange;
            serializableDataGrid.IsDynamicItemsSource = Datagrid.IsDynamicItemsSource;
            serializableDataGrid.RowHeaderWidth = Datagrid.RowHeaderWidth;
            serializableDataGrid.GridCopyOption = Datagrid.GridCopyOption;
            serializableDataGrid.GridPasteOption = Datagrid.GridPasteOption;
            serializableDataGrid.IndentColumnWidth = Datagrid.IndentColumnWidth;
            serializableDataGrid.ExpanderColumnWidth = Datagrid.ExpanderColumnWidth;
        }

        /// <summary>
        /// Stores the specified <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> properties to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridSummaryRow"/> during serialization process. 
        /// </summary>
        /// <param name="gridSummaryRow">
        /// The corresponding grid summary row to store its settings.
        /// </param>
        /// <param name="serializableSummaryRow">
        /// The corresponding SerializableGridSummaryRow to store the settings of grid summary row.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridSummaryRow"/> contains the GridSummaryRow settings for serialization.
        /// </returns>
        protected virtual SerializableGridSummaryRow StoreGridSummaryRow(GridSummaryRow gridSummaryRow, SerializableGridSummaryRow serializableSummaryRow = null)
        {
            if (serializableSummaryRow == null)
            {
                serializableSummaryRow = new SerializableGridSummaryRow();
            }

            serializableSummaryRow.Name = gridSummaryRow.Name;
            serializableSummaryRow.ShowSummaryInRow = gridSummaryRow.ShowSummaryInRow;
            serializableSummaryRow.SummaryColumns = new ObservableCollection<SerializableGridSummaryColumn>();
            serializableSummaryRow.Title = gridSummaryRow.Title;

            foreach (var summaryColumn in gridSummaryRow.SummaryColumns)
            {
                var column = new SerializableGridSummaryColumn
                {
                    Name = summaryColumn.Name,
                    MappingName = summaryColumn.MappingName,
                    Format = summaryColumn.Format,
                    SummaryType = summaryColumn.SummaryType
                };
                serializableSummaryRow.SummaryColumns.Add(column);
            }
            return serializableSummaryRow;
        }
       
        /// <summary>
        /// Stores the specified caption summary row property settings to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process. 
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to store the caption summary row property settings for serialization.
        /// </param>
        /// <param name="gridSummaryRow">
        /// The corresponding GridSummaryRow to store its settings.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGridGrid"/> that contains the specified property settings of caption summary row.
        /// </returns>
        protected virtual void StoreGridCaptionSummaryRow(SerializableDataGrid serializableDataGrid, GridSummaryRow gridSummaryRow, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeCaptionSummary)
                return;

            var summaryRow = new SerializableGridSummaryRow();
            serializableDataGrid.CaptionSummaryRow = gridSummaryRow != null ? (SerializableGridSummaryRow)StoreGridSummaryRow(gridSummaryRow, summaryRow) : summaryRow;
        }

        /// <summary>
        /// Stores the property settings of group summary rows to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process. 
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to store the group summary rows property settings.
        /// </param>
        /// <param name="GroupSummaryrows">
        /// The collection of grid summary rows that need to be stored in to serializableDataGrid for serializing group summary rows.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGridGrid"/> that contains the specified property settings of group summary rows for serialization.
        /// </returns>
        protected virtual void StoreGridGroupSummaryRows(SerializableDataGrid serializableDataGrid, ObservableCollection<GridSummaryRow> GroupSummaryrows, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeGroupSummaries)
                return;

            if (this.Datagrid.GroupSummaryRows != null)
            {
                foreach (var groupSummaryRow in this.Datagrid.GroupSummaryRows)
                {
                    var summaryRow = new SerializableGridSummaryRow();
                    summaryRow = (SerializableGridSummaryRow)StoreGridSummaryRow(groupSummaryRow, summaryRow);
                    serializableDataGrid.GroupSummaryRows.Add(summaryRow);
                }
            }
        }

        /// <summary>
        /// Stores the property settings of table summary rows to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process. 
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to store the table summary rows settings.
        /// </param>
        /// <param name="tableSummaryRows">
        /// The collection of table summary rows that need to be stored in to SerializableDataGrid for serializing table summary rows.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGridGrid"/> that contains the specified property settings of table summary rows for serialization.
        /// </returns>
        protected virtual void StoreGridTableSummaryRows(SerializableDataGrid serializableDataGrid, ObservableCollection<GridSummaryRow> tableSummaryRows, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeTableSummaries)
                return;

            if (this.Datagrid.TableSummaryRows != null)
            {
                foreach (var tableSummaryRow in tableSummaryRows)
                {
                    var summaryRow = new SerializableGridTableSummaryRow();
                    StoreGridSummaryRow(tableSummaryRow, summaryRow);
                    if (tableSummaryRow is GridTableSummaryRow)
                    {
                        summaryRow.Position = (tableSummaryRow as GridTableSummaryRow).Position;
                    }
                    serializableDataGrid.TableSummaryRows.Add(summaryRow);
                }
            }
        }
       
        /// <summary>
        /// Stores the property settings of unbound rows to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process. 
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializableDataGrid to store the unbound rows settings.
        /// </param>
        /// <param name="gridUnBoundRows">
        /// The corresponding unbound rows that need to be stored in to SerializableDataGrid for serializing unbound rows.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        protected virtual void StoreGridUnBoundRows(SerializableDataGrid serializableDataGrid, UnBoundRows gridUnBoundRows, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeUnBoundRows)
                return;

            if (this.Datagrid.UnBoundRows != null)
            {
                foreach (var gridUnBoundRow in this.Datagrid.UnBoundRows)
                {
                    var unBoundRow = new SerializableGridUnBoundRow();
                    unBoundRow.Position = gridUnBoundRow.Position;
                    unBoundRow.RowIndex = gridUnBoundRow.RowIndex;
                    unBoundRow.ShowBelowSummary = gridUnBoundRow.ShowBelowSummary;
                    unBoundRow.UnBoundRowIndex = gridUnBoundRow.UnBoundRowIndex;
                    serializableDataGrid.UnBoundRows.Add(unBoundRow);
                }
            }
        }
       
        /// <summary>
        /// Stores the specified DetailsViewDefinition settings to <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to store the DetailsViewDefinition settings.
        /// </param>
        /// <param name="detailsViewDefinition">
        /// The corresponding detailsViewDefinition that need to be stored in to serializableDataGrid for serializing DetailsViewDefinition.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        protected virtual void StoreDetailsViewDefinition(SerializableDataGrid serializableDataGrid, DetailsViewDefinition detailsViewDefinition, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeDetailsViewDefinition)
                return;

            serializableDataGrid.DetailsViewDefinition = new SerializableDetailsViewDefinition();
            if (detailsViewDefinition != null && detailsViewDefinition.Any())
            {
                foreach (var defintion in detailsViewDefinition)
                {
                    var serializableGridViewDefinition = new SerializableGridViewDefinition();
                    serializableGridViewDefinition.DataGrid = (defintion as GridViewDefinition).DataGrid.SerializationController.StoreGridSettings(serializeOptions);
                    serializableGridViewDefinition.RelationalColumn = defintion.RelationalColumn;
                    serializableDataGrid.DetailsViewDefinition.Add(serializableGridViewDefinition);
                }
            }
        }
        
        /// <summary>
        /// Stores the columns settings of SfDataGrid to <see cref="Syncfusion.UI.Xaml.Grid.SerializableColumns"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding dataGrid to store its columns settings for serialization.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableColumns"/> that contains the specified property settings of columns.
        /// </returns>
        protected virtual SerializableColumns StoreGridColumns(SfDataGrid dataGrid, SerializationOptions serializeOptions)
        {
            var columns = new SerializableColumns();
            if (!serializeOptions.SerializeColumns)
                return columns;

            foreach (var column in dataGrid.Columns)
            {
                SerializableGridColumn serializableColumn = GetSerializableGridColumn(column);
                StoreGridColumnProperties(column, serializableColumn);
                if (serializeOptions.SerializeFiltering)
                    serializableColumn.FilterRowText = column.FilterRowText;
                columns.Add(serializableColumn);
            }
            return columns;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridColumn"/> instance while serializing the SfDataGrid.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get serializable grid column.
        /// </param>
        /// <returns>Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridColumn"/> instance for the specified grid column </returns>
        /// <remarks>
        /// You can use this method while serializing the custom column.
        /// </remarks>
        protected virtual SerializableGridColumn GetSerializableGridColumn(GridColumn column)
        {
            SerializableGridColumn serializableColumn;
            //WPF-23148 (issue 1) Hyperlink column is derived from GridTextColumn so need to check type of the column instead of checking like column is GridTextColumn.
            if (column.GetType().Equals(typeof(GridTextColumn)))
            {
                var textColumn = column as GridTextColumn;
                serializableColumn = new SerializableGridTextColumn
                {
                    TextWrapping = textColumn.TextWrapping,
#if UWP
                    IsSpellCheckEnabled = textColumn.IsSpellCheckEnabled,
#endif
                };
            }
            else if (column is GridComboBoxColumn)
            {
                var comboBoxColumn = column as GridComboBoxColumn;
                serializableColumn = new SerializableGridComboBoxColumn()
                {
                    SelectedValuePath = comboBoxColumn.SelectedValuePath,
                    DisplayMemberPath = comboBoxColumn.DisplayMemberPath,
#if WPF
                    StaysOpenOnEdit = comboBoxColumn.StaysOpenOnEdit,
                    IsEditable = comboBoxColumn.IsEditable
#endif
                };
            }
            else if (column is GridCheckBoxColumn)
            {
                var checkBoxColumn = column as GridCheckBoxColumn;
                serializableColumn = new SerializableGridCheckBoxColumn()
                {
                    HorizontalAlignment = checkBoxColumn.HorizontalAlignment,
                    VerticalAlignment = checkBoxColumn.VerticalAlignment,
                    IsThreeState = checkBoxColumn.IsThreeState
                };
            }
            else if (column is GridMultiColumnDropDownList)
            {
                var multiColumn = column as GridMultiColumnDropDownList;
                serializableColumn = new SerializableGridMultiColumnDropDownList()
                {
                    DisplayMember = multiColumn.DisplayMember,
                    ValueMember = multiColumn.ValueMember,
                    ShowResizeThumb = multiColumn.ShowResizeThumb,
                    PopUpHeight = multiColumn.PopUpHeight,
                    PopUpWidth = multiColumn.PopUpWidth,
                    AllowAutoComplete = multiColumn.AllowAutoComplete,
                    AllowSpinOnMouseWheel = multiColumn.AllowSpinOnMouseWheel,
                    AllowIncrementalFiltering = multiColumn.AllowIncrementalFiltering,
                    SearchCondition = multiColumn.SearchCondition,
                    AllowCasingforFilter = multiColumn.AllowCasingforFilter,
                    PopUpMaxHeight = multiColumn.PopUpMaxHeight,
                    PopUpMaxWidth = multiColumn.PopUpMaxWidth,
                    PopUpMinHeight = multiColumn.PopUpMinHeight,
                    PopUpMinWidth = multiColumn.PopUpMinWidth,
                    IsTextReadOnly = multiColumn.IsTextReadOnly,
                    AllowNullInput = multiColumn.AllowNullInput,
                    AutoGenerateColumns = multiColumn.AutoGenerateColumns,
                    GridColumnSizer = multiColumn.GridColumnSizer,
                    IsAutoPopupSize = multiColumn.IsAutoPopupSize,
                };
            }
#if UWP
            else if (column is GridUpDownColumn)
            {
                var upDownColumn = column as GridUpDownColumn;
                serializableColumn = new SerializableGridUpDownColumn()
                    {
                        SmallChange = upDownColumn.SmallChange,
                        MinValue = upDownColumn.MinValue,
                        MaxValue = upDownColumn.MaxValue,
                        NumberDecimalDigits = upDownColumn.NumberDecimalDigits,
                        AutoReverse = upDownColumn.AutoReverse,
                        ParsingMode = upDownColumn.ParsingMode,
                    };
            }
#else
            else if (column is GridCurrencyColumn)
            {
                var currencyColumn = column as GridCurrencyColumn;
                serializableColumn = new SerializableGridCurrencyColumn()
                {
                    AllowNullValue = currencyColumn.AllowNullValue,
                    AllowScrollingOnCircle = currencyColumn.AllowScrollingOnCircle,
                    CurrencyDecimalDigits = currencyColumn.CurrencyDecimalDigits,
                    CurrencyDecimalSeparator = currencyColumn.CurrencyDecimalSeparator,
                    CurrencyGroupSeparator = currencyColumn.CurrencyGroupSeparator,
                    CurrencyGroupSizes = currencyColumn.CurrencyGroupSizes,
                    CurrencyNegativePattern = currencyColumn.CurrencyNegativePattern,
                    CurrencyPositivePattern = currencyColumn.CurrencyPositivePattern,
                    CurrencySymbol = currencyColumn.CurrencySymbol,
                    MaxValue = currencyColumn.MinValue,
                    MinValue = currencyColumn.MaxValue,
                    NullValue = currencyColumn.NullValue,
                    NullText = currencyColumn.NullText,
                    MaxValidation = currencyColumn.MaxValidation,
                    MinValidation = currencyColumn.MinValidation,
                };
            }
            else if (column is GridPercentColumn)
            {
                var percentageColumn = column as GridPercentColumn;
                serializableColumn = new SerializableGridPercentageColumn()
                {
                    AllowNullValue = percentageColumn.AllowNullValue,
                    AllowScrollingOnCircle = percentageColumn.AllowScrollingOnCircle,
                    PercentDecimalDigits = percentageColumn.PercentDecimalDigits,
                    PercentDecimalSeparator = percentageColumn.PercentDecimalSeparator,
                    PercentGroupSeparator = percentageColumn.PercentGroupSeparator,
                    PercentGroupSizes = percentageColumn.PercentGroupSizes,
                    PercentNegativePattern = percentageColumn.PercentNegativePattern,
                    PercentPositivePattern = percentageColumn.PercentPositivePattern,
                    PercentSymbol = percentageColumn.PercentSymbol,
                    MaxValue = percentageColumn.MinValue,
                    MinValue = percentageColumn.MaxValue,
                    PercentEditMode = percentageColumn.PercentEditMode,
                    NullValue = percentageColumn.NullValue,
                    NullText = percentageColumn.NullText,
                    MaxValidation = percentageColumn.MaxValidation,
                    MinValidation = percentageColumn.MinValidation,
                };
            }
            else if (column is GridTimeSpanColumn)
            {
                var timeSpanColumn = column as GridTimeSpanColumn;
                serializableColumn = new SerializableGridTimeSpanColumn()
                {
                    AllowNull = timeSpanColumn.AllowNull,
                    AllowScrollingOnCircle = timeSpanColumn.AllowScrollingOnCircle,
                    NullText = timeSpanColumn.NullText,
                    Format = timeSpanColumn.Format,
                    ShowArrowButtons = timeSpanColumn.ShowArrowButtons,
                    MaxValue = timeSpanColumn.MaxValue,
                    MinValue = timeSpanColumn.MinValue,
                };
            }
            else if (column is GridMaskColumn)
            {
                var maskColumn = column as GridMaskColumn;
                serializableColumn = new SerializableGridMaskColumn()
                {
                    SelectTextOnFocus = maskColumn.SelectTextOnFocus,
                    IsNumeric = maskColumn.IsNumeric,
                    TimeSeparator = maskColumn.TimeSeparator,
                    DateSeparator = maskColumn.DateSeparator,
                    DecimalSeparator = maskColumn.DecimalSeparator,
                    Mask = maskColumn.Mask,
                    MaskFormat = maskColumn.MaskFormat,
                    PromptChar = maskColumn.PromptChar,
                };
            }
#endif
            else if (column is GridDateTimeColumn)
            {
                var dateTimeColumn = column as GridDateTimeColumn;
                serializableColumn = new SerializableGridDateTimeColumn()
                {
#if UWP
                    AllowInlineEditing = dateTimeColumn.AllowInlineEditing,
                    FormatString = dateTimeColumn.FormatString,
                    ShowDropDownButton = dateTimeColumn.ShowDropDownButton,
#else
                    AllowScrollingOnCircle = dateTimeColumn.AllowScrollingOnCircle,
                    AllowNullValue = dateTimeColumn.AllowNullValue,
                    EnableClassicStyle = dateTimeColumn.EnableClassicStyle,
                    DisableDateSelection = dateTimeColumn.DisableDateSelection,
                    ShowRepeatButton = dateTimeColumn.ShowRepeatButton,
                    NullValue = dateTimeColumn.NullValue,
                    NullText = dateTimeColumn.NullText,
                    DateTimeFormat = dateTimeColumn.DateTimeFormat,
                    CanEdit = dateTimeColumn.CanEdit,
                    EnableBackspaceKey = dateTimeColumn.EnableBackspaceKey,
                    EnableDeleteKey = dateTimeColumn.EnableDeleteKey,
                    Pattern = dateTimeColumn.Pattern,
                    CustomPattern = dateTimeColumn.CustomPattern
#endif
                };
            }
            else if (column is GridHyperlinkColumn)
            {
                var hyperlinkColumn = column as GridHyperlinkColumn;
                serializableColumn = new SerializableGridHyperlinkColumn()
                {
                    HorizontalAlignment = hyperlinkColumn.HorizontalAlignment,
                    VerticalAlignment = hyperlinkColumn.VerticalAlignment,
                };
            }
            else if (column is GridNumericColumn)
            {
                var numericColumn = column as GridNumericColumn;
                serializableColumn = new SerializableGridNumericColumn()
                {
#if UWP
                    BlockCharactersOnTextInput = numericColumn.BlockCharactersOnTextInput,
                    AllowNullInput = numericColumn.AllowNullInput,
                    FormatString = numericColumn.FormatString,
                    ParsingMode = numericColumn.ParsingMode,
#else
                    AllowScrollingOnCircle = numericColumn.AllowScrollingOnCircle,
                    AllowNullValue = numericColumn.AllowNullValue,
                    NumberGroupSizes = numericColumn.NumberGroupSizes,
                    MinValue = numericColumn.MinValue,
                    MaxValue = numericColumn.MaxValue,
                    NumberDecimalDigits = numericColumn.NumberDecimalDigits,
                    NumberDecimalSeparator = numericColumn.NumberDecimalSeparator,
                    NumberGroupSeparator = numericColumn.NumberGroupSeparator,
                    NumberNegativePattern = numericColumn.NumberNegativePattern,
                    NullValue = numericColumn.NullValue,
                    NullText = numericColumn.NullText,
                    MaxValidation = numericColumn.MaxValidation,
                    MinValidation = numericColumn.MinValidation,
#endif
                };
            }
            else if (column is GridUnBoundColumn)
            {
                var unBoundColumn = column as GridUnBoundColumn;
                serializableColumn = new SerializableGridUnBoundColumn()
                {
                    CaseSensitive = unBoundColumn.CaseSensitive,
                    Format = unBoundColumn.Format,
                    Expression = unBoundColumn.Expression,
                };
            }
            else if (column is GridTemplateColumn)
            {
                var templateColumn = column as GridTemplateColumn;
                serializableColumn = new SerializableGridTemplateColumn()
                {
                    HorizontalAlignment = templateColumn.HorizontalAlignment,
                    VerticalAlignment = templateColumn.VerticalAlignment,
                };
            }
            else
            {
                serializableColumn = new SerializableGridTextColumn();
            }
            return serializableColumn;
        }
        
        /// <summary>
        /// Stores the grid column properties to the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridColumn"/> during serialization process.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to store it property settings for serialization.
        /// </param>
        /// <param name="serializableColumn">
        /// The corresponding SerializableGridColumn where the column properties are stored.
        /// </param>
        protected virtual void StoreGridColumnProperties(GridColumn column, SerializableGridColumn serializableColumn)
        {
            if (column.ReadLocalValue(GridColumn.AllowSortingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowSorting = column.AllowSorting;
            else
                serializableColumn.AllowSorting = null;

            if (column.ReadLocalValue(GridColumn.AllowDraggingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowDragging = column.AllowDragging;
            else
                serializableColumn.AllowDragging = null;

            if (column.ReadLocalValue(GridColumn.AllowFilteringProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowFiltering = column.AllowFiltering;
            else
                serializableColumn.AllowFiltering = null;

            if (column.ReadLocalValue(GridColumn.AllowEditingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowEditing = column.AllowEditing;
            else
                serializableColumn.AllowEditing = null;

            if (column.ReadLocalValue(GridColumn.AllowGroupingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowGrouping = column.AllowGrouping;
            else
                serializableColumn.AllowGrouping = null;

            if (column.ReadLocalValue(GridColumn.AllowResizingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.AllowResizing = column.AllowResizing;
            else
                serializableColumn.AllowResizing = null;
            serializableColumn.IsAutoGenerated = column.IsAutoGenerated;
            serializableColumn.ColumnMemberType = column.ColumnMemberType;

            if (column.ReadLocalValue(GridColumn.AllowResizingProperty) != DependencyProperty.UnsetValue)
                serializableColumn.ColumnSizer = column.ColumnSizer;
            else
                serializableColumn.ColumnSizer = null;

            serializableColumn.ColumnFilter = column.ColumnFilter;
            serializableColumn.IsCaseSensitiveFilterRow = column.IsCaseSensitiveFilterRow;
            serializableColumn.FilterRowCondition = column.FilterRowCondition;
            serializableColumn.FilterRowEditorType = column.FilterRowEditorType;
            serializableColumn.FilterRowOptionsVisibility = column.FilterRowOptionsVisibility;
            serializableColumn.HeaderText = column.HeaderText;
            serializableColumn.HorizontalHeaderContentAlignment = column.HorizontalHeaderContentAlignment;
            serializableColumn.IsHidden = column.IsHidden;
            serializableColumn.MappingName = column.MappingName;
            serializableColumn.ImmediateUpdateColumnFilter = column.ImmediateUpdateColumnFilter;
            serializableColumn.AllowBlankFilters = column.AllowBlankFilters;
            serializableColumn.AllowFocus = column.AllowFocus;
            serializableColumn.MaximumWidth = column.MaximumWidth;
            serializableColumn.MinimumWidth = column.MinimumWidth;
            serializableColumn.Width = column.Width;
            serializableColumn.TextAlignment = column.TextAlignment;
            serializableColumn.UseBindingValue = column.UseBindingValue;
            serializableColumn.GridValidationMode = column.GridValidationMode;
            serializableColumn.FilteredFrom = column.FilteredFrom;
            serializableColumn.Padding = column.Padding;
            serializableColumn.SetCellBoundValue = column.SetCellBoundValue;
            serializableColumn.SetCellBoundToolTip = column.SetCellBoundToolTip;
            serializableColumn.ShowToolTip = column.ShowToolTip;
            serializableColumn.ShowHeaderToolTip = column.ShowHeaderToolTip;
            serializableColumn.UpdateTrigger = column.UpdateTrigger;
        }

        /// <summary>
        /// Stores the filter predicates to <see cref="Syncfusion.UI.Xaml.Grid.SerializableFilterSettings"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding SfDataGrid to serialize the filter predicates settings.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.       
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableFilterSettings"/> that contains the specified filter predicates in SfDataGrid. 
        /// </returns>
        protected virtual SerializableFilterSettings StoreFilterPredicates(SfDataGrid dataGrid, SerializationOptions serializeOptions)
        {
            SerializableFilterSettings filterSettings = new SerializableFilterSettings();
            if (!serializeOptions.SerializeFiltering)
                return filterSettings;

            foreach (var column in dataGrid.Columns.Where(c => c.FilterPredicates.Count > 0))
            {
                SerializableFilterSetting filterSetting = new SerializableFilterSetting() { ColumnName = column.MappingName, Filter = new ObservableCollection<SerializableFilter>() };
                foreach (var filterPredicate in column.FilterPredicates)
                {
                    SerializableFilter filter = new SerializableFilter() { FilterBehavior = filterPredicate.FilterBehavior, FilterType = filterPredicate.FilterType, FilterValue = filterPredicate.FilterValue, IsCaseSensitive = filterPredicate.IsCaseSensitive, PredicateType = filterPredicate.PredicateType };
                    filterSetting.Filter.Add(filter);
                }
                filterSettings.Add(filterSetting);
            }
            return filterSettings;
        }

        /// <summary>
        /// Stores the sort column descriptions to <see cref="Syncfusion.UI.Xaml.Grid.SerializableSortColumnDescriptions"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding SfDataGrid to serialize its sort column descriptions property settings.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.          
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableSortColumnDescriptions"/> that contains the specified sort column descriptions in SfDataGrid. 
        /// </returns>
        protected virtual SerializableSortColumnDescriptions StoreSortColumnDescriptions(SfDataGrid dataGrid, SerializationOptions serializeOptions)
        {
            var serializableSortColumnDescriptions = new SerializableSortColumnDescriptions();
            if (!serializeOptions.SerializeSorting)
                return serializableSortColumnDescriptions;

            foreach (var columnDescription in dataGrid.SortColumnDescriptions)
            {
                var sortColumn = new SerializableSortColumnDescription
                {
                    ColumnName = columnDescription.ColumnName,
                    SortDirection = columnDescription.SortDirection
                };
                serializableSortColumnDescriptions.Add(sortColumn);
            }
            return serializableSortColumnDescriptions;
        }

        /// <summary>
        /// Stores the group column descriptions to <see cref="Syncfusion.UI.Xaml.Grid.SerializableSortColumnDescriptions"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> during serialization process.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding SfDataGrid to serialize its group column descriptions.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.          
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGroupColumnDescriptions"/> that contains the specified group column descriptions in SfDataGrid. 
        /// </returns>
        protected virtual SerializableGroupColumnDescriptions StoreGroupColumnDescriptions(SfDataGrid dataGrid, SerializationOptions serializeOptions)
        {
            var groupColumnDescriptions = new SerializableGroupColumnDescriptions();
            if (!serializeOptions.SerializeGrouping)
                return groupColumnDescriptions;

            foreach (var columnDescription in dataGrid.GroupColumnDescriptions)
            {
                var groupColumn = new SerializableGroupColumnDescription { ColumnName = columnDescription.ColumnName };
                groupColumnDescriptions.Add(groupColumn);
            }
            return groupColumnDescriptions;
        }
       
        /// <summary>
        /// Stores the stacked header settings to the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> during serialization process.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializable DataGrid to store the grid stacked header rows.
        /// </param>
        /// <param name="stackedHeaderRows">
        /// The corresponding stacked header rows that need to be copied to serializable DataGrid for serialization.
        /// </param>
        /// <param name="serializeOptions">
        /// The corresponding serialization options.          
        /// </param>
        protected virtual void StoreGridStackedHeaderRow(SerializableDataGrid serializableDataGrid, StackedHeaderRows stackedHeaderRows, SerializationOptions serializeOptions)
        {
            if (!serializeOptions.SerializeStackedHeaders)
                return;

            if (stackedHeaderRows != null)
            {
                foreach (var stackedHeaderRow in stackedHeaderRows)
                {
                    var headerRow = new SerializableStackedHeaderRow();
                    headerRow.Name = stackedHeaderRow.Name;
                    headerRow.StackedColumns = new SerializableStackedColumns();

                    foreach (var stackedColumn in stackedHeaderRow.StackedColumns)
                    {
                        var stackheadercolumn = new SerializableStackedColumn()
                        {
                            ChildColumns = stackedColumn.ChildColumns,
                            HeaderText = stackedColumn.HeaderText
                        };
                        headerRow.StackedColumns.Add(stackheadercolumn);
                    }
                    serializableDataGrid.StackedHeaderRows.Add(headerRow);
                }
            }
        }
        #endregion
        #endregion

        #region Deserialization

        #region Public Virtual methods
        /// <summary>
        /// Deserializes the SfDataGrid from the given XML stream based on the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/>.
        /// </summary>
        /// <param name="stream">
        /// Contains the XML document to deserialize.
        /// </param>
        /// <param name="deserializeOptions">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> to decide the type of grid operations such as sorting ,filtering ,and etc to be deserialized.
        /// </param>
        public virtual void Deserialize(Stream stream, DeserializationOptions deserializeOptions)
        {
            SerializableGridColumn.DataGrid = this.Datagrid;
            var serializer = new DataContractSerializer(typeof(SerializableDataGrid));
            var grid = serializer.ReadObject(stream);
            ReloadGrid(grid as SerializableDataGrid, deserializeOptions);
        }

        #endregion

        #region Protected Virtual Methods      

        /// <summary>
        /// Restores the SfDataGrid settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization process.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializable DataGrid to restore the grid settings.
        /// </param>
        /// <param name="dataGrid">
        /// The corresponding SfDataGrid to deserialize its settings.
        /// </param>
        /// <param name="options">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> for deserializing the grid settings.
        /// </param>
        protected virtual void RestoreGridSettings(SerializableDataGrid serializableDataGrid, SfDataGrid dataGrid, DeserializationOptions options)
        {
            RestoreGridProperties(serializableDataGrid);
            RestoreGridColumns(serializableDataGrid, options);
            RestoreFilterPredicates(serializableDataGrid, options);
            RestoreGroupColumnDescriptions(serializableDataGrid, options);
            RestoreSortColumnDescriptions(serializableDataGrid, options);
            RestoreGridGroupSummaryRows(serializableDataGrid, options);
            RestoreGridCaptionSummaryRow(serializableDataGrid, options);
            RestoreGridTableSummaryRows(serializableDataGrid, options);
            RestoreGridStackedHeaderRows(serializableDataGrid, options);
            RestoreGridUnBoundRows(serializableDataGrid, options);
            RestoreDetailsViewDefinition(serializableDataGrid, options);

        }      

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> from the SerializableGridColumn during deserialization process.
        /// </summary>
        /// <param name="serializableColumn">
        /// The serializable column to get its grid column.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/>.
        /// </returns>
        /// <remarks>
        /// Override this method to get the custom column during deserialization process.
        /// </remarks>        
        protected virtual GridColumn GetGridColumn(SerializableGridColumn serializableColumn)
        {
            var cache = cachedColumns.Where(x => x.mappingName == serializableColumn.MappingName).FirstOrDefault();
            if (serializableColumn is SerializableGridTextColumn)
            {
                var column = cache is GridTextColumn ? cache as GridTextColumn : new GridTextColumn();
                var serializableTextCoumn = serializableColumn as SerializableGridTextColumn;
                column.TextWrapping = serializableTextCoumn.TextWrapping;
#if UWP
                column.IsSpellCheckEnabled = serializableTextCoumn.IsSpellCheckEnabled;
#endif
                return column;
            }
            else if (serializableColumn is SerializableGridComboBoxColumn)
            {
                var column = cache is GridComboBoxColumn ? cache as GridComboBoxColumn : new GridComboBoxColumn();
                var serializableComboBoxColumn = serializableColumn as SerializableGridComboBoxColumn;
                column.SelectedValuePath = serializableComboBoxColumn.SelectedValuePath;
                column.DisplayMemberPath = serializableComboBoxColumn.DisplayMemberPath;
#if WPF
                column.StaysOpenOnEdit = serializableComboBoxColumn.StaysOpenOnEdit;
                column.IsEditable = serializableComboBoxColumn.IsEditable;
#endif
                return column;
            }
            else if (serializableColumn is SerializableGridCheckBoxColumn)
            {
                var column = cache is GridCheckBoxColumn ? cache as GridCheckBoxColumn : new GridCheckBoxColumn();
                var serializableCheckboxColumn = serializableColumn as SerializableGridCheckBoxColumn;
                column.HorizontalAlignment = serializableCheckboxColumn.HorizontalAlignment;
                column.VerticalAlignment = serializableCheckboxColumn.VerticalAlignment;
                column.IsThreeState = serializableCheckboxColumn.IsThreeState;
                return column;
            }
            else if (serializableColumn is SerializableGridMultiColumnDropDownList)
            {
                var column = cache is GridMultiColumnDropDownList ? cache as GridMultiColumnDropDownList : new GridMultiColumnDropDownList();
                var serializableMutiColumn = serializableColumn as SerializableGridMultiColumnDropDownList;
                column.DisplayMember = serializableMutiColumn.DisplayMember;
                column.ValueMember = serializableMutiColumn.ValueMember;
                column.ShowResizeThumb = serializableMutiColumn.ShowResizeThumb;
                column.PopUpHeight = serializableMutiColumn.PopUpHeight;
                column.PopUpWidth = serializableMutiColumn.PopUpWidth;
                column.AllowAutoComplete = serializableMutiColumn.AllowAutoComplete;
                column.AllowSpinOnMouseWheel = serializableMutiColumn.AllowSpinOnMouseWheel;
                column.AllowIncrementalFiltering = serializableMutiColumn.AllowIncrementalFiltering;
                column.SearchCondition = serializableMutiColumn.SearchCondition;
                column.AllowCasingforFilter = serializableMutiColumn.AllowCasingforFilter;
                column.PopUpMaxHeight = serializableMutiColumn.PopUpMaxHeight;
                column.PopUpMaxWidth = serializableMutiColumn.PopUpMaxWidth;
                column.PopUpMinHeight = serializableMutiColumn.PopUpMinHeight;
                column.PopUpMinWidth = serializableMutiColumn.PopUpMinWidth;
                column.IsTextReadOnly = serializableMutiColumn.IsTextReadOnly;
                column.AllowNullInput = serializableMutiColumn.AllowNullInput;
                column.AutoGenerateColumns = serializableMutiColumn.AutoGenerateColumns;
                column.GridColumnSizer = serializableMutiColumn.GridColumnSizer;
                column.IsAutoPopupSize = serializableMutiColumn.IsAutoPopupSize;
                return column;
            }
#if UWP
            else if (serializableColumn is SerializableGridUpDownColumn)
            {
                var column = cache is GridUpDownColumn ? cache as GridUpDownColumn : new GridUpDownColumn();
                var serializableUpDownColumn = (serializableColumn as SerializableGridUpDownColumn);
                column.SmallChange = serializableUpDownColumn.SmallChange;
                column.MinValue = serializableUpDownColumn.MinValue;
                column.MaxValue = serializableUpDownColumn.MaxValue;
                column.NumberDecimalDigits = serializableUpDownColumn.NumberDecimalDigits;
                column.AutoReverse = serializableUpDownColumn.AutoReverse;
                column.ParsingMode = serializableUpDownColumn.ParsingMode;
                return column;
            }
#else
            else if (serializableColumn is SerializableGridCurrencyColumn)
            {
                var column = cache is GridCurrencyColumn ? cache as GridCurrencyColumn : new GridCurrencyColumn();
                var serializableCurrencyColumn = (serializableColumn as SerializableGridCurrencyColumn);
                column.AllowNullValue = serializableCurrencyColumn.AllowNullValue;
                column.AllowScrollingOnCircle = serializableCurrencyColumn.AllowScrollingOnCircle;
                column.CurrencyDecimalDigits = serializableCurrencyColumn.CurrencyDecimalDigits;
                column.CurrencyDecimalSeparator = serializableCurrencyColumn.CurrencyDecimalSeparator;
                column.CurrencyGroupSeparator = serializableCurrencyColumn.CurrencyGroupSeparator;
                column.CurrencyGroupSizes = serializableCurrencyColumn.CurrencyGroupSizes;
                column.CurrencyNegativePattern = serializableCurrencyColumn.CurrencyNegativePattern;
                column.CurrencyPositivePattern = serializableCurrencyColumn.CurrencyPositivePattern;
                column.CurrencySymbol = serializableCurrencyColumn.CurrencySymbol;
                column.MaxValue = serializableCurrencyColumn.MinValue;
                column.MinValue = serializableCurrencyColumn.MaxValue;
                column.NullValue = serializableCurrencyColumn.NullValue;
                column.NullText = serializableCurrencyColumn.NullText;
                column.MaxValidation = serializableCurrencyColumn.MaxValidation;
                column.MinValidation = serializableCurrencyColumn.MinValidation;
                return column;
            }
            else if (serializableColumn is SerializableGridPercentageColumn)
            {
                var column = cache is GridPercentColumn ? cache as GridPercentColumn : new GridPercentColumn();
                var serializablePercentageColumn = (serializableColumn as SerializableGridPercentageColumn);
                column.AllowNullValue = serializablePercentageColumn.AllowNullValue;
                column.AllowScrollingOnCircle = serializablePercentageColumn.AllowScrollingOnCircle;
                column.PercentDecimalDigits = serializablePercentageColumn.PercentDecimalDigits;
                column.PercentDecimalSeparator = serializablePercentageColumn.PercentDecimalSeparator;
                column.PercentGroupSeparator = serializablePercentageColumn.PercentGroupSeparator;
                column.PercentGroupSizes = serializablePercentageColumn.PercentGroupSizes;
                column.PercentNegativePattern = serializablePercentageColumn.PercentNegativePattern;
                column.PercentPositivePattern = serializablePercentageColumn.PercentPositivePattern;
                column.PercentSymbol = serializablePercentageColumn.PercentSymbol;
                column.MaxValue = serializablePercentageColumn.MinValue;
                column.MinValue = serializablePercentageColumn.MaxValue;
                column.PercentEditMode = serializablePercentageColumn.PercentEditMode;
                column.NullValue = serializablePercentageColumn.NullValue;
                column.NullText = serializablePercentageColumn.NullText;
                column.MaxValidation = serializablePercentageColumn.MaxValidation;
                column.MinValidation = serializablePercentageColumn.MinValidation;
                return column;
            }
            else if (serializableColumn is SerializableGridTimeSpanColumn)
            {
                var column = cache is GridTimeSpanColumn ? cache as GridTimeSpanColumn : new GridTimeSpanColumn();
                var serializableTimeSpanColumn = (serializableColumn as SerializableGridTimeSpanColumn);
                column.AllowNull = serializableTimeSpanColumn.AllowNull;
                column.AllowScrollingOnCircle = serializableTimeSpanColumn.AllowScrollingOnCircle;
                column.NullText = serializableTimeSpanColumn.NullText;
                column.Format = serializableTimeSpanColumn.Format;
                column.ShowArrowButtons = serializableTimeSpanColumn.ShowArrowButtons;
                column.MaxValue = serializableTimeSpanColumn.MaxValue;
                column.MinValue = serializableTimeSpanColumn.MinValue;
                return column;
            }
            else if (serializableColumn is SerializableGridMaskColumn)
            {
                var column = cache is GridMaskColumn ? cache as GridMaskColumn : new GridMaskColumn();
                var serializableMaskColumn = (serializableColumn as SerializableGridMaskColumn);
                column.SelectTextOnFocus = serializableMaskColumn.SelectTextOnFocus;
                column.IsNumeric = serializableMaskColumn.IsNumeric;
                column.DateSeparator = serializableMaskColumn.DateSeparator;
                column.DecimalSeparator = serializableMaskColumn.DecimalSeparator;
                column.TimeSeparator = serializableMaskColumn.TimeSeparator;
                column.Mask = serializableMaskColumn.Mask;
                column.MaskFormat = serializableMaskColumn.MaskFormat;
                column.PromptChar = serializableMaskColumn.PromptChar;
                return column;
            }
#endif
            else if (serializableColumn is SerializableGridDateTimeColumn)
            {
                var column = cache is GridDateTimeColumn ? cache as GridDateTimeColumn : new GridDateTimeColumn();
                var serializableDateTimeColumn = (serializableColumn as SerializableGridDateTimeColumn);
#if UWP
                column.AllowInlineEditing = serializableDateTimeColumn.AllowInlineEditing;
                column.FormatString = serializableDateTimeColumn.FormatString;
                column.ShowDropDownButton = serializableDateTimeColumn.ShowDropDownButton;
#else
                column.AllowScrollingOnCircle = serializableDateTimeColumn.AllowScrollingOnCircle;
                column.AllowNullValue = serializableDateTimeColumn.AllowNullValue;
                column.EnableClassicStyle = serializableDateTimeColumn.EnableClassicStyle;
                column.DisableDateSelection = serializableDateTimeColumn.DisableDateSelection;
                column.ShowRepeatButton = serializableDateTimeColumn.ShowRepeatButton;
                column.NullValue = serializableDateTimeColumn.NullValue;
                column.NullText = serializableDateTimeColumn.NullText;
                column.DateTimeFormat = serializableDateTimeColumn.DateTimeFormat;
                column.CanEdit = serializableDateTimeColumn.CanEdit;
                column.EnableBackspaceKey = serializableDateTimeColumn.EnableBackspaceKey;
                column.EnableDeleteKey = serializableDateTimeColumn.EnableDeleteKey;
                column.Pattern = serializableDateTimeColumn.Pattern;
                column.CustomPattern = serializableDateTimeColumn.CustomPattern;
#endif
                return column;
            }
            else if (serializableColumn is SerializableGridHyperlinkColumn)
            {
                var column = cache is GridHyperlinkColumn ? cache as GridHyperlinkColumn : new GridHyperlinkColumn();
                var serializableHyperlinkColumn = (serializableColumn as SerializableGridHyperlinkColumn);
                column.HorizontalAlignment = serializableHyperlinkColumn.HorizontalAlignment;
                column.VerticalAlignment = serializableHyperlinkColumn.VerticalAlignment;
                return column;
            }
            else if (serializableColumn is SerializableGridNumericColumn)
            {
                var column = cache is GridNumericColumn ? cache as GridNumericColumn : new GridNumericColumn();
                var serializableNumericColumn = (serializableColumn as SerializableGridNumericColumn);
#if UWP
                column.BlockCharactersOnTextInput = serializableNumericColumn.BlockCharactersOnTextInput;
                column.AllowNullInput = serializableNumericColumn.AllowNullInput;
                column.FormatString = serializableNumericColumn.FormatString;
                column.ParsingMode = serializableNumericColumn.ParsingMode;
#else
                column.AllowScrollingOnCircle = serializableNumericColumn.AllowScrollingOnCircle;
                column.AllowNullValue = serializableNumericColumn.AllowNullValue;
                column.MinValue = serializableNumericColumn.MinValue;
                column.MaxValue = serializableNumericColumn.MaxValue;
                column.NumberGroupSizes = serializableNumericColumn.NumberGroupSizes;
                column.NumberDecimalDigits = serializableNumericColumn.NumberDecimalDigits;
                column.NumberDecimalSeparator = serializableNumericColumn.NumberDecimalSeparator;
                column.NumberGroupSeparator = serializableNumericColumn.NumberGroupSeparator;
                column.NumberNegativePattern = serializableNumericColumn.NumberNegativePattern;
                column.NullValue = serializableNumericColumn.NullValue;
                column.NullText = serializableNumericColumn.NullText;
                column.MaxValidation = serializableNumericColumn.MaxValidation;
                column.MinValidation = serializableNumericColumn.MinValidation;
#endif
                return column;
            }
            else if (serializableColumn is SerializableGridUnBoundColumn)
            {
                var column = cache is GridUnBoundColumn ? cache as GridUnBoundColumn : new GridUnBoundColumn();
                var serializableUnboundColumn = (serializableColumn as SerializableGridUnBoundColumn);
                column.CaseSensitive = serializableUnboundColumn.CaseSensitive;
                column.Format = serializableUnboundColumn.Format;
                column.Expression = serializableUnboundColumn.Expression;
                return column;
            }
            else if (serializableColumn is SerializableGridTemplateColumn)
            {
                var column = cache is GridTemplateColumn ? cache as GridTemplateColumn : new GridTemplateColumn();
                var serializableTemplateColumn = (serializableColumn as SerializableGridTemplateColumn);
                column.HorizontalAlignment = serializableTemplateColumn.HorizontalAlignment;
                column.VerticalAlignment = serializableTemplateColumn.VerticalAlignment;
                return column;
            }
            else
                return new GridTextColumn();
        }
       
        /// <summary>
        /// Restores the grid columns from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization process.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// Restores the columns settings from the SerializableDataGrid.
        /// </param>
        /// <param name="options">
        /// Contains the deserialization options.
        /// </param>
        protected virtual void RestoreGridColumns(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeColumns)
                return;
            //WRT-7024 maintain a copy of DataGrid columns collection to restore itemsource for GridMultiColumnDropDownList and GridComboBoxColumn
             cachedColumns=this.Datagrid.Columns.ToList();
             //WPF-31221 unwire Column events
             cachedColumns.ForEach(x => this.Datagrid.GridModel.UnWireColumnDescriptor(x));
            if (this.Datagrid.Columns != null)
                this.Datagrid.Columns.Clear();
            var columns = new Columns();
            foreach (var serializableColumn in serializableDataGrid.Columns)
            {
                var column = GetGridColumn(serializableColumn);
                RestoreColumnProperties(serializableColumn, column);
                if (options.DeserializeFiltering)
                    column.FilterRowText = serializableColumn.FilterRowText;
                columns.Add(column);
            }
            this.Datagrid.Columns = columns;
            // WPF-20509 - Need to set DataGrid here since it will be used in grouping, sorting, column sizer,.. cases
            this.Datagrid.Columns.ForEach(x => this.Datagrid.GridModel.WireColumnDescriptor(x));
            cachedColumns.Clear();
        }
       
        /// <summary>
        /// Restores the grid column properties from the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridColumn"/> to the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> during deserialization.
        /// </summary>
        /// <param name="serializableColumn">
        /// The corresponding SerializableGridColumn to restore the grid column properties.
        /// </param>
        /// <param name="column">
        /// The corresponding column to restore its properties for deserialization.
        /// </param>
        protected virtual void RestoreColumnProperties(SerializableGridColumn serializableColumn, GridColumn column)
        {
            if (serializableColumn.AllowDragging != null)
                column.AllowDragging = (bool)serializableColumn.AllowDragging;
            if (serializableColumn.AllowGrouping != null)
                column.AllowGrouping = (bool)serializableColumn.AllowGrouping;
            if (serializableColumn.AllowResizing != null)
                column.AllowResizing = (bool)serializableColumn.AllowResizing;
            if (serializableColumn.AllowSorting != null)
                column.AllowSorting = (bool)serializableColumn.AllowSorting;
            if (serializableColumn.ColumnSizer != null)
                column.ColumnSizer = (GridLengthUnitType)serializableColumn.ColumnSizer;

            column.IsCaseSensitiveFilterRow = serializableColumn.IsCaseSensitiveFilterRow;
            column.FilterRowCondition = serializableColumn.FilterRowCondition;
            column.FilterRowEditorType = serializableColumn.FilterRowEditorType;
            column.FilterRowOptionsVisibility = serializableColumn.FilterRowOptionsVisibility;            
            column.HeaderText = serializableColumn.HeaderText;
            column.HorizontalHeaderContentAlignment = serializableColumn.HorizontalHeaderContentAlignment;
            column.IsHidden = serializableColumn.IsHidden;
            if (serializableColumn.AllowEditing != null)
                column.AllowEditing = (bool)serializableColumn.AllowEditing;
            column.AllowFocus = serializableColumn.AllowFocus;
            if (serializableColumn.AllowFiltering != null)
                column.AllowFiltering = (bool)serializableColumn.AllowFiltering;
            column.IsAutoGenerated = serializableColumn.IsAutoGenerated;
            column.ColumnMemberType = serializableColumn.ColumnMemberType;
            column.ColumnFilter = serializableColumn.ColumnFilter;
            column.ImmediateUpdateColumnFilter = serializableColumn.ImmediateUpdateColumnFilter;
            column.AllowBlankFilters = serializableColumn.AllowBlankFilters;
            column.MappingName = serializableColumn.MappingName;
            column.MaximumWidth = serializableColumn.MaximumWidth;
            column.MinimumWidth = serializableColumn.MinimumWidth;
            column.Width = serializableColumn.Width;
            column.TextAlignment = serializableColumn.TextAlignment;
            column.UseBindingValue = serializableColumn.UseBindingValue;
            column.UpdateTrigger = serializableColumn.UpdateTrigger;
            column.SetCellBoundValue = serializableColumn.SetCellBoundValue;
            column.Padding = serializableColumn.Padding;
            column.GridValidationMode = serializableColumn.GridValidationMode;
            column.FilteredFrom = serializableColumn.FilteredFrom;
            column.SetCellBoundToolTip = serializableColumn.SetCellBoundToolTip;
            column.ShowToolTip = serializableColumn.ShowToolTip;
            column.ShowHeaderToolTip = serializableColumn.ShowHeaderToolTip;
        }        

        /// <summary>
        /// Restores the filter predicates from <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to restore the filter predicates.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the filter predicates.
        /// </param>
        protected virtual void RestoreFilterPredicates(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            //WPF-29780  Return nothing, if View is null or DeserializeFiltering is false.
            if (!options.DeserializeFiltering || this.Datagrid.View == null)
                return;

            //WPF-29780 Clear FilterPredicates if View is not null.
            this.Datagrid.View.FilterPredicates.Clear();
            //WPF-31221 clear filterpredicates  in columns because columns are reused while deserialization
            this.Datagrid.Columns.ForEach( (x)=> x.FilterPredicates.Clear());
            if ((serializableDataGrid as SerializableDataGrid).FilterSettings != null)
                RestoreColumnFilterPredicates((serializableDataGrid as SerializableDataGrid).FilterSettings);
        }

        /// <summary>
        /// Restores the column filter predicates from the DataGrid filter predicates or serializable DataGrid filter settings.
        /// </summary>
        /// <param name="datagridFilterSettings">
        /// The corresponding DataGrid filter settings for deserializing filter predicates.
        /// </param>
        protected virtual void RestoreColumnFilterPredicates(dynamic datagridFilterSettings)
        {
            foreach (var filterSetting in datagridFilterSettings)
            {
                var column = Datagrid.Columns.FirstOrDefault(c => c.MappingName == (datagridFilterSettings.GetType() == typeof(SerializableFilterSettings) ?filterSetting.ColumnName:filterSetting.MappingName));
                if (column == null)
                    continue;
                foreach (var filter in (datagridFilterSettings.GetType() == typeof(SerializableFilterSettings) ? filterSetting.Filter : filterSetting.FilterPredicates))
                {
                    column.FilterPredicates.Add(new FilterPredicate()
                    {
                        FilterBehavior = filter.FilterBehavior,
                        FilterType = filter.FilterType,
                        FilterValue = filter.FilterValue,
                        IsCaseSensitive = filter.IsCaseSensitive,
                        PredicateType = filter.PredicateType
                    });
                }
            }
        }

        /// <summary>
        /// Restores the sort column descriptions settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The serializable DataGrid to deserialize the sort column descriptions.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the sort column descriptions.
        /// </param>
        protected virtual void RestoreSortColumnDescriptions(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeSorting)
                return;

            if (Datagrid.View != null)
            {
                this.Datagrid.View.SortDescriptions.Clear();
                this.Datagrid.View.SortComparers.Clear();
            }
            else
            {
                // For SourceDataGrid, view will be null. So SortColumn present in DataGrid will be cleared
                this.Datagrid.SortColumnDescriptions.Clear();
                this.Datagrid.SortComparers.Clear();
            }
            var sortColumnDescriptions = new SortColumnDescriptions();
            foreach (var sortColumnDescription in serializableDataGrid.SortColumnDescriptions)
            {
                if (!CheckColumnAvailability(sortColumnDescription.ColumnName))
                    continue;
                var sortColumn = new SortColumnDescription
                {
                    ColumnName = sortColumnDescription.ColumnName,
                    SortDirection = sortColumnDescription.SortDirection
                };
                this.Datagrid.SortColumnDescriptions.Add(sortColumn);
            }
        }

        /// <summary>
        /// Restores the group column descriptions from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The serializable DataGrid to deserialize the group column descriptions.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the group column descriptions.
        /// </param>
        protected virtual void RestoreGroupColumnDescriptions(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeGrouping)
                return;

            if (this.Datagrid.View != null)
                this.Datagrid.View.GroupDescriptions.Clear();
            else
                // For SourceDataGrid, view will be null. So GroupColumn present in DataGrid will be cleared
                this.Datagrid.GroupColumnDescriptions.Clear();
            var groupColumnDescriptions = new GroupColumnDescriptions();
            foreach (var groupColumnDescription in serializableDataGrid.GroupColumnDescriptions)
            {
                if (!CheckColumnAvailability(groupColumnDescription.ColumnName))
                    continue;
                var groupColumn = new GroupColumnDescription { ColumnName = groupColumnDescription.ColumnName };
                Datagrid.GroupColumnDescriptions.Add(groupColumn);
            }
        }

        /// <summary>
        /// Restores grid properties from the specified <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/>.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializable DataGrid to restore DataGrid properties for deserialization.
        /// </param>
        protected virtual void RestoreGridProperties(SerializableDataGrid serializableDataGrid)
        {
            Datagrid.AllowDraggingColumns = serializableDataGrid.AllowDraggingColumns;
            Datagrid.AllowFiltering = serializableDataGrid.AllowFiltering;
            Datagrid.CurrentCellBorderThickness = serializableDataGrid.CurrentCellBorderThickness;
            Datagrid.EditTrigger = serializableDataGrid.EditTrigger;
            Datagrid.AllowEditing = serializableDataGrid.AllowEditing;
            Datagrid.AllowFrozenGroupHeaders = serializableDataGrid.AllowFrozenGroupHeaders;
            Datagrid.AllowGrouping = serializableDataGrid.AllowGrouping;
            Datagrid.AllowResizingColumns = serializableDataGrid.AllowResizingColumns;
            Datagrid.AllowResizingHiddenColumns = serializableDataGrid.AllowResizingHiddenColumns;
            Datagrid.AllowSelectionOnPointerPressed = serializableDataGrid.AllowSelectionOnPointerPressed;
            Datagrid.AllowSorting = serializableDataGrid.AllowSort;
            Datagrid.AllowTriStateSorting = serializableDataGrid.AllowTriStateSorting;
            Datagrid.AutoExpandGroups = serializableDataGrid.AutoExpandGroups;
            Datagrid.AutoGenerateColumnsMode = serializableDataGrid.AutoGenerateColumnsMode;
            Datagrid.AutoGenerateColumns = serializableDataGrid.AutoGenerateColumns;
            Datagrid.AutoGenerateRelations = serializableDataGrid.AutoGenerateRelations;
            Datagrid.ColumnSizer = serializableDataGrid.ColumnSizer;
            Datagrid.FrozenColumnCount = serializableDataGrid.FrozenColumnCount;
            Datagrid.FrozenRowsCount = serializableDataGrid.FrozenRowsCount;
            Datagrid.FooterColumnCount = serializableDataGrid.FooterColumnCount;
            Datagrid.FooterRowsCount = serializableDataGrid.FooterRowsCount;
            Datagrid.GroupCaptionTextFormat = serializableDataGrid.GroupCaptionTextFormat;
            Datagrid.GroupDropAreaText = serializableDataGrid.GroupDropAreaText;
            Datagrid.HeaderRowHeight = serializableDataGrid.HeaderRowHeight;
            Datagrid.IsGroupDropAreaExpanded = serializableDataGrid.IsGroupDropAreaExpanded;
            Datagrid.LiveDataUpdateMode = serializableDataGrid.LiveDataUpdateMode;
            Datagrid.RowHeight = serializableDataGrid.RowHeight;
            Datagrid.SelectionMode = serializableDataGrid.SelectionMode;
            Datagrid.SelectionUnit = serializableDataGrid.SelectionUnit;
            Datagrid.ShowColumnWhenGrouped = serializableDataGrid.ShowColumnWhenGrouped;
            Datagrid.ShowGroupDropArea = serializableDataGrid.ShowGroupDropArea;
            Datagrid.ShowSortNumbers = serializableDataGrid.ShowSortNumbers;
            Datagrid.SortClickAction = serializableDataGrid.SortClickAction;
            Datagrid.AddNewRowPosition = serializableDataGrid.AddNewRowPosition;
            Datagrid.FilterRowPosition = serializableDataGrid.FilterRowPosition;
            Datagrid.AllowDeleting = serializableDataGrid.AllowDeleting;
            Datagrid.AllowRowHoverHighlighting = serializableDataGrid.AllowRowHoverHighlighting;
            Datagrid.DataFetchSize = serializableDataGrid.DataFetchSize;
            Datagrid.GridValidationMode = serializableDataGrid.GridValidationMode;
            Datagrid.NavigationMode = serializableDataGrid.NavigationMode;
            Datagrid.ShowRowHeader = serializableDataGrid.ShowRowHeader;
            Datagrid.EditorSelectionBehavior = serializableDataGrid.EditorSelectionBehavior;
            Datagrid.UsePLINQ = serializableDataGrid.UsePLINQ;
            Datagrid.ShowBusyIndicator = serializableDataGrid.ShowBusyIndicator;
            Datagrid.DetailsViewPadding = serializableDataGrid.DetailsViewPadding;
            Datagrid.ReuseRowsOnItemssourceChange = serializableDataGrid.ReuseRowsOnItemssourceChange;
            Datagrid.IsDynamicItemsSource = serializableDataGrid.IsDynamicItemsSource;
            Datagrid.RowHeaderWidth = serializableDataGrid.RowHeaderWidth;
            Datagrid.GridCopyOption = serializableDataGrid.GridCopyOption;
            Datagrid.GridPasteOption = serializableDataGrid.GridPasteOption;
            Datagrid.IndentColumnWidth = serializableDataGrid.IndentColumnWidth;
            Datagrid.ExpanderColumnWidth = serializableDataGrid.ExpanderColumnWidth;
        }
      
        /// <summary>
        /// Restores the grid summary row settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableGridSummaryRow"/> to <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> during deserialization.
        /// </summary>
        /// <param name="serializableGridSummaryRow">
        /// The corresponding SerializableGridSummaryRow to restore the summary row settings.
        /// </param>
        /// <param name="summaryRow">
        /// The corresponding summary row to restore its settings for deserialization.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/>.
        /// </returns>
        protected virtual GridSummaryRow RestoreGridSummaryRow(SerializableGridSummaryRow serializableGridSummaryRow, GridSummaryRow summaryRow = null)
        {
            if (summaryRow == null)
            {
                summaryRow = new GridSummaryRow();
            }

            summaryRow.Name = serializableGridSummaryRow.Name;
            summaryRow.Title = serializableGridSummaryRow.Title;
            summaryRow.SummaryColumns = new ObservableCollection<ISummaryColumn>();
            summaryRow.ShowSummaryInRow = serializableGridSummaryRow.ShowSummaryInRow;

            foreach (var summaryColumn in serializableGridSummaryRow.SummaryColumns)
            {
                if (!CheckColumnAvailability(summaryColumn.MappingName))
                    continue;
                var column = new GridSummaryColumn
                {
                    Name = summaryColumn.Name,
                    MappingName = summaryColumn.MappingName,
                    Format = summaryColumn.Format,
                    SummaryType = summaryColumn.SummaryType
                };
                summaryRow.SummaryColumns.Add(column);
            }
            return summaryRow;
        }

        /// <summary>
        /// Restores the group summary rows settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializable DataGrid to restore the group summary rows settings.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the group summary rows.
        /// </param>       
        protected virtual void RestoreGridGroupSummaryRows(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeGroupSummaries)
                return;

            if (Datagrid.View != null)
                this.Datagrid.View.SummaryRows.Clear();
            var groupSummaryRows = new ObservableCollection<GridSummaryRow>();
            foreach (var gridSummaryRow in serializableDataGrid.GroupSummaryRows)
            {
                var summaryRow = RestoreGridSummaryRow(gridSummaryRow);
                groupSummaryRows.Add(summaryRow);
            }
            Datagrid.GroupSummaryRows = groupSummaryRows;
        }

        /// <summary>
        /// Restores the caption summary row settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding serializable DataGrid to restore the caption summary row settings.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the caption summary row.
        /// </param>  
        protected virtual void RestoreGridCaptionSummaryRow(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeCaptionSummary)
                return;
            this.Datagrid.CaptionSummaryRow = null;
            if (serializableDataGrid.CaptionSummaryRow != null)
            {
                var captionSummaryRow = RestoreGridSummaryRow(serializableDataGrid.CaptionSummaryRow);
                Datagrid.CaptionSummaryRow = captionSummaryRow;
            }
        }

        /// <summary>
        /// Restores the table summary rows settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to restore the table summary rows settings.
        /// </param>
        /// <param name="options">
        /// Specifies the options for deserializing the table summary rows.
        /// </param>  
        protected virtual void RestoreGridTableSummaryRows(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeTableSummaries)
                return;

            if (Datagrid.View != null)
            {
                this.Datagrid.View.TableSummaryRows.Clear();
                this.Datagrid.View.Records.TableSummaries.Clear();
            }
            var tableSummaryRows = new ObservableCollection<GridSummaryRow>();
            foreach (var serializableGridSummaryRow in serializableDataGrid.TableSummaryRows)
            {
                var summaryRow = new GridTableSummaryRow();
                if (serializableGridSummaryRow.Position == TableSummaryRowPosition.Top)
                {
                    summaryRow.Position = TableSummaryRowPosition.Top;
                }
                RestoreGridSummaryRow(serializableGridSummaryRow, summaryRow);
                tableSummaryRows.Add(summaryRow);
            }
            Datagrid.TableSummaryRows = tableSummaryRows;
        }

        /// <summary>
        /// Restores the stacked header rows settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to restore the stacked header rows settings.
        /// </param>
        /// <param name="options">
        /// The deserialization options.
        /// </param>  
        protected virtual void RestoreGridStackedHeaderRows(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {

            if (!options.DeserializeStackedHeaders)
                return;

            var stackedHeaderRows = new StackedHeaderRows();
            foreach (var stackedHeaderRow in serializableDataGrid.StackedHeaderRows)
            {
                var headerRow = new StackedHeaderRow()
                {
                    Name = stackedHeaderRow.Name,
                };

                foreach (var stackedColumn in stackedHeaderRow.StackedColumns)
                {
                    var stackedheadercolumn = new StackedColumn()
                    {
                        ChildColumns = stackedColumn.ChildColumns,
                        HeaderText = stackedColumn.HeaderText
                    };
                    headerRow.StackedColumns.Add(stackedheadercolumn);
                }
                stackedHeaderRows.Add(headerRow);
            }
            Datagrid.StackedHeaderRows = stackedHeaderRows;
        }

        /// <summary>
        /// Restores the unbound rows settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to restore the unbound rows settings.
        /// </param>
        /// <param name="options">
        /// The deserialization options.
        /// </param>  
        protected virtual void RestoreGridUnBoundRows(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeUnBoundRows)
                return;

            var unBoundRows = new UnBoundRows();
            foreach (var gridUnBoundRow in serializableDataGrid.UnBoundRows)
            {
                var unBoundRow = new GridUnBoundRow
                {
                    Position = gridUnBoundRow.Position,
                    RowIndex = -1,
                    UnBoundRowIndex = gridUnBoundRow.UnBoundRowIndex,
                    ShowBelowSummary = gridUnBoundRow.ShowBelowSummary
                };
                unBoundRows.Add(unBoundRow);
            }
            Datagrid.UnBoundRows = unBoundRows;
        }

        /// <summary>
        /// Restores the DetailsViewDefinition settings from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="serializableDataGrid">
        /// The corresponding SerializableDataGrid to restore the DetailsViewDefinition settings.
        /// </param>
        /// <param name="options">
        /// The deserialization options.
        /// </param>  
        protected virtual void RestoreDetailsViewDefinition(SerializableDataGrid serializableDataGrid, DeserializationOptions options)
        {
            if (!options.DeserializeDetailsViewDefinition)
                return;

            if (this.Datagrid.DetailsViewDefinition != null)
                this.Datagrid.DetailsViewDefinition.Clear();
            else
                this.Datagrid.DetailsViewDefinition = new DetailsViewDefinition();
            foreach (var gridViewDefinition in serializableDataGrid.DetailsViewDefinition)
            {
                var sourceDataGrid = new SfDataGrid();
                sourceDataGrid.SerializationController.RestoreGridSettings(gridViewDefinition.DataGrid, sourceDataGrid, options);
                var gridViewDefinitions = new GridViewDefinition
                {
                    DataGrid = sourceDataGrid,
                    RelationalColumn = gridViewDefinition.RelationalColumn
                };
                this.Datagrid.DetailsViewDefinition.Add(gridViewDefinitions);
            }
        }

        /// <summary>
        /// Reloads the grid properties from the <see cref="Syncfusion.UI.Xaml.Grid.SerializableDataGrid"/> with the specified <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/> during deserialization.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding SerializableDataGrid to restore the grid settings.
        /// </param>
        /// <param name="deserializationOptions">
        /// The deserialization options.
        /// </param>
        protected virtual void ReloadGrid(SerializableDataGrid dataGrid, DeserializationOptions deserializationOptions)
        {
            // If grid is SourceDataGrid, need to suspend notification
            if (this.Datagrid.IsSourceDataGrid)
                (this.Datagrid as IDetailsViewNotifier).SuspendNotifyListener();
            //WPF-23076 need to clear selection before  clear the row generator items.
            this.Datagrid.SelectionController.ClearSelections(false);
            ClearGridSettings(deserializationOptions);
            RestoreGridSettings(dataGrid, this.Datagrid, deserializationOptions);
            this.Datagrid.SerializationController.SetProperties(deserializationOptions);
            this.Datagrid.SelectionController.ClearSelections(false);
            // If SourceDataGrid is deserialized, need to reset the properties of DetailsViewDataGrids present in the corresponding level
            if (this.Datagrid.IsSourceDataGrid)
            {
                (this.Datagrid as IDetailsViewNotifier).ResumeNotifyListener();
                foreach (var destinationDataGrid in (this.Datagrid.NotifyListener as DetailsViewNotifyListener).ClonedDataGrid)
                {
                    (destinationDataGrid as IDetailsViewNotifier).SuspendNotifyListener();
                    destinationDataGrid.SerializationController.ClearGridSettings(deserializationOptions);
                    destinationDataGrid.SelectionController.ClearSelections(false);
                    (this.Datagrid.NotifyListener as DetailsViewNotifyListener).CopyPropertiesFromSourceDataGrid(this.Datagrid, destinationDataGrid as DetailsViewDataGrid, true);
                    destinationDataGrid.SerializationController.SetProperties(deserializationOptions);
                    (destinationDataGrid as IDetailsViewNotifier).ResumeNotifyListener();
                }
            }
        }

        /// <summary>
        /// Unwire events and clear row generator and GroupDropArea items
        /// </summary>
        /// <param name="deserializationOptions">
        /// The deserialization options.
        /// </param>
        private void ClearGridSettings(DeserializationOptions deserializationOptions)
        {
            this.Datagrid.IsInDeserialize = true;
            UnWireSerializablePropertyEvents();
            this.Datagrid.FrozenColumnCount = 0;
            if (this.Datagrid.View != null)
                this.Datagrid.View.BeginInit(false);
            if (this.Datagrid.GroupDropArea != null)
                this.Datagrid.GroupDropArea.RemoveAllGroupDropItems();
            if (this.Datagrid.RowGenerator != null)
            {
                this.Datagrid.RowGenerator.Items.ForEach(row => row.VisibleColumns.ForEach(col => this.Datagrid.RowGenerator.UnloadUIElements(row, col)));
                this.Datagrid.RowGenerator.Items.Clear();
            }
            if (this.Datagrid.VisualContainer != null)
            {
                this.Datagrid.VisualContainer.OnItemSourceChanged();
                this.Datagrid.VisualContainer.previousArrangeWidth = 0;
                this.Datagrid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        /// <summary>
        /// Apply properties in View and update row and column count
        /// </summary>
        /// <param name="deserializationOptions">deserializationOptions</param>
        private void SetProperties(DeserializationOptions deserializationOptions)
        {
            if (this.Datagrid.View != null)
            {
                if (deserializationOptions.DeserializeSorting)
                    this.Datagrid.InitialSort();
                if (deserializationOptions.DeserializeGrouping)
                    this.Datagrid.InitialGroup();
                else if (this.Datagrid.View.GroupDescriptions.Any() && !this.Datagrid.GroupColumnDescriptions.Any())
                {
                    foreach (var description in this.Datagrid.View.GroupDescriptions)
                    {
                        var columnName = (description as ColumnGroupDescription).PropertyName;
                        if (!CheckColumnAvailability(columnName))
                            continue;
                        this.Datagrid.GroupColumnDescriptions.Add(new GroupColumnDescription() { ColumnName = columnName, Converter = (description as ColumnGroupDescription).Converter, Comparer = (description as ColumnGroupDescription).Comparer });
                    }
                }

                if (deserializationOptions.DeserializeGroupSummaries)
                    this.Datagrid.InitializeGroupSummaryRows();
                if (deserializationOptions.DeserializeTableSummaries)
                {
                    this.Datagrid.View.TableSummaryRows.Clear();
                    this.Datagrid.View.Records.TableSummaries.Clear();
                    this.Datagrid.InitializeTableSummaries();
                }
                if (deserializationOptions.DeserializeCaptionSummary)
                    this.Datagrid.InitializeCaptionSummaryRow();

                this.Datagrid.View.EndInit();
            }
            this.Datagrid.IsInDeserialize = false;
            //WPF-38094 NullReferenceException has been thrown while deserializing grid in Selectionchanged event of TabControl.
            if (this.Datagrid.isGridLoaded)
                this.Datagrid.GridModel.RefreshBatchUpdate(NotifyCollectionChangedAction.Reset);  
            this.Datagrid.RefreshHeaderLineCount();
            this.Datagrid.RefreshUnBoundRows();
            this.Datagrid.UpdateRowAndColumnCount(true);
            WireSerializablePropertyEvents();
            // For SourceDataGrid, WireColumnDescriptor will be called from RestoreGridSettings.
            // For DetailsViewDataGrid, need to call here
            if (this.Datagrid is DetailsViewDataGrid)
                this.Datagrid.Columns.ForEach(x => this.Datagrid.GridModel.WireColumnDescriptor(x));
        }


        /// <summary>
        /// Wires the events associates with the serializable property.
        /// </summary> 
        protected virtual void WireSerializablePropertyEvents()
        {
            this.Datagrid.SortColumnDescriptions.CollectionChanged += this.Datagrid.GridModel.OnSortColumnsChanged;
            this.Datagrid.GroupColumnDescriptions.CollectionChanged += this.Datagrid.GridModel.OnGroupColumnDescriptionsChanged;
            this.Datagrid.GroupSummaryRows.CollectionChanged += this.Datagrid.GridModel.OnSummaryRowsChanged;
            this.Datagrid.TableSummaryRows.CollectionChanged += this.Datagrid.GridModel.OnTableSummaryRowsChanged;
            this.Datagrid.UnBoundRows.CollectionChanged += this.Datagrid.GridModel.OnUnBoundRowsChanged;
            this.Datagrid.SortComparers.CollectionChanged += this.Datagrid.GridModel.SortComparers_CollectionChanged;
            (this.Datagrid.Columns as INotifyCollectionChanged).CollectionChanged += this.Datagrid.OnGridColumnCollectionChanged;
        }

        /// <summary>
        /// Unwires the events associates with the serializable property.
        /// </summary> 
        protected virtual void UnWireSerializablePropertyEvents()
        {
            if (this.Datagrid.GridModel != null)
            {
                this.Datagrid.SortColumnDescriptions.CollectionChanged -= this.Datagrid.GridModel.OnSortColumnsChanged;
                this.Datagrid.GroupColumnDescriptions.CollectionChanged -= this.Datagrid.GridModel.OnGroupColumnDescriptionsChanged;
                this.Datagrid.GroupSummaryRows.CollectionChanged -= this.Datagrid.GridModel.OnSummaryRowsChanged;
                this.Datagrid.TableSummaryRows.CollectionChanged -= this.Datagrid.GridModel.OnTableSummaryRowsChanged;
                this.Datagrid.UnBoundRows.CollectionChanged -= this.Datagrid.GridModel.OnUnBoundRowsChanged;
                this.Datagrid.SortComparers.CollectionChanged -= this.Datagrid.GridModel.SortComparers_CollectionChanged;
            }
            // while clear the GridColumns in RestoreGridColumns method we have wire OnGridColumnCollectionChanged event so need to unwire this event
            if (this.Datagrid.Columns != null)
                (this.Datagrid.Columns as INotifyCollectionChanged).CollectionChanged -= this.Datagrid.OnGridColumnCollectionChanged;
        }

        #endregion

        #region Private Method
        /// <summary>
        /// Check if the column with specified mapping name is present in grid
        /// </summary>
        /// <param name="mappingName">mappingName</param>
        /// <returns>bool</returns>
        private bool CheckColumnAvailability(string mappingName)
        {
            return Datagrid.Columns.Any(column => column.MappingName == mappingName);
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize SfDataGrid property settings.    
    /// </summary>
    [DataContract(Name = "SfDataGrid")]

    public class SerializableDataGrid
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the column can be repositioned by using mouse or touch in SerializableDataGrid.
        /// </summary>              
        [DataMember(EmitDefaultValue = false)]
        public bool AllowDraggingColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the editing is enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowEditing { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the filtering is enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the group header remains fixed at the width of display area or scrolled(horizontal scrolling) out of its visibility in SerializableDataGrid .
        /// </summary>        
        [DataMember(EmitDefaultValue = false)]
        public bool AllowFrozenGroupHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the grouping is enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowGrouping { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can adjust the column width by using the mouse in SerializableDataGrid.
        /// </summary>        
        [DataMember(EmitDefaultValue = false)]
        public bool AllowResizingColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the hidden column can be resized using the mouse in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowResizingHiddenColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the selection should be present at the PointerPressed state in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowSelectionOnPointerPressed { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the sorting is enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowSort { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data to its initial order other than Ascending or Descending state in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowTriStateSorting { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the group is expanded automatically , when the column is grouped in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AutoExpandGroups { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be created automatically in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AutoGenerateColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the columns are generated during automatic column generation .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AutoGenerateColumnsMode AutoGenerateColumnsMode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the relations for Master-Details View is generated automatically in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AutoGenerateRelations { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the column widths are determined.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridLengthUnitType ColumnSizer { get; set; }

        /// <summary>
        /// Gets or sets the width for the boundaries of the current cell border in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Thickness CurrentCellBorderThickness { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the editing action can be accomplished either at single or double click in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public EditTrigger EditTrigger { get; set; }

        /// <summary>
        /// Gets or sets the number of columns left to the SerializableDataGrid remains fixed in the view, when scrolled out of its visibility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int FrozenColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of top-most rows remains fixed in view, when scrolled out of its visibility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int FrozenRowsCount { get; set; }

        /// <summary>
        /// Gets or sets the number of right-most columns remains fixed in the view ,when scrolled out of its visibility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int FooterColumnCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bottom-wise rows remains fixed in the view , when scrolled out of its visibility.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int FooterRowsCount { get; set; }

        /// <summary>
        /// Gets or sets the format of group caption text.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GroupCaptionTextFormat { get; set; }

        /// <summary>
        /// Gets or sets the string that is used to displayed on the GroupDropArea in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GroupDropAreaText { get; set; }

        /// <summary>
        /// Gets or sets the height of the header row.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double HeaderRowHeight { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the GroupDropArea can be expanded by default.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsGroupDropAreaExpanded { get; set; }

        /// <summary>
        /// Gets or sets a value to control data manipulation operations during data updates.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public LiveDataUpdateMode LiveDataUpdateMode { get; set; }

        /// <summary>
        /// Gets or sets the height of all data rows in SerializableDataGrid .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double RowHeight { get; set; }

        /// <summary>
        /// Gets or sets the value that indicates how the rows or cells are selected in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridSelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets a value that decides the type of selection behavior to be performed in SerializableDataGrid.        
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridSelectionUnit SelectionUnit { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the column is displayed on SerializableDataGrid after it is grouped.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowColumnWhenGrouped { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusio.UI.Xaml.Grid.GroupDropArea"/> is enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowGroupDropArea { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the sequence number should be displayed on the header cell of sorted column during multi-column sorting.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowSortNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data either at single or double click.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SortClickAction SortClickAction { get; set; }

        /// <summary>
        /// Gets or sets the collection that contains all the columns in SerializableDataGrid.
        /// </summary> 
        [DataMember(EmitDefaultValue = false)]
        public SerializableColumns Columns { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> objects that are used to perform programmatic sorting.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableSortColumnDescriptions SortColumnDescriptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescription"/> object that describes how the column to be grouped in to view .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableGroupColumnDescriptions GroupColumnDescriptions { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays summary information at the footer of each group.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableGridSummaryRows GroupSummaryRows { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays the summary information at the header of each group .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableGridSummaryRow CaptionSummaryRow { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays the summary information of table either at top or bottom of SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableTableGridSummaryRows TableSummaryRows { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRow"/> that enables you to group more than one columns under particular category. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableStackedHeaderRows StackedHeaderRows { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> that displays the custom information in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableGridUnBoundRows UnBoundRows { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can delete the rows using Delete Key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowDeleting { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the AddNewRow can be positioned either at top or bottom of SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AddNewRowPosition AddNewRowPosition { get; set; }

        /// <summary>
        /// Gets or sets the Expander column width
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double ExpanderColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets the Indent column width.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double IndentColumnWidth { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the FilterRow can be positioned either at top or bottom of SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FilterRowPosition FilterRowPosition { get; set; }

        /// <summary>
        /// Gets or sets a value to enable the built-in validation (IDataErrorInfo/DataAnnonations) to validate the user input and displays the error.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridValidationMode GridValidationMode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can highlight the row being hovered through mouse or touch.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowRowHoverHighlighting { get; set; }

        /// <summary>
        /// Gets or sets the amount of data to fetch for virtualizing operations.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int DataFetchSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.NavigationMode"/> that indicates whether the navigation can be accomplished based on either cell or row in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public NavigationMode NavigationMode { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the RowHeader can be enabled in SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowRowHeader { get; set; }

        /// <summary>
        /// Gets or sets the selection behavior of editor that is loaded in GridCell.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public EditorSelectionBehavior EditorSelectionBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the Parallel LINQ is enabled during Sorting, Filtering, Grouping and summary calculation to improve performance.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool UsePLINQ { get; set; }

        /// <summary>
        /// Gets or sets a value that determines whether the underlying data source type is dynamic or not.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsDynamicItemsSource { get; set; }

        /// <summary>
        /// Gets or sets the width of the RowHeader.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double RowHeaderWidth { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the content is copied from SerializableDataGrid control to the clipboard.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridCopyOption GridCopyOption { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the clipboard value is pasted into SerializableDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridPasteOption GridPasteOption { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the busy indicator should be displayed while fetching the large amount data in <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCollectionView"/>.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowBusyIndicator { get; set; }

        /// <summary>
        /// Gets or sets the filter settings for serializing filter predicates.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableFilterSettings FilterSettings { get; set; }

        /// <summary>
        /// Gets or sets the space between the edge of DetailsViewDataGrid and its content.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Thickness DetailsViewPadding { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.ViewDefinition"/> that enables you to represent the data in to hierarchical format.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableDetailsViewDefinition DetailsViewDefinition { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the rows are reused when the ItemsSource for DetailsViewDataGrid(reusing DetailsViewDataRow) has changed.
        /// </summary>     
        [DataMember(EmitDefaultValue = false)]
        public bool ReuseRowsOnItemssourceChange { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridColumn")]
    [KnownType("KnownTypes")]
    public class SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the name to map the data member in the underlying data object to
        /// SerializableGridColumn .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MappingName { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment for the header content of the SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public HorizontalAlignment HorizontalHeaderContentAlignment { get; set; }

        /// <summary>
        /// Gets or sets the text that is displayed on the header cell of SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data by clicking on its header cell of SerializableGridColumn .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowSorting { get; set; }

        /// <summary>
        /// Gets or sets the value that indicates how the SerializableGridColumn width is determined.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridLengthUnitType? ColumnSizer { get; set; }

        /// <summary>
        /// Gets a value that determines whether the column is generated automatically. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsAutoGenerated { get; set; }

        /// <summary>
        /// Gets or sets a value that decides whether to filter based on display value or based on MappingName. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ColumnFilter ColumnFilter { get; set; }

        /// <summary>
        /// Gets or sets the member type to load the appropriate <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType"/> to <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> .       
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Type ColumnMemberType { get; set; }

        /// <summary>
        /// Gets or sets value that indicates whether the case sensitive filtering is enabled on FilterRowCell of a SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsCaseSensitiveFilterRow { get; set; }

        /// <summary>
        /// Gets or sets filtered value of the SerializableGridColumn where the filtering has been applied through FilterRow.        
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public object FilterRowText { get; set; }

        /// <summary>
        /// Gets or sets a value that decides the default FilterRowCondition that have to be filter while typing in corresponding FilterRow cell.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FilterRowCondition FilterRowCondition { get; set; }

        /// <summary>
        /// Gets or sets a value which denotes the Editor which have to be load in corresponding FilterRowCell.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FilterRowEditorType { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the FilterRowOptions button is visible in the GridFilterRowCell.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Visibility FilterRowOptionsVisibility { get; set; }

        /// <summary>
        /// Gets or sets the width of SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the SerializableGridColumn is hidden from the view.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the maximum width constraint of the SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double MaximumWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum width constraint of the SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double MinimumWidth { get; set; }

        /// <summary>
        ///  Gets or sets a value that indicates whether the user can reposition the SerializableGridColumn in to new position.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowDragging { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the grouping is enabled in SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowGrouping { get; set; }

        /// <summary>
        ///  Gets or sets a value that indicates whether the resizing is enabled in SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowResizing { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the filtering is enabled in the SerializableGridColumn .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the data is automatically filtered as soon as an user selects the value from the filter popup in SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ImmediateUpdateColumnFilter { get; set; }

        /// <summary>
        /// Get or sets a value that indicates whether the blank values are allowed for filtering in SerializableGridColumn.
        /// </summary> 
        [DataMember(EmitDefaultValue = false)]
        public bool AllowBlankFilters { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the editing is enabled in SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool? AllowEditing { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the SerializableGridColumn receives focus or not.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowFocus { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment property for the text.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TextAlignment TextAlignment { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the binding gets property value
        /// from column wrapper instead of reflecting value from the item properties.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool UseBindingValue { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the data context of <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplate"/> is <see cref="T:Syncfusion.UI.Xaml.Grid.Cells.DataContextHelper"/> instead of Record./>        
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool SetCellBoundValue { get; set; }
        /// <summary>
        /// Gets or sets the timing of binding source updates in the column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UpdateSourceTrigger UpdateTrigger { get; set; }
        /// <summary>
        /// Gets or sets the space between the edge of cell and its content with in the specified SerializableGridColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Thickness Padding { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can validate their input either in display or when the cell lost its focus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public GridValidationMode GridValidationMode { get; set; }

        /// <summary>
        /// Gets a value that determines whether the SerializableGridColumn is filtered either from excel or advanced filter type.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FilteredFrom FilteredFrom { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the tool tip is displayed on the column being mouse hovered on it.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowToolTip { get; set; }

        /// <summary>
        /// Gets or sets value that indicates whether the tool tip should be displayed being mouse hovered on the header cell of column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowHeaderToolTip { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the data context of <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.ToolTipTemplate"/> is <see cref="T:Syncfusion.UI.Xaml.Grid.Cells.DataContextHelper"/> instead of Record./>        
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool SetCellBoundToolTip { get; set; }

        /// <summary>
        /// Gets the reference of the SfDataGrid control.
        /// </summary>
        public static SfDataGrid DataGrid;

        /// <summary>
        /// Gets the known column types during serialization and deserialization operation.
        /// </summary>
        /// <returns>
        /// Returns the corresponding column type.
        /// </returns>
        public static Type[] KnownTypes()
        {
            return DataGrid.SerializationController.KnownTypes();
        }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridTextColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridTextColumn")]

    public class SerializableGridTextColumn : SerializableGridColumn
    {
        [DataMember(EmitDefaultValue = false)]
        /// <summary>
        /// Gets or sets a value that indicates how cell content should wrap the text in the SerializableGridTextColumn.
        /// </summary>
        public TextWrapping TextWrapping { get; set; }
#if UWP
        [DataMember(EmitDefaultValue = false)]
        public bool IsSpellCheckEnabled { get; set; }
#endif
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridTemplateColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridTemplateColumn")]

    public class SerializableGridTemplateColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the horizontal alignment for the SerializableGridTemplateColumn .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment for the SerializableGridTemplateColumn .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public VerticalAlignment VerticalAlignment { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridComboBoxColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridComboBoxColumn")]

    public class SerializableGridComboBoxColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the path that is used to get the SelectedValue from the SelectedItem.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string SelectedValuePath { get; set; }

        /// <summary>
        /// Gets or sets the path that is used to display the visual representation of object.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DisplayMemberPath { get; set; }

#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether a SerializableGridComboBoxColumn that opens and displays a drop-down control when a user clicks its text area .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool StaysOpenOnEdit { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can edit the cell value by typing through editor of SerializableGridComboBoxColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsEditable { get; set; }
#endif
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridCheckBoxColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridCheckBoxColumn")]
    public class SerializableGridCheckBoxColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the horizontal alignment for the column .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment for the column .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can enable the Intermediate state of the CheckBox other than the Checked and Unchecked state.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsThreeState { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridMultiColumnDropDownList property settings.    
    /// </summary>
    [DataContract(Name = "GridMultiColumnDropDownList")]

    public class SerializableGridMultiColumnDropDownList : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets a string that specifies the name of data member to represent its value to the display mode of the SfMultiColumnDropDownControl .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DisplayMember { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies the name of data member to display its values to the drop-down list of SfMultiColumnDropDownControl.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ValueMember { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the resizing cursor is visible at the edge of the drop-down popup.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Visibility ShowResizeThumb { get; set; }

        /// <summary>
        /// Gets or sets the height of the drop-down popup.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpHeight { get; set; }

        /// <summary>
        /// Gets or sets the width of the drop-down popup.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpWidth { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether cells in the column will match the characters being entered in the cell with one from the possible selections.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowAutoComplete { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether cell values of SerializableGridMultiColumnDropDownList column can be rotated using the mouse wheel or up and down arrow key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowSpinOnMouseWheel { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can filter the values from drop-down grid dynamically being characters entered on the cell in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowIncrementalFiltering { get; set; }

        /// <summary>
        /// Gets or sets a value for search conditions when <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering"/> is enabled.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SearchCondition SearchCondition { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the case-sensitive is enabled during <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering"/> in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowCasingforFilter { get; set; }

        /// <summary>
        /// Gets or sets the maximum height constraint of the popup in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpMaxHeight { get; set; }

        /// <summary>
        /// Gets or sets the maximum width constraint of the popup in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpMaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the minimum height constraint of popup in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpMinHeight { get; set; }

        /// <summary>
        /// Gets or sets the minimum width constraint of popup in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double PopUpMinWidth { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the value in the editor of SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsTextReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in the SerializableGridMultiColumnDropDownList column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowNullInput { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be created automatically in SerializableGridMultiColumnDropDownList.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AutoGenerateColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the width of SerializableGridMultiColumnDropDownList is determined.
        /// </summary>   
        [DataMember(EmitDefaultValue = false)]
        public GridLengthUnitType GridColumnSizer { get; set; }

        /// <summary>
        /// Gets or sets value that indicates whether the popup size is adjusted automatically based on its content.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsAutoPopupSize { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates how the columns are generated during automatic column generation.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public AutoGenerateColumnsMode AutoGenerateColumnsMode { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridUnBoundColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridUnBoundColumn")]

    public class SerializableGridUnBoundColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the case sensitive is enabled in SerializableGridUnBoundColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets the format of value displayed in SerializableGridUnBoundColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the expression used to calculate the values in SerializableGridUnBoundColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Expression { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridDateTimeColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridDateTimeColumn")]
#if WPF
    [KnownType(typeof(GregorianCalendar))]
#endif
    public class SerializableGridDateTimeColumn : SerializableGridColumn
    {
#if UWP
        /// <summary>
        /// Gets or sets a value that indicates whether the editing is enabled for SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowInlineEditing { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the bounded value in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FormatString { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a drop-down button control is used to adjust the date time value.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowDropDownButton { get; set; }
#else
        /// <summary>
        /// Gets or sets a value that indicates whether the user can rotate the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowScrollingOnCircle { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowNullValue { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the classic style is enabled on the drop-down of SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool EnableClassicStyle { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the date selection is disabled on the calendar popup of SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool DisableDateSelection { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a repeat button control is used to adjust the date and time value in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowRepeatButton { get; set; }

        /// <summary>
        /// Gets or sets a value that is displayed instead of null value if the cell value contains null in SerializableGridDateTimeColumn.
        /// </summary>  
        [DataMember(EmitDefaultValue = false)]
        public DateTime? NullValue { get; set; }

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value contains null in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NullText { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="System.Globalization.DateTimeFormatInfo"/> that defines the format of date and time values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTimeFormatInfo DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets a value that decides whether the date and time value can be edited in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool EnableBackspaceKey { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool EnableDeleteKey { get; set; }

        /// <summary>
        /// Gets or sets the format string for a date and time value in SerializableGridDateTimeColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTimePattern Pattern { get; set; }

        /// <summary>
        /// Gets or sets the custom pattern for date and time value.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CustomPattern { get; set; }
#endif
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridHyperlinkColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridHyperlinkColumn")]
    public class SerializableGridHyperlinkColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the horizontal alignment of the SerializableGridHyperlinkColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment of the SerializableGridHyperlinkColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public VerticalAlignment VerticalAlignment { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridEditorColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridEditorColumn")]
    public class SerializableGridEditorColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the user can rotate the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowScrollingOnCircle { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed to the editor columns.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowNullValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum value constraint of the column.
        /// </summary>       
        [DataMember(EmitDefaultValue = false)]
        public decimal MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value constraint of the column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public decimal MaxValue { get; set; }

        /// <summary>
        /// Gets or sets an object that is displayed instead of null value if the cell value contains null.
        /// </summary>    
        [DataMember(EmitDefaultValue = false)]
        public object NullValue { get; set; }

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value contains null.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NullText { get; set; }

#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValue"/> can be validated either key press or focus lost on editor in SerializableGridEditorColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MaxValidation MaxValidation { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValue"/> can be validated either key press or focus lost on editor in SerializableGridEditorColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MinValidation MinValidation { get; set; }
#endif
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridNumericColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridNumericColumn")]
#if WPF
    public class SerializableGridNumericColumn : SerializableGridEditorColumn
#else     
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridNumericColumn property settings.    
    /// </summary>
    public class SerializableGridNumericColumn : SerializableGridColumn
#endif
    {
#if UWP
         /// <summary>
        /// Gets or sets a value indicating whether the characters is blocked from an user input.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool BlockCharactersOnTextInput { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in SerializableGridNumericColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowNullInput { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the bound value in SerializableGridNumericColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string FormatString { get; set; }

        /// <summary>
        /// Gets or sets a value that decides whether the user can parse either decimal or double value in SerializableGridNumericColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Parsers ParsingMode { get; set; }
#else
        /// <summary>
        /// Gets or sets the number of decimal places to use in numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int NumberDecimalDigits { get; set; }

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NumberDecimalSeparator { get; set; }

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NumberGroupSeparator { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the group separator is enabled .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool EnableGroupSeparator { get; set; }

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Int32Collection NumberGroupSizes { get; set; }

        /// <summary>
        /// Gets or sets the format pattern for negative numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int NumberNegativePattern { get; set; }
#endif
    }

#if UWP
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridUpDownColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridUpDownColumn")]
    public class SerializableGridUpDownColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets the value to increment or decrement the UpDown value when the up or down buttons are clicked.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double SmallChange { get; set; }

        /// <summary>
        /// Gets or sets the culture of SerializableGridUpDownColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed value for the SerializableGridUpDownColumn. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed value for the SerializableGridUpDownColumn. 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places to use in numeric values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int NumberDecimalDigits { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the column automatically reverses the value when it reaches MinValue or MaxValue.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AutoReverse { get; set; }

        /// <summary>
        /// Gets or sets a value that decides whether the user can parse either decimal or double value in SerializableGridUpDownColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Parsers ParsingMode { get; set; }
    }
#endif

#if WPF
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridCurrencyColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridCurrencyColumn")]
    public class SerializableGridCurrencyColumn : SerializableGridEditorColumn
    {
        /// <summary>
        /// Gets or sets the number of decimal places to use in currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CurrencyDecimalDigits { get; set; }

        /// <summary>
        /// Gets or sets the string that separates the group of digits to the left of the decimal in currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CurrencyGroupSeparator { get; set; }

        /// <summary>
        /// Gets or sets the string that is used as the currency symbol.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CurrencySymbol { get; set; }

        /// <summary>
        /// Gets or sets the string that separates the decimal part in currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string CurrencyDecimalSeparator { get; set; }

        /// <summary>
        /// Gets or sets the format pattern of positive currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CurrencyPositivePattern { get; set; }

        /// <summary>
        /// Gets or sets the format pattern of negative currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int CurrencyNegativePattern { get; set; }

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in currency values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Int32Collection CurrencyGroupSizes { get; set; }
    }
#endif
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridMaskColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridMaskColumn")]
    public class SerializableGridMaskColumn : SerializableGridColumn
    {
#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether the entire cell value is selected when it receives focus.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool SelectTextOnFocus { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the SerializableGridMaskColumn that loads the numeric values in it.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsNumeric { get; set; }

        /// <summary>
        /// Gets or sets the string that separates the components of date,that is, day ,month and year in SerializableGridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DateSeparator { get; set; }

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string DecimalSeparator { get; set; }

        /// <summary>
        /// Gets or sets the string that separates the components of time, that is, the hour , minutes and seconds .
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string TimeSeparator { get; set; }

#else
        /// <summary>
        /// Gets or sets the type of mask used in SerializableGridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MaskType MaskType { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the input validated either key press or focus lost on editor in SerializableGridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public InputValidationMode ValidationMode { get; set;}

        /// <summary>
        /// Gets or sets the keyboard options for the SerializableGridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public KeyboardOptions KeyboardType { get; set;}

        /// <summary>
        /// Gets or sets the culture for the SerializableGridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CultureInfo Culture {get; set;}
#endif
        /// <summary>
        /// Gets or sets the input mask to use at runtime.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Mask { get; set; }

        /// <summary>
        /// Gets or sets the format of masked input.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MaskFormat MaskFormat { get; set; }

        /// <summary>
        /// Gets or sets the character used to represent the absence of user input in GridMaskColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public char PromptChar { get; set; }
    }
#if WPF
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridPercentageColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridPercentageColumn")]
    public class SerializableGridPercentageColumn : SerializableGridEditorColumn
    {
        /// <summary>
        /// Gets or sets the number of decimal places to use in percent values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PercentDecimalDigits { get; set; }

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in percent values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PercentDecimalSeparator { get; set; }

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in percent values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PercentGroupSeparator { get; set; }

        /// <summary>
        /// Gets or sets the format pattern for negative values in SerializableGridPercentageColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PercentNegativePattern { get; set; }

        /// <summary>
        /// Gets or sets the format pattern for the positive values in SerializableGridPercentageColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int PercentPositivePattern { get; set; }

        /// <summary>
        /// Gets or sets the string to use as the percent symbol.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string PercentSymbol { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the percent editor loads either percent or double value being edited in SerializableGridPercentageColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PercentEditMode PercentEditMode { get; set; }

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in percent values.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Int32Collection PercentGroupSizes { get; set; }
    }
#endif
    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridTimeSpanColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridTimeSpanColumn")]
    public class SerializableGridTimeSpanColumn : SerializableGridColumn
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in SerializableGridTimeSpanColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowNull { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can rotate the cell values using mouse wheel or up and down arrow key.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool AllowScrollingOnCircle { get; set; }

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value contains null in SerializableGridTimeSpanColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string NullText { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the time span value.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a arrow button control is used to adjust the time span value.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowArrowButtons { get; set; }

        /// <summary>
        /// Gets or sets the maximum value allowed for SerializableGridTimeSpanColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum value allowed for SerializableGridTimeSpanColumn.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan MinValue { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize columns property settings.    
    /// </summary>
    public class SerializableColumns : ObservableCollection<SerializableGridColumn>
    {
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize SortColumnDescription property settings.    
    /// </summary>
    [DataContract(Name = "SortColumnDescription")]
    public class SerializableSortColumnDescription
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>        
        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ListSortDirection SortDirection { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize SortColumnDescriptions property settings.    
    /// </summary>
    public class SerializableSortColumnDescriptions : ObservableCollection<SerializableSortColumnDescription>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GroupColumnDescription property settings.    
    /// </summary>
    [DataContract(Name = "GroupColumnDescription")]
    public class SerializableGroupColumnDescription
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>  
        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GroupColumnDescriptions property settings.    
    /// </summary>
    public class SerializableGroupColumnDescriptions : ObservableCollection<SerializableGroupColumnDescription>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridSummaryRow property settings.    
    /// </summary>
    [DataContract(Name = "GridSummaryRow")]
    public class SerializableGridSummaryRow 
    {
        /// <summary>
        /// Gets or sets the name of the SerializableGridSummaryRow .
        /// </summary>      
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the summary value should be displayed corresponding to its column or row basis.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowSummaryInRow { get; set; }

        /// <summary>
        /// Gets or sets the collection of summary columns.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ObservableCollection<SerializableGridSummaryColumn> SummaryColumns { get; set; }

        /// <summary>
        /// Gets or sets the title of SerializableGridSummaryRow.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TitleColumnCount { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridTableSummaryRow property settings.    
    /// </summary>
    [DataContract(Name = "GridTableSummaryRow")]
    public class SerializableGridTableSummaryRow : SerializableGridSummaryRow
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the serializable table summary row is positioned at either the top or bottom of the SfDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TableSummaryRowPosition Position { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridSummaryRows property settings.    
    /// </summary>
    public class SerializableGridSummaryRows : ObservableCollection<SerializableGridSummaryRow>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridTableSummaryRows property settings.    
    /// </summary>
    public class SerializableTableGridSummaryRows : ObservableCollection<SerializableGridTableSummaryRow>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridSummaryColumn property settings.    
    /// </summary>
    [DataContract(Name = "GridSummaryColumn")]
    public class SerializableGridSummaryColumn
    {
        /// <summary>
        /// Gets or sets the string that indicates how the summary value is formatted in display.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MappingName"/> of the column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string MappingName { get; set; }

        /// <summary>
        /// Gets or sets the name of summary column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the summary.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SummaryType SummaryType { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize StackedHeaderRows property settings.    
    /// </summary>
    public class SerializableStackedHeaderRows : ObservableCollection<SerializableStackedHeaderRow>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize StackedHeaderRow property settings.    
    /// </summary>
    [DataContract(Name = "StackedHeaderRow")]

    public class SerializableStackedHeaderRow
    {
        /// <summary>
        /// Gets or sets the name of the SerializableStackedHeaderRow.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the collection of the serializable StackedColumns to group under particular category.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableStackedColumns StackedColumns { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize StackedColumns property settings.    
    /// </summary>
    public class SerializableStackedColumns : ObservableCollection<SerializableStackedColumn>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize StackedColumn property settings.    
    /// </summary>
    [DataContract(Name = "StackedColumn")]
    public class SerializableStackedColumn
    {
        /// <summary>
        /// Gets or sets the name of child columns that need to be stacked under the specified serializable stacked column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ChildColumns { get; set; }

        /// <summary>
        /// Gets or sets the header text of the serializable stacked column.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string HeaderText { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridUnBoundRow property settings.    
    /// </summary>
    [DataContract(Name = "UnBoundRow")]
    public class SerializableGridUnBoundRow
    {
        /// <summary>
        /// Gets the row index of the UnBouSerializableGridUnBoundRowndRow.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets the index of UnBoundRow from the SerializableGridUnBoundRows collection.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int UnBoundRowIndex { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the SerializableGridUnBoundRow should be displayed above or below of the SerializableTableGridSummaryRows.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ShowBelowSummary { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the SerializableGridUnBoundRow is positioned at either top or bottom of SfDataGrid.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UnBoundRowsPosition Position { get; set; }
    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize GridUnBoundRows property settings.    
    /// </summary>
    public class SerializableGridUnBoundRows : ObservableCollection<SerializableGridUnBoundRow>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize filter settings.
    /// </summary>
    public class SerializableFilterSettings : ObservableCollection<SerializableFilterSetting>
    {

    }

    /// <summary>
    /// Represents a class that is used to serialize and deserialize filter settings.
    /// </summary>
    [DataContract(Name = "SerializableFilterSetting")]
    public class SerializableFilterSetting
    {
        /// <summary>
        /// Gets or sets the name of the column to serialize filter settings.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the collection of serializable filter.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ObservableCollection<SerializableFilter> Filter { get; set; }
    }
    /// <summary>
    /// Represents a class that is used to serialize and deserialize filters.
    /// </summary>
    [DataContract(Name = "SerializableFilter")]
    public class SerializableFilter
    {
        /// <summary>
        /// Gets or sets the type of filter.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FilterType FilterType { get; set; }

        /// <summary>
        /// Gets or sets the filter value.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public object FilterValue { get; set; }

        /// <summary>
        /// Gets or sets the type of the predicate.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PredicateType PredicateType { get; set; }

        /// <summary>
        /// Gets or sets the type of the filter behavior.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public FilterBehavior FilterBehavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the case sensitive filter is enabled.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsCaseSensitive { get; set; }
    }

    /// <summary>
    /// Represents a class that provides options to serialize SfDataGrid settings.
    /// </summary>
    public class SerializationOptions
    {
        bool serializeSorting = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the sorting should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the sorting is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeSorting
        {
            get
            {
                return serializeSorting;
            }
            set
            {
                serializeSorting = value;
            }
        }

        bool serializeGrouping = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the grouping should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the grouping is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeGrouping
        {
            get
            {
                return serializeGrouping;
            }
            set
            {
                serializeGrouping = value;
            }
        }


        bool serializeGroupSummaries = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the GroupSummaryRows should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the GroupSummaryRows is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeGroupSummaries
        {
            get
            {
                return serializeGroupSummaries;
            }
            set
            {
                serializeGroupSummaries = value;
            }
        }

        bool serializeCaptionSummary = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the CaptionSummaryRow should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the CaptionSummaryRow is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeCaptionSummary
        {
            get
            {
                return serializeCaptionSummary;
            }
            set
            {
                serializeCaptionSummary = value;
            }
        }

        bool serializeTableSummaries = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the TableSummaryRows should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the TableSummaryRows is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeTableSummaries
        {
            get
            {
                return serializeTableSummaries;
            }
            set
            {
                serializeTableSummaries = value;
            }
        }

        bool serializeunBoundRows = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the unbound rows should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the unbound rows is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeUnBoundRows
        {
            get
            {
                return serializeunBoundRows;
            }
            set
            {
                serializeunBoundRows = value;
            }
        }

        bool serializeFiltering = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the filter predicates should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the filter predicates is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeFiltering
        {
            get
            {
                return serializeFiltering;
            }
            set
            {
                serializeFiltering = value;
            }
        }

        bool serializeColumns = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the columns is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeColumns
        {
            get
            {
                return serializeColumns;
            }
            set
            {
                serializeColumns = value;
            }
        }

        bool serializeStackedHeaders = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the StackedHeaderRows should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the StackedHeaderRows is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeStackedHeaders
        {
            get
            {
                return serializeStackedHeaders;
            }
            set
            {
                serializeStackedHeaders = value;
            }
        }

        bool serializeDetailsViewDefinition = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the DetailsViewDefinition should be serialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the stacked headers is serialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool SerializeDetailsViewDefinition
        {
            get
            {
                return serializeDetailsViewDefinition;
            }
            set
            {
                serializeDetailsViewDefinition = value;
            }
        }

    }
    /// <summary>
    /// Represents a class that provide options to deserialize the SfDataGrid settings.
    /// </summary>
    public class DeserializationOptions
    {
        bool deserializeSorting = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the sorting should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the sorting is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeSorting
        {
            get
            {
                return deserializeSorting;
            }
            set
            {
                deserializeSorting = value;
            }
        }

        bool deserializeGrouping = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the grouping should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the grouping is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeGrouping
        {
            get
            {
                return deserializeGrouping;
            }
            set
            {
                deserializeGrouping = value;
            }
        }

        bool deserializeCaptionSummary = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the CaptionSummaryRow should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the CaptionSummaryRow is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeCaptionSummary
        {
            get
            {
                return deserializeCaptionSummary;
            }
            set
            {
                deserializeCaptionSummary = value;
            }
        }

        bool deserializeGroupSummaries = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the GroupSummaryRow should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the GroupSummaryRow is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeGroupSummaries
        {
            get
            {
                return deserializeGroupSummaries;
            }
            set
            {
                deserializeGroupSummaries = value;
            }
        }

        bool deserializeTableSummaries = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the TableSummaryRows should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the TableSummaryRows is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeTableSummaries
        {
            get
            {
                return deserializeTableSummaries;
            }
            set
            {
                deserializeTableSummaries = value;
            }
        }

        bool deserializeUnBoundRows = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the unbound rows should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the unbound rows is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeUnBoundRows
        {
            get
            {
                return deserializeUnBoundRows;
            }
            set
            {
                deserializeUnBoundRows = value;
            }
        }

        bool deserializeFiltering = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the filtering should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the filtering is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeFiltering
        {
            get
            {
                return deserializeFiltering;
            }
            set
            {
                deserializeFiltering = value;
            }
        }

        bool deserializeColumns = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the columns is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeColumns
        {
            get
            {
                return deserializeColumns;
            }
            set
            {
                deserializeColumns = value;
            }
        }

        bool deserializeStackedHeaders = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the StackedHeaderRows should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the StackedHeaderRows is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeStackedHeaders
        {
            get
            {
                return deserializeStackedHeaders;
            }
            set
            {
                deserializeStackedHeaders = value;
            }
        }

        bool deserializeDetailsViewDefinition = true;
        /// <summary>
        /// Gets or sets a value that indicates whether the DetailsViewDefinition should be deserialized in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the DetailsViewDefinition is deserialized; otherwise, <b>false</b>. The default is <b>true</b>.
        /// </value>
        public bool DeserializeDetailsViewDefinition
        {
            get
            {
                return deserializeDetailsViewDefinition;
            }
            set
            {
                deserializeDetailsViewDefinition = value;
            }
        }
    }

    /// <summary>
    /// Provides the base implementation for serializing and deserializing the GridViewDefinition in SfDataGrid.
    /// </summary>
    [DataContract(Name = "GridViewDefinition")]
    public class SerializableGridViewDefinition
    {
        /// <summary>
        /// Gets or sets the reference to the SfDataGrid instance.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SerializableDataGrid DataGrid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the relational column name to form master details view relation.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RelationalColumn
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Provides the base implementation for serializing and deserializing the DetailsViewDefinition in SfDataGrid.
    /// </summary>   
    public class SerializableDetailsViewDefinition : ObservableCollection<SerializableGridViewDefinition>
    {

    }
}
