<!--
Translator notes:
- This is the English README source for translated documentation copies.
- Keep file paths, URLs, LiveryGUI, Phantom Brigade, and keyboard/control names accurate.
- Prefer Phantom Brigade's existing localized terminology for mech, unit, pilot, livery/paint scheme, base, and mission briefing.
-->

![workshop_preview.png](workshop_preview.png)

## Supported Languages

<table>
  <tr>
    <td>English</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/README.md">README</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>简体中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/README.md">说明文档</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseSimplified/FAQ.md">常见问题</a></td>
  </tr>
  <tr>
    <td>繁體中文</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/README.md">說明文件</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/ChineseTraditional/FAQ.md">常見問題</a></td>
  </tr>
  <tr>
    <td>Français</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/README.md">Lisez-moi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/French/FAQ.md">FAQ</a></td>
  </tr>
  <tr>
    <td>Deutsch</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/README.md">Liesmich</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/German/FAQ.md">Häufige Fragen</a></td>
  </tr>
  <tr>
    <td>Italiano</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/README.md">Leggimi</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Italian/FAQ.md">Domande frequenti</a></td>
  </tr>
  <tr>
    <td>日本語</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/README.md">はじめに</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Japanese/FAQ.md">よくある質問</a></td>
  </tr>
  <tr>
    <td>한국어</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/README.md">읽어보기</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Korean/FAQ.md">자주 묻는 질문</a></td>
  </tr>
  <tr>
    <td>Português Brasileiro</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/README.md">Leia-me</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/PortugueseBrazilian/FAQ.md">Perguntas frequentes</a></td>
  </tr>
  <tr>
    <td>Русский</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/README.md">Прочитайте</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Russian/FAQ.md">Часто задаваемые вопросы</a></td>
  </tr>
  <tr>
    <td>Español</td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/README.md">Léeme</a></td>
    <td><a href="https://github.com/three-3s/pb-livery-gui/blob/main/docs/Spanish/FAQ.md">Preguntas frecuentes</a></td>
  </tr>
</table>

## Overview

In the base game, mech paint schemes ("liveries") are limited to preset color and material combinations.

Can't find quite the right color combination?

This mod adds a livery editor that lets you customize livery colors and material properties, save them as new liveries, and apply them to your mechs.

This mod is compatible with Phantom Brigade v2.0.

Basic usage

1. Go into the base's list of units.
2. Select a mech, then open the Livery selection page.
3. Click the new "sliders" button to toggle the livery editor UI.
4. Select an existing livery to use as a starting point.
5. Enter a new name if you want to create a new editable copy.
6. Adjust colors and material settings using the sliders.
7. Click Save to store the livery.
8. The livery is ready to use.

These liveries are saved to a single common location, and are usable across all your save games.

Additional controls:

* Reset: Reverts the current livery to its last saved state.
* Favorite: Toggles PB's built-in favorited-livery flag, keeping the livery near the front of the list.
* Right-click-and-drag: A higher-precision adjustment. Can combine with left-Shift/Control/Alt (faster/slower).
* Pilot mode: Lets you edit a livery set for a pilot instead of the mech. Assigned pilot liveries are layered over the mech's own base livery in the base and briefing screens; unpainted pilot slots fall back to the mech's livery.

The game also lets you use the "move camera" keys to change the camera angle.

([Steam Workshop page here](https://steamcommunity.com/sharedfiles/filedetails/?id=3649585996))

For questions and troubleshooting, consider looking at the [FAQ](https://github.com/three-3s/pb-livery-gui/blob/main/FAQ.md).

Bonus features:

* (developer tool) In the dev-console `ui.view-enter uitools`: Added ability to select sprite entries by clicking on sprites in the atlas.
