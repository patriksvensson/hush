namespace Hush
{
    public static class MicrophoneStatusExtensions
    {
        public static string GetStatusDescription(this MicrophoneStatus? status)
        {
            var name = string.IsNullOrWhiteSpace(status?.Name) ? "Hush" : status.Name;
            var text = (status?.State ?? MicrophoneState.Error) switch
            {
                MicrophoneState.Muted => "Muted",
                MicrophoneState.Unmuted => "Unmuted",
                _ => "Error",
            };

            return $"{text}: {name}";
        }
    }
}
