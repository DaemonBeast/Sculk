using BenchmarkDotNet.Running;

namespace Sculk.Protocol.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}