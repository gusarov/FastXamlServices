using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FastXamlServices.UnitTests
{

	public class AmountConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			switch (Type.GetTypeCode(sourceType))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
				case TypeCode.Int64:
				case TypeCode.UInt64:
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
				case TypeCode.String:
					return true;
			}
			return false;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
			{
				return default(Amount);
			}
			var s = value as string;
			if (s != null)
			{
				return FromString(s, culture);
			}
			if (value is decimal)
			{
				return new Amount((decimal)value);
			}
			if (value is double)
			{
				return new Amount((decimal)(double)value);
			}
			Debug.Assert(false, "This should not happen usually");
			return new Amount(Convert.ToDecimal(value));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
			{
				throw new ArgumentNullException("destinationType");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!(value is Amount))
			{
				throw new ArgumentException();
			}
			var amount = (Amount)value;
			if (destinationType == typeof(string))
			{
				return ToString(amount, null, culture);
			}
			if (destinationType != typeof(InstanceDescriptor))
			{
				throw new ArgumentException();
			}
			return new InstanceDescriptor(typeof(Amount).GetConstructor(new[] { typeof(decimal) }), new object[] { (decimal)amount });
		}

		public static Amount FromString(string str, IFormatProvider format)
		{
			str = str.TrimEnd();
			var c = char.ToUpperInvariant(str.Last());
			long m = 1;
			switch (c)
			{
				case 'H':
					m = 100;
					break;
				case 'K':
					m = 1000;
					break;
				case 'M':
					m = 1000 * 1000;
					break;
				case 'Y':
					m = 1000 * 1000 * 1000;
					break;
			}
			if (m > 1)
			{
				str = str.Substring(0, str.Length - 1).Trim();
			}

			var main = decimal.Parse(str, format);
			return main * m;
		}

		public static string ToString(Amount value, string format = null, IFormatProvider formatProvider = null)
		{
			decimal dec = value;
			var nedative = dec < 0;
			dec = Math.Abs(dec);

			int i;
			for (i = 0; i < 3; i++)
			{
				var r = dec % 1000;
				if (r > 0 || dec == 0)
				{
					break;
				}
				dec /= 1000;
			}

			string p = "";
			switch (i)
			{
				case 0:
					break;
				case 1:
					p = "K";
					break;
				case 2:
					p = "M";
					break;
				case 3:
					p = "Y";
					break;
			}

			return (((nedative ? -1 : 1) * dec).Normalize().ToString(format, formatProvider) + "\xA0" + p).Trim();
		}


	}


	[TypeConverter(typeof(AmountConverter))]
	public struct Amount : IEquatable<Amount>, IConvertible
	{
		private readonly decimal _value;

		public Amount(decimal value)
		{
			_value = value;
		}

		/*
		public Amount(string value)
		{
			this = AmountConverter.FromString(value);
		}
		public static implicit operator Amount(string value)
		{
			return new Amount(value);
		}
		public static implicit operator string(Amount amount)
		{
			return amount.ToString(CultureInfo.InvariantCulture);
		}
		*/

		public static implicit operator Amount(decimal value)
		{
			return new Amount(value);
		}

		public static implicit operator decimal(Amount amount)
		{
			return amount._value;
		}

		public static explicit operator double(Amount amount)
		{
			return (double)amount._value;
		}

		public override string ToString()
		{
			return AmountConverter.ToString(this, NumberFormat); // this is obviously formatting for user, not serialization
		}

		public const string NumberFormat = "#,0.######";


		public string ToString(IFormatProvider formatProvider)
		{
			return AmountConverter.ToString(this, formatProvider: formatProvider);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return AmountConverter.ToString(this, format, formatProvider);
		}

		public static Amount Parse(string str)
		{
			return AmountConverter.FromString(str, CultureInfo.CurrentCulture);
		}

		public static Amount Parse(string str, IFormatProvider format)
		{
			return AmountConverter.FromString(str, format);
		}

		public bool Equals(Amount other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			if (null == obj)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Amount)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		#region IConvertible

		TypeCode IConvertible.GetTypeCode()
		{
			return TypeCode.Decimal;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			throw new NotSupportedException();
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return (char)_value;
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return (sbyte)_value;
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return (byte)_value;
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return (short)_value;
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return (ushort)_value;
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return (int)_value;
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return (uint)_value;
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return (long)_value;
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return (ulong)_value;
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return (float)_value;
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return (double)_value;
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return _value;
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new NotSupportedException();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			if (conversionType == typeof(string))
			{
				return ((IConvertible)this).ToString(provider);
			}
			if (conversionType == typeof(double))
			{
				return ((IConvertible)this).ToDouble(provider);
			}
			if (conversionType == typeof(decimal))
			{
				return ((IConvertible)this).ToDecimal(provider);
			}
			throw new NotSupportedException(string.Format("Convertion to typeName {0} is not supported", conversionType.Name));
		}

		#endregion
	}
}