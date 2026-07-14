# Cat Museum Stealth

**Cat Museum Stealth** は、2頭身のかわいい猫怪盗が美術館に潜入し、美術品を盗んで脱出する3Dステルスゲームです。

就活向けポートフォリオ作品として、Unityの3D移動、インタラクト、AI、ステージ遷移、ScriptableObjectによるデータ管理、バックパック型インベントリ、UI、NavMeshを使った警備員AIなどを見せられる構成を目指しています。

現在はプロトタイプ段階ですが、以下の大きな流れは動作する状態です。

```text
MainMenuで作戦準備
-> バックパックを購入・装備
-> ダミーや補助アイテムをバックパックに配置
-> Museum Mapへ出発
-> 美術品を盗む / ダミーと交換する
-> 盗品をバックパックへ自動収納
-> 警備員に見つからないように出口へ向かう
-> 盗品を持って脱出するとClear
```

---

## 開発環境

```text
Engine: Unity
Render Pipeline: Universal Render Pipeline
Language: C#
Main Genre: 3D Stealth / Inventory Puzzle
```

---

## ゲームコンセプト

プレイヤーは猫型の怪盗です。

美術館には複数の展示品が置かれており、警備員が巡回しています。プレイヤーは展示品を盗めますが、展示台が空になると警備員に発見されやすくなります。

そこで、作戦前にバックパックへ持ち込んだダミーを使い、本物の美術品と入れ替えることで発覚を遅らせます。

重要な判断は以下です。

```text
どの美術品を狙うか
どのダミーを持ち込むか
バックパック容量をどう使うか
強引に盗むか、ダミー交換するか
盗んだあと逃げ切れるか
```

かわいい見た目に対して、内部システムはステルス、所持品管理、警戒度、警備員AIが絡む構成です。

---

## 現在のシーン構成

### MainMenu

作戦準備用のシーンです。

現在の役割:

```text
バックパックUIを開く
アイテムを購入する
バックパックを装備する
ダミーや補助アイテムをバックパックに配置する
選択中の美術館マップへ出発する
```

### Map_01_Museum

美術館本編のテストマップです。

現在の役割:

```text
プレイヤー移動
警備員巡回
美術品の窃盗
ダミー交換
盗品のバックパック自動収納
警備員による発見・追跡
出口から脱出してClear
捕まるとGame Over
```

---

## 操作方法

### 共通

```text
WASD        移動
Mouse       カメラ操作
Esc         バックパックUIを閉じる
```

### MainMenu

```text
E           近くのステーションを調べる
Left Click  UI上の購入・配置操作
```

### 美術館ステージ

```text
WASD        移動
Mouse       カメラ操作
Left Shift  ダッシュ
E           ダミー交換 / 空展示台へダミー設置
F           強奪 / 置かれたダミー回収
Tab         装備中バックパックを開く
R           Clear / Game Over後にMainMenuへ戻る
```

### バックパックUI

```text
Left Click             アイテム選択 / 詳細表示
Left Drag              アイテムを配置・移動
Right Click            配置済みアイテムを外す
R                      ドラッグ中アイテムを回転
Esc / Close Button     UIを閉じる
```

ミッション中のバックパックでは、アイテムの並べ替えはできますが、捨てる・購入する・バックパックを外す操作はできない想定です。

---

## 実装済みシステム

### 1. PlayerProfile

`PlayerProfile` はプレイヤーの準備状態を保持する中心データです。

保持しているもの:

```text
所持金
バックパック装備状態
バックパックの横幅・縦幅
購入済みアイテム
バックパック内に配置済みのアイテム
選択中のマップ名
```

主な処理:

```text
アイテム購入
アイテム所持数管理
バックパックへの配置
配置済みアイテムの移動
配置済みアイテムの削除
バックパック空き判定
盗品の自動収納
```

盗品は `ArtData` から一時的な `BackpackItemData` を生成して、バックパック内へ配置します。これにより、盗品ごとに個別の見た目や説明を持たせられます。

### 2. BackpackItemData

バックパックに入る通常アイテム用のデータです。

対象:

```text
ダミー
補助アイテム
盗品として生成された一時アイテム
```

主な項目:

```text
itemName
displayName
infoText
itemType
width
height
canRotate
price
icon
modelPrefab
modelLocalPosition
modelLocalRotationEuler
modelLocalScale
spinAxis
spinSpeed
linkedArtData
supportType
```

