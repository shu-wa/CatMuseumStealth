using UnityEngine;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Header("alert level")]
    [SerializeField] private float alertLevel = 0f;
    [SerializeField] private float maxAlertLevel = 100f;

    public float AlertLevel => alertLevel;
    public float MaxAlertLevel => maxAlertLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddAlert(float amount)
    {
        alertLevel += amount;
        alertLevel = Mathf.Clamp(alertLevel, 0f, maxAlertLevel);

        Debug.Log($"Alert Level: {alertLevel}");

        if (alertLevel >= 80)
        {
            Debug.Log("Maximum alert!! Check body Level");
        }
        else if (alertLevel >= 50)
        {
            Debug.Log("High alert!! Security adding Level");
        }
        else if (alertLevel >= 25)
        {
            Debug.Log("Middle alert!! Strengthen partrols Level");
        }
    }
}