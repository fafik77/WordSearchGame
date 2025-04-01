using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	}
}
