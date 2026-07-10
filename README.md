# Dreamy Feedback

Reusable Unity feedback package for mobile and casual games. Modules are independent services: VFX, haptic, floating text, icon fly, screen flash, camera shake, and optional feedback sequence orchestration.

This package targets the current Dreamy Unity 6000 baseline and depends on Dreamy Core plus UniTask. Services remain plain C# objects; Dreamy Core integration is provided through the optional `FeedbackServiceRegistry` helper.

## Setup

Create a `FeedbackRoot` in your bootstrap scene and initialize only the services you need.

```csharp
var vfx = new VfxService();
vfx.Initialize(vfxDatabase, feedbackRoot.WorldVfxRoot);

var haptic = new HapticService();

var floatingText = new FloatingTextService();
floatingText.Initialize(floatingTextDatabase, feedbackRoot.FloatingTextRoot);

var iconFly = new IconFlyService();
iconFly.Initialize(feedbackRoot.IconFlyRoot);

var screenFeedback = new ScreenFeedbackService();
screenFeedback.Initialize(feedbackRoot.ScreenFeedbackRoot);

var cameraShake = new CameraShakeService();
cameraShake.Initialize(cameraShakeDatabase, Camera.main);

FeedbackServiceRegistry.Register(
    vfx,
    haptic,
    floatingText,
    iconFly,
    screenFeedback,
    cameraShake);
```

## Usage

```csharp
vfx.Play("level_up", player.transform.position);
haptic.Play(HapticType.Medium);
floatingText.Play("+100", coinWorldPosition);
iconFly.Fly(iconFlyOptions);
screenFeedback.Flash(ScreenFlashOptions.WhiteFlash());
cameraShake.Shake("small");
```

## Editor Tools

Use `Tools/Dreamy/Feedback/Create/...` to create databases. Select databases and run `Tools/Dreamy/Feedback/Generate IDs` to generate constants under `Assets/DreamyFeedbackGenerated`.

Run `Tools/Dreamy/Feedback/Validate All` to check empty IDs, duplicate IDs, missing prefabs, invalid lifetimes, and incomplete sequence actions.

## Constraints

Version 0.1.0 uses UniTask for internal async timing. It does not depend on DOTween, Addressables, Cinemachine, Firebase, Ads, IAP, or Analytics. Package metadata targets Unity 6000.0 for this project.
