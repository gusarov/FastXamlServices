using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace FastXamlServices.MetadataProviderDynamic
{
	class TypeMetadata
	{
		private Type type;

		public TypeMetadata(Type type)
		{
			this.type = type;
			Elements = new List<Element>(GetElementsCore(type));

			if (IsX())
			{
				XmlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
			}
			else
			{
				var attrs = type.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true);
				foreach (XmlnsDefinitionAttribute attr in attrs)
				{
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
			return type.IsPrimitive || type == typeof(string) || type.IsValueType;
		}
		public bool IsBody()
		{
			return type.IsPrimitive || type == typeof(string) || type.IsValueType;
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

		public string XmlNamespace;

		public IList<Element> Elements { get; }
	}
}