using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class SFXLib : MonoBehaviour {

    [Header("Library")]
    [SerializeField] private List<SFXEntry> soundDict = new List<SFXEntry>();

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool hasErrors = false;

    public enum Sounds {

        #region Item Sounds

        Item_CrowbarHit,
        Item_KeyJangle,

        #endregion

        #region Object Sounds

        Object_WoodenBreak,
        Object_DoorOpen,
        Object_SoftBells,
        Object_MetalDoorOpen,
        Object_MetalDoorClose,
        Object_WoodDoorOpen,

        #endregion

        #region Music

        Music_RadioSong1,
        Music_RadioSong2,
        Music_RadioSong3,
        Music_RadioSong4,

        #endregion

        #region General

        //General
        General_Thump,
        General_Creak,
        General_LongCreak,
        General_Slam,

        #endregion

        #region Misc

        //Misc
        Misc_Test,
        Misc_NoSound,
        Misc_Oof

        #endregion
    }
    private void Start() => ValidateDict(modifyDict: false);

    private void OnValidate() => hasErrors = false;

    // make sure the library looks how it should (?)
    public void ValidateDict(bool modifyDict) {

        if (modifyDict) {

            // array of each enum in Sounds
            Sounds[] allSounds = (Sounds[])System.Enum.GetValues(typeof(Sounds));

            //all entries already listed in existingEntries
            HashSet<Sounds> existingEntries = soundDict.Select(entry => entry.Key).ToHashSet();

            // check for missing and add it

            foreach (Sounds sound in allSounds) {

                if (!existingEntries.Contains(sound)) {

                    soundDict.Add(new SFXEntry(sound));
                    Debug.Log("Automatically added new entry for " + sound + " to SFXLib");

                }

            }

            // automatically alphabetize
            //soundDict = soundDict.OrderBy(entry => entry.Key.ToString()).ToList();

        }

        // check for dupe keys and popups in the library, as well as keys with no popup references

        // check for duped keys 
        HashSet<Sounds> seenKeys = new HashSet<Sounds>();
        Dictionary<Sounds, string> duplicateKeyLog = new Dictionary<Sounds, string>();

        // check for duped sounds
        HashSet<PopupSound> seenSounds = new HashSet<PopupSound>();
        Dictionary<PopupSound, string> duplicateSoundLog = new Dictionary<PopupSound, string>();

        HashSet<string> missingRefLog = new HashSet<string>();

        for (int i = 0; i < soundDict.Count; i++) {
            SFXEntry entry = soundDict[i];

            // if we have seen this key before
            if (!seenKeys.Add(entry.Key)) {

                // only do this for the first dupe
                if (!duplicateKeyLog.ContainsKey(entry.Key)) {

                    // this line is equivalent to a for loop that checks everything in soundDict until it finds the index of a matching object
                    // how to read it: FindIndex of an entry "target" in soundDict such that the target entry key equals the entry key
                    int firstAppearance = soundDict.FindIndex(target => target.Key == entry.Key);

                    duplicateKeyLog.Add(entry.Key, "The SFXLib's soundDict has multiple definitions for the sound key " +
                        entry.Key + ". Please remove duplicates. \n\t\tConflicts found at these indices: " + firstAppearance + ", " + i + ", ");

                }

                else
                    duplicateKeyLog[entry.Key] += i + ", ";
            }

            List<PopupSound> entrySounds = soundDict[i].Sounds;

            // check if missing sound
            if (entrySounds == null || entrySounds.Count == 0)
                missingRefLog.Add("The SFXLib's soundDict entry for " + entry.Key + " is missing a reference to a PopupSound (Index " + i + ")");


            else {

                // check for dupes
                foreach (PopupSound sound in entrySounds)
                    // if we have seen this sound before
                    if (!seenSounds.Add(sound))
                        // if we have not already reported this dupe
                        if (!duplicateSoundLog.ContainsKey(sound)) {

                            // find the index of an entry in soundDict such that the entry's Sounds array contains a given sound
                            int firstAppearanceIdx = soundDict.FindIndex(entry => entry.Sounds.Contains(sound));
                            duplicateSoundLog.Add(sound, "The SFXLib has multiple keys associated with the PopupSound " + sound.name +
                                ". This is unsupported: a PopupSound may only appear once in the library. \n\t\tConflicts found at these indices: " + firstAppearanceIdx + ", " + i + ", ");

                        }

                        else
                            duplicateSoundLog[sound] += i + ", ";

            }

        }

        int totalErrors = duplicateKeyLog.Count + duplicateSoundLog.Count + missingRefLog.Count;
        hasErrors = totalErrors > 0;

        if (!hasErrors && modifyDict)
            Debug.Log("SFXLib has no errors: good to go!");
        else if (hasErrors) {

            Debug.LogWarning("The SFXLib has " + totalErrors + " errors.\n\t\t(Debug tip: Remove duplicate keys first)");

            foreach (Sounds key in duplicateKeyLog.Keys)
                Debug.LogError(duplicateKeyLog[key]);

            foreach (PopupSound key in duplicateSoundLog.Keys)
                Debug.LogError(duplicateSoundLog[key]);

            foreach (string err in missingRefLog)
                Debug.LogError(err);

        }
    }

    // get [a] popup associated with a key (one from the list of popups that key is associated with)
    public PopupSound GetPopup(Sounds key) {

        foreach (SFXEntry entry in soundDict)
            if (entry.Key == key)
                return entry.GetSound();

        return null;

    }

    // find which key a popup corresponds to
    public Sounds? GetKey(PopupSound popup) {

        // this only works because two keys cannot refer to the same PopupSound. that is a big limitation of the system 
        // to work around this limitation, the code for PopupSound would need to be changed. 
        // in AudioPlayer.Stop(), the AudioPlayer uses a key to determine which Popup to stop. So, it has to check the key of the popup. 
        // Therefore, ech PopupSound needs a way to know which key it belongs to. If there are conflicts, AudioPlayer.Stop() can break. 

        foreach (SFXEntry entry in soundDict)
            foreach (PopupSound sound in entry.Sounds)
                if (sound == popup)
                    return entry.Key;

        return null;

    }

}
