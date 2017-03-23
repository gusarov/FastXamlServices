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
		public static DynamicMetadataProvider Instance = new DynamicMetadataProvider();

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

		public Action<SerializationContext, object> GetWriter(Type type)
		{
			return UniversalWriter;
		}

		void UniversalWriter(SerializationContext ctx, object instance)
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

		void UniversalWriterAttributes(SerializationContext ctx, object instance, IEnumerable<Element> elements)
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
			/*
			var cvta = (TypeConverterAttribute)type.GetCustomAttributes(typeof(TypeConverterAttribute), true).FirstOrDefault();
			if (cvta != null)
			{
				var aqn = cvta.ConverterTypeName;
				var cvtType = Type.GetType(aqn, true);
				var ct = (TypeConverter)Activator.CreateInstance(cvtType);
				return ct;
			}
			return null;
			*/
		}

		private string ConvertToString(object value)
		{
			if (value is DateTime)
			{
				return ((DateTime)value).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK"); // modified ISO 8601 (without trailing zeros)
			}
			var ct = GetTypeConverter(value.GetType());
			if (ct != null)
			{
				return ct.ConvertToInvariantString(value);
			}
			return value.ToString();
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

		void UniversalWriterSubnodes(SerializationContext ctx, object instance, IEnumerable<Element> elements)
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
							foreach (var item in subList)
							{
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
