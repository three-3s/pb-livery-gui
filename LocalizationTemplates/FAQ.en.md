<!--
Translator notes:
- This is the English FAQ source for translated documentation copies.
- Keep file paths, URLs, LiveryGUI, Phantom Brigade, Player.log, and file extensions accurate.
- Prefer Phantom Brigade's existing localized terminology for mech, unit, pilot, livery/paint scheme, base, and mission briefing.
-->

# Questions & Answers

## Which languages are supported?

The mod has basic translations for all languages currently supported by the game.

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

## Do I need to start a new game?

No. This mod should fully support existing saves.

## Can I still use other mods that add liveries?

Yes. You won't be able to overwrite them, but you can copy them and edit the copies.

## Where are the livery files saved?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

For example:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

Feel free to package these files into a mod of liveries.

## Is it possible to use larger/smaller values than the sliders allow?

Yes, but the mod does not currently support this. For colors at least, negative values "break" the color and make it black. Colors that are too bright will eventually "break" the color and start showing increasingly large black portions. The material W parameters can trigger some major rendering glitches, particularly with certain effect XYZ values.

## Can I change a built-in livery or a livery provided by other mods?

Not directly, but you can save a copy of the livery and edit the copy.

## What if I uninstall this mod?

It should be OK. The liveries that no longer exist will look like default liveries. But you may need to re-assign affected parts to use different liveries, or right-click in the liveries page to clear out the liveries to "no livery". If you had an old "upper torso" livery that no longer exists, changing the livery of the whole mech or whole torso can leave the upper torso stuck on "default livery" until it is cleared (right-click).

Other than that, the game seems to handle disabling/uninstalling the mod well enough.

## I'm seeing big white circles?

The normal range for values is between 0 and 1. Anything else is experimental.

The major culprit for the "big white circles" rendering glitch is having a primary, secondary, or tertiary material W value that is too large, positive or negative. You can sometimes work around this by adjusting the corresponding effect XYZ slider (for effect, X=primary, Y=secondary, Z=tertiary), or reducing the RGB color of the problem part (make R+G+B be less). Reducing the RGB may make it dimmer, but some brightness can be restored by using a negative A component for that color.

Note: The big white circles may only be visible from certain angles, so use the "move camera" buttons (WASD by default, for QWERTY keyboards) to examine the mech from multiple angles. Try to overshoot the elimination of the white circles by a bit, to help ensure they don't pop up when seen from just the right angle.

## The "Supporter DLC" sliders don't do anything?

The effects are only visible if the Supporter Upgrade DLC is purchased and installed. Also, to start with, make sure the colors (XYZ=RGB) are, say, between 0.5 and 1.0, and you can adjust them from there. The W value should be set to something other than "none".

## The "effect W" slider doesn't do anything?

As far as I can tell, this value has no effect.

## What are the other livery values in the livery .yaml files? Can I edit those?

I haven't looked into all of them yet, and am not sure what if anything they do. The mod doesn't currently support anything other than primary/secondary/tertiary colors/materials, and the effects.

## Is there a way to delete liveries?

Not from inside the mod at the moment. But you can manually delete any of them from AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

This should be safe, other than your mech parts will be using the default livery until you specifically reassign liveries to the affected mech parts.

## Something went wrong?

There may be additional information in C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

In particular, look for [LiveryGUI] log messages, or log messages about problems in "LiveryGUI" files.
