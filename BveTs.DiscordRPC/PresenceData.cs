using System;

namespace BveTs.DiscordRPC;

/// <summary>
/// Discord Rich Presence に表示する情報をまとめたデータクラス
/// </summary>
public class PresenceData
{
	/// <summary>路線名</summary>
	public string RouteName { get; set; } = string.Empty;

	/// <summary>シナリオ名/列番</summary>
	public string ScenarioName { get; set; } = string.Empty;

	/// <summary>車両名</summary>
	public string CarName { get; set; } = string.Empty;

	/// <summary>現在位置 (km)</summary>
	public double LocationKm { get; set; }

	/// <summary>現在速度 (km/h)</summary>
	public double SpeedKmh { get; set; }

	/// <summary>ゲーム内時刻文字列</summary>
	public string GameTime { get; set; } = string.Empty;

	/// <summary>現在駅名 (nullable)</summary>
	public string? CurrentStation { get; set; }

	/// <summary>次駅名 (nullable)</summary>
	public string? NextStation { get; set; }

	/// <summary>セッション開始時刻 (UTC)</summary>
	public DateTime SessionStartUtc { get; set; }
}
