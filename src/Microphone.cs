using NAudio.CoreAudioApi;
using System;

namespace Hush
{
    public sealed class Microphone : IDisposable
    {
        private readonly MMDevice _device;

        public string Name => _device.FriendlyName;
        public bool IsMuted => _device.AudioEndpointVolume.Mute;

        public Microphone(MMDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
        }

        public MicrophoneStatus GetStatus()
        {
            var state = IsMuted ? MicrophoneState.Muted : MicrophoneState.Unmuted;
            return new MicrophoneStatus(state, Name);
        }

        public void Dispose()
        {
            _device?.Dispose();
        }

        public void Mute()
        {
            _device.AudioEndpointVolume.Mute = true;
        }

        public void Unmute()
        {
            _device.AudioEndpointVolume.Mute = false;
        }

        public static MicrophoneStatus GetPrimaryMicrophoneStatus()
        {
            using var microphone = GetPrimaryMicrophone();
            if (microphone == null)
            {
                return new MicrophoneStatus(MicrophoneState.Error, string.Empty);
            }

            return microphone.GetStatus();
        }

        public static MicrophoneStatus SetPrimaryMicrophoneState(MicrophoneState state)
        {
            using var microphone = GetPrimaryMicrophone();
            if (microphone != null)
            {
                if (state == MicrophoneState.Muted && !microphone.IsMuted)
                {
                    microphone.Mute();
                    return microphone.GetStatus();
                }
                else if (state == MicrophoneState.Unmuted && microphone.IsMuted)
                {
                    microphone.Unmute();
                    return microphone.GetStatus();
                }
            }

            return new MicrophoneStatus(MicrophoneState.Error, string.Empty);
        }

        private static Microphone? GetPrimaryMicrophone()
        {
            using var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            if (device == null)
            {
                return null;
            }

            return new Microphone(device);
        }
    }
}
