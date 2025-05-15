using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Internal
{
	public static class ColorExtension
	{
		public static Color FromHexRGB(this Color color, int hexValue)
		{
			float[] rgb = new float[3];
			rgb[2] = (float)(hexValue & 0xff);
			hexValue >>= 8;
			rgb[1] = (float)(hexValue & 0xff);
			hexValue >>= 8;
			rgb[0] = (float)(hexValue & 0xff);
			var oneOf255 = 1 / 255f;

			Color col = new Color(rgb[0] * oneOf255, rgb[1] * oneOf255, rgb[2] * oneOf255);
			return col;
		}
		public static Color FromHexRGBA(this Color color, int hexValue)
		{
			float[] rgba = new float[4];
			rgba[3] = (float)(hexValue & 0xff);
			hexValue >>= 8;
			rgba[2] = (float)(hexValue & 0xff);
			hexValue >>= 8;
			rgba[1] = (float)(hexValue & 0xff);
			hexValue >>= 8;
			rgba[0] = (float)(hexValue & 0xff);
			var oneOf255 = 1 / 255f;

			Color col = new Color(rgba[0] * oneOf255, rgba[1] * oneOf255, rgba[2] * oneOf255, rgba[3] * oneOf255);
			return col;
		}
		public static Color FromHex(this Color color, string hexValue)
		{
			float[] rgba = new float[4];
			rgba[3] = 255;
			var oneOf255 = 1 / 255f;
			hexValue.Trim().ToLower();
			Regex hexRegex = new("[0-9a-f]{2,8}");
			var PureHex = hexRegex.Matches(hexValue).First().Value;
			List<int> ints = new List<int>();
			if (PureHex.Length == 3 || PureHex.Length == 4)
			{
				ints = FromHexToInts(PureHex, CharactersOfHexToInt: 1);
			}
			else if (PureHex.Length == 6 || PureHex.Length == 8)
			{
				ints = FromHexToInts(PureHex, CharactersOfHexToInt: 2);
			}
			for (int i = 0; i != ints.Count; ++i)
			{
				rgba[i] = ints[i] * oneOf255;
			}

			Color col = new Color(rgba[0] * oneOf255, rgba[1] * oneOf255, rgba[2] * oneOf255, rgba[3] * oneOf255);
			return col;
		}
		/// <summary>
		/// Convert String of hex value to ints
		/// </summary>
		/// <param name="hex"></param>
		/// <param name="CharactersOfHexToInt">Def: 2(0xF1)</param>
		/// <returns></returns>
		private static List<int> FromHexToInts(string hex, int CharactersOfHexToInt = 2)
		{
			List<int> ints = new List<int>();
			Regex hexRegex = new($"[0-9a-f]{CharactersOfHexToInt}", RegexOptions.IgnoreCase);
			foreach(Match hexPart in hexRegex.Matches(hex))
			{
				ints.Add(Convert.ToInt32(hexPart.Value.ToString(), 16));
			}
			return ints;
		}
	}
}
