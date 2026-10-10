# GitHub Copilot Instructions for 4M Mehni's Misc Modifications (Continued)

## Mod Overview and Purpose
"4M Mehni's Misc Modifications (Continued)" is an updated version of Mehni's original mod designed to enhance and customize your RimWorld gaming experience. The mod introduces various tweaks, settings, and quality-of-life improvements that address vanilla game limitations and offer players more control over in-game mechanics.

## Key Features and Systems
- **Bugfixes for Vanilla Issues**: Resolves specific game mechanics like the cessation of manhunter packs upon reaching certain colony sizes.
- **Extended Pawn Undrafting**: Adjusts the automatic undrafting behavior of colonists at strategic placements.
- **Customizable Flee Threshold**: Allows players to randomize raider flee behavior or turn off fleeing entirely for added challenge.
- **Nutrient Paste Dispenser Alert**: Alerts players when access to the dispenser is blocked, preventing unexpected food shortages.
- **Informational Enhancements**: Displays detailed relationship statuses, plant yield info, DPS of weapons, and suppresses unnecessary alerts.
- **Various AI Tweaks**: Enables players to modify pawn tendencies and interactions to better suit player strategies.
- **Mods for Modders**: Removes workshop cooldowns, expands developer mode capabilities, and improves message management.

## Coding Patterns and Conventions
- **C# File Structure and Naming**: The code base is organized with meaningful class names like `Alert_NPDBlocked` and `HarmonyPatches` for clear identification of function and purpose.
- **Design Patterns**: Utilizes singleton-like static methods for settings management and extensions to existing game classes.
- **Descriptive Method Names**: Methods such as `GetReport` and `DoThingDef_Transpiler` are intuitively named to reflect their functionality in the code.

## XML Integration
- XML files are an essential component for defining in-game configurations and translations.
- The `Patches\XML_is_icky.xml` likely includes framework for XML-based configurations, supplemented by translatable data in `TranslationTemplate\StatDef\xml_is_gross.xml`.
- Ensure XML definitions are kept separate and clear, preventing overlap with C# logic.

## Harmony Patching
- Essential for modifying existing game logic without altering core game files.
- Key patch implementations include:
  - `BigManhunterPackFix`: Corrects vanilla logic limiting manhunter events.
  - `FleeTrigger_PostFix`: Adjusts fleeing mechanics for raiders.
  - `StayWhereIPutYou_Postfix`: Ensures pawns remain stationed as intended.
  
Utilize the Harmony library (`brrainz.harmony`) for dynamic patching, ensuring compatibility and minimizing conflicts with other mods.

## Suggestions for Copilot
1. **Comment Extraction**: Emphasize including comments above key class methods to provide context and guide Copilot suggestions accurately.
2. **Pattern Recognition**: Copilot could assist by suggesting extensions for XML definitions and adjustments based on similar patterns in existing code.
3. **Automated XML Editing**: Propose code completions that correctly interpret the integration between XML data and C# logic.
4. **Visual Studio Integration**: Leverage IntelliSense style suggestions to further benefit from Copilot's completion capabilities when developing harmony patches and XML updates.

By adhering to these guidelines and leveraging automated tools like Copilot, the development process for RimWorld mods can be significantly streamlined, allowing for more efficient feature and fix implementations.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

