# Current starter sample verification — 2026-10-09

This section supersedes the earlier sample design and counts below. Local working tree, Unity 6000.4.12f1 Linux; no published tag.

- Removed Feedback Economy sample, puzzle tiles and one-time upgrade/build generators. Foundation wallet observation is project-owned; its Feedback button now plays presentation directly without opening an Economy panel.
- Added a clean GameFeedbackRig (seven transforms; no Camera, Light, EventSystem, sample definitions or SpriteRenderer), FeedbackPlayer and Coin/Star/Energy/Gem reward presets. Per-request icon overrides leave shared definitions unchanged.
- Preserved the user's Magic font and Hyper Casual confetti in the sandbox preview. Portable sample uses OFL Liberation Sans, original sample VFX and original resource icons; third-party asset dependencies are excluded and verified in a consumer without those assets.
- README now covers importing the rig, existing HUD anchors, Inspector events, primitive/facade calls, camera setup, asset replacement, lifetime and extension boundaries. Built-in graph node types still require executor/editor changes to extend; only services/backends/animators are directly replaceable externally.

Evidence: sandbox `.omo/evidence/feedback-starter/`.

| Gate | Result |
| --- | --- |
| Sandbox runtime + DOTween + starter + Foundation | 28/28 PlayMode passed |
| Clean consumer without Core, DOTween, Hyper Casual FX or custom font | 24/24 PlayMode passed |
| Definition authoring EditMode | 4/4 passed |
| Linux development build and error-sensitive player smoke | Passed, exit 0 |
| Reusable rig with existing HUD and owner disable | Passed in sandbox and clean consumer |
| Request sprite override without graph mutation | Passed |
| Camera world projection movement/restoration, without board | Passed |

The Foundation fixture now scrolls buttons into the viewport before raycasting, preserving the user's scrollable Foundation prefab. One failed fixture run (offscreen Feedback button) and incomplete assembly-filtered runs are not counted as passes. New screenshots are named starter-portrait/starter-landscape. Build/player smoke and EditMode results are recorded in the final evidence summary.

Physical Android/iOS haptics, device performance and thermal budgets remain untested. The historical results below describe earlier revisions and are retained only as history.

---

# Feedback 0.3.0 local validation

Validation date: 2026-10-09. Unity 6000.4.12f1, Linux Editor/player. This validates the local working tree, not a published tag. Historical 0.2.0 results below are not gates for this refactor.

## Current implementation

- Engine-only Contracts assembly, definition/request facade, Sequence/Parallel completion, bounded graph validation, shared Stop/Complete handles, owner cancellation, pooled primitives, UI Punch and optional Core registration.
- Removed legacy sequence API and host reward glue. Economy and Foundation now submit requests; retry/reopen/close/raycast regression coverage remains.
- Separate optional `com.dreamy.feedback.dotween` adapter uses the observed DOTween.dll distribution. No DOTween or Editor references in base Runtime/Contracts.
- Basic has five icon trajectories (Straight, Arc, ScatterMagnet, Fountain, Spiral), composite reward/hit/click, Stop/Complete controls and a world-space puzzle board for visible camera shake.
- Source sample assets use portable shaders/materials and omit DOTween-provider/URP components. Imported sandbox rigs explicitly use the adapter. Existing imported 0.2.0 folder and scene/prefab GUIDs are retained.

## Current evidence

Evidence is in sandbox `.omo/evidence/feedback-modular/`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Sandbox package, adapter, samples and Foundation PlayMode | 29/29 passed | final-29-playmode-mcp.json |
| Definition authoring EditMode | 4/4 passed | final-editmode-mcp.json |
| Basic after portable board material change | 2/2 passed | MCP job ece4f43a59cc41d1b5cff0fd9d5f3df7 |
| Clean base consumer without Core, DOTween or URP | 22/22 PlayMode passed | feedback-consumer-playmode.xml |
| Clean consumer with optional DOTween adapter, without Core | 25/25 PlayMode passed | feedback-dotween-consumer-playmode.xml |
| Linux development player build | Passed | feedback-consumer-build.log |
| Error-sensitive Linux player smoke | Passed, exit 0 | feedback-player-smoke.log |
| Manifest/asmdef JSON, Runtime/Editor and DOTween boundary, source diff whitespace | Passed | validation-summary.json and local source inspection |

