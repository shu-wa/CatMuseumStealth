using UnityEngine;

public class PlayerBackpackVisual : MonoBehaviour
{
    [Header("backpack visual")]
    [SerializeField] private GameObject backpackPrefab;
    [SerializeField] private GameObject existingBackpackObject;

    [Header("socket")]
    [SerializeField] private Transform backpackSocket;

    [Header("local adjustment")]
    [SerializeField] private Vector3 localPosition = new Vector3(0f, 0.9f, -0.28f);
    [SerializeField] private Vector3 localRotationEuler = new Vector3(0f, 180f, 0f);
    [SerializeField] private Vector3 localScale = Vector3.one;

    private GameObject spawnedBackpack;
    private bool lastEquippedState;

    private void Start()
    {
        SetupBackpackObject();
        RefreshVisual(true);
    }

    private void Update()
    {
        RefreshVisual(false);
    }

    private void SetupBackpackObject()
    {
        if (backpackSocket == null)
        {
            backpackSocket = transform;
        }

        if (existingBackpackObject != null)
        {
            spawnedBackpack = existingBackpackObject;
        }
        else if (backpackPrefab != null)
        {
            spawnedBackpack = Instantiate(backpackPrefab, backpackSocket);
        }

        if (spawnedBackpack == null)
        {
            return;
        }

        spawnedBackpack.transform.SetParent(backpackSocket, false);
        spawnedBackpack.transform.localPosition = localPosition;
        spawnedBackpack.transform.localRotation = Quaternion.Euler(localRotationEuler);
        spawnedBackpack.transform.localScale = localScale;
    }

    private void RefreshVisual(bool force)
    {
        bool equipped = PlayerProfile.Instance != null && PlayerProfile.Instance.BackpackEquipped;

        if (!force && equipped == lastEquippedState)
        {
            return;
        }

        lastEquippedState = equipped;

        if (spawnedBackpack != null)
        {
            spawnedBackpack.SetActive(equipped);
        }
    }
}