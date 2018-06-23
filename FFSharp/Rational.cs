using System;

using JetBrains.Annotations;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp
{
    /// <summary>
    /// Represents a rational number made from two <see cref="int"/>s.
    /// </summary>
    [PublicAPI]
    public struct Rational :
        IEquatable<Rational>,
        IComparable<Rational>,
        IComparable<double>,
        IComparable<float>,
        IComparable
    {
        [Pure]
        static int Gcd(int A, int B)
        {
            if (A == 0) return Math.Abs(B);

            while (B != 0)
            {
                var temp = A % B;
                A = B;
                B = temp;
            }

            return Math.Abs(A);
        }
        [Pure]
        static int Lcm(int A, int B)
        {
            var gcd = Gcd(A, B);

            if (gcd == 0) return 0;
            return A * B / gcd;
        }

        /// <summary>
        /// Reduce a <see cref="Rational"/> as far as possible.
        /// </summary>
        /// <param name="ARational">The input <see cref="Rational"/>.</param>
        public static void Reduce(ref Rational ARational)
        {
            var gcd = Gcd(ARational.Value.num, ARational.Value.den);

            if (gcd > 0)
            {
                ARational = new Rational(ARational.Value.num / gcd, ARational.Value.den / gcd);
            }
        }
        /// <summary>
        /// Extend a <see cref="Rational"/>.
        /// </summary>
        /// <param name="ARational">The <see cref="Rational"/>.</param>
        /// <param name="AFactor">The factor to extend with.</param>
        public static void Extend(ref Rational ARational, int AFactor)
        {
            ARational = new Rational(ARational.Value.num * AFactor, ARational.Value.den * AFactor);
        }
        /// <summary>
        /// Extends two <see cref="Rational"/>s. so that their denominator matches.
        /// </summary>
        /// <param name="ALhs">The left hand side.</param>
        /// <param name="ARhs">The right hand side.</param>
        /// <returns><see cref="Tuple{T1, T2}"/> of the factors for each side.</returns>
        public static Tuple<int, int> Extend(ref Rational ALhs, ref Rational ARhs)
        {
            var lcm = Lcm(ALhs.Value.den, ARhs.Value.den);
            var lf = lcm / ALhs.Value.den;
            var rf = lcm / ARhs.Value.den;

            Extend(ref ALhs, lf);
            Extend(ref ARhs, rf);

            return Tuple.Create(lf, rf);
        }
        /// <summary>
        /// Approximate a <see cref="double"/> using a <see cref="Rational"/>.
        /// </summary>
        /// <param name="ADouble">The floating point value.</param>
        /// <param name="AMaxDen">The maximum denominator.</param>
        /// <returns>The approximated <see cref="Rational"/>.</returns>
        public static Rational Approximate(double ADouble, int AMaxDen = 1000000)
        {
            int a = 0, b = 1;
            int c = 1, d = 0;

            while (b <= AMaxDen && d <= AMaxDen)
            {
                // Calculate the median and compare with it.
                double med = (double)(a+c) / (double)(b+d);
                int cmp = ADouble.CompareTo(med);

                if (cmp == 0)
                {
                    if (b + d <= AMaxDen)
                    {
                        return new Rational(a + c, b + d);
                    }

                    break;
                }

                if (cmp > 0)
                {
                    // Continue in the right branch.
                    a += c;
                    b += d;
                }

                // Continue in the left branch.
                c += a;
                d += b;
            }

            // Return the more precise one.
            if (d > b)
            {
                return new Rational(c, d);
            }

            return new Rational(a, b);
        }

        /// <summary>
        /// Underlying <see cref="Unsafe.AVRational"/>.
        /// </summary>
        internal Unsafe.AVRational Value;

        /// <summary>
        /// Initialize a <see cref="Rational"/>.
        /// </summary>
        /// <param name="AValue">The <see cref="Unsafe.AVRational"/>.</param>
        internal Rational(Unsafe.AVRational AValue)
        {
            Value = AValue;

            Reduce(ref this);
        }
        /// <summary>
        /// Initialize a <see cref="Rational"/>.
        /// </summary>
        /// <param name="ANumerator">The numerator.</param>
        /// <param name="ADenominator">The denominator.</param>
        public Rational(int ANumerator, int ADenominator)
        {
            Value.num = ANumerator;
            Value.den = ADenominator;

            Reduce(ref this);
        }

        #region IEquatable<Rational>
        /// <inheritdoc />
        public bool Equals(Rational ARational)
        {
            return Value.num == ARational.Value.num
                   && Value.den == ARational.Value.den;
        }
        #endregion

        #region IComparable<Rational>
        /// <inheritdoc />
        public int CompareTo(Rational ARational)
        {
            var lhs = this;
            var rhs = ARational;
            Extend(ref lhs, ref rhs);

            return lhs.Numerator.CompareTo(rhs.Numerator);
        }
        #endregion

        #region IComparable<double>
        /// <inheritdoc />
        public int CompareTo(double ADouble)
        {
            return AsDouble.CompareTo(ADouble);
        }
        #endregion

        #region IComparable<float>
        /// <inheritdoc />
        public int CompareTo(float AFloat)
        {
            return AsFloat.CompareTo(AFloat);
        }
        #endregion

        #region IComparable
        /// <inheritdoc />
        public int CompareTo(object AOther)
        {
            switch (AOther)
            {
                case Rational rational:
                    return CompareTo(rational);

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
        public override bool Equals(object AOther)
        {
            switch (AOther)
            {
                case Rational rational:
                    return Equals(rational);

                default:
                    return false;
            }
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Numerator.GetHashCode() * 7 + Denominator.GetHashCode();
        }
        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Numerator}/{Denominator})";
        }
        #endregion

        /// <summary>
        /// Get the numerator.
        /// </summary>
        public int Numerator => Value.num;
        /// <summary>
        /// Get the denominator.
        /// </summary>
        public int Denominator => Value.den;

        /// <summary>
        /// Get the value as a <see cref="double"/>.
        /// </summary>
        public double AsDouble => (double)Numerator / (double)Denominator;
        /// <summary>
        /// Get the value as a <see cref="float"/>.
        /// </summary>
        public float AsFloat => (float)Numerator / (float)Denominator;

        #region Operator overloads
        // IM NOT DOCUMENTING THIS MESS.

        public static explicit operator double(Rational ARational) => ARational.AsDouble;
        public static explicit operator Rational(double ADouble) => Approximate(ADouble);
        public static explicit operator float(Rational ARational) => ARational.AsFloat;
        public static explicit operator Rational(float AFloat) => Approximate(AFloat);

        public static implicit operator Rational(Unsafe.AVRational ARat) => new Rational(ARat);
        public static implicit operator Unsafe.AVRational(Rational ARat) => ARat.Value;

        public static bool operator ==(Rational ALhs, Rational ARhs) => ALhs.Equals(ARhs);
        public static bool operator !=(Rational ALhs, Rational ARhs) => !ALhs.Equals(ARhs);

        public static bool operator >(Rational ALhs, Rational ARhs) => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(Rational ALhs, Rational ARhs) => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(Rational ALhs, Rational ARhs) => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(Rational ALhs, Rational ARhs) => ALhs.CompareTo(ARhs) < 0;

        public static bool operator >(Rational ALhs, double ARhs) => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(Rational ALhs, double ARhs) => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(Rational ALhs, double ARhs) => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(Rational ALhs, double ARhs) => ALhs.CompareTo(ARhs) < 0;

        public static bool operator >(Rational ALhs, float ARhs) => ALhs.CompareTo(ARhs) > 0;
        public static bool operator >=(Rational ALhs, float ARhs) => ALhs.CompareTo(ARhs) >= 0;
        public static bool operator <=(Rational ALhs, float ARhs) => ALhs.CompareTo(ARhs) <= 0;
        public static bool operator <(Rational ALhs, float ARhs) => ALhs.CompareTo(ARhs) < 0;
        #endregion
    }
}
