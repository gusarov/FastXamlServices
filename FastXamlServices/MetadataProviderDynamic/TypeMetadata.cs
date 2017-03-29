using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace FastXamlServices.MetadataProviderDynamic
{
	class TypeMetadata
	{
		private readonly Type _type;

		public TypeMetadata(Type type)
		{
			_type = type;
			Elements = new List<Element>(GetElementsCore(type));

			if (IsX())
			{
				XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
			}
			else
			{
				var attrs = (XmlnsDefinitionAttribute[])type.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
				for (int index = 0; index < attrs.Length; index++)
				{
					var attr = attrs[index];
					if (attr.ClrNamespace == type.Namespace)
					{
						XmlNamespace = attr.XmlNamespace;
						break;
					}
				}
			}
			// clr
			if (XmlNamespace == null)
			{
				
			}
		}

		public bool IsX()
		{
			return _type.IsPrimitive || _type == typeof(string) || _type.IsValueType;
		}

		public bool IsBody()
		{
			return _type.IsPrimitive || _type == typeof(string) || _type.IsValueType;
		}

		IEnumerable<Element> GetElementsCore(Type type)
		{
			if (type == typeof(string))
			{
				// system type
				yield break;
			}
			if (type.IsPrimitive)
			{
				// system type
				yield break;
			}
			foreach (var pi in type.GetProperties())
			{
				yield return new Element(pi);
			}
		}

		public readonly string XmlNamespace;

		public IList<Element> Elements { get; }
	}
}