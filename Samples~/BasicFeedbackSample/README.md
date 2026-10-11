# Basic Feedback

Import this sample and official TMP Essential Resources. The sample has four existing resource icons, seven shared effect SO, thirteen top-level Definitions and one nested group for LevelComplete timing. No extra art or expanded preset set is required.

## Use in a game

1. Drag `Generated/Rigs/GameFeedbackRig.prefab` into your scene.
2. Add FeedbackPlayer to your button/panel. Set Host and **Asset** (a single effect or a Definition).
3. Assign icon source/target for icon fly, target for punch, and a game camera on Host for shake/world text.
4. Connect Button.onClick to FeedbackPlayer.Play(). StopLast/CompleteLast control the latest request; disabling the owner cancels its facade requests.

The rig does not create a game camera, EventSystem, Light, gameplay board or economy owner. Gameplay still owns rewards; feedback only presents them.

## Folders

- `Generated/Effects/<type>`: seven shared visual settings assets, without inheritance or overrides.
- `Generated/Definitions/Rewards`, `UI`, `Gameplay`, `Screen`: reusable compositions.
- `Generated/Definitions/Groups`: one nested group needed to keep LevelComplete sequence/parallel timing.
- `Generated/Databases`: existing VFX/text/shake databases with SO reference lists and compatibility entries.
- `Generated/Libraries`: optional editor catalog, not a mandatory runtime lookup registry.
- `Generated/Rigs` and `Generated/Scenes`: reusable rigs and the ready-to-use demo.

## Definition and runtime values

A Definition chooses Sequence, Parallel or Staggered and stores an Effects list. Expand a row for Delay Before and Required. Delay is timing, not a separate effect asset. Nested Definitions may be dragged into the same list.

Preset SOs contain defaults only. Icon source/target, icon/count, world position, amount and owner are runtime data. Amount=100 and Count=8 are independent. Code requests or FeedbackPlayer runtime settings can customize a playback without changing the asset.

The demo component stores runtime step bindings for its visual variations (colors, counts, text, haptic type, strengths). A direct Definition request uses the seven shared defaults unless the caller supplies these bindings/customization. No override is hidden in the SO. For independently configured static presets, duplicate an effect and change its settings.

## Play by ID

Assign a database for the desired type once on Host. Each database contains references to that type's SO and reads IDs from them; no duplicate settings entry is needed. VFX/text/shake already have sample databases; create other databases only when you need ID lookup. DefinitionDatabase is separate from effect databases.

~~~csharp
host.Vfx.Play("effect.reward-vfx", VfxPlayOptions.At(position));

host.IconFly.Play("effect.reward-icons", new IconFlyPlayArgs
{
    Source = rewardAnchor, Target = coinHud, Icon = coinSprite,
    Count = 8, Owner = this, Cancellation = cancellationToken
});
~~~

The IconFly call requires assigning an IconFlyDatabase containing RewardIcons. A direct effect reference/facade request does not require that database. Full examples: [assets, runtime values and UniTask waiting](../../Documentation~/effect-assets.md).

## Demo and limits

Open `Generated/Scenes/FeedbackDemo.unity`. Four rewards and fourteen channel buttons are already wired. The controls report terminal/unavailable states; Play All may replace effects sharing a target/channel. Stop/Complete controls belong to this dedicated preview host.

Desktop/Editor haptics report unsupported. Generic mobile vibration has one intensity fallback; native device patterns and performance need device testing. Input needs an EventSystem configured for the project (sample uses StandaloneInputModule). TMP Essential Resources are required; portable font is Liberation Sans with its included OFL license. Sandbox custom fonts/confetti remain project-owned.

Legacy definitions are readable but authored through the migration window. Legacy template factory lives under Dreamy > Feedback > Legacy Templates; new authoring uses SO + mode/list. New sample imports do not require running the authoring tool.
