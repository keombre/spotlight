using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

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

        private bool PreventClose = false;

        private void Button_Settings(object sender, RoutedEventArgs e)
        {
            PreventClose = true;
            Process.Start(Path.Combine(Directory.GetCurrentDirectory(), @"config.xml"));
            Close();
        }

        private void Button_Close(object sender, RoutedEventArgs e)
        {
            PreventClose = true;
            MessageBoxResult res = MessageBox.Show("Opravdu chcete ukončit Spotlight?", "Ukončit", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
                Program.InvokeShutdown();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (PreventClose)
                Hide();
            else
                Close();
        }
    }
}
