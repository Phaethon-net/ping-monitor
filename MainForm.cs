using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PingMonitor
{
    public partial class MainForm : Form
    {
        private Timer pingTimer;
        private string targetHost = "8.8.8.8"; // Default to Google DNS
        private DateTime? lastOfflineTime;
        private TimeSpan totalDowntime = TimeSpan.Zero;
        private bool isOnline = true;
        private Panel statusChart;
        private List<bool> statusHistory = new List<bool>();
        private TextBox hostTextBox;
        private Label statusLabel;
        private Label downtimeLabel;
        private Label lastCheckLabel;
        private Button startStopButton;
        private bool isMonitoring = false;
        private string settingsFilePath;

        public MainForm()
        {
            InitializeComponent();
            SetupChart();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PingMonitor", "settings.txt");
                
                if (File.Exists(settingsFilePath))
                {
                    string savedHost = File.ReadAllText(settingsFilePath).Trim();
                    if (!string.IsNullOrEmpty(savedHost))
                    {
                        targetHost = savedHost;
                        hostTextBox.Text = targetHost;
                    }
                }
            }
            catch
            {
                // If loading fails, use default values
            }
        }

        private void SaveSettings()
        {
            try
            {
                string directory = Path.GetDirectoryName(settingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(settingsFilePath, targetHost);
            }
            catch
            {
                // If saving fails, continue without error
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Network Ping Monitor";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Host input
            var hostLabel = new Label
            {
                Text = "Target Host/IP:",
                Location = new Point(10, 15),
                Size = new Size(100, 20)
            };
            this.Controls.Add(hostLabel);

            hostTextBox = new TextBox
            {
                Text = targetHost,
                Location = new Point(120, 12),
                Size = new Size(200, 25)
            };
            this.Controls.Add(hostTextBox);

            // Start/Stop button
            startStopButton = new Button
            {
                Text = "Start Monitoring",
                Location = new Point(330, 10),
                Size = new Size(120, 30),
                BackColor = Color.LightGreen
            };
            startStopButton.Click += StartStopButton_Click;
            this.Controls.Add(startStopButton);

            // Status labels
            statusLabel = new Label
            {
                Text = "Status: Not monitoring",
                Location = new Point(10, 50),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            this.Controls.Add(statusLabel);

            downtimeLabel = new Label
            {
                Text = "Total Downtime: 00:00:00",
                Location = new Point(10, 75),
                Size = new Size(300, 20)
            };
            this.Controls.Add(downtimeLabel);

            lastCheckLabel = new Label
            {
                Text = "Last Check: Never",
                Location = new Point(10, 100),
                Size = new Size(400, 20)
            };
            this.Controls.Add(lastCheckLabel);

            // Chart Panel
            statusChart = new Panel
            {
                Location = new Point(10, 130),
                Size = new Size(760, 420),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            statusChart.Paint += StatusChart_Paint;
            this.Controls.Add(statusChart);

            // Timer
            pingTimer = new Timer
            {
                Interval = 10000 // 10 seconds
            };
            pingTimer.Tick += async (s, e) => await PingHost();
        }

        private void SetupChart()
        {
            // Chart will be drawn using custom Paint event
            statusHistory.Clear();
        }

        private void StatusChart_Paint(object sender, PaintEventArgs e)
        {
            if (statusHistory.Count == 0) return;

            Graphics g = e.Graphics;
            int width = statusChart.Width - 40;
            int height = statusChart.Height - 60;
            int barWidth = Math.Max(1, width / Math.Max(statusHistory.Count, 50));
            
            // Draw background grid
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            {
                // Horizontal lines
                g.DrawLine(gridPen, 20, 30, width + 20, 30); // Online line
                g.DrawLine(gridPen, 20, height / 2 + 30, width + 20, height / 2 + 30); // Middle
                g.DrawLine(gridPen, 20, height + 30, width + 20, height + 30); // Offline line
            }

            // Draw labels
            using (Font font = new Font("Arial", 10))
            using (Brush blackBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("ONLINE", font, blackBrush, 5, 11);  // Raised by another ~7 pixels
                g.DrawString("OFFLINE", font, blackBrush, 5, height + 39); // Lowered by another ~7 pixels
            }

            // Draw status bars
            for (int i = 0; i < statusHistory.Count; i++)
            {
                bool isOnlineStatus = statusHistory[i];
                Color barColor = isOnlineStatus ? Color.Green : Color.Red;
                
                int x = 20 + (i * barWidth);
                int y = isOnlineStatus ? 30 : height / 2 + 30;
                int barHeight = height / 2;

                using (Brush brush = new SolidBrush(barColor))
                {
                    g.FillRectangle(brush, x, y, barWidth - 1, barHeight);
                }
            }
        }

        private void StartStopButton_Click(object sender, EventArgs e)
        {
            if (!isMonitoring)
            {
                targetHost = hostTextBox.Text.Trim();
                if (string.IsNullOrEmpty(targetHost))
                {
                    MessageBox.Show("Please enter a valid host or IP address.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save the target host for next time
                SaveSettings();

                isMonitoring = true;
                startStopButton.Text = "Stop Monitoring";
                startStopButton.BackColor = Color.LightCoral;
                hostTextBox.Enabled = false;
                
                // Clear previous data
                statusHistory.Clear();
                totalDowntime = TimeSpan.Zero;
                lastOfflineTime = null;
                
                pingTimer.Start();
                _ = PingHost(); // Start immediately
            }
            else
            {
                isMonitoring = false;
                startStopButton.Text = "Start Monitoring";
                startStopButton.BackColor = Color.LightGreen;
                hostTextBox.Enabled = true;
                pingTimer.Stop();
                statusLabel.Text = "Status: Monitoring stopped";
            }
        }

        private async Task PingHost()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(targetHost, 5000);
                    var currentTime = DateTime.Now;
                    var timeString = currentTime.ToString("HH:mm:ss");
                    
                    bool currentlyOnline = reply.Status == IPStatus.Success;
                    
                    // Update chart data
                    statusHistory.Add(currentlyOnline);
                    
                    // Keep only last 50 points for performance
                    if (statusHistory.Count > 50)
                    {
                        statusHistory.RemoveAt(0);
                    }
                    
                    // Update status tracking
                    if (!currentlyOnline && isOnline)
                    {
                        // Just went offline
                        lastOfflineTime = currentTime;
                    }
                    else if (currentlyOnline && !isOnline)
                    {
                        // Just came back online
                        if (lastOfflineTime.HasValue)
                        {
                            totalDowntime += currentTime - lastOfflineTime.Value;
                            lastOfflineTime = null;
                        }
                    }
                    else if (!currentlyOnline && lastOfflineTime.HasValue)
                    {
                        // Still offline, update current downtime display
                        var currentDowntime = totalDowntime + (currentTime - lastOfflineTime.Value);
                        downtimeLabel.Text = $"Total Downtime: {currentDowntime:hh\\:mm\\:ss}";
                    }
                    
                    isOnline = currentlyOnline;
                    
                    // Update labels
                    if (currentlyOnline)
                    {
                        statusLabel.Text = $"Status: ONLINE (0% packet loss) - {reply.RoundtripTime}ms";
                        statusLabel.ForeColor = Color.Green;
                        if (!lastOfflineTime.HasValue)
                        {
                            downtimeLabel.Text = $"Total Downtime: {totalDowntime:hh\\:mm\\:ss}";
                        }
                    }
                    else
                    {
                        statusLabel.Text = "Status: OFFLINE (100% packet loss)";
                        statusLabel.ForeColor = Color.Red;
                    }
                    
                    lastCheckLabel.Text = $"Last Check: {currentTime:yyyy-MM-dd HH:mm:ss}";
                    
                    statusChart.Invalidate(); // Trigger repaint
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            pingTimer?.Stop();
            pingTimer?.Dispose();
            
            // Save current target host if it's been changed
            if (!string.IsNullOrEmpty(hostTextBox.Text.Trim()) && hostTextBox.Text.Trim() != targetHost)
            {
                targetHost = hostTextBox.Text.Trim();
                SaveSettings();
            }
            
            base.OnFormClosing(e);
        }
    }
}