Tests cover real Sequence/Parallel completion, required-module failure, owner destruction, stale pool handles, repeated Stop/Complete, unscaled playback, graph cycles/budgets, disabled VFX cleanup, five DOTween icon paths and camera movement/restoration. Consumer build validation checks missing scripts, definitions, renderer materials/shaders and absence of Core/DOTween. Player smoke plays every demo channel and fails on logged errors.

Portrait/landscape captures are camera RenderTexture evidence, not physical-device screenshots. Existing static font glyph/atlas assets were preserved together during rebuild. TMP Essential Resources remain required; the sample does not ship duplicate TMP Settings.

## Reproduce current gates

1. Select the exact sandbox in Unity MCP before mutation. Use Test Runner for package/adapter/sample/Foundation PlayMode assemblies and Feedback EditMode tests. Close Device Simulator during pointer tests so simulated safe-area coordinates do not affect GameView fixtures.
2. Create a Unity 6000.4.12f1 project with the base manifest recorded in `consumer-manifest.json`; import official TMP Essential Resources and Basic from `Samples~`. Run PlayMode tests. `consumer-FeedbackConsumerValidation.cs` records import/build validation; `consumer-FeedbackPlayerSmoke.cs` records the player smoke fixture.
3. Add the optional adapter and DOTween distribution using `dotween-consumer-manifest.json`, then run PlayMode tests. DOTween 0.0.3 includes Audio/Physics/Physics2D module source and requires those engine modules in a minimal consumer.
4. Build and run the Linux development player. Both build and smoke logs include explicit PASS markers; aborted/zero-test runs are not counted.

## Current limits

- Android/iOS builds, physical vibration, touch/safe-area and performance/thermal budgets have not been measured. Haptics use generic Unity vibration.
- Tested engine is 6000.4.12f1; minimum 6000.0 in the manifest is not proof of all versions. Adapter validation covers DOTween package 0.0.3 / DLL 1.2.320; other distributions may need asmdef changes.
- Default restricted Linux player startup encountered SDL/input initialization failure; the successful smoke used the normal host environment. No result is inferred from the failed startup.
- Unity-generated YAML includes native serialization whitespace; source/config Markdown/JSON/C# checks pass. No manual scene/prefab YAML rewrite was used.
- No commit, push or version tag was made. The new adapter must be included separately when publishing the working tree.

---

## Historical 0.2.0 validation

# Feedback 0.2.0 local validation

Validation date: 2026-10-07. Unity 6000.4.12f1, Windows Editor/player, sandbox URP 17.4.0 and clean consumer built-in renderer. Toolkit canonical version: 0.1.0-alpha.2. This validates the 0.2.0 Git source revision; a tagged release and mobile store readiness are not claimed.

## Implemented

- Git submodule and local UPM dependency; Runtime/Editor/test/sample assembly split aligned with current Dreamy packages.
- Cancellation and ownership fixes for sequence, screen, shake, VFX/text/icon pools; generation-safe service handles; serializable options; scoped registration and FeedbackHost.
- Two complete samples with scenes, prefabs, databases, original sprite/tone, static ASCII font and sample shaders. Basic uses no Input System/URP asset references. Economy uses explicit UI/Audio/wallet composition.
- Foundation bootstrap owns shared feedback, observes committed coin changes and opens the Economy sample panel with its persistent wallet. Retry cannot grant or replay twice; panel reopen does not multiply subscriptions.
- Sample builders validate databases and sequence references, preserve existing output by creating unique folders, and require edit mode, closed prefab stage and a saved scene.

## Evidence

Evidence is stored in the sandbox `.omo/evidence/feedback/`; larger original logs are in `Logs/`.

| Gate | Result | Evidence |
| --- | --- | --- |
| Original runtime regressions | 4/4 failed before fixes | feedback-before.xml |
| Package authoring EditMode | 4/4 passed | editmode-mcp.json |
| Package + samples + Foundation final PlayMode | 14/14 passed | final-playmode-mcp.json |
| Foundation shared-wallet integration | 1/1 passed | foundation-mcp.json |
| Minimal consumer UPM Basic import and PlayMode | 10/10 passed | feedback-consumer-basic-tests.xml |
| Clean consumer Economy/UI/Audio final PlayMode | 13/13 passed | feedback-consumer-final-tests.xml |
| Windows development player build and smoke | Passed, exit 0, no logged errors | player-validation.json |
| Static assembly gate | Passed, 82 asmdefs parsed | asmdef-final.json |
| Package/project completeness | Passed | package-check-final.json |

