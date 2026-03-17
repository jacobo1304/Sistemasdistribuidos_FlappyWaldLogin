using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class LoadSceneOnCollision : MonoBehaviour
{
    [Tooltip("The name of the scene to load upon collision.")]
    public string sceneNameToLoad = "Nivel3";

    [Tooltip("The tag of the object that should trigger the scene load (e.g., \"Player\").")]
    public string triggeringTag = "Player";

    // Start is called before the first frame update
    void Start()
    {
        // You can add initialization code here if needed
    }

    // Update is called once per frame
    void Update()
    {
        // You can add update code here if needed
    }

    // Called when another Collider2D enters a trigger Collider2D attached to this object.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object has the specified tag
        if (other.gameObject.CompareTag(triggeringTag))
        {
            Debug.Log($"Triggered by {other.gameObject.name} which has the tag '{triggeringTag}'. Loading scene: {sceneNameToLoad}");
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.Log($"Triggered by {other.gameObject.name} (Tag: {other.gameObject.tag}), but it does not have the tag '{triggeringTag}'.");
        }
    }

    // OnCollisionEnter2D is used for non-trigger physics collisions.
    // If you intend a physical collision to load the scene, uncomment this and comment OnTriggerEnter2D.
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the colliding object has the specified tag
        if (collision.gameObject.CompareTag(triggeringTag))
        {
            Debug.Log($"Collided with {collision.gameObject.name} which has the tag '{triggeringTag}'. Loading scene: {sceneNameToLoad}");
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.Log($"Collided with {collision.gameObject.name}, but it does not have the tag '{triggeringTag}'.");
        }
    }
    */

    // If your game is 3D, you would use OnTriggerEnter or OnCollisionEnter respectively:
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(triggeringTag))
        {
            Debug.Log($"Triggered by {other.gameObject.name} which has the tag '{triggeringTag}'. Loading scene: {sceneNameToLoad}");
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.Log($"Triggered by {other.gameObject.name}, but it does not have the tag '{triggeringTag}'.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(triggeringTag))
        {
            Debug.Log($"Collided with {collision.gameObject.name} which has the tag '{triggeringTag}'. Loading scene: {sceneNameToLoad}");
            SceneManager.LoadScene(sceneNameToLoad);
        }
        else
        {
            Debug.Log($"Collided with {collision.gameObject.name}, but it does not have the tag '{triggeringTag}'.");
        }
    }
    */
}
