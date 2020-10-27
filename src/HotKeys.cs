using System;
using System.Windows.Forms;

namespace Hush
{
    public sealed class KeyboardHook : IDisposable
    {
        private readonly KeyboardHookWindow _window;
        private const int HOTKEY_ID = 0;

        public event EventHandler<KeyPressedEventArgs> KeyCombinationPressed = (s, e) => { };

        public KeyboardHook()
        {
            _window = new KeyboardHookWindow((modifiers, key) => KeyCombinationPressed(this, new KeyPressedEventArgs(modifiers, key)));
            NativeMethods.RegisterHotKey(_window.Handle, HOTKEY_ID, (uint)(ModifierKeys.Alt | ModifierKeys.Shift), (uint)Keys.M);
        }

        private class KeyboardHookWindow : NativeWindow, IDisposable
        {
            private const int WM_HOTKEY = 0x0312;
            private readonly Action<ModifierKeys, Keys> _callback;

            public KeyboardHookWindow(Action<ModifierKeys, Keys> callback)
            {
                _callback = callback ?? throw new ArgumentNullException(nameof(callback));
                CreateHandle(new CreateParams());
            }

            public void Dispose()
            {
                DestroyHandle();
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_HOTKEY)
                {
                    var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    _callback(modifier, key);
                }
            }
        }

        public void Dispose()
        {
            NativeMethods.UnregisterHotKey(_window.Handle, HOTKEY_ID);
            _window.Dispose();
        }
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public ModifierKeys Modifier { get; }
        public Keys Key { get; }

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            Modifier = modifier;
            Key = key;
        }
    }

    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Alt = 1,
        Control = 1 << 1,
        Shift = 1 << 2,
        Win = 1 << 3
    }
}
