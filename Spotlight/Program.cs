using Invoker;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Spotlight
{
    class Program
    {
        public static void Main(string[] args)
        {
            HotKeyManager.RegisterHotKey(Keys.Space, KeyModifiers.Control | KeyModifiers.Alt);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyPressed);
            App app = new App();
            app.InitializeComponent();
            app.Run();
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
