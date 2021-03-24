using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyWhenDone : MonoBehaviour
{
    public ParticleSystem particles;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, particles.main.duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
