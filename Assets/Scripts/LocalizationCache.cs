using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizationCache : MonoBehaviour
{
	public static Dictionary<string,string> localiCache = new Dictionary<string,string>();

	public static string GetTrasnaltion(string key)
	{
		//if (localiCache.ContainsKey(key))
			//return localiCache[key];

        //LocalizationSettings.SelectedLocale
        var res = LocalizationSettings.StringDatabase.GetLocalizedString("StringTable", key);
		if (res == null)
			throw new NotFoundException($"Localization Key: {key} Not Found");
		//localiCache.Add(key, res);
		return res;
	}

}
