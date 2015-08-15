using Helios.Channel;
using Xunit;
using System;

namespace Helios.Tests.Channel
{
    public class DefaultMessageSizeEstimatorTests
    {
        private readonly string MessageSizeEstimatorName = "MESSAGE_SIZE_ESTIMATOR";

        [Fact]
        public void DefaultMessageSizeEstimator_should_exist_channel_option()
        {
            Assert.True(ChannelOption.Exists(MessageSizeEstimatorName));

            ChannelOption<IMessageSizeEstimator> option = ChannelOption.ValueOf(MessageSizeEstimatorName);
            Assert.True(ChannelOption.Exists(MessageSizeEstimatorName));
            Assert.NotNull(option);
        }

        [Fact]
        public void DefaultMessageSizeEstimator_should_properly_initialize()
        {
            var mse = new DefaultMessageSizeEstimator(0);
            var handle = mse.NewHandle();
            var handle2 = mse.NewHandle();
            Assert.Equal(handle, handle2);
        }

        [Fact]
        public void DefaultMessageSizeEstimator_should_fail_initialization()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { var mse = new DefaultMessageSizeEstimator(-10); });

            Assert.Throws<ArgumentOutOfRangeException>(
                delegate { var mse = new DefaultMessageSizeEstimator(-500); });
        }

        [Fact]
        public void DefaultMessageSizeEstimator_should_return_unknown_size()
        {
            var unknownSize = 100;

            var mse = new DefaultMessageSizeEstimator(unknownSize);

            Assert.Equal(unknownSize, mse.NewHandle().Size(null));
            Assert.Equal(unknownSize, mse.NewHandle().Size(1));
        }
    }
}