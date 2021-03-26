using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickScript : MonoBehaviour
{
    public Action onClick;

    private void OnMouseUpAsButton()
    {
        onClick.DynamicInvoke();
    }
}
