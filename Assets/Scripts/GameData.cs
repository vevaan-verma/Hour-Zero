using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameData {

    [Header("Data")]
    private static Dictionary<SurvivorType, int> rescuedSurvivors;

    static GameData() {

        SurvivorType[] survivorTypes = (SurvivorType[]) Enum.GetValues(typeof(SurvivorType));
        rescuedSurvivors = new Dictionary<SurvivorType, int>();

        foreach (SurvivorType type in survivorTypes)
            rescuedSurvivors[type] = 0; // Initialize each type with a count of 0

    }

    public static void AddSurvivor(SurvivorType type) => rescuedSurvivors[type]++;

    public static void RemoveSurvivor(SurvivorType type) {

        if (rescuedSurvivors[type] > 0)
            rescuedSurvivors[type]--;

    }

    public static int GetSurvivorCount(SurvivorType type) {

        if (rescuedSurvivors.TryGetValue(type, out int count))
            return count;

        return 0; // Return 0 if the type does not exist

    }

    public static bool HasSurvivor(SurvivorType type) => rescuedSurvivors.TryGetValue(type, out int count) && count > 0;

}
