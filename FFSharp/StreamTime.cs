using System;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Represents a <see cref="Stream"/>-based time value.
    /// </summary>
    [PublicAPI]
    public struct StreamTime :
        IEquatable<StreamTime>,
        IComparable<StreamTime>,
        IComparable<double>,
        IComparable<float>,
        IComparable
    {
        /// <summary>
        /// The internal time base <see cref="Rational"/> used by FFmpeg.
        /// </summary>
        public static readonly Rational InternalBase = new Rational(1, Unsafe.ffmpeg.AV_TIME_BASE);

        /// <summary>
        /// Get a relative <see cref="StreamTime"/>.
        /// </summary>
        /// <param name="ABase">The time base <see cref="Rational"/>.</param>
        /// <param name="ASeconds">The value in absolute seconds.</param>
        /// <returns>The <see cref="StreamTime"/> with <paramref name="ABase"/>.</returns>
        public static StreamTime Of(Rational ABase, double ASeconds)
        {
            return new StreamTime(ABase, (long)Math.Floor(ASeconds / InternalBase.AsDouble));
        }

        /// <summary>
        /// The time base <see cref="Rational"/>.
        /// </summary>
        public Rational Base { get; }
        /// <summary>
        /// The value.
        /// </summary>
        public long Value { get; }

        /// <summary>
        /// Initialize a <see cref="StreamTime"/>.
        /// </summary>
        /// <param name="ABase">The time base <see cref="Rational"/>.</param>
        /// <param name="AValue">The value.</param>
        public StreamTime(Rational ABase, long AValue)
        {
            Base = ABase;
            Value = AValue;
        }

        #region IEquatable<StreamTime>
        /// <inheritdoc />
        public bool Equals(StreamTime AStreamTime)
        {
            var lhs = Base;
            var rhs = AStreamTime.Base;
            var fac = Rational.Extend(ref lhs, ref rhs);

            return CompareTo(AStreamTime) == 0;
        }
        #endregion

        #region IComparable<StreamTime>
        /// <inheritdoc />
        public int CompareTo(StreamTime AStreamTime)
        {
            var lhs = Base;
            var rhs = AStreamTime.Base;
            var fac = Rational.Extend(ref lhs, ref rhs);

            return (Value * fac.Item1).CompareTo(AStreamTime.Value * fac.Item2);
        }
        #endregion

        #region IComparable<double>
        /// <inheritdoc />
        public int CompareTo(double ADouble)
        {
            return Seconds.CompareTo(ADouble);
        }
        #endregion

        #region IComparable<float>
        /// <inheritdoc />
        public int CompareTo(float AFloat)
        {
            return Seconds.CompareTo(AFloat);
        }
        #endregion

        #region IComparable
        /// <inheritdoc />
        public int CompareTo(object AOther)
        {
            switch (AOther)
            {
            case StreamTime streamTime:
                return CompareTo(streamTime);

            case double dbl:
                return CompareTo(dbl);

            case float flt:
                return CompareTo(flt);

            default:
                throw new ArgumentException("Invalid type.");
            }
        }
        #endregion

        #region System.Object overrides
        /// <inheritdoc />
        public override bool Equals(object AObject)
        {
            switch (AObject)
            {
                case StreamTime streamTime:
                    return Equals(streamTime);

                default:
                    return false;
            }
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Base.GetHashCode() * 7 + Value.GetHashCode();
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}T (~ {Seconds:F3}s)";
        }
        #endregion

        /// <summary>
        /// Get the value in absolute seconds.
        /// </summary>
        public double Seconds => Value * Base.AsDouble;

        #region Operator overloads
        // IM NOT DOCUMENTING THIS MESS.

        public static implicit operator double(StreamTime ATime) => ATime.Seconds;
        public static implicit operator float(StreamTime ATime) => (float)ATime.Seconds;

        public static bool operator ==(StreamTime ALhs, StreamTime ARhs) => ALhs.Equals(ARhs);
        public static bool operator !=(StreamTime ALhs, StreamTime ARhs) => !ALhs.Equals(ARhs);

        public static bool operator >(StreamTime ALhs, StreamTime ARhs)
            => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(StreamTime ALhs, StreamTime ARhs)
            => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(StreamTime ALhs, StreamTime ARhs)
            => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(StreamTime ALhs, StreamTime ARhs)
            => ALhs.CompareTo(ARhs) < 0;

        public static bool operator >(StreamTime ALhs, double ARhs) => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(StreamTime ALhs, double ARhs) => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(StreamTime ALhs, double ARhs) => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(StreamTime ALhs, double ARhs) => ALhs.CompareTo(ARhs) < 0;

        public static bool operator >(StreamTime ALhs, float ARhs) => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(StreamTime ALhs, float ARhs) => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(StreamTime ALhs, float ARhs) => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(StreamTime ALhs, float ARhs) => ALhs.CompareTo(ARhs) < 0;
        #endregion
    }
}
