using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
	public static class CollectionExtensions
	{
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			TValue result;
			if(!dictionary.TryGetValue(key, out result))
			{
				result = new TValue();
				dictionary.Add(key, result);
			}
			return result;
		}
	}
}
