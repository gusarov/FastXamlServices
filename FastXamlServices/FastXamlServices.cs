using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xaml;
using FastXamlServices.Internal;

namespace FastXamlServices
{
	public class FastXamlServices : IXamlServices
	{
		public object Load(string fileName)
		{
			using (var file = File.OpenRead(fileName))
			{
				return Load(file);
			}
		}

		public object Load(Stream stream)
		{
			throw new System.NotImplementedException();
		}

		public object Parse(string xaml)
		{
			using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
			{
				return Load(ms);
			}
		}

		public string Save(object instance)
		{
			using (var ms = new MemoryStream())
			{
				Save(ms, instance);
				return Encoding.UTF8.GetString(ms.ToArray());
			}
		}

		public void Save(string fileName, object instance)
		{
			using (var file = File.OpenWrite(fileName))
			{
				Save(file, instance);
				// file.Flush();
				file.SetLength(file.Position);
			}
		}

		public void Save(Stream stream, object instance)
		{
			using (var sw = new StreamWriter(stream))
			{
				Save(sw, instance);
			}
		}

		public void Save(StreamWriter stream, object instance)
		{
			var ctx = new SerializationContext
			{
				TextWriter = stream,
			};
			Write(ctx, instance);
		}

		void Write(SerializationContext ctx,  object instance)
		{
			var type = instance.GetType();
			WriterFor(type)(ctx, instance);
			/*
			var elements = GetElements(type);
			stream.Write("<" + GetTypeName(type));
			// attribs
			foreach (var attrib in elements.Values.OfType<AttribElement>())
			{
				stream.Write(attrib.Name);
				stream.Write("=\"");
				stream.Write('\"');
			}
			*/
		}

		private static readonly ConcurrentDictionary<Type, string> _typeNames = new ConcurrentDictionary<Type, string>();

		string GetTypeName(Type type)
		{
			return _typeNames.GetOrAdd(type, GetTypeNameCore);
		}
		string GetTypeNameCore(Type type)
		{
			return type.Name;
		}
		private static readonly ConcurrentDictionary<Type, IDictionary<string, Element>> _elements = new ConcurrentDictionary<Type, IDictionary<string, Element>>();

		IDictionary<string, Element> GetElements(Type type)
		{
			return _elements.GetOrAdd(type, GetElementsCore);
		}

		IDictionary<string, Element> GetElementsCore(Type type)
		{
			var elements = new Dictionary<string, Element>();
			foreach (var pi in type.GetProperties())
			{
				if (pi.PropertyType.IsEnum || pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string))
				{
					elements.Add(pi.Name, new AttribElement(pi));
				}
				else
				{
					elements.Add(pi.Name, new SubnodeElement(pi));
				}
			}
			return elements;
		}

		abstract class Element
		{
			public string Name { get; }
			public Type Type { get; }
			public PropertyInfo _pi;

			public Element(PropertyInfo pi)
			{
				_pi = pi;
				Name = pi.Name;
				Type = pi.PropertyType;
			}


		}
		class AttribElement : Element
		{
			public AttribElement(PropertyInfo pi) : base(pi)
			{
				
			}
		}
		class SubnodeElement : Element
		{
			public SubnodeElement(PropertyInfo pi) : base(pi)
			{
				
			}
		}

		private static readonly ConcurrentDictionary<Type, Action<SerializationContext,object>> _writers = new ConcurrentDictionary<Type, Action<SerializationContext, object>>();

		public static Action<SerializationContext, object> WriterFor(Type type)
		{
			return _writers.GetOrAdd(type, CreateWriter);
		}

		static Action<SerializationContext, object> CreateWriter(Type type)
		{
			return (x, o) =>
			{
				x.TextWriter.WriteLine("<" + type.Name + "/>");
			};
		}
	}

	namespace Internal
	{
		public class SerializationContext
		{
			public bool XmlnsEmited { get; set; }
			public TextWriter TextWriter { get; set; }
		}

	}

}