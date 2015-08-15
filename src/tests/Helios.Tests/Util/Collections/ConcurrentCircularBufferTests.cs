using Helios.Util.Collections;
using Xunit;

namespace Helios.Tests.Util.Collections
{
    public class ConcurrentCircularBufferTests : CircularBufferTests
    {
        protected override ICircularBuffer<T> GetBuffer<T>(int capacity)
        {
            return new ConcurrentCircularBuffer<T>(capacity);
        }

        protected override ICircularBuffer<T> GetBuffer<T>(int capacity, int maxCapacity)
        {
            return new ConcurrentCircularBuffer<T>(capacity,maxCapacity);
        }
    }
}