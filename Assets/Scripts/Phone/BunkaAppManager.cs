using System;
using TMPro;
using UnityEngine;

public class BunkaAppManager : ViewManager {

    [Header("References")]
    private BunkerPanelManager bunkerPanelManager;
    private RectTransform rectTransform;

    [Header("UI References")]
    [SerializeField] private BunkerSystemUI[] bunkerSystemUIs;
    [SerializeField] private TMP_Text refreshText;

    // use Awake for validation to ensure all systems are initialized before any UI updates/app refreshes, which happen in Start
    private void Awake() {

        bunkerPanelManager = FindFirstObjectByType<BunkerPanelManager>();
        rectTransform = GetComponent<RectTransform>();

        #region VALIDATION
        // make sure each bunker system type has a corresponding UI
        BunkerSystemType[] systemTypes = (BunkerSystemType[]) Enum.GetValues(typeof(BunkerSystemType));

        foreach (BunkerSystemUI systemUI in bunkerSystemUIs) {

            BunkerSystemType type = systemUI.GetSystemType();
            systemUI.Initialize(bunkerPanelManager, bunkerPanelManager.GetBunkerSystemByType(type)); // initialize the UI with the corresponding system

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
                Debug.LogError($"No UI defined for BunkerSystemType: {type}.");

        }
        #endregion

    }

    private new void OnEnable() {

        base.OnEnable();

        // subscribe to events from the bunker panel manager to update the app UI
        bunkerPanelManager.onPanelRefresh += RefreshApp;
        bunkerPanelManager.onRefreshTextUpdate += UpdateRefreshText;

        UpdateRefreshText(); // initial update of the refresh text

    }

    private void OnDisable() {

        // unsubscribe from events to avoid memory leaks
        bunkerPanelManager.onPanelRefresh -= RefreshApp;
        bunkerPanelManager.onRefreshTextUpdate -= UpdateRefreshText;

    }

    public override void RefreshApp() {

        foreach (BunkerSystemUI slider in bunkerSystemUIs) {

            BunkerSystem system = bunkerPanelManager.GetBunkerSystemByType(slider.GetSystemType());
            slider.UpdateSystemStatus(system.GetCurrentDurability());

        }

        RefreshLayout(rectTransform); // refresh the layout of the app UI

    }

    public void UpdateRefreshText() => refreshText.text = bunkerPanelManager.GetRefreshText(); // update the refresh text from the bunker panel manager

}
