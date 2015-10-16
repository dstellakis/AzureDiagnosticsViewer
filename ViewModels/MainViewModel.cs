using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AzureDiagnosticsViewer.ViewModels
{
	public class MainViewModel : GalaSoft.MvvmLight.ViewModelBase
	{
		#region Public Properties

		public ObservableCollection<Subscription> Subscriptions { get; set; }

		public ObservableCollection<WadLogEntity> LogEntries { get; set; }
		public ObservableCollection<WadPerformanceCounterEntity> PerformanceCounterEntries { get; set; }

		private Subscription _SelectedSubscription;
		public Subscription SelectedSubscription
		{
			get { return _SelectedSubscription; }
			set
			{
				_SelectedSubscription = value;
				RaisePropertyChanged("SelectedSubscription");
			}
		}

		private Role _SelectedRole;
		public Role SelectedRole
		{
			get { return _SelectedRole; }
			set
			{
				_SelectedRole = value;
				RaisePropertyChanged("SelectedRole");
			}
		}

		private string _StatusBarMessage;
		public string StatusBarMessage
		{
			get { return _StatusBarMessage; }
			set
			{
				_StatusBarMessage = value;
				RaisePropertyChanged("StatusBarMessage");
			}
		}

		#endregion

		public MainViewModel()
		{
			XDocument doc = XDocument.Load("Subscriptions.config");

			List<Subscription> subscriptions = new List<Subscription>();

			foreach (XElement elem in doc.Root.Elements("Subscription"))
			{
				Subscription subscription = new Subscription()
				{
					Title = elem.Attribute("title").Value,
					AccountName = elem.Attribute("accountName").Value,
					AccountKey = elem.Attribute("accountKey").Value,
					Roles = elem.Element("Roles").Elements("Role").Select(r => new Role()
					{
						Name = r.Attribute("name").Value,
						PerformanceCounters = r.Element("PerformanceCounters").Elements("PerformanceCounterConfiguration").Select(o => o.Attribute("counterSpecifier").Value).ToArray()
					}).ToArray()
				};

				subscriptions.Add(subscription);
			}

			this.Subscriptions = new ObservableCollection<Subscription>(subscriptions);
			RaisePropertyChanged("Subscriptions");

			this.LogEntries = new ObservableCollection<WadLogEntity>();
			RaisePropertyChanged("LogEntries");
		}

		public void RefreshLogEntries(IEnumerable<WadLogEntity> entries)
		{
			this.LogEntries = new ObservableCollection<WadLogEntity>(entries);
			RaisePropertyChanged("LogEntries");

			this.StatusBarMessage = String.Format("Total entries: {0}", this.LogEntries.Count);
		}

		public void RefreshPerformanceCounterEntries(IEnumerable<WadPerformanceCounterEntity> entries)
		{
			this.PerformanceCounterEntries = new ObservableCollection<WadPerformanceCounterEntity>(entries);
			RaisePropertyChanged("PerformanceCounterEntries");

			double average = this.PerformanceCounterEntries.Any() ? this.PerformanceCounterEntries.Average(e => e.CounterValue) : 0D;

			this.StatusBarMessage = String.Format("Total entries: {0}, Average: {1:0.00}", this.PerformanceCounterEntries.Count, average);
		}
	}
}
