using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using WCell.Util;

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

		private void OnMouseClick(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					ConsoleUtil.ConsoleVisible = !ConsoleUtil.ConsoleVisible;
					ConsoleUtil.SetConsoleWindowVisibility(ConsoleUtil.ConsoleVisible);
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
			notifyIcon.Text = Console.Title;
			menu.MenuItems.Add("&Say hello world");
			menu.MenuItems.Add("-");
			menu.MenuItems.Add("&Close");
			notifyIcon.ContextMenu = menu;
			notifyIcon.MouseClick += new MouseEventHandler(OnMouseClick);
		}
	}
}
