using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MQTTnet;

namespace MqttDashboard;

public partial class MainPage : ContentPage
{
	private static readonly string STEAM_TOPIC = "test/topic";


	private readonly IMqttClient mqttClient;
	private readonly ObservableCollection<string> steamHistory = [];

	public MainPage()
	{
		MqttClientFactory factory = new();
		mqttClient = factory.CreateMqttClient();

		InitializeComponent();
		SteamMessages.ItemsSource = steamHistory;
		ConnectToMqttBroker();
	}

	private async void ConnectToMqttBroker()
	{
		try
		{
			MqttClientOptions options = new MqttClientOptionsBuilder()
				.WithTcpServer("localhost", 1883)
				.WithClientId("MauiClient")
				.WithCleanSession()
				.Build();

			mqttClient.ApplicationMessageReceivedAsync += async e =>
			{
				string topic = e.ApplicationMessage.Topic;
				string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

				// Update UI with new message
				MainThread.BeginInvokeOnMainThread(() =>
				{
					if (topic == STEAM_TOPIC) {
						steamHistory.Insert(0, message);
					}
				});
			};

			mqttClient.ConnectedAsync += async e =>
			{
				Console.WriteLine("Connected to MQTT broker!");
				await mqttClient.SubscribeAsync(STEAM_TOPIC);
			};

			mqttClient.DisconnectedAsync += async e =>
			{
				Console.WriteLine("Disconnected from MQTT broker!");
				await mqttClient.ConnectAsync(options);
			};

			await mqttClient.ConnectAsync(options);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"MQTT Error: {ex.Message}");
		}
	}
}

