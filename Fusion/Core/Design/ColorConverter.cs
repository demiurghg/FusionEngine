﻿// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
#if !W8CORE
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Fusion.Core.Mathematics;

namespace Fusion.Core.Design
{
	/// <summary>
	/// Defines a type converter for <see cref="Color"/>.
	/// </summary>
	public class ColorConverter : BaseConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ColorConverter"/> class.
		/// </summary>
		public ColorConverter()
		{
			var type = typeof(Color);
			Properties = new PropertyDescriptorCollection(new[] 
            { 
                new FieldPropertyDescriptor(type.GetField("R")), 
                new FieldPropertyDescriptor(type.GetField("G")),
                new FieldPropertyDescriptor(type.GetField("B")),
                new FieldPropertyDescriptor(type.GetField("A"))
            });
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// The <paramref name="destinationType"/> parameter is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if (value is Color)
			{
				var color = (Color)value;

				if (destinationType == typeof(string))
					return ConvertFromValues(context, culture, color.ToArray());

				if (destinationType == typeof(InstanceDescriptor))
				{
					var constructor = typeof(Color).GetConstructor(MathUtil.Array(typeof(byte), 4));
					if (constructor != null)
						return new InstanceDescriptor(constructor, color.ToArray());
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var values = ConvertToValues<byte>(context, culture, value);
			return values != null ? new Color(values) : base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Creates an instance of the type that this <see cref="T:System.ComponentModel.TypeConverter"/> is associated with, using the specified context, given a set of property values for the object.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="propertyValues">An <see cref="T:System.Collections.IDictionary"/> of new property values.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> representing the given <see cref="T:System.Collections.IDictionary"/>, or null if the object cannot be created. This method always returns null.
		/// </returns>
		public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
		{
			if (propertyValues == null)
				throw new ArgumentNullException("propertyValues");

			return new Color((byte)propertyValues["R"], (byte)propertyValues["G"], (byte)propertyValues["B"], (byte)propertyValues["A"]);
		}
	}
}
#endif