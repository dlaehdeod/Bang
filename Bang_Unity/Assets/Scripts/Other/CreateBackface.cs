using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBackface : MonoBehaviour
{
    public GameObject backfacePrefab;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            GameObject obj = Instantiate(backfacePrefab, transform.GetChild(i));
        }

        Destroy(this);
    }
}
