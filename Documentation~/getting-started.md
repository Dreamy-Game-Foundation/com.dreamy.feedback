# Getting started

See the package [README](../README.md) for install, sample prerequisites, ownership and migration.

1. Import **Basic Feedback**, open `Generated/FeedbackDemo.unity`, press Play and try all seven channels.
2. Install Dreamy Economy/UI/Audio and dependencies, import **Feedback Economy** and open `Generated/FeedbackEconomyDemo.unity`.
3. Grant 100 coins, retry and close/reopen. The first grant changes balance; retry does not replay feedback.
4. Inject your wallet, audio and feedback owner at the composition root. Observe confirmed events; keep save/reward mutations outside feedback.
5. Initialize FeedbackHost with explicit root, databases and camera. Use UI RectTransforms under a canvas for text/icons. Dispose/shut down the owner on unloading.

Editor tools and sample builders use `Dreamy/Feedback`. Save open scenes before invoking builders, which create new output folders and retain user assets.

Before running either sample in a fresh project, import **Window > TextMeshPro > Import TMP Essential Resources** once. UGUI 2.0 TMP still needs its project-wide TMP Settings/default style resources in players. The sample uses its own static font and shaders, but does not ship a second `Resources/TMP Settings` asset that could conflict with your project.
