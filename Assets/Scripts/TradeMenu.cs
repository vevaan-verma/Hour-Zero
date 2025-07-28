using System.Linq;
using UnityEngine;

public class TradeMenu : ExchangeMenu {

    private new void OnEnable() {

        exchangeInventory = FindFirstObjectByType<TradeInventory>(); // find the trade inventory (must be done before base.Initialize() to ensure exchange inventory is set)
        base.OnEnable();

    }

    private new void Start() {

        exchangeBackpackUI = FindObjectsByType<BackpackUI>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(ui => ui.GetBackpackType() == BackpackType.Trade); // find the trade backpack UI (must be done before base.Start() to ensure the repair backpack UI is set)
        closeMenuButton.onClick.AddListener(() => uiManager.CloseTradeMenu()); // add listener to close menu button; call the UIManager method to close the trade menu rather than this class directly to ensure the extra logic is executed (e.g., re-opening the hotbar UI)
        base.Start();

    }

    protected override void ProcessExchange() {

        uiManager.CloseTradeMenu(true); // close the menu when the trade inventory is full (with a flag that the trade requirements were met); don't call CloseMenu() directly to ensure the UIManager logic is executed (e.g., re-opening the hotbar UI)

        ItemStack[] outputStacks = currExchangeData.GetOutputItemStacks(); // get the output stacks for the trade

        foreach (ItemStack outputStack in outputStacks)
            backpack.AddItemStack(outputStack, true); // add the output stacks to the backpack and drop the remainder if the backpack is full

        exchangeInventory.Clear();

        phoneManager.SendNotification(new NotificationData(null, "Tradr", "Trade successful!")); // send a notification to the player that the trade was successful

    }
}
