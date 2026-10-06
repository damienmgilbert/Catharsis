using System.Reflection;

namespace Catharsis.Serialization;

///<summary>
///Serializes and deserializes instances of <typeparamref name="T"/> to and from a fixed binary layout, driven by
///<see cref="BinaryFieldAttribute"/> on each participating property. Supports <see cref="bool"/>, <see cref="byte"/>,
///<see cref="short"/>, <see cref="int"/>, <see cref="long"/>, <see cref="float"/>, and <see cref="double"/> fields,
///written in <see cref="BinaryFieldAttribute.Order"/> order using little-endian byte order.
///</summary>
///<typeparam name="T">The record type to serialize. Must have a public parameterless constructor.</typeparam>
///<exception cref="InvalidOperationException">
///<typeparamref name="T"/> has no properties decorated with <see cref="BinaryFieldAttribute"/>.
///</exception>
public sealed class BinaryRecordSerializer<T> where T : new()
{
    #region Fields
    static readonly PropertyInfo[] _fields = [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(static property => property.GetCustomAttribute<BinaryFieldAttribute>() is not null)
        .OrderBy(static property => property.GetCustomAttribute<BinaryFieldAttribute>()!.Order)];
    #endregion

    #region Constructors
    ///<remarks>
    ///This validation deliberately lives here rather than in the static field initializer above: an exception
    ///thrown from a type initializer is wrapped in <see cref="TypeInitializationException"/> by the runtime, which
    ///would obscure the actual failure reason from callers.
    ///</remarks>
    public BinaryRecordSerializer()
    {
        if (_fields.Length == 0)
        {
            throw new InvalidOperationException($"Type '{typeof(T).Name}' has no properties decorated with [BinaryField].");
        }
    }
    #endregion

    #region Private methods

    static object ReadValue(BinaryReader reader, Type type)
    {
        if (type == typeof(int))
        {
            return reader.ReadInt32();
        }

        if (type == typeof(long))
        {
            return reader.ReadInt64();
        }

        if (type == typeof(short))
        {
            return reader.ReadInt16();
        }

        if (type == typeof(byte))
        {
            return reader.ReadByte();
        }

        if (type == typeof(float))
        {
            return reader.ReadSingle();
        }

        if (type == typeof(double))
        {
            return reader.ReadDouble();
        }

        if (type == typeof(bool))
        {
            return reader.ReadBoolean();
        }

        throw new NotSupportedException($"Field type '{type.Name}' is not supported by {nameof(BinaryRecordSerializer<T>)}.");
    }

    static void WriteValue(BinaryWriter writer, Type type, object? value)
    {
        switch (value)
        {
            case int i:
                writer.Write(i);
                break;
            case long l:
                writer.Write(l);
                break;
            case short s:
                writer.Write(s);
                break;
            case byte b:
                writer.Write(b);
                break;
            case float f:
                writer.Write(f);
                break;
            case double d:
                writer.Write(d);
                break;
            case bool bo:
                writer.Write(bo);
                break;
            default:
                throw new NotSupportedException($"Field type '{type.Name}' is not supported by {nameof(BinaryRecordSerializer<T>)}.");
        }
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Deserializes a record from its fixed binary layout.
    ///</summary>
    ///<param name="data">The bytes to deserialize.</param>
    ///<returns>The deserialized record.</returns>
    public T Deserialize(ReadOnlySpan<byte> data)
    {
        T record = new();

        using MemoryStream stream = new(data.ToArray());
        using BinaryReader reader = new(stream);

        foreach (PropertyInfo property in _fields)
        {
            property.SetValue(record, ReadValue(reader, property.PropertyType));
        }

        return record;
    }

    ///<summary>
    ///Serializes a record to its fixed binary layout.
    ///</summary>
    ///<param name="record">The record to serialize.</param>
    ///<returns>The serialized bytes.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="record"/> is <c>null</c>.</exception>
    public byte[] Serialize(T record)
    {
        ArgumentNullException.ThrowIfNull(record);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        foreach (PropertyInfo property in _fields)
        {
            WriteValue(writer, property.PropertyType, property.GetValue(record));
        }

        return stream.ToArray();
    }
    #endregion
}