Foundation test uses a fresh temporary save directory and checks service cleanup. Basic checks missing scripts, assigned fonts, shader references, button sizing, all seven channels and pooled cleanup. Portrait/landscape PNGs are camera RenderTexture captures with temporary canvas/scaler settings, not physical-device screenshots. Foundation portrait also captures reward and retry status. Short landscape controls are scrollable.

A player smoke run initially exited 0 but logged TMP dynamic-font exceptions without TMP Settings. The fixture was strengthened to fail on logged errors; samples now ship an ASCII static atlas. Static fonts also exposed TMP default stylesheet access in players; therefore the documented prerequisite is to import official TMP Essential Resources once before running either sample. No duplicate TMP Settings resource is shipped in the samples. The final Windows player build and error-sensitive smoke pass, with exit code 0 and no logged errors (player-validation.json). The clean Economy/UI/Audio consumer also passes 13/13 PlayMode tests.

## Reproduce

From the sandbox:

```powershell
node ../dreamy-codex-toolkit/harness/dreamy-harness asmdef .
node ../dreamy-codex-toolkit/harness/dreamy-harness package-check .
```

Use Unity Test Runner for `Dreamy.Feedback` EditMode/PlayMode and `Dreamy.Template.Tests.FoundationFeedbackTests` PlayMode. MCP must identify `D:/Game/dreamy-package-sandbox` before mutation. `Dreamy/Sandbox/Build Feedback Samples and Integration` refreshes imported sample assets and narrowly updates Foundation references.

Import Basic Feedback through UPM in a clean Unity 6000 project with Feedback/Core/UniTask dependencies, then import TMP Essential Resources. Then add Economy/UI/Audio and their dependencies before importing Feedback Economy. The sandbox DOTween revision requires the built-in Physics2D module. Shipped scenes use StandaloneInputModule: select Old Input Manager or Both, or replace that module for Input System-only projects.

## Limits and publication

- Android/iOS builds, device vibration, touch/safe-area and thermal/frame budgets have not been validated. Haptics use generic Unity vibration rather than distinct native intensity patterns.
- Tested engine is 6000.4.12f1; manifest minimum 6000.0 is not proof of all engine/version compatibility.
- Unity 6000.4 reports obsolete object-search API warnings retained for Unity 6000.0 source compatibility. Input Manager also has a deprecation warning.
- Direct3D 12 batch startup crashed on this host; clean consumer validation uses Direct3D 11. No package pass is inferred from a crashed run.
- Existing dirty Economy/LuckyWheel/Missions/Progression/Tutorial submodules were preserved. Integration compatibility describes their current sandbox working tree, not hypothetical published versions.
- Feedback source is committed in its package repository and the sandbox integration pins that commit as a gitlink. No version tag is created. Initialize submodules and import the documented dependencies before reproducing these gates.

## 2026-10-08: Foundation input after Feedback close

A pointer/raycast regression reproduced Back/Close hitting the Foundation Toggle instead of its own Button. The generated root Canvas retained a zero local scale and did not stretch correctly after reparenting into PanelManager; nested sorting also inherited the parent order. Normalize scale/anchors and enable sorting override in FeedbackEconomyPanel.Init after registration. The builder and shipped prefab use stretch anchors. The package/sample PlayMode suite also passes 13/13 after this fix (close-fix-playmode.json). No shared UI package changes are needed.

The regression uses a real Canvas/EventSystem fixture, waits for show transitions and dispatches clicks through the first EventSystem raycast hit. After Back/Close, Add Score and Economy/Missions/Progression/Lucky Wheel work, and Feedback reopens. It uses a temporary wallet save. Evidence: close-raycast-before.json and close-raycast-after.json (1/1 passed). Screenshot capture is excluded from this input regression so temporary render settings cannot affect pointer coordinates.
