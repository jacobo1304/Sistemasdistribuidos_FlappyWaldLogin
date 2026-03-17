using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [Header("Fallback on Disable/Destroy")]
    [Tooltip("GameObject to activate when this object is deactivated or destroyed.")]
    public GameObject fallbackObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        if (fallbackObject != null)
        {
            fallbackObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (fallbackObject != null)
        {
            fallbackObject.SetActive(true);
        }
    }
}
