using UnityEngine;

public enum AlertStage
{
    Normal,
    Middle,
    High,
    Maximum
}

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Header("caution level")]
    [SerializeField] private float alertLevel = 0f;
    [SerializeField] private float maxAlertLevel = 100f;

    public float AlertLevel => alertLevel;
    public float MaxAlertLevel => maxAlertLevel;

    public AlertStage CurrentStage
    {
        get
        {
            if (alertLevel >= 80f)
            {
                return AlertStage.Maximum;
            }

            if (alertLevel >= 50f)
            {
                return AlertStage.High;
            }

            if (alertLevel >= 25f)
            {
                return AlertStage.Middle;
            }

            return AlertStage.Normal;
        }
    }

    public float AlertRatio
    {
        get
        {
            if (maxAlertLevel <= 0f)
            {
                return 0f;
            }

            return alertLevel / maxAlertLevel;
        }
    }

    public float GuardViewDistanceMultiplier
    {
        get
        {
            switch (CurrentStage)
            {
                case AlertStage.Middle:
                    return 1.15f;
                case AlertStage.High:
                    return 1.35f;
                case AlertStage.Maximum:
                    return 1.6f;
                default:
                    return 1.0f;
            }
        }
    }

    public float GuardViewAngleMultiplier
    {
        get
        {
            switch (CurrentStage)
            {
                case AlertStage.Middle:
                    return 1.1f;
                case AlertStage.High:
                    return 1.2f;
                case AlertStage.Maximum:
                    return 1.35f;
                default:
                    return 1.0f;
            }
        }
    }

    public float GuardPatrolSpeedMultiplier
    {
        get
        {
            switch (CurrentStage)
            {
                case AlertStage.Middle:
                    return 1.05f;
                case AlertStage.High:
                    return 1.15f;
                case AlertStage.Maximum:
                    return 1.25f;
                default:
                    return 1.0f;
            }
        }
    }

    public float GuardChaseSpeedMultiplier
    {
        get
        {
            switch (CurrentStage)
            {
                case AlertStage.Middle:
                    return 1.05f;
                case AlertStage.High:
                    return 1.15f;
                case AlertStage.Maximum:
                    return 1.3f;
                default:
                    return 1.0f;
            }
        }
    }

    public float GuardSearchTimeMultiplier
    {
        get
        {
            switch (CurrentStage)
            {
                case AlertStage.Middle:
                    return 1.2f;
                case AlertStage.High:
                    return 1.5f;
                case AlertStage.Maximum:
                    return 2.0f;
                default:
                    return 1.0f;
            }
        }
    }

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
        AlertStage previousStage = CurrentStage;

        alertLevel += amount;
        alertLevel = Mathf.Clamp(alertLevel, 0f, maxAlertLevel);

        AlertStage newStage = CurrentStage;

        Debug.Log($"Alert Level: {alertLevel} / Stage: {newStage}");

        if (newStage != previousStage)
        {
            Debug.Log($"Alert stage changed: {previousStage} -> {newStage}");
        }

        if (alertLevel >= 80f)
        {
            Debug.Log("Maximum alert!! Check body Level");
        }
        else if (alertLevel >= 50f)
        {
            Debug.Log("High alert!! Security adding Level");
        }
        else if (alertLevel >= 25f)
        {
            Debug.Log("Middle alert!! Strengthen patrols Level");
        }
    }

    public void ResetAlert()
    {
        alertLevel = 0f;
    }
}