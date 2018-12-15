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

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
    /// <summary>
    /// Provides the required methods that need to connect from GridFilterRowCell and Renderers.
    /// </summary>
    public interface IGridFilterRowRenderer
    {
        /// <summary>
        /// Process the filtering when the FilterRowCondition is changed in corresponding column.
        /// </summary>
        /// <param name="filterRowCondition">The new FilterRowCondition that have been changed.</param>
        void OnFilterRowConditionChanged(string filterRowCondition);

        /// <summary>
        /// Invoked when filter cleared programmatically to update the cell            
        /// </summary>
        /// <param name="dataColumn">DataColumn of FilterRowCell.</param>
        void ClearFilter(DataColumnBase dataColumn);       
    }
}
