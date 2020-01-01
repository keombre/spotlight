using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spotlight.Windows
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    public partial class Search : Window
    {

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private readonly Parser parser = new Parser();
        private bool IsClosing = false;

        public Search()
        {
            InitializeComponent();
            Left = (SystemParameters.PrimaryScreenWidth / 2) - (Width / 2);
            Top = (SystemParameters.PrimaryScreenWidth / 7);
            Topmost = true;
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            List<string> explorers = parser.GetOpenExplorers();
            if (explorers.Count > 0)
                Path.Content = explorers[0];
            else
                Path.Content = Environment.SystemDirectory;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!query.IsEnabled)
                return;
            query.Foreground = Brushes.Black;

            if (e.Key == Key.Enter)
            {
                bool asAdmin = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    RunInConsole(asAdmin);
                else
                    Validate(asAdmin);
            }
            else if (e.Key == Key.Escape)
                Close();
        }

        private void Validate(bool asAdmin)
        {
            HidePopupConsole();
            query.IsEnabled = false;
            Command? cmd = parser.Parse(query.Text, asAdmin);

            if (cmd != null && parser.Invoke(cmd.Value))
                Close();
            else
            {
                query.IsEnabled = true;
                query.Select(0, query.Text.Length);
                query.Foreground = Brushes.Red;
                query.Focus();
            }
        }

        private void PopupConsole()
        {
            Height = 390;
            ConsoleOut.Visibility = Visibility.Visible;
            ConsoleOut.Text = "";
        }

        private void HidePopupConsole()
        {
            Height = 60;
            ConsoleOut.Visibility = Visibility.Collapsed;
        }

        private void RunInConsole(bool asAdmin)
        {

            query.IsEnabled = false;
            Command? cmd = parser.Parse(query.Text, asAdmin);

            string output = "";

            if (cmd != null && parser.InvokeLocal(cmd.Value, ref output))
            {
                PopupConsole();
                query.IsEnabled = true;
                ConsoleOut.Text = output;
                query.Text = "";
                query.Focus();
            } 
            else
            {
                HidePopupConsole();
                query.IsEnabled = true;
                query.Select(0, query.Text.Length);
                query.Foreground = Brushes.Red;
                query.Focus();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (query.Text == "" && !IsClosing)
                Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosing = true;
        }
    }
}
