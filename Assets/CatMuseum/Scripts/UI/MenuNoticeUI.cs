using TMPro;
using UnityEngine;

public class MenuNoticeUI : MonoBehaviour
{
    public static MenuNoticeUI Instance { get; private set; }

    [Header("text")]
    [SerializeField] private TextMeshProUGUI noticeText;

    private float timer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (noticeText == null)
        {
            return;
        }

        if (timer <= 0f)
        {
            noticeText.gameObject.SetActive(false);
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            noticeText.gameObject.SetActive(false);
        }
    }

    public void ShowNotice(string message, float duration = 2.0f)
    {
        if (noticeText == null)
        {
            Debug.Log(message);
            return;
        }

        noticeText.gameObject.SetActive(true);
        noticeText.text = message;
        timer = duration;
    }
}