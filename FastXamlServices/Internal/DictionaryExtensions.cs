using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastXamlServices.Internal
{
	public static class DictionaryExtensions
	{
		public static TV Item<TK, TV>(this IDictionary<TK, TV> dic, TK key) where TV : new()
		{
			return Item(dic, key, k => new TV());
		}
		public static TV ItemOrDefault<TK, TV>(this IDictionary<TK, TV> dic, TK key)
		{
			return Item(dic, key, k => default(TV));
		}

		public static TV Item<TK, TV>(this IDictionary<TK, TV> dic, TK key, Func<TK, TV> factory)
		{
			TV value;
			if (!dic.TryGetValue(key, out value))
			{
				dic[key] = value = factory(key);
			}
			return value;
		}
	}
}
