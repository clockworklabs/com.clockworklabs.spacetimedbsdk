using System;
using System.Collections.Generic;
using System.IO;

namespace SpacetimeDB.SATS
{
    public struct BuiltinValue
    {
        private object value;

        public bool AsBool() => (bool)value;
        public sbyte AsI8() => (sbyte)value;
        public byte AsU8() => (byte)value;
        public short AsI16() => (short)value;
        public ushort AsU16() => (ushort)value;
        public int AsI32() => (int)value;
        public uint AsU32() => (uint)value;
        public long AsI64() => (long)value;
        public ulong AsU64() => (ulong)value;
        public byte[] AsI128() => (byte[])value;
        public byte[] AsU128() => (byte[])value;
        public float AsF32() => (float)value;
        public double AsF64() => (double)value;
        public byte[] AsBytes() => (byte[])value;
        public string AsString() => (string)value;
        public List<AlgebraicValue> AsArray() => (List<AlgebraicValue>)value;
        public Dictionary<AlgebraicValue, AlgebraicValue> AsMap() => (Dictionary<AlgebraicValue, AlgebraicValue>)value;

        public static BuiltinValue CreateBool(bool value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateI8(sbyte value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateU8(byte value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateI16(short value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateU16(ushort value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateI32(int value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateU32(uint value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateI64(long value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateU64(ulong value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateI128(byte[] value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateU128(byte[] value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateF32(float value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateF64(double value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateString(string value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateBytes(byte[] value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateArray(List<AlgebraicValue> value) => new BuiltinValue { value = value };
        public static BuiltinValue CreateMap(Dictionary<AlgebraicValue, AlgebraicValue> value) => new BuiltinValue { value = value };

        public void Serialize(BuiltinType type, BinaryWriter writer)
        {
            void WriteByteBuffer(byte[] buf)
            {
                if (buf.LongLength > uint.MaxValue)
                {
                    throw new Exception("Serializing a buffer that is too large for SATS.");
                }

                writer.Write((uint)buf.LongLength);
                writer.Write(buf);
            }

            switch (type.type)
            {
                case BuiltinType.Type.Bool:
                    writer.Write(AsBool());
                    break;
                case BuiltinType.Type.I8:
                    writer.Write(AsI8());
                    break;
                case BuiltinType.Type.U8:
                    writer.Write(AsU8());
                    break;
                case BuiltinType.Type.I16:
                    writer.Write(AsI16());
                    break;
                case BuiltinType.Type.U16:
                    writer.Write(AsU16());
                    break;
                case BuiltinType.Type.I32:
                    writer.Write(AsI32());
                    break;
                case BuiltinType.Type.U32:
                    writer.Write(AsU32());
                    break;
                case BuiltinType.Type.I64:
                    writer.Write(AsI64());
                    break;
                case BuiltinType.Type.U64:
                    writer.Write(AsU64());
                    break;
                case BuiltinType.Type.I128:
                    writer.Write(AsI128());
                    break;
                case BuiltinType.Type.U128:
                    writer.Write(AsU128());
                    break;
                case BuiltinType.Type.F32:
                    writer.Write(AsF32());
                    break;
                case BuiltinType.Type.F64:
                    writer.Write(AsF64());
                    break;
                case BuiltinType.Type.String:
                    WriteByteBuffer(System.Text.Encoding.UTF8.GetBytes((string)value));
                    break;
                case BuiltinType.Type.Array:
                    if (type.arrayType.type == AlgebraicType.Type.Builtin &&
                        type.arrayType.builtin.type == BuiltinType.Type.U8)
                    {
                        WriteByteBuffer(AsBytes());
                        break;
                    }

                    var array = AsArray();
                    writer.Write(array.Count);
                    foreach (var entry in array)
                    {
                        entry.Serialize(type.arrayType, writer);
                    }
                    break;
                case BuiltinType.Type.Map:
                    throw new NotImplementedException();
            }
        }

        public static BuiltinValue Deserialize(BuiltinType type, BinaryReader reader)
        {
            byte[] ReadByteArray()
            {
                var len = reader.ReadUInt32();
                if (len > int.MaxValue)
                {
                    var arrays = new List<byte[]>();
                    long read = 0;
                    while (read < len)
                    {
                        var remaining = len - read;
                        var readResult = reader.ReadBytes(remaining > int.MaxValue ? int.MaxValue : (int)remaining);
                        arrays.Add(readResult);
                        read += readResult.Length;
                    }

                    var result = new byte[len];
                    long pos = 0;
                    foreach (var array in arrays)
                    {
                        Array.Copy(array, 0, result, pos, array.LongLength);
                        pos += array.LongLength;
                    }

                    return result;
                }

                return reader.ReadBytes((int)len);
            }

            switch (type.type)
            {
                case BuiltinType.Type.Bool:
                    return CreateBool(reader.ReadByte() != 0);
                case BuiltinType.Type.I8:
                    return CreateI8(reader.ReadSByte());
                case BuiltinType.Type.U8:
                    return CreateU8(reader.ReadByte());
                case BuiltinType.Type.I16:
                    return CreateI16(reader.ReadInt16());
                case BuiltinType.Type.U16:
                    return CreateU16(reader.ReadUInt16());
                case BuiltinType.Type.I32:
                    return CreateI32(reader.ReadInt32());
                case BuiltinType.Type.U32:
                    return CreateU32(reader.ReadUInt32());
                case BuiltinType.Type.I64:
                    return CreateI64(reader.ReadInt64());
                case BuiltinType.Type.U64:
                    return CreateU64(reader.ReadUInt64());
                case BuiltinType.Type.I128:
                    return CreateI128(reader.ReadBytes(16));
                case BuiltinType.Type.U128:
                    return CreateU128(reader.ReadBytes(16));
                case BuiltinType.Type.F32:
                    return CreateF32(reader.ReadSingle());
                case BuiltinType.Type.F64:
                    return CreateF64(reader.ReadDouble());
                case BuiltinType.Type.String:
                    return CreateString(System.Text.Encoding.UTF8.GetString(ReadByteArray()));
                case BuiltinType.Type.Array:
                    if (type.arrayType.type == AlgebraicType.Type.Builtin &&
                        type.arrayType.builtin.type == BuiltinType.Type.U8)
                    {
                        return CreateBytes(ReadByteArray());
                    }

                    var length = reader.ReadInt32();
                    var arrayResult = new List<AlgebraicValue>();
                    for (var x = 0; x < length; x++)
                    {
                        arrayResult.Add(AlgebraicValue.Deserialize(type.arrayType, reader));
                    }

                    return CreateArray(arrayResult);
                case BuiltinType.Type.Map:
                    {
                        var len = reader.ReadUInt32();
                        var mapResult = new Dictionary<AlgebraicValue, AlgebraicValue>();
                        for (var x = 0; x < len; x++)
                        {
                            var key = AlgebraicValue.Deserialize(type.mapType.keyType, reader);
                            var value = AlgebraicValue.Deserialize(type.mapType.valueType, reader);
                            mapResult.Add(key, value);
                        }

                        return CreateMap(mapResult);
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool Compare(BuiltinType t, BuiltinValue v1, BuiltinValue v2)
        {
            switch (t.type)
            {
                case BuiltinType.Type.Bool:
                    return v1.AsBool() == v2.AsBool();
                case BuiltinType.Type.U8:
                    return v1.AsU8() == v2.AsU8();
                case BuiltinType.Type.I8:
                    return v1.AsI8() == v2.AsI8();
                case BuiltinType.Type.U16:
                    return v1.AsU16() == v2.AsU16();
                case BuiltinType.Type.I16:
                    return v1.AsI16() == v2.AsI16();
                case BuiltinType.Type.U32:
                    return v1.AsU32() == v2.AsU32();
                case BuiltinType.Type.I32:
                    return v1.AsI32() == v2.AsI32();
                case BuiltinType.Type.U64:
                    return v1.AsU64() == v2.AsU64();
                case BuiltinType.Type.I64:
                    return v1.AsI64() == v2.AsI64();
                case BuiltinType.Type.U128:
                case BuiltinType.Type.I128:
                case BuiltinType.Type.F32:
                case BuiltinType.Type.F64:
                case BuiltinType.Type.Map:
                    throw new NotImplementedException();
                case BuiltinType.Type.String:
                    return v1.AsString() == v2.AsString();
                case BuiltinType.Type.Array:
                    if (t.arrayType.type == AlgebraicType.Type.Builtin &&
                        t.arrayType.builtin.type == BuiltinType.Type.U8)
                    {
                        var arr1 = v1.AsBytes();
                        var arr2 = v2.AsBytes();

                        if (arr1.Length != arr2.Length)
                        {
                            return false;
                        }

                        for (var i = 0; i < arr1.Length; i++)
                        {
                            if (arr1[i] != arr2[i])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    var list1 = v1.AsArray();
                    var list2 = v2.AsArray();
                    if (list1.Count != list2.Count)
                    {
                        return false;
                    }

                    for (var i = 0; i < list1.Count; i++)
                    {
                        if (!AlgebraicValue.Compare(t.arrayType, list1[i], list2[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class SumValue
    {
        public byte tag;
        public AlgebraicValue value;

        private SumValue()
        {
        }

        public SumValue(byte tag, AlgebraicValue value)
        {
            this.tag = tag;
            this.value = value;
        }

        public static SumValue Deserialize(SumType type, BinaryReader reader)
        {
            var result = new SumValue();
            result.tag = reader.ReadByte();
            result.value = AlgebraicValue.Deserialize(type.variants[result.tag].algebraicType, reader);
            return result;
        }

        public void Serialize(SumType type, BinaryWriter writer)
        {
            writer.Write(tag);
            value.Serialize(type.variants[tag].algebraicType, writer);
        }

        public static bool Compare(SumType t, SumValue v1, SumValue v2)
        {
            if (v1.tag != v2.tag)
            {
                return false;
            }

            return AlgebraicValue.Compare(t.variants[v1.tag].algebraicType, v1.value, v2.value);
        }
    }

    public class ProductValue
    {
        public List<AlgebraicValue> elements;

        public ProductValue()
        {
            elements = new List<AlgebraicValue>();
        }

        public void Serialize(ProductType type, BinaryWriter writer)
        {
            writer.Write((uint)elements.Count);
            for (var x = 0; x < elements.Count; x++)
            {
                elements[x].Serialize(type.elements[x].algebraicType, writer);
            }
        }

        public static ProductValue Deserialize(ProductType type, BinaryReader reader)
        {
            var result = new ProductValue();
            for (var x = 0; x < type.elements.Count; x++)
            {
                result.elements.Add(AlgebraicValue.Deserialize(type.elements[x].algebraicType, reader));
            }

            return result;
        }

        public static bool Compare(ProductType type, ProductValue v1, ProductValue v2)
        {
            if (v1.elements.Count != v2.elements.Count)
            {
                return false;
            }

            for (var i = 0; i < type.elements.Count; i++)
            {
                if (!AlgebraicValue.Compare(type.elements[i].algebraicType, v1.elements[i], v2.elements[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class AlgebraicValue
    {
        public SumValue sum;
        public ProductValue product;
        public BuiltinValue builtin;

        public bool AsBool() => builtin.AsBool();
        public sbyte AsI8() => builtin.AsI8();
        public byte AsU8() => builtin.AsU8();
        public short AsI16() => builtin.AsI16();
        public ushort AsU16() => builtin.AsU16();
        public int AsI32() => builtin.AsI32();
        public uint AsU32() => builtin.AsU32();
        public long AsI64() => builtin.AsI64();
        public ulong AsU64() => builtin.AsU64();
        public byte[] AsI128() => builtin.AsI128();
        public byte[] AsU128() => builtin.AsU128();
        public float AsF32() => builtin.AsF32();
        public double AsF64() => builtin.AsF64();
        public string AsString() => builtin.AsString();
        public byte[] AsBytes() => builtin.AsBytes();
        public List<AlgebraicValue> AsArray() => builtin.AsArray();
        public Dictionary<AlgebraicValue, AlgebraicValue> AsMap() => builtin.AsMap();
        public static AlgebraicValue CreateBool(bool v) => new AlgebraicValue { builtin = BuiltinValue.CreateBool(v) };
        public static AlgebraicValue CreateI8(sbyte v) => new AlgebraicValue { builtin = BuiltinValue.CreateI8(v) };
        public static AlgebraicValue CreateU8(byte v) => new AlgebraicValue { builtin = BuiltinValue.CreateU8(v) };
        public static AlgebraicValue CreateI16(short v) => new AlgebraicValue { builtin = BuiltinValue.CreateI16(v) };
        public static AlgebraicValue CreateU16(ushort v) => new AlgebraicValue { builtin = BuiltinValue.CreateU16(v) };
        public static AlgebraicValue CreateI32(int v) => new AlgebraicValue { builtin = BuiltinValue.CreateI32(v) };
        public static AlgebraicValue CreateU32(uint v) => new AlgebraicValue { builtin = BuiltinValue.CreateU32(v) };
        public static AlgebraicValue CreateI64(long v) => new AlgebraicValue { builtin = BuiltinValue.CreateI64(v) };
        public static AlgebraicValue CreateU64(ulong v) => new AlgebraicValue { builtin = BuiltinValue.CreateU64(v) };
        public static AlgebraicValue CreateI128(byte[] v) => new AlgebraicValue { builtin = BuiltinValue.CreateI128(v) };
        public static AlgebraicValue CreateU128(byte[] v) => new AlgebraicValue { builtin = BuiltinValue.CreateU128(v) };
        public static AlgebraicValue CreateF32(float v) => new AlgebraicValue { builtin = BuiltinValue.CreateF32(v) };
        public static AlgebraicValue CreateF64(double v) => new AlgebraicValue { builtin = BuiltinValue.CreateF64(v) };
        public static AlgebraicValue CreateString(string v) => new AlgebraicValue { builtin = BuiltinValue.CreateString(v) };
        public static AlgebraicValue CreateBytes(byte[] v) => new AlgebraicValue { builtin = BuiltinValue.CreateBytes(v) };
        public static AlgebraicValue CreateArray(List<AlgebraicValue> v) => new AlgebraicValue { builtin = BuiltinValue.CreateArray(v) };
        public static AlgebraicValue CreateMap(Dictionary<AlgebraicValue, AlgebraicValue> v) => new AlgebraicValue { builtin = BuiltinValue.CreateMap(v) };

        public BuiltinValue AsBuiltInValue() => builtin;
        public ProductValue AsProductValue() => product;
        public SumValue AsSumValue() => sum;

        public static AlgebraicValue Create(BuiltinValue value) => new AlgebraicValue { builtin = value };
        public static AlgebraicValue Create(ProductValue value) => new AlgebraicValue { product = value };
        public static AlgebraicValue Create(SumValue value) => new AlgebraicValue { sum = value };

        public static bool Compare(AlgebraicType t, AlgebraicValue v1, AlgebraicValue v2)
        {
            switch (t.type)
            {
                case AlgebraicType.Type.Builtin:
                    return BuiltinValue.Compare(t.builtin, v1.builtin, v2.builtin);
                case AlgebraicType.Type.Sum:
                    return SumValue.Compare(t.sum, v1.sum, v2.sum);
                case AlgebraicType.Type.Product:
                    return ProductValue.Compare(t.product, v1.product, v2.product);
                case AlgebraicType.Type.TypeRef:
                case AlgebraicType.Type.None:
                default:
                    throw new NotImplementedException();
            }
        }

        public static AlgebraicValue Deserialize(AlgebraicType type, BinaryReader reader)
        {
            switch (type.type)
            {
                case AlgebraicType.Type.Builtin:
                    return Create(BuiltinValue.Deserialize(type.builtin, reader));
                case AlgebraicType.Type.Product:
                    return Create(ProductValue.Deserialize(type.product, reader));
                case AlgebraicType.Type.Sum:
                    return Create(SumValue.Deserialize(type.sum, reader));
                default:
                    throw new NotImplementedException();
            }
        }

        public void Serialize(AlgebraicType type, BinaryWriter writer)
        {
            switch (type.type)
            {
                case AlgebraicType.Type.Builtin:
                    builtin.Serialize(type.builtin, writer);
                    break;
                case AlgebraicType.Type.Product:
                    product.Serialize(type.product, writer);
                    break;
                case AlgebraicType.Type.Sum:
                    sum.Serialize(type.sum, writer);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
