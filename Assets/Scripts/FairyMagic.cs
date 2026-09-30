using UnityEngine;
using UnityEngine.UI;

public class FairyMagic : MonoBehaviour
{
    public float maxMagic = 100f;
    public float currentMagic;

    public Slider moonlightBar;
    public GameObject capturedPanel;

    private bool isCaptured = false;
    public bool IsCaptured => isCaptured;
    AssassinSpawner spawner;
    void Awake() { spawner = FindAnyObjectByType<AssassinSpawner>(); }

    void Start()
    {
        currentMagic = maxMagic;

        if (moonlightBar != null)
        {
            moonlightBar.maxValue = maxMagic;
            moonlightBar.value = currentMagic;
        }

        // Hide capture screen when game begins
        if (capturedPanel != null)
        {
            capturedPanel.SetActive(false);
        }
    }

    public void DrainMagic(float amount)
    {
        if (isCaptured || (spawner && spawner.VictoryTriggered))
            return;

        currentMagic -= amount;
        currentMagic = Mathf.Clamp(currentMagic, 0f, maxMagic);

        if (moonlightBar != null)
        {
            moonlightBar.value = currentMagic;
        }

        if (currentMagic <= 0f)
        {
            Captured();
        }
    }

    void Captured()
    {
        isCaptured = true;

        if (capturedPanel != null)
        {
            capturedPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
