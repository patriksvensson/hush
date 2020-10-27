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

        public event EventHandler TaskbarInitialized = (s, e) => { };
        public event EventHandler<TaskbarEvent> TaskbarItemClicked = (s, e) => { };
        public event EventHandler TaskbarDoubleClicked = (s, e) => { };

        public TaskbarIcon(MicrophoneStatus status)
        {
            _inititalized = new ManualResetEvent(false);

            _muteMenuItem = new ToolStripMenuItem("Mute", null, (s, e) => TaskbarItemClicked(this, TaskbarEvent.Mute));
            _muteMenuItem.ShortcutKeyDisplayString = "CTRL+ALT+M";
            _unmuteMenuItem = new ToolStripMenuItem("Unmute", null, (s, e) => TaskbarItemClicked(this, TaskbarEvent.Unmute));
            _unmuteMenuItem.ShortcutKeyDisplayString = "CTRL+ALT+M";
            _exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => TaskbarItemClicked(this, TaskbarEvent.Exit));

            _menu = new ContextMenuStrip();
            _menu.HandleCreated += (s, e) =>
            {
                _inititalized.Set();
                TaskbarInitialized(this, EventArgs.Empty);
            };

            _menu.Items.Add(_muteMenuItem);
            _menu.Items.Add(_unmuteMenuItem);
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add(_exitMenuItem);

            _notifyIcon = new NotifyIcon();
            _notifyIcon.ContextMenuStrip = _menu;
            _notifyIcon.MouseDoubleClick += (s, e) => TaskbarDoubleClicked(s, e);

            UpdateTaskbarIcon(status);
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
            _notifyIcon.ShowBalloonTip(1000, title, message, ToolTipIcon.Info);
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
            UpdateTaskbarIcon(status);
            UpdateMenu(status);
        }

        private void UpdateMenu(MicrophoneStatus? status)
        {
            if (status == null)
            {
                return;
            }

            if (!_inititalized.WaitOne(0))
            {
                return;
            }

            // Invoke on the UI thread
            _menu.Invoke((Action)(() =>
            {
                switch (status.State)
                {
                    case MicrophoneState.Muted:
                        _muteMenuItem.Enabled = false;
                        _unmuteMenuItem.Enabled = true;
                        break;
                    case MicrophoneState.Unmuted:
                        _unmuteMenuItem.Enabled = false;
                        _muteMenuItem.Enabled = true;
                        break;
                    case MicrophoneState.Error:
                        _unmuteMenuItem.Enabled = false;
                        _muteMenuItem.Enabled = false;
                        break;
                }
            }));
        }

        private void UpdateTaskbarIcon(MicrophoneStatus? status)
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
                    break;
                case MicrophoneState.Unmuted:
                    _notifyIcon.Icon = Resources.Icon_Unmuted;
                    break;
                case MicrophoneState.Error:
                    _notifyIcon.Icon = Resources.Icon_Error;
                    break;
            };
        }
    }
}
