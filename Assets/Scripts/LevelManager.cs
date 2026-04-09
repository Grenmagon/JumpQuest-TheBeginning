using UnityEngine;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiText;
    [SerializeField] private CanvasGroup canvasGroup; // Hier die Canvas Group reinziehen
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float displayDuration = 3.0f;

    void Start()
    {
        ShowMessage("Finde den Schatz!");
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMessageRoutine(message));
    }

    private IEnumerator FadeMessageRoutine(string message)
    {
        uiText.text = message;

        // Einblenden
        float counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / fadeDuration);
            yield return null;
        }

        // Wartezeit
        yield return new WaitForSeconds(displayDuration);

        // Ausblenden
        counter = 0f;
        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / fadeDuration);
            yield return null;
        }
    }
}
