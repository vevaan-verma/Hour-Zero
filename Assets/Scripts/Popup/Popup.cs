using System.Collections;
using UnityEngine;

public abstract class Popup : MonoBehaviour {

    private Vector3? targetPosition;
    private GameObject targetObject;
    [Header("Default Duration")]
    [SerializeField] protected float duration;

    private PopupPlayer popups;

    // switches the component configuration of this popup to that of Popup other
    // ex: for a PopupItem, wwap out the sprite and the runtime animation controller
    public abstract void SwapPopup(Popup popup);

    // get/set all components
    // call this after instantiating a new popup
    public abstract void Initialize();

    // Popup plays either at a GameObject, or at a fixed position (set in PopupPlayer).
    // it will either play forever or for a fixed duration, which can be the default
    //      duration or a custom one
    public IEnumerator HandlePlay(float? overrideDuration = null, GameObject target = null, bool persistent = false) {

        float remaining = overrideDuration ?? duration;
        gameObject.SetActive(true);

        if (persistent)
            while (gameObject.activeSelf) {
                if (target != null)
                    transform.position = target.transform.position;

                yield return new WaitForEndOfFrame();
            }
        else {

            while (remaining > 0f) {
                if (target != null)
                    transform.position = target.transform.position;

                yield return new WaitForEndOfFrame();
                remaining -= Time.deltaTime;
            }

            gameObject.SetActive(false);
        }

        OnFinish();

    }

    // deactivate object if its active
    public void StopPlaying() {

        // Popups are deactivated when not playing, and you cant stop a stopped Popup
        if (gameObject.activeSelf)
            gameObject.SetActive(false);

    }

    // repool this Popup. override to enable special behavior on finish.
    // WHEN REIMPLEMENTING, MAKE SURE TO CALL base.OnFinish()!!!!!!!!!!
    protected virtual void OnFinish() {

        if (popups == null)
            popups = FindAnyObjectByType<PopupPlayer>();

        popups.Pool(this);

    }

    #region Legacy

    /*
    // for a FIXED POSITION,
    // passed into Play(Popup popup, Vector3 position, float duration)
    // overrides default duration
    public IEnumerator HandlePlay(float duration) {

        float elapsed = duration;

        gameObject.SetActive(true);

        while (elapsed > 0) {

            yield return new WaitForEndOfFrame();
            elapsed -= Time.deltaTime;

        }

        gameObject.SetActive(false);

    }

    // for a CHANGING POSITION, copying that of a GameObject,
    // passed into PopupPlayer.Play(Popup popup, GameObject target, float duration)
    // overrides default duration
    public IEnumerator HandlePlay(GameObject target, float duration) {

        float elapsed = duration;

        gameObject.SetActive(true);

        while (elapsed > 0) {

            yield return new WaitForEndOfFrame();
            elapsed -= Time.deltaTime;

            if (target != null)
                gameObject.transform.position = target.transform.position;

        }

        gameObject.SetActive(false);

    }

    // for a FIXED POSITION,
    // passed into Play(Popup popup, Vector3 position)
    // uses default duration, or is infinite if peristent = true
    public IEnumerator HandlePlay(bool persistent) {

        float elapsed = duration;

        gameObject.SetActive(true);

        if (!persistent) {

            while (elapsed > 0) {

                yield return new WaitForEndOfFrame();
                elapsed -= Time.deltaTime;

            }

            gameObject.SetActive(false);

        }

        // no need to deactivate the object, that is already done in Stop();

    }

    // for a CHANGING POSITION, copying that of a GameObject,
    // passed into PopupPlayer.Play(Popup popup, GameObject target)
    // uses default duration, or is infinite if peristent = true
    public IEnumerator HandlePlay(GameObject target, bool persistent) {

        float elapsed = duration;

        gameObject.SetActive(true);

        if (!persistent) {

            while (elapsed > 0) {

                yield return new WaitForEndOfFrame();
                elapsed -= Time.deltaTime;

                if (target != null)
                    gameObject.transform.position = target.transform.position;

            }

            gameObject.SetActive(false);
        }
        else
            while (gameObject.activeSelf) {

                yield return new WaitForEndOfFrame();

                if (target != null)
                    gameObject.transform.position = target.transform.position;

            }


        // no need to deactivate the object, that is already done in Stop();

    }
    */

    #endregion
}