# Enhanced Data-Driven Animation System

This system provides a flexible, user-friendly way to manage character animations in your tactical RPG. It combines the power of data-driven design with the convenience of auto-assignment.

## Quick Start

### 1. For Individual Characters

1. **Select your CharacterDefinition asset** in the Project window
2. **In the Inspector**, scroll down to the "Animation Mappings" section
3. **Click "Auto-Populate Common Actions"** to add standard animation actions
4. **Click "Auto-Assign All Clips"** to automatically find and assign animation clips
5. **Review and adjust** any missing or incorrect assignments

### 2. For Multiple Characters (Batch Processing)

1. **Open the Batch Processor**: `Shogun > Characters > Batch Process Character Definitions`
2. **Select the characters** you want to process
3. **Use the batch operations**:
   - "Auto-Populate Common Actions" - Adds standard actions to all selected characters
   - "Auto-Assign All Clips" - Finds and assigns clips for all selected characters
   - "Assign Animator Controller" - Assigns the same controller to multiple characters
   - "Generate Validation Report" - Creates a detailed report of all character states

## How It Works

### Auto-Assignment Logic

The system automatically searches for animation clips using these patterns (in order):
1. `{CharacterName}_{ActionName}` (e.g., "Ryoma_ATTACK 1")
2. `{CharacterName}{ActionName}` (e.g., "RyomaATTACK 1")
3. `{ActionName}` (e.g., "ATTACK 1")
4. Custom search pattern (if specified)

### Search Locations

The system searches in these folders:
- `Assets/_Project/Features/Characters/Art/Animations`
- `Assets/_Project/Features/Characters/Art/Sprites`

### Common Animation Actions

The system includes these standard actions:
- **Movement**: IDLE, RUN, WALK, DASH
- **Combat**: ATTACK 1, ATTACK 2, ATTACK 3, DEFEND, SPECIAL ATTACK
- **Status**: HURT, HEALING, DEATH
- **Movement**: JUMP, JUMP-START, JUMP-FALL, JUMP-TRANSITION

## Features

### ✅ Validation
- Real-time validation of animation assignments
- Clear error messages for missing clips
- Visual indicators (green/yellow/red) for mapping status

### ✅ Auto-Assignment
- Automatic clip discovery based on naming conventions
- Custom search patterns for special cases
- Batch processing for multiple characters

### ✅ User-Friendly UI
- Clean, organized Inspector interface
- Quick action buttons for common tasks
- Scrollable list for many animations
- Individual "Auto" buttons for each mapping

### ✅ Batch Processing
- Process multiple characters at once
- Generate validation reports
- Bulk assign animator controllers

## Best Practices

### Naming Conventions
- **Recommended**: `{CharacterName}_{ActionName}` (e.g., "Ryoma_ATTACK 1")
- **Alternative**: `{ActionName}` (e.g., "ATTACK 1") for shared animations
- **Custom**: Use custom search patterns for special cases

### Organization
- Keep animation clips with their source art
- Use consistent naming across all characters
- Group related animations in subfolders

### Workflow
1. **Setup**: Auto-populate common actions for new characters
2. **Assign**: Use auto-assignment to find clips
3. **Review**: Check validation section for any issues
4. **Customize**: Manually adjust any special cases
5. **Validate**: Generate reports to ensure everything is working

## Troubleshooting

### Missing Animations
- Check that animation clips exist in the search folders
- Verify naming conventions match your clips
- Use custom search patterns for non-standard names

### Invisible Characters
- Ensure Animator Controller is assigned
- Check that animation clips are properly assigned
- Verify the base Animator Controller has the required states

### Performance Issues
- The system only searches in editor mode
- Runtime performance is unaffected
- Batch operations are optimized for large numbers of characters

## Advanced Usage

### Custom Search Patterns
For special cases, you can specify custom search patterns:
1. Disable "Use Auto-Assignment" for a specific mapping
2. Set a custom search pattern (e.g., "Special_Ryoma_Attack")
3. The system will use this pattern instead of the default logic

### Adding New Actions
To add new animation actions:
1. Add the action name to the `commonAnimationActions` array in the editor scripts
2. Or manually add new mappings in the Inspector
3. Use the same naming conventions for consistency

### Runtime Overrides
The system uses `AnimatorOverrideController` at runtime, so you can:
- Swap animations dynamically
- Support alternate skins/costumes
- Handle special events or conditions

## Why Data-Driven?

### Benefits
- **Flexibility**: Any clip can be assigned to any action
- **Designer-Friendly**: Non-programmers can manage animations
- **Scalable**: Easy to add new characters and actions
- **Maintainable**: Clear separation of data and logic
- **Future-Proof**: Supports complex scenarios like alternate skins

### vs Convention-Based
While convention-based systems are simpler initially, data-driven systems:
- Handle exceptions better (e.g., characters with unique animations)
- Support designer workflows
- Scale better with team size
- Provide better error checking and validation
- Enable runtime flexibility

This enhanced system gives you the best of both worlds: the power of data-driven design with the convenience of auto-assignment! 