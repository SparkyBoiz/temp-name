using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float trapDuration = 2f;
    // The Trap is only responsible for game mechanics (trapping a Ghost).
    // Presentation (creak visuals / sound) is requested via GameEvents so the
    // visual/audio systems can respond independently.

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            // Request a trap creak effect via the event system
            GameEvents.RequestSoundWord(SoundType.TrapCreak, transform.position, Vector3.up, 1f);

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