using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetUi : MonoBehaviour
{
    [SerializeField] GameObject myUiTarget;


    void Update()
    {
        if(SonicController.instace.targetToHomingAttack() != null)
        {
            myUiTarget.SetActive(true);
            transform.position = SonicController.instace.targetToHomingAttack().position;
            transform.rotation = Camera.main.transform.rotation;
        }
        else{
            myUiTarget.SetActive(false);
        }
    }
}
