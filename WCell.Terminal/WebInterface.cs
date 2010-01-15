using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WCell.Terminal
{
	class WebInterface
	{
		private TcpListener myListener;
		public static string ListenAddress = IPAddress.Loopback.ToString();
		public static int Port = 8080;
		public static bool Enabled = true;
		String dateTime;

		public WebInterface()
		{
			dateTime = DateTime.Now.ToString("hh:mm");

			if (Enabled)
			{				
				try
				{
					myListener = new TcpListener( IPAddress.Parse(ListenAddress), Port);
					myListener.Start();
					Console.WriteLine("({0}) <Web Interface> Running on Port {1}...", dateTime, Port);

					Thread th = new Thread(new ThreadStart(StartListen));
					th.Start();
				}
				catch (Exception e)
				{
					Console.WriteLine("({0}) <Web Interface> An Exception Occurred while Listening: {1}", dateTime, e.ToString());
				}
			}
			else
			{
				Console.WriteLine("({0}) <Web Interface> Disabled", dateTime);
			}
		}

		public void StartListen()
		{
			int iStartPos = 0;
			String sRequest;
			String sDirName;
			String sRequestedFile;
			String sErrorMessage;
			String sResponse = "";
			StreamReader terminaloutput;

			while (true)
			{
				Socket mySocket = myListener.AcceptSocket();
				Console.WriteLine("({0}) <Web Interface> Socket Type {1}", dateTime, mySocket.SocketType);
				if (mySocket.Connected)
				{
					Console.WriteLine("({0}) <Web Interface> Client Connected!!!", dateTime);
					Console.WriteLine("({0}) <Web Interface> Client IP {1}", dateTime, mySocket.RemoteEndPoint);
					Byte[] bReceive = new Byte[1024];
					int i = mySocket.Receive(bReceive, bReceive.Length, 0);
					string sBuffer = Encoding.ASCII.GetString(bReceive);
					if (sBuffer.Substring(0, 3) != "GET")
					{
						Console.WriteLine("({0}) <Web Interface> Only Get Method is supported...", dateTime);
						mySocket.Close();
						return;
					}
					iStartPos = sBuffer.IndexOf("HTTP", 1);
					string sHttpVersion = sBuffer.Substring(iStartPos, 8);
					sRequest = sBuffer.Substring(0, iStartPos - 1);
					sRequest.Replace("\\", "/");
					if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
					{
						sRequest = sRequest + "/";
					}
					iStartPos = sRequest.LastIndexOf("/") + 1;
					sRequestedFile = sRequest.Substring(iStartPos);
					sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);
					if (sDirName == "/")
					{
						terminaloutput = new StreamReader("WCell.Terminal.log");
						sResponse = terminaloutput.ReadToEnd();
						SendHeader(sHttpVersion, "text/plain", sResponse.Length, " 200 OK", ref mySocket);
						SendToBrowser(sResponse, ref mySocket);
						mySocket.Close();
					}
					else
					{
						sErrorMessage = "<H2>Error!! Requested Directory does not exists</H2><Br>";
						SendHeader(sHttpVersion, "text/plain", sErrorMessage.Length, " 404 Not Found", ref mySocket);
						SendToBrowser(sErrorMessage, ref mySocket);
						mySocket.Close();
						continue;
					}
				}
			}
		}

		public void SendHeader(string sHttpVersion,	string sMIMEHeader,	int iTotBytes, string sStatusCode, ref Socket mySocket)
		{
			String sBuffer = "";

			if (sMIMEHeader.Length == 0)
			{
				sMIMEHeader = "text/plain";
			}
			sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
			sBuffer = sBuffer + "Server: cx1193719-b\r\n";
			sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
			sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
			sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
			Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
			SendToBrowser(bSendData, ref mySocket);
			Console.WriteLine("({0}) <Web Interface> Total Bytes : ", dateTime ,iTotBytes.ToString());
		}

		public void SendToBrowser(String sData, ref Socket mySocket)
		{
			SendToBrowser(Encoding.ASCII.GetBytes(sData), ref mySocket);
		}

		public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
		{
			int numBytes = 0;
			try
			{
				if (mySocket.Connected)
				{
					if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1)
					{
						Console.WriteLine("({0}) <Web Interface> Socket Error cannot Send Packet", dateTime);
					}
					else
					{
						Console.WriteLine("({0}) <Web Interface> No. of bytes send {1}", dateTime, numBytes);
					}
				}
				else
				{
					Console.WriteLine("({0}) <Web Interface> Connection Dropped....", dateTime);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("({0}) <Web Interface> Error Occurred : {1}", dateTime, e);
			}
		}
	}
}