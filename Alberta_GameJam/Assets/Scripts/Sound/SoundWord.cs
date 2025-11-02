using UnityEngine;
using UnityEngine.VFX;

public class SoundWord : MonoBehaviour
{
    VisualEffect visualEffect;
    [SerializeField] Texture2D texture;
    [SerializeField] float speed;

    void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();
        visualEffect.SetTexture("Texture", texture);
    }

    public void Spawn(Vector3 position, Vector3 direction, float size)
    {
        visualEffect.SetVector3("Position", position);
        visualEffect.SetVector3("Direction", direction);
        visualEffect.SetFloat("Size", size);
        visualEffect.SetFloat("Speed", speed);
        visualEffect.SendEvent("OnSpawn");
    }
}