using UnityEngine;

public class PopupIcon : Popup {

    private SpriteRenderer spriteRenderer;
    private Animator anim;

    /// <summary>
    /// 
    /// PopupIcons are... icons!
    /// they can have a SpriteRenderer and an Animator
    /// 
    /// </summary>

    public override void Initialize() {

        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        gameObject.name = "Popup Icon";
        //idx++;

    }

    // replace this's sprite and animationcontroller 
    // with the ones from other
    override public void SwapPopup(Popup other) {

        if (other is not PopupIcon) {

            Debug.Log("Failed to update Popup... Popup passed in must be a PopupIcon");
            return;

        }

        transform.localScale = other.transform.localScale;
        transform.localEulerAngles = other.transform.localEulerAngles;


        //PopupIcon other = (PopupIcon)popup;
        SpriteRenderer otherSpriteRenderer = other.GetComponent<SpriteRenderer>();
        Animator otherAnimator = other.GetComponent<Animator>();

        if (otherSpriteRenderer != null) {

            spriteRenderer.sprite = otherSpriteRenderer.sprite;
            spriteRenderer.color = otherSpriteRenderer.color;
            spriteRenderer.flipX = otherSpriteRenderer.flipX;
            spriteRenderer.flipY = otherSpriteRenderer.flipY;
            spriteRenderer.drawMode = otherSpriteRenderer.drawMode;
            spriteRenderer.spriteSortPoint = otherSpriteRenderer.spriteSortPoint;
            spriteRenderer.sharedMaterial = otherSpriteRenderer.sharedMaterial;
            spriteRenderer.sortingLayerID = otherSpriteRenderer.sortingLayerID;
            spriteRenderer.sortingOrder = otherSpriteRenderer.sortingOrder;
            spriteRenderer.renderingLayerMask = otherSpriteRenderer.renderingLayerMask;

        }
        if (otherAnimator != null) {

            anim.runtimeAnimatorController = otherAnimator.runtimeAnimatorController;
            anim.applyRootMotion = otherAnimator.applyRootMotion;
            anim.animatePhysics = otherAnimator.animatePhysics;
            anim.updateMode = otherAnimator.updateMode;
            anim.cullingMode = otherAnimator.cullingMode;

        }


    }

}