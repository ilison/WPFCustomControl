using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// NavigationPanelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationPanelWindow : Window
    {
        public NavigationPanelWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_navigationpanel_.Width == 300) _navigationpanel_.Width = 60; else _navigationpanel_.Width = 300;
            
        }
    }
}
