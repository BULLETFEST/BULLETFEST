# BULLETFEST

## Developing:

Required Unity Version: `2021.3.27f1` \
Recommended IDE: `Visual Studio Code` \
If you're using VSCode, follow [this](https://code.visualstudio.com/docs/other/unity) setup.

## Branches & Pushes

Pushing to `main` only with permission from @EliasVal ! \
`git checkout` from the `dev` branch, make your changes and when ready, merge the `dev` branch with the branch you've created, and then close it. \
The game is automatically build when a push to `main` occurs, make sure the game's version is different from the previous builds, or the current build action will fail.

## Versioning

Once we hit 1.0, we will follow [SemVer](https://semver.org/). Until then here's the guide:

**`X.Y.Z-STAGE.BUILD`**

- X - `Major`: a change to this number indicates a big change, usually breaking.
- Y - `Minor`: not a huge change but sizeable enough
- Z - `Patch`: small updates, usually very small features, bug fixes, etc.
- `STAGE`: currently `alpha`, later on `beta`, and when release hits, this section will be removed.
- `BUILD`: The build number of the current version. This number is increased automatically every time the game builds.

## Testing

Recommended way of testing is using two machines, but if limited to one, follow [this](https://github.com/FakeByte/EpicOnlineTransport/tree/master#testing-multiplayer-on-one-device) guide. \
**Back-end**: Set `testMode` (in `Assets/Scripts/FirebaseManager.cs:13`) to `false`, otherwise the code will try to connect to `localhost:3000`.
