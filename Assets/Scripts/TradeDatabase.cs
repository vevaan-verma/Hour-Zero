using System;
using UnityEngine;

public class TradeDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private TradeData[] tradeData;

    private void Start() {

        #region VALIDATION
        // make sure each trade data doesn't have multiple item stacks with the same item in both the input and output item stacks
        foreach (TradeData data in tradeData) {

            ItemStack[] inputItemStacks = data.GetInputItemStacks();
            ItemStack[] outputItemStacks = data.GetOutputItemStacks();

            // check for duplicate items in input item stacks
            for (int i = 0; i < inputItemStacks.Length; i++)
                for (int j = i + 1; j < inputItemStacks.Length; j++)
                    if (inputItemStacks[i].GetItem() == inputItemStacks[j].GetItem())
                        Debug.LogError($"Duplicate item {inputItemStacks[i].GetItem().name} found in input item stacks of trade data.");

            // check for duplicate items in output item stacks
            for (int i = 0; i < outputItemStacks.Length; i++)
                for (int j = i + 1; j < outputItemStacks.Length; j++)
                    if (outputItemStacks[i].GetItem() == outputItemStacks[j].GetItem())
                        Debug.LogError($"Duplicate item {outputItemStacks[i].GetItem().name} found in output item stacks of trade data.");

        }
        #endregion

    }

    public TradeData GetRandomTradeData() => tradeData[UnityEngine.Random.Range(0, tradeData.Length)];

}
