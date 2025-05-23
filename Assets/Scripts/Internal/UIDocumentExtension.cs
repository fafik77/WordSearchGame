using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace Assets.Scripts.Internal
{
	public static class UIDocumentExtension
	{
		public static void PickingMode_Ignore(this UIDocument ui)
		{
			PickingMode_IgnoreRecursive(ui.rootVisualElement);
		}
		public static void PickingMode_IgnoreRecursive(VisualElement element)
		{
			element.pickingMode = PickingMode.Ignore;
			foreach (var subElem in element.Children())
			{
				PickingMode_IgnoreRecursive(subElem);
			}
		}

	}
}
