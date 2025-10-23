# プレイヤー視点追従システム - セットアップガイド

## 概要

プレイヤーの視点（頭）の位置と回転を追従するオブジェクトシステムです。VRCPlayerApiのplayerIdを使用して特定のプレイヤーを追従できます。

## 機能

- プレイヤーの頭部トラッキングをリアルタイム追従
- 位置と回転の個別制御
- スムージング機能
- 位置・回転オフセット
- ボタンでの制御

## ファイル構成

1. **PlayerViewTracker.cs** - メイン追従システム
2. **PlayerViewTrackerButton.cs** - ボタン制御システム

## セットアップ手順

### 1. 基本セットアップ

1. 空のGameObjectを作成し「PlayerViewTracker」と名付ける
2. `PlayerViewTracker`コンポーネントを追加

### 2. 追従オブジェクトの設定

1. 追従させたいオブジェクトを作成（例：カメラ、ライト、UIなど）
2. `PlayerViewTracker`の「Target Object」に設定

### 3. プレイヤーIDの設定

1. 追従したいプレイヤーのVRCPlayerApiのplayerIdを「Target Player Id」に設定

### 4. ボタンの設定（オプション）

1. ボタンオブジェクトを作成
2. `PlayerViewTrackerButton`コンポーネントを追加
3. 設定項目：
   - `Player View Tracker`: PlayerViewTrackerを設定
   - `Target Player Id`: 追従するプレイヤーID
   - `Action`: ボタンの動作（Start/Stop/Toggle）

## 使用方法

### 基本的な使い方

1. オブジェクトにPlayerViewTrackerを設定
2. 追従させたいオブジェクトをTarget Objectに設定
3. プレイヤーIDを設定
4. ボタンで追従開始/停止

### スクリプトからの制御

```csharp
// 追従を開始
playerViewTracker.SetTargetPlayer(playerId);
playerViewTracker.StartTracking();

// 追従を停止
playerViewTracker.StopTracking();

// 追従の切り替え
playerViewTracker.ToggleTracking();

// 追従状態確認
bool isActive = playerViewTracker.IsTrackingActive();
```

## 設定項目

### PlayerViewTracker

#### 追従設定
- **Target Object**: 追従するオブジェクト
- **Target Player**: 追従するプレイヤー（自動設定）
- **Target Player Id**: 追従対象のプレイヤーID（VRCPlayerApiのplayerId）

#### 追従オプション
- **Track Position**: 位置を追従するか
- **Track Rotation**: 回転を追従するか
- **Smoothing**: スムージングの強さ（0-1）
- **Position Offset**: 位置オフセット
- **Rotation Offset**: 回転オフセット

#### 制御
- **Is Tracking Active**: 追従が有効かどうか（読み取り専用）
- **Auto Start Tracking**: 自動的に追従を開始するか

#### デバッグ設定
- **Enable Debug Log**: デバッグログの表示
- **Update Frequency**: 更新頻度（フレーム単位）

### PlayerViewTrackerButton

- **Player View Tracker**: PlayerViewTracker
- **Target Player Id**: 追従対象のプレイヤーID
- **Action**: ボタンの動作（Start/Stop/Toggle）
- **Button Text**: ボタンのテキスト
- **Active Button Text**: 追従中のボタンテキスト
- **Button Text Component**: テキストコンポーネント
- **Button Image Component**: 画像コンポーネント
- **Inactive Color**: 追従停止時の色
- **Active Color**: 追従中の色

## 使用例

### 1. カメラの追従
```csharp
// カメラオブジェクトをプレイヤーの視点に追従させる
playerViewTracker.SetTargetObject(cameraTransform);
playerViewTracker.SetPositionOffset(Vector3.zero);
playerViewTracker.SetRotationOffset(Vector3.zero);
playerViewTracker.StartTracking();
```

### 2. ライトの追従
```csharp
// ライトをプレイヤーの頭上に配置
playerViewTracker.SetTargetObject(lightTransform);
playerViewTracker.SetPositionOffset(new Vector3(0, 0.2f, 0));
playerViewTracker.SetRotationOffset(Vector3.zero);
playerViewTracker.StartTracking();
```

### 3. UIの追従
```csharp
// UIをプレイヤーの前方に配置
playerViewTracker.SetTargetObject(uiTransform);
playerViewTracker.SetPositionOffset(new Vector3(0, 0, 1f));
playerViewTracker.SetRotationOffset(Vector3.zero);
playerViewTracker.StartTracking();
```

## 高度な設定

### スムージング
```csharp
// スムージングを設定（0=瞬時追従、1=追従しない）
playerViewTracker.SetSmoothing(0.1f);
```

### オフセット
```csharp
// 位置オフセットを設定
playerViewTracker.SetPositionOffset(new Vector3(0, 0.5f, 0));

// 回転オフセットを設定
playerViewTracker.SetRotationOffset(new Vector3(0, 180, 0));
```

### 動的なプレイヤー変更
```csharp
// 別のプレイヤーに切り替え
playerViewTracker.SetTargetPlayer(newPlayerId);
```

## 注意事項

1. **プレイヤーID**: VRCPlayerApiのplayerIdを正しく設定する必要があります
2. **オブジェクト**: 追従するオブジェクトが設定されている必要があります
3. **パフォーマンス**: 更新頻度を調整してパフォーマンスを最適化

## トラブルシューティング

### オブジェクトが追従しない
- Target Objectが設定されているか確認
- プレイヤーが実際に参加しているか確認
- Target Player Idが正しいか確認

### 追従が滑らかでない
- Update Frequencyを調整
- Smoothing値を調整
- フレームレートを確認

### ボタンが動作しない
- PlayerViewTrackerが設定されているか確認
- Target Player Idが正しいか確認
- ボタンのAction設定を確認

## ライセンス

このシステムはVRChat用に作成されており、VRChatの利用規約に従って使用してください。
