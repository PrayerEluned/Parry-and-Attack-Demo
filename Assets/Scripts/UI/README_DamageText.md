# ダメージテキストシステム

このシステムは、ゲーム内でダメージや回復の数値をポップアップ表示するためのものです。

## 機能

- ダメージテキストのポップアップ表示
- クリティカルダメージの色分け表示
- 回復の表示（緑色）
- シンプルなアニメーション（上に上がって少し下がって0.3秒後に消える）
- オブジェクトプーリングによる最適化
- ランダムオフセットで重複防止

## セットアップ手順

### 1. プレハブの作成

1. 空のGameObjectを作成
2. `DamageTextSetup`スクリプトをアタッチ
3. インスペクターで「Create Damage Text Prefab」を実行
4. プレハブが`Assets/Prefabs/`に作成されます

### 2. マネージャーの設定

1. 空のGameObjectを作成し、`DamageTextManager`をアタッチ
2. 作成したプレハブを`damageTextPrefab`に設定
3. プールサイズを調整（デフォルト: 20）

### 3. 使用方法

#### 基本的な使用方法

```csharp
// 通常ダメージ
DamageTextHelper.ShowNormalDamage(target, 50);

// クリティカルダメージ
DamageTextHelper.ShowCriticalDamage(target, 100);

// 回復
DamageTextHelper.ShowHeal(target, 30);

// ワールド座標で表示
DamageTextHelper.ShowDamageAtPosition(worldPosition, 75, true);
```

#### 既存の戦闘システムとの統合

1. プレイヤーやエネミーに`DamageTextComponent`をアタッチ
2. ダメージ処理時に以下のメソッドを呼び出し：

```csharp
// ダメージを受けた時
damageTextComponent.OnTakeDamage(damage, isCritical);

// ダメージを与えた時
damageTextComponent.OnDealDamage(damage, isCritical);

// 回復を受けた時
damageTextComponent.OnHeal(healAmount);
```

#### 直接的な使用方法

```csharp
// DamageTextManagerの直接使用
DamageTextManager.ShowDamageOnTarget(target, damage, isCritical, isHeal);
DamageTextManager.ShowDamageAt(worldPosition, damage, isCritical, isHeal);
```

## カスタマイズ

### アニメーション設定

`DamageText`コンポーネントで以下の設定を調整できます：

- `moveUpDistance`: 上に上がる距離
- `moveDownDistance`: 下がる距離
- `totalDuration`: 全体の表示時間（0.3秒）
- `upDuration`: 上がる時間（0.15秒）
- `downDuration`: 下がる時間（0.15秒）

### 色設定

- `normalDamageColor`: 通常ダメージの色
- `criticalDamageColor`: クリティカルダメージの色
- `healColor`: 回復の色

### 表示設定

`DamageTextManager`で以下の設定を調整できます：

- `randomOffsetX`: X軸のランダムオフセット
- `randomOffsetY`: Y軸のランダムオフセット
- `poolSize`: プールサイズ

## 注意事項

1. このシステムはTextMeshProを使用しています
2. カメラが設定されている必要があります
3. プレハブは必ずCanvas内に配置してください
4. プールサイズは同時に表示されるダメージテキストの最大数に合わせて調整してください
5. ダメージテキストは自動的にUIレイヤーに設定され、Sorting Orderは+2に設定されます

## トラブルシューティング

### テキストが表示されない
- カメラが設定されているか確認
- プレハブが正しく設定されているか確認
- Canvasの設定を確認

### アニメーションが重い
- プールサイズを増やす
- 同時表示数を制限する
- アニメーション設定を調整する

### テキストが重なる
- `randomOffsetX`と`randomOffsetY`を増やす
- 表示タイミングを調整する 