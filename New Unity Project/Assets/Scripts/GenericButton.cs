using UnityEngine;
using System.Collections;
using System;

public class GenericButton : IRaycastable {

    public Texture normalTex;
    public Texture hoverTex;
    public Texture pressTex;
    public TextMesh text;
    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    public override void OnClick() {
        text.text = "Clicked";
        mat.mainTexture = pressTex;
    }

    public override void OnHoverEnter() {
        text.text = "Enter Hover";
        mat.mainTexture = hoverTex;
    }

    public override void OnHoverExit() {
        text.text = "Exit Hover";
        mat.mainTexture = normalTex;
    }

    public override void OnHoverStay() {
        text.text = "Still Hoverin'";
    }
}
