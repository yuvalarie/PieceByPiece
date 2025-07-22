using System.Collections;
using Core.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class StartImageFade : MonoBehaviour
    {
        [SerializeField] private Image image; // Assign this in the Inspector
        [SerializeField] private float duration = 2f; // Fade duration in seconds

        private void Start()
        {
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(2f);

            Color color = image.color;
            float elapsed = 0f;
            float startAlpha = 1f;
            float endAlpha = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                color.a = alpha;
                image.color = color;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;

            EventManager.Instance.OnStartFadeEnded();
        }
    }
}
