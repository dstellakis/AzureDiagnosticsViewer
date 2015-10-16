using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using AzureDiagnosticsViewer.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDiagnosticsViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Timers.Timer _Timer = null;
		private DateTime _LastUpdateDate;

		public MainWindow()
		{
			InitializeComponent();

			txtDateFrom.Text = DateTime.Now.Date.ToString("d/M/yyyy HH:mm");

			_Timer = new System.Timers.Timer(90 * 1000);
			_Timer.Elapsed += Timer_Elapsed;
		}

		private async void SearchEventLogButton_Click(object sender, RoutedEventArgs e)
		{
			(this.DataContext as MainViewModel).StatusBarMessage = "Getting data from event logs table, please wait...";

			DateTime fromDate = !String.IsNullOrEmpty(txtDateFrom.Text) ? DateTime.Parse(txtDateFrom.Text) : DateTime.MinValue;
			DateTime? toDate = !String.IsNullOrEmpty(txtDateTo.Text) ? DateTime.Parse(txtDateTo.Text) : (DateTime?)null;

			if (subscriptionsList.SelectedItem == null)
			{
				(this.DataContext as MainViewModel).StatusBarMessage = "Please select a subscription";
				return;
			}

			IEnumerable<WadLogEntity> entries = await BeginGetEventLogEntries(subscriptionsList.SelectedItem as Subscription, fromDate, toDate, GetSelectedLogLevel(), txtQuery.Text, txtMessage.Text);

			(this.DataContext as MainViewModel).RefreshLogEntries(entries);
		}

		private async void ExportEventLogButton_Click(object sender, RoutedEventArgs e)
		{
			XDocument doc = new XDocument(
				new XDeclaration("1.0", "UTF-8", "true"),
				new XElement("LogEntries",
					from entry in (this.DataContext as MainViewModel).LogEntries
					select new XElement("LogEntry",
						new XElement("Date", entry.EventDateTimeUTC),
						new XElement("Type", entry.Level),
						new XElement("DeploymentId", entry.DeploymentId),
						new XElement("Role", entry.Role),
						new XElement("Instance", entry.RoleInstance),
						new XElement("Message", entry.Message)
					)
				)
			);

			// Configure save file dialog box
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.FileName = "Export.xml";

			// Show save file dialog box
			if (dlg.ShowDialog() == true)
			{
				doc.Save(dlg.FileName);
			}
		}

		private async void SearchPerformanceCountersButton_Click(object sender, RoutedEventArgs e)
		{
			(this.DataContext as MainViewModel).StatusBarMessage = "Getting data from performance counters table, please wait...";

			DateTime fromDate = !String.IsNullOrEmpty(txtDateFrom.Text) ? DateTime.Parse(txtDateFrom.Text) : DateTime.MinValue;
			DateTime? toDate = !String.IsNullOrEmpty(txtDateTo.Text) ? DateTime.Parse(txtDateTo.Text) : (DateTime?)null;

			if (subscriptionsList.SelectedItem == null)
			{
				(this.DataContext as MainViewModel).StatusBarMessage = "Please select a subscription";
				return;
			}

			if (performanceCounterList.SelectedItem == null)
			{
				(this.DataContext as MainViewModel).StatusBarMessage = "Please select a performance counter";
				return;
			}

			if (performanceRolesList.SelectedItem == null)
			{
				(this.DataContext as MainViewModel).StatusBarMessage = "Please select a role";
				return;
			}

			IEnumerable<WadPerformanceCounterEntity> entries = await BeginGetPerformanceCounterEntries(
																																		subscriptionsList.SelectedItem as Subscription,
																																		fromDate, toDate,
																																		(performanceRolesList.SelectedItem as Role).Name,
																																		performanceCounterList.SelectedItem as string
																																);

			(this.DataContext as MainViewModel).RefreshPerformanceCounterEntries(entries);
		}

		private async void ExportPerformanceCountersButton_Click(object sender, RoutedEventArgs e)
		{
		}

		private async Task<IEnumerable<WadLogEntity>> BeginGetEventLogEntries(Subscription subscription, DateTime fromDate, DateTime? toDate, int? logLevel, string query, string messagePattern)
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(String.Format("DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1}", subscription.AccountName, subscription.AccountKey));

			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			CloudTable table = tableClient.GetTableReference("WADLogsTable");

			string filter = String.Format("(PartitionKey ge '0{0}')", fromDate.ToUniversalTime().Ticks);

			if (toDate != null)
				filter += String.Format("and (PartitionKey le '0{0}')", toDate.Value.ToUniversalTime().Ticks);

			if (!String.IsNullOrEmpty(query))
				filter += String.Format(" and ({0})", query);

			if (logLevel != null)
				filter += String.Format(" and (Level eq {0})", logLevel);

			var logQuery = new TableQuery<WadLogEntity>() { FilterString = filter };

			return await Task.Factory.StartNew(() =>
				{
					IEnumerable<WadLogEntity> entries = table.ExecuteQuery(logQuery);

					if (!String.IsNullOrEmpty(messagePattern))
					{
						Regex regex = new Regex(messagePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

						entries = entries.Where(e => regex.IsMatch(e.Message));
					}
					return entries;
				});
		}

		private async Task<IEnumerable<WadPerformanceCounterEntity>> BeginGetPerformanceCounterEntries(Subscription subscription, DateTime fromDate, DateTime? toDate, string role, string counterName)
		{
			CloudStorageAccount storageAccount = CloudStorageAccount.Parse(String.Format("DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1}", subscription.AccountName, subscription.AccountKey));

			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			CloudTable table = tableClient.GetTableReference("WADPerformanceCountersTable");

			string filter = String.Format("(PartitionKey ge '0{0}')", fromDate.ToUniversalTime().Ticks);

			if (toDate != null)
				filter += String.Format("and (PartitionKey le '0{0}')", toDate.Value.ToUniversalTime().Ticks);

			filter += String.Format(" and (Role eq '{0}')", role);
			filter += String.Format(" and (CounterName eq '{0}')", counterName);

			var logQuery = new TableQuery<WadPerformanceCounterEntity>() { FilterString = filter };

			return await Task.Factory.StartNew(() =>
			{
				IEnumerable<WadPerformanceCounterEntity> entries = table.ExecuteQuery(logQuery);

				return entries;
			});
		}

		private void EventLogCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (chkAutoRefresh.IsChecked == true)
			{
				_LastUpdateDate = DateTime.Now;
				_Timer.Start();
			}
			else
				_Timer.Stop();
		}

		private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			this.Dispatcher.Invoke(new Action(async () =>
			{
				IEnumerable<WadLogEntity> entries = await BeginGetEventLogEntries(subscriptionsList.SelectedItem as Subscription, _LastUpdateDate, null, GetSelectedLogLevel(), txtQuery.Text, txtMessage.Text);

				foreach (WadLogEntity entry in entries)
				{
					(this.DataContext as MainViewModel).LogEntries.Add(entry);
				}

				if (entries.Any())
					_LastUpdateDate = DateTime.Now;
			}));
		}

		private int? GetSelectedLogLevel()
		{
			int? logLevel = null;

			if (typeList.SelectedIndex == 0)
				logLevel = null;
			else if (typeList.SelectedIndex == 1)
				logLevel = 4;
			else if (typeList.SelectedIndex == 2)
				logLevel = 3;
			else if (typeList.SelectedIndex == 3)
				logLevel = 2;
			else
				logLevel = 1;

			return logLevel;
		}
	}
}
