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
    public class GridStackedHeaderCellControlAutomationPeer : FrameworkElementAutomationPeer
    {
        #region constructor
        public GridStackedHeaderCellControlAutomationPeer(GridStackedHeaderCellControl control)
            : base(control)
        {
        }
        #endregion

        #region overrides

        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            var stackedHeaderControl = this.Owner as GridStackedHeaderCellControl;
            if (stackedHeaderControl.Content is string)
                stackedHeaderControl.Content.ToString();
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
            if (!AutomationPeerHelper.EnableCodedUI)
                return base.GetItemStatusCore();

            var stackedHeaderControl = this.Owner as GridStackedHeaderCellControl;
            var dataItems = new StringBuilder();
            if (stackedHeaderControl.Content != null)
            {
                dataItems.Append(string.Format("{0}", stackedHeaderControl.Content.ToString()));
            }
            return dataItems.ToString();
        }

        #endregion
    }
}
