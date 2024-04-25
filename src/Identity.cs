using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpacetimeDB.SATS;

namespace SpacetimeDB
{
    public struct Identity : IEquatable<Identity>
    {
        private byte[] bytes;

        public const int SIZE = 32;

        public byte[] Bytes => bytes;

        public static AlgebraicType GetAlgebraicType()
        {
            return new AlgebraicType
            {
                type = AlgebraicType.Type.Builtin,
                builtin = new BuiltinType
                {
                    type = BuiltinType.Type.Array,
                    arrayType = new AlgebraicType
                    {
                        type = AlgebraicType.Type.Builtin,
                        builtin = new BuiltinType
                        {
                            type = BuiltinType.Type.U8
                        }
                    }
                }
            };
        }

        public static explicit operator Identity(AlgebraicValue v) => new Identity
        {
            bytes = v.AsBytes(),
        };

        public static Identity From(byte[] bytes)
        {
            // TODO: should we validate length here?
            return new Identity
            {
                bytes = bytes,
            };
        }

        public bool Equals(Identity other) => ByteArrayComparer.Instance.Equals(bytes, other.bytes);

        public override bool Equals(object o) => o is Identity other && Equals(other);

        public static bool operator ==(Identity a, Identity b) => a.Equals(b);
        public static bool operator !=(Identity a, Identity b) => !a.Equals(b);

        public override int GetHashCode() => ByteArrayComparer.Instance.GetHashCode(bytes);

        public override string ToString() => ByteArrayComparer.ToHexString(bytes);
    }
}
