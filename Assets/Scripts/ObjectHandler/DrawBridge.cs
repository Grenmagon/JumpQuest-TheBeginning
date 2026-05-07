using UnityEngine;

public class Drawbridge: MonoBehaviour
{
    [SerializeField] private Transform platformLeft;
    [SerializeField] private Transform platformRight;

    [SerializeField] private Vector3 openRotation = new Vector3(0, 0, 90); // Raufgeklappt
    [SerializeField] private Vector3 closedRotation = new Vector3(0, 0, 0); // Liegend
    [SerializeField] private float speed = 2f;

    private bool isOpening = false;
    private float progress = 0f;

    void Update()
    {
        // Berechne den Fortschritt (0 = zu, 1 = offen)
        if (isOpening) progress += Time.deltaTime * speed;
        else progress -= Time.deltaTime * speed;

        progress = Mathf.Clamp01(progress);

        // Rotation anwenden (Lerp für weichen Übergang)
        Quaternion targetOpen = Quaternion.Euler(openRotation);
        Quaternion targetClosed = Quaternion.Euler(closedRotation);

        /*
        // Plattformen spiegelverkehrt oder identisch rotieren
        platformLeft.localRotation = Quaternion.Lerp(targetClosed, targetOpen, progress);

        // Falls die rechte Plattform in die andere Richtung klappen soll:
        platformRight.localRotation = Quaternion.Lerp(targetClosed, Quaternion.Euler(-openRotation), progress);
        */
        this.transform.rotation = Quaternion.Lerp(targetClosed, targetOpen, progress);
    }

    // Diese Methode rufst du vom Hebel aus auf
    public void ToggleBridge(bool open)
    {
        isOpening = open;
    }

    private void Start()
    {
        isOpening = false;
    }
}
