using System;
using System.Linq;
using BveTypes.ClassWrappers;

namespace BveTs.DiscordRPC;

/// <summary>
/// BVE の Scenario.Map.Stations と列車位置から現在駅・次駅を判定するクラス
/// </summary>
public class StationTracker
{
	private Station[]? stations;
	private double trainHalfLength;

	/// <summary>
	/// シナリオ読み込み時に駅リストをキャッシュ
	/// </summary>
	/// <param name="scenario">BVE シナリオオブジェクト</param>
	public void LoadStations(Scenario scenario)
	{
		stations = scenario.Map.Stations.Cast<Station>().ToArray();
		
		// 編成長を計算
		double motorCarCount = scenario.Vehicle.Dynamics.MotorCar.Count;
		double trailerCarCount = scenario.Vehicle.Dynamics.TrailerCar.Count;
		double trainLength = (motorCarCount + trailerCarCount) * scenario.Vehicle.Dynamics.CarLength;
		trainHalfLength = trainLength / 2;
	}

	/// <summary>
	/// 駅リストをクリア
	/// </summary>
	public void Clear()
	{
		stations = null;
		trainHalfLength = 0;
	}

	/// <summary>
	/// 列車位置から (現在駅, 次駅) のタプルを返す
	/// </summary>
	/// <param name="location_m">列車位置 (m)</param>
	/// <returns>(現在駅名, 次駅名) のタプル。該当なしの場合は null</returns>
	public (string? currentStation, string? nextStation) GetStationInfo(double location_m)
	{
		if (stations == null || stations.Length == 0)
			return (null, null);

		// 検知半径: trainHalfLength + 50m
		double detectRadius = trainHalfLength + 50;

		string? currentStation = null;
		string? nextStation = null;

		// 現在駅の検出
		for (int i = 0; i < stations.Length; i++)
		{
			var station = stations[i];
			double distance = Math.Abs(station.Location - location_m);
			
			if (distance <= detectRadius)
			{
				currentStation = station.Name;
				
				// 次の停車駅を探す（通過駅はスキップ）
				for (int j = i + 1; j < stations.Length; j++)
				{
					if (!stations[j].Pass)
					{
						nextStation = stations[j].Name;
						break;
					}
				}
				break;
			}
		}

		// 現在駅が見つからなかった場合、次の停車駅を探す
		if (currentStation == null)
		{
			for (int i = 0; i < stations.Length; i++)
			{
				if (stations[i].Location > location_m && !stations[i].Pass)
				{
					nextStation = stations[i].Name;
					break;
				}
			}
		}

		return (currentStation, nextStation);
	}
}
