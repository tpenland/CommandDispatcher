namespace CommandDispatcher.Mqtt.Core.Tests
{
    public class GuardTests
    {
        [Fact]
        public void IsNull_NullValue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Guard.IsNull<string>(null));
        }

        [Fact]
        public void IsNull_NonNullValue_DoesNotThrow()
        {
            Guard.IsNull("test");
            Assert.True(true);
        }

        [Fact]
        public void IsEmpty_NullList_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Guard.IsEmpty<string>(null));
        }

        [Fact]
        public void IsEmpty_EmptyList_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Guard.IsEmpty(new List<string>()));
        }

        [Fact]
        public void IsEmpty_NonEmptyList_DoesNotThrow()
        {
            Guard.IsEmpty(new List<string> { "test" });
            Assert.True(true);
        }
    }
}
