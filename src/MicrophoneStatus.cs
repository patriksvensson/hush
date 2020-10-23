using System;

namespace Hush
{
    public sealed class MicrophoneStatus
    {
        public MicrophoneState State { get; }
        public string Name { get; }

        public MicrophoneStatus(MicrophoneState state, string name)
        {
            State = state;
            Name = name ?? string.Empty;
        }
    }

    public enum MicrophoneState
    {
        Muted,
        Unmuted,
        Error,
    }
}