`displayName` と `infoText` は、バックパック上でクリックしたときの詳細表示に使います。

### 3. ArtData

美術品そのもののデータです。

主な項目:

```text
artName
category
size
value
suspicionWhenStolen
suspicionWhenSwapped
backpackWidth
backpackHeight
backpackCanRotate
backpackDisplayName
backpackInfoText
backpackIcon
backpackModelPrefab
backpackModelLocalPosition
backpackModelLocalRotationEuler
backpackModelLocalScale
backpackSpinAxis
backpackSpinSpeed
```

盗んだ美術品は、この `ArtData` のバックパック用設定を使って、バックパック内に表示されます。

以前は `BP_Loot_...` のような盗品専用データを別に作る案でしたが、現在は **ArtDataに盗品表示情報を持たせる方式** に変更しています。

### 4. BackpackMenuUI

バックパック画面全体を制御します。

実装済み:

```text
MainMenuでの購入
バックパック装備 / 解除
アイテム一覧表示
ドラッグで配置
Rキーで回転
配置済みアイテムの移動
右クリックで取り外し
ミッション中の購入・削除・装備解除禁止
クリック時のアイテム詳細表示
```

クリックとドラッグは分けています。

```text
短いクリック       詳細表示
一定距離以上ドラッグ アイテム移動開始
```

詳細表示の内容:

```text
Icon
Name Text
Grid占有数
Category
Size
Info Text
```

詳細パネルは、3Dボードの上側に表示する想定です。

### 5. Backpack3DBoard

Main Camera配下に置く3Dバックパック盤面です。

特徴:

```text
チェス盤風の3Dグリッド
BackpackItemDataのサイズに応じた占有表示
モデルPrefabがあればそれを表示
モデルPrefabがなければ簡易モデルを生成
アイコンマーカーを盤面上に表示
クリック / ドラッグ判定はアイコンマーカーのみ
ドラッグ中は緑 / 赤の配置プレビューを表示
配置済みアイテムの見た目だけ回転
```

現在は、アイテムの3Dモデルそのものではなく、盤面上のアイコンマーカーに当たり判定を置いています。これにより、見た目のモデルが大きかったり複雑だったりしても、操作判定が安定します。

### 6. ArtPiece

展示品オブジェクトの状態と盗み処理を管理します。

状態:

```text
Real   本物が置かれている
Dummy  ダミーが置かれている
Empty  展示台が空
```

実装済みの挙動:

```text
Eで対応ダミーと交換
Fで強引に盗む
置かれたダミーをFで回収
空展示台にEでダミー設置
盗んだ美術品をバックパックへ自動収納
バックパックが満杯なら盗めない
バックパック未装備なら盗めない
展示状態に応じて見た目を切り替える
```

### 7. PlayerInteractor

プレイヤーが近くの展示品を操作するための処理です。

現在は、Raycastではなく近くのインタラクト対象を探す方式を採用しています。これにより、展示品に近づきすぎたときにRayが当たらない問題を避けています。

### 8. PlayerInventory

本編中の簡易所持品管理です。

現在は、ダミー交換や盗品管理の一部で使用しています。バックパックの3D配置情報は `PlayerProfile` 側で管理します。

### 9. RuntimeBackpackInput

ミッション中に `Tab` でバックパックUIを開くための入力処理です。

条件:

```text
PlayerProfileが存在する
バックパックを装備している
BackpackMenuUIが参照されている
```

バックパック未装備の場合は開けません。

### 10. PlayerBackpackVisual

バックパック装備中に、プレイヤーの背中へバックパックモデルを表示するための処理です。

装備状態に応じて、背中のバックパック表示をON/OFFします。

### 11. Guard AI

警備員AIは以下の状態を持ちます。

```text
Patrol  巡回
Chase   追跡
Search  捜索
```

関連スクリプト:

```text
GuardController
GuardVision
GuardPatrol
GuardMemoryNetwork
```

実装済みの方向性:

```text
巡回ポイントに沿って移動
視界内のプレイヤーを発見
盗み作業中のプレイヤーを発見すると追跡
記憶済みのプレイヤーを見つけると追跡
見失ったら最後に見た位置を捜索
一定時間後に他の警備員へ情報共有
追跡中のプレイヤーを他の警備員が見ると追跡に参加
```

### 12. AlertManager

警戒レベルを管理します。

現在の段階:

```text
0-24    Normal
25-49   Middle
50-79   High
80-100  Maximum
```

