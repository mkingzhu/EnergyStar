using EnergyStar.Interop;
using Microsoft.Win32;

namespace EnergyStar
{
    internal class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();

        static async void HouseKeepingThreadProc()
        {
            Console.WriteLine("House keeping thread started.");
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var houseKeepingTimer = new PeriodicTimer(TimeSpan.FromMinutes(5));
                    await houseKeepingTimer.WaitForNextTickAsync(cts.Token);
                    EnergyManager.ThrottleAllUserBackgroundProcesses();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        static void SystemEventsPowerModeChanged()
        {
            EnergyManager.IsAcConnected = BatteryManager.IsAcConnected;
            EnergyManager.ThrottleAllUserBackgroundProcesses();
        }

        static void SetUpUI(BatteryManager batteryManager)
        {
            // Create context menu
            var contextMenu = new ContextMenuStrip();

            // Add exit item
            var exitItem = contextMenu.Items.Add("Exit");
            exitItem.Click += new EventHandler((sender, e) => { Environment.Exit(0); });

            // Create system tray icon
            NotifyIcon icon = new NotifyIcon();
            icon.ContextMenuStrip = contextMenu;
            icon.Icon = new Icon("./icon.ico");
            icon.Visible = true;
            icon.MouseMove += new MouseEventHandler((sender, e) => { icon.Text = batteryManager.Status; });
        }

        static void Main(string[] args)
        {
            // Well, this program only works for Windows Version starting with Cobalt...
            // Nickel or higher will be better, but at least it works in Cobalt
            //
            // In .NET 5.0 and later, System.Environment.OSVersion always returns the actual OS version.
            if (Environment.OSVersion.Version.Build < 22000)
            {
                Console.WriteLine("E: You are too poor to use this program.");
                Console.WriteLine("E: Please upgrade to Windows 11 22H2 for best result, and consider ThinkPad Z13 as your next laptop.");
                // ERROR_CALL_NOT_IMPLEMENTED
                Environment.Exit(120);
            }

            BatteryManager batteryManager = new BatteryManager();

            SetUpUI(batteryManager);

            // Call SystemEventsPowerModeChanged() to init the power adapter status.
            SystemEventsPowerModeChanged();

            HookManager.SubscribeToWindowEvents();
            EnergyManager.ThrottleAllUserBackgroundProcesses();

            var houseKeepingThread = new Thread(new ThreadStart(HouseKeepingThreadProc));
            houseKeepingThread.Start();

            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler((object sender, PowerModeChangedEventArgs e) => SystemEventsPowerModeChanged());
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler((object sender, PowerModeChangedEventArgs e) => batteryManager.PowerModeChangedEventHandler());

            while (true)
            {
                if (Event.GetMessage(out Win32WindowForegroundMessage msg, IntPtr.Zero, 0, 0))
                {
                    if (msg.Message == Event.WM_QUIT)
                    {
                        cts.Cancel();
                        break;
                    }

                    Event.TranslateMessage(ref msg);
                    Event.DispatchMessage(ref msg);
                }
            }

            cts.Cancel();
            HookManager.UnsubscribeWindowEvents();
        }
    }
}
