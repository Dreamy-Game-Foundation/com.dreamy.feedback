# Dreamy Feedback

Feedback cho Unity: VFX, haptic, floating text, icon bay về HUD, UI punch, camera shake và screen flash. Một `FeedbackDefinition` có thể ghép các effect bằng Sequence, Parallel và Delay. Package chỉ trình bày kết quả; việc cộng tiền, năng lượng hoặc phần thưởng thuộc gameplay.

## Dùng ngay trong game

1. Cài package và dependency trong `package.json`. DOTween và Dreamy Core là tùy chọn.
2. Import sample **Basic Feedback** trong Package Manager và import **TMP Essential Resources** nếu project chưa có.
3. Kéo `Generated/GameFeedbackRig.prefab` vào scene. Prefab đã có host, pool roots, Canvas effect và database mặc định; không chứa camera, đèn, puzzle tiles hay UI demo.
4. Thêm **Feedback Player** lên một GameObject trong UI. Gán Host và Definition, rồi kéo anchor bắt đầu vào Icon Source và icon HUD vào Target.
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

Nếu dùng floating text hoặc camera shake, gán camera game vào Host trước khi chạy. Từ bootstrap có thể gọi `host.Initialize(gameCamera)`; thay camera sẽ dừng playback hiện tại và khởi tạo lại service. Với camera follow, gán Camera Root của FeedbackRoot vào transform offset nằm dưới follow rig và phía trên camera.

IconSource/Target trong preset reward là **UI anchors**. World position không tự chuyển thành vị trí icon HUD. [Hướng dẫn sample](Samples~/BasicFeedbackSample/README.md) có setup HUD, đổi asset và các lỗi thường gặp.

## Mở rộng

- Gameplay có thể inject `IFeedbackService` hoặc từng primitive interface; không cần ServiceLocator.
- `FeedbackServices` cho phép thay implementation primitive khi tự compose `FeedbackService`.
- `IFeedbackAnimationBackend` và `IIconFlyAnimator` cho phép thay animation implementation. Sample tùy chọn **DOTween Feedback** triển khai hai điểm này (cài DOTween trước, rồi import trong Package Manager → Samples); API gameplay không chứa Tween/Sequence/Ease của DOTween.
- Graph hiện có 10 node built-in. Thêm một loại node mới cần bổ sung enum, executor và Inspector/validation; graph chưa có registry custom node cho package bên ngoài.

Contracts chỉ phụ thuộc Unity engine; Runtime dùng UniTask/UGUI. Core registration nằm trong assembly integration tùy chọn. Effect có pooling và active budgets; Stop/Complete idempotent, handle cũ không tác động instance đã tái sử dụng. Các request độc lập, nhưng Punch/Shake/Flash mới thay effect trước trên cùng target/channel.

Chi tiết: [semantics và authoring](Documentation~/getting-started.md), [migration 0.3](Documentation~/migration-0.3.md), [validation](VALIDATION.md).

Adapter DOTween được đóng gói tại `Samples~/DOTweenFeedbackSample`, không cần repo/package riêng. Xem [hướng dẫn cài, gán provider và chỉnh icon fly](Samples~/DOTweenFeedbackSample/README.md).

Tạo data nhanh: **Assets → Create → Dreamy → Feedback → Common Presets** hoặc dùng 7 asset trong `Basic Feedback/Generated/CommonPresets`. Definition có menu Add effect và lookup ID theo rig; databases có entry theo ID và validation inline. [Preset, setup tối thiểu và ví dụ gọi](Samples~/BasicFeedbackSample/README.md#tạo-và-chỉnh-data-nhanh).
