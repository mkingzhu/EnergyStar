namespace EnergyStar
{
    using EnergyStar.Interop;
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

                Win32Api.SYSTEM_POWER_STATUS status = Win32Api.GetSystemPowerStatus();
                int currentBatteryLevel = status.BatteryLifePercent;
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
            Win32Api.SYSTEM_POWER_STATUS status = Win32Api.GetSystemPowerStatus();

            isAcConnected = status.ACLineStatus == Win32Api.SYSTEM_POWER_STATUS.AC_LINE_STATUS_ONLINE;
            timeWhenPowerModeChanged = DateTime.UtcNow;
            batteryLevelWhenPowerModeChanged = status.BatteryLifePercent;
        }

        internal void PowerModeChangedEventHandler()
        {
            Win32Api.SYSTEM_POWER_STATUS status = Win32Api.GetSystemPowerStatus();

            // AC disconnected
            if (isAcConnected && status.ACLineStatus == Win32Api.SYSTEM_POWER_STATUS.AC_LINE_STATUS_OFFLINE)
            {
                isAcConnected = false;
                timeWhenPowerModeChanged = DateTime.UtcNow;
                batteryLevelWhenPowerModeChanged = status.BatteryLifePercent;
            }

            // AC connected
            if (!isAcConnected && status.ACLineStatus == Win32Api.SYSTEM_POWER_STATUS.AC_LINE_STATUS_ONLINE)
            {
                isAcConnected = true;
                timeWhenPowerModeChanged = DateTime.UtcNow;
                batteryLevelWhenPowerModeChanged = status.BatteryLifePercent;
            }
        }
    }
}
