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
    /// PopupPlayer also creates a bunch of Popups on Start, saving memory.
    /// 
    /// </summary>

    [Header("Pool")]
    [SerializeField][Tooltip("All popups are children of this GameObject's transform")] private GameObject poolParent;
    [SerializeField][Tooltip("Reccomended to leave this on unless using really expensive popups or are in a very memory-tight scenario")] private bool infinitePool;
    [SerializeField][Tooltip("Will start deleting Popups after the pool reaches this size")][Range(0, 1000)] private int maxPoolSize;
    [SerializeField][Tooltip("Use to spawn pooled popups on Start. Each entry should contain a different type of Popup Prefab, probably the ones in /[Prefabs]/Popups/Init")] private PopupPoolConfigurator[] poolInit;

    [Header("Debug")]
    [SerializeField] private bool logMetrics;
    int trackedMaxSize;
    float timeSinceLastMax;

    private Dictionary<Type, Queue<GameObject>> pool;

    #region Pooling 

    private void Awake() {

        pool = new(); // sus little shortcut

        trackedMaxSize = 0;
        timeSinceLastMax = 0;

        if (!infinitePool) {

            int initPoolSize = 0;

            foreach (PopupPoolConfigurator config in poolInit)
                initPoolSize += config.NumToSpawn;

            if (maxPoolSize < initPoolSize) {
                Debug.LogWarning("The max PopupPlayer pool size is lower than the configured initial pool size: the max pool size has been automatically adjusted.");
                maxPoolSize = initPoolSize;
            }

        }

        // spawn initial pool
        foreach (PopupPoolConfigurator config in poolInit) {

            trackedMaxSize += config.NumToSpawn;

            for (int i = 0; i < config.NumToSpawn; i++) {

                Popup spawned = SpawnNewPopup(config.Popup);
                Pool(spawned);
                spawned.gameObject.SetActive(false);

            }

        }

    }

    private void Update() {

        // destroy pooled objects when max pool size is reached, and log metrics

        // don't waste time on any of Update if the pool is infinite and you doesn't want metrics
        // the null check on pool is mainly just because i don't want this to run with [AlwaysExecute], i just want OnValidate
        if (pool != null && (logMetrics || !infinitePool)) {

            // get total pool size
            // also figure out which type of popup has the most stuff in it
            int poolSize = 0;
            int maxPoolStackSize = 0;
            Type maxPoolStackType = null;

            foreach (Type key in pool.Keys) {

                int poolStackSize = pool[key].Count;

                poolSize += poolStackSize;

                if (poolStackSize > maxPoolStackSize) {

                    maxPoolStackSize = poolStackSize;
                    maxPoolStackType = key;

                }
            }

            // if pool does not have inf size, the pool has surpassed the max pool size, and the Queue is not empty or missing for the most pool-hogging Type
            if (!infinitePool && poolSize > maxPoolSize && maxPoolStackType != null && pool[maxPoolStackType].Count == 0)
                StartCoroutine(QueueForDestruction(pool[maxPoolStackType].Dequeue()));

            if (logMetrics && poolSize > trackedMaxSize) {

                trackedMaxSize = poolSize;

                Debug.Log("New max pool size reached: " + trackedMaxSize + ". Last max occured " + timeSinceLastMax + "s ago");

                timeSinceLastMax = 0;

            }

            timeSinceLastMax += Time.deltaTime;
        }

    }

    public void Pool(Popup popup) {

        Type key = popup.GetType();

        if (pool.ContainsKey(key))
            pool[key].Enqueue(popup.gameObject);
        else {

            pool.Add(key, new Queue<GameObject>());
            pool[key].Enqueue(popup.gameObject);

        }

    }

    // allows a Popup to finish playing then destroys it
    private IEnumerator QueueForDestruction(GameObject pooledItem) {

        while (pooledItem.activeSelf == true)
            yield return new WaitForEndOfFrame();

        Destroy(pooledItem);

    }

    // spawn and initialize a new Popup, then pool it. returns the spawned Popup. it is inactive by default
    private Popup SpawnNewPopup(Popup popup) {

        Popup spawned = Instantiate(popup, poolParent.transform);

        spawned.Initialize();

        //Pool(spawned);

        return spawned;

    }

    // try to get a pooled item of a specific Popup type 
    // for type, pass in a child class of Popup (ex: PopupIcon)
    // popups are dequeued on play, requeued when told to by the popup
    // check that a Queue of that type exists in the pool dict, and that it has stuff in it. if so, give me something from it!!
    private Popup GetPooledItem(Type type) => pool.TryGetValue(type, out Queue<GameObject> typeQueue) && typeQueue.Count > 0 ? typeQueue.Dequeue().GetComponent<Popup>() : null;

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

        // got popup from pool
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

    #region Custom Editor

#if UNITY_EDITOR

    // hide the maxPoolSize field if infinitePool = true
    // using UnityEditor prefix to avoid needing to hide the import in the final build -vv
    [UnityEditor.CustomEditor(typeof(PopupPlayer), true)]
    public class PopupPlayerEditor : UnityEditor.Editor {

        public override void OnInspectorGUI() {

            serializedObject.Update();

            // make sure its in the right order
            UnityEditor.SerializedProperty poolParentProp = serializedObject.FindProperty("poolParent");
            UnityEditor.SerializedProperty infinitePoolProp = serializedObject.FindProperty("infinitePool");
            UnityEditor.SerializedProperty maxPoolSizeProp = serializedObject.FindProperty("maxPoolSize");
            UnityEditor.SerializedProperty poolInitProp = serializedObject.FindProperty("poolInit");
            UnityEditor.SerializedProperty logMetricsProp = serializedObject.FindProperty("logMetrics");

            UnityEditor.EditorGUILayout.PropertyField(poolParentProp);

            UnityEditor.EditorGUILayout.PropertyField(infinitePoolProp);

            if (!infinitePoolProp.boolValue)
                UnityEditor.EditorGUILayout.PropertyField(maxPoolSizeProp);

            UnityEditor.EditorGUILayout.PropertyField(poolInitProp, true);
            UnityEditor.EditorGUILayout.PropertyField(logMetricsProp);

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif

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
