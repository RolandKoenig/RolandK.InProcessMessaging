namespace RolandK.InProcessMessaging.Tests;

[Collection(TestConstants.SINGLE_TEST_COLLECTION_NAME)]
public class InProcessMessageMetadataHelperTests
{
    [Fact]
    public void GoodCase_MessageClass()
    {
        // Arrange
        var msg = new TestMessageClass();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.True(resultType);
        Assert.True(resultTypeAndValue);
    }

    [Fact]
    public void GoodCase_MessageStruct()
    {
        // Arrange
        var msg = new TestMessageStruct();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.True(resultType);
        Assert.True(resultTypeAndValue);
    }

    [Fact]
    public void GoodCase_MessageRecord()
    {
        // Arrange
        var msg = new TestMessageRecord();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.True(resultType);
        Assert.True(resultTypeAndValue);
    }

    [Fact]
    public void BadCase_MessageClass_NoAttributeOnClass()
    {
        // Arrange
        var msg = new TestMessageClass_WithoutAttribute();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.False(resultType);
        Assert.False(resultTypeAndValue);
    }

    [Fact]
    public void GoodCase_MessageStruct_NoAttributeOnStruct()
    {
        // Arrange
        var msg = new TestMessageStruct_WithoutAttribute();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.False(resultType);
        Assert.False(resultTypeAndValue);
    }

    [Fact]
    public void BadCase_MessageRecord_NoAttributeOnRecord()
    {
        // Arrange
        var msg = new TestMessageRecord_WithoutAttribute();

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(msg.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(msg, out _);

        // Assert
        Assert.False(resultType);
        Assert.False(resultTypeAndValue);
    }

    [Fact]
    public void BadCase_MessageIsNull()
    {
        // Act
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue<TestMessageClass>(null!, out _);

        // Assert
        Assert.False(resultTypeAndValue);
    }

    [Fact]
    public void BadCase_InvalidMessageType()
    {
        // Arrange
        var message = "Test Message";

        // Act
        var resultType = InProcessMessageMetadataHelper.ValidateMessageType(message.GetType(), out _);
        var resultTypeAndValue = InProcessMessageMetadataHelper.ValidateMessageTypeAndValue(message, out _);

        // Assert
        Assert.False(resultType);
        Assert.False(resultTypeAndValue);
    }

    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private class TestMessageClass { }

    private class TestMessageClass_WithoutAttribute { }

    [InProcessMessage]
    private struct TestMessageStruct { }

    private struct TestMessageStruct_WithoutAttribute { }

    [InProcessMessage]
    private record TestMessageRecord;

    private record TestMessageRecord_WithoutAttribute;
}
