﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Commands
{
	public class NestedCmdTrigger<C> : CmdTrigger<C>
		where C : ICmdArgs
	{
		private readonly CmdTrigger<C> m_Trigger;

		public NestedCmdTrigger(CmdTrigger<C> trigger, C args)
		{
			Args = args;
			m_text = trigger.Text;
			selectedCmd = trigger.selectedCmd;
			m_Trigger = trigger;
		}

		public CmdTrigger<C> Trigger
		{
			get { return m_Trigger; }
		}

		public override void Reply(string text)
		{
			m_Trigger.Reply(text);
		}

		public override void ReplyFormat(string text)
		{
			m_Trigger.ReplyFormat(text);
		}
	}
}
