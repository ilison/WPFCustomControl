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
    public class SfMultiColumnDropDownControlAutomationPeer : FrameworkElementAutomationPeer
    {
        # region Constructor
        public SfMultiColumnDropDownControlAutomationPeer(SfMultiColumnDropDownControl control)
            : base(control)
        {
        }
        # endregion

        #region Overrides

        protected override string GetNameCore()
        {
            if (AutomationPeerHelper.EnableCodedUI)
                return this.Owner.GetType().Name;
            return "Dropdown Editor";
        }

        protected override string GetClassNameCore()
        {
            return "SfMultiColumnDropDownControl";
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

            if(!(this.Owner is SfMultiColumnDropDownControl))
                return null;

            var SfMultiColumn = this.Owner as SfMultiColumnDropDownControl;
            StringBuilder dataitem = new StringBuilder();
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowAutoComplete));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowNullInput));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowImmediatePopup));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowIncrementalFiltering));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowCaseSensitiveFiltering));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.AllowSpinOnMouseWheel));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.DisplayMember));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.IsDropDownOpen));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.SelectedIndex));
            dataitem.Append(string.Format("{0}#", SfMultiColumn.ValueMember));
            return dataitem.ToString();
        }

        #endregion
    }
  
}
