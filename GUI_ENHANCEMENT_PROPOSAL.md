# GUI Enhancement Proposal for Dotman

## Overview

This document outlines two distinct design directions and a set of functional UX improvements to transform **Dotman** into a "geeky", intuitive, and highly usable dotfile manager for macOS.

The goal is to move away from the generic corporate dashboard look and embrace a unique identity that resonates with developers and power users.

## Functional UX Improvements (Common)

Before diving into aesthetics, these functional enhancements are critical for a "straightforward and useful" experience on macOS.

### 1. macOS Native Integration
- **Global Menu Bar**: Move app menus (File, Edit, View) to the native macOS global menu bar using Avalonia's `NativeMenu`.
- **Dock Integration**: Show sync status badge on the Dock icon (e.g., a red dot if there are unpushed changes).
- **Keyboard Shortcuts**: Standardize shortcuts (e.g., `Cmd+,` for Settings, `Cmd+R` for Refresh/Sync).

### 2. "Command Palette" (`Cmd+K` / `Cmd+P`)
- Implement a modal search bar that allows users to:
    - Jump to a file.
    - Trigger actions (Sync, Add File, Restore).
    - Toggle settings.
- **Why:** Extremely keyboard-friendly and fits the "geeky" vibe perfectly.

### 3. Visual Diff Viewer
- **Current Limitation:** The `IVCSBackend` currently only provides a status string.
- **Proposal:** Extend `IVCSBackend` to include `Task<string> GetDiffAsync(string filePath)`.
- **UI:** A split-view or unified diff viewer within the app so users see exactly *what* changed before syncing.

### 4. Drag-and-Drop
- Allow users to drag files from Finder directly onto the Dotman window to add them to tracking.

---

## Design Concept A: "The Retro Terminal" (CRT / Hacker)

*A love letter to the golden age of computing. High contrast, text-heavy, keyboard-driven.*

### Visual Language
- **Typography:** Strictly Monospace. Recommended: **JetBrains Mono**, **Fira Code**, or **VT323** for a more "old-school" feel.
- **Color Palette:**
    - Background: Deep Black (`#0C0C0C`) or Dark Blue (`#000018`).
    - Primary: Phosphor Green (`#33FF00`) or Amber (`#FFB000`).
    - Accent: Cyan (`#00FFFF`) for headers.
    - Error: Bright Red (`#FF0000`).
- **Styling:**
    - **No rounded corners.** Everything is sharp (0px border radius).
    - **Borders:** Use ASCII-style graphical borders or solid 2px lines.
    - **Scanlines:** A subtle overlay opacity mask to mimic CRT scanlines.
    - **Glow:** TextShadows to simulate phosphor bloom.

### UI Components
- **Buttons:** Represented as text blocks like `[ SYNC NOW ]`. On hover, they invert colors (Background becomes Green, Text becomes Black).
- **Navigation:** A sidebar that looks like a directory tree (`Tree` command output).
- **Progress Bars:** ASCII style: `[#####.....] 50%`.

### Mockup Description
> The window looks like a terminal emulator. The title bar is minimal.
> The dashboard displays stats in a table format drawn with lines.
> "Syncing..." shows a scrolling log of git commands.

### Technical Implementation
- **Font:** Bundle `VT323` or generic Monospace.
- **Avalonia Styles:**
    - Remove `FluentTheme` rounding.
    - Use `BoxShadow` for "bloom" effects.
    - Custom `DrawOperation` or `ShaderEffect` for scanlines (optional, advanced).

---

## Design Concept B: "Pixel Art" (Cozy / 8-Bit)

*Playful, chunky, and nostalgic. Reminiscent of 16-bit RPGs or Stardew Valley.*

### Visual Language
- **Typography:** Bitmap/Pixel fonts. Recommended: **Press Start 2P** or **Unifont**.
- **Color Palette:**
    - "Gruvbox" or "Catppuccin" palette (warm, earthy, or pastel tones).
    - Beige/Cream backgrounds for panels, dark borders.
- **Styling:**
    - **Chunky Borders:** 4px solid borders, creating a "blocky" feel.
    - **Drop Shadows:** Hard, non-blurred shadows (offset 4px, 4px).

### UI Components
- **Icons:** 16x16 or 32x32 pixel art icons (floppy disk for save, sword for conflict, potion for heal/sync).
- **Buttons:** Beveled pixel art buttons that physically "depress" (transform Y+2px) when clicked.
- **Mascot:** A small 8-bit character ("Dotman") that sits in the corner and has speech bubbles for status ("All good!", "Conflicts detected!").

### Mockup Description
> The app looks like a game menu.
> The "Sync" button is a big, chunky "START" button.
> File lists look like inventory slots in an RPG.
> Statuses are indicated by hearts (Full heart = Synced, Broken heart = Error).

### Technical Implementation
- **Font:** Bundle `Press Start 2P.ttf`.
- **Assets:** Requires a set of pixel art SVGs or PNGs.
- **Avalonia Styles:**
    - `BorderThickness="4"`.
    - `CornerRadius="0"` (or stepped corners using ClipGeometry).
    - `RenderOptions.BitmapInterpolationMode="HighQuality"` (NearestNeighbor) for sharp scaling of pixel art.

---

## Recommendation & Next Steps

Based on the goal of making it "easy and straightforward" while keeping it "geeky/silly":

**I recommend a hybrid approach leaning towards Concept A (Retro Terminal) but with modern UX usability.**
Why?
1. It feels more "native" to a dotfile tool (which manipulates shell scripts).
2. It's easier to implement purely with CSS/XAML styling without needing custom art assets (unlike Pixel Art).
3. It scales better for varying amounts of text (filenames can get long).

### Proposed Implementation Plan

1. **Phase 1: Foundation (Theme Engine)**
   - Refactor `App.axaml` to support switching themes.
   - Import a Monospace font family.

2. **Phase 2: The "Hacker" Theme**
   - Create `HackerTheme.axaml`.
   - Implement the "High Contrast" colors.
   - Build the `[ BUTTON ]` styles and ASCII borders.

3. **Phase 3: macOS Polish**
   - Implement `NativeMenu`.
   - Add Drag-and-Drop support to `FilesView`.

4. **Phase 4: Functional Upgrades**
   - Implement `Cmd+K` Command Palette.
   - Update `Dotman.Core` to support Diffing.
