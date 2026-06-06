using PhantomBrigade.Data;
using System;
using System.Collections.Generic;

namespace LiveryGUIMod {
    public static class Localization {
        public const string Sector = "ui_base";
        static bool languageChangeCallbackRegistered = false;
        static readonly Dictionary<string, string> englishFallbacks = new Dictionary<string, string>() {
            { "livery_gui_toggle_header", "Livery GUI" },
            { "livery_gui_toggle_text", "Show/hide advanced livery customization options" },
            { "livery_gui_pilot_mode_toggle_header", "Toggle Pilot Livery-Set Editing Mode" },
            { "livery_gui_pilot_mode_toggle_text", "In this mode, the selected pilot's livery set is edited by assigning liveries to the mech's parts. Mech parts that have no pilot livery are transparent and will show the mech's livery. For example, it is possible to paint a mech gray in mech edit mode, then enable pilot edit mode and assign a red livery to the upper body. Whenever that pilot is piloting that mech, the mech will be gray with a red upper body. Any mech this pilot is assigned to will have a red upper body." },
            { "livery_gui_pilot_mode_base_toggle_header", "Show/Hide Mech Livery" },
            { "livery_gui_pilot_mode_base_toggle_text", "Shows or hides the mech's base livery underneath the pilot's livery. Only the view in this editor is affected. Right click on livery slots to unassign the pilot's livery from that slot. Note: Assigning a livery to a slot with a '+' also affects all parts contained within that slot." },
            { "livery_gui_previous_pilot_header", "Previous Pilot" },
            { "livery_gui_previous_pilot_text", "Select the previous pilot. This controls which pilot's livery set is being edited in pilot livery-set editing mode. This does not assign the pilot to the mech; assigning a pilot to a mech is done in mission briefing." },
            { "livery_gui_next_pilot_header", "Next Pilot" },
            { "livery_gui_next_pilot_text", "Select the next pilot. This controls which pilot's livery set is being edited in pilot livery-set editing mode. This does not assign the pilot to the mech; assigning a pilot to a mech is done in mission briefing." },
            { "livery_gui_reset_button_header", "Revert Changes" },
            { "livery_gui_reset_button_text", "Reset this livery to its last saved values." },
            { "livery_gui_save_button_header", "Save Livery" },
            { "livery_gui_save_button_text", "Save this livery, using the given name. The name used must not match any built-in livery, nor any livery provided by a mod. Using a name that does not exist yet will save to a new livery with that name." },
            { "livery_gui_favorite_button_header", "Toggle as Favorite" },
            { "livery_gui_favorite_button_text", "Marks this livery as a favorite. Favorites are kept at the front of the list of liveries." },
            { "livery_gui_hints_header", "Hints" },
            { "livery_gui_hints_basic_text", "Click on sliders or click-and-drag to set.\n\nRemember to save.\nNormal values for sliders are between 0 and 1. Other values might work, but they might cause problems.\n\nYou can use keyboard \"Move Camera\" (Move Up/Down/Left/Right) keys to adjust view." },
            { "livery_gui_hints_precise_left_text", "For precise adjustments:\nRight-click-and-hold, and move mouse <--->\nChange speed: Hold SHIFT, ALT, or CTRL." },
            { "livery_gui_hints_precise_drag_text", "For precise adjustments:\nRight-click-and-hold, and move mouse <--->\nChange speed: Hold SHIFT, ALT, or CTRL." },
            { "livery_gui_pilot_livery_slot_marker_header", "Pilot livery" },
            { "livery_gui_pilot_livery_slot_marker_text", "The pilot's own livery-set is applied to this slot. This livery will be applied to this slot of any mech that is piloted by this pilot. Note: Any slot marked with a '+' has child slots, which will be affected by the parent slot." },
            { "livery_gui_mech_livery_slot_marker_header", "Mech base livery" },
            { "livery_gui_mech_livery_slot_marker_text", "No pilot livery has been assigned to this slot. The mech's own base livery will control this slot. Assigning a livery to this slot will assign the livery to the pilot's livery-set." },
            { "livery_gui_livery_name_input", "Livery Name" },
            { "livery_gui_pilot_none", "[none]" },
            { "livery_gui_supporter_pattern_none", "none (w=0.0)" },
            { "livery_gui_supporter_pattern_dots", "dots (w=1.0)" },
            { "livery_gui_supporter_pattern_lines", "lines (w=2.0)" },
            { "livery_gui_supporter_pattern_sheen", "sheen (w=3.0)" },
            { "livery_gui_message_load_failed", "LiveryGUI: Failed to load {0} liveries. See Player.log for more info. [sp=s_text_24_exclamation]" },
            { "livery_gui_message_save_blocked_name", "No. Change the livery Name. LiveryGUI does not own the current Name. [sp=s_icon_l32_cancel]" },
            { "livery_gui_message_save_failed", "Failed to save. See Player.log for more info. [sp=s_text_24_exclamation]" },
            { "livery_gui_message_livery_name_exists", "No. A livery with that Name already exists. [sp=s_icon_l32_cancel]" },
            { "livery_gui_message_save_livery_sets_error", "Error saving livery-sets (LiveryGUI). [sp=s_icon_l32_cancel]" },
            { "livery_gui_message_load_livery_sets_error", "Error loading livery-sets (LiveryGUI). [sp=s_icon_l32_cancel]" },
            { "livery_gui_message_init_livery_sets_error", "Error initializing livery-sets (LiveryGUI). [sp=s_icon_l32_cancel]" },
            { "livery_gui_slider_primaryr_label", "R              Primary Color" },
            { "livery_gui_slider_primaryg_label", "G" },
            { "livery_gui_slider_primaryb_label", "B" },
            { "livery_gui_slider_primarya_label", "A" },
            { "livery_gui_slider_secondaryr_label", "R              Secondary Color" },
            { "livery_gui_slider_secondaryg_label", "G" },
            { "livery_gui_slider_secondaryb_label", "B" },
            { "livery_gui_slider_secondarya_label", "A" },
            { "livery_gui_slider_tertiaryr_label", "R              Tertiary Color" },
            { "livery_gui_slider_tertiaryg_label", "G" },
            { "livery_gui_slider_tertiaryb_label", "B" },
            { "livery_gui_slider_tertiarya_label", "A" },
            { "livery_gui_slider_contentx_label", "R         Supporter DLC Color" },
            { "livery_gui_slider_contenty_label", "G" },
            { "livery_gui_slider_contentz_label", "B" },
            { "livery_gui_slider_contentw_label", "Sup. DLC Pattern" },
            { "livery_gui_slider_primaryx_label", "low       Primary Shininess" },
            { "livery_gui_slider_primaryy_label", "mid" },
            { "livery_gui_slider_primaryz_label", "high" },
            { "livery_gui_slider_primaryw_label", "           Metalness" },
            { "livery_gui_slider_secondaryx_label", "low       Secondary Shininess" },
            { "livery_gui_slider_secondaryy_label", "mid" },
            { "livery_gui_slider_secondaryz_label", "high" },
            { "livery_gui_slider_secondaryw_label", "           Metalness" },
            { "livery_gui_slider_tertiaryx_label", "low       Tertiary Shininess" },
            { "livery_gui_slider_tertiaryy_label", "mid" },
            { "livery_gui_slider_tertiaryz_label", "high" },
            { "livery_gui_slider_tertiaryw_label", "           Metalness" },
            { "livery_gui_slider_effectx_label", "Primary            Iridescence" },
            { "livery_gui_slider_effecty_label", "Secondary" },
            { "livery_gui_slider_effectz_label", "Tertiary" },
            { "livery_gui_slider_effectw_label", "Unused" },
            { "livery_gui_slider_primaryr_text", "Sets the red amount for the primary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primaryg_text", "Sets the green amount for the primary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primaryb_text", "Sets the blue amount for the primary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primarya_text", "Adjusts how strongly the primary color is applied. Around 1 is the normal look. Lower or negative values tend to brighten or wash the color toward white; higher values darken it toward black, with a unique interaction with colors brighter than 1.\n\nThis behaves more like a brightness/contrast control than ordinary transparency, and extreme values can produce unusual results.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryr_text", "Sets the red amount for the secondary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryg_text", "Sets the green amount for the secondary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryb_text", "Sets the blue amount for the secondary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondarya_text", "Adjusts how strongly the secondary color is applied. Around 1 is the normal look. Lower or negative values tend to brighten or wash the color toward white; higher values darken it toward black, with a unique interaction with colors brighter than 1.\n\nThis behaves more like a brightness/contrast control than ordinary transparency, and extreme values can produce unusual results.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryr_text", "Sets the red amount for the tertiary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryg_text", "Sets the green amount for the tertiary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryb_text", "Sets the blue amount for the tertiary areas of the livery.\n\nPrimary, secondary, and tertiary are different areas on each mech part. Normal color values are usually 0 to 1. Values above 1 can make colors brighter.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiarya_text", "Adjusts how strongly the tertiary color is applied. Around 1 is the normal look. Lower or negative values tend to brighten or wash the color toward white; higher values darken it toward black, with a unique interaction with colors brighter than 1.\n\nThis behaves more like a brightness/contrast control than ordinary transparency, and extreme values can produce unusual results.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_contentx_text", "Sets the red amount for the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed and a pattern other than \"none\" is selected (the Supporter DLC W setting).\n\nValues are additive and can create bright glow or bloom. Values that are too negative can disable the effect. Mixing positive and negative values can produce unusual glow, such as (R,G,B)=(+5, -5, -1.8)" },
            { "livery_gui_slider_contenty_text", "Sets the green amount for the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed and a pattern other than \"none\" is selected (the Supporter DLC W setting).\n\nValues are additive and can create bright glow or bloom. Values that are too negative can disable the effect. Mixing positive and negative values can produce unusual glow, such as (R,G,B)=(+5, -5, -1.8)" },
            { "livery_gui_slider_contentz_text", "Sets the blue amount for the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed and a pattern other than \"none\" is selected (the Supporter DLC W setting).\n\nValues are additive and can create bright glow or bloom. Values that are too negative can disable the effect. Mixing positive and negative values can produce unusual glow, such as (R,G,B)=(+5, -5, -1.8)" },
            { "livery_gui_slider_contentw_text", "Chooses the Supporter Upgrade DLC overlay pattern. This only has a visible effect if the Supporter Upgrade DLC is installed. The pattern is \"additive\", so the color must be brighter than black to be visible." },
            { "livery_gui_slider_primaryx_text", "Controls the matte-to-shiny response for the duller regions of the primary material. Lower values look flatter and more painted; higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primaryy_text", "Controls the matte-to-shiny response for the middle regions of the primary material (the areas that are not marked as \"dullest\" nor \"smoothest\"). Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primaryz_text", "Controls the matte-to-shiny response for the brightest or most polished regions of the primary material. Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_primaryw_text", "Controls \"metalness\" for the primary material. Around 0 behaves more like paint or plastic: the angle of the material doesn't affect how it looks. Around 1 behaves more like metal: the angle of the material does affect how that part of the material looks. Values outside 0 to 1 are experimental. Negative values seem to \"glow\", and larger positive values affect brightness of smooth-shininess and can cause exotic color effects, especially with mixed RGB values like (R,G,B)=(1.2, 0.5, 0.8) or (1.2, 0.4, 1.2), with a mix of above-1 and below-1 values.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs. When using large 'W' values, turn down X,Y,Z shininess to help avoid this." },
            { "livery_gui_slider_secondaryx_text", "Controls the matte-to-shiny response for the duller regions of the secondary material. Lower values look flatter and more painted; higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryy_text", "Controls the matte-to-shiny response for the middle regions of the secondary material (the areas that are not marked as \"dullest\" nor \"smoothest\"). Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryz_text", "Controls the matte-to-shiny response for the brightest or most polished regions of the secondary material. Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_secondaryw_text", "Controls \"metalness\" for the secondary material. Around 0 behaves more like paint or plastic: the angle of the material doesn't affect how it looks. Around 1 behaves more like metal: the angle of the material does affect how that part of the material looks. Values outside 0 to 1 are experimental. Negative values seem to \"glow\", and larger positive values affect brightness of smooth-shininess and can cause exotic color effects, especially with mixed RGB values like (R,G,B)=(1.2, 0.5, 0.8) or (1.2, 0.4, 1.2), with a mix of above-1 and below-1 values.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs. When using large 'W' values, turn down X,Y,Z shininess to help avoid this." },
            { "livery_gui_slider_tertiaryx_text", "Controls the matte-to-shiny response for the duller regions of the tertiary material. Lower values look flatter and more painted; higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryy_text", "Controls the matte-to-shiny response for the middle regions of the tertiary material (the areas that are not marked as \"dullest\" nor \"smoothest\"). Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryz_text", "Controls the matte-to-shiny response for the brightest or most polished regions of the tertiary material. Higher values produce sharper reflected-light highlights. Values near 1 are almost mirror-like.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_tertiaryw_text", "Controls \"metalness\" for the tertiary material. Around 0 behaves more like paint or plastic: the angle of the material doesn't affect how it looks. Around 1 behaves more like metal: the angle of the material does affect how that part of the material looks. Values outside 0 to 1 are experimental. Negative values seem to \"glow\", and larger positive values affect brightness of smooth-shininess and can cause exotic color effects, especially with mixed RGB values like (R,G,B)=(1.2, 0.5, 0.8) or (1.2, 0.4, 1.2), with a mix of above-1 and below-1 values.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs. When using large 'W' values, turn down X,Y,Z shininess to help avoid this." },
            { "livery_gui_slider_effectx_text", "Controls \"iridescence\" for the primary paint areas. Positive values tend toward stronger rainbow iridescence. Negative values tend toward a pearlescent look. Values near 0 have little or no effect. Large values (positive or negative) will look \"unusual\". \"Metalness\" and the part's R,G,B color affect how this looks.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_effecty_text", "Controls \"iridescence\" for the secondary paint areas. Positive values tend toward stronger rainbow iridescence. Negative values tend toward a pearlescent look. Values near 0 have little or no effect. Large values (positive or negative) will look \"unusual\". \"Metalness\" and the part's R,G,B color affect how this looks.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_effectz_text", "Controls \"iridescence\" for the tertiary paint areas. Positive values tend toward stronger rainbow iridescence. Negative values tend toward a pearlescent look. Values near 0 have little or no effect. Large values (positive or negative) will look \"unusual\". \"Metalness\" and the part's R,G,B color affect how this looks.\n\nExtremely bright parts, especially when shiny, can cause major visual bugs." },
            { "livery_gui_slider_effectw_text", "Reserved effect value. It currently does not appear to have a visible effect, but it is saved for completeness and experimentation." },
        };

