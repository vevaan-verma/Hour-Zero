using UnityEngine;

public class ItemInteractable : Interactable {

    [Header("References")]
    private Hotbar hotbar;

    [Header("Item")]
    [SerializeField] private ItemProperties item;

    private new void Start() {

        base.Start();
        hotbar = FindFirstObjectByType<Hotbar>();

    }

    public override void Interact() {

        base.Interact();
        hotbar.AddItem(item);
        Destroy(gameObject);

    }
}
