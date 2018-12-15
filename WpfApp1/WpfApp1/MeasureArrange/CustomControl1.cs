using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public class CustomControl1 : Panel
    {
        /* 使用代码 
        <local:CustomControl1  Width="300" Background="Gray" HorizontalAlignment="Left" >
            <Rectangle Width="100" Height="50" Fill="Red" Margin="10,10,0,0" />
            <Rectangle Width="100" Height="50" Fill="Yellow" Margin="10,10,0,0" />
            <Rectangle Width="100" Height="50" Fill="Green" Margin="10,10,0,0" />
        </local:CustomControl1> 
        */

        /// <summary>
        /// 先测量子控件需要多大尺寸，做个申报准备
        /// </summary>
        /// <param name="constraint">限定的尺寸，比如，规定了width和height</param>
        /// /// <param name="constraint">这个尺寸是This自己的尺寸大小，根据上文的Width手动给定了，所以固定，height没有
        ///     指定，所以默认会用父窗体的高。 当 width和height变化时，会回调此方法
        /// </param>
        /// 此方法中会调用 ArrangeOverride 方法
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            //定义预期的宽度和高度
            double height = 0, width = 0;
            UIElement element;
            //遍历每个元素，计算所需的总尺寸
            for (int i = 0; i < Children.Count; i++)
            {
                element = Children[i];
                //按照限定的尺寸测量一下自己，拿镜子找着自己
                element.Measure(constraint);
                if (height < element.DesiredSize.Height)
                    height = element.DesiredSize.Height;
                width += element.DesiredSize.Width;
            }
            //申报，我需要这个尺寸
            //这里返回的是所有子控件总的Size。子控件一般为自己手动添加的，尺寸一般也是手动加入的。
            return new Size(width, height);
        }

        /// <summary>
        /// 排列每个元素
        /// </summary>
        /// <param name="arrangeBounds">测量的尺寸</param>
        ///  <param name="arrangeBounds">这个尺寸是This自己的尺寸大小</param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double currentX = 100;
            UIElement element;
            for (int i = 0; i < Children.Count; i++)
            {
                element = Children[i];
                //排列每个元素
                //参数 Rect(Margion-Left,Margion-Top,Width,Height)
                Children[i].Arrange(new Rect(currentX, 0, element.DesiredSize.Width, element.DesiredSize.Height));
                currentX += element.DesiredSize.Width;
            }
            return arrangeBounds;
        }
    }
}
