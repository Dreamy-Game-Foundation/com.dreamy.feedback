# Dreamy Feedback

Authoring bằng SO: mỗi loại effect có asset riêng; Definition chọn Sequence/Parallel/Staggered và chứa list asset.
SO không có inheritance hoặc override; dữ liệu runtime truyền qua code/component. Sample giữ 4 icon hiện có và
có preset VFX 2D cho Canvas. Xem [hướng dẫn effect assets](Documentation~/effect-assets.md).

Feedback cho Unity: VFX, haptic, floating text, icon bay về HUD, UI punch, camera shake và screen flash. Một `FeedbackDefinition` có thể ghép các effect bằng Sequence, Parallel và Delay. Package chỉ trình bày kết quả; việc cộng tiền, năng lượng hoặc phần thưởng thuộc gameplay.

## Dùng ngay trong game

1. Cài package và dependency trong `package.json`. DOTween và Dreamy Core là tùy chọn.
2. Import sample **Basic Feedback** trong Package Manager và import **TMP Essential Resources** nếu project chưa có.
3. Kéo `Generated/Rigs/GameFeedbackRig.prefab` vào scene. Prefab đã có host, pool roots, Canvas effect và database mặc định; không chứa camera, đèn, puzzle tiles hay UI demo.
4. Thêm **Feedback Player** lên một GameObject trong UI. Gán Host và Asset (effect đơn hoặc Definition), rồi kéo anchor bắt đầu vào Icon Source và icon HUD vào Target.
5. Gán `FeedbackPlayer.Play()` vào `Button.onClick`. Có thể chọn `CoinReward`, `StarReward`, `EnergyReward`, `GemReward` hoặc definition riêng.

Không cần chạy builder. `FeedbackDemo.unity` dùng để xem trước các preset và so sánh kiểu animation. `FeedbackRig.prefab` chứa camera/anchor của preview; dùng **GameFeedbackRig** khi tích hợp vào game.

## Gọi từ code

Gán `host` và `rewardDefinition` qua Inspector hoặc inject service từ composition root:

```csharp
// Chạy sau khi gameplay đã xác nhận phần thưởng.
host.Initialize();
FeedbackHandle handle = host.Feedback.Play(
    FeedbackRequestBuilder.For(rewardDefinition)
        .From(this)                 // disable/destroy owner sẽ hủy request
        .IconsFrom(rewardAnchor)    // RectTransform trong UI
        .To(starHud)                // HUD đích được theo dõi khi di chuyển
        .WithAmount(3)              // dữ liệu text; không phải số icon animation
        .WithIcon(starSprite)       // tùy chọn: thay sprite, giữ nguyên graph
        .At(worldPosition)          // dùng cho VFX/floating text
        .Build());

// Khi cần hủy riêng request này:
handle.Stop();
// Hoặc tua đến cuối các bước, gồm effect chưa bắt đầu:
// handle.Complete();
```

Nếu chỉ cần một effect, gọi primitive trực tiếp:

```csharp
var options = IconFlyOptions.Create(energySprite, source.position, energyHud.position);
options.Target = energyHud;
options.Count = 8;
options.Style = IconFlyStyle.Fountain;
FeedbackHandle flight = host.IconFly.Fly(options);
host.Ui.Punch(energyHud, UiPunchOptions.Default);
host.Vfx.Play("reward", worldPosition);
```

Primitive playback cần được caller giữ handle và Stop khi đóng UI. Request composite có `.From(this)` tự theo lifetime của owner. `StopAll()` trên host ảnh hưởng mọi caller dùng chung host.

## Preset và camera

| Preset sample | Animation mặc định |
| --- | --- |
| CoinReward | Scatter Magnet, 12 icon, punch HUD khi đến |
| StarReward | Arc, 4 icon, không xoay |
| EnergyReward | Fountain, 8 icon |
| GemReward | Spiral, 6 icon |
| ButtonClick | UI punch + haptic |
| Hit | VFX + camera shake + haptic chạy song song |

Duplicate definition để chỉnh thời gian, Count, Size, Spread, Stagger, ArcHeight, Spin và Ease. `Amount` là giá trị reward thật; `Count` là số icon dùng để minh họa, giới hạn 64 mỗi lượt. `.WithIcon()` chỉ thay sprite của lượt phát, không sửa shared definition.

