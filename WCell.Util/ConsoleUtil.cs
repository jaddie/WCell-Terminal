using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace WCell.Util
{
	public class ConsoleUtil
	{
		public static bool ConsoleVisible = true;

		public const int WM_SETICON = 0x80;
		public const int ICON_SMALL = 0;
		public const int ICON_BIG = 1;
		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;
		public const int CONSOLE_FULLSCREEN_MODE = 1;
		public const int CONSOLE_WINDOWED_MODE = 2;

		[StructLayout(LayoutKind.Sequential)]
		public struct COORD
		{
			public short X;
			public short Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CONSOLE_FONT_INFO
		{
			public int nFont;
			public COORD dwFontSize;
		}

		[DllImport("kernel32")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32")]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32")]
		public static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out CONSOLE_FONT_INFO lpConsoleCurrentFont);

		[DllImport("user32")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32")]
		public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32")]
		public static extern int SendMessage(IntPtr hwnd, int message, int wParam, IntPtr lParam);

		public static void SetConsoleIcon(Icon icon)
		{
			IntPtr hWnd = GetConsoleWindow();
			SendMessage(hWnd, WM_SETICON, ICON_SMALL, icon.Handle);
			SendMessage(hWnd, WM_SETICON, ICON_BIG, icon.Handle);
		}

		public static COORD GetConsoleFontSize()
		{
			COORD FontSize;
			CONSOLE_FONT_INFO FontInfo;

			IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);
			if (hConsoleOutput != IntPtr.Zero)
			{
				bool Result = GetCurrentConsoleFont(hConsoleOutput, false, out FontInfo);
				if (Result)
				{
					return FontInfo.dwFontSize;
				}
			}
			FontSize.X = 0;
			FontSize.Y = 0;
			return FontSize;
		}

		public static void CenterConsoleWindow(int Width, int Height)
		{
			Screen scr = Screen.PrimaryScreen;
			COORD FontSize = GetConsoleFontSize();
			if (FontSize.X > 0 && FontSize.Y > 0)
			{
				int X = (scr.Bounds.Width - (Width * FontSize.X)) / 2;
				int Y = (scr.Bounds.Height - (Height * FontSize.Y)) / 2;
				IntPtr hWnd = GetConsoleWindow();
				if (hWnd != IntPtr.Zero)
				{
					MoveWindow(hWnd, X, Y, Width * FontSize.X, Height * FontSize.Y, true);
				}
			}
		}

		public static void SetConsoleWindowVisibility(bool visible)
		{
			IntPtr hWnd = GetConsoleWindow();
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
	}
}
