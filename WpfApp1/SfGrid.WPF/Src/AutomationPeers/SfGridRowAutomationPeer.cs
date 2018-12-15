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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;

namespace Syncfusion.UI.Xaml.Grid
{
    public class SfGridRowAutomationPeer : FrameworkElementAutomationPeer
    {
        public SfGridRowAutomationPeer(VirtualizingCellsControl virtualizingCellsControl)
            : base(virtualizingCellsControl)
        {

        }

        #region Overrides
        protected override string GetNameCore()
        {
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

        //WPF-30436 Need to override it and return true to find the disabled Control in coded UI
        protected override bool IsEnabledCore()
        {
            return true;
        }
       
        protected override string GetItemStatusCore()
        {
            if(!AutomationPeerHelper.EnableCodedUI)
            {
                return base.GetItemStatusCore();
            }
            if (this.Owner is VirtualizingCellsControl)
            {
                var virtualizingCellsControl = this.Owner as VirtualizingCellsControl;
                StringBuilder gridrowItems = new StringBuilder();
                if (!(virtualizingCellsControl.Content is OrientedCellsPanel))
                    return null;
                if ((virtualizingCellsControl.Content as OrientedCellsPanel).GetDataRow == null)
                    return null;

                var datarow = (virtualizingCellsControl.Content as OrientedCellsPanel).GetDataRow();
                if (datarow == null)
                    return null;
                gridrowItems.Append(string.Format("{0}#", datarow.RowIndex));
                gridrowItems.Append(string.Format("{0}#", datarow.RowType));
                gridrowItems.Append(string.Format("{0}#", datarow.IsSelectedRow));
                return gridrowItems.ToString();
            }
            return null;
        }

        # endregion
    }
}
