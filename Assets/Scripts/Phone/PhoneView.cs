using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class PhoneView {

    [Header("References")]
    protected PhoneManager phoneManager;
    private MonoBehaviour coroutineHost; // host for coroutines so they can be run through this class
    private Coroutine viewCloseCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup viewMenu;
    private Animator viewAnimator;

    [Header("Data")]
    [SerializeField] protected ViewType viewType;

    public void Initialize(PhoneManager phoneManager) {

        this.phoneManager = phoneManager;
        viewAnimator = viewMenu.GetComponent<Animator>();
        coroutineHost = phoneManager; // Use PhoneManager as the host for coroutines

    }

    public void OpenView() {

        if (viewCloseCoroutine != null) coroutineHost.StopCoroutine(viewCloseCoroutine); // stop any existing close coroutine

        viewMenu.gameObject.SetActive(true); // show the view menu
        viewAnimator.SetTrigger("openView"); // trigger the animation to open the view

    }

    public virtual void CloseView() {

        if (viewCloseCoroutine != null) coroutineHost.StopCoroutine(viewCloseCoroutine); // stop any existing close coroutine
        viewCloseCoroutine = coroutineHost.StartCoroutine(HandleViewClose()); // start the coroutine to handle view close animation

    }

    public void ForceCloseView() {

        if (viewCloseCoroutine != null) coroutineHost.StopCoroutine(viewCloseCoroutine); // stop any existing close coroutine
        viewMenu.gameObject.SetActive(false); // hide the view menu immediately without animation

    }

    private IEnumerator HandleViewClose() {

        viewAnimator.SetTrigger("closeView"); // trigger the animation to close the view

        yield return null; // wait for the next frame to ensure the animation starts
        yield return new WaitForSeconds(viewAnimator.GetCurrentAnimatorStateInfo(0).length); // wait for the animation to finish

        viewMenu.gameObject.SetActive(false); // hide the view menu after the animation is done

    }

    public ViewType GetViewType() => viewType;

}

[Serializable]
public class AppView : PhoneView {

    [Header("UI References")]
    private AppButton appButton;

    [Header("Data")]
    [SerializeField] private string appName;
    [SerializeField] private Sprite appIcon;
    private int notificationCount;

    public void Initialize(PhoneManager phoneManager, AppButton appButton) {

        base.Initialize(phoneManager);
        this.appButton = appButton;

    }

    public override void CloseView() {

        phoneManager.ClearAppTrayNotifications(viewType); // clear any notifications for this view type
        base.CloseView(); // call the base method to close the view

    }

    public string GetName() => appName;

    public Sprite GetIcon() => appIcon;

    public void IncrementNotificationCount() => appButton.SetNotificationCount(++notificationCount);

    public void ResetNotificationCount() {

        notificationCount = 0; // reset the notification count
        appButton.SetNotificationCount(notificationCount); // update the app button with the new count

    }
}

[Serializable]
public class NotificationTrayView : PhoneView {


}