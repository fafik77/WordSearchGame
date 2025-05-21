using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace Assets.Scripts.Internal
{
	public static class FocusableExtension
	{
		public static void Focus(this Focusable focusable, bool focus)
		{
			if (focusable == null) return;
			if (focus == true) focusable.Focus();
			else focusable.Blur();
		}
	}
}
