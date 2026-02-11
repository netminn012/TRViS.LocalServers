using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace BveTs.DiscordRPC;

/// <summary>
/// Discord Rich Presence の更新と管理を行うサービスクラス
/// </summary>
public class DiscordPresenceService : IDisposable
{
	// TODO: Discord Developer Portal でアプリケーションを作成し、実際のアプリケーションIDに置き換えてください
	private const string APPLICATION_ID = "000000000000000000";

	private readonly DiscordRpcClient client;
	private bool disposed;

	public DiscordPresenceService()
	{
		client = new DiscordRpcClient(APPLICATION_ID);
		
		// イベントハンドラの設定
		client.OnReady += (sender, e) =>
		{
			Console.WriteLine($"Discord RPC Ready: {e.User.Username}");
		};

		client.OnConnectionFailed += (sender, e) =>
		{
			Console.WriteLine($"Discord RPC Connection Failed: {e.FailedPipe}");
		};

		client.OnError += (sender, e) =>
		{
			Console.WriteLine($"Discord RPC Error: {e.Message}");
		};

		// ログレベルの設定
		client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

		// 接続開始
		client.Initialize();
	}

	/// <summary>
	/// Rich Presence を更新
	/// </summary>
	/// <param name="data">表示するデータ</param>
	public void UpdatePresence(PresenceData data)
	{
		if (disposed)
			return;

		var presence = new RichPresence
		{
			Details = $"{data.ScenarioName} - {data.SpeedKmh:F0} km/h",
			State = GetStateText(data),
			Assets = new Assets
			{
				LargeImageKey = "bve_logo",
				LargeImageText = $"{data.RouteName} - {data.CarName}",
				SmallImageKey = "train_icon",
				SmallImageText = data.GameTime
			},
			Timestamps = new Timestamps
			{
				Start = data.SessionStartUtc
			}
		};

		client.SetPresence(presence);
	}

	/// <summary>
	/// 状態テキストを生成（駅情報）
	/// </summary>
	private string GetStateText(PresenceData data)
	{
		if (data.CurrentStation != null && data.NextStation != null)
		{
			return $"At {data.CurrentStation} → {data.NextStation}";
		}
		else if (data.CurrentStation != null)
		{
			return $"At {data.CurrentStation}";
		}
		else if (data.NextStation != null)
		{
			return $"Next: {data.NextStation}";
		}
		else
		{
			return $"Location: {data.LocationKm:F2} km";
		}
	}

	/// <summary>
	/// シナリオ未ロード時の待機表示を設定
	/// </summary>
	public void SetIdle()
	{
		if (disposed)
			return;

		var presence = new RichPresence
		{
			Details = "Waiting for scenario...",
			State = "Idle",
			Assets = new Assets
			{
				LargeImageKey = "bve_logo",
				LargeImageText = "BVE Trainsim"
			}
		};

		client.SetPresence(presence);
	}

	/// <summary>
	/// リソースを解放
	/// </summary>
	public void Dispose()
	{
		if (disposed)
			return;

		client.ClearPresence();
		client.Dispose();
		disposed = true;
	}
}
