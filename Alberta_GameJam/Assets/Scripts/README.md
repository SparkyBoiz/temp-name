# Game Architecture Overview

## Event-Based Visual Effects System

The game uses an event-based architecture to separate game logic from visual/audio presentation. This makes the code more maintainable and allows for easier modifications to visual effects without touching game mechanics.

### Key Components

1. **GameEvents (Static Event Bridge)**
   - Located in `Sound/GameEvents.cs`
   - Provides events for requesting visual effects and sounds
   - Game logic scripts use this to request effects without direct references to prefabs

2. **SoundWordDisplay (Presentation Listener)**
   - Located in `Sound/SoundWordDisplay.cs`
   - Listens for visual effect requests and spawns the actual effects
   - Manages the lifetime of spawned effects

### Setup Instructions

1. **Scene Setup**
   1. Create an empty GameObject named "Presentation" in your scene
   2. Add the `SoundWordDisplay` component to it
   3. Assign a `SoundWord` prefab to the component's "Sound Word Prefab" field
   4. Set the desired duration in "Sound Word Duration"

2. **Using Visual Effects in Game Logic**
   - Instead of referencing `SoundWord` prefabs directly, use:
   ```csharp
   GameEvents.RequestSoundWord(position, direction, size);
   ```
   - The presentation system will handle the actual spawning and cleanup

### Visual Effect Parameters

- **Position**: World position where the effect should appear
- **Direction**: Usually `Vector3.up` for vertical effects or `Vector3.right` for horizontal
- **Size**: 
  - 1.0f for normal effects
  - 0.7f for smaller/subtle effects (e.g., footsteps)
  - 1.5f for larger effects (e.g., death effects)

### Example Usage

```csharp
// In your game logic script:
void OnTrigger()
{
    // Request a visual effect
    GameEvents.RequestSoundWord(transform.position, Vector3.up, 1.0f);
    
    // Continue with game logic...
}
```

### Prefab Requirements

Ensure your SoundWord prefabs have:
1. The `SoundWord` component
2. A properly configured `VisualEffect` component with:
   - "Position" property
   - "Direction" property
   - "Size" property
   - "Speed" property

## Future Improvements

- Adding audio event system
- Game state management
- Input handling system
- More visual effect types