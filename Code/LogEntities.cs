using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureDiagnosticsViewer
{
	public class WadLogEntity : TableEntity
	{
		public WadLogEntity()
		{
			PartitionKey = "a";
			RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
		}

		public string DeploymentId { get; set; }
		public string Role { get; set; }
		public string RoleInstance { get; set; }
		public int Level { get; set; }
		public string Message { get; set; }
		public int Pid { get; set; }
		public int Tid { get; set; }
		public int EventId { get; set; }
		public long EventTickCount { get; set; }
		public DateTime EventDateTimeUTC { get { return new DateTime(EventTickCount); } }
		public DateTime EventDateTimeLocal { get { return EventDateTimeUTC.ToLocalTime(); } }
	}

	public class WadPerformanceCounterEntity : TableEntity
	{
		public WadPerformanceCounterEntity()
		{
			PartitionKey = "a";
			RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
		}

		public long EventTickCount { get; set; }
		public string DeploymentId { get; set; }
		public string Role { get; set; }
		public string RoleInstance { get; set; }
		public string CounterName { get; set; }
		public double CounterValue { get; set; }
		public DateTime EventDateTimeUTC { get { return new DateTime(EventTickCount); } }
		public DateTime EventDateTimeLocal { get { return EventDateTimeUTC.ToLocalTime(); } }
	}
}
