using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Assets.Scripts.Internal
{
	public static class LanguagesManager
	{
		public static SortedDictionary<string, Locale> Languages { get; private set; } = new();

		/// <summary>
		/// Gets locale by full NativeName or the first locale that contains 'name'
		/// </summary>
		/// <param name="name"></param>
		/// <returns>matching locale</returns>
		/// <exception cref="ArgumentNullException">name can not be null or epty</exception>
		public static Locale GetLocale(string name)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Language name can not be null or epty");
			Locale locale;
			bool langExists = Languages.TryGetValue(name, out locale);
			if (langExists) return locale;
			return Languages.Where(item =>
			{
				return item.Key.ToLower().Contains(name.ToLower());
			}).First().Value;
		}
		public static string GetNameByLocale(Locale locale)
		{
			return Languages.Where(item =>
			{
				return item.Value == locale;
			}).First().Key;
		}


		/// <summary>
		/// Applies in game locale by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns>applied locale</returns>
		public static Locale SetLocale(string name)
		{
			var chengeTo = LanguagesManager.GetLocale(name);
			LocalizationSettings.SelectedLocale = chengeTo;
			return chengeTo;
		}


		public static void AddLanguage(string name, Locale locale)
		{
			if (Languages.ContainsKey(name) == false)
			{
				Languages.Add(name, locale);
			}
		}
		public static void AddLanguageNative(string nameInLanguage)
		{
			if (Languages.ContainsKey(nameInLanguage) == false)
			{
				var lang = LocalizationSettings.AvailableLocales.Locales.Where(item =>
				{
					return item.Identifier.CultureInfo.NativeName.ToLower().StartsWith(nameInLanguage.ToLower());
				}
				).First();
				Languages.Add(nameInLanguage, lang);
			}
		}

		/// <summary>
		/// Adds all AvailableLocales declared in the project by their NativeName
		/// </summary>
		/// <returns>amount of languages loaded</returns>
		public static int AddAvailableLocales()
		{
			foreach(var locale in LocalizationSettings.AvailableLocales.Locales)
			{
				//get NativeName
				string NativeName = new(locale.Identifier.CultureInfo.NativeName);
				//get only the first part of "Polski (Polska)"
				Match match = Regex.Matches(NativeName, @"\w+", RegexOptions.IgnoreCase).First();
				NativeName = match.Value;
				//make the first letter UpperCase
				NativeName = NativeName[0].ToString().ToUpper() + NativeName.Substring(1);
				//add only once
				AddLanguage(NativeName.ToString(), locale);
			}

			return Languages.Count;
		}
	}
}
