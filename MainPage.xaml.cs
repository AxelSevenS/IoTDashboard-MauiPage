﻿using System.Text;
using MQTTnet;

namespace MqqtDashboard;

public partial class MainPage : ContentPage
{
	private readonly IMqttClient mqttClient;

	public MainPage()
	{
		MqttClientFactory factory = new();
		mqttClient = factory.CreateMqttClient();

		InitializeComponent();
		ConnectToMqttBroker();
	}

	private async void ConnectToMqttBroker()
	{
		try
		{
			var options = new MqttClientOptionsBuilder()
				.WithTcpServer("localhost", 1883) // Replace with your broker's IP/hostname if needed
				.WithClientId("MauiClient")
				.WithCleanSession()
				.Build();

			mqttClient.ApplicationMessageReceivedAsync += async e =>
			{
				string message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
				string topic = e.ApplicationMessage.Topic;

				// Update UI with new message
				MainThread.BeginInvokeOnMainThread(() =>
				{
					MessagesLabel.Text += $"\n[{topic}]: {message}";
				});
			};

			mqttClient.ConnectedAsync += async e =>
			{
				Console.WriteLine("Connected to MQTT broker!");
				await mqttClient.SubscribeAsync("test/topic"); // Subscribe to a test topic
			};

			await mqttClient.ConnectAsync(options);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"MQTT Error: {ex.Message}");
		}
	}
}

