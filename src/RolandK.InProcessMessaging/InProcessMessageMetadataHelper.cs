﻿using System;
using System.Reflection;
using RolandK.InProcessMessaging.Exceptions;

namespace RolandK.InProcessMessaging;

public static class InProcessMessageMetadataHelper
{
    public static bool ValidateMessageTypeAndValue<T>(T messageValue, out string? errorMessage)
    {
        errorMessage = null;
        var messageType = typeof(T);

        if(!ValidateMessageType(messageType, out errorMessage))
        {
            return false;
        }

        if((messageType.IsClass) &&
           (messageValue == null))
        {
            errorMessage = $"Invalid message type {messageType.FullName}: Message value is null!";
            return false;
        }

        return true;
    }

    public static bool ValidateMessageType(Type messageType, out string? errorMessage)
    {
        errorMessage = null;
        if (messageType.GetCustomAttribute<InProcessMessageAttribute>() == null)
        {
            errorMessage =
                $"Invalid message type {messageType.FullName}: Message types have to be marked with InProcessMessageAttribute!";
            return false;
        }
        return true;
    }

    public static void EnsureValidMessageTypeAndValue<T>(T messageValue)
    {
        if (!ValidateMessageTypeAndValue(messageValue, out var errorMessage))
        {
            throw new InProcessMessagingCheckException(errorMessage!);
        }
    }

    public static void EnsureValidMessageType(Type messageType)
    {
        if (!ValidateMessageType(messageType, out var errorMessage))
        {
            throw new InProcessMessagingCheckException(errorMessage!);
        }
    }

    /// <summary>
    /// Gets a list containing all target messengers for message routing.
    /// An empty list means that no routing logic applies.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    public static string[] GetAsyncRoutingTargetMessengersOfMessageType(Type messageType)
    {
        if (messageType.GetTypeInfo().GetCustomAttribute(typeof(MessageAsyncRoutingTargetsAttribute)) is
            MessageAsyncRoutingTargetsAttribute routingAttrib)
        {
            return routingAttrib.AsyncTargetMessengers;
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Gets a list containing all possible source messengers for the given message type.
    /// An empty list means that every messenger can fire this message
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    public static string[] GetPossibleSourceMessengersOfMessageType(Type messageType)
    {
        if (messageType.GetTypeInfo().GetCustomAttribute(typeof(MessagePossibleSourceAttribute)) is
            MessagePossibleSourceAttribute routingAttrib)
        {
            return routingAttrib.PossibleSourceMessengers;
        }
        return Array.Empty<string>();
    }
}