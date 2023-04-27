using Microsoft.Extensions.Hosting;
using NetUtil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetUtil.Servers
{
    internal class TcpClient : IDisposable
    {
        private readonly TcpClientConfig config;
		private readonly Socket bindSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private readonly Channel<TcpEvent>? eventChannel;

		bool disposed = false;

        public TcpClient(TcpClientConfig config, Channel<TcpEvent>? eventChannel)
        {
            this.config = config;
			this.eventChannel = eventChannel;
        }

		public TcpClient(TcpClientConfig config) : this(config, null)
		{
		}

		public async Task RunAsync(CancellationToken stoppingToken)
        {
			int connID = 1;
			string source = string.Empty;
			string dest = string.Empty;

			try
			{
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					socket.Connect(config.Connect);
					if (socket.LocalEndPoint is not null)
						source = socket.LocalEndPoint.ToString() ?? string.Empty;
					if (socket.RemoteEndPoint is not null)
						dest = socket.RemoteEndPoint.ToString() ?? string.Empty;

					if (eventChannel is not null)
					{
						await eventChannel.Writer.WriteAsync(new TcpEvent
						{
							Type = TcpEventType.Connected,
							Source = source,
							Destination = dest,
							ConnectionID = connID,
							TimeStampUtc = DateTime.UtcNow,
							Data = null
						});
					}

					byte[] buffer = new byte[4096];
					while (true)
					{
						stoppingToken.ThrowIfCancellationRequested();
						var readLen = await socket.ReceiveAsync(buffer, SocketFlags.None, stoppingToken);
						if (readLen > 0)
						{
							if (config.IncludeDataEvents)
								await WriteInboundData(eventChannel, source, dest, connID, buffer, readLen);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (eventChannel is not null)
				{
					await eventChannel.Writer.WriteAsync(new TcpEvent
					{
						Type = TcpEventType.Error,
						Source = source,
						Destination = dest,
						ConnectionID = connID,
						TimeStampUtc = DateTime.UtcNow,
						Data = Encoding.UTF8.GetBytes(ex.Message)
					});
				}
			}
		}

        private static async Task WriteInboundData(Channel<TcpEvent>? eventChannel, string source, string dest, int connID, byte[] buffer, int len)
        {
			if (eventChannel is null)
				return;

			byte[] data = new byte[len];
			Buffer.BlockCopy(buffer, 0, data, 0, len);
			await eventChannel.Writer.WriteAsync(new TcpEvent
			{
				Type = TcpEventType.InboundData,
				Source = source,
				Destination = dest,
				ConnectionID = connID,
				TimeStampUtc = DateTime.UtcNow,
				Data = data
			});
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~TcpClient()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				// Dispose managed resources
				try
				{
					bindSocket.Dispose();
				}
				catch { }
			}

			// Dispose unmanaged resources

			disposed = true;
		}

		private static class IdGenerator
        {
			private static int nextID = 0;
			private static readonly object locker = new();

			public static int Next()
            {
				lock (locker)
                {
					return ++nextID;
                }
            }

        }

	}
}
