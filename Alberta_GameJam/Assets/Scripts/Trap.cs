using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float trapDuration = 2f;
    // Optional SoundWord prefab that represents the "creak" visual when the trap is triggered.
    // Assign a prefab that has the `SoundWord` component (and a VisualEffect) in the Inspector.
    // The prefab will be instantiated, `Spawn` will be called, and the instance will be
    // destroyed after `soundWordDuration` seconds to avoid accumulating objects.
    [SerializeField] private SoundWord soundWordPrefab;
    [SerializeField] private float soundWordDuration = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            // Spawn SoundWord visual (if provided)
            if (soundWordPrefab != null)
            {
                // Instantiate the prefab and call Spawn to trigger the VisualEffect.
                SoundWord sw = Instantiate(soundWordPrefab, transform.position, Quaternion.identity);
                // direction and size are configurable; using upward direction and size 1 by default.
                sw.Spawn(transform.position, Vector3.up, 1f);
                // Destroy the spawned object after the configured duration so it doesn't linger.
                Destroy(sw.gameObject, soundWordDuration);
            }

            StartCoroutine(TrapGhost(ghost));
            // Use the 2D collider type here (this script is handling OnTriggerEnter2D)
            Collider2D trapCollider = GetComponent<Collider2D>();
            if (trapCollider != null)
            {
                trapCollider.enabled = false;
            }
        }
    }

    private System.Collections.IEnumerator TrapGhost(Ghost ghost)
    {
        ghost.EnterTrapped(trapDuration);
        yield return new WaitForSeconds(trapDuration);
        gameObject.SetActive(false);
    }
}