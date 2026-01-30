# Candy Mayhem - Match 3 Game

A robust, feature-rich Match-3 puzzle game built in Unity. Designed with scalability and clean architecture in mind.

## 🎮 Features

*   **Classic Match-3 Gameplay**: Smooth matching logic, swapping, and cascades.
*   **Visual Level Editor**: Built with [Odin Inspector](https://odininspector.com/) for easy level design.
*   **Performance Optimized**: Uses object pooling for tiles and effects.
*   **Juicy Animations**: Powered by [DOTween](http://dotween.demigiant.com/) for polished visuals.
*   **Flexible Level System**: Support for goals, limited moves, custom layouts, and blockers.
*   **Special Gems**: Logic for bombs, rockets, and other special power-ups.

## 🛠 Tech Stack

*   **Engine**: Unity 2022.3+ (Recommended)
*   **Language**: C#
*   **Dependencies**:
    *   [DOTween (HOTween v2)](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
    *   [Odin Inspector](https://odininspector.com/) (Required for editor tools)

## 🚀 Getting Started

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/DennyEZ/candyMayhem.git
    ```
2.  **Open in Unity**: Add the project via Unity Hub.
3.  **Install Dependencies**:
    *   Ensure **Odin Inspector** is installed (or removed if you want to strip editor tools).
    *   Import **DOTween** via Package Manager or Asset Store if missing.
4.  **Open a Scene**: Go to `Assets/Scenes` and open the main gameplay scene.
5.  **Play**: Hit the Play button to start matching!

## 📂 Project Structure

*   `Assets/Scripts/Core`: Main game logic (GameManager, BoardController).
*   `Assets/Scripts/Levels`: Level data definitions and editor tools.
*   `Assets/Scripts/Views`: UI and visual components.
*   `Assets/Prefabs`: pre-configured game objects.

## 📝 Level Design

Level designers can create new levels by creating `LevelData` assets:
1.  Right-click in Project view -> `Create` -> `Match3` -> `Level Data`.
2.  Set board dimensions, available colors, and move limits.
3.  Use the visual grid to place blockers or pre-set tiles.

For a detailed walkthrough, see [SETUP_GUIDE.md](Assets/SETUP_GUIDE.md).

## 📄 License

[MIT License](LICENSE)
