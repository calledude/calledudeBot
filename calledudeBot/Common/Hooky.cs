using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Services;
using Open.WinKeyboardHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace calledudeBot
{
    public sealed class Hooky
    {
        private bool _allIsSelected;
        private bool _ctrlKeyToggled;
        private KeyboardInterceptor _key;
        private bool _f9KeyToggled;
        private string _messageToSend = "";
        private int _position;
        private readonly TwitchBot _twitchBot;

        public Hooky(TwitchBot twitchBot) => _twitchBot = twitchBot;

        public void Start()
        {
            _key = new KeyboardInterceptor();

            _key.KeyDown += Key_KeyDown;
            _key.KeyUp += Key_KeyUp;
            _key.KeyPress += Key_KeyPress;

            _key.StartCapturing();

            Logger.Log("[Hooky] Started Hooky.");
            Application.Run();
        }

        private void Key_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.KeyCode.ToString().Contains("Control")) return;
            if (ApplicationIsActivated())
                _ctrlKeyToggled = false;
        }

        private void Key_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ApplicationIsActivated()) return;
            if (e.KeyCode == Keys.F9)
            {
                _f9KeyToggled = !_f9KeyToggled;
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (_f9KeyToggled)
                {
                    _f9KeyToggled = false;
                }
            }
            else if (e.KeyCode == Keys.A && _ctrlKeyToggled)
            {
                _allIsSelected = true;
            }
            else if (e.KeyCode.ToString().Contains("Control"))
            {
                _ctrlKeyToggled = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (_position > 0) _position--;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (_position < _messageToSend.Length) _position++;
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (_allIsSelected)
                {
                    _messageToSend = "";
                    _allIsSelected = false;
                    _position = 0;
                }
                else if (_messageToSend.Length > 0 && _position > 0)
                {
                    _messageToSend = _messageToSend.Remove(_position - 1, 1);
                    if (_position > 0) _position--;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_allIsSelected)
                {
                    _messageToSend = "";
                    _allIsSelected = false;
                    _position = 0;
                }

                if (_messageToSend.Length > 0 && _position < _messageToSend.Length)
                    _messageToSend = _messageToSend.Remove(_position, 1);
            }
        }

        private async void Key_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_f9KeyToggled || !ApplicationIsActivated()) return;
            if (e.KeyChar == (char)Keys.Escape)
            {
                _f9KeyToggled = false;
                return;
            }

            if (char.IsLetterOrDigit(e.KeyChar) || char.IsSymbol(e.KeyChar)
                || char.IsWhiteSpace(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsSeparator(e.KeyChar))
            {
                _messageToSend = _messageToSend.Insert(_position, e.KeyChar.ToString());
                if (_position < _messageToSend.Length) _position++;
                if (_messageToSend.Length == 0) _position++;
            }

            if (e.KeyChar == (char)Keys.Return && _messageToSend.Length > 0)
            {
                _position = 0;
                await _twitchBot.SendMessageAsync(new IrcMessage(_messageToSend));
                _messageToSend = "";
            }
        }

        private bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false; // No window is currently activated

            const string procName = "osu!";
            var procId = 0;

            foreach (var proc in Process.GetProcesses())
            {
                if (proc.ProcessName.Equals(procName))
                {
                    procId = proc.Id;
                    break;
                }
            }
            GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            return activeProcId == procId;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}