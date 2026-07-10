# Getting Started

1. Add `FeedbackRoot` to a scene GameObject.
2. Create databases from `Tools/Dreamy/Feedback`.
3. Assign game-specific prefabs to database entries.
4. Initialize services from your bootstrap code.
5. Optionally call `FeedbackServiceRegistry.Register(...)` to publish interfaces into Dreamy Core `ServiceLocator`.
6. Call individual module services directly from gameplay code.

The package ships framework code only. It intentionally does not include production VFX, UI, audio, haptic native plugins, or game-specific IDs.
