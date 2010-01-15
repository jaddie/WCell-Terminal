using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Variables;

namespace WCell.Terminal
{
	public class WCellConfig<C> : VariableConfiguration<C, WCellVariableDefinition>
		where C : VariableConfiguration<WCellVariableDefinition>
	{
		public WCellConfig(Action<string> onError)
			: base(onError)
		{
		}
	}
}