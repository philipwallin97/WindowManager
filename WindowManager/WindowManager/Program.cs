using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace WindowManager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                GenerateJson();
            }
            else
            {
                MoveWindows();
            }
        }

        const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static void MoveWindow(string progName, int x, int y, int width, int height)
        {
            for (int i = 0; i < Process.GetProcessesByName(progName).Length; i++)
            {
                IntPtr ptr = Process.GetProcessesByName(progName)[i].MainWindowHandle;
                SetWindowPos(ptr, IntPtr.Zero, x, y, width, height, SWP_SHOWWINDOW);
            }
        }

        static void MoveWindows()
        {
            AppList appl = JsonConvert.DeserializeObject<AppList>(File.ReadAllText(Directory.GetCurrentDirectory() + "/applist.json"));

            foreach (var app in appl.Apps)
            {
                MoveWindow(app.Name, app.X, app.Y, app.Width, app.Height);
            }
        }

        static void GenerateJson()
        {
            AppList appl = new AppList
            {
                Apps = new List<App>()
            };

            var procs = Process.GetProcesses();
            var sortedProcs = procs.OrderBy(p => p.ProcessName);
            foreach (var proc in sortedProcs)
            {
                if (!string.IsNullOrEmpty(proc.MainWindowTitle))
                {
                    IntPtr ptr = proc.MainWindowHandle;
                    Rect r = new Rect();
                    GetWindowRect(ptr, ref r);
                    App _t = new App
                    {
                        Name = proc.ProcessName,
                        X = r.Left,
                        Y = r.Top,
                        Width = (r.Right - r.Left),
                        Height = (r.Bottom - r.Top)
                    };

                    appl.Apps.Add(_t);
                }
            }

            var jsonString = JsonConvert.SerializeObject(appl);

            File.WriteAllText(Directory.GetCurrentDirectory() + "/applist.json", jsonString);
        }
    }

    public class AppList
    {
        public List<App> Apps { get; set; }
    }

    public class App
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
