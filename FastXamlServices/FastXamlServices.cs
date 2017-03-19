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
			var ctx = new SerializationContext();
			Write(ctx, instance);
			stream.Write(ctx.Result.ToString());
		}

		void Write(SerializationContext ctx,  object instance)
		{
			var type = instance.GetType();
			ObjectWriterFor(type)(ctx, instance);
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


		private readonly ConcurrentDictionary<Type, Action<SerializationContext, object>> _objectWriters = new ConcurrentDictionary<Type, Action<SerializationContext, object>>();

		private Action<SerializationContext, object> ObjectWriterFor(Type type)
		{
			return _objectWriters.GetOrAdd(type, CreateFallbackWriter);
		}

		private Action<SerializationContext, object> CreateFallbackWriter(Type type)
		{
			return MetadataProviderDynamic.DynamicMetadataProvider.Instance.GetWriter(type);
		}

		public void RegisterSerializer(Type type, Action<SerializationContext, object> writer)
		{
			_objectWriters[type] = writer;
		}
	}


}