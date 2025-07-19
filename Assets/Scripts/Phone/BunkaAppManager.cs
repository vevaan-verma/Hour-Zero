using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BunkaAppManager : MonoBehaviour {

    [Header("References")]
    private BunkerPanelManager bunkerPanelManager;
    private BunkerSystem[] bunkerSystems; // array of bunker systems managed by the panel

    [Header("UI References")]
    [SerializeField] private BunkerSystemUI[] bunkerSystemUIs;
    [SerializeField] private TMP_Text refreshText;

    private void OnEnable() {

        bunkerPanelManager = FindFirstObjectByType<BunkerPanelManager>();
        bunkerPanelManager.onPanelRefresh += RefreshApp;
        bunkerPanelManager.onRefreshTextUpdate += UpdateRefreshText;

    }

    private void Start() {

        #region VALIDATION
        // make sure each bunker system type has a corresponding UI
        BunkerSystemType[] systemTypes = (BunkerSystemType[]) Enum.GetValues(typeof(BunkerSystemType));
        bunkerSystems = bunkerPanelManager.GetBunkerSystems();

        foreach (BunkerSystemUI systemUI in bunkerSystemUIs) {

            BunkerSystemType type = systemUI.GetSystemType();
            systemUI.Initialize(bunkerPanelManager, Array.Find(bunkerSystems, s => s.GetSystemType() == type)); // initialize the UI with the corresponding system

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
        #endregion

    }

    public void RefreshApp() {

        foreach (BunkerSystemUI slider in bunkerSystemUIs) {

            BunkerSystem system = Array.Find(bunkerSystems, s => s.GetSystemType() == slider.GetSystemType());
            slider.UpdateSystemStatus(system.GetCurrentDurability());

        }

        RefreshLayout(transform.parent.GetComponent<RectTransform>()); // refresh the layout of the app UI

    }

    public void UpdateRefreshText() => refreshText.text = bunkerPanelManager.GetRefreshText(); // update the refresh text from the bunker panel manager

    private void RefreshLayout(RectTransform root) {

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

    }
}
