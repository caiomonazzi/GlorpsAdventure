using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class PlayerCamera : MonoBehaviour
{
    #region Variables
    [Header("Variables")]
    //Player position
    Transform playerTransform;

    public float smoothFollow = 0.125f; //Smooth parameter of camera
    public Vector3 offset; //Camera position offset
    #endregion

    #region Unity Methods
    private void Start()
    {
        FindPlayer();
    }

    private void FixedUpdate()
    {
        CameraFollow();
    }

    #endregion

    #region Private Methods
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("Player character not found. Please assign the playerTransform in the PlayerCamera");
        }
    }

    //Camera follow method
    void CameraFollow()
    {
        Vector3 desiredPosition = playerTransform.position + offset; //Make offset
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothFollow); //Smooth move
        transform.position = smoothedPosition; //Set position
    }
    #endregion

}
