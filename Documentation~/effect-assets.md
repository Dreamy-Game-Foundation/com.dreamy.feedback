# Feedback assets and runtime values

Create effects through **Assets > Create > Dreamy > Feedback > Effects**. Each SO contains the settings for one effect and an automatically generated stable ID (Advanced). The common FeedbackAsset base lets both effects and Definitions be dragged into a Definition list. SOs have no preset inheritance or override fields.

## Definition

Choose **Sequence**, **Parallel** or **Staggered**, then drag effect assets into **Effects**. A row shows one asset reference; expand it for Delay Before and Required. Nested Definitions are supported by reference, with cycle/depth/budget validation.

- Sequence waits for each effect to finish before the next delay/effect.
- Parallel starts each effect after its own delay and waits for all.
- Staggered starts step i after `i * interval + step delay`, without waiting for earlier effects.

Delay is timing on a step, not an effect SO. Runtime time policy comes from the root request. Stop/Complete applies to the whole composition, including pending delays.

## One effect

FeedbackPlayer has one **Asset** field that accepts an effect or a Definition. Assign Host and the anchors required by that effect, then connect Button.onClick to Play(). Optional runtime settings live on the component, not on the SO.

~~~csharp
var handle = host.Feedback.Play(
    FeedbackRequestBuilder.For(iconPreset)
        .From(this).IconsFrom(sourceAnchor).To(hudAnchor)
        .WithIcon(coinSprite).WithIconCount(8).WithAmount(100)
        .Build());
await handle.WaitAsync(cancellationToken);
~~~

Use `using Dreamy.Feedback;` and `using Cysharp.Threading.Tasks;` in an async UniTask method. Wait cancellation only cancels the caller's wait. CancelWith on a request cancels playback. Multiple callers may await one handle.

## ID lookup: separate databases

Each type has its own database containing **SO references**. IDs/settings are read from the SO and are not entered twice. Assign only the databases needed under Host > ID lookup databases. Configure/reinitialize before playback; indexes are built at initialization. Runtime does not scan Assets/Resources.

VfxDatabase, FloatingTextDatabase and CameraShakeDatabase retain their old inline entries for compatibility. New entries use the assets list. IconFlyDatabase, UiPunchDatabase, ScreenFlashDatabase, HapticDatabase and DefinitionDatabase are optional, created only when required.

~~~csharp
IVfxService vfx = host.Vfx;
vfx.Play("effect.reward-vfx", VfxPlayOptions.At(position));

IIconFlyService icons = host.IconFly;
icons.Play("effect.reward-icons", new IconFlyPlayArgs
{
    Source = sourceAnchor,
    Target = hudAnchor,
    Icon = coinSprite,
    Count = 8,
    Owner = this,
    Cancellation = cancellationToken
});

host.Feedback.Play("reward.coin", new FeedbackContext(position, this,
    hudAnchor, amount: 100, iconSource: sourceAnchor, icon: coinSprite));
~~~

The last call requires DefinitionDatabase. The facade ID overload looks up only Definitions, so IDs in separate effect databases are not ambiguous. Other typed service extension overloads accept their own IDs: text Play(id, context), UI Play(id, target), camera/screen/haptic Play(id). Custom service implementations may implement IFeedbackAssetLookup plus the relevant direct-preset capability; existing interfaces stay compatible.

Direct VFX by reference is also available: `vfx.Play(vfxPreset, VfxPlayOptions.At(position))`. Primitive callers own Stop/Complete unless the overload takes owner/cancellation (IconFly does). Use the facade for composition and automatic owner lifetime.

## Runtime values

IconFlyFeedbackSettings contains visual defaults only: icon/count, duration, size, style, easing and trajectory parameters. It contains no scene anchors or positions.

IconFlyPlayArgs supplies source/target and optional icon/count. Count=null uses the preset; Count=0 completes without spawning; negative or greater than 64 is rejected. Missing effective icon or required anchors produces an error. Source/target/owner destruction and cancellation stop the group and release its leases. The backend continues to follow the destination as before.

Amount is the actual value formatted into text; Count is the number of visual icons. `Amount=100` does not spawn 100 icons. Floating text can use WithText or the preset format with `{amount}`. World position, follow target and UI targets are request data.

Runtime customization uses WithOverrides on a code request or the component Runtime settings section. Per-step code/component bindings use snapshot paths (`0`, `0/1`, etc.); invalid/duplicate/non-effect paths are rejected. They do not serialize into Definition SOs. Order: effect settings, runtime customization, then runtime icon/count/text. Shared assets are never mutated and delayed steps use the initial snapshot.

## Migration

Legacy graph storage is hidden and read for compatibility. Open migration from a legacy Definition Inspector; lookup rig appears only in the migration window.

Preview writes no files. Convert first resolves inline ID references, then creates a list. Differing static legacy settings are baked into independent presets rather than dropped; nested groups become Definition references. Newly created assets remain after Undo, while Definition changes have Undo. Unsupported standalone/parallel/trailing delay layouts are reported and require an explicit redesign.

The scoped sample authoring tool exports old per-step differences into serialized **demo component** runtime bindings so its seven shared effect presets remain reusable. Generic migration bakes differences instead. A migration that exports bindings must pass those bindings on future requests; it does not hide them inside SOs.

## Portable sample

Import Basic Feedback and TMP Essential Resources. Use `Generated/Rigs/GameFeedbackRig.prefab`; preview is `Generated/Scenes/FeedbackDemo.unity`. Effects are grouped by type; Definitions by Rewards/UI/Gameplay/Screen; databases and optional library have separate folders.

The sample keeps the existing four icons and seven effect SO, thirteen top-level Definitions and one nested group needed for LevelComplete timing. Four rewards and fourteen preview buttons are already configured; no builder is required after import. Optional databases are not all instantiated in the sample.

Desktop/Editor haptics report unsupported. Physical mobile vibration and performance still require device validation.
