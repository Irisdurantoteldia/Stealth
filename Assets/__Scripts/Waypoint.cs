using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Waypoint : MonoBehaviour
{
    [Tooltip("Temps en segons que l'enemic s'esperarà en aquest punt")]
    public float waitTime = 2f;

    [Tooltip("Objecte TextMeshPro per mostrar el número del waypoint")]
    public TextMeshPro waypointNumberText;

    private void OnDrawGizmos()
    {
        // Dibuixar una esfera al waypoint
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.3f);

        // Dibuixar una fletxa indicant la direcció
        Gizmos.color = Color.red;
        Vector3 direction = transform.forward;
        Gizmos.DrawRay(transform.position, direction * 1f);
    }

    // Mètode per actualitzar el número del waypoint
    public void SetWaypointNumber(int number)
    {
        if (waypointNumberText != null)
        {
            waypointNumberText.text = number.ToString();
        }
    }

    // Getters per accedir fàcilment a la posició i orientació
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Quaternion GetRotation()
    {
        return transform.rotation;
    }
}