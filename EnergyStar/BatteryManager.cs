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
                int batteryLevelChanged = batteryLevelWhenPowerModeChanged - currentBatteryLevel;

                if (currentBatteryLevel != 100)
                {
                    sb.Append($": {currentBatteryLevel}%");
                }

                // Append the status only when the current battery level is not full and there is a change in number.
                if (batteryLevelChanged != 0 && currentBatteryLevel != 100)
                {
                    TimeSpan duration = DateTime.UtcNow - timeWhenPowerModeChanged;
                    double pace = duration.TotalMinutes / Math.Abs(batteryLevelChanged);
                    sb.Append(string.Format("\nDuration: {0}, {1} pace: {2}.", duration.ToString(@"hh\:mm"), batteryLevelChanged < 0 ? "charging" : "draining", string.Format("{0:0.00}", pace)));
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
