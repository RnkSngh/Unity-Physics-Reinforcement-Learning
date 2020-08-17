using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Move_shoulder : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody shoulder;
    CharacterJoint shoulderjoint;
    Rigidbody upperarm;
    Rigidbody elbow;
    JointMotor elbowmotor; 
    Rigidbody forearm;
    Rigidbody hand;
    HingeJoint elbowjoint;
    Vector3 torque; 
    
    void Start()
    {

        //Apply constraints
        shoulder = GameObject.Find("Shoulder").GetComponent<Rigidbody>();
        //shoulderjoint = shoulder.GetComponent<CharacterJoint>();
        upperarm = GameObject.Find("UpperArm").GetComponent<Rigidbody>();
        forearm = GameObject.Find("Forearm").GetComponent<Rigidbody>();
        shoulder.constraints = RigidbodyConstraints.FreezePosition;
        elbow = GameObject.Find("Elbow").GetComponent<Rigidbody>();
        elbowjoint = elbow.GetComponent<HingeJoint>();
        elbowmotor = elbowjoint.motor;
        elbowmotor.force = 100;
        elbowmotor.targetVelocity = 90;

        // shoulder.rotation

        var motor = elbowjoint.motor;
        motor.force = 100;
        motor.targetVelocity = 0;
        motor.freeSpin = false;
        elbowjoint.motor = motor;
        elbowjoint.useMotor = true;
        torque = new Vector3(0,0,100000);

        //shoulder.AddTorque(torque);
    }

    int count = 0;

    // Update is called once per frame
    void Update()
    {

        if(count%100 == 0)
        {
            var motor = elbowjoint.motor;
            if (motor.targetVelocity == 90)
            {
                motor.targetVelocity = -90;
            }
            else
            {
                motor.targetVelocity = 0;
            }
            elbowjoint.motor = motor;
            elbowjoint.useMotor = true;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            upperarm.AddTorque(-torque);
            
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            upperarm.AddTorque(torque);

        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            forearm.AddTorque(-torque);

        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            forearm.AddTorque(torque);

        }


    }
}
