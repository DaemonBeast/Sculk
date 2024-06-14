using Microsoft.Extensions.Hosting;
using Sculk.Protocol;
using Serilog;
using Serilog.Events;

namespace Sculk.Client;

internal static class Program
{
    private static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting {Name}", Constants.Project.Name);

            Span<byte> value = stackalloc byte[5];
            Serializer.UnsafeWriteVarInt(value, 2097151, out var length);
            Log.Information("{Value}", string.Join(" ", value[..length].ToArray().Select(v => "0x" + v.ToString("X").PadLeft(2, '0'))));
            Log.Information("{Length}", length);
            
            Span<byte> stringValue = stackalloc byte[50];
            Serializer.UnsafeWriteString(stringValue, "Some String", out var stringLength);
            Log.Information("{Value}", string.Join(" ", stringValue[..stringLength].ToArray().Select(v => "0x" + v.ToString("X").PadLeft(2, '0'))));
            Log.Information("{Length}", stringLength);

            using var host = BuildHostApplication(args);
            host.Run();

            return 0;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "{Name} terminated unexpectedly", Constants.Project.Name);
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHost BuildHostApplication(string[] args)
    {
        var hostBuilder = Host.CreateApplicationBuilder();

        hostBuilder.Services.AddSerilog(loggerConfiguration =>
            loggerConfiguration
#if DEBUG
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
#else
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(hostBuilder.Configuration)
                .WriteTo.Console());

        return hostBuilder.Build();
    }
}