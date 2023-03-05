using System.Collections;
using UnityEngine;
using HInteractions;

namespace HPlayer
{
    [RequireComponent(typeof(PlayerInteractions))]
    public class PlayerCursor : MonoBehaviour
    {
        [SerializeField] private GameObject cursorCanvas;
        [SerializeField, Min(0)] private float minShowDistance;

        private PlayerInteractions playerInteractions;
        private IEnumerator cursorUpdater;

        private void OnEnable()
        {
            playerInteractions = GetComponent<PlayerInteractions>();
            if (playerInteractions == null)
                return;

            playerInteractions.OnSelect += ActiveCursor;
        }
        private void OnDisable()
        {
            if (playerInteractions == null)
                return;

            playerInteractions.OnSelect -= ActiveCursor;

            DesactiveCursor();
        }

        private void ActiveCursor()
        {
            if (playerInteractions == null)
                return;

            if (playerInteractions.SelectedObject is Interactable interactable && interactable.ShowPointerOnInterract)
            {
                cursorUpdater = UpdateCursor();
                StartCoroutine(cursorUpdater);
            }
        }
        private void DesactiveCursor()
        {
            cursorCanvas?.SetActive(false);

            if (cursorUpdater != null)
            {
                StopCoroutine(cursorUpdater);
                cursorUpdater = null;
            }
        }


        private IEnumerator UpdateCursor()
        {
            if (cursorCanvas == null)
                yield break;

            while (playerInteractions.SelectedObject != null)
            {
                float distance = Vector3.Distance(playerInteractions.SelectedObject.transform.position, transform.position);
                cursorCanvas.SetActive(distance >= minShowDistance);

                yield return new WaitForSeconds(0.2f);
            }

            cursorUpdater = null;
            DesactiveCursor();
        }
    }
}