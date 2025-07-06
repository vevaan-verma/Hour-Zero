using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class SFXLib : MonoBehaviour {

    [Header("Library")]
    [SerializeField] private List<SFXEntry> soundDict = new List<SFXEntry>();

    [Header("Debug")]
    [SerializeField] private bool logWarningsOnValidate;

    public enum Sounds {

        #region Item Sounds

        Item_CrowbarHit,
        Item_KeyJangle,

        #endregion

        #region Object Sounds

        Object_WoodenBreak,

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

        #endregion

        #region Misc

        //Misc
        Misc_Test

        #endregion
    }
    private void Start() {

        ValidateDict(true);

    }

    // automatically give each enum its own entry in the serialized list, and sort the list
    private void OnValidate() {

        ValidateDict(logWarningsOnValidate);

    }

    // make sure the library looks how it should (?)
    private void ValidateDict(bool logWarnings) {

        // array of each enum in Sounds
        Sounds[] allSounds = (Sounds[])System.Enum.GetValues(typeof(Sounds));

        //all entries already listed in existingEntries
        HashSet<Sounds> existingEntries = soundDict.Select(entry => entry.Key).ToHashSet();

        // check for missing and add it

        ///                                                                         BIG TODO: THIS BREAKS THE LIBRARY AND MESSES SOME REFERENCES UP!!!
        ///                                                                         remove?
      /*  foreach (Sounds sound in allSounds) {

            if (!existingEntries.Contains(sound)) {

                soundDict.Add(new SFXEntry(sound));
                Debug.Log("Automatically added new entry for " + sound + " to SFXLib");

            }

        }*/

        // automatically alphabetize
        //soundDict = soundDict.OrderBy(entry => entry.Key.ToString()).ToList();

        // check for dupe keys and popups in the library, as well as keys with no popup references
        if (logWarnings) {

            // check for duped keys 
            HashSet<Sounds> seenKeys = new HashSet<Sounds>();
            HashSet<Sounds> duplicateKeys = new HashSet<Sounds>();

            HashSet<PopupSound> seenSounds = new HashSet<PopupSound>();
            HashSet<PopupSound> duplicateSounds = new HashSet<PopupSound>();

            for (int i = 0; i < soundDict.Count; i++) {
                SFXEntry entry = soundDict[i];

                // if we have seen this key before
                if (!seenKeys.Add(entry.Key)) {

                    // if we havent reported this duplicate already
                    if (!duplicateKeys.Contains(entry.Key)) {

                        Debug.LogError("The SFXLib's soundDict has multiple definitions for the sound key " + entry.Key + ". Please remove duplicates.");

                        // Keep track of this key so we dont print a dupe warning for it again
                        duplicateKeys.Add(entry.Key);

                    }

                    // do NOT check for missing sounds on dupe objects, that leads to console spam 
                    continue;
                }

                List<PopupSound> entrySounds = soundDict[i].Sounds;

                // check if missing sound
                if (entrySounds == null || entrySounds.Count == 0)
                    Debug.LogError("The SFXLib's soundDict entry for " + entry.Key + " is missing a reference to a PopupSound (Element " + i + ")");
                else {

                    // check for dupes
                    foreach (PopupSound sound in entrySounds)
                        // if we have seen this sound before
                        if (!seenSounds.Add(sound))
                            // if we have not already reported this dupe
                            if (!duplicateSounds.Contains(sound)) {

                                Debug.LogError("The SFXLib has multiple keys associated with the PopupSound " + sound.name + ". This is unsupported: a PopupSound may only appear once in the library.");

                                // dont report this popup being duped again
                                duplicateSounds.Add(sound);

                            }

                }


            }

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
