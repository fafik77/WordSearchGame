using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MenuMgr;

namespace Assets.Scripts.Internal
{
	public interface ICameraView
	{
		/// <summary>
		/// hides this camera menu view when entering deeper into menu
		/// </summary>
		void Hide();
		/// <summary>
		/// shows this camera menu view
		/// </summary>
		void Show();
		/// <summary>
		/// MenuMgr will call this to hook up all menus to main system
		/// </summary>
		/// <param name="action">When menu navigation is needed</param>
		void OnNavigateToSet(Action<MenuNavigationEnum> action);
	}
}
