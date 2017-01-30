using HtmlAgilityPack;
using MarketPriceLogger.Model;
using MarketPriceLogger.ViewModel.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarketPriceLogger.ViewModel
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private ObservableCollection<Card> _cards = new ObservableCollection<Card>();
		public ObservableCollection<Card> Cards
		{
			get { return _cards; }
			set
			{
				if (_cards != value)
				{
					_cards = value;
					OnPropertyChanged();
				}
			}
		}

		private int _selectedIndex = -1;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				if (_selectedIndex != value)
				{
					_selectedIndex = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(History));
				}
			}
		}

		public ObservableCollection<string> History
		{ get { return SelectedIndex >= 0 ? Cards[SelectedIndex].History : null; } }

		private ObservableCollection<Thread> _threads = new ObservableCollection<Thread>();

		private string _cardIdToAdd = string.Empty;
		public string CardIdToAdd
		{
			get { return _cardIdToAdd; }
			set
			{
				if (_cardIdToAdd != value)
				{
					_cardIdToAdd = value;
					OnPropertyChanged();
					AddCard.RaiseCanExecuteChanged();
				}
			}
		}

		public int CardIdToAddValue { get { return int.Parse(CardIdToAdd); } }

		public SimpleCommand AddCard { get; set; }

		public MainViewModel()
		{
			AddCard = new SimpleCommand(() =>
			{
				Card card = new Card()
				{
					Id = CardIdToAddValue
				};
				Cards.Add(card);
				Thread thread = new Thread(new ParameterizedThreadStart((object c) =>
				{
					Card marketCard = c as Card;
					HttpClient httpClient = new HttpClient();
					long lastTimestamp = 0;
					while (true)
					{
						Thread.Sleep(500);
						string responseText = httpClient.GetStringAsync($@"https://steamcommunity.com/market/itemordersactivity?country=RU&language=russian&currency=5&item_nameid={marketCard.Id}&two_factor=0").Result;
						JObject responseJson = JObject.Parse(responseText);
						long timestamp = (long)responseJson["timestamp"];
						if (timestamp == lastTimestamp)
						{
							continue;
						}
						lastTimestamp = timestamp;
						JArray activityJArray = (JArray)responseJson["activity"];
						for (int i = 0; i < activityJArray.Count; i++)
						{
							string activityString = (string)activityJArray[i];
							HtmlDocument htmlDoc = new HtmlDocument();
							htmlDoc.LoadHtml(activityString);
							marketCard.AddEvent(htmlDoc.DocumentNode.InnerText.Trim('\n', '\t', '\r'));
							if (!activityString.Contains("приобрел"))
							{
								continue;
							}
							int firstIndex = activityString.IndexOf(" за ") + 4;
							int lastIndex = activityString.LastIndexOf(" p");
							string priceString = activityString.Substring(firstIndex, lastIndex - firstIndex);

							decimal price = decimal.Parse(priceString);
							marketCard.AddOrder(price);

							StringBuilder sb = new StringBuilder();
							
							HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//span[@class='market_ticker_name']");
							sb.Append(nodes[0].InnerText);
							sb.Append(" - ");
							sb.Append(nodes[1].InnerText);
							sb.Append(" - ");
							sb.Append(price.ToString("C"));
						}
						//marketCard.AddOrder((decimal)responseJson["timestamp"]);
					}
				}));
				_threads.Add(thread);
				thread.Start(card);
			}, () =>
			{
				return !string.IsNullOrWhiteSpace(CardIdToAdd);
			});
		}

		public void StopAllThreads()
		{
			foreach (Thread thread in _threads)
			{
				thread.Abort();
			}
		}
	}
}
