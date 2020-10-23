using System;
using System.Threading;

namespace Hush
{
    public sealed class MicrophoneThread : IDisposable
    {
        private readonly Thread _thread;
        private readonly ManualResetEvent _started;
        private readonly ManualResetEvent _stopped;
        private bool _disposed;

        public event EventHandler<MicrophoneStatus> StateChanged = (s, e) => { };

        public MicrophoneThread()
        {
            _started = new ManualResetEvent(false);
            _stopped = new ManualResetEvent(false);
            _thread = new Thread(UpdateState)
            {
                IsBackground = true
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();

                _disposed = true;
                _started.Dispose();
                _stopped.Dispose();
            }
        }

        public void Start()
        {
            if (!_started.WaitOne(0))
            {
                _thread.Start();
            }
        }

        public void Stop()
        {
            if (!_started.WaitOne(0))
            {
                // Wait for the status checker to exit.
                if (!_thread.Join(TimeSpan.FromSeconds(2)))
                {
                    _thread.Abort();
                }
            }
        }

        private void UpdateState()
        {
            try
            {
                _started.Set();

                var knownState = Microphone.GetPrimaryMicrophoneStatus();

                while (true)
                {
                    var status = Microphone.GetPrimaryMicrophoneStatus();
                    if (status.State != knownState.State)
                    {
                        if (status.State == MicrophoneState.Muted)
                        {
                            StateChanged(this, status);
                        }
                        else if(status.State == MicrophoneState.Unmuted)
                        {
                            StateChanged(this, status);
                        }

                        knownState = status;
                    }

                    // Wait for a little while. 
                    if (_stopped.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Thread was aborted
            }
        }
    }
}
