using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Numerics;
using System;

public class sphere_agent : Agent
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
    Rigidbody target;
    UnityEngine.Vector3 torque;
    int velocity_scale = 90; // scale of which to apply the velocity input for elbow joint
    int torque_scale = 100;
    UnityEngine.Vector3 shoulder_pos;
    UnityEngine.Vector3 upperarm_pos;
    UnityEngine.Vector3 elbow_pos;
    UnityEngine.Vector3 forearm_pos;
    UnityEngine.Vector3 hand_pos;
    int framecount = 0;

    UnityEngine.Vector3 target_position;

    // Start is called before the first frame update
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
        hand = GameObject.Find("Hand").GetComponent<Rigidbody>();
        elbowmotor = elbowjoint.motor;
        elbowmotor.force = 100;
        elbowmotor.targetVelocity = 90;
        target = GameObject.Find("Target").GetComponent<Rigidbody>();

        var motor = elbowjoint.motor;
        motor.force = 100;
        motor.targetVelocity = 0;
        motor.freeSpin = false;
        elbowjoint.motor = motor;
        elbowjoint.useMotor = true;
        

        //store positions of each component
        shoulder_pos = shoulder.transform.position;
        upperarm_pos = upperarm.transform.position;
        elbow_pos = elbow.transform.position;
        forearm_pos = forearm.transform.position;
        hand_pos = hand.transform.position;

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hand.transform.position - target_position);
        sensor.AddObservation(hand.velocity);
        sensor.AddObservation(elbow.position - target_position);
        sensor.AddObservation(elbow.velocity);
        sensor.AddObservation(shoulder.rotation);
    }

    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.Q)){
            EndEpisode();
        }

        actionsOut[0] = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.O))
        {
            actionsOut[1] = -1;
        }
        if (Input.GetKey(KeyCode.K))
        {
            actionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.P))
        {
            actionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.L))
        {
            actionsOut[2] = 1;
        }

        actionsOut[3] = Input.GetAxis("Vertical");

    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //apply goal velocity to elbow
        var motor = elbowjoint.motor;
        motor.force = 200;
        motor.targetVelocity = vectorAction[0]*velocity_scale;
        motor.freeSpin = false;
        elbowjoint.motor = motor;
        elbowjoint.useMotor = true;

        torque = new UnityEngine.Vector3(vectorAction[1] * 20* torque_scale, vectorAction[2] * torque_scale, vectorAction[3] * torque_scale);

        //apply rotational torque vector to the thing
        shoulder.AddTorque(torque);

        UnityEngine.Vector3 distance =  hand.transform.position - target_position;

        if (distance.magnitude < .7 || framecount>100000) //reset if target is reached
        {
            EndEpisode(); 
        }

        Debug.Log(distance.magnitude);
        // Reward is the negative difference between hand position and goal position
        SetReward(-distance.magnitude);
        framecount++;

    }

    public override void OnEpisodeBegin()
    {
        //generate some random position from random rotations
        target_position = new UnityEngine.Vector3(
            UnityEngine.Random.Range((float)0.0, (float)4),
            UnityEngine.Random.Range((float)0.0, (float)4) + 5,
            UnityEngine.Random.Range((float)0.0, (float)4)
            );
        target.position = target_position;

        shoulder.transform.position = shoulder_pos;
        shoulder.velocity = UnityEngine.Vector3.zero;

        upperarm.transform.position = upperarm_pos;
        upperarm.velocity = UnityEngine.Vector3.zero;
        elbow.transform.position = elbow_pos;
        elbow.velocity = UnityEngine.Vector3.zero;
        forearm.transform.position = forearm_pos;
        forearm.velocity = UnityEngine.Vector3.zero;
        hand.transform.position = hand_pos;
        hand.velocity = UnityEngine.Vector3.zero;
        framecount = 0;
    }






}
