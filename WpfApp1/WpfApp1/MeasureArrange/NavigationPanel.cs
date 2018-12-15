using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public class NavigationPanel:Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size();
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                var child = this.InternalChildren[i];
                child.Measure(availableSize);
                size = child.DesiredSize;
            }
            return base.MeasureOverride(size);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var thisWidth = finalSize.Width;
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                var child = this.InternalChildren[i];
                var poingX = thisWidth - child.DesiredSize.Width;
                if (poingX > 0)
                {
                    child.Arrange(new Rect(new Point(poingX, 0), child.DesiredSize));
                }
                else
                {
                    child.Arrange(new Rect(new Point(0, 0), child.DesiredSize));
                }
            }
            return base.ArrangeOverride(finalSize);
        }
    }
}
