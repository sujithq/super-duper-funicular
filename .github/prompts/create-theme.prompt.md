---
mode: agent
description: Create a new animated theme for demo command
---

# Create Demo Theme (Spectre.Console.Cli)

Create a new animated theme for the SolarScope CLI demo command, following Spectre.Console.Cli command structure and project UI/UX standards for beautiful terminal animations and effects.

## Step 1: Define Theme Concept

If not already provided, ask the user:
- What should the theme be called?
- What visual style or concept should it represent?
- What colors and effects should be used?
- What message or story should the animation tell?

## Step 2: Design Animation Sequence

1. Plan the animation flow and timing
2. Consider terminal compatibility and size constraints
3. Design ASCII art or text-based visuals
4. Plan color transitions and effects
5. Consider the educational/inspirational message

## Step 3: Implement Theme in CLI Command

1. Add or extend a theme method in `DemoCommand.cs` (in `src/Commands/`)
2. Ensure the command inherits from `AsyncCommand<TSettings>` and uses a nested `Settings` class for options (e.g., theme selection, speed)
3. Use async/await and support cancellation with `CancellationToken`
4. Use Spectre.Console widgets:
   - `FigletText` for large text
   - `Panel` for framed content
   - `Rule` for separators
   - Custom animations with `AnsiConsole.Live`
5. Integrate the theme as a selectable option via `[CommandOption]` in the CLI

## Step 4: Animation Techniques & UI/UX

### Text Effects
- Typewriter effects with character-by-character reveal
- Color transitions and gradients
- Pulsing or blinking effects
- Text sliding and positioning

### Visual Elements
- ASCII art representations
- Progress bars and spinners
- Charts and graphs with animated data
- Geometric patterns and shapes

### Timing and Flow
- Respect speed settings (slow: 2000ms, normal: 1000ms, fast: 500ms)
- Include pause points for readability
- Support cancellation with CancellationToken
- Smooth transitions between scenes

### UI/UX Standards
- Use project color schemes (green, blue, yellow, red)
- Include emojis for visual appeal
- Ensure high contrast and accessibility
- Provide joyful, educational, and engaging experiences

## Step 5: Theme Integration & Testing

1. Register the theme option in the CLI command using Spectre.Console.Cli
2. Update help text and documentation with the new theme description
3. Test the theme at different terminal sizes and speeds
4. Validate output formatting, accessibility, and performance

## Example Theme Structure

```csharp
private async Task DisplayCustomThemeDemo(AnimationSpeed speed, CancellationToken cancellationToken)
{
    var delay = GetDelayForSpeed(speed);
    // Scene 1: Introduction
    await ShowIntroAnimation(delay, cancellationToken);
    // Scene 2: Main visual
    await ShowMainAnimation(delay, cancellationToken);
    // Scene 3: Conclusion
    await ShowOutroAnimation(delay, cancellationToken);
}
```

## Theme Ideas
- **Space Theme**: Solar system with planets and energy flows
- **Nature Theme**: Growing plants powered by solar energy
- **City Theme**: Urban landscape transitioning to clean energy
- **Seasonal Theme**: Year cycle showing seasonal production patterns
- **Ocean Theme**: Waves of clean energy flowing across the screen

Consider the educational value and how the theme reinforces the renewable energy message of SolarScope CLI. Ensure all new themes are discoverable via CLI help and follow the project's joyful, accessible design principles.
