using BenchmarkDotNet.Attributes;

namespace Sculk.Protocol.Benchmarks;

public class SerializerBenchmarks
{
    [Params(false, true)]
    public bool BoolData { get; set; }

    private readonly byte[] _buffer = new byte[1000];

    [Benchmark]
    public void UnsafeWriteBoolBenchmark()
        => Serializer.UnsafeWriteBool(_buffer, BoolData);
    
    // TODO: the rest of the benchmarks
}