using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float trapDuration = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            GameEvents.RequestSoundWord(SoundType.TrapCreak, transform.position, Vector3.up, 1f);

            StartCoroutine(TrapGhost(ghost));
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