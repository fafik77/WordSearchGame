using System.Collections.Generic;

namespace Assets.Scripts.Internal
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
