# Computer Graphics (Unity)

Procedural graphics coursework in Unity, progressing from **2D raster drawing** → **procedural 3D meshes** → a **cinematic short** with lighting, shading, and animation.

**Author:** Junji Liao (Elana Liao)  
**Unity:** 6000.6.0f1  
**Project folder:** [`computer_graphic_CS/`](./computer_graphic_CS)

---

## Overview

| Stage | Focus | Main location |
|-------|--------|----------------|
| **2D Drawing** | Pixel buffers, lines, transforms, simple animation | `Assets/2D Drawing/` |
| **3D Graphics** | Procedural meshes (cubes, sweeps, keys, hat) | `Assets/3D Graphics/` |
| **Lighting, Shading & Animation** | Custom shaders + Assignment 3 movie | `Assets/Lighting, Shading, and Animation/` |

---

## Screenshots

<!-- > Drop your images into a folder such as `docs/screenshots/` and replace the paths below. -->

### 2D Drawing

<!-- ![2D Tutorial 2](docs/screenshots/2d-tutorial1.png) -->
![2D Tutorial 1](docs/screenshots/2d-tutorial1.png) 
<!-- ![2D Tutorial 2](docs/screenshots/2d-tutorial2.png) -->
![2D Tutorial 2](docs/screenshots/2d-tutorial2.png)
<!-- ![2D Tutorial 3](docs/screenshots/2d-tutorial3.png) -->
![2D Tutorial 3](docs/screenshots/2d-tutorial3.png)

### 3D Graphics
<!-- ![Bowler hat / keys](docs/screenshots/3d-objects.png) -->
![Bowler hat / keys](docs/screenshots/3d-objects.png)

### Assignment 3 — Movie

<!-- ![Movie wide shot](docs/screenshots/a3-movie-01.png) -->
![Movie wide shot](docs/screenshots/a3-movie-01.png)
<!-- ![Small spider on path](docs/screenshots/a3-movie-02.png) -->
![Small spider on path](docs/screenshots/a3-movie-02.png)
<!-- ![Web & big spider](docs/screenshots/a3-movie-03.png) -->
![Web & big spider](docs/screenshots/a3-movie-03.png)
<!-- ![Fly exit / close-up](docs/screenshots/a3-movie-04.png) -->
![Fly exit / close-up](docs/screenshots/a3-movie-04.png)

---

## Getting started

1. Open with Unity Hub and let it resolve the editor version).
2. Open the project at `computer_graphic_CS/`.
3. Load a scene from the tables below and press **Play**.

---

## 1. 2D Drawing

Full-screen orthographic quads with a `Texture2D` framebuffer. Drawing helpers (DDA lines, fills, transforms) build up simple scenes and animation.

| Scene | Script | Notes |
|-------|--------|--------|
| `Tutorial 1.unity` | `Tutorial1.cs` | Line drawing / basic raster ops |
| `Tutorial 2.unity` | `Tutorial2.cs` | Hexagon + matrix transforms |
| `Tutorial 3.unity` | `Tutorial3.cs` | Tower / pendulum-style swing animation |

**Path:** `computer_graphic_CS/Assets/2D Drawing/`

---

## 2. 3D Graphics

Meshes are built in code via shared utilities (`MeshUtilities`: cube, cylinder, pyramid, sweep, spline profiles) and assignment task scripts.

### Tutorial scenes

| Scene | Related makers |
|-------|----------------|
| `Tutorial3.unity` | Early mesh experiments |
| `Tutorial4.unity` | Cylinder / lamp-style construction |
| `Tutorial5.unity` | Spline / sweep objects |

### Assignment 1 — procedural objects

**Scene:** `Assignment2.unity`  
**Path:** `computer_graphic_CS/Assets/3D Graphics/`

