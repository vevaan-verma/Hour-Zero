using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NameDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private NameData[] nameData;

    public string GetRandomName(Sex sex) {

        NameData[] filteredNames = nameData.Where(n => n.GetSex() == sex).ToArray();
        return filteredNames[UnityEngine.Random.Range(0, filteredNames.Length)].GetName();

    }
}

[Serializable]
public class NameData {

    [Header("Data")]
    [SerializeField] private string name;
    [SerializeField] private Sex sex;

    public string GetName() => name;

    public Sex GetSex() => sex;

}

public enum Sex {

    Male, Female

}
