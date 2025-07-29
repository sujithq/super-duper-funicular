---
mode: agent
description: Create a new command for SolarScope CLI
---

# Create New CLI Command

Create a new command for the SolarScope CLI following established patterns and conventions.

## Step 1: Gather Command Requirements

If not already provided, ask the user:
- What should the command be called?
- What functionality should it provide?
- What arguments/options should it accept?
- How should it display results to the user?

## Step 2: Create Command Class

1. Create a new file in the `src/Commands/` directory
2. Follow the naming convention: `[CommandName]Command.cs`
3. Use the Command Pattern with proper attributes:
   - `[Verb]` attribute with command name and description
   - `[Option]` attributes for command-line arguments
   - Proper error handling and validation

## Step 3: Implement Command Logic

1. Add `ExecuteAsync()` method as the entry point
2. Integrate with `SolarDataService` for data operations
3. Use Spectre.Console for beautiful terminal output
4. Include progress indicators for long operations
5. Follow established color schemes and emoji usage

## Step 4: Add Command Registration

1. Update the main program to register the new command
2. Ensure proper command-line parser integration
3. Add the command to help documentation

## Step 5: Testing and Validation

1. Test the command with various input scenarios
2. Verify error handling works correctly
3. Check that output formatting matches project standards
4. Ensure cross-platform compatibility

Reference the existing commands in `src/Commands/` for patterns and examples. Follow the repository's coding standards and UI/UX guidelines.
