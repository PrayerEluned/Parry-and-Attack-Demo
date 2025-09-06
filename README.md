## Parry-and-Attack Demo

### 概要
アクションRPGの試作プロジェクト。コアとなる戦闘・成長要素の検証用デモです。

### 重要なお知らせ（開発中）
- 現状は開発途中のため、UI/挙動は随時更新中です。表示やバランスが不完全な箇所があります。

### 簡易プレイガイド（操作と進め方）
スマホ(PC)の場合
- 左上のボタンを押して表示される各ボタンの役割
  - 剣: 攻撃 ( J )
  - 盾: 防御 ( L )
  - 右端の火花: パリィ（タイミング良く押すと有効） (space)
- ボタンが表示されていない時は、自動攻撃が行われます
- 画面タップ（ WASD ）で移動が可能
- 敵を倒して入手したアイテム（素材）で、画面左側にいる NPC から強化が可能
- 画面上側にいる NPC のステータスから、3枠あるスロットに装備を設定可能

-攻撃ボタン J

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