空の展示台を警備員が見つけると、警戒レベルが上昇します。

---

## バックパック仕様

現在の基本サイズ:

```text
Width: 8
Height: 5
Total: 40 cells
```

現在登録している通常アイテム:

```text
BP_Dummy_SmallPainting
BP_Dummy_MediumPainting
BP_Dummy_LargePainting
BP_Dummy_SmallSculpture
BP_Dummy_MediumSculpture
BP_Dummy_LargeSculpture
BP_MouseToy
BP_SmokeBomb
```

### ダミーの役割

ダミーは、美術品と同じ `category` と `size` を持つ場合に交換できます。

例:

```text
Medium Paintingの美術品
-> Medium Painting Dummyで交換可能
```

### 補助アイテム

現在ある補助アイテム:

```text
Mouse Toy
Smoke Bomb
```

現時点ではデータとバックパック配置が中心です。実際のミッション中効果は今後実装予定です。

### 盗品

盗品は専用の `BP_Loot` アセットを作らず、`ArtData` から実行時に `BackpackItemData` を生成します。

利点:

```text
美術品ごとに個別のアイコンを設定できる
美術品ごとに個別の3Dモデルを設定できる
ArtDataだけ見ればバックパック表示も確認できる
BP_Loot系アセットが増えすぎない
```

---

## 現在登録している美術品データ

```text
Art_TinyFish
Art_MoonCat
Art_RoyalCatPortrait
Art_Goldenyarn
Art_SleepingCat
Art_BigCatStatue
```

各 `ArtData` には、通常の美術品情報に加えて、バックパック内表示用の情報も設定できます。

```text
Backpack Width
Backpack Height
Backpack Can Rotate
Backpack Display Name
Backpack Info Text
Backpack Icon
Backpack Model Prefab
Backpack Model Local Position
Backpack Model Local Rotation Euler
Backpack Model Local Scale
Backpack Spin Axis
Backpack Spin Speed
```

現在、バックパック内モデルの調整値は、おおむね以下を基準にしています。

```text
Backpack Model Local Position: 0, 1, 0
Backpack Model Local Scale:    0.1, 0.1, 0.1
```

---

## クリア条件

盗品を持った状態で出口エリアに入るとClearです。

```text
美術品を盗む
-> 出口へ向かう
-> ExitZoneに入る
-> Clear
```

クリア時には盗品価値に応じて報酬を得る想定です。

---

## ゲームオーバー条件

警備員に追いつかれるとGame Overです。

```text
警備員に見つかる
-> 追跡される
-> 一定距離まで接近される
-> Game Over
```

Clear / Game Over後は `R` キーでMainMenuへ戻れます。

---

## 主なフォルダ構成

```text
Assets/CatMuseum
├── Scenes
│   ├── MainMenu
│   └── Maps
├── Scripts
│   ├── Core
│   ├── Data
│   ├── Enviroment
│   ├── Guard
│   ├── Interact
│   ├── Items
│   ├── Player
│   └── UI
├── ScriptableObjects
│   ├── ArtData
│   ├── BackpackItemData
│   └── DummyData
├── Prefabs
└── Materials
```

---

## 主なスクリプト

### Core

```text
GameManager      シーン遷移、Clear、Game Over、MainMenu復帰
PlayerProfile    所持金、バックパック装備、購入済み/配置済みアイテム管理
AlertManager     警戒レベル管理
```

### Data

```text
ArtData           美術品データ、盗品としてのバックパック表示データ
BackpackItemData  ダミー、補助アイテム、実行時盗品データ
```

### Items

```text
ArtPiece          展示品状態、盗み、ダミー交換、盗品収納
```

### Player

```text
SimplePlayerController  プレイヤー移動
ThirdPersonCameraController / SimpleFollowCamera  カメラ制御
PlayerInteractor        展示品とのインタラクト
PlayerInventory         本編中の簡易所持品管理
RuntimeBackpackInput    Tabでバックパックを開く
PlayerBackpackVisual    背中のバックパック表示
```

### UI

```text
BackpackMenuUI          バックパックUI全体
Backpack3DBoard         3Dバックパック盤面
Backpack3DItem          3D盤面上の配置済みアイテム参照
Backpack3DCell          3D盤面上のマス
Backpack3DSpin          表示モデルの回転
BackpackItemListEntryUI 購入リスト表示
HUDManager              本編HUD
MenuNoticeUI            通知表示
```

