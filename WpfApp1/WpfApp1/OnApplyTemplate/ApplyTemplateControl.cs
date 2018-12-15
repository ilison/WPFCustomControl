using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp1
{
    public class ApplyTemplateControl : Control
    {
        private DispatcherTimer timer;

        #region Ctor
        static ApplyTemplateControl()
            {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplyTemplateControl)
                    , new FrameworkPropertyMetadata(typeof(ApplyTemplateControl)));
            }
        #endregion

        #region Override

        //获取Template中控件的代码，需要重载OnApplyTemplate（）方法，可以使用GetTemplateChild方法获取
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate(); 
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            UpdateDateTime();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000 - DateTime.Now.Millisecond);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();

            timer.Interval = TimeSpan.FromMilliseconds(1000 - DateTime.Now.Millisecond);
            timer.Start();
        }
    private void UpdateDateTime()
        {
            this.DateTime = System.DateTime.Now;
        }
    #endregion

    #region Dependency Property

    public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("DateTime", typeof(DateTime), typeof(ApplyTemplateControl), 
                new PropertyMetadata(DateTime.Now,new PropertyChangedCallback(OnDateTimeChanged)));

        public static void OnDateTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ApplyTemplateControl apply = (ApplyTemplateControl)sender;

            DateTime oldValue = (DateTime)args.OldValue;
            DateTime newValue = (DateTime)args.NewValue;

            //apply.OnDateTimeChanged(oldValue, newValue);
        }

        #endregion

        #region Routed Event
        public static readonly RoutedEvent DateTimeChangedEvent = EventManager.RegisterRoutedEvent(
            "DateTimeChanged", RoutingStrategy.Bubble
            , typeof(RoutedPropertyChangedEventHandler<DateTime>)
            , typeof(ApplyTemplateControl));

        protected virtual void OnDateTimeChanged(DateTime oldValue, DateTime newValue)
        {
            RoutedPropertyChangedEventArgs<DateTime> args = new RoutedPropertyChangedEventArgs<DateTime>
                (oldValue, newValue);
            args.RoutedEvent = ApplyTemplateControl.DateTimeChangedEvent;
            RaiseEvent(args);
        }
        #endregion
    }
}
