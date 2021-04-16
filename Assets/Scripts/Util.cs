using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    // Rotates by 90 degrees anticlockwise
    public static Vector2 Rotate(Vector2 r)
    {
        return new Vector2(-r.y, r.x);
    }

    // Rotates by 90 * rotate degrees anticlockwise
    public static Vector2 RotateBy(Vector2 r, int rotate)
    {
        rotate = (rotate % 4 + 4) % 4;
        for (int i = 0; i < rotate; i++)
            r = Rotate(r);
        return r;
    }

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

    // -0.3 -> 0.7
    // 3.8 -> 0.8
    // etc.
    public static float MoveToOne(float f)
    {
        // x = floor(f), then
        // x < f < x+1
        // 0 < f-x < 1
        return f - Mathf.Floor(f);
    }

    // f(5, 360) = 5, etc.
    public static float AngleDifDegree(float a, float b)
    {
        // TODO: Wtf does this even do
        a %= 360.0f;
        b %= 360.0f;
        return Mathf.Min(Mathf.Abs(a - b), Mathf.Abs(a - b - 360));
    }

    public static string Percentage(float a, int decimal_places = 1)
    {
        return (a * 100).ToString("n" + decimal_places) + "%";
    }

    public static float SqDist(Vector2 a, Vector2 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }
}
