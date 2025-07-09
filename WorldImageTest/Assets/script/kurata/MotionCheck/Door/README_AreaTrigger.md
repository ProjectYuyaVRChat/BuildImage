# ドア範囲検知システム 使用方法

## 概要
このシステムは、特定の範囲に人が入ったら、その範囲に紐づけされたドアだけがモーション検知を開始する機能を提供します。

## システム構成

### 1. DoorAreaTrigger
- **役割**: 特定の範囲に人が入ったかどうかを検知
- **機能**: 
  - プレイヤーの入退場を検知
  - 範囲内のプレイヤー数を管理
  - ドアシステムのアクティブ/非アクティブを制御

### 2. DoorGimmickSystemNew (拡張版)
- **新機能**: 範囲検知システムとの連携
- **設定項目**:
  - `useAreaTrigger`: 範囲検知システムを使用するかどうか
  - `areaTrigger`: 連携するDoorAreaTriggerの参照

### 3. モーション検出器 (拡張版)
- **新機能**: 範囲検知システムとの連携
- **設定項目**: `areaTrigger` - 連携するDoorAreaTriggerの参照

## セットアップ手順

### 1. 範囲エリアの作成
1. 空のGameObjectを作成
2. `DoorAreaTrigger`スクリプトを追加
3. Collider (Trigger) を追加して範囲を設定
4. 以下の設定を行う:
   - `targetDoorSystem`: 制御するドアシステムを設定
   - `areaName`: エリアの名前を設定
   - `showDebugInfo`: デバッグ情報の表示設定

### 2. ドアシステムの設定
1. `DoorGimmickSystemNew`の設定で以下を有効にする:
   - `useAreaTrigger`: true
   - `areaTrigger`: 作成したDoorAreaTriggerを設定

### 3. モーション検出器の設定
1. 各モーション検出器で以下を設定:
   - `areaTrigger`: 同じDoorAreaTriggerを設定
   - `doorGimmickSystem`: 対応するドアシステムを設定

## 動作フロー

### プレイヤーが範囲に入った時
1. `DoorAreaTrigger.OnPlayerTriggerEnter()`が呼ばれる
2. プレイヤーが範囲内プレイヤーリストに追加される
3. 最初のプレイヤーの場合、`ActivateArea()`が呼ばれる
4. `DoorGimmickSystemNew.SetAreaActive(true)`が呼ばれる
5. ドアシステムがアクティブになり、モーション検知が開始される

### プレイヤーが範囲から出た時
1. `DoorAreaTrigger.OnPlayerTriggerExit()`が呼ばれる
2. プレイヤーが範囲内プレイヤーリストから削除される
3. 最後のプレイヤーの場合、`DeactivateArea()`が呼ばれる
4. `DoorGimmickSystemNew.SetAreaActive(false)`が呼ばれる
5. ドアシステムが非アクティブになり、モーション検知が停止される

## 設定例

### 基本的な設定
```
DoorAreaTrigger:
├── targetDoorSystem: DoorGimmickSystemNew
├── areaName: "エントランスドア"
└── showDebugInfo: true

DoorGimmickSystemNew:
├── useAreaTrigger: true
├── areaTrigger: DoorAreaTrigger
└── その他のドア設定...

モーション検出器:
├── areaTrigger: DoorAreaTrigger
├── doorGimmickSystem: DoorGimmickSystemNew
└── その他の検出設定...
```

### 複数ドアの設定
```
エリア1 (DoorAreaTrigger):
├── targetDoorSystem: DoorGimmickSystemNew_1
└── areaName: "ドア1エリア"

エリア2 (DoorAreaTrigger):
├── targetDoorSystem: DoorGimmickSystemNew_2
└── areaName: "ドア2エリア"

各ドアシステム:
├── useAreaTrigger: true
├── areaTrigger: 対応するDoorAreaTrigger
└── 個別のドア設定...
```

## 注意事項

1. **Collider設定**: DoorAreaTriggerには必ずTrigger設定されたColliderが必要です
2. **参照設定**: 各コンポーネント間の参照を正しく設定してください
3. **プレイヤー数制限**: `maxPlayersInArea`で範囲内の最大プレイヤー数を制限できます
4. **デバッグ**: `showDebugInfo`を有効にすると動作状況を確認できます

## トラブルシューティング

### ドアが反応しない
- `useAreaTrigger`が有効になっているか確認
- `areaTrigger`の参照が正しく設定されているか確認
- 範囲内にプレイヤーがいるか確認

### 複数のドアが同時に反応する
- 各ドアシステムが異なる`DoorAreaTrigger`を参照しているか確認
- エリアが重複していないか確認

### モーション検知が停止しない
- プレイヤーが範囲外に出ているか確認
- `areaTrigger`の設定が正しいか確認 