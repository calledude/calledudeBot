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
        private bool allIsSelected;
        private bool CONTROLKEY;
        private KeyboardInterceptor key;
        private bool KEYF9;
        private string MessageToSend = "";
        private int position;
        private readonly TwitchBot twitchBot;

        public Hooky(TwitchBot twitchBot) => this.twitchBot = twitchBot;

        public void Start()
        {
            key = new KeyboardInterceptor();

            key.KeyDown += key_KeyDown;
            key.KeyUp += key_KeyUp;
            key.KeyPress += key_KeyPress;

            key.StartCapturing();

            Logger.Log("[Hooky] Started Hooky.");
            Application.Run();
        }

        private void key_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.KeyCode.ToString().Contains("Control")) return;
            if (applicationIsActivated())
                CONTROLKEY = false;
        }

        private void key_KeyDown(object sender, KeyEventArgs e)
        {
            if (!applicationIsActivated()) return;
            if (e.KeyCode == Keys.F9)
            {
                KEYF9 = !KEYF9;
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (KEYF9)
                {
                    KEYF9 = false;
                }
            }
            else if (e.KeyCode == Keys.A && CONTROLKEY)
            {
                allIsSelected = true;
            }
            else if (e.KeyCode.ToString().Contains("Control"))
            {
                CONTROLKEY = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (position > 0) position--;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (position < MessageToSend.Length) position++;
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (allIsSelected)
                {
                    MessageToSend = "";
                    allIsSelected = false;
                    position = 0;
                }
                else if (MessageToSend.Length > 0 && position > 0)
                {
                    MessageToSend = MessageToSend.Remove(position - 1, 1);
                    if (position > 0) position--;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (allIsSelected)
                {
                    MessageToSend = "";
                    allIsSelected = false;
                    position = 0;
                }

                if (MessageToSend.Length > 0 && position < MessageToSend.Length)
                    MessageToSend = MessageToSend.Remove(position, 1);
            }
        }

        private void key_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!KEYF9 || !applicationIsActivated()) return;
            if (e.KeyChar == (char)Keys.Escape)
            {
                KEYF9 = false;
                return;
            }

            if (char.IsLetterOrDigit(e.KeyChar) || char.IsSymbol(e.KeyChar)
                || char.IsWhiteSpace(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsSeparator(e.KeyChar))
            {
                MessageToSend = MessageToSend.Insert(position, e.KeyChar.ToString());
                if (position < MessageToSend.Length) position++;
                if (MessageToSend.Length == 0) position++;
            }

            if (e.KeyChar == (char)Keys.Return && MessageToSend.Length > 0)
            {
                position = 0;
                twitchBot.SendMessage(new IrcMessage(MessageToSend, false));
                MessageToSend = "";
            }
        }

        private bool applicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) return false; // No window is currently activated
            const string procName = "osu!";
            var procId = 0;

            foreach (var proc in Process.GetProcesses())
            {
                if (proc.ProcessName == procName)
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