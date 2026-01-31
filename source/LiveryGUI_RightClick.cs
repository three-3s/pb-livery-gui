using PhantomBrigade;
using PhantomBrigade.Game;
using System;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;

// While right-click-held is active,
// captures mouse movement, converting it to high-precision adjustment of current slider-bar.
// Detects right-click-not-held condition.
//
// (Probably only ever need one of these, attached as a component to one obj on the livery page
// (so it doesn't get invoked while not on the livery page).)

namespace LiveryGUIMod {

    public class SliderRightClickHandler :
        MonoBehaviour
    {
        public float mouseMovementPerFullSlider = 2000f; // (base sensitivity. move mouse this far to move slider fully from one side to the other)
        private CIBar sliderBar;
        private Color origBarColor;
        private Color highlightColor = new Color(1f, 0.5f, 0f);
        private CursorLockMode origLockMode = CursorLockMode.None; // (user may have confined-to-window enabled via Options)

        //==============================================================================
        public void Awake()
        {
            // called once (per instance), when navigating to livery tab
        }

        //==============================================================================
        public CIBar GetSliderBar() { return sliderBar; }
        public void CaptureRightClickFor(CIBar thisSlider)
        {
            ReleaseMouseCapture();
            sliderBar = thisSlider;
            if (sliderBar != null)
            {
                origBarColor = sliderBar.spriteFill.color;
                sliderBar.spriteFill.color = highlightColor;
            }

            ////// Locked-to-None restores the mouse to center of window? Hrm. Can capture mouse position,
            ////// but I'm not sure how to restore (other than a win32-specific-dll hacky thing).
            ////Cursor.lockState = CursorLockMode.Locked;
            ////savedMouseX = Input.mousePosition.x;
            ////savedMouseY = Input.mousePosition.y;
            origLockMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.Confined;
        }

        //==============================================================================
        public void ReleaseMouseCapture()
        {
            Cursor.lockState = origLockMode;
            if (sliderBar != null)
                sliderBar.spriteFill.color = origBarColor;
            sliderBar = null;
        }

        //==============================================================================
        public void OnDisable()
        {
            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.OnDisable (sliderBar={sliderBar?.name ?? "{none}"})");
            ReleaseMouseCapture();
            sliderBar = null;
        }
        public void OnDestroy()
        {
            ReleaseMouseCapture();
        }
        public void OnApplicationFocus(bool hasFocus)
        {
            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.OnApplicationFocus (hasFocus={hasFocus})");
            if (!hasFocus) ReleaseMouseCapture();
        }
        public void OnApplicationPause(bool paused)
        {
            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.OnApplicationPause (paused={paused})");
            if (paused) ReleaseMouseCapture();
        }

        //==============================================================================
        public void Update()
        {
            // called frequently, likely once per frame (per instance)
            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.Update (sliderBar={sliderBar?.name ?? "{none}"})");
            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.Update: GetMouseButton(1)={Input.GetMouseButton(1)} GetMouseButtonDown(1)={Input.GetMouseButtonDown(1)}, GetMouseButtonUp(1)={Input.GetMouseButtonUp(1)}");
            
            if (sliderBar == null)
                return;

            if (!Input.GetMouseButton(1))
            {
                //Debug.Log($"[LiveryGUI] SliderRightClickHandler.Update: Secondary click released (sliderBar={sliderBar?.name ?? "{none}"})");
                ReleaseMouseCapture();
                return;
            }

            if (!sliderBar.isActiveAndEnabled)
            {
                //Debug.Log($"[LiveryGUI] SliderRightClickHandler.Update: sliderBar became inactive (sliderBar={sliderBar?.name ?? "{none}"})");
                ReleaseMouseCapture();
                return;
            }

            float mouseDeltaX = Input.GetAxis("Mouse X");

            float range = sliderBar.valueLimit - sliderBar.valueMin;
            float delta = (mouseDeltaX / mouseMovementPerFullSlider) * range;

            if (Input.GetKey(KeyCode.LeftControl))
                delta *= 4f;    // fast
            if (Input.GetKey(KeyCode.LeftShift))
                delta *= 0.25f; // slower
            if (Input.GetKey(KeyCode.LeftAlt))
                delta *= 0.1f;  // slowest (and combinable)

            float newValue = Mathf.Clamp(
                sliderBar.valueRaw + delta,
                sliderBar.valueMin,
                sliderBar.valueLimit
            );

            //Debug.Log($"[LiveryGUI] SliderRightClickHandler.Update (sliderBar={sliderBar?.name ?? "{none}"}, mouseDeltaX={mouseDeltaX}, newValue={newValue})");

            sliderBar.valueRaw = newValue;
            sliderBar.callbackOnAdjustment.Invoke();
        }
    } // class CIBarExtra
} // namespace