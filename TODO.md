// PRIORITY ITEMS:
//  - This may or may not need some future proofing. E.g., copy the definition of the Serializable
//    Data-livery container, so we can always load the consistently-defined version, then copy
//    the values over item-by-item into an actual Livery. Or maybe it's OK to let the liveries
//    fail to load if they change the livery definition. But I'm a bit concerned they could make
//    a trivial change that would break the livery-loading. Presumably there's a safer pipeline
//    built into the game. TODO: At least test it and confirm that eg adding/removing/renaming a
//    field from the yaml does in fact prevent the livery from loading.

// BUGS:
//  - The livery-name text-box is initially "-". Doing just about anything makes it work, but
//    I haven't figured out how to get it to refresh or unstuck before whatever it is fixes it.
//    Sort of worked around by making the text field visible before 'show GUI' is clicked.

// TODO.post-release:
//  - Ergonomics: Clearer highlight of current livery? Favorite-icon color? Jump to current?
//    Next/prev buttons? Clone-this-then-reset-orig? Could be as reset button labeled for prev.
//  - Might be nice to apply exponential scaling curve to slider values, though that might need
//    to be more or less built-in (or implemented into the slider).
//  - Consider adding tooltips to buttons.
//  - Ideally would want to be able to delete liveries.
//  - Maybe a button to open the directory containing the livery .yaml files?
//  - Maybe mark the liveries in the selection-list eg with a different color if they've been
//    created (or overwritten by) this mod? Or have unsaved changes?
//  - There might be some desire to go beyond the value-limits currently hardcoded. E.g., maybe
//    up to 3.0 instead of just 2.0. Or maybe some params are effective to much higher..?
//    Could/should at least let it support not-clamping upon load just to fit in the slider
//    limits. But maybe it would be possible to create a set of widgets where you could specify
//    min/max limits, activate picker mode, and apply those limits to any clicked-on sliders.
//  - Fix the label above the text input field from "Name" to eg "Livery Name".
//  - Remove the forced-capitalization on the text input field. (Seems to only affect display,
//    not the actual string.)
//  - Prefix-intercept and prevent CIViewBaseLoadout.UpdateCamera when the text-input field is
//    selected (like is done for headerInputUnitName.isSelected in that module).
//  - It seems like the liveryKey=null should map to "default" (?). But that's not hooked up right
//    now, and the sliders do nothing for the null livery. No reason that case couldn't be made
//    to work, though it'd possibly affect many operations needing to know that null="default"
//    mapping.
//
// ADDITIONAL POSSIBLE IMPROVEMENTS:
//  - There's a fair amount of brute-force item-by-item a.primR=b.primR,a.primG=b.primG etc.