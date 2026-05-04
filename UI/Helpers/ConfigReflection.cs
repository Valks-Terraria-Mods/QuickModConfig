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
        Type? underlying = Nullable.GetUnderlyingType(value.GetType());
        if (underlying != null)
            value = Convert.ChangeType(value, underlying);

        return value switch
        {
            float f => f,
            double d => (float)d,
            decimal m => (float)m,
            int i => i,
            uint ui => ui,
            long l => l,
            ulong ul => ul,
            short s => s,
            ushort us => us,
            byte b => b,
            sbyte sb => sb,
            _ => Convert.ToSingle(value)
        };
    }

    internal static object ConvertToMemberType(float sliderValue, Type targetType)
    {
        // Unwrap Nullable<T> – we always work with the underlying type
        Type? underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying != null)
            targetType = underlying;

        if (targetType == typeof(float)) return sliderValue;
        if (targetType == typeof(double)) return (double)sliderValue;
        if (targetType == typeof(decimal)) return (decimal)sliderValue;
        if (targetType == typeof(bool)) return sliderValue >= 0.5f;

        // All integer types: round first, then clamp to the type's full range.
        double rounded = Math.Round(sliderValue);

        if (targetType == typeof(int)) return (int)Math.Clamp(rounded, int.MinValue, int.MaxValue);
        if (targetType == typeof(long)) return (long)Math.Clamp(rounded, long.MinValue, long.MaxValue);
        if (targetType == typeof(short)) return (short)Math.Clamp(rounded, short.MinValue, short.MaxValue);
        if (targetType == typeof(sbyte)) return (sbyte)Math.Clamp(rounded, sbyte.MinValue, sbyte.MaxValue);
        if (targetType == typeof(byte)) return (byte)Math.Clamp(rounded, byte.MinValue, byte.MaxValue);
        if (targetType == typeof(ushort)) return (ushort)Math.Clamp(rounded, ushort.MinValue, ushort.MaxValue);
        if (targetType == typeof(uint)) return (uint)Math.Clamp(rounded, uint.MinValue, uint.MaxValue);
        if (targetType == typeof(ulong)) return (ulong)Math.Clamp(rounded, ulong.MinValue, ulong.MaxValue);

        // Fallback for any other convertible type
        return Convert.ChangeType(sliderValue, targetType);
    }
}
