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
			using (var sr = new StreamReader(stream))
			{
				return Load(sr);
			}
		}

		public object Load(TextReader reader)
		{
			var ctx = new SerializationReaderContext(reader.ReadToEnd());
			var obj = Read(ctx);
			return obj;
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

		public void Save(TextWriter stream, object instance)
		{
			var ctx = new SerializationWriterContext();
			Write(ctx, instance);
			stream.Write(ctx.Result.ToString());
		}

		object Read(SerializationReaderContext ctx)
		{
			return ReadNodeObject(ctx, 0);
		}

		object ReadNodeObject(SerializationReaderContext ctx, int from)
		{
			var nodeAttribs = ReadAttributes(ctx, from);
			if (!ctx.NamespaceToAlias.Any())
			{
				ReadSystemAttributes(ctx, nodeAttribs);
			}
			string nodeType = ParseNodeType(ctx, from);
			var type = ctx.GetType(nodeType);

			ctx.CurrentNodeAttribs = nodeAttribs;

			var obj = ObjectReaderFor(type)(ctx);

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
			int i = ctx.Xaml.IndexOf(' ', from + 1); ;
			do
			{
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
				i = ctx.Xaml.IndexOf(' ', i + 1);
			} while (i < indexOfNodeEnd && i >= 0);
			return dic;
		}

		void Write(SerializationWriterContext ctx, object instance)
		{
			var type = instance.GetType();
			ObjectWriterFor(type)(ctx, instance);
		}


		private readonly ConcurrentDictionary<Type, Action<SerializationWriterContext, object>> _objectWriters = new ConcurrentDictionary<Type, Action<SerializationWriterContext, object>>();
		private readonly ConcurrentDictionary<Type, Func<SerializationReaderContext, object>> _objectReaders = new ConcurrentDictionary<Type, Func<SerializationReaderContext, object>>();

		private Action<SerializationWriterContext, object> ObjectWriterFor(Type type)
		{
			return _objectWriters.GetOrAdd(type, CreateFallbackWriter);
		}

		private Func<SerializationReaderContext, object> ObjectReaderFor(Type type)
		{
			return _objectReaders.GetOrAdd(type, CreateFallbackReader);
		}

		private Action<SerializationWriterContext, object> CreateFallbackWriter(Type type)
		{
			return MetadataProviderDynamic.DynamicMetadataProvider.Instance.GetWriter(type);
		}

		private Func<SerializationReaderContext, object> CreateFallbackReader(Type type)
		{
			return MetadataProviderDynamic.DynamicMetadataProvider.Instance.GetReader(type);
		}

		public void RegisterSerializer(Type type, Action<SerializationWriterContext, object> writer, Func<SerializationReaderContext, object> reader)
		{
			_objectWriters[type] = writer;
			_objectReaders[type] = reader;
		}
	}


}