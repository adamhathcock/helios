using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{    
    public class AdaptiveRecvByteBufAllocatorTests
    {
        [Fact]
        public void Should_initialize_default()
        {
            var def = AdaptiveRecvByteBufAllocator.Default;
            var handle = def.NewHandle();
            Assert.Equal(1024, handle.Guess());
        }

        [Fact]
        public void Should_initialize_new()
        {
            var min = 100;
            var max = 10000;
            var init = 1024;

            var def = new AdaptiveRecvByteBufAllocator(min, init, max);
            var handle = def.NewHandle();

            Assert.Equal(init, handle.Guess());
        }
    }
}