using System;

namespace Assets.Scripts.Internal
{
    public static class StringExtension
	{
		public static string ReverseString(this string str)
		{
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
	}
}
