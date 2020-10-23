using System;
using System.Windows.Forms;

namespace Hush
{
    public sealed class Program : ApplicationContext
    {
        private readonly TaskbarIcon _taskbar;
        private readonly MicrophoneThread _statusThread;

        [STAThread]
        public static void Main()
        {
            if (InstanceDetector.IsAnotherInstanceRunning())
            {
                return;
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var program = new Program())
            {
                Application.Run(program);
            }
        }

        public Program()
        {
            _taskbar = new TaskbarIcon(Microphone.GetPrimaryMicrophoneStatus());
            _taskbar.Initialized += OnInitialized;
            _taskbar.ItemClicked += OnTaskBarItemClicked;

            _statusThread = new MicrophoneThread();
            _statusThread.StateChanged += OnMicrophoneStateChanged;
            _statusThread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _taskbar.Dispose();
            _statusThread.Dispose();
        }

        private void OnInitialized(object? sender, EventArgs e)
        {
            _taskbar.Update(Microphone.GetPrimaryMicrophoneStatus());
        }

        private void OnMicrophoneStateChanged(object? sender, MicrophoneStatus e)
        {
            _taskbar.Update(e);
        }

        private void OnTaskBarItemClicked(object? sender, TaskbarEvent @event)
        {
            if (@event == TaskbarEvent.Exit)
            {
                Exit();
            }
            else if (@event == TaskbarEvent.Mute)
            {
                // Mute microphone
                var status = Microphone.SetPrimaryMicrophoneState(MicrophoneState.Muted);
                if (status.State == MicrophoneState.Muted)
                {
                    _taskbar.ShowMessage("Muted microphone", status.Name);
                    _taskbar.Update(status);
                }
                else
                {
                    _taskbar.ShowError($"An error occured when muting microphone {status.Name}.");
                }
            }
            else if (@event == TaskbarEvent.Unmute)
            {
                // Unmute microphone
                var status = Microphone.SetPrimaryMicrophoneState(MicrophoneState.Unmuted);
                if (status.State == MicrophoneState.Unmuted)
                {
                    _taskbar.ShowMessage("Unmuted microphone", status.Name);
                    _taskbar.Update(status);
                }
                else
                {
                    _taskbar.ShowError($"An error occured when unmuting microphone {status.Name}.");
                }
            }
        }

        private void Exit()
        {
            // Stop the status thread
            _statusThread.Stop();

            // Terminate the message pump
            ExitThread();
        }
    }
}
