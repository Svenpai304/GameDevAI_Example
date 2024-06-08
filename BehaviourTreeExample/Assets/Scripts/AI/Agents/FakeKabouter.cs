using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeKabouter : MonoBehaviour
{
    [HideInInspector] public bool Found = false;
    [SerializeField] private GameObject OnDestroyedParticles;

    public void Setup(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            Found = true;
        }
    }

    public void Die(bool useParticles)
    {
        if (useParticles)
        {
            Instantiate(OnDestroyedParticles, transform.position, transform.rotation).GetComponent<ParticleSystem>().Play();
        }

        Destroy(gameObject);
    }
}
