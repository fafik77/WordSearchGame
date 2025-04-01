using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ResourceAcquisitionException : Exception
{
    int amount;

    public ResourceAcquisitionException(int amount) : base() { this.amount = amount; }
	public ResourceAcquisitionException(int amount, string msg) : base(msg) { this.amount = amount; }
}

