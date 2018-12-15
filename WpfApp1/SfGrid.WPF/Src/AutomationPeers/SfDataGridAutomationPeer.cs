#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace Syncfusion.UI.Xaml.Grid
{
    public class SfDataGridAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Contructor
        public SfDataGridAutomationPeer(SfDataGrid dataGrid)
            : base(dataGrid)
        {

        }
        # endregion

        # region Overrides

        protected override string GetNameCore()
        { 
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Grid";
        }

        protected override string GetClassNameCore()
        {
            if (this.Owner is DetailsViewDataGrid)
                return "DetailsViewDataGrid";
            return "SfDataGrid";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
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

            if (!(this.Owner is SfDataGrid))
                return null;

            var grid = this.Owner as SfDataGrid;
            if (grid.View != null)
            {
                int RowCount = grid.View.Records.Count;
                int ColumnCount = grid.Columns.Count;
                int selectedItemCount = grid.SelectedItems.Count;
                StringBuilder dataitems = new StringBuilder();
                dataitems.Append(string.Format("{0}#", RowCount));
                dataitems.Append(string.Format("{0}#", ColumnCount));
                dataitems.Append(string.Format("{0}#", grid.SelectionMode.ToString()));
                dataitems.Append(string.Format("{0}#", grid.SelectionUnit.ToString()));
                dataitems.Append(string.Format("{0}#", grid.SelectedIndex));
                dataitems.Append(string.Format("{0}#", selectedItemCount));
                return dataitems.ToString();
            }
            return null;
        }

        # endregion
    }

    public class GridFilterRowCellAutomationPeer : FrameworkElementAutomationPeer
    {
        public GridFilterRowCellAutomationPeer(GridCell gridCell)
            : base(gridCell)
        {

        }

        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Filter Cell";
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override bool IsEnabledCore()
        {
            return true;
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }

        protected override string GetItemStatusCore()
        {
            return base.GetItemStatusCore();
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

    }

    public class GridIndentCellAutomationPeer : FrameworkElementAutomationPeer
    {
        # region Contructor
        public GridIndentCellAutomationPeer(GridCell gridCell)
            : base(gridCell)
        {

        }
        #endregion

        #region Overrides
        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Empty Cell";
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override bool IsEnabledCore()
        {
            return true;
        }
        protected override string GetItemStatusCore()
        {
            return base.GetItemStatusCore();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }
        #endregion

    }

    public class GridRowHeaderCellAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Contructor
        public GridRowHeaderCellAutomationPeer(GridCell gridCell)
                : base(gridCell)
        {

        }
        #endregion

        #region Overrides
        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Row Header Cell";
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override bool IsEnabledCore()
        {
            return true;
        }
        protected override string GetItemStatusCore()
        {
            if (!AutomationPeerHelper.EnableCodedUI)
                return base.GetItemStatusCore();

            StringBuilder gridCellItems = new StringBuilder();

            if ((this.Owner is GridRowHeaderCell))
            {
                string RowErrorMessage = (this.Owner as GridRowHeaderCell).RowErrorMessage;
                gridCellItems.Append(string.Format("{0}#", (this.Owner as GridRowHeaderCell).RowIndex));
                gridCellItems.Append(string.Format("{0}#", (this.Owner as GridRowHeaderCell).State));
                gridCellItems.Append(string.Format("{0}#", RowErrorMessage));
                return gridCellItems.ToString();
            }

            return base.GetItemStatusCore();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }
        #endregion

    }
    public class GridSummaryCellAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Constructor
        public GridSummaryCellAutomationPeer(GridCell gridCell)
            : base(gridCell)
        {

        }
        #endregion
        #region Overrides
        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            var cellValue = ((System.Windows.Controls.ContentControl)(this.Owner)).Content;
            if (cellValue is string)
                return cellValue.ToString();
            return this.Owner.GetType().Name;
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }

        protected override bool IsEnabledCore()
        {
            return true;
        }

        #endregion
    }

    public class GridUnboundRowCellAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Constructor
        public GridUnboundRowCellAutomationPeer(GridCell gridCell)
            : base(gridCell)
        {

        }
        #endregion
        #region Overrides
        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Unbound Row cell";
        }

        protected override string GetClassNameCore()
        {
            return this.Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return base.GetChildrenCore();
        }

        protected override bool IsEnabledCore()
        {
            return true;
        }
        protected override string GetItemStatusCore()
        {
            return base.GetItemStatusCore();
        }
        #endregion
    }

    /// <summary>
    /// To detect whether the OnCreateAutomationPeer create for the System screen reader else any client bases accessibility tool.
    /// </summary>
    internal class UnsafeNativeMethods
    {
        internal const uint GetScreenReader = 0x0046;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);
    }

    internal static class ScreenReader
    {
        internal static bool IsRunning
        {
            get
            {
                bool returnValue = false;
                if (!UnsafeNativeMethods.SystemParametersInfo(UnsafeNativeMethods.GetScreenReader, 0, ref returnValue, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "error calling SystemParametersInfo");
                }
                return returnValue;
            }
        }
    }

}
