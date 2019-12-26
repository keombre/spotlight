using Invoker;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Spotlight
{
    class Program
    {
        static Mutex mutex = new Mutex(true, "{d26b4955-4c78-4b8d-8fc0-947a2dd8703a}");
        public static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Control | KeyModifiers.Alt);
                HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            else
                MessageBox.Show("Spotlight is already running");
        }

        private static volatile Thread WindowThread;

        static void HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (WindowThread == null || !WindowThread.IsAlive)
            {
                WindowThread = new Thread(new ThreadStart(() =>
                {
                    Windows.Search main = new Windows.Search();
                    main.Show();
                    main.Closed += (sender2, e2) => main.Dispatcher.InvokeShutdown();
                    System.Windows.Threading.Dispatcher.Run();
                }));

                WindowThread.SetApartmentState(ApartmentState.STA);
                WindowThread.IsBackground = true;
                WindowThread.Start();
            }
        }
    }
}
