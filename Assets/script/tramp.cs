using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class tramp : MonoBehaviour
{ 
    [SerializeField] float force;
    [SerializeField] bool isTranpResetSpeed;
    [SerializeField] LayerMask SonicLayer;
    [SerializeField] Vector3 PositionCorrect;
    
    void Update()
    {
        if(collSonic())
        {
            SonicController.instace.getRigBodySonic().velocity = Vector3.zero;
            SonicController.instace.getRigBodySonic().AddForce(transform.up * force, ForceMode.Impulse);
                    
            SonicController.instace.SpeedToZeroInTramplin(isTranpResetSpeed);
        }
    }
    public bool collSonic()
    {
        Vector3 posRay = transform.position + PositionCorrect;
        Collider[] coll = Physics.OverlapSphere(posRay, 0.5f, SonicLayer);
        if (coll.Length > 0)
        {
            return true;
        }
        return false;
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Vector3 Pos = transform.position + PositionCorrect;
        Gizmos.DrawSphere(Pos, 0.5f);
    }
}
