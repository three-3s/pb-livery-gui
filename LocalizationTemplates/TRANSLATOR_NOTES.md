# LiveryGUI Translation Notes

Translate the values in `LocalizationTemplates/ui_base.en.yaml`, then copy the result to `LocalizationEdits/<Language>/ui_base.yaml`.

## Target Languages

Use Phantom Brigade's visible player languages:

* ChineseSimplified
* ChineseTraditional
* French
* German
* Italian
* Japanese
* Korean
* PortugueseBrazilian
* Russian
* Spanish

Do not translate `Pseudoloc` unless it is later needed for development testing.

## Preservation Rules

Do not translate YAML keys such as `livery_gui_save_button_text`.

Preserve:

* `LiveryGUI`
* `Player.log`
* Sprite tags such as `[sp=s_icon_l32_cancel]`
* Placeholders such as `{0}`
* Parameter names such as RGB, XYZ, W, X, Y, Z
* Numeric hints such as `(w=0.0)`
* File paths and file extensions

## Terminology

Prefer Phantom Brigade's existing localized terminology for important game terms. Useful reference areas include the localized text for mech/unit labels, pilot labels, base UI, mission briefing UI, and livery or paint-scheme tutorials.

The following glossary is intended to keep translations consistent between the game's own translation and the mod's translation, and across `ui_base.en.yaml`, README files, and FAQ files.

Terms marked as "PB-confirmed" were checked against Phantom Brigade localization files in `pb-spy/Configs/TextLocalizations`. If a shipped PB localization uses a different term in a more specific context, prefer PB's term.

Terms marked as "mod guidance" are recommended translations for LiveryGUI's paint/material controls and may be adjusted by a fluent translator if they sound unnatural in context.

For Chinese and Japanese, prioritize broad player understandability over literal technical precision. Prefer Phantom Brigade's own reading level and terminology, then common game/UI wording, then clear compounds made from familiar characters. Do not rely on ruby/furigana or other pronunciation annotations; assume the game UI only supports plain text. Avoid rare characters, academic compounds, and obscure katakana technical loans unless the tooltip also explains the concept in simpler language.

### PB-Confirmed Terms

| English | ChineseSimplified | ChineseTraditional | French | German | Italian | Japanese | Korean | PortugueseBrazilian | Russian | Spanish |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| mech | 机甲 | 機甲 | Mech | Mech | Mech | メカ | 메카 | Meka | Мех | Meca |
| unit | 单位 | 單位 | Unité | Einheit | Unità | ユニット | 유닛 | Unidade | Юнит | Unidad |
| pilot | 驾驶员 | 駕駛員 | Pilote | Pilot | Pilota | パイロット | 조종사 | Mekanauta | Пилот | Piloto |
| base | 基地 | 基地 | Base | Basis | Base | 基地 | 기지 | Base | База | Base |
| mission briefing | 任务简报 | 任務簡報 | Briefing de mission | Missionsbriefing | Briefing della missione | ミッションブリーフィング | 임무 브리핑 | Briefing da missão | Брифинг задания | Instrucciones de la misión |

### LiveryGUI Paint And Material Terms

| English | ChineseSimplified | ChineseTraditional | French | German | Italian | Japanese | Korean | PortugueseBrazilian | Russian | Spanish |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| livery | 涂装 | 塗裝 | livrée / peinture | Lackierung | livrea / verniciatura | 塗装 | 도색 | pintura | окраска | pintura |
| paint scheme | 涂装方案 | 塗裝方案 | schéma de peinture | Farbschema | schema di verniciatura | 塗装パターン | 도색 구성 | esquema de pintura | схема окраски | esquema de pintura |
| primary | 主色 / 主要 | 主色 / 主要 | principal | Haupt- / primär | primario | メイン / 第1 | 주 / 기본 | principal / primário | основной | principal / primario |
| secondary | 副色 / 次要 | 副色 / 次要 | secondaire | Neben- / sekundär | secondario | サブ / 第2 | 보조 | secundário | дополнительный / второй | secundario |
| tertiary | 第三色 / 第三 | 第三色 / 第三 | troisième / tertiaire | dritte / tertiär | terzo / terziario | 第3 | 제3 | terciário / terceiro | третий | terciario / tercero |
| shininess | 光泽度 | 光澤度 | brillance | Glanz | brillantezza | 光沢 | 광택 | brilho | блеск | brillo |
| metalness | 金属感 | 金屬感 | aspect métallique | Metallanteil / Metallwirkung | aspetto metallico | 金属感 | 금속감 | aspecto metálico | металлический вид | aspecto metálico |
| iridescence | 虹色效果 / 虹彩效果 | 虹色效果 / 虹彩效果 | iridescence | Schillern / Irideszenz | iridescenza | 虹色効果 | 무지갯빛 효과 | iridescência | радужный перелив | iridiscencia / efecto iridiscente |

For compact slider labels, it is OK to shorten these terms if the meaning remains clear. For Japanese in particular, prefer compact understandable labels like `メイン`, `サブ`, and `第3` over direct katakana technical loans like `プライマリ`, `セカンダリ`, and especially `ターシャリ`. Tooltips can spell the concept out more fully.

## Fit Guidance

Slider labels and discrete-selector names are cramped. Keep them short.

Tooltip headers should also be brief.

Tooltip bodies can be longer, but avoid turning one short tooltip into several long paragraphs. The game UI is not built for very large tooltip text.

The translated README and FAQ copies live under `docs/<Language>/`. Those files can be more natural and less compact than in-game UI text.
