using Xunit;

namespace Sculk.Protocol.Tests;

public class SerializerTests
{
    [Theory]
    // TODO: use `[MemberData]` instead and generate combinations at runtime
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 5)]
    [InlineData(true, 5)]
    public unsafe void UnsafeWriteBoolTest(bool value, int bufferSize)
    {
        Span<byte> expected = stackalloc byte[bufferSize];
        expected[0] = *(byte*) &value;

        Span<byte> actual = stackalloc byte[bufferSize];
        Serializer.UnsafeWriteBool(actual, value);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 5)]
    [InlineData(true, 5)]
    public unsafe void TryWriteBoolSucceedTest(bool value, int bufferSize)
    {
        var expected = new byte[bufferSize];
        expected[0] = *(byte*) &value;

        var actual = new byte[bufferSize];
        var success = Serializer.TryWriteBool(actual, value);

        Assert.Multiple(
            () => Assert.True(success),
            () => Assert.Equal(expected, actual));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TryWriteBoolFailTest(bool value)
    {
        var buffer = Span<byte>.Empty;
        var success = Serializer.TryWriteBool(buffer, value);

        Assert.False(success);
    }

    // TODO: the rest of the tests
}