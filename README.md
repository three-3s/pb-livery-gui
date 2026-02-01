![workshop_preview.png](workshop_preview.png)



## Overview

In the base game, mech paint schemes (“liveries”) are limited to pre-set color and material combinations.

This mod adds a livery editor that lets you customize livery colors and material properties, save them as new liveries, and apply them to your mechs.

Basic usage

1. Go to the Base.
2. Select a Mech, then open the Livery selection page.
3. Click the new “Sliders” button to toggle the livery editor UI.
4. Select an existing livery to use as a starting point.
5. Enter a new name, then click the Clone button (the 2×2 grid with a “+” icon).
6. Adjust colors and material settings using the sliders.
7. Click Save to store the livery.
8. The livery is ready to use.

Additional controls:

* Reset: Reverts the current livery to its last saved state.
* Favorite: Toggles the (PB built-in) 'favorited livery' flag. (Keeps livery at front of list.)
* Right-click-and-drag: A higher-precision adjustment. Can combine with left-Shift/Control/Alt (faster/slower).

These liveries are saved to a single common location, and are usable across all save games on that machine.

Created after Phantom Brigade v2.0.

([Steam Workshop page here](https://steamcommunity.com/sharedfiles/filedetails/?id=3649585996))



## Questions & Answers

### Where are the livery files saved?

AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml

For example:
C:/Users/yourUserID/AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI

### Is it possible to use larger/smaller values than the sliders allow?

Yes, but the mod does not currently support this. (For colors at least, negative values 'break' the color and make it black. Colors that are too bright will eventually 'break' the color and start showing increasingly large black portions.)

### I can't change built-in liveries, or liveries provided by other mods?

No. But you can copy them and edit the copy.

### What if I uninstall this mod?

It seems to be OK. The liveries that no longer exist will look like default liveries. But you may need to re-assign affected parts to use different liveries, or right-click in the liveries page to clear out the liveries to "no livery". (Otherwise, if you had an old "upper torso" livery that no longer exists, if you change the livery of the whole mech or whole torso, the upper torso will still be stuck on "default livery".)

But otherwise the game seems to handle disabling/uninstalling the mod well enough.

### What are the other livery values in the livery .yaml files? Can I edit those?

I haven't looked into all of them yet, and am not sure what if anything they do. The mod doesn't currently support anything other than primary/secondary/tertiary colors/materials, and the effects.

### Is there a way to delete liveries?

Not in-mod at the moment. But you can go manually delete any of them from AppData/Local/PhantomBrigade/ModSavedData/LiveryGUI/*.yaml.

This should be safe, other than your mech parts will be using the default livery until you specifically reassign liveries to the affected mech parts.

### Something went wrong?

There may be additional information in C:\Users\yourUserId\AppData\LocalLow\Brace Yourself Games\Phantom Brigade\Player.log.

In particular, look for \[LiveryGUI\] items.

### Can I still use other mods that add liveries?

Yes. You won't be able to overwrite them, but you can copy them and edit the copies.
