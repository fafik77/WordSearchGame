using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
	using System;
	class RetriesTimeoutException : Exception
	{
		int amount;
		public RetriesTimeoutException(int amount) : base() { this.amount = amount; }
		public RetriesTimeoutException(int amount, string msg) : base(msg) { this.amount = amount; }
	}

}