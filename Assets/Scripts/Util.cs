using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    // Rotates by 90 degrees anticlockwise
    public static Vector2Int Rotate(Vector2Int r) {
        return new Vector2Int(-r.y, r.x);
    }

    // Rotates by 90 * rotate degrees anticlockwise
    public static Vector2Int RotateBy(Vector2Int r, int rotate) {
        rotate = (rotate % 4 + 4) % 4;
        for (int i=0; i<rotate; i++)
            r = Rotate(r);
        return r;
    }
}
