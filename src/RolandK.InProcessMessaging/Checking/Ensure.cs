using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RolandK.InProcessMessaging.Exceptions;

namespace RolandK.InProcessMessaging.Checking;

/// <summary>
/// This class contains some helper methods which can be used
/// to check method parameters.
/// </summary>
internal static class Ensure
{
    public static void EnsureEqual(
        this object toCompare, object other, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (toCompare != other)
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} has not the expected value!");
        }
    }

    public static void EnsureEqualComparable<T>(
        this T toCompare, T other, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
        where T : IComparable<T>
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (toCompare.CompareTo(other) != 0)
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} has not the expected value!");
        }
    }

    public static void EnsureFalse(
        this bool boolValue, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (boolValue)
        {
            throw new InProcessMessagingCheckException(
                $"Boolean {checkedVariableName} within method {callerMethod} must be false!");
        }
    }

    public static void EnsureTrue(
        this bool boolValue, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (!boolValue)
        {
            throw new InProcessMessagingCheckException(
                $"Boolean {checkedVariableName} within method {callerMethod} must be true!");
        }
    }

    public static void EnsureNotNullOrEmpty<T>(
        this T[] array, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (array == null ||
            array.Length == 0)
        {
            throw new InProcessMessagingCheckException(
                $"Array {checkedVariableName} within method {callerMethod} must not be null or empty!");
        }
    }

    public static void EnsureNotNullOrEmpty<T>(
        this ICollection<T> collection, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (collection == null ||
            collection.Count == 0)
        {
            throw new InProcessMessagingCheckException(
                $"Collection {checkedVariableName} within method {callerMethod} must not be null or empty!");
        }
    }

    public static void EnsureNotNullOrEmpty<T>(
        this IReadOnlyCollection<T> collection, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (collection == null ||
            collection.Count == 0)
        {
            throw new InProcessMessagingCheckException(
                $"Collection {checkedVariableName} within method {callerMethod} must not be null or empty!");
        }
    }

    public static void EnsureNotNullOrEmpty(
        this string stringParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (string.IsNullOrEmpty(stringParam))
        {
            throw new InProcessMessagingCheckException(
                $"String {checkedVariableName} within method {callerMethod} must not be null or empty!");
        }
    }

    public static void EnsureNotNullOrEmptyOrWhiteSpace(
        this string stringParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (string.IsNullOrWhiteSpace(stringParam))
        {
            throw new InProcessMessagingCheckException(
                $"String {checkedVariableName} within method {callerMethod} must not be null or empty!");
        }
    }

    public static void EnsureNotNull(
        this object objParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (objParam == null)
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} must not be null!");
        }
    }
    
    public static void EnsureNotNullWhenNullIsNotAllowed<T>(
        this object objParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (objParam == null &&
            default(T) != null)
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} must not be null because type argument is not by ref!");
        }
    }
    
    public static void EnsureAssignableTo<T>(
        this object? objParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (objParam == null) { return; }
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        var genericTypeInfo = typeof(T).GetTypeInfo();
        var argTypeInfo = objParam.GetType().GetTypeInfo();
        if (!genericTypeInfo.IsAssignableFrom(argTypeInfo))
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} can not be assigned to type {genericTypeInfo.FullName}!");
        }
    }

    public static void EnsureNull(
        this object objParam, string checkedVariableName,
        [CallerMemberName]
        string callerMethod = "")
    {
        if (string.IsNullOrEmpty(callerMethod)) { callerMethod = "Unknown"; }

        if (objParam != null)
        {
            throw new InProcessMessagingCheckException(
                $"Object {checkedVariableName} within method {callerMethod} must be null!");
        }
    }
}