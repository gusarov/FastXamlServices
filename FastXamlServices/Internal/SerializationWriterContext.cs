using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;

namespace FastXamlServices.Internal
{
	public abstract class SerializationContext
	{
		public string RootNamespace;
		public Dictionary<string, string> NamespaceToAlias = new Dictionary<string, string>();
		public Dictionary<string, string> AliasToNamespace = new Dictionary<string, string>();
	}

	public class SerializationReaderContext : SerializationContext
	{
		private static readonly Dictionary<Assembly, AssemblyKnownTypes> _assemblyKnownTypes = new Dictionary<Assembly, AssemblyKnownTypes>();
		class AssemblyKnownTypes
		{
			public readonly Dictionary<string, Dictionary<string, Type>> XmlNses = new Dictionary<string, Dictionary<string, Type>>();
		}

		static void AnalyzeAssembly(Assembly asm)
		{
			AssemblyKnownTypes types;
			if (!_assemblyKnownTypes.TryGetValue(asm, out types))
			{
				_assemblyKnownTypes[asm] = types = GenerateAssemblyKnownTypes(asm);
			}
		}

		static AssemblyKnownTypes GenerateAssemblyKnownTypes(Assembly asm)
		{
			var akt = new AssemblyKnownTypes();
			var xmlns = ((XmlnsDefinitionAttribute[])asm.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), true))
				.ToDictionary(xm => xm.ClrNamespace);
			foreach (var type in asm.GetTypes())
			{
				if (type.Namespace != null && type.IsVisible && ((type.IsClass && !type.IsAbstract) || type.IsValueType))
				{
					var attr = xmlns.ItemOrDefault(type.Namespace);
					if (attr != null)
					{
						akt.XmlNses.Item(attr.XmlNamespace).Add(type.Name, type);
					}
				}
			}
			return akt;
		}

		public string Xaml { get; private set; }

		public SerializationReaderContext(string xaml)
		{
			Xaml = xaml;
		}

		private readonly Dictionary<string, Type> _typeNameToType = new Dictionary<string, Type>();

		public Type GetType(string nodeType)
		{
			Type type;
			if (!_typeNameToType.TryGetValue(nodeType, out type))
			{
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					AnalyzeAssembly(asm);
				}

				foreach (var akt in _assemblyKnownTypes.Values)
				{
					var xmlns = akt.XmlNses.ItemOrDefault(RootNamespace);
					if (xmlns != null)
					{
						type = xmlns.ItemOrDefault(nodeType);
						if (type != null)
						{
							break;
						}
					}
				}

				if (type == null)
				{
					throw new Exception($"Type {nodeType} not found");
				}

				_typeNameToType[nodeType] = type;
			}
			return type;
		}


		public IDictionary<string, string> CurrentNodeAttribs;
	}

	public class SerializationWriterContext : SerializationContext
	{
		public bool XmlnsEmited
		{
			get
			{
				return _xmlnsEmited;
			}
			set
			{
				_xmlnsEmited = value;
				if (_parent != null)
				{
					_parent.XmlnsEmited = value;
				}
			}
		}

		public int Indentation;
		public StringBuilder Result = new StringBuilder();
		public HashSet<string> NamespacesUsed = new HashSet<string>();

		public void Write(char c)
		{
			Result.Append(c);
		}

		public void Write(string str)
		{
			Result.Append(str);
		}
		public void WriteLine(string str)
		{
			Result.AppendLine(str);
		}

		private char _nextAlias = 'a';

		public string GetAliasFor(string xmlNamespace)
		{
			NamespacesUsed.Add(xmlNamespace);
			string alias;
			if (!NamespaceToAlias.TryGetValue(xmlNamespace, out alias))
			{
				if (xmlNamespace == "http://schemas.microsoft.com/winfx/2006/xaml")
				{
					alias = "x";
				}
				else
				{
					alias = (_nextAlias++).ToString();
				}
				NamespaceToAlias[xmlNamespace] = alias;
				AliasToNamespace[alias] = xmlNamespace;
			}
			return alias;
		}

		private SerializationWriterContext _parent;
		private bool _xmlnsEmited;

		public SerializationWriterContext Nested()
		{
			var ctx = (SerializationWriterContext)MemberwiseClone();
			ctx._parent = this;
			ctx.Result = new StringBuilder();
			ctx.Indentation++;
			return ctx;
		}
	}
}