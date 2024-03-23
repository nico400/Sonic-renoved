using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [Header ("configs Cam")]
    [SerializeField] float Senci;
    [SerializeField] Transform camPos;
    [SerializeField] Transform campivot;
    [SerializeField] Transform playerTarget;
    [SerializeField] Transform Cam;
    [SerializeField] float minAngleTotal;
    [SerializeField] float maxAngleTotal;
    float mouseX;  
    [SerializeField] float mouseY;
    [SerializeField] float distanceCam;
    [SerializeField] LayerMask layerGroundColision;
    Vector3 dirToCam;

     

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * Senci;
        mouseY = Input.GetAxis("Mouse Y");  
    
        //rotate camera
        transform.position = camPos.position;
        Quaternion LerpQuar = Quaternion.FromToRotation(transform.up, camPos.up) * transform.rotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, LerpQuar, 5 * Time.deltaTime);

        transform.RotateAround(transform.position, transform.up, mouseX * Time.deltaTime);

        dirToCam = Cam.position - transform.position;
        float angleToUpPlayer = Vector3.Angle(dirToCam, playerTarget.up);

        campivot.transform.RotateAround(transform.position, transform.right, (mouseY  * Senci) * Time.deltaTime);
        if(angleToUpPlayer <= minAngleTotal)
        {
            campivot.transform.RotateAround(transform.position, transform.right, -Mathf.Abs(mouseY  * Senci) * Time.deltaTime);
        }
        if(angleToUpPlayer >= maxAngleTotal)
        {
            campivot.transform.RotateAround(transform.position, transform.right, Mathf.Abs(mouseY  * Senci) * Time.deltaTime);
        }

        //colision Cam
        RaycastHit hit;
        Vector3 direction = Cam.position - camPos.position;
        if(Physics.Raycast(camPos.position, direction, out hit, direction.magnitude, layerGroundColision))
        {
            float distaceColl = hit.distance;
            Vector3 distance = new Vector3(0,0, -distaceColl);
            Cam.localPosition = Vector3.Lerp(Cam.localPosition, distance, 5 * Time.deltaTime);
        }else
        {
            Vector3 distance = new Vector3(0,0, -distanceCam);
            Cam.localPosition = Vector3.Lerp(Cam.localPosition, distance, 5 * Time.deltaTime);
        }  
    }
}
