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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Helper;
using System.Reflection;
using System.Collections;
using System.Dynamic;
using Syncfusion.Dynamic;

namespace Syncfusion.UI.Xaml.TreeGrid.Helpers
{
    public static class TreeGridHelper
    {
#if WPF
        internal static bool CanAllowNull(this SfTreeGrid treeGrid, PropertyDescriptor propertyinfo)
#else
        internal static bool CanAllowNull(this SfTreeGrid treeGrid, PropertyInfo propertyinfo)
#endif
        {
            var nullablememberType = NullableHelperInternal.GetNullableType(propertyinfo.PropertyType);
            return propertyinfo.PropertyType.IsAssignableFrom(nullablememberType);
        }
        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> of SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var treePanel = this.treeGrid.GetTreePanel();
        /// ]]></code>
        /// </example>
        public static TreeGridPanel GetTreePanel(this SfTreeGrid treeGrid)
        {
            return treeGrid.TreeGridPanel;
        }

        /// <summary>
        /// Gets the value for the specific property.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="property">the property whose value needs to return.</param>
        /// <returns>value.</returns>
        internal static object GetValue(object data, string propertyName)
        {
            if (DynamicHelper.CheckIsDynamicObject(data.GetType()))
            {
                return new DynamicHelper().GetValue(data, propertyName);
                //return DynamicPropertiesProvider.GetDynamicValue(data, propertyName);
            }
            else
            {
#if WPF
                PropertyDescriptorCollection descriptor = TypeDescriptor.GetProperties(data.GetType());
#else
                PropertyInfoCollection descriptor = new PropertyInfoCollection(data.GetType());
#endif
                return descriptor.GetValue(data, propertyName);
            }
        }


        internal static bool SetValue(object data, string propertyName, object value)
        {
            if (DynamicHelper.CheckIsDynamicObject(data.GetType()))
            {
                return new DynamicHelper().SetValue(data, propertyName, value);
                //return DynamicPropertiesProvider.SetDynamicValue(data, propertyName, value);
            }
            else
            {
#if WPF
                PropertyDescriptorCollection descriptor = TypeDescriptor.GetProperties(data.GetType());
#else
                PropertyInfoCollection descriptor = new PropertyInfoCollection(data.GetType());
#endif
                return descriptor.SetValue(data, value, propertyName);
            }
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper"/> of SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper"/>.
        /// </returns>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// var validationHelper = this.treeGrid.GetValidationHelper();
        /// ]]></code>
        /// </example>
        public static TreeGridValidationHelper GetValidationHelper(this SfTreeGrid treeGrid)
        {
            return treeGrid.ValidationHelper;
        }
      
        /// <summary>
        /// Updates the data row in the given row index.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <param name="rowIndex">
        /// Specifies the row index to update the data row in view.
        /// </param>       
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// this.treeGrid.UpdateDataRow(1);
        /// ]]></code>
        /// </example>
        public static void UpdateDataRow(this SfTreeGrid treeGrid, int index)
        {
            treeGrid.TreeGridModel.UpdateDataRow(index);
        }

        /// <summary>
        /// Gets the TreeGridModel of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>
        /// The TreeGridModel of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </returns>
        public static TreeGridModel GetGridModel(this SfTreeGrid treeGrid)
        {
            return treeGrid.TreeGridModel;
        }

        /// <summary>
        /// Gets the TreeGridRowGenerator of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>The TreeGridRowGenerator of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.</returns>
        public static TreeGridRowGenerator GetTreeGridRowGenerator(this SfTreeGrid treeGrid)
        {
            return treeGrid.RowGenerator;
        }

        internal static IList GetSourceListCollection(IEnumerable collection)
        {
            if (collection == null)
                return null;
            IList list = null;

            if ((collection as IList) != null)
            {
                list = collection as IList;
            }
#if WPF
            else if ((collection as IListSource) != null)
            {
                var listSource = collection as IListSource;
                list = listSource.GetList();
            }
#endif
            return list;
        }

        /// <summary>
        /// Refreshes the column count, width in SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        public static void RefreshColumns(this SfTreeGrid treeGrid)
        {
            if (treeGrid.TreeGridPanel == null)
                return;
            // Update column count
            treeGrid.UpdateColumnCount(false);

            // Update Indent column widths
            treeGrid.UpdateIndentColumnWidths();

            // Freeze columns updated when adding and removing columns
            treeGrid.UpdateFreezePaneColumns();
            // Update the scroll bars
            treeGrid.TreeGridPanel.UpdateScrollBars();
            treeGrid.TreeGridPanel.NeedToRefreshColumn = true;
            treeGrid.TreeGridPanel.InvalidateMeasure();

            if (treeGrid.TreeGridPanel.ColumnCount > 0)
            {
                // Refresh Column sizer
                treeGrid.TreeGridColumnSizer.Refresh();
            }
        }

        #region TreeGridUnboundView data manipulations

        /// <summary>
        /// Add the data to the child collection of particular tree node.
        /// </summary>
        /// <param name="treeGrid">treeGrid.</param>
        /// <param name="node">The parent node. this is null if root node needs to be added.</param>
        /// <param name="data">the data.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void AddNode(this SfTreeGrid treeGrid, TreeNode node, object data)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("AddNode method is not applicable if RequestTreeItems is not used");
            view.AddNode(node, data);
        }

