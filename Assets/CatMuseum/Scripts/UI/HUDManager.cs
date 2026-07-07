using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PlayerRoomTracker roomTracker;

    [Header("texts")]
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Update()
    {
        UpdateAlertText();
        UpdateCapacityText();
        UpdateScoreText();
        UpdateRoomText();
        UpdatePromptText();
    }

    private void UpdateRoomText()
    {
        if (roomText == null || roomTracker == null)
        {
            return;
        }

        roomText.text = roomTracker.CurrentRoomText;
    }

    private void UpdateAlertText()
    {
        if (alertText == null)
        {
            return;
        }

        float alertLevel = 0f;
        float maxAlertLevel = 100f;
        AlertStage stage = AlertStage.Normal;

        if (AlertManager.Instance != null)
        {
            alertLevel = AlertManager.Instance.AlertLevel;
            maxAlertLevel = AlertManager.Instance.MaxAlertLevel;
            stage = AlertManager.Instance.CurrentStage;
        }

        alertText.text = $"Alert Level: {alertLevel:0} / {maxAlertLevel:0} [{stage}]";
    }

    private void UpdateCapacityText()
    {
        if (capacityText == null || inventory == null)
        {
            return;
        }

        capacityText.text = $"Capacity: {inventory.UsedCapacity} / {inventory.MaxCapacity}";
    }

    private void UpdateScoreText()
    {
        if (scoreText == null || inventory == null)
        {
            return;
        }

        scoreText.text = $"Score: {inventory.TotalStolenValue}";
    }

    private void UpdatePromptText()
    {
        if (promptText == null || interactor == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = $"GAME OVER\n{GameManager.Instance.GameOverMessage}\nPress R to Return to Main Menu";
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsClear)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = $"CLEAR!\n{GameManager.Instance.ClearMessage}\nPress R to Return to Main Menu";
            return;
        }

        if (interactor.IsInteracting)
        {
            promptText.gameObject.SetActive(true);

            float progress = Mathf.Clamp01(interactor.InteractionProgress);
            int percent = Mathf.RoundToInt(progress * 100f);

            promptText.text = $"{interactor.InteractionText}... {percent}%";
            return;
        }

        if (!string.IsNullOrEmpty(interactor.NoticeMessage))
        {
            promptText.gameObject.SetActive(true);
            promptText.text = interactor.NoticeMessage;
            return;
        }

        string prompt = interactor.CurrentPrompt;

        if (string.IsNullOrEmpty(prompt))
        {
            promptText.gameObject.SetActive(false);
            return;
        }

        promptText.gameObject.SetActive(true);
        promptText.text = prompt;
    }
}