# LLM guidance for this project
The following guidelines were authored while working with GitHub Copilot GPT-5 mini. (I've since switched to using gpt-5.5, which has been much better.)

## Code style guidelines
- Keep to an absolute minimum the introduction of exception handling. Nesting try-blocks shall be avoided.
- Keep to an absolute minimum the introduction of events/callbacks/"Action" mechanisms.
- Reflection shall ONLY be used to access private members, e.g., from within PB. Reflection shall NOT be used to "try and see if there is a member of this name or type". Instead of guessing what things might be named, look to find their actual names and types.
- If you are tempted to author code that tries multiple different approaches to accomplish something as a "fallback" or "best effort" scheme, specifically call attention to this, so we can figure out which case actually works and then delete the junk code. But, this pattern may indicate that you are missing a key piece of information that you need to locate.
- This mod project is targeting Phantom Brigade specifically. We have no desire to support for example generic sprite atlas variants.
- When creating Harmony hooks, prefer to use the actual type of the parameters, not simply "var".
