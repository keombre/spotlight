using Invoker;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Spotlight
{
    class Program
    {
        static Mutex mutex = new Mutex(true, "{d26b4955-4c78-4b8d-8fc0-947a2dd8703a}");

        [STAThread]
        public static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Control | KeyModifiers.Alt);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
                ShowSystemTray();
                app = new App();
                app.InitializeComponent();
                AppDispatcher = app.Dispatcher;
                app.Run();
            }
            else
                MessageBox.Show("Spotlight is already running");
            Destroy();
        }

        internal static void InvokeShutdown()
        {
            AppDispatcher.Invoke(() => app.Shutdown());
            Destroy();
        }

        private static void Destroy()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            WindowThread?.Abort();
            PopupThread?.Abort();
        }

        private static volatile Dispatcher AppDispatcher;

        private static volatile App app;
        private static Thread WindowThread;
        private static Thread PopupThread;
        private static volatile NotifyIcon notifyIcon;


        static void ShowSystemTray()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Click += new EventHandler(PopupSystemTray);
            notifyIcon.Icon = Properties.Resources.console_icon;
            notifyIcon.Visible = true;
        }

        private static void PopupSystemTray(object sender, EventArgs e)
        {
            CreateThreadedWindow<Windows.SystemTray>(ref PopupThread);
        }

        static void HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            CreateThreadedWindow<Windows.Search>(ref WindowThread);
        }

        static void CreateThreadedWindow<T>(ref Thread thread) where T : System.Windows.Window, new()
        {
            if (thread == null || !thread.IsAlive)
            {
                thread = new Thread(new ThreadStart(() =>
                {
                    T main = new T();
                    main.Show();
                    main.Activate();
                    main.Closed += (sender2, e2) => main.Dispatcher.InvokeShutdown();
                    Dispatcher.Run();
                }));

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
        }
    }
}
