# 突きのモーション攻撃システム

## 概要

このシステムは、敵の攻撃をスプライトベースの動きパターンで実現する新しい攻撃システムです。従来の赤い範囲円による警告表示や複雑な攻撃判定システムを廃止し、シンプルで直感的な攻撃システムを提供します。

## 主要機能

### 1. 突きのモーション
- **引く動作**: プレイヤーから遠ざかる
- **停止**: 一定時間停止
- **突く動作**: プレイヤー側にまっすぐ進む（イージング付き）
- **ついた後の停止**: 攻撃判定ありの状態で停止

### 2. スプライトベースの攻撃判定
- 攻撃スプライトのアルファ値が0でない部分に当たり判定
- 敵の`sorting order + 1`で表示
- 物理判定なし（プレイヤーの移動を阻害しない）

### 3. 攻撃パターン管理
- ScriptableObjectによる設定管理
- インスペクターでの詳細調整
- ランダム選択による攻撃パターンの多様化

### 4. 統合された機能
- 音響効果（攻撃開始時、判定時）
- ビジュアルエフェクト
- アニメーション連携
- デバッグ機能

## セットアップ手順

### 1. 敵オブジェクトの準備

#### 基本構造
```
Enemy (GameObject)
├── EnemyController
├── EnemyHealth
├── EnemyAttackSystem
├── EnemyUIController
└── ThrustAttackMovement (子オブジェクト)
    └── SpriteRenderer (攻撃スプライト用)
```

#### 必要なコンポーネント
1. **EnemyController**: 敵の基本行動制御
2. **EnemyHealth**: 敵のHP管理
3. **EnemyAttackSystem**: 攻撃システム管理
4. **EnemyUIController**: UI管理
5. **ThrustAttackMovement**: 突きのモーション制御

### 2. ThrustAttackMovementの設定

#### 基本設定
- **引く動作設定**: 距離、速度
- **停止設定**: 停止時間
- **突く動作設定**: 距離、速度、加速時間
- **ついた後の停止設定**: 停止時間
- **攻撃判定設定**: 攻撃力、範囲

#### スプライト設定
- **Attack Sprite**: 攻撃に使用するスプライト
- **Attack Sprite Color**: スプライトの色

#### 音響・エフェクト設定
- **Attack Start Sound**: 攻撃開始時の音
- **Attack Hit Sound**: 攻撃判定時の音
- **Attack Start Effect**: 攻撃開始時のエフェクト
- **Attack Hit Effect**: 攻撃判定時のエフェクト

#### アニメーション設定
- **Attack Start Trigger**: 攻撃開始時のアニメーショントリガー
- **Attack Hit Trigger**: 攻撃判定時のアニメーショントリガー

### 3. 攻撃パターンの作成

#### ScriptableObjectの作成
1. Project ウィンドウで右クリック
2. Create → Enemy → Thrust Attack Pattern
3. パラメータを調整

#### 推奨設定値
```
基本設定:
- 攻撃力: 10-30
- 攻撃範囲: 1.0-2.0
- クールダウン: 2-5秒

引く動作:
- 距離: 1.0-1.5
- 速度: 2.0-3.0

停止:
- 時間: 0.3-0.5秒

突く動作:
- 距離: 3.0-4.0
- 速度: 5.0-6.0
- 加速: 0.3-0.5

ついた後の停止:
- 時間: 0.5-1.0秒
```

### 4. システムの統合

#### EnemyAttackSystemの設定
- **Available Attacks**: ThrustAttackMovementのリスト
- **Attack Cooldown**: 攻撃のクールダウン時間
- **Use Random Attacks**: ランダム選択の有効/無効

#### EnemyControllerの設定
- **Detect Range**: プレイヤー検出範囲
- **Attack Range**: 攻撃開始範囲
- **Show Debug Info**: デバッグ情報の表示

## 使用方法

### 1. 基本的な攻撃

#### 自動攻撃
- プレイヤーが攻撃範囲内に入ると自動的に攻撃開始
- 設定されたクールダウン時間後に次の攻撃が可能

#### 手動攻撃
```csharp
// 攻撃システムの参照を取得
EnemyAttackSystem attackSystem = GetComponent<EnemyAttackSystem>();

// 攻撃を開始
if (attackSystem.CanAttack)
{
    attackSystem.StartAttack();
}
```

### 2. 攻撃パターンの動的変更

