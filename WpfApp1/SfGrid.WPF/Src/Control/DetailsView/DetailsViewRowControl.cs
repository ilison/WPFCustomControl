#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
#if !WinRT && !UNIVERSAL
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#else
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the content control which holds DetailsViewDataGrid
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class DetailsViewRowControl : VirtualizingCellsControl
    {
        #region Ctor
        public DetailsViewRowControl()
        {
            this.DefaultStyleKey = typeof(DetailsViewRowControl);
        }
        #endregion

        protected override void SetContent()
        {
            this.Content = this.ItemsPanel = new DetailsViewRowPanel();
        }

        internal void InitializeDetailsViewRowControl(Func<DataRowBase> getDataRow)
        {
            var detailsViewRowPanel = this.ItemsPanel as DetailsViewRowPanel;
            if (detailsViewRowPanel == null) return;
            detailsViewRowPanel.GetDataRow = getDataRow;
        }
    }
    
    public class DetailsViewRowPanel : Panel, IDisposable
    {
        internal Func<DataRowBase> GetDataRow;              
        private bool isdisposed = false;

        public DetailsViewRowPanel()
        {
            
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var dataRow = (DetailsViewDataRow)this.GetDataRow();
            if (dataRow == null)
                return base.MeasureOverride(availableSize);

            MeasureCells();
            return availableSize;
        }

        private void MeasureCells()
        {
            var dataRow = (DetailsViewDataRow)this.GetDataRow();
            if (dataRow == null)
                return;           

            var visibleCells = dataRow.GetVisibleColumns();
            if (visibleCells == null)
                return;

            foreach (var column in visibleCells)
            {
                if (column.ColumnElement.Visibility != Visibility.Visible || column.ColumnIndex < 0) continue;
                if (!this.Children.Contains(column.ColumnElement))
                    this.Children.Add(column.ColumnElement);
                var rect = dataRow.GetCellPosition(column.ColumnIndex);
                if (rect.IsEmpty) continue;
                var cell = column.ColumnElement;
                //WPF-32265 Need to set padding before measure the control to get the correct availablewidth.
                if (cell is IDetailsViewInfo)
                {
                    var padding = (Thickness)dataRow.GetDetailsViewInfo("Padding");
                    var control = cell as Control;
                    if (control != null && control.Padding != padding)
                        control.Padding = padding;
                }

                cell.Measure(new Size(rect.Width, rect.Height));
                if (cell is IDetailsViewInfo)
                {
                    var scrollInfo = cell as IDetailsViewInfo;
                    var clipRect = (Rect)dataRow.GetDetailsViewInfo("Clip");
                    scrollInfo.SetClipRect(clipRect);

                    var offset = (double)dataRow.GetDetailsViewInfo("HorizontalOffset");
                    scrollInfo.SetHorizontalOffset(offset);

                    offset = (double)dataRow.GetDetailsViewInfo("VerticalOffset");
                    scrollInfo.SetVerticalOffset(offset);

                    //var padding = (Thickness)dataRow.GetDetailsViewInfo("Padding");
                    //var control = cell as Control;
                    //if (control != null) control.Padding = padding;
                }
            }
        }

        private void ArrangeCells()
        {
            var dataRow = (DetailsViewDataRow)this.GetDataRow();
            if (dataRow == null)
                return;
            var visibleColumns = dataRow.GetVisibleColumns();
            if (visibleColumns == null)
                return;
            foreach (var column in visibleColumns)
            {
                if (column.ColumnElement.Visibility != Visibility.Visible) continue;
                var rect = dataRow.GetCellPosition(column.ColumnIndex);
                if (rect.IsEmpty) continue;
                column.ColumnElement.Arrange(rect);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var dataRow = (DetailsViewDataRow)this.GetDataRow();
            if (dataRow == null)
                return finalSize;

            ArrangeCells();
            return finalSize;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewRowPanel"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewRowPanel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)            
                this.GetDataRow = null;                             
            isdisposed = true;
        }
    }

    public class DetailsViewContentPresenter : ContentControl, IDetailsViewInfo
    {
        public DetailsViewContentPresenter()
        {
            this.DefaultStyleKey = typeof (DetailsViewContentPresenter);
        }

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            if (!string.IsNullOrEmpty(_currentVisualState))
                ApplyVisualState(_currentVisualState);
        }

        private string _currentVisualState;
        internal string CurrentVisualState;
        internal void ApplyVisualState(string visualState)
        {
            _currentVisualState = VisualStateManager.GoToState(this, visualState, true) ? string.Empty : visualState;
            CurrentVisualState = visualState;
        }


        public void SetClipRect(Rect rect)
        {
            if (this.Content is IDetailsViewInfo)
                (this.Content as IDetailsViewInfo).SetClipRect(rect);
        }

        public void SetHorizontalOffset(double offset)
        {
            if(this.Content is IDetailsViewInfo)
                (this.Content as IDetailsViewInfo).SetHorizontalOffset(offset);
        }

        public void SetVerticalOffset(double offset)
        {
            if (this.Content is IDetailsViewInfo)
                (this.Content as IDetailsViewInfo).SetVerticalOffset(offset);
        }

        public double GetExtendedWidth()
        {
            if (this.Content is IDetailsViewInfo)
                return (this.Content as IDetailsViewInfo).GetExtendedWidth();
            return double.NaN;
        }
    }

    public class GridDetailsViewIndentCell : ContentControl
    {
        public GridDetailsViewIndentCell()
        {
            this.DefaultStyleKey = typeof (GridDetailsViewIndentCell);
        }

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            if (!string.IsNullOrEmpty(_currentVisualState))
                ApplyVisualState(_currentVisualState);
        }

        private string _currentVisualState;
        internal void ApplyVisualState(string visualState)
        {
            _currentVisualState = VisualStateManager.GoToState(this, visualState, true) ? string.Empty : visualState;
        }
    }
}
