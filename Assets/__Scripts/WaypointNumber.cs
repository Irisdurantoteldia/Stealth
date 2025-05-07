using UnityEngine;
using TMPro;

public class WaypointNumber : MonoBehaviour
{
    [Tooltip("El component TextMeshPro que mostra el número")]
    public TextMeshPro numberText;

    [Tooltip("Si cert, el text sempre s'orientarà cap a la càmera")]
    public bool lookAtCamera = true;

    private Camera mainCamera;

    private void Start()
    {
        // Trobar la càmera principal si és necessari
        if (lookAtCamera)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("Camera.main no trobada. Desactivant lookAtCamera.");
                lookAtCamera = false;
            }
        }
    }

    private void LateUpdate()
    {
        // Fer que el text miri cap a la càmera
        if (lookAtCamera && mainCamera != null && numberText != null)
        {
            numberText.transform.LookAt(mainCamera.transform);
            // Invertir la direcció perquè el text sigui llegible
            numberText.transform.Rotate(0, 180, 0);
        }
    }

    // Establir el número del waypoint
    public void SetNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number.ToString();
        }
    }
}