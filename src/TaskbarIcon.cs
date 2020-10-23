using Hush.Properties;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Hush
{
    public enum TaskbarEvent
    {
        Mute,
        Unmute,
        Exit,
    }

    public sealed class TaskbarIcon : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _muteMenuItem;
        private readonly ToolStripMenuItem _unmuteMenuItem;
        private readonly ToolStripMenuItem _exitMenuItem;
        private readonly ManualResetEvent _inititalized;

        public event EventHandler Initialized = (s, e) => { };
        public event EventHandler<TaskbarEvent> ItemClicked = (s, e) => { };

        public TaskbarIcon(MicrophoneStatus status)
        {
            _inititalized = new ManualResetEvent(false);

            _muteMenuItem = new ToolStripMenuItem("Mute", null, (s, e) => ItemClicked(this, TaskbarEvent.Mute));
            _unmuteMenuItem = new ToolStripMenuItem("Unmute", null, (s, e) => ItemClicked(this, TaskbarEvent.Unmute));
            _exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => ItemClicked(this, TaskbarEvent.Exit));

            _menu = new ContextMenuStrip();
            _menu.HandleCreated += (s, e) =>
            {
                _inititalized.Set();
                Initialized(this, EventArgs.Empty);
            };

            _menu.Items.Add(_muteMenuItem);
            _menu.Items.Add(_unmuteMenuItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(_exitMenuItem);

            _notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = _menu,
            };

            SetStatus(status);
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
            _menu.Dispose();
            _muteMenuItem.Dispose();
            _unmuteMenuItem.Dispose();
            _exitMenuItem.Dispose();
            _inititalized.Dispose();
        }

        public void ShowMessage(string title, string message)
        {
            if (_inititalized.WaitOne(0))
            {
                _notifyIcon.ShowBalloonTip(1000, title, message, ToolTipIcon.Info);
            }
        }

        public void ShowError(string message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (_inititalized.WaitOne(0))
            {
                _notifyIcon.ShowBalloonTip(3000, "Hush", message, ToolTipIcon.Error);
            }
            else
            {
                // We could not show a notification bubble so show
                // the error as an old fashion message box instead.
                MessageBox.Show(message, "Hush: Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Update(MicrophoneStatus? status)
        {
            if (_inititalized.WaitOne(0))
            {
                // Invoke on the UI thread
                _menu.Invoke((Action)(() =>
                {
                    SetStatus(status);
                }));
            }
        }

        private void SetStatus(MicrophoneStatus? status)
        {
            if (status == null)
            {
                return;
            }

            _notifyIcon.Text = status.GetStatusDescription();
            _notifyIcon.Visible = true;

            switch (status.State)
            {
                case MicrophoneState.Muted:
                    _notifyIcon.Icon = Resources.Icon_Muted;
                    _muteMenuItem.Enabled = false;
                    _unmuteMenuItem.Enabled = true;
                    break;
                case MicrophoneState.Unmuted:
                    _notifyIcon.Icon = Resources.Icon_Unmuted;
                    _unmuteMenuItem.Enabled = false;
                    _muteMenuItem.Enabled = true;
                    break;
                case MicrophoneState.Error:
                    _notifyIcon.Icon = Resources.Icon_Error;
                    _unmuteMenuItem.Enabled = false;
                    _muteMenuItem.Enabled = false;
                    break;
            };
        }
    }
}
