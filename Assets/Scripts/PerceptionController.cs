using UnityEngine;
using UnityEngine.Rendering;

public class PerceptionController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Volume airQualityVolume;
    [SerializeField] private Volume waterQualityVolume;
    [SerializeField] private Volume powerAbudanceVolume;
    private BunkerPanelManager bunkerPanelManager;

    private void Start() {

        bunkerPanelManager = FindFirstObjectByType<BunkerPanelManager>();

        airQualityVolume.weight = 0f; // start with no air quality effect
        waterQualityVolume.weight = 0f; // start with no water quality effect
        powerAbudanceVolume.weight = 0f; // start with no power abundance effect

    }

    private void Update() {

        airQualityVolume.weight = 1f - (bunkerPanelManager.GetBunkerSystemByType(BunkerSystemType.AirFiltration).GetCurrentDurability() / 100f); // adjust the air quality effect based on the air filtration system's durability
        waterQualityVolume.weight = 1f - (bunkerPanelManager.GetBunkerSystemByType(BunkerSystemType.WaterPurification).GetCurrentDurability() / 100f); // adjust the water quality effect based on the water purification system's durability
        powerAbudanceVolume.weight = 1f - (bunkerPanelManager.GetBunkerSystemByType(BunkerSystemType.PowerSupply).GetCurrentDurability() / 100f); // adjust the power abundance effect based on the power supply system's durability

    }
}
