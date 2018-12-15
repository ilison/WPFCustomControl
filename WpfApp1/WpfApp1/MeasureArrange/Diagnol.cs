using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    /// <summary>
    /// 只能在奇数个子控件中有效
    /// </summary>
    public class Diagnol:Panel
    {
        /// <summary>
        /// 测量
        /// </summary>
        /// <param name="availableSize">This的尺寸</param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = new Size();
            foreach (UIElement item in this.InternalChildren)
            {
                item.Measure(availableSize);
                size.Width += item.DesiredSize.Width;
                size.Height += item.DesiredSize.Height;
            }
            //返回所有子控件需要的 总尺寸
            return base.MeasureOverride(size);
        }

        /// <summary>
        /// 排列每个子控件
        /// </summary>
        /// <param name="finalSize">This的尺寸</param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Point point1 = new Point();
            //子控件个数 
            int count = this.InternalChildren.Count;
            //总个数的中间值
            int index = (count + 1) / 2-1;
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                var child = this.InternalChildren[i];
                //如果就一个没法玩，直接排列
                if (count <= 1)
                {
                    //设置子控件的位置
                    child.Arrange(new Rect(point1, child.DesiredSize));
                    break;
                }

                //玩起来
                if (i < index)
                {
                    child.Arrange(new Rect(point1, child.DesiredSize));
                    point1.X += child.DesiredSize.Width;
                    point1.Y += child.DesiredSize.Height;
                }
                else
                {
                    child.Arrange(new Rect(point1, child.DesiredSize));
                    point1.X += child.DesiredSize.Width;
                    point1.Y -= child.DesiredSize.Height;
                }
            }
            return base.ArrangeOverride(finalSize);
        }
    }
}
