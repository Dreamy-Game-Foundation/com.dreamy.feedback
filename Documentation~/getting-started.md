# Getting started

Start with the [ready-to-use sample guide](../Samples~/BasicFeedbackSample/README.md): GameFeedbackRig, FeedbackPlayer, four reward presets and game HUD setup. No generator or Economy sample is required.

1. Install Dreamy Feedback and its UPM dependencies. Core is optional for base playback.
2. Create a FeedbackRoot and FeedbackHost. Assign the root, roots/canvases for enabled channels, optional databases and an explicit world camera. Keep UI effect roots as RectTransforms below a configured Canvas.
3. Create a Dreamy/Feedback/Definition asset; set a stable ID. Expand Root, choose Sequence/Parallel or a primitive, then Add child. Configure only the fields shown for each node. VFX nodes require a positive maximum Duration. Root and children are managed-reference data; do not edit shared assets during playback.
4. Send a FeedbackRequest with source/target/world position/amount/intensity. Builder creates only request/context, never graph structure. Amount is a long; text nodes replace `{amount}`. Icon count is authored and bounded independently of amount. Intensity scales punch/shake/VFX; generic Unity vibration cannot express distinct intensity patterns.
5. Retain handles. Stop cancels remaining steps, active primitives and delays. Complete skips waits, finishes current effects and dispatches unstarted one-shots once. It can create a presentation burst; it never grants wallet rewards. Completion resolves to Completed, Stopped or Faulted.

WorldPosition is a world coordinate. FloatingText projects it through the explicit camera to its UI root. IconSource/Target are UI anchors: icon positions are transformed into the effect root's local canvas units. Spread, ArcHeight and Size remain stable under CanvasScaler; Target is followed live while anchors move.

Repeated UI Punch replaces the previous session on the same transform and restores its captured scale. Camera shake replaces the previous shake on its rig and restores its captured local position. Use a camera offset root below the follow rig and above the camera. Screen Flash replaces its overlay playback. Replaced children become Stopped, which stops their composite. VFX/text/icon leases are independent.

Enable DOTween by installing `com.dreamy.feedback.dotween` and its DOTween dependency, then assign DotweenFeedbackProvider. Assign the provider explicitly before initializing the host. Shipped portable rigs/scenes use the Unity backend and work without DOTween. All five icon styles are available through options: Straight, Arc, ScatterMagnet, Fountain, Spiral.

Basic's shipped input module uses StandaloneInputModule: select Old Input Manager/Both or replace it in Input System-only projects. Import official TMP Essential Resources once. Generic mobile haptics respect Enabled and throttle; physical vibration still needs device validation.
