using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetUtil;
using NetUtil.Domain;
using NetUtil.Servers;
using NetUtil.Utilities;
using System.Net;
using System.Text;
using System.Threading.Channels;


var result = Parser.Default.ParseArguments(args, new[]
            {
                typeof(Options.Proxy),
                typeof(Options.Receive)
            });

await result.WithParsedAsync<Options.Proxy>(async opts =>
{
    try
    {
        string version = GetVersion();
        Console.WriteLine($"netutil [Version {version}] - Proxy Server");
        Console.WriteLine();

        var config = new TcpProxyServerConfig
        {
            Bind = IPEndPoint.Parse(opts.BindEndPoint),
            Destination = IPEndPoint.Parse(opts.ConnectEndPoint),
            IncludeDataEvents = opts.WriteDataToConsole
        };

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };

        var eventChannel = Channel.CreateUnbounded<TcpEvent>();
        var serverTask = Task.Run(async () =>
        {
            using (var server = new TcpProxyServer(config, eventChannel))
            {
                await server.RunAsync(cts.Token);
            }
        });

        var consoleTask = Task.Run(async () =>
        {
            while (true)
            {
                var tcpEvent = await eventChannel.Reader.ReadAsync(cts.Token);
                switch (tcpEvent.Type)
                {
                    case TcpEventType.Connected:
                        ConsoleWriteLine($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Connected", ConsoleColor.Green);
                        break;
                    case TcpEventType.OutboundData:
                        ConsoleWrite($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Out Data: ", ConsoleColor.Cyan);
                        ConsoleWriteData(tcpEvent.Data, opts.Format);
                        break;
                    case TcpEventType.InboundData:
                        ConsoleWrite($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, In Data:  ", ConsoleColor.Yellow);
                        ConsoleWriteData(tcpEvent.Data, opts.Format);
                        break;
                    case TcpEventType.Error:
                        ConsoleWriteLine($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Error: {Encoding.UTF8.GetString(tcpEvent.Data ?? Array.Empty<byte>())}", ConsoleColor.Red);
                        break;
                }
            }
        });

        await Task.WhenAll(serverTask, consoleTask);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        ConsoleWriteLine("Proxy server stopped.", ConsoleColor.Green);
        Environment.ExitCode = 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        ConsoleWriteErrorLine("An error occurred:", ConsoleColor.Red);
        Console.Error.WriteLine(ex.Message);
        Environment.ExitCode = 1;
    }
});

await result.WithParsedAsync<Options.Receive>(async opts =>
{
    try
    {
        string version = GetVersion();
        Console.WriteLine($"netutil [Version {version}] - Proxy Server");
        Console.WriteLine();

        var config = new TcpClientConfig
        {
            Connect = IPEndPoint.Parse(opts.ConnectEndPoint),
            IncludeDataEvents = opts.WriteDataToConsole
        };

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };

        var eventChannel = Channel.CreateUnbounded<TcpEvent>();
        var serverTask = Task.Run(async () =>
        {
            using (var server = new TcpClient(config, eventChannel))
            {
                await server.RunAsync(cts.Token);
            }
        });

        var consoleTask = Task.Run(async () =>
        {
            while (true)
            {
                var tcpEvent = await eventChannel.Reader.ReadAsync(cts.Token);
                switch (tcpEvent.Type)
                {
                    case TcpEventType.Connected:
                        ConsoleWriteLine($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Connected", ConsoleColor.Green);
                        break;
                    case TcpEventType.OutboundData:
                        ConsoleWrite($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Out Data: ", ConsoleColor.Cyan);
                        ConsoleWriteData(tcpEvent.Data, opts.Format);
                        break;
                    case TcpEventType.InboundData:
                        ConsoleWrite($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, In Data:  ", ConsoleColor.Yellow);
                        ConsoleWriteData(tcpEvent.Data, opts.Format);
                        break;
                    case TcpEventType.Error:
                        ConsoleWriteLine($"{tcpEvent.TimeStampUtc.ToUtcIso8601String()}, {tcpEvent.Source} -> {tcpEvent.Destination}, Conn ID: {tcpEvent.ConnectionID}, Error: {Encoding.UTF8.GetString(tcpEvent.Data ?? Array.Empty<byte>())}", ConsoleColor.Red);
                        break;
                }
            }
        });

        await Task.WhenAll(serverTask, consoleTask);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine();
        ConsoleWriteLine("Proxy server stopped.", ConsoleColor.Green);
        Environment.ExitCode = 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        ConsoleWriteErrorLine("An error occurred:", ConsoleColor.Red);
        Console.Error.WriteLine(ex.Message);
        Environment.ExitCode = 1;
    }
});

static string GetVersion()
{
    return AssemblyVersion.Instance.Get(typeof(Program));
}

static void ConsoleWrite(string message, ConsoleColor color)
{
    var prevColour = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Write(message);
    Console.ForegroundColor = prevColour;
}

static void ConsoleWriteLine(string message, ConsoleColor color)
{
    var prevColour = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ForegroundColor = prevColour;
}

static void ConsoleWriteErrorLine(string message, ConsoleColor color)
{
    var prevColour = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.Error.WriteLine(message);
    Console.ForegroundColor = prevColour;
}

static void ConsoleWriteWhiteInverted(string message)
{
    Console.BackgroundColor = ConsoleColor.Gray;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.Write(message);
    Console.ResetColor();
}

static void ConsoleWriteData(byte[]? data, DataFormat format)
{
    if (data is null || data.Length == 0)
        return;

    switch (format)
    {
        case DataFormat.Binary:
            Console.Write(BitConverter.ToString(data));
            break;
        case DataFormat.AsciiText:
            foreach (var val in data)
            {
                if (val >= 0x20 && val < 0x7F)
                {
                    Console.Write((char)val);
                }
                else
                {
                    ConsoleWriteWhiteInverted($"[{val:x2}]");
                }
            }
            break;
        case DataFormat.Utf8Text:
            Console.Write(Encoding.UTF8.GetString(data));
            break;
    }
    Console.WriteLine();
}
