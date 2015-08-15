using Helios.Buffers;

namespace Helios.Tests.Buffer
{
    public interface IBufferFactory
    {
        IByteBuf GetBuffer(int initialCapacity);
        IByteBuf GetBuffer(int initialCapacity, int maxCapacity);
    }
}