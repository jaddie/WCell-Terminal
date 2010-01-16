using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WCell.Terminal
{
	class SysTrayNotifyIcon 
	{
		private const string WCellTerminalIcon = "WCell.Terminal.Resources.WCell.Terminal.ico";

		private NotifyIcon notifyIcon = new NotifyIcon();
		private ContextMenu menu = new ContextMenu();

		public bool Visible
		{
			get { return notifyIcon.Visible; }
			set { notifyIcon.Visible = value; }
		}

		public void Dispose()
		{
			menu.Dispose();
			notifyIcon.Dispose();
		}

		private bool ConsoleVisible = true;

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private void setConsoleWindowVisibility(bool visible, string title)
		{
			IntPtr hWnd = FindWindow(null, title);
			if (hWnd != IntPtr.Zero)
			{
				if (!visible)
				{
					ShowWindow(hWnd, 0);
				}
				else
				{
					ShowWindow(hWnd, 1);
					SetForegroundWindow(hWnd);
				}
			}
		}

		private void OnMouseClick(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					ConsoleVisible = !ConsoleVisible;
					setConsoleWindowVisibility(ConsoleVisible, Console.Title);
					break;
				case MouseButtons.Right:
					break;
				case MouseButtons.Middle:
					break;
				default:
					break;
			}
		}

		public SysTrayNotifyIcon()
		{
			Assembly a = Assembly.GetExecutingAssembly();
			Icon icon = new Icon(a.GetManifestResourceStream(WCellTerminalIcon));
			notifyIcon.Icon = icon;
			menu.MenuItems.Add("&Say hello world");
			menu.MenuItems.Add("-");
			menu.MenuItems.Add("&Close");
			notifyIcon.ContextMenu = menu;
			notifyIcon.MouseClick += new MouseEventHandler(OnMouseClick);
		}
	}
}