        public static void RegisterLanguageChangeCallback() {
            if (languageChangeCallbackRegistered)
                return;

            languageChangeCallbackRegistered = true;
            DataManagerText.RegisterCallback(OnLanguageChanged);
        }

        static void OnLanguageChanged() {
            GUI.RefreshLocalization();
        }

        public static string Get(string key) {
            return Get(key, GetFallback(key));
        }

        public static string Get(string key, string englishFallback) {
            string text = DataManagerText.GetText(Sector, key, true);
            return string.IsNullOrEmpty(text) ? englishFallback : text;
        }

        public static string GetFallback(string key) {
            string fallback;
            return englishFallbacks.TryGetValue(key, out fallback) ? fallback : key;
        }

        public static string Format(string key, params object[] args) {
            return string.Format(Get(key), args);
        }

        public static string Format(string key, string englishFallback, params object[] args) {
            return string.Format(Get(key, englishFallback), args);
        }

        public static void SetTooltip(CIButton button, string headerKey, string textKey) {
            SetTooltip(button, headerKey, GetFallback(headerKey), textKey, GetFallback(textKey));
        }

        public static void SetTooltip(CIButton button, string headerKey, string headerFallback, string textKey, string textFallback) {
            if (button == null)
                return;

            button.tooltipFromLibrary = false;
            button.tooltipKey = null;
            button.AddTooltip(Get(headerKey, headerFallback), Get(textKey, textFallback));
        }

        public static void AddOverworldMessage(string key, params object[] args) {
            CIViewOverworldLog.AddMessage(Format(key, args));
        }
    }
}
