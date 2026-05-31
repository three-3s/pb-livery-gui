# Questions & Answers

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
