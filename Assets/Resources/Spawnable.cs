/* Spawnable
 * 
 * This script initilizes shader settings for the spawnable objects, so that the 
 *  highlight color of a spawnable object (present in the HUD?) differs from the
 *  color of a selectable object. Moreover, this script must be present in any
 *  object that is to be used as a spawnable object, as this is the script that
 *  the hand looks for in an object in order to determine if it should grab, or
 *  create a new object.
 */

using UnityEngine;

public class Spawnable : MonoBehaviour
{
    public Shader standardShader;
    public Shader outlineShader;

	void Awake ()
    {
        // Pre-initilize some shader components
        Renderer thisRenderer = gameObject.GetComponent<Renderer>();
        thisRenderer.material.shader = outlineShader;
        thisRenderer.material.SetColor("_OutlineColor", Color.green);
        thisRenderer.material.shader = standardShader;
    }
}
