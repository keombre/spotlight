using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Spotlight
{
    struct Command
    {
        public string command;
        public string[] args;
        public bool admin;
    }

    class Parser
    {
        readonly string ConfigFile = Path.Combine(Directory.GetCurrentDirectory(), @"config.xml");
        readonly XElement Config;

        internal Parser()
        {
            if (File.Exists(ConfigFile))
                Config = XElement.Load(ConfigFile);
            else
            {
                Config = new XElement("Config", new XElement("commands"));
                Config.Save(ConfigFile);
            }
        }

        internal Command? Parse(string text, bool asAdmin)
        {
            if (text.Length == 0)
                return null;

            string[] cargs = text.Split(' ');

            string command = cargs[0];
            string[] args = cargs.Skip(1).ToArray();

            XElement aliases = Config.Element("commands");
            XElement alias = aliases.XPathSelectElement(string.Format(@"//alias[@short=""{0}""]", command));
            if (alias != null)
                command = alias.Value;

            Command cmd = new Command
            {
                admin = asAdmin,
                command = command,
                args = args
            };

            return cmd;
        }

        internal bool Invoke(Command cmd)
        {

            List<string> windows = GetOpenExplorers();
            try
            {
                Process proc;
                proc = new Process();
                proc.StartInfo.FileName = cmd.command;
                proc.StartInfo.Arguments = string.Join(" ", cmd.args);
                
                if (windows.Count > 0)
                    proc.StartInfo.WorkingDirectory = windows[0];
                else
                    proc.StartInfo.WorkingDirectory = Environment.SystemDirectory;

                if (cmd.admin)
                {
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Verb = "runas";
                }
                proc.Start();
                return true;
            }
            catch (Exception ex) when (
                ex is FileNotFoundException
                || ex is System.ComponentModel.Win32Exception
            )
            {
                return false;
            }
        }

        internal List<string> GetOpenExplorers()
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();
            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                string filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                try
                {
                    if (filename.Equals("explorer"))
                        windows.Add((IntPtr)ie.HWND, new Uri(ie.LocationURL).LocalPath);
                }
                catch (UriFormatException) { };
            }

            List<string> ret = new List<string>();
            foreach(IntPtr win in BuildZOrder())
            {
                if (windows.ContainsKey(win))
                    ret.Add(windows[win]);
            }

            return ret;
        }

        private List<IntPtr> BuildZOrder()
        {
            List<IntPtr> ret = new List<IntPtr>();

            IntPtr win = GetWindow(GetForegroundWindow(), 0);
            ret.Add(win);
            while (win != IntPtr.Zero)
            {
                IntPtr tmp = GetWindow(win, 2);
                if (tmp == win || ret.Contains(tmp))
                    break;
                ret.Add(tmp);
                win = tmp;
            }
            return ret;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        internal bool InvokeLocal(Command cmd, ref string output)
        {

            List<string> windows = GetOpenExplorers();
            try
            {
                Process proc;
                proc = new Process();
                proc.StartInfo.FileName = cmd.command;
                proc.StartInfo.Arguments = string.Join(" ", cmd.args);

                if (windows.Count > 0)
                    proc.StartInfo.WorkingDirectory = windows[0];
                else
                    proc.StartInfo.WorkingDirectory = Environment.SystemDirectory;

                proc.StartInfo.UseShellExecute = false;

                if (cmd.admin)
                    proc.StartInfo.Verb = "runas";

                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();

                output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                return true;
            }
            catch (Exception ex) when (
                ex is FileNotFoundException
                || ex is System.ComponentModel.Win32Exception
            )
            {
                return false;
            }
        }
    }
}
