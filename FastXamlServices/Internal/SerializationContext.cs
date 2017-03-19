using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastXamlServices.Internal
{
	public class SerializationContext
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
		public string RootNamespace;
		public HashSet<string> NamespacesUsed = new HashSet<string>();
		public Dictionary<string, string> NamespaceToAlias = new Dictionary<string, string>();
		public Dictionary<string, string> AliasToNamespace = new Dictionary<string, string>();

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

		private SerializationContext _parent;
		private bool _xmlnsEmited;

		public SerializationContext Nested()
		{
			var ctx = (SerializationContext)MemberwiseClone();
			ctx._parent = this;
			ctx.Result = new StringBuilder();
			ctx.Indentation++;
			return ctx;
		}
	}
}