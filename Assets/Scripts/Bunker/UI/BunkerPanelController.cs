using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BunkerPanelController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private BunkerSystem[] bunkerSystems;
    private Coroutine refreshCoroutine;

    [Header("UI References")]
    [SerializeField] private BunkerSystemUI[] bunkerSystemUIs;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private TMP_Text refreshText;

    [Header("Settings")]
    [SerializeField] private int refreshInterval;

    [Header("Status")]
    [SerializeField] private BunkerSystemStatusProperties[] bunkerSystemStatusProperties;

    private void Start() {

        #region VALIDATION
        // make sure each bunker system type has a corresponding UI
        BunkerSystemType[] systemTypes = (BunkerSystemType[]) Enum.GetValues(typeof(BunkerSystemType));

        foreach (BunkerSystemUI systemUI in bunkerSystemUIs) {

            BunkerSystemType type = systemUI.GetSystemType();
            systemUI.Initialize(this, Array.Find(bunkerSystems, s => s.GetSystemType() == type)); // initialize the UI with the corresponding system

        }

        foreach (BunkerSystemType type in systemTypes) {

            bool found = false;

            foreach (BunkerSystemUI systemUI in bunkerSystemUIs) {

                if (systemUI.GetSystemType() == type) {

                    found = true;
                    break;

                }
            }

            if (!found)
                Debug.LogError($"No UI defined for BunkerSystemType: {type}");

        }

        // make sure each enum value for bunker system status has corresponding properties defined
        BunkerSystemStatus[] systemStatuses = (BunkerSystemStatus[]) Enum.GetValues(typeof(BunkerSystemStatus));

        foreach (BunkerSystemStatus status in systemStatuses) {

            bool found = false;

            foreach (BunkerSystemStatusProperties statusProperties in bunkerSystemStatusProperties) {

                if (statusProperties.GetStatus() == status) {

                    found = true;
                    break;

                }
            }

            if (!found)
                Debug.LogError($"No properties defined for BunkerSystemStatus: {status}");

        }

        // make sure that the percent ranges in bunkerSystemStatusProperties cover every integer from 0 to 100 inclusively
        // and that no percent is covered by more than one range
        int[] coveredBy = new int[101];

        foreach (BunkerSystemStatusProperties statusProperties in bunkerSystemStatusProperties) {

            Vector2 range = statusProperties.GetPercentRange();

            int min = Mathf.FloorToInt(range.x);
            int max = Mathf.CeilToInt(range.y);

            for (int i = min; i <= max; i++)
                if (i >= 0 && i <= 100)
                    coveredBy[i]++;

        }

        for (int i = 0; i <= 100; i++) {

            if (coveredBy[i] == 0)
                Debug.LogError($"No BunkerSystemStatusProperties covers durability percent value: {i}");
            else if (coveredBy[i] > 1)
                Debug.LogError($"Multiple BunkerSystemStatusProperties cover durability percent value: {i}");

        }
        #endregion

        refreshCoroutine = StartCoroutine(HandlePanelRefresh());

    }

    private IEnumerator HandlePanelRefresh() {

        while (true) {

            refreshText.text = "Refreshing...";
            RefreshPanel();
            RefreshLayout(panelRectTransform);
            yield return new WaitForSeconds(1f); // simulate time taken to refresh

            int timeUntilNextRefresh = refreshInterval - 1; // -1 because we already waited 1 second above

            refreshText.text = "Refreshes in " + timeUntilNextRefresh + "s...";
            RefreshLayout(panelRectTransform);

            while (timeUntilNextRefresh > 0) {

                yield return new WaitForSeconds(1f);
                timeUntilNextRefresh--;
                refreshText.text = "Refreshes in " + timeUntilNextRefresh + "s...";

            }
        }
    }

    private void RefreshPanel() {

        foreach (BunkerSystemUI slider in bunkerSystemUIs) {

            BunkerSystem system = Array.Find(bunkerSystems, s => s.GetSystemType() == slider.GetSystemType());
            slider.UpdateSystemStatus(system.GetCurrentDurability());

        }
    }

    public BunkerSystemStatusProperties GetSystemStatusFromDurability(int durability) {

        foreach (BunkerSystemStatusProperties statusProperties in bunkerSystemStatusProperties) {

            Vector2 percentRange = statusProperties.GetPercentRange();

            if (durability >= percentRange.x && durability <= percentRange.y)
                return statusProperties;

        }

        return null; // should never happen if validation is correct, but just in case

    }

    private void RefreshLayout(RectTransform root) {

        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

    }
}

[Serializable]
public class BunkerSystemStatusProperties {

    [Header("Settings")]
    [SerializeField] private BunkerSystemStatus systemStatus;
    [SerializeField] private Vector2 percentRange; // min and max percentage for this status
    [SerializeField] private Color color;

    public BunkerSystemStatus GetStatus() => systemStatus;

    public Vector2 GetPercentRange() => percentRange;

    public Color GetColor() => color;

}