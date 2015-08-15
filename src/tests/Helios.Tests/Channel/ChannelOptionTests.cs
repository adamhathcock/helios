using System;
using Helios.Channel;
using Xunit;

namespace Helios.Tests.Channel
{
    public class ChannelOptionTests
    {
        [Fact]
        public void TestExists()
        {
            var name = "test";
            Assert.False(ChannelOption.Exists(name));

            ChannelOption<string> option = ChannelOption.ValueOf(name);
            Assert.True(ChannelOption.Exists(name));
            Assert.NotNull(option);
        }

        [Fact]
        public void TestValueOf()
        {
            var name = "test1";
            Assert.False(ChannelOption.Exists(name));
            ChannelOption<string> option = ChannelOption.ValueOf(name);
            ChannelOption<string> option2 = ChannelOption.ValueOf(name);

            Assert.Same(option, option2);
        }

        [Fact]
        public void TestCreateOrFail()
        {
            var name = "test2";
            Assert.False(ChannelOption.Exists(name));
            ChannelOption<string> option = ChannelOption.NewInstance<string>(name);
            Assert.True(ChannelOption.Exists(name));
            Assert.NotNull(option);

            Assert.Throws<ArgumentException>(()=> ChannelOption.NewInstance<string>(name));
        }
    }
}
