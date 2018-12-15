#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Controls.DataPager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Syncfusion.UI.Xaml.Grid
{
    public class SfDataPagerAutomationPeer : FrameworkElementAutomationPeer
    {
        # region Constructor
        public SfDataPagerAutomationPeer(SfDataPager DataPager)
            : base(DataPager)
        {
        }
        # endregion

        # region Overrides

        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Pager";
        }

        protected override string GetClassNameCore()
        {
            return "SfDataPager";
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

                var pager = this.Owner as SfDataPager;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("{0}#", pager.AccentBackground.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.AccentForeground.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.AutoEllipsisMode.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.AutoEllipsisText.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.DisplayMode.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.EnableGridPaging.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.NumericButtonCount.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.Orientation.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.PageCount.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.PageSize.ToString()));
                stringBuilder.Append(string.Format("{0}#", pager.UseOnDemandPaging.ToString()));
                return stringBuilder.ToString();
        }

        # endregion
    }
}
