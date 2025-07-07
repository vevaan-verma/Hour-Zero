using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

    /// <summary>
    /// This class keeps all audio-related code in one place for organization and standardization
    /// 
    /// I'm not a huge fan of this implementation, but here is how it works:
    ///     - Anything in the world that wants to play audio will utilize an AudioPlayer, 
    ///     most likely located on the object itself. 
    ///     
    ///     - The Popup system is being used here to have an optimized way to have as many audio
    ///     sources as needed per object at any time dynamically (pooling). However, Popups
    ///     do not have the best architecture for playing sounds specifically, which is what I
    ///     don't like 
    ///     
    ///     - AudioPlayer uses SFXLib as a dictionary of enums and Popups. Scripts only need to 
    ///     define which enums they want to use, and AudioPlayer handles getting the Popup by
    ///     using SFXLib 
    /// 
    /// </summary>
    /// 

    private SFXLib sfx;
    private PopupPlayer popups;

    private List<PopupSound> currentlyPlaying = new List<PopupSound>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

        sfx = FindAnyObjectByType<SFXLib>();
        popups = FindAnyObjectByType<PopupPlayer>();

    }

    public void Play(SFXLib.Sounds sound, bool looping = false, GameObject target = null) {

        // get the popup prefab
        PopupSound toPlay = sfx.GetPopup(sound);

        if (toPlay != null) {

            // play the popup and get the reference to the GameObject
            PopupSound playing = (PopupSound)popups.Play(toPlay, target ?? gameObject, looping);

            // start tracking this sound
            currentlyPlaying.Add(playing);

            // tell the PopupSound which AudioPlayer created it, so the PopupSound can later tell this AudioPlayer to stop tracking it when it finishes playing
            playing.SetOwner(this);
            // let it know what its key is
            // consider removing the key field in PopupSound and rely entirely on SFXLib.GetKey();
            playing.SetKey(sound);

        }
        else
            Debug.LogError("SFXLib: No PopupSound defined for SFXLib.Sounds." + sound);

    }

    // attempt to stop a sound of a given key. fails if no sound of that key is being tracked by this AudioPlayer
    public void Stop(SFXLib.Sounds key) {

        PopupSound toRemove = null;

        // check if a PopupSound this AudioPlayer is tracking has the given key
        foreach (PopupSound playing in currentlyPlaying)
            if (playing.GetKey() == key) {

                // found the PopupSound: now save the reference
                toRemove = playing;
                break;

            }

        // stop the sound from playing. Note that this will call Popup.OnFinish() which calls this.OnSoundComplete()
        if (toRemove != null)
            toRemove.StopPlaying();
        if (toRemove == null)
            Debug.LogError("No such sound " + key + " is currently acknowledged by " + gameObject.name + "'s AudioPlayer");

    }


    // like Stop, but this is only used when we know for a fact that this specific PopupSound is playing and being tracked
    //      by this AudioPlayer. For example, it is called when a PopupSound finishes playing
    public void OnSoundComplete(PopupSound sound) {

        if (sound != null)
            currentlyPlaying.Remove(sound);

    }

    public List<PopupSound> Playing() => currentlyPlaying;

}
