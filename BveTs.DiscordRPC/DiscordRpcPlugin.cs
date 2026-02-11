using System;
using BveEx.PluginHost.Plugins;
using BveEx.PluginHost.Plugins.Extensions;
using BveTypes.ClassWrappers;

namespace BveTs.DiscordRPC;

/// <summary>
/// BVE Trainsim の運転情報を Discord Rich Presence に表示する BveEX プラグイン
/// </summary>
[Plugin(PluginType.Extension)]
public partial class DiscordRpcPlugin : PluginBase, IExtension
{
	private readonly DiscordPresenceService discordService;
	private readonly StationTracker stationTracker;
	private bool wasScenarioLoaded;
	private DateTime sessionStartUtc;
	private TimeSpan timeSinceLastUpdate = TimeSpan.Zero;
	private const double UPDATE_INTERVAL_SECONDS = 15.0; // Discord Rate Limit 対策

	public DiscordRpcPlugin(PluginBuilder builder) : base(builder)
	{
		discordService = new DiscordPresenceService();
		stationTracker = new StationTracker();
		wasScenarioLoaded = false;
		
		// 初期状態を待機中に設定
		discordService.SetIdle();
	}

	public override void Tick(TimeSpan elapsed)
	{
		timeSinceLastUpdate += elapsed;

		bool isScenarioLoaded = BveHacker.IsScenarioCreated;

		// シナリオ読み込み検知
		if (isScenarioLoaded && !wasScenarioLoaded)
		{
			OnScenarioLoaded();
		}
		// シナリオ終了検知
		else if (!isScenarioLoaded && wasScenarioLoaded)
		{
			OnScenarioUnloaded();
		}

		wasScenarioLoaded = isScenarioLoaded;

		// 15秒間隔で更新（Discord Rate Limit 対策）
		if (isScenarioLoaded && timeSinceLastUpdate.TotalSeconds >= UPDATE_INTERVAL_SECONDS)
		{
			UpdatePresence();
			timeSinceLastUpdate = TimeSpan.Zero;
		}
	}

	private void OnScenarioLoaded()
	{
		try
		{
			Scenario scenario = BveHacker.Scenario;
			stationTracker.LoadStations(scenario);
			sessionStartUtc = DateTime.UtcNow;
			
			// すぐに更新
			UpdatePresence();
			timeSinceLastUpdate = TimeSpan.Zero;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Discord RPC: Error loading scenario - {ex.Message}");
		}
	}

	private void OnScenarioUnloaded()
	{
		stationTracker.Clear();
		discordService.SetIdle();
	}

	private void UpdatePresence()
	{
		try
		{
			Scenario scenario = BveHacker.Scenario;
			ScenarioInfo scenarioInfo = BveHacker.ScenarioInfo;

			// 列車位置と速度を取得
			double location_m = scenario.VehicleLocation.Location;
			double speed_ms = scenario.VehicleLocation.Speed;
			double speedKmh = speed_ms * 3.6;

			// ゲーム内時刻を取得
			long timeMs = scenario.TimeManager.TimeMilliseconds;
			TimeSpan gameTime = TimeSpan.FromMilliseconds(timeMs);
			string gameTimeStr = $"{(int)gameTime.TotalHours:D2}:{gameTime.Minutes:D2}:{gameTime.Seconds:D2}";

			// 駅情報を取得
			var (currentStation, nextStation) = stationTracker.GetStationInfo(location_m);

			// PresenceData を構築
			var presenceData = new PresenceData
			{
				RouteName = scenarioInfo.RouteTitle,
				ScenarioName = scenarioInfo.Title,
				CarName = scenarioInfo.VehicleTitle,
				LocationKm = location_m / 1000.0,
				SpeedKmh = speedKmh,
				GameTime = gameTimeStr,
				CurrentStation = currentStation,
				NextStation = nextStation,
				SessionStartUtc = sessionStartUtc
			};

			discordService.UpdatePresence(presenceData);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Discord RPC: Error updating presence - {ex.Message}");
		}
	}

	public override void Dispose()
	{
		discordService.Dispose();
	}
}
