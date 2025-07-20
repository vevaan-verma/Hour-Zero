using Nobi.UiRoundedCorners;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class AppManager : MonoBehaviour {

    [Header("References")]
    private Coroutine refreshLayoutCoroutine;

    public abstract void RefreshApp();

    protected void RefreshLayout(RectTransform root) {

        if (refreshLayoutCoroutine != null) StopCoroutine(refreshLayoutCoroutine); // stop any existing layout refresh coroutine
        refreshLayoutCoroutine = StartCoroutine(HandleRefreshLayout(root));

    }

    private IEnumerator HandleRefreshLayout(RectTransform root) {

        yield return null; // wait for the end of the frame to ensure all UI elements are properly initialized

        foreach (LayoutGroup layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        refreshLayoutCoroutine = null; // reset the coroutine reference after completion

    }
}
