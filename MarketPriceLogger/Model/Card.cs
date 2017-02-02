using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MarketPriceLogger.Model
{
	public struct Order
	{
		public DateTime DateTime;
		public decimal Price;
	}

	public class Card : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private int _id;
		public int Id
		{
			get { return _id; }
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged();
				}
			}
		}

		private decimal _averagePrice;
		public string AveragePrice { get { return _averagePrice.ToString("C"); } }

		public string SalesCount { get { return Orders.Count.ToString(); } }

		private ObservableCollection<Order> _orders = new ObservableCollection<Order>();
		public ObservableCollection<Order> Orders { get { return _orders; } }

		private ObservableCollection<string> _history = new ObservableCollection<string>();
		public ObservableCollection<string> History { get { return _history; } }

		public void AddOrder(decimal price)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				Orders.Add(new Order()
				{
					Price = price,
					DateTime = DateTime.Now
				});
			});

			CalculateAveragePrice(price);
		}

		public void AddEvent(string eventLog)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				History.Add(eventLog);
			});
		}

		private void CalculateAveragePrice(decimal price)
		{
			int count = Orders.Count;
			_averagePrice = (_averagePrice * (count - 1) + price) / count;
			OnPropertyChanged(nameof(AveragePrice));
			OnPropertyChanged(nameof(SalesCount));
		}
	}
}
