# Changelog

## [Unreleased]

## [0.4.0] - 2026-10-12

- Update sample regressions to distinguish icon instances from the persistent UI VFX container; isolate fallback camera fixtures from other loaded scenes.

- Add Canvas-native pooled UI burst VFX, explicit World/UI preset routing, pause-aware playback and a non-overwriting UI starter generator.
- Resolve MainCamera at Host initialization when enabled and show missing-camera guidance in the Host Inspector; overlay UI remains independent of camera shake.

- Breaking: simplify SO authoring: shared ID base, Definition mode/list with per-step delay, no preset inheritance or SO overrides; retain hidden legacy graph reader for migration.
- Add per-type SO-reference databases, primitive ID lookup, direct VFX overloads, typed IconFly runtime args and runtime-only component/step bindings.
- Organize sample Effects/Definitions/Databases/Rigs/Scenes without changing asset GUIDs; keep seven effect SO and four icons, plus one necessary nested Definition.
- Add seven per-effect ScriptableObject types, direct-asset requests, immutable playback snapshots, runtime request customization and optional libraries.
- Add drag-and-drop graph authoring, inline-to-SO migration preview, scoped migration and UniTask WaitAsync with independent wait cancellation.
- Keep existing inline/ID APIs and engine-only Contracts; SO VFX/text do not require database registration.
- Reuse exactly seven starter effect SO and the four existing icons; convert portable definitions and bind fourteen demo previews through serialized entries.
- Repair VFX/text prefab file references, preserve independent pool leases and stop later Parallel one-shots after synchronous required failure.
- Show unavailable/terminal states and unsupported desktop haptics in the demo; add real raycast button/reopen regressions.
- Restore sample text rendering in the clean Built-in consumer with explicit clipping, depth and culling defaults.
- Improve definition/database authoring: node defaults, runtime-field filtering, scoped ID lookup and inline database validation.
- Add seven common definition templates and matching portable starter assets, with setup/call examples.

## [0.3.0] - 2026-10-09

- Breaking: replace legacy sequences with definition/request facade, real Sequence/Parallel completion and independent Stop/Complete handles.
- Split engine-only contracts and optional Core integration; remove mandatory Core dependency and reward glue from FeedbackHost.
- Ship the optional DOTween adapter as the DOTween Feedback sample (no separate adapter package), five icon trajectories, pooling/active budgets, owner cancellation and UI Punch.
- Replace Economy sample with a reusable game starter: GameFeedbackRig, Coin/Star/Energy/Gem reward presets, original resource icons and Inspector-friendly FeedbackPlayer.
- Add per-request sprite override and explicit camera binding for hosts used in game scenes.
- Remove puzzle tiles and one-off sample upgrade/generation code. Keep a standalone preview and update Foundation integration.
- Rewrite setup/sample README around importing the rig, binding existing HUD, replacing assets and extending service/backend implementations.

## [0.2.0] - 2026-10-07 (Git source revision; no version tag)

- Align Runtime assembly, author and Unity 6000 UGUI dependencies; preserve GUIDs.
- Add scene-owned FeedbackHost and ownership-safe service registrations.
- Fix sequence cancellation, shake races, stale/double pool handles, active cleanup and screen Stop.
- Serialize camera/screen/icon options and support explicit world-to-UI projection.
- Add mobile haptic guard/throttle, escaped IDs and aligned menus.
- Ship two complete samples, builders, responsive controls, tests and Economy/UI/Audio integration.
- Document migration and validation.
- Fix Economy panel layout/scale and Canvas sorting after PanelManager registration so Back/Close receives pointer clicks and releases Foundation navigation.


## 0.1.0

- Added independent VFX, haptic, floating text, icon fly, screen feedback, and camera shake services.
- Added optional feedback sequence orchestration through module interfaces.
- Added ScriptableObject databases, pooling, editor validators, menu items, and ID generation.
