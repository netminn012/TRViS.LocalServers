# BveTs.DiscordRPC

BVE Trainsim の運転情報を Discord Rich Presence に表示する、独立した BveEX 拡張プラグインです。

## 特徴

- BveEX の API (`BveHacker`) から直接データを取得
- TRViS.LocalServers の他のコンポーネントには一切依存しない完全独立プラグイン
- Discord に以下の情報を表示:
  - 路線名
  - シナリオ名（列番）
  - 車両名
  - 現在速度
  - 現在位置
  - 現在駅・次駅
  - ゲーム内時刻
  - セッション開始からの経過時間

## Discord 上の表示例

```
Discord Rich Presence
━━━━━━━━━━━━━━━━━━━
🚊 BVE Trainsim

各駅停車 1001A - 65 km/h
At 東京 → 品川

路線: 東海道本線 - 車両: E231系
ゲーム内時刻: 09:30:15
経過時間: 00:15:30
```

## セットアップ

### 1. Discord Developer Portal でアプリケーションを作成

1. [Discord Developer Portal](https://discord.com/developers/applications) にアクセス
2. "New Application" をクリックしてアプリケーションを作成
3. アプリケーション名を入力（例: "BVE Trainsim"）
4. Application ID をコピー

### 2. Art Assets の登録（オプション）

Rich Presence に画像を表示するには:

1. Discord Developer Portal のアプリケーション設定で "Rich Presence" → "Art Assets" に移動
2. 以下のキー名で画像をアップロード:
   - `bve_logo` - BVE のロゴやメインイメージ
   - `train_icon` - 列車アイコン

### 3. APPLICATION_ID の差し替え

`DiscordPresenceService.cs` の以下の行を編集:

```csharp
private const string APPLICATION_ID = "000000000000000000";
```

コピーした Application ID に置き換えます:

```csharp
private const string APPLICATION_ID = "123456789012345678";
```

### 4. ビルドとインストール

```bash
dotnet publish ./BveTs.DiscordRPC/BveTs.DiscordRPC.csproj -f net48 -c Release -o ./out
```

生成された DLL を BVE の BveEX プラグインフォルダにコピーします。

## ビルド方法

### ローカルビルド

```bash
cd BveTs.DiscordRPC
dotnet restore
dotnet build -c Release
```

### GitHub Actions

このプロジェクトは GitHub Actions による自動ビルドに対応しています:

- ワークフロー: `.github/workflows/build-discord-rpc.yaml`
- トリガー: `main` ブランチへの push、pull request
- 成果物: ビルドされた DLL が Artifacts としてアップロードされます

## 技術的な動作説明

### アーキテクチャ

```
DiscordRpcPlugin (main)
├── DiscordPresenceService (Discord IPC 通信)
├── StationTracker (駅情報管理)
└── PresenceData (データモデル)
```

### 更新間隔

Discord API の Rate Limit に配慮し、15秒間隔で更新を行います。

### 駅検知ロジック

- 列車位置から ±(編成長/2 + 50m) の範囲内に駅があれば「現在駅」と判定
- 通過駅 (`station.Pass = true`) は次駅候補から除外
- 現在駅が見つからない場合、前方の最初の停車駅を「次駅」として表示

## 依存関係

- **BveEx.PluginHost** 2.0.0 - BveEX プラグインフレームワーク
- **BveEx.CoreExtensions** 2.0.0 - BveEX コア拡張
- **DiscordRichPresence** 1.2.1.24 - Discord RPC クライアント (discord-rpc-csharp)

## ライセンス

MIT License

Copyright (c) 2024 Tetsu Otter

詳細は [LICENSE](../LICENSE) を参照してください。
