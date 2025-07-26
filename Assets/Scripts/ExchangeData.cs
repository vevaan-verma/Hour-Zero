using System;
using UnityEngine;

[Serializable]
public class ExchangeData {

    [Header("Data")]
    [SerializeField] private ItemStack[] inputItemStacks;
    [SerializeField] private ItemStack[] outputItemStacks;

    public ItemStack[] GetInputItemStacks() => inputItemStacks;

    public ItemStack[] GetOutputItemStacks() => outputItemStacks;

}

[Serializable]
public class RepairData : ExchangeData {

    [Header("Data")]
    [SerializeField] private BunkerSystemType systemType;
    [SerializeField] private int repairPercent;

    public BunkerSystemType GetSystemType() => systemType;

    public int GetRepairPercent() => repairPercent;

}

// TODO: test if it works without the Serializable attribute
[Serializable]
public class TradeData : ExchangeData {

}
