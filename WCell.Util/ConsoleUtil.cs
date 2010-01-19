using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

namespace WCell.Util
{
	public class ConsoleUtil
	{
		public static bool ConsoleVisible = true;

		public const int WM_FONTCHANGE = 0x001D;
		public const int HWND_BROADCAST = 0xffff;
		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;

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
		public static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

		[DllImport("kernel32")]
		public static extern bool SetConsoleIcon(IntPtr hIcon);

		[DllImport("kernel32")]
		public static extern bool AllocConsole();

		[DllImport("kernel32")]
		public static extern bool FreeConsole();

		[DllImport("kernel32")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("kernel32")]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32")]
		public static extern bool GetCurrentConsoleFont(IntPtr hConsoleOutput, bool bMaximumWindow, out CONSOLE_FONT_INFO lpConsoleCurrentFont);

		[DllImport("kernel32")]
		public static extern bool SetConsoleFont(IntPtr hConsoleOutput, int fontIndex);

		[DllImport("gdi32")]
		public static extern int AddFontResource(string lpFileName);

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
			SetConsoleIcon(icon.Handle);
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

		public static void ApplyFont(string FontName)
		{
			RegistryKey defaultKey = Registry.CurrentUser.CreateSubKey("Console");
			defaultKey.SetValue("FaceName", FontName);
			defaultKey.Close();
			RegistryKey masterKey = Registry.CurrentUser.CreateSubKey(@"Console\" + Application.ExecutablePath.Replace(@"\","_"));
			masterKey.SetValue("FaceName", FontName);
			masterKey.Close();
		}

		public static void InstallFont(string FontType ,string FontName, string FontFileName)
		{
			SendMessage((IntPtr)HWND_BROADCAST, WM_FONTCHANGE, 0, IntPtr.Zero);
			string FontPath = Path.Combine(Directory.GetCurrentDirectory(), @"Resources\" + FontFileName);
			AddFontResource(FontPath);
			WriteProfileString("fonts", FontName + " " + FontType, FontPath);
		}

		public static void ApplyCustomFont()
		{
			ApplyFont("LerosC");
		}

		public static void InstallCustomFontAndApply()
		{
			InstallFont("(All Res)", "LerosC", "LerosC.fon");
			for (var i = 0; i <= 9; i++)
			{
				SetConsoleFont(i);
				COORD FontSize = GetConsoleFontSize();
				if (FontSize.X == 6 && FontSize.Y == 11)
				{
					break;
				}
				else if (FontSize.X == 8 && FontSize.Y == 12)
				{
					break;
				}
			}
		}

		public static void SetConsoleFont(int font)
		{
			IntPtr hConsoleOutput = GetStdHandle(STD_OUTPUT_HANDLE);
			if (hConsoleOutput != IntPtr.Zero)
			{
				SetConsoleFont(hConsoleOutput, font);
			}
		}

		public static void PositionConsoleWindow(int Left, int Top, int Width, int Height)
		{
			COORD FontSize = GetConsoleFontSize();
			if (FontSize.X > 0 && FontSize.Y > 0)
			{
				IntPtr hWnd = GetConsoleWindow();
				if (hWnd != IntPtr.Zero)
				{
					MoveWindow(hWnd, Left, Top, Width * FontSize.X, Height * FontSize.Y, true);
				}
			}
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
