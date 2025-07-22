using System;
using UnityEngine;

public class TradeDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private TradeData[] tradeData;

    public TradeData GetRandomTradeData() => tradeData[UnityEngine.Random.Range(0, tradeData.Length)];

}

[Serializable]
public class TradeData {

    [Header("Data")]
    [SerializeField] private ItemStack[] inputItems;
    [SerializeField] private ItemStack[] outputItems;

    public ItemStack[] GetInputItems() => inputItems;

    public ItemStack[] GetOutputItems() => outputItems;

}
