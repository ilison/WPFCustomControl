#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.ComponentModel;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public interface IElement : INotifyPropertyChanged, IComparable
    {
        FrameworkElement Element { get; }
        int Index { get; }
    }

    [ClassReference(IsReviewed = false)]
    public interface IRowElement : IElement
    {
        RowRegion RowRegion { get; }
		int Level { get; }
        RowType RowType { get; }
        bool IsFixedRow { get; }
        Rect ArrangeRect { get; set; }
        void MeasureElement(Size size);
        void ArrangeElement(Rect rect);
        bool IsSpannedRow { get; }
        bool IsCurrentRow { get; set; }
    }

    [ClassReference(IsReviewed = false)]
    public interface IColumnElement : IElement
    {
        IGridCellRenderer Renderer { get; }
        int RowIndex { get; }
        bool IsEditing { get; }
		int RowSpan { get; }
        int ColumnSpan { get; }    
        bool IsSpannedColumn { get; }
		void UpdateCellStyle(object dataContext);    
    }
}
