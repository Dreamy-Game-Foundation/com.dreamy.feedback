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
