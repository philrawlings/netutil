using Microsoft.Extensions.Hosting;
using NetUtil.Domain;
using NetUtil.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetUtil.Servers
{
    public class TcpEchoServer : IDisposable
    {
        private readonly TcpEchoServerConfig config;
		private readonly Socket bindSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private readonly Channel<TcpEvent>? externalEventChannel;
		private readonly Channel<TcpEvent>? logFileEventChannel;
		private CancellationTokenSource? logFileTaskCts = null;
		private Task? logFileTask = null;

		bool disposed = false;

        public TcpEchoServer(TcpEchoServerConfig config, Channel<TcpEvent>? eventChannel)
        {
            this.config = config;
			this.externalEventChannel = eventChannel;
			if (!string.IsNullOrWhiteSpace(config.EventLogFilePath))
			{
				this.logFileEventChannel = Channel.CreateUnbounded<TcpEvent>();
			}
		}

		public TcpEchoServer(TcpEchoServerConfig config) : this(config, null)
		{
		}

		public async Task RunAsync(CancellationToken stoppingToken)
        {
			bindSocket.Bind(config.Bind);
			bindSocket.Listen();

			if (logFileEventChannel is not null)
			{
				// Start event log file task
				// TODO - improve this to properly handle exceptions
				logFileTask = Task.Run(async () =>
				{
					logFileTaskCts = new CancellationTokenSource();
					using (var writer = new CsvStreamWriter(config.EventLogFilePath))
					{
						writer.WriteEntireRow("Timestamp", "Connection ID", "End Point 1", "End Point 2", "Type", "Data");
						while (true)
						{
							var tcpEvent = await logFileEventChannel.Reader.ReadAsync(logFileTaskCts.Token);
							writer.WriteEntireRow(tcpEvent.TimeStampUtc, tcpEvent.ConnectionID, tcpEvent.Source, tcpEvent.Destination, tcpEvent.Type, tcpEvent.GetFormattedDataString(config.Format));
						}
					}
				});
			}

			while (true)
			{
				Socket client = await bindSocket.AcceptAsync(stoppingToken);

				// Fire off task to handle new client connection
				// TODO - improve this to close the connection after a timeout
				_ = Task.Run(async () =>
				{
					int connID = IdGenerator.Next();
					string source = string.Empty;
					string dest = string.Empty;

					if (bindSocket.LocalEndPoint is not null)
						source = bindSocket.LocalEndPoint.ToString() ?? string.Empty;

					try
					{
						if (client.RemoteEndPoint is not null)
							dest = client.RemoteEndPoint.ToString() ?? string.Empty;

						await WriteEvent(TcpEventType.Connected, source, dest, connID, null, 0);

						byte[] buffer = new byte[4096];
						while (true)
						{
							var readLen = await client.ReceiveAsync(buffer, SocketFlags.None, stoppingToken);
							if (readLen > 0)
							{
								await WriteEvent(TcpEventType.OutboundData, source, dest, connID, buffer, readLen);

								var writeLen = 0;
								while (readLen > 0)
								{
									var sent = client.Send(buffer, writeLen, readLen, SocketFlags.None);
									readLen -= sent;
									writeLen += sent;
								}
							}
						}
					}
					catch (Exception ex)
					{
						var data = Encoding.UTF8.GetBytes(ex.Message);
						await WriteEvent(TcpEventType.Error, source, dest, connID, data, data.Length);
					}
					finally
                    {
						try { client.Dispose(); } catch { }

						if (logFileTask is not null)
						{
							logFileTaskCts?.Cancel();
							try
							{
								await logFileTask;
							}
							catch { }
						}
					}
				}, stoppingToken);
			}
		}

		private async Task WriteEvent(TcpEventType eventType, string source, string dest, int connID, byte[]? buffer, int len)
        {
			if (externalEventChannel is not null)
            {
				if (config.SendDataEventsToEventChannel || (eventType != TcpEventType.InboundData && eventType != TcpEventType.OutboundData))
				{
					byte[]? data = null;
					if (buffer is not null)
					{
						data = new byte[len];
						Buffer.BlockCopy(buffer, 0, data, 0, len);
					}

					await externalEventChannel.Writer.WriteAsync(new TcpEvent
					{
						Type = eventType,
						Source = source,
						Destination = dest,
						ConnectionID = connID,
						TimeStampUtc = DateTime.UtcNow,
						Data = data
					});
				}
			}

			if (logFileEventChannel is not null)
			{
				byte[]? data = null;
				if (buffer is not null)
				{
					data = new byte[len];
					Buffer.BlockCopy(buffer, 0, data, 0, len);
				}

				await logFileEventChannel.Writer.WriteAsync(new TcpEvent
				{
					Type = eventType,
					Source = source,
					Destination = dest,
					ConnectionID = connID,
					TimeStampUtc = DateTime.UtcNow,
					Data = data
				});
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~TcpEchoServer()
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
