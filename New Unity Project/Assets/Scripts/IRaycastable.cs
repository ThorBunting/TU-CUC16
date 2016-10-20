using UnityEngine;

public abstract class IRaycastable : MonoBehaviour {
    public abstract void OnClick();
    public abstract void OnHoverEnter();
    public abstract void OnHoverStay();
    public abstract void OnHoverExit();
}