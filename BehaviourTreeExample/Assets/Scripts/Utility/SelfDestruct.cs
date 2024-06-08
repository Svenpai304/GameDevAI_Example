using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float lifetime;

    private void FixedUpdate()
    {
        lifetime -= Time.deltaTime;
        if (lifetime < 0) { Destroy(gameObject); }
    }
}
