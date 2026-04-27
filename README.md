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

コード

git clone https://github.com/neo557/OWL_Engine.git

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

# v1.1.0 – Grid Overhaul & Object Rotation Update

## Grid System Improvements
- Implemented a fully redesigned infinite grid system

- Added fine-grid rendering with dynamic subdivision

- Grid now supports adjustable resolution (gridSize)

- Fixed UV mapping to align grid tiles with world coordinates

- Improved grid visibility and stability in 3D space

- Resolved transparency and backface issues on grid plane

### Object Transform Improvements
- Added Y-axis rotation for all objects

- Established a unified Transform3DGroup structure

 - Scale

 - Rotate

 - Translate

- Ensured rotation persists through highlight/unhighlight

- Improved highlight system to avoid destroying transforms

### New 3D Object Geometry
- TriangleObject upgraded from flat polygon → 3D triangular prism

- RectangleObject upgraded from flat quad → 3D box

- Objects now have proper thickness and render correctly from all angles

- Improved selection accuracy and visual clarity

### Rendering & Architecture Enhancements
- Cleaned up transform handling across renderer

- Improved object creation pipeline

- Ensured consistent behavior across all object types

- Fixed several issues related to object visibility and transform resets
- 
# v1.0.0 – Initial Release
- Implemented 3D grid rendering

- Added object creation, selection, movement, and deletion

- Added hierarchy panel

- Added camera orbit controls

- Implemented grid and object raycasting

- Added highlight system for selected objects

- Established modular architecture (WorldController, SelectionManager, Renderer, etc.)

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
