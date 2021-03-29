using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkScript : MonoBehaviour
{
    public static GameObject sparkPrefab;
    public static void CreateSparks(Vector2 position, float damage)
    {
        GameObject sparkObj = Instantiate(sparkPrefab, (Vector3)position + new Vector3(0, 0, -1), Quaternion.identity);
        sparkObj.GetComponent<SparkScript>().SetIntensity(damage);
    }

    [SerializeField]
    private ParticleSystem particles;

    private const float maxdamage = 50.0f;

    public void SetIntensity(float damage)
    {
        float multiplier = Mathf.Clamp(damage / maxdamage, 0.0f, 1.0f);

        var part = particles.main;
        part.startSpeedMultiplier = Mathf.Max(0.5f, multiplier);

        // Max damage = red, min damage = yellow (I dunno, hotter I guess)
        part.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, new Color(1.0f, 1-multiplier, 0.0f));

        var burst = particles.emission.GetBurst(0);
        burst.count = new ParticleSystem.MinMaxCurve(Mathf.Max(150 * multiplier, 15));
        particles.emission.SetBurst(0, burst);
    }
}
