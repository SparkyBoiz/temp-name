using System;
using UnityEngine;

/// <summary>
/// Lightweight static event bridge used to decouple game logic from presentation.
/// Game code should invoke the Request... methods. Presentation systems should
/// subscribe to the corresponding events and handle visuals/audio.
/// </summary>
public static class GameEvents
{
    // Request a SoundWord visual with type. Parameters: type, position, direction, size
    public static event Action<SoundType, Vector3, Vector3, float> OnSoundWordRequested;

    public static void RequestSoundWord(SoundType type, Vector3 position, Vector3 direction, float size)
    {
        OnSoundWordRequested?.Invoke(type, position, direction, size);
    }

    // Future events can be added here, for example OnPlaySound, OnScoreChanged, etc.
}
