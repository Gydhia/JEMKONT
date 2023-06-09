using UnityEngine;

public class SpriteRotation : MonoBehaviour
{
    public float rotationSpeed = 10f; // Vitesse de rotation du sprite

    void Update()
    {
        // Rotation du sprite sur l'axe Z
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