#### パターンの適用
```csharp
// ThrustAttackMovementの参照を取得
ThrustAttackMovement movement = GetComponentInChildren<ThrustAttackMovement>();

// 攻撃パターンを適用
ThrustAttackPattern pattern = Resources.Load<ThrustAttackPattern>("AttackPatterns/NewPattern");
movement.ApplyPattern(pattern);
```

#### パターンの追加/削除
```csharp
// 攻撃システムの参照を取得
EnemyAttackSystem attackSystem = GetComponent<EnemyAttackSystem>();

// 新しい攻撃パターンを追加
ThrustAttackMovement newAttack = CreateNewAttack();
attackSystem.AddAttackPattern(newAttack);

// 攻撃パターンを削除
attackSystem.RemoveAttackPattern(oldAttack);
```

### 3. カスタマイズ

#### 新しい動きパターンの作成
1. `ThrustAttackMovement`を継承した新しいクラスを作成
2. 独自の移動ロジックを実装
3. `EnemyAttackSystem`に登録

#### エフェクトのカスタマイズ
- プレハブエフェクトの作成
- 音声ファイルの設定
- アニメーションの調整

## デバッグ機能

### 1. インスペクターでの設定

#### デバッグ設定
- **Show Debug Info**: コンソールログの表示
- **Show Attack Hitbox**: 攻撃範囲の可視化

#### リアルタイム調整
- 各パラメータのスライダー調整
- 即座に反映される設定変更

### 2. シーンビューでの可視化

#### Gizmos表示
- **黄色の円**: 検出範囲
- **赤い円**: 攻撃範囲
- **青い線**: プレイヤーへの方向
- **青い点**: 引く位置
- **赤い点**: 突く目標位置

### 3. コンソールログ

#### 情報ログ
- 攻撃開始/完了
- 状態変更
- パラメータ適用
- エラー情報

## パフォーマンス最適化

### 1. 推奨設定

#### オブジェクト管理
- 攻撃スプライトの事前ロード
- エフェクトのオブジェクトプーリング
- 不要なコンポーネントの削除

#### 更新頻度
- 攻撃中のみの更新処理
- フレーム単位での当たり判定
- 効率的な状態管理

### 2. メモリ使用量

#### 最適化ポイント
- スプライトの適切なサイズ
- 音声ファイルの圧縮
- エフェクトの軽量化

## トラブルシューティング

### 1. よくある問題

#### 攻撃が開始されない
- `ThrustAttackMovement`が子オブジェクトに存在するか確認
- `EnemyAttackSystem`の設定を確認
- プレイヤーのタグが"Player"になっているか確認

#### 攻撃判定が動作しない
- スプライトのアルファ値設定を確認
- 攻撃範囲の設定値を確認
- プレイヤーの`PlayerStats`コンポーネントの存在確認

#### スプライトが表示されない
- `SpriteRenderer`の設定を確認
- スプライトファイルの割り当て確認
- `sorting order`の設定確認

### 2. デバッグ手順

1. **コンソールログの確認**: エラーメッセージの確認
2. **インスペクターの確認**: 各コンポーネントの設定確認
3. **シーンビューの確認**: Gizmosによる可視化確認
4. **ステップ実行**: ブレークポイントによる動作確認

## 拡張性

### 1. 新しい動きパターン

#### 実装方法
1. 基底クラスの作成
2. 移動ロジックの実装
3. パラメータの定義
4. システムへの統合

#### 例：円運動攻撃
- 中心点を軸とした円運動
- 半径と角速度の設定
- 回転方向の制御

### 2. 複合攻撃パターン

#### 実装方法
1. 複数の動きパターンの組み合わせ
2. タイミング制御の実装
3. 状態管理の拡張

#### 例：連続突き攻撃
- 複数回の突き動作
- 各突きの間隔制御
- 方向の変化

## まとめ

この突きのモーション攻撃システムは、従来の複雑な攻撃システムを大幅に簡素化し、直感的で柔軟な攻撃システムを提供します。ScriptableObjectによる設定管理、インスペクターでの詳細調整、豊富なカスタマイズオプションにより、開発効率とゲームプレイの品質を向上させることができます。

### 主な利点
- **シンプルな実装**: 複雑な警告表示システムの廃止
- **柔軟な設定**: インスペクターでの詳細調整
- **拡張性**: 新しい動きパターンの簡単追加
- **パフォーマンス**: 効率的な更新処理
- **デバッグ性**: 豊富な可視化とログ機能

### 今後の発展
- より多様な動きパターンの追加
- AIによる動的パターン選択
- プレイヤーの行動に応じた適応的攻撃
- マルチプレイヤー対応
