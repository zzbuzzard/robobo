using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileOnlyObj : MonoBehaviour
{
    private void Awake()
    {
        if (SystemInfo.deviceType != DeviceType.Handheld)
            Destroy(gameObject);
    }
}
