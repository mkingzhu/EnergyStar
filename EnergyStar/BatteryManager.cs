namespace EnergyStar
{
    using System.Text;

    internal class BatteryManager
    {
        private bool isAcConnected;
        private DateTime timeWhenPowerModeChanged;
        private int batteryLevelWhenPowerModeChanged;

        public string Status
        {
            get
            {
                StringBuilder sb = new StringBuilder(isAcConnected ? "AC Mode" : "Battery Mode");

                int currentBatteryLevel = BatteryLifePercent;
                int batteryLevelChanged = Math.Abs(batteryLevelWhenPowerModeChanged - currentBatteryLevel);

                // Append the status only when the current battery level is less than 96% and there is a change in number.
                if (batteryLevelChanged > 0 && currentBatteryLevel < 96)
                {
                    TimeSpan duration = DateTime.UtcNow - timeWhenPowerModeChanged;
                    double pace = duration.TotalMinutes / batteryLevelChanged;
                    sb.Append(string.Format("\nDuration: {0}, {1} pace: {2}.", duration.ToString(@"hh\:mm"), isAcConnected ? "charging" : "draining", string.Format("{0:0.00}", pace)));
                }

                return sb.ToString();
            }
        }

        internal BatteryManager()
        {
            isAcConnected = IsAcConnected;
            timeWhenPowerModeChanged = DateTime.UtcNow;
            batteryLevelWhenPowerModeChanged = BatteryLifePercent;
        }

        internal void PowerModeChangedEventHandler()
        {
            // AC disconnected
            if (isAcConnected && !IsAcConnected)
            {
                isAcConnected = false;
                timeWhenPowerModeChanged = DateTime.UtcNow;
                batteryLevelWhenPowerModeChanged = BatteryLifePercent;
            }

            // AC connected
            if (!isAcConnected && IsAcConnected)
            {
                isAcConnected = true;
                timeWhenPowerModeChanged = DateTime.UtcNow;
                batteryLevelWhenPowerModeChanged = BatteryLifePercent;
            }
        }

        internal static PowerLineStatus PowerLineStatus
        {
            get
            {
                return SystemInformation.PowerStatus.PowerLineStatus;
            }
        }

        internal static bool IsAcConnected
        {
            get
            {
                return PowerLineStatus == PowerLineStatus.Online;
            }
        }

        internal static int BatteryLifePercent
        {
            get
            {
                return (int)(SystemInformation.PowerStatus.BatteryLifePercent * 100);
            }
        }
    }
}
