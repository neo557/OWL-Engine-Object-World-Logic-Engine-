# OWL Engine (Object World Logic Engine)

## 概要

OWL Engine は C# / WPF で構築された軽量 3D ワールド操作エンジンです。

3D グリッド上でのオブジェクト生成・選択・移動・削除を直感的に行えるエディタを備え、

論理ベースの 3D ワールド構築を最小構造で扱う ことを目的としています。

プロトタイピング、ロジック検証、ツール開発の基盤として利用できるよう、

明確な責務分離・拡張性・デバッグ容易性 を重視して設計されています。

## 設計思想（Design Philosophy）

OWL Engine は以下の原則に基づいて設計されています。

###  責務分離（Separation of Concerns）

- WorldController：ワールド状態とオブジェクト管理

- SelectionManager：選択処理・ハイライト処理

- Renderer：描画と Transform パイプライン

- InputHelper：入力抽象化

各コンポーネントが明確に分離されており、拡張やデバッグが容易です。

### 統一 Transform パイプライン

すべてのオブジェクトは

"Scale → Rotate → Translate"

の順で一貫して処理されるように設計されています。

#### 副作用の排除

ハイライト処理などで Transform を破壊しないよう、
Transform3DGroup を固定し、Material のみを差し替える方式を採用。

#### デバッグ容易性

状態遷移が追跡しやすい構造を採用し、
バグの原因特定と再発防止がしやすいように設計されています。

## 未実装機能（追加予定）
- Light block

- MLT installer

- .obj Push

- Events call

##  Features（機能一覧）

### 3D Grid System

- 無限グリッドの描画

- 動的サブディビジョンによる細分化

- マウス座標からの正確なレイキャスト

### Object Interaction

- オブジェクト生成

- マウスピッキングによる選択

- 移動・削除

- ハイライト表示

### Hierarchy Panel

- ワールド内の全オブジェクトを一覧表示

- 追加・削除時に自動更新

### Camera Controls

- オービット

- パン

- ズーム

- スムーズで直感的な操作

### Raycasting System

- グリッド用レイキャスト

-オブジェクト用レイキャスト

- 高精度なヒット判定

### Modular Architecture
- WorldController：ワールドロジック

- SelectionManager：選択処理

- Renderer：描画処理

- InputHelper：入力抽象化

## Installation

コード:

[git clone https://github.com/neo557/OWL_Engine.git
](https://github.com/neo557/OWL-Engine-Object-World-Logic-Engine-.git)

Visual Studio 2022 以降で OWL Engine.slnx を開いてビルドしてください。

## Usage

- Left Click：選択 / 生成

- Right Click：選択解除

- Mouse Drag：カメラ移動

- Scroll Wheel：ズーム

- Delete Key：削除

## Roadmap（今後の予定）

- オブジェクトの回転・スケーリング

- ワールドデータの保存 / 読み込み

- カスタムオブジェクト対応

- マテリアル / カラー編集

- Undo / Redo

- UI 改善

## Changelog

### v1.1.0 – グリッドの全面刷新とオブジェクト回転機能のアップデート

#### グリッドシステムの改善
- 完全に再設計された無限グリッドシステムを実装

- 動的な細分化によるファイングリッドのレンダリングを追加

- グリッドの解像度（gridSize）を調整可能に

- グリッドタイルをワールド座標に合わせるようUVマッピングを修正

- 3D空間におけるグリッドの視認性と安定性を改善

- グリッド平面上の透明度および裏面表示の問題を解決

#### オブジェクト変換の改善
- すべてのオブジェクトにY軸回転を追加

- 統一されたTransform3DGroup構造を確立

 - スケール

 - 回転

 - 平行移動

- ハイライトのオン/オフ切り替え時にも回転が保持されるようにした

- 変換が破棄されないようハイライトシステムを改善

#### 新しい3Dオブジェクトのジオメトリ
- TriangleObjectを平面ポリゴンから3D三角柱へアップグレード

- RectangleObjectを平面四角形から3Dボックスへアップグレード

- オブジェクトに適切な厚みが付与され、あらゆる角度から正しくレンダリングされるようになりました

- 選択精度と視認性を向上

#### レンダリングおよびアーキテクチャの強化
- レンダラー全体でのトランスフォーム処理を整理

- オブジェクト作成パイプラインを改善

- すべてのオブジェクトタイプで一貫した動作を確保

- オブジェクトの可視性とトランスフォームのリセットに関連するいくつかの問題を修正

- 

### v1.0.0 – 初回リリース
- 3Dグリッドのレンダリングを実装

- オブジェクトの作成、選択、移動、削除機能を追加

- 階層パネルを追加

- カメラの軌道制御機能を追加

- グリッドおよびオブジェクトのレイキャスティングを実装

- 選択されたオブジェクトのハイライト表示機能を追加

- モジュール型アーキテクチャを構築（WorldController、SelectionManager、Rendererなど）

## License
MIT License

## Author
Cro (neo557)
Creator of OWL Engine

## English Summary（簡易英語版）

OWL Engine (Object World Logic Engine)

OWL Engine is a lightweight 3D world manipulation tool built with C# and WPF.
It provides an intuitive environment for creating, selecting, moving, and managing objects on a 3D grid.

### Design Philosophy
Separation of Concerns: WorldController / SelectionManager / Renderer

- Unified Transform Pipeline: Scale → Rotate → Translate

- No Side Effects: Highlighting does not alter transforms

- Debug-Friendly Structure: Easy-to-track state transitions

- For detailed design notes (in Japanese), see the full README above.
