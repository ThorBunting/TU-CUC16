using UnityEngine;
using System.Collections;
using System;

public class TestInteract : IRaycastable {

    public override void OnClick() {
        gameObject.transform.position += Vector3.left;
    }

    public override void OnHoverEnter() {
        gameObject.transform.localScale = new Vector3(2, 2, 2);
    }

    public override void OnHoverExit() {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    public override void OnHoverStay() {
        gameObject.transform.Rotate(Vector3.up * Time.deltaTime * 60);
    }
}
