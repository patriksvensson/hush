using System.Threading;

namespace Hush
{
    public static class InstanceDetector
    {
        private static EventWaitHandle? _eventWaitHandle;
        private const string HushApplicationId = "1ff598cf-d14e-4941-97d8-91da73d27a7d";

        public static bool IsAnotherInstanceRunning()
        {
            try
            {
                _eventWaitHandle = EventWaitHandle.OpenExisting(HushApplicationId);
                return true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, HushApplicationId);
            }

            return false;
        }
    }
}