        /// <summary>
        /// Insert the data into the child collection of particular tree node at the specified index.
        /// </summary>
        /// <param name="treeGrid"></param>
        /// <param name="node">The parent node. this is null if root node needs to be inserted.</param>
        /// <param name="data">the data.</param>
        /// /// <param name="index">the index.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void InsertNode(this SfTreeGrid treeGrid, TreeNode node, object data, int index)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("InsertNode method is not applicable if RequestTreeItems is not used");
            view.InsertNode(node, data, index);
        }

        /// <summary>
        /// Remove the data from the child collection of particular tree node.
        /// </summary>
        /// <param name="treeGrid">treeGrid.</param>
        /// <param name="node">The parent node. this is null if root node needs to be removed.</param>
        /// <param name="data">the data.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void RemoveNode(this SfTreeGrid treeGrid, TreeNode node, object data)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("RemoveNode method is not applicable if RequestTreeItems is not used");
            view.RemoveNode(node, data);
        }


        /// <summary>
        /// Clear child nodes of the particular node.
        /// </summary>
        /// <param name="node">The parent node. this is null if root nodes need to be cleared.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void ResetNodes(this SfTreeGrid treeGrid, TreeNode node)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("ResetNodes method is not applicable if RequestTreeItems is not used");
            view.ResetNodes(node);
        }

        /// <summary>
        /// Moves the child node from an index to the another index in child nodes of particular tree node.
        /// </summary>
        /// <param name="treeGrid">treeGrid.</param>
        /// <param name="node">The parent node. this is null if root node needs to be moved.</param>
        /// <param name="oldIndex">the oldIndex.</param>
        /// <param name="newIndex">the newIndex.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void MoveNode(this SfTreeGrid treeGrid, TreeNode node, int oldIndex, int newIndex)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("MoveNode method is not applicable if RequestTreeItems is not used");

            view.MoveNode(node, oldIndex, newIndex);
        }

        /// <summary>
        /// Replaces the node at specified index with the data in child nodes of the particular tree node. 
        /// </summary>
        /// <param name="treeGrid">treeGrid.</param>
        /// <param name="node">The parent node. this is null if root node needs to be replaced.</param>
        /// <param name="data">the data.</param>
        /// <param name="index">the index.</param>
        /// <remarks>
        /// This is applicable only when you use the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RequestTreeItems"/> event to populate the treegrid.
        /// </remarks>
        public static void ReplaceNode(this SfTreeGrid treeGrid, TreeNode node, object data, int index)
        {
            var view = treeGrid.View as TreeGridUnboundView;
            if (view == null)
                throw new InvalidOperationException("ReplaceNode method is not applicable if RequestTreeItems is not used");
            view.ReplaceNode(node, data, index);
        }

        #endregion
    }

    /// <summary>
    /// Provides classes that simplify programming by providing ready made solution 
    /// for certain functionalities of SfTreeGrid.
    /// </summary>
    class NamespaceDoc
    {

    }
}
