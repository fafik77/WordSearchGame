using Exceptions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DedicatedServer;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;


namespace Assets.Scripts.Internal
{
	public class LocalizationHelper
	{
		/// <summary>
		/// Gets localized string from StringTable
		/// </summary>
		/// <param name="key"></param>
		/// <param name="arguments">additionall params</param>
		/// <returns>localized string</returns>
		/// <exception cref="NotFoundException">the 'key' was not found</exception>
		public static string GetTranslation(string key, params object[] arguments)
		{
			//LocalizationSettings.SelectedLocale
			var res = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", key, arguments: arguments);
			if (res == null)
				throw new NotFoundException($"Localization Key: {key} Not Found");
			//localiCache.Add(key, res);
			return res;
		}

	}

}