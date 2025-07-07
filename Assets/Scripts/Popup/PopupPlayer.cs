using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPlayer : MonoBehaviour {

    /// <summary>
    /// 
    /// used to play popups
    /// ideally, this script should handle everything, so other classes only ever need one simple call to PlayPopup
    /// also handles pooling popups
    /// 
    /// use this to create a popup object that appears at a location for a time
    /// once the time runs out, the popup object is deactivated and added to the pool
    /// later calls to PlayPopup will prefer reactivating a pooled popup over making a new one
    /// new popup objects are only created when nothing in the pool is available for use
    /// 
    /// </summary>

    [Header("Pool")]
    [SerializeField] private GameObject poolParent;
    [SerializeField][Tooltip("Set to zero for no max. Reccomended to leave at zero unless memory is super expensive (mobile game, or super heavy popups")][Min(0)] private int maxPoolSize;
    [SerializeField][Tooltip("Use to spawn pooled popups on Start. Each entry should contain a different type of Popup Prefab, probably the ones in /[Prefabs]/Popups/Init")] private PopupPoolConfigurator[] poolInit;

    [Header("Debug")]
    [SerializeField] private bool logMetrics;
    int trackedMaxSize;
    float timeSinceLastMax;

    // the pool is primarily accessed using poolParent, with all of its children being pooled items and all pooled items being one of its children 
    // this Stack is only used to facilitate destroying pooled items when a maxPoolSize is set
    // given the nature of a Stack, the oldest popup will be the one to be destroyed when a new one needs to be made and the max has been reached
    private Stack<GameObject> pool;

    #region Pooling 

    private void Start() {

        pool = new Stack<GameObject>();

        trackedMaxSize = 0;
        timeSinceLastMax = 0;

        // spawn initial pool
        foreach (PopupPoolConfigurator config in poolInit) {

            trackedMaxSize += config.NumToSpawn;

            for (int i = 0; i < config.NumToSpawn; i++)
                SpawnNewPopup(config.Popup).gameObject.SetActive(false);

        }

    }

    private void Update() {

        if (maxPoolSize != 0 && pool.Count > maxPoolSize) {

            GameObject pooledItem = pool.Pop();

            StartCoroutine(QueueForDestruction(pooledItem));

        }
        if (pool.Count > trackedMaxSize) {

            trackedMaxSize = pool.Count;

            Debug.Log("New max pool size reached: " + trackedMaxSize + ". Last max occured " + timeSinceLastMax + "s ago");

            timeSinceLastMax = 0;

        }

        timeSinceLastMax += Time.deltaTime;

    }

    // try to get a pooled item of a specific Popup type 
    // for type, pass in a child class of Popup (ex: PopupIcon)
    private Popup GetPooledItem(Type type) {

        int itemCount = poolParent.transform.childCount;

        for (int i = 0; i < itemCount; i++) {

            Popup item = poolParent.transform.GetChild(i).GetComponent<Popup>();

            // we want an INACTIVE one, meaning it is not currently playing
            if (!item.gameObject.activeSelf && item.GetType() == type)
                return item;

        }

        return null;

    }

    // allows a Popup to finish playing then destroys it
    // note that Popups are only active when playing 
    // they deactivate when they are done playing and want to swim in the pool
    private IEnumerator QueueForDestruction(GameObject pooledItem) {

        while (pooledItem.activeSelf == true)
            yield return new WaitForEndOfFrame();

        Destroy(pooledItem);

    }

    private Popup SpawnNewPopup(Popup popup) {

        Popup spawned = Instantiate(popup, poolParent.transform);

        spawned.Initialize();

        pool.Push(spawned.gameObject);

        return spawned;

    }

    #endregion

    #region Playing

    // All methods return the Popup being played
    // What you pass in and how it changes behavior:
    //      Vector3 position -> fixed position to play Popup at
    //      GameObject target -> make the Popup always stick to a certain object so the position can change
    //      float overrideDuration -> duration to play the popup for, overriding the default duration
    //      bool persistent -> set to true for an infinitely playing popup (to be pooled manually), false to play for default duration

    // Play a popup at a FIXED POSITION for a FIXED DURATION
    public Popup Play(Popup popup, Vector3 position, float duration) => Play(popup, fixedPosition: position, overrideDuration: duration);

    // play a popup at a CHANGING LOCATION (sticking to a GameObject) for a FIXED DURATION
    public Popup Play(Popup popup, GameObject targetObj, float duration) => Play(popup, target: targetObj, overrideDuration: duration);

    // play a popup at a FIXED POSITION
    // if persistent = true it will play forever unless manually stopped
    // if persistent = false it will play for the default duration
    public Popup Play(Popup popup, Vector3 position, bool persistent) => Play(popup, fixedPosition: position, persistent: persistent);

    // play a popup at a FIXED POSITION
    // if persistent = true it will play forever unless manually stopped
    // if persistent = false it will play for the default duration
    public Popup Play(Popup popup, GameObject targetObj, bool persistent) => Play(popup, target: targetObj, persistent: persistent);


    // "Master method" for playing popups
    //  popup is the only required field 
    private Popup Play(Popup popup, Vector3? fixedPosition = null, GameObject target = null, float? overrideDuration = null, bool persistent = false) {

        Popup toPlay = GetPooledItem(popup.GetType());

        if (toPlay != null)
            toPlay.SwapPopup(popup);
        else
            toPlay = SpawnNewPopup(popup);

        if (target != null)
            toPlay.transform.position = target.transform.position;
        else if (fixedPosition.HasValue)
            toPlay.transform.position = fixedPosition.Value;

        StartCoroutine(toPlay.HandlePlay(overrideDuration, target, persistent));

        return toPlay;

    }

    #endregion

    #region Playing (Legacy)

    /*
    // play a popup at a FIXED POSITION for a duration
    // popup being played is returned to allow for manual pooling
    public Popup Play(Popup popup, Vector3 position, float duration) {

        Popup toPlay = GetPooledItem(popup.GetType());

        // found available pooled item of the right type
        if (toPlay != null)
            toPlay.SwapPopup(popup);

        else {

            toPlay = Instantiate(popup, poolParent.transform);
            toPlay.Initialize();
            pool.Push(toPlay.gameObject);

        }

        toPlay.transform.position = position;
        StartCoroutine(toPlay.HandlePlay(duration));

        return toPlay;

    }

    // play a popup AT THE POSITION OF A TARGET for a duration
    // popup being played is returned to allow for manual pooling
    public Popup Play(Popup popup, GameObject target, float duration) {

        Popup toPlay = GetPooledItem(popup.GetType());

        // found available pooled item of the right type
        if (toPlay != null)
            toPlay.SwapPopup(popup);

        else {

            toPlay = Instantiate(popup, poolParent.transform);
            toPlay.Initialize();
            pool.Push(toPlay.gameObject);

        }

        toPlay.transform.position = target.transform.position;
        StartCoroutine(toPlay.HandlePlay(target, duration));

        return toPlay;

    }

    // play a popup at a FIXED POSITION for the default duration of the popup
    // set persistent to true for manual pooling using Popup.Stop() (the popup will not automatically go away)
    // popup being played is returned to allow for manual pooling
    public Popup Play(Popup popup, Vector3 position, bool persistent) {

        Popup toPlay = GetPooledItem(popup.GetType());

        // found available pooled item of the right type
        if (toPlay != null)
            toPlay.SwapPopup(popup);

        else {

            toPlay = Instantiate(popup, poolParent.transform);
            pool.Push(toPlay.gameObject);

        }

        toPlay.transform.position = position;
        StartCoroutine(toPlay.HandlePlay(persistent));

        return toPlay;

    }


    // play a popup AT THE POSITION OF A TARGET for the default duration of the popup
    // set persistent to true for manual pooling using Popup.Stop() (the popup will not automatically go away)
    // popup being played is returned to allow for manual pooling
    public Popup Play(Popup popup, GameObject target, bool persistent) {

        Popup toPlay = GetPooledItem(popup.GetType());

        // found available pooled item of the right type
        if (toPlay != null)
            toPlay.SwapPopup(popup);

        else {

            toPlay = Instantiate(popup, poolParent.transform);
            toPlay.Initialize();
            pool.Push(toPlay.gameObject);

        }

        toPlay.transform.position = target.transform.position;

        StartCoroutine(toPlay.HandlePlay(target, persistent));

        return toPlay;

    }
    */

    #endregion

}
