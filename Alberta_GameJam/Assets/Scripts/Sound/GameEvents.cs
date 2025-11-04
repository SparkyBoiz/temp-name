using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<SoundType, Vector3, Vector3, float> OnSoundWordRequested;

    public static void RequestSoundWord(SoundType type, Vector3 position, Vector3 direction, float size)
    {
        OnSoundWordRequested?.Invoke(type, position, direction, size);
    }
}
