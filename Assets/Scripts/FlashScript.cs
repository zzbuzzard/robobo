using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashScript : MonoBehaviour
{
    private SpriteRenderer[] childSprites;
    private Material[] childMats;
    private static Material matWhite;

    private Coroutine currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        childSprites = GetComponentsInChildren<SpriteRenderer>();
        childMats = new Material[childSprites.Length];

        for (int i = 0; i < childSprites.Length; i++)
            childMats[i] = childSprites[i].material;

        if (matWhite == null)
            matWhite = Resources.Load<Material>("Materials/WhiteFlash");
    }

    private void ResetMaterial()
    {
        for (int i = 0; i < childSprites.Length; i++)
        {
            childSprites[i].material = childMats[i];
        }
        currentCoroutine = null;
    }

    public void Flash()
    {
        if (childSprites == null) return;

        for (int i = 0; i < childSprites.Length; i++)
        {
            childSprites[i].material = matWhite;
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ResetTimer());
    }

    IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(0.15f);
        ResetMaterial();
    }
}
