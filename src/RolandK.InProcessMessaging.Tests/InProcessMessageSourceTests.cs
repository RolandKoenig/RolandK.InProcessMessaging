namespace RolandK.InProcessMessaging.Tests;

[Collection(TestConstants.SINGLE_TEST_COLLECTION_NAME)]
public class InProcessMessageSourceTests
{
    [Fact]
    public void MessageSourceWithCustomTarget_Class()
    {
        // Prepare
        var customHandlerCalled = false;
        var messageSource = new InProcessMessageSource<TestMessageClass>("TestMessenger");
        messageSource.UnitTesting_ReplaceByCustomMessageTarget(
            _ => customHandlerCalled = true);

        // Execute test
        messageSource.Publish(new TestMessageClass("Testing argument"));

        // Check results
        Assert.True(customHandlerCalled);
    }

    [Fact]
    public void MessageSourceWithRealTarget_Class()
    {
        // Prepare
        var dummyMessenger = new InProcessMessenger();
        dummyMessenger.ConnectToGlobalMessaging(
            InProcessMessengerThreadingBehavior.Ignore,
            "DummyMessenger",
            null);
        try
        {
            var realHandlerCalled = false;
            dummyMessenger.Subscribe<TestMessageClass>(_ => realHandlerCalled = true);

            // Execute test
            var messageSource = new InProcessMessageSource<TestMessageClass>("DummyMessenger");
            messageSource.Publish(new TestMessageClass("Testing argument"));

            // Check results
            Assert.True(realHandlerCalled);
        }
        finally
        {
            // Cleanup
            dummyMessenger.DisconnectFromGlobalMessaging();
        }
    }

    [Fact]
    public void MessageSourceWithCustomTarget_Struct()
    {
        // Prepare
        var customHandlerCalled = false;
        var messageSource = new InProcessMessageSource<TestMessageStruct>("TestMessenger");
        messageSource.UnitTesting_ReplaceByCustomMessageTarget(
            _ => customHandlerCalled = true);

        // Execute test
        messageSource.Publish(new TestMessageStruct("Testing argument"));

        // Check results
        Assert.True(customHandlerCalled);
    }

    [Fact]
    public void MessageSourceWithRealTarget_Struct()
    {
        // Prepare
        var dummyMessenger = new InProcessMessenger();
        dummyMessenger.ConnectToGlobalMessaging(
            InProcessMessengerThreadingBehavior.Ignore,
            "DummyMessenger",
            null);
        try
        {
            var realHandlerCalled = false;
            dummyMessenger.Subscribe<TestMessageStruct>(_ => realHandlerCalled = true);

            // Execute test
            var messageSource = new InProcessMessageSource<TestMessageStruct>("DummyMessenger");
            messageSource.Publish(new TestMessageStruct("Testing argument"));

            // Check results
            Assert.True(realHandlerCalled);
        }
        finally
        {
            // Cleanup
            dummyMessenger.DisconnectFromGlobalMessaging();
        }
    }

    [Fact]
    public void MessageSourceWithCustomTarget_Record()
    {
        // Prepare
        var customHandlerCalled = false;
        var messageSource = new InProcessMessageSource<TestMessageRecord>("TestMessenger");
        messageSource.UnitTesting_ReplaceByCustomMessageTarget(
            _ => customHandlerCalled = true);

        // Execute test
        messageSource.Publish(new TestMessageRecord("Testing argument"));

        // Check results
        Assert.True(customHandlerCalled);
    }

    [Fact]
    public void MessageSourceWithRealTarget_Record()
    {
        // Prepare
        var dummyMessenger = new InProcessMessenger();
        dummyMessenger.ConnectToGlobalMessaging(
            InProcessMessengerThreadingBehavior.Ignore,
            "DummyMessenger",
            null);
        try
        {
            var realHandlerCalled = false;
            dummyMessenger.Subscribe<TestMessageRecord>(_ => realHandlerCalled = true);

            // Execute test
            var messageSource = new InProcessMessageSource<TestMessageRecord>("DummyMessenger");
            messageSource.Publish(new TestMessageRecord("Testing argument"));

            // Check results
            Assert.True(realHandlerCalled);
        }
        finally
        {
            // Cleanup
            dummyMessenger.DisconnectFromGlobalMessaging();
        }
    }

    //*************************************************************************
    //*************************************************************************
    //*************************************************************************

    [InProcessMessage]
    private class TestMessageClass
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string TestArg { get; }

        public TestMessageClass(string testArg)
        {
            this.TestArg = testArg;
        }
    }

    [InProcessMessage]
    private struct TestMessageStruct
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string TestArg { get; }

        public TestMessageStruct(string testArg)
        {
            this.TestArg = testArg;
        }
    }

    [InProcessMessage]
    public record TestMessageRecord(string TestArg);
}
