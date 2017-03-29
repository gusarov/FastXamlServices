using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FastXamlServices.Internal;

namespace FastXamlServices.MetadataProviderDynamic
{
	class DynamicMetadataProvider : IMetadataProvider
	{
		public static readonly IMetadataProvider Instance = new DynamicMetadataProvider();

		private DynamicMetadataProvider()
		{
			
		}

		private static readonly ConcurrentDictionary<Type, TypeMetadata> _typeMetadata = new ConcurrentDictionary<Type, TypeMetadata>();

		static TypeMetadata GetTypeMetada(Type type)
		{
			return _typeMetadata.GetOrAdd(type, CreateTypeMetadata);
		}

		private static TypeMetadata CreateTypeMetadata(Type type)
		{
			return new TypeMetadata(type);
		}

		IEnumerable<Element> GetElements(Type type)
		{
			return GetTypeMetada(type).Elements;
		}

		public Action<SerializationWriterContext, object> GetWriter(Type type)
		{
			return UniversalWriter;
		}

		public Func<SerializationReaderContext, object> GetReader(Type type)
		{
/*
var obj = Activator.CreateInstance(type);

foreach (var attrib in nodeAttribs)
{
	type.GetProperty(attrib.Key).SetValue(obj, attrib.Value);
}
*/
			return ctx => UniversalReader(ctx, type);
		}

		/*
		object ReadNodeObject(SerializationReaderContext ctx, int from)
		{
			var nodeAttribs = ReadAttributes(ctx, from);
			if (!ctx.NamespaceToAlias.Any())
			{
				ReadSystemAttributes(ctx, nodeAttribs);
			}
			string nodeType = ParseNodeType(ctx, from);
			var type = ctx.GetType(nodeType);
			var obj = Activator.CreateInstance(type);

			foreach (var attrib in nodeAttribs)
			{
				type.GetProperty(attrib.Key).SetValue(obj, attrib.Value);
			}

			return obj;
		}

		void ReadSystemAttributes(SerializationReaderContext ctx, IDictionary<string, string> attribs)
		{
			foreach (var kvp in attribs.ToArray())
			{
				if (kvp.Key == "xmlns")
				{
					ctx.RootNamespace = kvp.Value;
					attribs.Remove(kvp);
				}
				if (kvp.Key.StartsWith("xmlns:"))
				{
					var i = kvp.Key.IndexOf(':');
					var pref = kvp.Key.Substring(i + 1);
					ctx.NamespaceToAlias[kvp.Value] = pref;
					ctx.AliasToNamespace[pref] = kvp.Value;
					attribs.Remove(kvp);
				}
			}
		}

		string ParseNodeType(SerializationReaderContext ctx, int from)
		{
			int indexOfNodeNickStart = ctx.Xaml.IndexOf('<', from) + 1;
			int indexOfNodeNickEnd = ctx.Xaml.IndexOf(' ', indexOfNodeNickStart);
			string nodeType = ctx.Xaml.Substring(indexOfNodeNickStart, indexOfNodeNickEnd - indexOfNodeNickStart);
			return nodeType;
		}

		IDictionary<string, string> ReadAttributes(SerializationReaderContext ctx, int from)
		{
			var dic = new Dictionary<string, string>();
			int indexOfNodeEnd = ctx.Xaml.IndexOf('>', from);
			int i = from;
			do
			{
				i = ctx.Xaml.IndexOf(' ', i + 1);
				int iProNameEnd = ctx.Xaml.IndexOf('=', i);
				if (iProNameEnd < 0)
				{
					break;
				}
				string proName = ctx.Xaml.Substring(i, iProNameEnd - i).Trim();
				int iProValStart = ctx.Xaml.IndexOf('"', i) + 1;
				int iProValEnd = ctx.Xaml.IndexOf('"', iProValStart + 1);
				string proVal = ctx.Xaml.Substring(iProValStart, iProValEnd - iProValStart);
				dic[proName] = proVal;
			} while (i < indexOfNodeEnd && i >= 0);
			return dic;
		}
		*/

		object UniversalReader(SerializationReaderContext ctx, Type type)
		{
			var obj = Activator.CreateInstance(type);

			foreach (var attrib in ctx.CurrentNodeAttribs)
			{
				var pi = type.GetProperty(attrib.Key);
				if (pi == null)
				{
					throw new Exception($"Property {attrib.Key} not found");
				}
				pi.SetValue(obj, ConvertFromString(pi.PropertyType, attrib.Value));
			}

			return obj;
		}

