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
using System.Windows.Automation.Peers;

namespace Syncfusion.UI.Xaml.Grid
{
    public class GroupDropAreaItemAutomationPeer : FrameworkElementAutomationPeer
    {
        # region Constructor
        public GroupDropAreaItemAutomationPeer(GroupDropAreaItem groupDropAreaItem)
            : base(groupDropAreaItem)
        {
        }
        # endregion

        # region Overrides

        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return (this.Owner as GroupDropAreaItem).GroupName;
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

            var groupItems = this.Owner as GroupDropAreaItem;
            StringBuilder dataItems = new StringBuilder();
            dataItems.Append(string.Format("{0}#", groupItems.GroupName));
            dataItems.Append(string.Format("{0}#",groupItems.SortDirection.ToString()));
            return dataItems.ToString();
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return null;
        }

        # endregion
    }
}
