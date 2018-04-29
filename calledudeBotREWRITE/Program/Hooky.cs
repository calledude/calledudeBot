using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Open.WinKeyboardHook;

namespace calledudeBot
{
    public class Hooky
    {
        private bool allIsSelected;
        private bool CONTROLKEY;
        private KeyboardInterceptor key;
        private bool KEYF9;
        private string MessageToSend = "";
        private int position;
        private TwitchBot twitchBot;


        //TODO: Poll ApplicationIsActivated() instead?
        //                          to decrease system overhead

        public Hooky(TwitchBot twitchBot)
        {
            this.twitchBot = twitchBot;
        }

        public void Start()
        {
            key = new KeyboardInterceptor();

            key.KeyDown += key_KeyDown;
            key.KeyUp += key_KeyUp;
            key.KeyPress += key_KeyPress;

            key.StartCapturing();

            Console.WriteLine("[Hooky] Started Hooky.");
            Application.Run();
        }

        private void key_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString().Contains("Control"))
                if (ApplicationIsActivated())
                    CONTROLKEY = false;
        }

        private void key_KeyDown(object sender, KeyEventArgs e)
        {
            if (ApplicationIsActivated())
                if (e.KeyCode == Keys.F9)
                {
                    KEYF9 = KEYF9 ? false : true;
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
                    if (position > 0) position -= 1;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    if (position < MessageToSend.Length - 1) position += 1;
                }
                else if (e.KeyCode == Keys.Back)
                {
                    if (allIsSelected)
                    {
                        MessageToSend = "";
                        allIsSelected = false;
                        position = 0;
                        return;
                    }

                    if (MessageToSend.Length > 0 && position > 0)
                    {
                        MessageToSend = MessageToSend.Remove(position - 1, 1);
                        if (position > 0) position -= 1;
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
            if (KEYF9 && ApplicationIsActivated())
            {
                if (e.KeyChar == (char) Keys.Escape)
                {
                    KEYF9 = false;
                    return;
                }

                if (char.IsLetterOrDigit(e.KeyChar) || char.IsSymbol(e.KeyChar) ||
                    char.IsWhiteSpace(e.KeyChar) || char.IsPunctuation(e.KeyChar) || char.IsSeparator(e.KeyChar))
                {
                    MessageToSend = MessageToSend.Insert(position, e.KeyChar.ToString());
                    if (position <= MessageToSend.Length - 1) position += 1;
                    if (MessageToSend.Length == 0) position += 1;
                }

                if (e.KeyChar == (char) Keys.Return && MessageToSend.Length > 0)
                {
                    MessageToSend.Trim();
                    MessageToSend = MessageToSend.Replace("\t", "").Replace("\r", "").Replace("\n", "");
                    MessageToSend = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(MessageToSend));
                    position = 0;
                    twitchBot.sendMessage(new Message(MessageToSend));
                    MessageToSend = "";
                }
            }
        }


        private static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) return false; // No window is currently activated
            var procName = "osu!";
            var procId = 0;


            var processlist = Process.GetProcesses();

            foreach (var theprocess in processlist)
            {
                if (theprocess.ProcessName == procName)
                {
                    procId = theprocess.Id;
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