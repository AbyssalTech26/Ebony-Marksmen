using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 0.1f; //Time before the object is destroyed
    void Start()
    {
        Destroy(gameObject, time);   
    }
}
