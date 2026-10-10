# Migration from 0.2 to 0.3

This is an intentional breaking refactor. Legacy sequence APIs/assets and Host.PlayReward are removed.

| Old | New |
| --- | --- |
| IFeedbackSequenceService / FeedbackSequenceDatabase | IFeedbackService / FeedbackDefinition |
| Sequence action list with delay-before-dispatch | Authored Sequence/Parallel/Delay graph with real completion |
| VfxHandle / void IconFly.Fly | FeedbackHandle for VFX and icon group |
| FeedbackHost.PlayReward / rewardIcon | Project/sample presenter supplies a definition/request |
| Static global registration from host | Composition root calls scoped FeedbackServiceRegistry (optional Core integration) |
| FeedbackContext constructor | Context now includes Source, Target, Amount, Intensity, IconSource and optional Icon override; use named parameters/builder |

Reauthor composite assets and update consumers together. If old actions overlapped, use Parallel explicitly rather than blindly converting them to Sequence. Starter definitions are CoinReward, StarReward, EnergyReward, GemReward, Hit and ButtonClick. Existing preset databases/prefab GUIDs remain usable. UI Punch, Camera Shake and Screen Flash replace previous playback on the same target; their composite observes that cancellation.

Contracts are in `Dreamy.Feedback.Contracts`; add the assembly reference to asmdefs consuming API types. Runtime remains `Dreamy.Feedback.Runtime`. Scoped Core registration is in `Dreamy.Feedback.Integration.Core`. Package Core dependency is no longer mandatory. Definitions and request builder expose no UniTask or DOTween types.

Reimport **Basic Feedback** into a fresh consumer. The sandbox retains the 0.2.0 import folder while using the new API. The Economy sample and one-time FeedbackSampleUpgrade tool are removed. Foundation owns its wallet observer directly; Feedback has no Economy/UI/Audio sample dependencies.

Use `GameFeedbackRig` with existing game camera/HUD. `FeedbackRig` is the preview rig. `FeedbackPlayer` supports Inspector/UnityEvent setup; `.WithIcon()` overrides one request without editing its definition. `host.Initialize(camera)` binds an explicit game camera and stops/rebuilds services if the camera changes.

## DOTween adapter packaging

The adapter is now the optional **DOTween Feedback** sample in this package. Install DOTween, remove the old `com.dreamy.feedback.dotween` manifest dependency/testables entry, then import the sample. Preserve provider assignments; script GUIDs and assembly names are unchanged. Do not install both copies. See [sample setup](../Samples~/DOTweenFeedbackSample/README.md).
