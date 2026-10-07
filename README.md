# Dreamy Feedback

Unity 6000 feedback services: VFX, mobile haptics, floating text, icon fly, screen flash/fade, camera shake and sequences. Version 0.2.0 includes scene ownership, cancellation-safe handles and two complete samples.

## Installation

In this sandbox, the Git submodule lives at `LocalPackages/com.dreamy.feedback` and UPM uses `file:../LocalPackages/com.dreamy.feedback`. Initialize submodules before opening Unity.

For another project, install Dreamy Core 1.1.2 and UniTask 2.5.10 or compatible verified revisions before installing via Git. UGUI 2.0.0 supplies TMP on Unity 6000; do not add the legacy standalone TMP 3.0.6 package. Runtime assembly: `Dreamy.Feedback.Runtime`. Editor assembly: `Dreamy.Feedback.Editor`.

Version 0.2.0 is available from the package Git repository. No version tag is created by this change; pin the tested package commit when consuming it. See [VALIDATION.md](VALIDATION.md) for evidence and platform limits.

## Samples

Import from Package Manager > Dreamy Feedback > Samples:

| Sample | Open after import | Prerequisites |
| --- | --- | --- |
| Basic Feedback | `Generated/FeedbackDemo.unity` | Feedback and its declared dependencies; TMP Essential Resources |
| Feedback Economy | `Generated/FeedbackEconomyDemo.unity` | Import Basic Feedback first; install Dreamy Economy, UI, Audio and their dependencies |

Basic Feedback has buttons for all seven channels, Play All, Stop/Clear and a haptic toggle. It ships a rig prefab, particle/text prefabs, original coin sprite, databases, camera, sample shaders and a static ASCII Liberation Sans font with its OFL license. Controls scroll on short/landscape displays. The simple sample shaders can be replaced with production art.

Feedback Economy uses Dreamy UI, committed wallet balance events and Dreamy Audio. Grant commits 100 coins, then shows reward presentation; Retry uses the same transaction and does not replay the reward. The standalone scene uses an **in-memory demonstration wallet** and an original generated audio tone. Foundation injects its persistent Datasave wallet and existing audio; the sample never creates a second production save.

To rebuild into a new folder, save the active scene, close Prefab Stage and exit Play Mode. Use `Dreamy/Feedback/Build Basic Demo`, then `Dreamy/Feedback/Build Economy Demo`. Import TMP Essential Resources once before running or rebuilding the samples. The shipped scene/prefab assets run without rebuilding. Sample editor/runtime/test assemblies stay separate. The shipped EventSystem uses StandaloneInputModule and requires Input Manager (Old) or Both; replace it with InputSystemUIInputModule in new-input-only projects.

## Ownership and setup

`FeedbackHost` is an optional scene component which initializes explicit database/camera/root references on enable and shuts down on disable/destroy. Use the sample `FeedbackRig.prefab` as a starting point. Assign your own art and camera. If gameplay drives camera movement, shake a dedicated camera child rather than a transform driven by another controller.

For manual composition, retain and dispose the concrete services:

```csharp
var vfx = new VfxService();
vfx.Initialize(vfxDatabase, root.WorldVfxRoot);
var text = new FloatingTextService();
text.Initialize(textDatabase, root.FloatingTextRoot, worldCamera);
var registration = FeedbackServiceRegistry.RegisterOwned(vfx: vfx, floatingText: text);

vfx.Play("reward", rewardWorldPosition);
text.Play("+100", rewardWorldPosition);

// At owner shutdown:
registration.Dispose();
text.Dispose();
vfx.Dispose();
```

`RegisterOwned` removes a registration only while the registered instance still belongs to that owner. Legacy `Register` remains available but its caller must unregister manually. Leaves receive explicit dependencies rather than resolving global services.

`Clear` cancels and destroys active and idle pooled objects. Repeated/stale service-issued handle stops cannot release a newer lease. `Dispose` also drops database/root references; call `Initialize` to reuse the service. Sequence Stop cancels pending actions; already dispatched effects belong to individual services. `FeedbackHost.StopAll` resets pending sequences, shake/flash and all pooled effects.

Floating text's three-argument Initialize projects world positions through an explicit camera to its UI root. The legacy overload keeps native transform-position behavior. Icon fly start/end positions use UI transform space, for example RectTransform positions on the same canvas. Never use icon arrival to grant resources.

Haptic `Enabled` and `MinimumInterval` control the built-in fallback. On Android/iOS players, non-None types use Unity's generic vibration with one intensity. Editor/unsupported platforms do not vibrate. Distinct light/medium/heavy native patterns are not implemented.

## Editor tools

Use `Dreamy/Feedback/Create/...`, `Generate IDs` and `Validate All`. Validators check IDs, prefabs, timing/options and incomplete actions. ID output belongs to `Assets/DreamyFeedbackGenerated`; constants escape quotes/control characters and disambiguate identifier collisions.

## Migration from 0.1.0

- Replace asmdef references to `Dreamy.Feedback` with `Dreamy.Feedback.Runtime`; existing asmdef/script GUIDs are retained.
- Update menu paths from `Tools/Dreamy/Feedback` to `Dreamy/Feedback`.
- Replace the old skeleton sample's implicit scene search/global registration with an assigned FeedbackHost and owner cleanup.
- Camera, screen and icon option structs now serialize correctly. Re-author values lost by the old non-serializable structs; missing data cannot be recovered.
- `ScreenFlashView.Flash` now returns a handle. Statement calls stay source-compatible; method-group bindings may need updating.

Feedback has no direct dependency on Audio, Economy, UI, DataConfig, Datasave, DOTween, Addressables or Cinemachine. These remain host/sample concerns. See [VALIDATION.md](VALIDATION.md) for results and platform limits.

Before running either sample in a fresh project, import **Window > TextMeshPro > Import TMP Essential Resources** once. UGUI 2.0 TMP still needs its project-wide TMP Settings/default style resources in players. The sample uses its own static font and shaders, but does not ship a second `Resources/TMP Settings` asset that could conflict with your project.
