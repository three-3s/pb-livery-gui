using HarmonyLib;
using PhantomBrigade.Data;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LiveryGUIMod
{
    // Patch CIHelperLoadoutLivery.Redraw to:
    //  - Change the general button border, including the "favorite" icon (which is baked
    //    into the outline around the button) to always be simple gray, with white used for
    //    selected/hovered icons. (Base PB has visibility problems showing the current
    //    selection the and favorite icon.)
    [HarmonyPatch(typeof(CIHelperLoadoutLivery), "Redraw")]
    public static class LiverySelectionStylesPatch
    {
        public static void Postfix(CIHelperLoadoutLivery __instance, DataContainerEquipmentLivery liveryData, bool unlocked, bool selected, bool favorite, bool fromContent)
        {
            __instance.spriteFrame.gradientTop = Color.white;
            __instance.spriteFrame.gradientBottom = Color.white;

            CIElement cielement = __instance.button.elements[0];
            cielement.colorMain = selected ? Color.white : Color.gray;
        }
    }
}
