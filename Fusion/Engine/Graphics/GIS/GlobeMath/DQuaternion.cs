﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Fusion.Engine.Graphics.GIS.GlobeMath
{
    /// <summary>
    /// Represents a four dimensional mathematical DQuaternion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DQuaternion : IEquatable<DQuaternion>, IFormattable
    {
        /// <summary>
        /// The size of the <see cref="DQuaternion"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(DQuaternion));

        /// <summary>
        /// A <see cref="DQuaternion"/> with all of its components set to zero.
        /// </summary>
        public static readonly DQuaternion Zero = new DQuaternion();

        /// <summary>
        /// A <see cref="DQuaternion"/> with all of its components set to one.
        /// </summary>
        public static readonly DQuaternion One = new DQuaternion(1.0f, 1.0f, 1.0f, 1.0f);

        /// <summary>
        /// The identity <see cref="DQuaternion"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly DQuaternion Identity = new DQuaternion(0.0f, 0.0f, 0.0f, 1.0f);

        /// <summary>
        /// The X component of the DQuaternion.
        /// </summary>
        public double X;

        /// <summary>
        /// The Y component of the DQuaternion.
        /// </summary>
        public double Y;

        /// <summary>
        /// The Z component of the DQuaternion.
        /// </summary>
        public double Z;

        /// <summary>
        /// The W component of the DQuaternion.
        /// </summary>
        public double W;

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public DQuaternion(double value)
        {
            X = value;
            Y = value;
            Z = value;
            W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the components.</param>
        public DQuaternion(DVector4 value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
        /// <param name="w">Initial value for the W component of the DQuaternion.</param>
        public DQuaternion(DVector3 value, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the DQuaternion.</param>
        /// <param name="w">Initial value for the W component of the DQuaternion.</param>
        public DQuaternion(DVector2 value, double z, double w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="x">Initial value for the X component of the DQuaternion.</param>
        /// <param name="y">Initial value for the Y component of the DQuaternion.</param>
        /// <param name="z">Initial value for the Z component of the DQuaternion.</param>
        /// <param name="w">Initial value for the W component of the DQuaternion.</param>
        public DQuaternion(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DQuaternion"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z, and W components of the DQuaternion. This must be an array with four elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than four elements.</exception>
        public DQuaternion(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for DQuaternion.");

            X = values[0];
            Y = values[1];
            Z = values[2];
            W = values[3];
        }

        /// <summary>
        /// Gets a value indicating whether this instance is equivalent to the identity DQuaternion.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity DQuaternion; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity
        {
            get { return this.Equals(Identity); }
        }

        /// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get { return DMathUtil.IsOne((X * X) + (Y * Y) + (Z * Z) + (W * W)); }
        }

        /// <summary>
        /// Gets the angle of the DQuaternion.
        /// </summary>
        /// <value>The DQuaternion's angle.</value>
        public double Angle
        {
            get
            {
                double length = (X * X) + (Y * Y) + (Z * Z);
                if (DMathUtil.IsZero(length))
                    return 0.0f;

                return (double)(2.0 * Math.Acos(DMathUtil.Clamp(W, -1f, 1f)));
            }
        }

        /// <summary>
        /// Gets the axis components of the DQuaternion.
        /// </summary>
        /// <value>The axis components of the DQuaternion.</value>
        public DVector3 Axis
        {
            get
            {
                double length = (X * X) + (Y * Y) + (Z * Z);
                if (DMathUtil.IsZero(length))
                    return DVector3.UnitX;

                double inv = 1.0f / (double)Math.Sqrt(length);
                return new DVector3(X * inv, Y * inv, Z * inv);
            }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z, or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component, 2 for the Z component, and 3 for the W component.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                }

                throw new ArgumentOutOfRangeException("index", "Indices for DQuaternion run from 0 to 3, inclusive.");
            }

            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for DQuaternion run from 0 to 3, inclusive.");
                }
            }
        }

        /// <summary>
        /// Conjugates the DQuaternion.
        /// </summary>
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }

        /// <summary>
        /// Conjugates and renormalizes the DQuaternion.
        /// </summary>
        public void Invert()
        {
            double lengthSq = LengthSquared();
            if (!DMathUtil.IsZero(lengthSq))
            {
                lengthSq = 1.0f / lengthSq;

                X = -X * lengthSq;
                Y = -Y * lengthSq;
                Z = -Z * lengthSq;
                W = W * lengthSq;
            }
        }

        /// <summary>
        /// Calculates the length of the DQuaternion.
        /// </summary>
        /// <returns>The length of the DQuaternion.</returns>
        /// <remarks>
        /// <see cref="DQuaternion.LengthSquared"/> may be preferred when only the relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double Length()
        {
            return (double)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        /// <summary>
        /// Calculates the squared length of the DQuaternion.
        /// </summary>
        /// <returns>The squared length of the DQuaternion.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="DQuaternion.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public double LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Converts the DQuaternion into a unit DQuaternion.
        /// </summary>
        public void Normalize()
        {
            double length = Length();
            if (!DMathUtil.IsZero(length))
            {
                double inverse = 1.0f / length;
                X *= inverse;
                Y *= inverse;
                Z *= inverse;
                W *= inverse;
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the DQuaternion.
        /// </summary>
        /// <returns>A four-element array containing the components of the DQuaternion.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z, W };
        }

        /// <summary>
        /// Adds two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to add.</param>
        /// <param name="right">The second DQuaternion to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two DQuaternions.</param>
        public static void Add(ref DQuaternion left, ref DQuaternion right, out DQuaternion result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
            result.Z = left.Z + right.Z;
            result.W = left.W + right.W;
        }

        /// <summary>
        /// Adds two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to add.</param>
        /// <param name="right">The second DQuaternion to add.</param>
        /// <returns>The sum of the two DQuaternions.</returns>
        public static DQuaternion Add(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Subtracts two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to subtract.</param>
        /// <param name="right">The second DQuaternion to subtract.</param>
        /// <param name="result">When the method completes, contains the difference of the two DQuaternions.</param>
        public static void Subtract(ref DQuaternion left, ref DQuaternion right, out DQuaternion result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
            result.Z = left.Z - right.Z;
            result.W = left.W - right.W;
        }

        /// <summary>
        /// Subtracts two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to subtract.</param>
        /// <param name="right">The second DQuaternion to subtract.</param>
        /// <returns>The difference of the two DQuaternions.</returns>
        public static DQuaternion Subtract(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Scales a DQuaternion by the given value.
        /// </summary>
        /// <param name="value">The DQuaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the DQuaternion.</param>
        /// <param name="result">When the method completes, contains the scaled DQuaternion.</param>
        public static void Multiply(ref DQuaternion value, double scale, out DQuaternion result)
        {
            result.X = value.X * scale;
            result.Y = value.Y * scale;
            result.Z = value.Z * scale;
            result.W = value.W * scale;
        }

        /// <summary>
        /// Scales a DQuaternion by the given value.
        /// </summary>
        /// <param name="value">The DQuaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the DQuaternion.</param>
        /// <returns>The scaled DQuaternion.</returns>
        public static DQuaternion Multiply(DQuaternion value, double scale)
        {
            DQuaternion result;
            Multiply(ref value, scale, out result);
            return result;
        }

        /// <summary>
        /// Multiplies a DQuaternion by another.
        /// </summary>
        /// <param name="left">The first DQuaternion to multiply.</param>
        /// <param name="right">The second DQuaternion to multiply.</param>
        /// <param name="result">When the method completes, contains the multiplied DQuaternion.</param>
        public static void Multiply(ref DQuaternion left, ref DQuaternion right, out DQuaternion result)
        {
            double lx = left.X;
            double ly = left.Y;
            double lz = left.Z;
            double lw = left.W;
            double rx = right.X;
            double ry = right.Y;
            double rz = right.Z;
            double rw = right.W;
            double a = (ly * rz - lz * ry);
            double b = (lz * rx - lx * rz);
            double c = (lx * ry - ly * rx);
            double d = (lx * rx + ly * ry + lz * rz);
            result.X = (lx * rw + rx * lw) + a;
            result.Y = (ly * rw + ry * lw) + b;
            result.Z = (lz * rw + rz * lw) + c;
            result.W = lw * rw - d;
        }

        /// <summary>
        /// Multiplies a DQuaternion by another.
        /// </summary>
        /// <param name="left">The first DQuaternion to multiply.</param>
        /// <param name="right">The second DQuaternion to multiply.</param>
        /// <returns>The multiplied DQuaternion.</returns>
        public static DQuaternion Multiply(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Reverses the direction of a given DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to negate.</param>
        /// <param name="result">When the method completes, contains a DQuaternion facing in the opposite direction.</param>
        public static void Negate(ref DQuaternion value, out DQuaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = -value.W;
        }

        /// <summary>
        /// Reverses the direction of a given DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to negate.</param>
        /// <returns>A DQuaternion facing in the opposite direction.</returns>
        public static DQuaternion Negate(DQuaternion value)
        {
            DQuaternion result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Returns a <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        /// <param name="result">When the method completes, contains a new <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of the specified point.</param>
        public static void Barycentric(ref DQuaternion value1, ref DQuaternion value2, ref DQuaternion value3, double amount1, double amount2, out DQuaternion result)
        {
            DQuaternion start, end;
            Slerp(ref value1, ref value2, amount1 + amount2, out start);
            Slerp(ref value1, ref value3, amount1 + amount2, out end);
            Slerp(ref start, ref end, amount2 / (amount1 + amount2), out result);
        }

        /// <summary>
        /// Returns a <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        /// <returns>A new <see cref="DQuaternion"/> containing the 4D Cartesian coordinates of the specified point.</returns>
        public static DQuaternion Barycentric(DQuaternion value1, DQuaternion value2, DQuaternion value3, double amount1, double amount2)
        {
            DQuaternion result;
            Barycentric(ref value1, ref value2, ref value3, amount1, amount2, out result);
            return result;
        }

        /// <summary>
        /// Conjugates a DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to conjugate.</param>
        /// <param name="result">When the method completes, contains the conjugated DQuaternion.</param>
        public static void Conjugate(ref DQuaternion value, out DQuaternion result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
        }

        /// <summary>
        /// Conjugates a DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to conjugate.</param>
        /// <returns>The conjugated DQuaternion.</returns>
        public static DQuaternion Conjugate(DQuaternion value)
        {
            DQuaternion result;
            Conjugate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Calculates the dot product of two DQuaternions.
        /// </summary>
        /// <param name="left">First source DQuaternion.</param>
        /// <param name="right">Second source DQuaternion.</param>
        /// <param name="result">When the method completes, contains the dot product of the two DQuaternions.</param>
        public static void Dot(ref DQuaternion left, ref DQuaternion right, out double result)
        {
            result = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Calculates the dot product of two DQuaternions.
        /// </summary>
        /// <param name="left">First source DQuaternion.</param>
        /// <param name="right">Second source DQuaternion.</param>
        /// <returns>The dot product of the two DQuaternions.</returns>
        public static double Dot(DQuaternion left, DQuaternion right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Exponentiates a DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to exponentiate.</param>
        /// <param name="result">When the method completes, contains the exponentiated DQuaternion.</param>
        public static void Exponential(ref DQuaternion value, out DQuaternion result)
        {
            double angle = (double)Math.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
            double sin = (double)Math.Sin(angle);

            if (!DMathUtil.IsZero(sin))
            {
                double coeff = sin / angle;
                result.X = coeff * value.X;
                result.Y = coeff * value.Y;
                result.Z = coeff * value.Z;
            }
            else
            {
                result = value;
            }

            result.W = (double)Math.Cos(angle);
        }

        /// <summary>
        /// Exponentiates a DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to exponentiate.</param>
        /// <returns>The exponentiated DQuaternion.</returns>
        public static DQuaternion Exponential(DQuaternion value)
        {
            DQuaternion result;
            Exponential(ref value, out result);
            return result;
        }

        /// <summary>
        /// Conjugates and renormalizes the DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to conjugate and renormalize.</param>
        /// <param name="result">When the method completes, contains the conjugated and renormalized DQuaternion.</param>
        public static void Invert(ref DQuaternion value, out DQuaternion result)
        {
            result = value;
            result.Invert();
        }

        /// <summary>
        /// Conjugates and renormalizes the DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to conjugate and renormalize.</param>
        /// <returns>The conjugated and renormalized DQuaternion.</returns>
        public static DQuaternion Invert(DQuaternion value)
        {
            DQuaternion result;
            Invert(ref value, out result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two DQuaternions.
        /// </summary>
        /// <param name="start">Start DQuaternion.</param>
        /// <param name="end">End DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two DQuaternions.</param>
        /// <remarks>
        /// This method performs the linear interpolation based on the following formula.
        /// <code>start + (end - start) * amount</code>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref DQuaternion start, ref DQuaternion end, double amount, out DQuaternion result)
        {
            double inverse = 1.0f - amount;

            if (Dot(start, end) >= 0.0f)
            {
                result.X = (inverse * start.X) + (amount * end.X);
                result.Y = (inverse * start.Y) + (amount * end.Y);
                result.Z = (inverse * start.Z) + (amount * end.Z);
                result.W = (inverse * start.W) + (amount * end.W);
            }
            else
            {
                result.X = (inverse * start.X) - (amount * end.X);
                result.Y = (inverse * start.Y) - (amount * end.Y);
                result.Z = (inverse * start.Z) - (amount * end.Z);
                result.W = (inverse * start.W) - (amount * end.W);
            }

            result.Normalize();
        }

        /// <summary>
        /// Performs a linear interpolation between two DQuaternion.
        /// </summary>
        /// <param name="start">Start DQuaternion.</param>
        /// <param name="end">End DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two DQuaternions.</returns>
        /// <remarks>
        /// This method performs the linear interpolation based on the following formula.
        /// <code>start + (end - start) * amount</code>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static DQuaternion Lerp(DQuaternion start, DQuaternion end, double amount)
        {
            DQuaternion result;
            Lerp(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Calculates the natural logarithm of the specified DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion whose logarithm will be calculated.</param>
        /// <param name="result">When the method completes, contains the natural logarithm of the DQuaternion.</param>
        public static void Logarithm(ref DQuaternion value, out DQuaternion result)
        {
            if (Math.Abs(value.W) < 1.0)
            {
                double angle = (double)Math.Acos(value.W);
                double sin = (double)Math.Sin(angle);

                if (!DMathUtil.IsZero(sin))
                {
                    double coeff = angle / sin;
                    result.X = value.X * coeff;
                    result.Y = value.Y * coeff;
                    result.Z = value.Z * coeff;
                }
                else
                {
                    result = value;
                }
            }
            else
            {
                result = value;
            }

            result.W = 0.0f;
        }

        /// <summary>
        /// Calculates the natural logarithm of the specified DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion whose logarithm will be calculated.</param>
        /// <returns>The natural logarithm of the DQuaternion.</returns>
        public static DQuaternion Logarithm(DQuaternion value)
        {
            DQuaternion result;
            Logarithm(ref value, out result);
            return result;
        }

        /// <summary>
        /// Converts the DQuaternion into a unit DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to normalize.</param>
        /// <param name="result">When the method completes, contains the normalized DQuaternion.</param>
        public static void Normalize(ref DQuaternion value, out DQuaternion result)
        {
            DQuaternion temp = value;
            result = temp;
            result.Normalize();
        }

        /// <summary>
        /// Converts the DQuaternion into a unit DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to normalize.</param>
        /// <returns>The normalized DQuaternion.</returns>
        public static DQuaternion Normalize(DQuaternion value)
        {
            value.Normalize();
            return value;
        }

        /// <summary>
        /// Creates a DQuaternion given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        /// <param name="result">When the method completes, contains the newly created DQuaternion.</param>
        public static void RotationAxis(ref DVector3 axis, double angle, out DQuaternion result)
        {
            DVector3 normalized;
            DVector3.Normalize(ref axis, out normalized);

            double half = angle * 0.5f;
            double sin = (double)Math.Sin(half);
            double cos = (double)Math.Cos(half);

            result.X = normalized.X * sin;
            result.Y = normalized.Y * sin;
            result.Z = normalized.Z * sin;
            result.W = cos;
        }

        /// <summary>
        /// Creates a DQuaternion given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        /// <returns>The newly created DQuaternion.</returns>
        public static DQuaternion RotationAxis(DVector3 axis, double angle)
        {
            DQuaternion result;
            RotationAxis(ref axis, angle, out result);
            return result;
        }

        /// <summary>
        /// Creates a DQuaternion given a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <param name="result">When the method completes, contains the newly created DQuaternion.</param>
        public static void RotationMatrix(ref DMatrix matrix, out DQuaternion result)
        {
            double sqrt;
            double half;
            double scale = matrix.M11 + matrix.M22 + matrix.M33;

            if (scale > 0.0f)
            {
                sqrt = (double)Math.Sqrt(scale + 1.0f);
                result.W = sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = 0.5f * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5f * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;
            }
            else
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5f / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5f * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }
        }

        /// <summary>
        /// Creates a DQuaternion given a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <param name="result">When the method completes, contains the newly created DQuaternion.</param>
        public static void RotationMatrix(ref DMatrix3x3 matrix, out DQuaternion result)
        {
            double sqrt;
            double half;
            double scale = matrix.M11 + matrix.M22 + matrix.M33;

            if (scale > 0.0f)
            {
                sqrt = (double)Math.Sqrt(scale + 1.0f);
                result.W = sqrt * 0.5f;
                sqrt = 0.5f / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = 0.5f * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5f * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;
            }
            else
            {
                sqrt = (double)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5f / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5f * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }
        }

        /// <summary>
        /// Creates a left-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at DQuaternion.</param>
        public static void LookAtLH(ref DVector3 eye, ref DVector3 target, ref DVector3 up, out DQuaternion result)
        {
            DMatrix3x3 matrix;
            DMatrix3x3.LookAtLH(ref eye, ref target, ref up, out matrix);
            RotationMatrix(ref matrix, out result);
        }

        /// <summary>
        /// Creates a left-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at DQuaternion.</returns>
        public static DQuaternion LookAtLH(DVector3 eye, DVector3 target, DVector3 up)
        {
            DQuaternion result;
            LookAtLH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at DQuaternion.</param>
        public static void RotationLookAtLH(ref DVector3 forward, ref DVector3 up, out DQuaternion result)
        {
            DVector3 eye = DVector3.Zero;
            DQuaternion.LookAtLH(ref eye, ref forward, ref up, out result);
        }

        /// <summary>
        /// Creates a left-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at DQuaternion.</returns>
        public static DQuaternion RotationLookAtLH(DVector3 forward, DVector3 up)
        {
            DQuaternion result;
            RotationLookAtLH(ref forward, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at DQuaternion.</param>
        public static void LookAtRH(ref DVector3 eye, ref DVector3 target, ref DVector3 up, out DQuaternion result)
        {
            DMatrix3x3 matrix;
            DMatrix3x3.LookAtRH(ref eye, ref target, ref up, out matrix);
            RotationMatrix(ref matrix, out result);
        }

        /// <summary>
        /// Creates a right-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at DQuaternion.</returns>
        public static DQuaternion LookAtRH(DVector3 eye, DVector3 target, DVector3 up)
        {
            DQuaternion result;
            LookAtRH(ref eye, ref target, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at DQuaternion.</param>
        public static void RotationLookAtRH(ref DVector3 forward, ref DVector3 up, out DQuaternion result)
        {
            DVector3 eye = DVector3.Zero;
            DQuaternion.LookAtRH(ref eye, ref forward, ref up, out result);
        }

        /// <summary>
        /// Creates a right-handed, look-at DQuaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at DQuaternion.</returns>
        public static DQuaternion RotationLookAtRH(DVector3 forward, DVector3 up)
        {
            DQuaternion result;
            RotationLookAtRH(ref forward, ref up, out result);
            return result;
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <param name="result">When the method completes, contains the created billboard DQuaternion.</param>
        public static void BillboardLH(ref DVector3 objectPosition, ref DVector3 cameraPosition, ref DVector3 cameraUpVector, ref DVector3 cameraForwardVector, out DQuaternion result)
        {
            DMatrix3x3 matrix;
            DMatrix3x3.BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out matrix);
            RotationMatrix(ref matrix, out result);
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard DQuaternion.</returns>
        public static DQuaternion BillboardLH(DVector3 objectPosition, DVector3 cameraPosition, DVector3 cameraUpVector, DVector3 cameraForwardVector)
        {
            DQuaternion result;
            BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <param name="result">When the method completes, contains the created billboard DQuaternion.</param>
        public static void BillboardRH(ref DVector3 objectPosition, ref DVector3 cameraPosition, ref DVector3 cameraUpVector, ref DVector3 cameraForwardVector, out DQuaternion result)
        {
            DMatrix3x3 matrix;
            DMatrix3x3.BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out matrix);
            RotationMatrix(ref matrix, out result);
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        /// <returns>The created billboard DQuaternion.</returns>
        public static DQuaternion BillboardRH(DVector3 objectPosition, DVector3 cameraPosition, DVector3 cameraUpVector, DVector3 cameraForwardVector)
        {
            DQuaternion result;
            BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector, out result);
            return result;
        }

        /// <summary>
        /// Creates a DQuaternion given a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <returns>The newly created DQuaternion.</returns>
        public static DQuaternion RotationMatrix(DMatrix matrix)
        {
            DQuaternion result;
            RotationMatrix(ref matrix, out result);
            return result;
        }

        /// <summary>
        /// Creates a DQuaternion given a yaw, pitch, and roll value.
        /// </summary>
        /// <param name="yaw">The yaw of rotation.</param>
        /// <param name="pitch">The pitch of rotation.</param>
        /// <param name="roll">The roll of rotation.</param>
        /// <param name="result">When the method completes, contains the newly created DQuaternion.</param>
        public static void RotationYawPitchRoll(double yaw, double pitch, double roll, out DQuaternion result)
        {
            double halfRoll = roll * 0.5f;
            double halfPitch = pitch * 0.5f;
            double halfYaw = yaw * 0.5f;

            double sinRoll = (double)Math.Sin(halfRoll);
            double cosRoll = (double)Math.Cos(halfRoll);
            double sinPitch = (double)Math.Sin(halfPitch);
            double cosPitch = (double)Math.Cos(halfPitch);
            double sinYaw = (double)Math.Sin(halfYaw);
            double cosYaw = (double)Math.Cos(halfYaw);

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
        }

        /// <summary>
        /// Creates a DQuaternion given a yaw, pitch, and roll value.
        /// </summary>
        /// <param name="yaw">The yaw of rotation.</param>
        /// <param name="pitch">The pitch of rotation.</param>
        /// <param name="roll">The roll of rotation.</param>
        /// <returns>The newly created DQuaternion.</returns>
        public static DQuaternion RotationYawPitchRoll(double yaw, double pitch, double roll)
        {
            DQuaternion result;
            RotationYawPitchRoll(yaw, pitch, roll, out result);
            return result;
        }

        /// <summary>
        /// Interpolates between two DQuaternions, using spherical linear interpolation.
        /// </summary>
        /// <param name="start">Start DQuaternion.</param>
        /// <param name="end">End DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the spherical linear interpolation of the two DQuaternions.</param>
        public static void Slerp(ref DQuaternion start, ref DQuaternion end, double amount, out DQuaternion result)
        {
            double opposite;
            double inverse;
            double dot = Dot(start, end);

            if (Math.Abs(dot) > 1.0f - DMathUtil.ZeroTolerance)
            {
                inverse = 1.0f - amount;
                opposite = amount * Math.Sign(dot);
            }
            else
            {
                double acos = (double)Math.Acos(Math.Abs(dot));
                double invSin = (double)(1.0 / Math.Sin(acos));

                inverse = (double)Math.Sin((1.0f - amount) * acos) * invSin;
                opposite = (double)Math.Sin(amount * acos) * invSin * Math.Sign(dot);
            }

            result.X = (inverse * start.X) + (opposite * end.X);
            result.Y = (inverse * start.Y) + (opposite * end.Y);
            result.Z = (inverse * start.Z) + (opposite * end.Z);
            result.W = (inverse * start.W) + (opposite * end.W);
        }

        /// <summary>
        /// Interpolates between two DQuaternions, using spherical linear interpolation.
        /// </summary>
        /// <param name="start">Start DQuaternion.</param>
        /// <param name="end">End DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The spherical linear interpolation of the two DQuaternions.</returns>
        public static DQuaternion Slerp(DQuaternion start, DQuaternion end, double amount)
        {
            DQuaternion result;
            Slerp(ref start, ref end, amount, out result);
            return result;
        }

        /// <summary>
        /// Interpolates between DQuaternions, using spherical quadrangle interpolation.
        /// </summary>
        /// <param name="value1">First source DQuaternion.</param>
        /// <param name="value2">Second source DQuaternion.</param>
        /// <param name="value3">Third source DQuaternion.</param>
        /// <param name="value4">Fourth source DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of interpolation.</param>
        /// <param name="result">When the method completes, contains the spherical quadrangle interpolation of the DQuaternions.</param>
        public static void Squad(ref DQuaternion value1, ref DQuaternion value2, ref DQuaternion value3, ref DQuaternion value4, double amount, out DQuaternion result)
        {
            DQuaternion start, end;
            Slerp(ref value1, ref value4, amount, out start);
            Slerp(ref value2, ref value3, amount, out end);
            Slerp(ref start, ref end, 2.0f * amount * (1.0f - amount), out result);
        }

        /// <summary>
        /// Interpolates between DQuaternions, using spherical quadrangle interpolation.
        /// </summary>
        /// <param name="value1">First source DQuaternion.</param>
        /// <param name="value2">Second source DQuaternion.</param>
        /// <param name="value3">Third source DQuaternion.</param>
        /// <param name="value4">Fourth source DQuaternion.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of interpolation.</param>
        /// <returns>The spherical quadrangle interpolation of the DQuaternions.</returns>
        public static DQuaternion Squad(DQuaternion value1, DQuaternion value2, DQuaternion value3, DQuaternion value4, double amount)
        {
            DQuaternion result;
            Squad(ref value1, ref value2, ref value3, ref value4, amount, out result);
            return result;
        }

        /// <summary>
        /// Sets up control points for spherical quadrangle interpolation.
        /// </summary>
        /// <param name="value1">First source DQuaternion.</param>
        /// <param name="value2">Second source DQuaternion.</param>
        /// <param name="value3">Third source DQuaternion.</param>
        /// <param name="value4">Fourth source DQuaternion.</param>
        /// <returns>An array of three DQuaternions that represent control points for spherical quadrangle interpolation.</returns>
        public static DQuaternion[] SquadSetup(DQuaternion value1, DQuaternion value2, DQuaternion value3, DQuaternion value4)
        {
            DQuaternion q0 = (value1 + value2).LengthSquared() < (value1 - value2).LengthSquared() ? -value1 : value1;
            DQuaternion q2 = (value2 + value3).LengthSquared() < (value2 - value3).LengthSquared() ? -value3 : value3;
            DQuaternion q3 = (value3 + value4).LengthSquared() < (value3 - value4).LengthSquared() ? -value4 : value4;
            DQuaternion q1 = value2;

            DQuaternion q1Exp, q2Exp;
            Exponential(ref q1, out q1Exp);
            Exponential(ref q2, out q2Exp);

            DQuaternion[] results = new DQuaternion[3];
            results[0] = q1 * Exponential(-0.25f * (Logarithm(q1Exp * q2) + Logarithm(q1Exp * q0)));
            results[1] = q2 * Exponential(-0.25f * (Logarithm(q2Exp * q3) + Logarithm(q2Exp * q1)));
            results[2] = q2;

            return results;
        }

        /// <summary>
        /// Adds two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to add.</param>
        /// <param name="right">The second DQuaternion to add.</param>
        /// <returns>The sum of the two DQuaternions.</returns>
        public static DQuaternion operator +(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Add(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Subtracts two DQuaternions.
        /// </summary>
        /// <param name="left">The first DQuaternion to subtract.</param>
        /// <param name="right">The second DQuaternion to subtract.</param>
        /// <returns>The difference of the two DQuaternions.</returns>
        public static DQuaternion operator -(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Subtract(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Reverses the direction of a given DQuaternion.
        /// </summary>
        /// <param name="value">The DQuaternion to negate.</param>
        /// <returns>A DQuaternion facing in the opposite direction.</returns>
        public static DQuaternion operator -(DQuaternion value)
        {
            DQuaternion result;
            Negate(ref value, out result);
            return result;
        }

        /// <summary>
        /// Scales a DQuaternion by the given value.
        /// </summary>
        /// <param name="value">The DQuaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the DQuaternion.</param>
        /// <returns>The scaled DQuaternion.</returns>
        public static DQuaternion operator *(double scale, DQuaternion value)
        {
            DQuaternion result;
            Multiply(ref value, scale, out result);
            return result;
        }

        /// <summary>
        /// Scales a DQuaternion by the given value.
        /// </summary>
        /// <param name="value">The DQuaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the DQuaternion.</param>
        /// <returns>The scaled DQuaternion.</returns>
        public static DQuaternion operator *(DQuaternion value, double scale)
        {
            DQuaternion result;
            Multiply(ref value, scale, out result);
            return result;
        }

        /// <summary>
        /// Multiplies a DQuaternion by another.
        /// </summary>
        /// <param name="left">The first DQuaternion to multiply.</param>
        /// <param name="right">The second DQuaternion to multiply.</param>
        /// <returns>The multiplied DQuaternion.</returns>
        public static DQuaternion operator *(DQuaternion left, DQuaternion right)
        {
            DQuaternion result;
            Multiply(ref left, ref right, out result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DQuaternion left, DQuaternion right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DQuaternion left, DQuaternion right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        ///// <inheritdoc/>
        //void IDataSerializable.Serialize(BinarySerializer serializer)
        //{
        //    // Write optimized version without using Serialize methods
        //    if (serializer.Mode == SerializerMode.Write)
        //    {
        //        serializer.Writer.Write(X);
        //        serializer.Writer.Write(Y);
        //        serializer.Writer.Write(Z);
        //        serializer.Writer.Write(W);
        //    }
        //    else
        //    {
        //        X = serializer.Reader.ReadSingle();
        //        Y = serializer.Reader.ReadSingle();
        //        Z = serializer.Reader.ReadSingle();
        //        W = serializer.Reader.ReadSingle();
        //    }
        //}

        /// <summary>
        /// Determines whether the specified <see cref="DQuaternion"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="DQuaternion"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DQuaternion"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref DQuaternion other)
        {
            return DMathUtil.NearEqual(other.X, X) && DMathUtil.NearEqual(other.Y, Y) && DMathUtil.NearEqual(other.Z, Z) && DMathUtil.NearEqual(other.W, W);
        }

        /// <summary>
        /// Determines whether the specified <see cref="DQuaternion"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="DQuaternion"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="DQuaternion"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(DQuaternion other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is DQuaternion))
                return false;

            var strongValue = (DQuaternion)value;
            return Equals(ref strongValue);
        }
    }
}