Nếu dùng floating text hoặc camera shake, gán camera game vào Host trước khi chạy. Host mặc định tìm `Camera.main` lúc khởi tạo nếu World Camera trống; có thể tắt `Use Main Camera` để chỉ dùng camera được gán rõ ràng. Từ bootstrap có thể gọi `host.Initialize(gameCamera)`; thay camera sẽ dừng playback hiện tại và khởi tạo lại service. Camera xuất hiện sau khi Host đã khởi tạo cần được bind lại bằng overload này; nếu service được đăng ký ở composition root, đăng ký lại các instance sau khi bind. Với camera follow, gán Camera Root của FeedbackRoot vào transform offset nằm dưới follow rig và phía trên camera. Camera shake chỉ tác động nội dung được camera render; UI **Screen Space Overlay** không rung theo camera.

## VFX 2D trên UI

VFX asset có `Space = UI` chạy dưới `FeedbackRoot.UiVfxRoot` trong Canvas, không cần camera. `Space = World` giữ hành vi particle/world hiện có. Nếu dùng rig tự tạo, gán Ui Vfx Root vào một RectTransform nằm dưới Canvas; root mặc định được tạo dưới IconFlyRoot của rig sample.

Sample có sẵn `Generated/UI VFX/UiRewardBurst.asset` (burst vàng) và `UiStarBurst.asset` (sparkle hình sao), cùng prefab tương ứng. Chọn **Tools → Dreamy → Feedback → Create UI VFX Starter** để tạo thêm một bộ `UiRewardBurst` trong thư mục mới. Prefab dùng `UiBurstVfxInstance`: chỉnh Sprite, Color, Count, Radius, Size và Spin để tạo sparkle/confetti. Không cần shader particle hoặc thư viện ngoài. Images không nhận raycast; số mảnh giới hạn 64 mỗi instance và dùng lại khi trả về pool.

Gán asset này vào `FeedbackPlayer` trên button/UI anchor và gán Host; `Play()` dùng vị trí của anchor. Có thể ghép cùng UI punch/icon fly trong Definition. Từ code:

```csharp
var burst = host.Feedback.Play(FeedbackRequestBuilder.For(uiBurstAsset)
    .From(this).At(button.transform.position).Build());
```

Vị trí UI là `Transform.position` của anchor trên cùng Canvas hoặc Canvas có cùng cách bố trí, không phải pixel screen hay vị trí world cần chiếu bằng camera. Để dùng một Canvas khác, gọi primitive với `VfxPlayOptions.Parent` là RectTransform dưới Canvas đó, `Position` là anchor.position. `UseLocalPosition` cho phép truyền tọa độ local của parent. Caller primitive giữ handle và Stop khi đóng UI. Burst và thời gian despawn cùng dùng lựa chọn unscaled của request, nên effect vẫn chạy khi game pause; `.UseUnscaledTime(false)` chọn scaled time cho effect đơn.

IconSource/Target trong preset reward là **UI anchors**. World position không tự chuyển thành vị trí icon HUD. [Hướng dẫn sample](Samples~/BasicFeedbackSample/README.md) có setup HUD, đổi asset và các lỗi thường gặp.

## Mở rộng

- Gameplay có thể inject `IFeedbackService` hoặc từng primitive interface; không cần ServiceLocator.
- `FeedbackServices` cho phép thay implementation primitive khi tự compose `FeedbackService`.
- `IFeedbackAnimationBackend` và `IIconFlyAnimator` cho phép thay animation implementation. Sample tùy chọn **DOTween Feedback** triển khai hai điểm này (cài DOTween trước, rồi import trong Package Manager → Samples); API gameplay không chứa Tween/Sequence/Ease của DOTween.
- Graph hiện có 10 node built-in. Thêm một loại node mới cần bổ sung enum, executor và Inspector/validation; graph chưa có registry custom node cho package bên ngoài.

Contracts chỉ phụ thuộc Unity engine; Runtime dùng UniTask/UGUI. Core registration nằm trong assembly integration tùy chọn. Effect có pooling và active budgets; Stop/Complete idempotent, handle cũ không tác động instance đã tái sử dụng. Các request độc lập, nhưng Punch/Shake/Flash mới thay effect trước trên cùng target/channel.

Chi tiết: [semantics và authoring](Documentation~/getting-started.md), [migration 0.3](Documentation~/migration-0.3.md), [validation](VALIDATION.md).

Adapter DOTween được đóng gói tại `Samples~/DOTweenFeedbackSample`, không cần repo/package riêng. Xem [hướng dẫn cài, gán provider và chỉnh icon fly](Samples~/DOTweenFeedbackSample/README.md).

Create a Definition, select Sequence/Parallel/Staggered, then drag effect SO into its list. The Inspector has no lookup rig or SO overrides. For ID playback, assign separate per-type databases containing SO references. See [effect assets and runtime values](Documentation~/effect-assets.md).
