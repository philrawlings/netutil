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
    internal class TcpProxyServer : IDisposable
    {
        private readonly TcpProxyServerConfig config;
		private readonly Socket bindSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private readonly Channel<TcpEvent>? eventChannel;

		bool disposed = false;

        public TcpProxyServer(TcpProxyServerConfig config, Channel<TcpEvent>? eventChannel)
        {
            this.config = config;
			this.eventChannel = eventChannel;
        }

		public TcpProxyServer(TcpProxyServerConfig config) : this(config, null)
		{
		}

		public async Task RunAsync(CancellationToken stoppingToken)
        {
			bindSocket.Bind(config.Bind);
			bindSocket.Listen();

			while (true)
			{
				stoppingToken.ThrowIfCancellationRequested();
				Socket client = await bindSocket.AcceptAsync(stoppingToken);

				_ = Task.Run(async () =>
				{
					int connID = IdGenerator.Next();
					string source = string.Empty;
					string dest = string.Empty;

					if (bindSocket.LocalEndPoint is not null)
						source = bindSocket.LocalEndPoint.ToString() ?? string.Empty;

					try
					{
						using (Socket destSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
						{
							destSocket.Connect(config.Destination);
							if (destSocket.RemoteEndPoint is not null)
								dest = destSocket.RemoteEndPoint.ToString() ?? string.Empty;

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

							var outboundTask = Task.Run(async () =>
							{
								byte[] buffer = new byte[4096];
								while (true)
								{
									stoppingToken.ThrowIfCancellationRequested();
									var readLen = await client.ReceiveAsync(buffer, SocketFlags.None, stoppingToken);
									if (readLen > 0)
									{
										if (config.IncludeDataEvents)
											await WriteOutboundData(eventChannel, source, dest, connID, buffer, readLen);

										var writeLen = 0;
										while (readLen > 0)
										{
											var sent = destSocket.Send(buffer, writeLen, readLen, SocketFlags.None);
											readLen -= sent;
											writeLen += sent;
										}
									}
								}
							}, stoppingToken);

							var inboundTask = Task.Run(async () =>
							{
								byte[] buffer = new byte[4096];
								while (true)
								{
									stoppingToken.ThrowIfCancellationRequested();
									var readLen = await destSocket.ReceiveAsync(buffer, SocketFlags.None, stoppingToken);
									if (readLen > 0)
									{
										if (config.IncludeDataEvents)
											await WriteInboundData(eventChannel, source, dest, connID, buffer, readLen);

										var writeLen = 0;
										while (readLen > 0)
										{
											var sent = client.Send(buffer, writeLen, readLen, SocketFlags.None);
											readLen -= sent;
											writeLen += sent;
										}
									}
								}
							}, stoppingToken);

							await Task.WhenAll(outboundTask, inboundTask);
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
					finally
                    {
						try { client.Dispose(); } catch { }
                    }
				}, stoppingToken);
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

		private static async Task WriteOutboundData(Channel<TcpEvent>? eventChannel, string source, string dest, int connID, byte[] buffer, int len)
		{
			if (eventChannel is null)
				return;

			byte[] data = new byte[len];
			Buffer.BlockCopy(buffer, 0, data, 0, len);
			await eventChannel.Writer.WriteAsync(new TcpEvent
			{
				Type = TcpEventType.OutboundData,
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

		~TcpProxyServer()
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