### Guard

```text
GuardController      警備員の状態管理
GuardVision          視界判定
GuardPatrol          巡回ポイント移動
GuardMemoryNetwork   警備員同士の情報共有
```

---

## 実装上の設計メモ

### 盗品はArtDataから生成する

以前は盗品用に `BP_Loot_...` アセットを作る構想でしたが、現在はやめています。

現在の流れ:

```text
ArtPieceを盗む
-> ArtDataを参照
-> ArtDataのバックパック設定から一時的なBackpackItemDataを生成
-> PlayerProfile.TryAutoPackArtで空きマスへ自動配置
```

これにより、各美術品の見た目や説明は `ArtData` で完結します。

### 3Dバックパックの判定はアイコンのみ

3Dモデルに直接Colliderを使うと、モデルの形や回転によりクリック判定が不安定になります。

そのため現在は:

```text
3Dモデル       見た目専用、Collider無効
IconMarker     クリック / ドラッグ判定用
```

という構成にしています。

### MainMenuとMissionで同じBackpackMenuUIを使う

`BackpackMenuUI` にはモード設定があります。

```text
allowBuying
allowBackpackToggle
allowItemRemoval
showItemLists
```

MainMenuでは購入や削除を許可し、Mission中は並べ替え中心にする想定です。

---

## 現在できていること

```text
MainMenuからMuseum Mapへ移動できる
Clear / Game Over後にMainMenuへ戻れる
バックパックを装備できる
アイテムを購入できる
バックパックにアイテムを配置できる
配置済みアイテムを移動できる
配置済みアイテムを回転できる
3Dボード上でアイコンをクリックして詳細を表示できる
ミッション中にTabでバックパックを開ける
ミッション中はバックパック内の並べ替えができる
美術品を盗める
ダミーと交換して盗める
空展示台にダミーを設置できる
置いたダミーを回収できる
盗品がバックパックへ自動収納される
バックパックが満杯なら盗めない
警備員が巡回する
警備員がプレイヤーを発見・追跡する
空展示台を警備員が発見すると警戒レベルが上がる
出口から脱出してClearできる
警備員に捕まるとGame Overになる
```

---

## 現在の注意点

プロトタイプ段階のため、以下は今後調整が必要です。

```text
一部UIの配置は仮
一部モデルやアイコンはテスト素材
補助アイテムの実際の効果は未実装または調整中
警備員AIの細かい挙動は今後調整予定
セーブ/ロードは未実装
ステージ数は現在テスト用のMap_01_Museum中心
```

---

## 今後の実装予定

### Backpack

```text
ミッション中UIの見た目調整
詳細パネルのデザイン改善
盗品モデル・アイコンの本素材化
バックパック内の盗品価値表示
整理しやすい操作感の改善
```

### Support Items

```text
Mouse Toyで持ち物チェックを回避
Smoke Bombで追跡中の視界を切る
アイテム使用UI
使用回数または消費処理
```

### Guard / Alert

```text
警戒レベルに応じた巡回速度変化
警戒レベルに応じた警備員追加
持ち物チェックイベント
警備員ごとの個性
```

### Stage

```text
展示室ごとの特徴
複数展示室の導線改善
美術品配置の増加
隠れ場所や視界遮蔽物の追加
```

### Presentation

```text
猫プレイヤーモデル差し替え
警備員猫モデル差し替え
美術館内装の作り込み
ポートフォリオ用プレイ動画
READMEへのスクリーンショット追加
```

---

## ポートフォリオで見せられる技術要素

```text
Unity 3Dキャラクター移動
三人称カメラ
ScriptableObjectによるデータ管理
ドラッグ&ドロップ式インベントリ
3D盤面UI
UIと3Dオブジェクトの連携
盗品の自動配置アルゴリズム
ステルスゲームの状態管理
警備員AI
NavMeshAgent
視界判定
警戒レベル制御
シーン遷移
Clear / Game Overループ
```

---

## 現在の開発方針

大作化しすぎず、まずは以下を優先します。

```text
1. 1マップで最後まで遊べる
2. 盗む・詰める・逃げるのループを完成させる
3. バックパック整理の遊びを分かりやすくする
4. 警備員AIの見せ場を作る
5. 見た目を猫怪盗らしく整える
```

この作品の強みは、単に「盗む」だけではなく、**事前準備、バックパック整理、ダミー交換、警備員の発見、盗品を持って脱出** までがつながっている点です。
