#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Controls.DataPager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Syncfusion.UI.Xaml.Grid
{
    public class SfGridCellAutomationPeer : FrameworkElementAutomationPeer
    {
        # region Contructor
        public SfGridCellAutomationPeer(GridCell gridCell)
            : base(gridCell)
        {
   
        }
        #endregion

        # region Overrides
  
        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            var gridCell = this.Owner as GridCell;
            if (gridCell != null && gridCell.ColumnBase != null && !gridCell.ColumnBase.IsEditing)
            {
                string formattedValue = "";
                object value = null;
                if (gridCell.ColumnBase.ColumnElement != null)
                { value = gridCell.ColumnBase.ColumnElement.DataContext; }
                if (gridCell.ColumnBase.GridColumn != null && gridCell.ColumnBase.GridColumn.DataGrid != null && gridCell.ColumnBase.GridColumn.DataGrid.View != null && gridCell.ColumnBase.GridColumn.MappingName != null)
                {
                    var tempFormatValue = gridCell.ColumnBase.GridColumn.DataGrid.View.GetPropertyAccessProvider().GetFormattedValue(value, gridCell.ColumnBase.GridColumn.MappingName);
                    formattedValue = tempFormatValue == null ? null : tempFormatValue.ToString();
                    return formattedValue;
                }
            }

            return this.Owner.GetType().Name;
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            if ((this.Owner as GridCell).ColumnBase == null)
                return AutomationControlType.Custom;
            return (this.Owner as GridCell).ColumnBase.IsEditing ? AutomationControlType.Edit : AutomationControlType.Custom;
        }

        //WPF-30436 Need to override it and return true to find the disabled Control in coded UI
        protected override bool IsEnabledCore()
        {
            return true;
        }

        protected override string GetItemStatusCore()
        {
            if (!AutomationPeerHelper.EnableCodedUI)
                return base.GetItemStatusCore();
            
            var gridCell = this.Owner as GridCell;
            if (gridCell != null && gridCell.ColumnBase != null && !gridCell.ColumnBase.IsEditing)
            {
                var gridCellItems = new StringBuilder();
                string cellValue = "";
                string formattedValue = "";
                object value = null;
                if (gridCell.ColumnBase.ColumnElement != null)
                { value = gridCell.ColumnBase.ColumnElement.DataContext; }
                //Added the Null Checks for the inciednt 176084 and also added for Null Check when use GridTemplatecolumn without using MaapingName
                if (gridCell.ColumnBase.GridColumn != null && gridCell.ColumnBase.GridColumn.DataGrid != null && gridCell.ColumnBase.GridColumn.DataGrid.View != null && gridCell.ColumnBase.GridColumn.MappingName != null)
                {
                    var tempValue = gridCell.ColumnBase.GridColumn.DataGrid.View.GetPropertyAccessProvider().GetValue(value, gridCell.ColumnBase.GridColumn.MappingName);
                    cellValue = tempValue == null ? null : tempValue.ToString();
                    var tempFormatValue = gridCell.ColumnBase.GridColumn.DataGrid.View.GetPropertyAccessProvider().GetFormattedValue(value, gridCell.ColumnBase.GridColumn.MappingName);
                    formattedValue = tempFormatValue == null ? null : tempFormatValue.ToString();
                }
                gridCellItems.Append(string.Format("{0}#", cellValue));
                gridCellItems.Append(string.Format("{0}#", formattedValue));
                gridCellItems.Append(string.Format("{0}#", gridCell.ColumnBase.RowIndex));
                gridCellItems.Append(string.Format("{0}#", gridCell.ColumnBase.ColumnIndex));
                gridCellItems.Append(string.Format("{0}#", gridCell.ColumnBase.GridColumn.MappingName));
                gridCellItems.Append(string.Format("{0}#", gridCell.ColumnBase.GridColumn.HeaderText));
                return gridCellItems.ToString();
            }
            return base.GetItemStatusCore();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            //we have provided the support for detect the inner child of the Grid cell. 
            if (AutomationPeerHelper.EnableCodedUI)
                return base.GetChildrenCore();

            var gridcell = Owner as GridCell;
            if ((gridcell.ColumnBase == null) || (gridcell.ColumnBase.GridColumn == null) || (gridcell.ColumnBase.GridColumn is GridTemplateColumn
               || gridcell.ColumnBase.GridColumn.hasCellTemplate) || gridcell.ColumnBase.GridColumn is GridCheckBoxColumn
               || gridcell.ColumnBase.GridColumn is GridHyperlinkColumn)
                return base.GetChildrenCore();
            else
            {
                if (gridcell.UseDrawing)
                    return null;
            }

            return base.GetChildrenCore();
        }
        
        #endregion

        #region PrivateMethods

        private UIElement GetChildAutomationElement()
        {
            if ((this.Owner as GridCell).Content is UIElement)
                return ((this.Owner as GridCell).Content) as UIElement;
            else
                return null;
        }

        #endregion

    }

}

