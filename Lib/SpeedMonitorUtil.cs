namespace SpeedMonitorUtil
{
    public class SpeedMonitor : IDisposable
    {
        private (DateTime, double) m_Previous, m_Now;
        public double MicrosecondsSpeed { get { return m_Val.Item1 / m_Val.Item2.Microseconds; } }
        public double MillisecondsSpeed { get { return m_Val.Item1 / m_Val.Item2.Milliseconds; } }
        public double SecondsSpeed { get { return m_Val.Item1 / m_Val.Item2.Seconds; } }
        public double Total { get; set; }
        private double m_accuracy = 100;
        private (double, TimeSpan) m_Val = (0,new(1000000));
        private readonly System.Timers.Timer? zTimer;
        private Label? Label;
        private ProgressBar? ProgressBar;
        public SpeedMonitor(double accuracy = 250)
        {
            m_accuracy = accuracy;
            zTimer = new System.Timers.Timer(m_accuracy);
            zTimer.Elapsed += zTimer_Elapsed;
            zTimer.Enabled = true;
        }
        public void Set(Label label, ProgressBar b)
        {
            Label = label;
            ProgressBar = b;
        }
        public void Dispose()
        {
            zTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
        public static string CountSize(ulong Size)
        {
            string m_strSize = "";
            ulong FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                m_strSize = FactSize.ToString("F0") + " B";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                m_strSize = (FactSize / 1024.00).ToString("F2") + " KB";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F3") + " MB";
            else if (FactSize >= 1073741824)
                m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F5") + " GB";
            return m_strSize;
        }
        private void zTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                zTimer!.Enabled = false;
                m_Previous = new(DateTime.Now, Total);
                Thread.Sleep((int)m_accuracy);
                m_Now = new(DateTime.Now, Total);
                m_Val = (m_Now.Item2 - m_Previous.Item2, m_Now.Item1 - m_Previous.Item1);
                double percent = (double)((double)ProgressBar!.Value / (double)ProgressBar.Maximum);
                if (MillisecondsSpeed > 0) Label!.Text = $"{ProgressBar.Value} / {ProgressBar.Maximum} - {percent:P3} @ {CountSize((ulong)(MillisecondsSpeed * 1000))}/s";
                else Label!.Text = "0 / 0 - 0.000% @ 0 B/s";
                zTimer!.Enabled = true;
            }
            catch
            {
                return;
            }
        }
    }
}
