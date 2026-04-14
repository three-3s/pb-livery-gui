using HarmonyLib;
using PhantomBrigade;
using PhantomBrigade.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LiveryGUIMod
{
    //=========================================================================================
    [HarmonyPatch(typeof(CIViewInternalUITools), MethodType.Normal), HarmonyPatch("RedrawSprite")]
    public class UIToolsSpriteLookupPatch
    {
        public static CIViewInternalUITools uitoolsView = null;

        //-------------------------------------------------------------------------------
        public static void Postfix(CIViewInternalUITools __instance)
        {
            if (uitoolsView) return;
            uitoolsView = __instance;

            bool success = false;
            try
            {
                foreach (var sprite in uitoolsView.gameObject.GetComponentsInChildren<UISprite>(true))
                {
                    if (sprite.parent.name != "Container_Atlas")
                        continue;

                    sprite.gameObject.AddComponent<AtlasSpriteClickHandler>();
                    Debug.Log($"[LiveryGUI.uitools-Patch] UIToolsSpriteLookup Patch: attached AtlasSpriteClickHandler to {sprite.name}, child of {sprite.parent?.name}");

                    // Add collider (else the OnClick behavior won't ever fire)
                    if (sprite.gameObject.GetComponent<Collider>() == null)
                    {
                        var collider = sprite.gameObject.AddComponent<BoxCollider>();
                        var bounds = sprite.CalculateBounds();
                        collider.size = bounds.size;
                        collider.center = bounds.center;
                        Debug.Log($"[LiveryGUI.uitools-Patch] UIToolsSpriteLookup Patch: adding a BoxCollider to that sprite: center={collider.center}, size={collider.size}, ({bounds})");
                    }

                    success = true;
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LiveryGUI.uitools-Patch] UIToolsSpriteLookup Patch: exception in Postfix: {ex}");
            }
            if (!success) Debug.LogWarning($"[LiveryGUI.uitools-Patch] UIToolsSpriteLookup Patch: failed to hook in");
        }//Postfix()
    }//class UIToolsSpriteLookupPatch

    //=========================================================================================
    public class AtlasSpriteClickHandler : MonoBehaviour
    {
        //-------------------------------------------------------------------------------
        public void OnClick()
        {
            //Debug.Log($"[LiveryGUI.uitools-Patch] in AtlasSpriteClickHandler::OnClick(), for {gameObject.name}, bounds={gameObject.GetComponent<BoxCollider>()?.bounds} (ctr={gameObject.GetComponent<BoxCollider>()?.center}, sz={gameObject.GetComponent<BoxCollider>()?.size}), child of {transform.parent?.name}");
            try
            {
                var sprite = GetComponent<UISprite>();
                var atlas = sprite.atlas;
                UIRoot root = UIToolsSpriteLookupPatch.uitoolsView.gameObject.GetComponentInParent<UIRoot>();
                float pixelSizeAdj = root.pixelSizeAdjustment;

                // get atlas's texture
                Texture2D atlasTex = null;
                var atype = atlas.GetType();
                var texProp = atype.GetProperty("texture", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                atlasTex = texProp.GetValue(atlas) as Texture2D;

                // get camera
                Vector3 clickPos = Input.mousePosition;
                Camera cam = UICamera.currentCamera ?? Camera.main;
                //Debug.Log($"[LiveryGUI.uitools-Patch]: preferred cam={UICamera.currentCamera}, fallback cam={Camera.main}");

                // get screen-coordinates of sprite
                Vector3 containerBottomLeftScreenCoord = cam.WorldToScreenPoint(sprite.worldCorners[0]);
                Vector3 containerTopRightScreenCoord = cam.WorldToScreenPoint(sprite.worldCorners[2]);
                Vector3 coordWithinContainer = clickPos - containerBottomLeftScreenCoord;
                Vector3 containerPad = new Vector3(14, 17) / pixelSizeAdj;
                //Debug.Log($"[LiveryGUI.uitools-Patch] pixelSizeAdj={pixelSizeAdj}, resulting pad={containerPad}");

                Vector3 atlasBottomLeftScreenCoord = containerBottomLeftScreenCoord + containerPad;
                Vector3 atlasBottomRightScreenCoord = containerTopRightScreenCoord - containerPad;
                Vector3 atlasDiagonal = atlasBottomRightScreenCoord - atlasBottomLeftScreenCoord;

                Vector3 screenCoordWithinAtlas = coordWithinContainer - containerPad;
                Vector3 clickPosNormalizedCoordWithinAtlas = new Vector3(
                    screenCoordWithinAtlas.x / atlasDiagonal.x,
                    screenCoordWithinAtlas.y / atlasDiagonal.y);
                Vector3 clickPosNormalizedTexCoord = new Vector3(
                    clickPosNormalizedCoordWithinAtlas.x,
                    1f - clickPosNormalizedCoordWithinAtlas.y);
                Vector3 clickPosTexelCoord = new Vector3(
                    clickPosNormalizedTexCoord.x * atlasTex.width,
                    clickPosNormalizedTexCoord.y * atlasTex.height);
                //Debug.Log($"[LiveryGUI.uitools-Patch] Sprite screen coords: ({containerBottomLeftScreenCoord:F1})..({containerTopRightScreenCoord:F1}), click at ({clickPos:F1}), click coordWithinContainer={coordWithinContainer:F1}, screenCoordWithinAtlas={screenCoordWithinAtlas:F1}, clickPosNormalizedTexCoord={clickPosNormalizedTexCoord:F1}, clickPosTexelCoord=clickPosTexelCoord{clickPosTexelCoord:F1}");

                // get sprite list (IList of UISpriteData-like objects)
                var listProp = atype.GetProperty("spriteList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var spriteList = listProp != null ? listProp.GetValue(atlas) as List<UISpriteData> : null;

                // lookup the clicked-on sprite-in-atlas
                int px = Mathf.RoundToInt(clickPosTexelCoord.x);
                int py = Mathf.RoundToInt(clickPosTexelCoord.y);
                UISpriteData found = null;
                int foundIndex = -1;

                // Track closest sprite when the click isn't directly inside any sprite rect
                UISpriteData closestSprite = null;
                int closestIndex = -1;
                float closestDistSq = float.MaxValue;

                for (int i = 0; i < spriteList.Count; i++)
                {
                    UISpriteData spr = spriteList[i];
                    int left = spr.x;
                    int right = spr.x + spr.width;
                    int bottom = spr.y;
                    int top = spr.y + spr.height;

                    if (px >= left && px <= right && py >= bottom && py <= top)
                    {
                        // Direct hit
                        found = spr;
                        foundIndex = i;
                        break;
                    }

                    // Compute squared distance from click point to rectangle
                    int nearestX = Mathf.Clamp(px, left, right);
                    int nearestY = Mathf.Clamp(py, bottom, top);
                    float dx = px - nearestX;
                    float dy = py - nearestY;
                    float distSq = dx * dx + dy * dy;
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closestSprite = spr;
                        closestIndex = i;
                    }
                }

                // If no direct hit, fall back to the closest sprite
                if (found == null && closestSprite != null)
                {
                    found = closestSprite;
                    foundIndex = closestIndex;
                    //Debug.Log($"[LiveryGUI.uitools-Patch] No direct sprite at px,py={px},{py}; using closest sprite: {found.name} (index={foundIndex}), distSq={closestDistSq}");
                }

                // select that sprite in the list
                if (found == null)
                {
                    Debug.LogWarning($"[LiveryGUI.uitools-Patch] No sprite found at click location, px,py={px},{py}");
                }
                else
                {
                    Debug.Log($"[LiveryGUI.uitools-Patch] Found sprite @px,py={px},{py}: {found.name} (index={foundIndex})");

                    CIViewInternalUITools uitoolsView = UIToolsSpriteLookupPatch.uitoolsView;
                    if (uitoolsView != null && foundIndex >= 0)
                    {
                        var vtype = UIToolsSpriteLookupPatch.uitoolsView.GetType();
                        FieldInfo f = vtype.GetField("spriteIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                        f.SetValue(uitoolsView, foundIndex);
                        //Debug.Log($"[LiveryGUI.uitools-Patch] Set parent spriteIndex to {foundIndex} via field {f.Name}");

                        var m = vtype.GetMethod("RedrawSprite", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        m.Invoke(uitoolsView, null);
                    }
                    else
                    {
                        Debug.LogWarning("[LiveryGUI.uitools-Patch] parent CIViewInternalUITools not found or invalid foundIndex");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LiveryGUI.uitools-Patch] AtlasSpriteClickHandler: exception in OnClick: {ex}");
            }
        }//OnClick()
    }//class AtlasSpriteClickHandler
}//namespace
