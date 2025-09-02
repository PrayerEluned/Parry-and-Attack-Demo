## Parry-and-Attack Demo

### 概要
アクションRPGの試作プロジェクト。コアとなる戦闘・成長要素の検証用デモです。

### 使用技術（主なスタック）
- Unity 6（URP）
- C#
- 新 Input System（`PlayerInput` / `InputSystemUIInputModule`）
- UI: UGUI / 一部 UI Toolkit 互換

### ビルド対象
- Windows Standalone
- WebGL（公開用ビルドは別リポジトリでホスティング）

### フォルダ構成（抜粋）
- `AFScripts/` アーティファクト関連
- `EnemyScripts/` 敵AI・攻撃
- `WeaponScripts/` 武器・強化
- `Scripts/` 共通ユーティリティ
- `ScriptableObjects/` 各種データ

### WebGL デモ
公開用リポジトリでホスティングしています。
[Play in your browser](https://PrayerEluned.github.io/Parry-and-Attack-Demo-webGL/)

### ライセンス / 注意
本リポジトリ内のコード・アセットはプロトタイプ用途です。外部アセットの権利は各作者に帰属します。
