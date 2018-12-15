#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Collections.Generic;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid
{
    public interface ITreeGridRowGenerator
    {
        SfTreeGrid Owner { get;  set; }
        IList<ITreeDataRowElement> Items { get; }
      
        void PregenerateRows(VisibleLinesCollection visibleRows, VisibleLinesCollection visibleColumns);
        void EnsureRows(VisibleLinesCollection visibleRows);
        void EnsureColumns(VisibleLinesCollection visibleColumns);

        void ApplyColumnSizerOnInitial(double availableWidth);
        void RowsArranged();
        void ColumnHiddenChanged(HiddenRangeChangedEventArgs args);
        void RowHiddenChanged(HiddenRangeChangedEventArgs args);
        void ColumnInserted(int index,int count);
        void ColumnRemoved(int index, int count);       
    }
}
