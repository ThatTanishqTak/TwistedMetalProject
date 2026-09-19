# Twisted Metal Project

A **Unity multiplayer vehicle-combat prototype** developed for Connected Games Development.

Players are assigned to vehicle teams with separate **Driver** and **Shooter** roles. The driver controls movement while the shooter operates the turret.

The current arena spawning implementation creates two team vehicles, making four players the natural configuration for testing both roles on both teams.

## Gameplay Systems

* Lobby creation and joining through Unity Lobby.
* Anonymous Unity Services authentication.
* Host/client networking through Netcode for GameObjects.
* Networked player data and team-role assignment.
* Character selection and ready-state handling.
* Network scene transitions.
* Server-side vehicle spawning and driver input handling.
* Turret aiming and firing.
* Networked health, damage, death and respawn behaviour.
* Role-specific camera management and health UI.

## Technology

* **Unity:** 6000.0.37f1.
* **Language:** C#.
* **Rendering:** Universal Render Pipeline.
* **Networking:** Netcode for GameObjects 2.2.0.
* **Services:** Unity Authentication and Lobby.
* **Camera tools:** Cinemachine.
* **Vehicle foundation:** PROMETEO Car Controller integration.

The project version is recorded in `ProjectSettings/ProjectVersion.txt`. Package versions are recorded in `Packages/manifest.json`.

## Open the Project

```bash
git clone https://github.com/ThatTanishqTak/TwistedMetalProject.git
```

1. Add the repository folder through Unity Hub.
2. Open it with Unity 6000.0.37f1.
3. Allow Unity to import assets and restore packages.
4. Configure a Unity Services project with Authentication and Lobby available.
5. Open `Assets/Scenes/MainMenu.unity`.

Use the existing project settings and scene references when preparing a build.

## Multiplayer Setup

The project creates a Lobby and then starts a Netcode host or client.

**Relay allocation and connection-data exchange are not implemented in the current lobby code.** Joining a lobby by code therefore does not configure the network transport automatically.

Before testing:

* Configure the NetworkManager transport for the intended local or LAN connection.
* Make sure clients can reach the host.
* Use a four-player lobby to exercise the current two-vehicle team setup.
* Check the Console for service authentication and connection failures.

Internet play needs additional transport/session setup.

## Scene Flow

The configured scenes are:

1. `MainMenu`
2. `CharacterSelect`
3. `Lobby`
4. `Arena`

They are listed in `ProjectSettings/EditorBuildSettings.asset`.

## Controls

| Role    | Input                 | Action             |
| ------- | --------------------- | ------------------ |
| Driver  | W / S or Up / Down    | Throttle / reverse |
| Driver  | A / D or Left / Right | Steering           |
| Driver  | Space                 | Handbrake          |
| Shooter | Mouse                 | Aim the turret     |
| Shooter | Left mouse button     | Fire               |

## Code Organisation

* `Assets/Scripts/Manager/`: Game flow, networking and camera coordination.
* `Assets/Scripts/Multiplayer/`: Lobby and ready-state UI.
* `Assets/Scripts/Gameplay/Car/`: Vehicle input integration.
* `Assets/Scripts/Gameplay/Turret/`: Turret control.
* `Assets/Scripts/Gameplay/Shooting/`: Weapons and firing.
* `Assets/Scripts/Gameplay/Health/`: Networked health.
* `Assets/Scripts/Gameplay/Spawning/`: Team vehicle spawning.
* `Assets/Scripts/Player/`: Player and role data.

## Current Scope

This is a coursework prototype. Connection recovery, lobby lifecycle management, broader team-count support and production networking behaviour need further development.

## Credits

The project integrates third-party assets and packages, including PROMETEO vehicle controls, environment assets and visual effects. Those components remain subject to their original licences and attribution requirements.
