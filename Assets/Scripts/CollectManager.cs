using UnityEngine;
using TMPro;

public class CollectManager : MonoBehaviour
{
    public static CollectManager instance;

    public int count = 0;
    public TextMeshProUGUI collectibleText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCollectible()
    {
        count++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        collectibleText.text = "Собрано доз: " + count;
    }
}
