# Changelog

## [0.3.0] - 2026-10-09

- Breaking: replace legacy sequences with definition/request facade, real Sequence/Parallel completion and independent Stop/Complete handles.
- Split engine-only contracts and optional Core integration; remove mandatory Core dependency and reward glue from FeedbackHost.
- Add optional DOTween adapter, five icon trajectories, pooling/active budgets, owner cancellation and UI Punch.
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
