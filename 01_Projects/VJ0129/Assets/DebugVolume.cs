using UnityEngine;

public class DebugVolume : MonoBehaviour
{
    public float Level { get;  set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Level);
    }
}
