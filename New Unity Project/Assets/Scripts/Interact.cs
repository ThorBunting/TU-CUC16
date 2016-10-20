using UnityEngine;
using System.Collections;

public class Interact : MonoBehaviour {
    private IRaycastable m_target;
    [SerializeField]
    private string m_clickInput = "Jump";

    #region Cursor Over

    private RaycastHit[] CursorOver() {
        Vector2 point = new Vector2();
        point.x = Screen.width / 2;
        point.y = Screen.height / 2;
        return Physics.RaycastAll(Camera.main.ScreenPointToRay(point));
    }


    private void OnHover() {
        RaycastHit[] rays = CursorOver();
        foreach(RaycastHit ray in rays) {
            IRaycastable comp = ray.collider.gameObject.GetComponent<IRaycastable>();
            if(comp != null) {
                if(comp != m_target) {
                    if(m_target)
                        m_target.OnHoverExit();
                    m_target = comp;
                    comp.OnHoverEnter();
                }
                comp.OnHoverStay();
                break;
            } else {
                if(m_target) {
                    m_target.OnHoverExit();
                    m_target = null;
                }
            }
        }
    }

    /// <summary>
    /// Calls the click events of the top clickable object.
    /// </summary>
    private void OnClick() {
        if(m_target != null && Input.GetButtonDown(m_clickInput)) {
            m_target.OnClick();
        }
    }

    #endregion

    void Start() {
    }

    void Update() {
        OnHover();
        OnClick();
    }
}