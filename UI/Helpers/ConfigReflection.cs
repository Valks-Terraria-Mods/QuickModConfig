using System;
using System.Reflection;
using Terraria.ModLoader;

namespace QuickModConfig;

internal static class ConfigReflection
{
    /// <summary>
    /// ModContent.GetInstance<T>() does not have a type parameter so that is why this method was created.
    /// </summary>
    internal static object? GetConfigInstance(Type configType)
    {
        var method = typeof(ModContent).GetMethod("GetInstance", Type.EmptyTypes);

        if (method == null)
        {
            QuickModConfig.Log("ModContent.GetInstance method not found - tModLoader API changed?");
            return null;
        }

        var generic = method.MakeGenericMethod(configType);

        var instance = generic.Invoke(null, null);

        if (instance == null)
        {
            QuickModConfig.Log($"ModContent.GetInstance<{configType.Name}>() returned null.");
            return null;
        }

        return instance;
    }

    internal static object? GetMemberValue(MemberInfo member, object target)
    {
        switch (member)
        {
            case PropertyInfo p: return p.GetValue(target);
            case FieldInfo f: return f.GetValue(target);
            default:
                QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                return null;
        }
    }

    internal static void SetMemberValue(MemberInfo member, object target, object value)
    {
        switch (member)
        {
            case PropertyInfo p: p.SetValue(target, value); break;
            case FieldInfo f: f.SetValue(target, value); break;
            default:
                QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                break;
        }
    }

    internal static Type? GetMemberType(MemberInfo member)
    {
        switch (member)
        {
            case PropertyInfo p:
                return p.PropertyType;
            case FieldInfo f:
                return f.FieldType;
            default:
                QuickModConfig.Log($"Unsupported member type: {member.MemberType}");
                return null;
        }
    }

    internal static float ConvertToFloat(object? value, float fallback)
    {
        if (value == null)
            return fallback;

        // Unwrap Nullable<T>
        Type type = value.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var hasValue = (bool)type.GetProperty("HasValue")!.GetValue(value)!;
            if (!hasValue) return fallback;
            value = type.GetProperty("Value")!.GetValue(value)!;
        }

        return value switch
        {
            float f => f,
            double d => (float)d,
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            _ => Convert.ToSingle(value)
        };
    }

    internal static object ConvertToMemberType(float sliderValue, Type targetType)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType)!;
        if (underlying != null)
            targetType = underlying;

        if (targetType == typeof(float)) return sliderValue;
        if (targetType == typeof(double)) return (double)sliderValue;
        if (targetType == typeof(int)) return (int)Math.Round(sliderValue);
        if (targetType == typeof(long)) return (long)Math.Round(sliderValue);
        if (targetType == typeof(short)) return (short)Math.Round(sliderValue);
        if (targetType == typeof(byte)) return (byte)Math.Clamp(Math.Round(sliderValue), 0, 255);
        if (targetType == typeof(bool)) return sliderValue >= 0.5f;

        return Convert.ChangeType(sliderValue, targetType);
    }
}
