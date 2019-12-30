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

namespace Spotlight.Windows
{
    /// <summary>
    /// Interakční logika pro SystemTray.xaml
    /// </summary>
    public partial class SystemTray : Window
    {
        public SystemTray()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Program.InvokeShutdown();
        }
    }
}