		void UniversalWriter(SerializationWriterContext ctx, object instance)
		{
			var type = instance.GetType();
			var meta = GetTypeMetada(type);
			var elements = meta.Elements;
			ctx.NamespacesUsed.Add(meta.XmlNamespace);
			string alias;
			if (ctx.Indentation > 0 && meta.XmlNamespace != ctx.RootNamespace || meta.IsX())
			{
				alias = ctx.GetAliasFor(meta.XmlNamespace) + ':';
			}
			else
			{
				alias = null;
				ctx.RootNamespace = meta.XmlNamespace;
			}

			if (ctx.Indentation != 0 && ctx.Result[ctx.Result.Length - 1] != '\n')
			{
				ctx.Result.AppendLine();
			}
			ctx.Write($"{Indentation(ctx.Indentation)}<{alias}{type.Name}");

			UniversalWriterAttributes(ctx, instance, elements);

			// <nested in new builder>
			var subCtx = ctx.Nested();
			UniversalWriterSubnodes(subCtx, instance, elements);
			// </nested in new builder>

			if (!ctx.XmlnsEmited && ctx.Indentation == 0)
			{
				if (!string.IsNullOrEmpty(ctx.RootNamespace))
				{
					ctx.Write($" xmlns=\"{ctx.RootNamespace}\"");
				}
				foreach (var ns in ctx.NamespacesUsed)
				{
					if (ns != ctx.RootNamespace)
					{
						ctx.Write($" xmlns:{ctx.GetAliasFor(ns)}=\"{ns}\"");
					}
				}
				ctx.XmlnsEmited = true;
			}

			// <body>
			string body = null;
			if (meta.IsBody())
			{
				body = string.Format(CultureInfo.InvariantCulture, "{0}", instance);
			}
			// </body>


			if (subCtx.Result.Length > 0)
			{
				ctx.WriteLine(">");
				ctx.Write(subCtx.Result.ToString());
				ctx.Write($"{Indentation(ctx.Indentation)}</{alias}{type.Name}>");
			}
			else if (body != null)
			{
				ctx.Write(">");
				ctx.Write(body);
				ctx.Write($"</{alias}{type.Name}>");
			}
			else
			{
				ctx.Write(" />");
			}
			if (ctx.Indentation != 0)
			{
				ctx.Result.AppendLine();
			}
		}

		string Indentation(int c)
		{
			return new string(' ', c * 2);
		}

		void UniversalWriterAttributes(SerializationWriterContext ctx, object instance, IEnumerable<Element> elements)
		{
			// PROPERTY as ATTRIBUTE:
			// primitive
			// composite null
			foreach (var element in elements.OrderBy(x => x.Name))
			{
				var val = element.GetValue(instance);
				if (IsInAttrib(val))
				{
					if (element.DefaultValueAttribute == null || !Equals(element.DefaultValueAttribute.Value, val))
					{
						if (ReferenceEquals(null, val))
						{
							var x = ctx.GetAliasFor("http://schemas.microsoft.com/winfx/2006/xaml");
							val = $"{{{x}:Null}}";
						}
						val = ConvertToString(val);
						ctx.Write($@" {element.Name}=""{val}""");
					}
				}
			}
		}

		private static readonly Dictionary<Type, TypeConverter> _typeConverters = new Dictionary<Type, TypeConverter>();

		private TypeConverter GetTypeConverter(Type type)
		{
			TypeConverter typeConverter;
			if (!_typeConverters.TryGetValue(type, out typeConverter))
			{
				_typeConverters[type] = typeConverter = GetTypeConverterCore(type);
			}
			return typeConverter;
		}

		private TypeConverter GetTypeConverterCore(Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return GetTypeConverterCore(type.GetGenericArguments()[0]);
			}

			var c = TypeDescriptor.GetConverter(type);
			return c;
		}


		private string ConvertToString(object value)
		{
			if (value is DateTime)
			{
				// couple hacks to make "o" format shorter!
				var dt = (DateTime)value;
				if (dt.TimeOfDay == default(TimeSpan))
				{
					return dt.ToString("yyyy'-'MM'-'dd");
				}
				return dt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK"); // modified ISO 8601 (without trailing zeros)
			}
			var ct = GetTypeConverter(value.GetType());
			if (ct != null)
			{
				return ct.ConvertToInvariantString(value);
			}
			return value.ToString();
		}

		private object ConvertFromString(Type expected, string str)
		{
			if (str == "{x:Null}") // the only currently supported Markup Extension - hardcoded 
			{
				return null;
			}

			if (expected == typeof(DateTime) || expected == typeof(DateTime?))
			{
				DateTime dt;
				var nd = DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt);
				if (nd)
				{
					return dt;
				}
				throw new Exception("Unable to parse datetime: " + str);
			}

			var ct = GetTypeConverter(expected);
			if (ct != null && ct.CanConvertFrom(typeof(string)))
			{
				return ct.ConvertFromInvariantString(str);
			}
			throw new Exception("Unable to convert value from string for " + expected.Name);
		}

		bool IsInAttrib(object propertyValue)
		{
			if (ReferenceEquals(null, propertyValue))
			{
				return true;
			}
			var type = propertyValue.GetType();
			if (type == typeof(string))
			{
				return true;
			}
			if (type.IsValueType)
			{
				return true;
			}
			return false;
		}

		void UniversalWriterSubnodes(SerializationWriterContext ctx, object instance, IEnumerable<Element> elements)
		{
			// PROPERTY as SUBNODE:
			// composite not null
			// collection
			foreach (var element in elements.OrderBy(x=>x.Name))
			{
				var subObj = element.GetValue(instance);
				if (!IsInAttrib(subObj))
				{
					var subList = subObj as IList;
					if (subObj != null && (subList == null || subList.Count > 0)) // subList == null - this is composite
					{
						ctx.Write($"{Indentation(ctx.Indentation)}<{instance.GetType().Name}.{element.Name}>");
						ctx.Indentation++;
						if (subList != null)
						{
							ctx.Result.AppendLine();
							for (int index = 0; index < subList.Count; index++)
							{
								var item = subList[index];
								GetWriter(item.GetType())(ctx, item);
							}
						}
						else
						{
							UniversalWriter(ctx, subObj);
						}
						ctx.Indentation--;
						ctx.WriteLine($"{Indentation(ctx.Indentation)}</{instance.GetType().Name}.{element.Name}>");
					}
				}
			}
		}

	}
}