| Object | Script | Description |
|--------|--------|-------------|
| BowlerHat | `A2Scripts/Task1.cs` | Hat crown, band, and brim via profile sweep |
| Key | `A2Scripts/Task3.cs` | Flat key with coded bit pattern |
| OldKey | `A2Scripts/Task4.cs` | Ye-olde key with stem and overlapping holes |

Supporting makers: `CubeMaker.cs`, `CylinderMaker.cs`, `PyramidMaker.cs`, `LampMaker.cs`, `MakeObjectFromSpline.cs`.

---

## 3. Lighting, Shading & Animation

### Custom shaders

| Asset | Role |
|-------|------|
| `Custom Phong Shader.shader` | Per-pixel Phong lighting |
| `Custom Gouraud Shader.shader` | Gouraud (vertex) shading |
| Scenes | `Week9 Lighting.unity`, `W10_Animation.unity` |

### Assignment 3 — Movie

Short cinematic: a **small spider** walks a path, a **procedural web** ripples, a **big spider** waits on the web, and a **fly** buzzes nearby before exiting — with camera follow, eye look-at, and audio.

**Main scene:** `Movie.unity`  
**Path:** `computer_graphic_CS/Assets/Lighting, Shading, and Animation/Assignment3/`

#### How to play

1. Open `Assignment3/Movie.unity`.
2. Ensure `MovieManager` has prefab references (small spider, big spider, fly, web) and audio clips assigned.
3. Press **Play** — the sequence runs automatically.

#### Movie beats (high level)

| Phase | What happens |
|-------|----------------|
| **A** | Small spider walks a straight path |
| **B** | Semicircle turn; web animation starts |
| **C** | Approach the web area |
| **D** | Climb onto the web; face the big spider |
| **E** | Big spider climbs toward the small spider |
| **F** | Fly exits the scene |

Camera follows the small spider, then holds a brief eye-focus close-up near the web.

#### Scripts

| Script | Role |
|--------|------|
| `MovieManager.cs` | Timeline, camera, audio, character orchestration |
| `SmallSpiderMaker.cs` / `SmallSpiderAniController.cs` | Build & animate small spider |
| `BigSpiderMaker.cs` / `BigSpiderAniController.cs` | Build & animate big spider |
| `FlyMaker.cs` (`MakeFly`) / `FlyAniController.cs` | Build & animate fly (wings, legs) |
| `SpiderWebMaker.cs` / `WebAniController.cs` | Procedural web mesh + ripple |
| `RibbonMaker.cs` | Möbius strip / textured ribbon demo (`Texture/`) |
| `StaticFly.cs` | Static preview (`Static_demo/View_Scene.unity`) |

#### Related scenes

| Scene | Purpose |
|-------|---------|
| `Movie.unity` | Full cinematic |
| `Static_demo/View_Scene.unity` | Static fly / look-dev |
| `Texture/Task2 Scene.unity` | Ribbon / texture task |

Prefabs live under `Assignment3/Prefabs/` (Elana objects + environment assets).

---

## Project structure

```
computer_graphic_CS/
├── Assets/
│   ├── 2D Drawing/                 # Raster tutorials
│   ├── 3D Graphics/                # Procedural meshes + Assignment2
│   │   └── A2Scripts/              # Task1, Task3, Task4
│   ├── Lighting, Shading, and Animation/
│   │   ├── Custom Phong / Gouraud shaders
│   │   └── Assignment3/            # Movie + makers + prefabs
│   └── Utilities/                  # MeshUtilities, editor drawers, wireframe
├── Packages/
└── ProjectSettings/
```

---

## Utilities

Shared helpers under `Assets/Utilities/`:

- **`MeshUtilities.cs`** — cube, cylinder, pyramid, sweep, spline profile/path helpers  
- **`Texture2DExtensionsPixels.cs`** — pixel drawing helpers for 2D tutorials  
- **`DoubleSidedWireframe.shader`** — debug wireframe  
- Editor drawers for `Vector4` / `Matrix4x4` / lockable vectors  

---

## License

MIT — see [`LICENSE`](./LICENSE).
