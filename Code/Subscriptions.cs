using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureDiagnosticsViewer
{
	public class Subscription
	{
		public string Title { get; set; }
		public string AccountName { get; set; }
		public string AccountKey { get; set; }

		public Role[] Roles { get; set; }

		public Subscription()
		{
			this.Roles = new Role[0];
		}
	}

	public class Role
	{
		public string Name { get; set; }
		public string[] PerformanceCounters { get; set; }

		public Role()
		{
			this.PerformanceCounters = new string[0];
		}
	}
}
