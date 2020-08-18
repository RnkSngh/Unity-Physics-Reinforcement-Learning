using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Numerics;
using UnityEngine.Experimental.XR;
using UnityEditor;
using System;
using Unity.MLAgents.Policies;

public class Agent_move : Agent
{
    public UnityEngine.Vector3 CurrentPosition;
    public Rigidbody rb;
    public Rigidbody weight;
    public UnityEngine.Vector3 force;
    public float delta; //amount the square changes in a frame
    public GameObject rod;
    public GameObject ball;
    public UnityEngine.Vector3 ball_start;
    public UnityEngine.Vector3 cube_start;
    public UnityEngine.Vector3 rod_start;
    // Start is called before the first frame update


    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        delta = 25f;
        rod = GameObject.Find("Cylinder");
        ball = GameObject.Find("Sphere");
        weight = ball.GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation; 
       // weight.constraints = RigidbodyConstraints.FreezePositionZ;
        ball_start = ball.transform.position;
        rod_start = rod.transform.position;
        cube_start = this.transform.position;

    }


    public override void OnEpisodeBegin()
    {
        rb.isKinematic = true;
        rod.GetComponent<Rigidbody>().isKinematic = true;
        weight.isKinematic = true;

        rod.GetComponent<CharacterJoint>().connectedBody = null;
        ball.GetComponent<FixedJoint>().connectedBody = null;

        weight.velocity = UnityEngine.Vector3.zero;
        weight.angularVelocity = UnityEngine.Vector3.zero;
        rod.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        rod.GetComponent<Rigidbody>().angularVelocity = UnityEngine.Vector3.zero;
        rb.velocity = UnityEngine.Vector3.zero;
        rb.angularVelocity = UnityEngine.Vector3.zero;



        ball.transform.position = ball_start;
        ball.transform.localRotation = UnityEngine.Quaternion.identity;
        rod.transform.position = rod_start;
        
        rod.transform.localRotation = UnityEngine.Quaternion.identity;
        this.transform.position = cube_start;



        rod.GetComponent<CharacterJoint>().connectedAnchor = new UnityEngine.Vector3(0, .4f, 0);
        rod.GetComponent<CharacterJoint>().anchor = new UnityEngine.Vector3(0, -1, 0);
        rod.GetComponent<CharacterJoint>().connectedBody = rb;        
        rod.GetComponent<CharacterJoint>().axis = new UnityEngine.Vector3(1, 0, 0);
        //ball.GetComponent<FixedJoint>().connectedBody = rod.GetComponent<Rigidbody>();
        ball.GetComponent<FixedJoint>().connectedBody = rod.GetComponent<Rigidbody>();


        //MyDelay(1);
        rb.isKinematic = false;
        rod.GetComponent<Rigidbody>().isKinematic = false;
        weight.isKinematic = false;

        weight.velocity = UnityEngine.Vector3.left;


    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(weight.transform.position.x - this.transform.position.x);
        sensor.AddObservation(weight.transform.position.y );
        sensor.AddObservation(weight.transform.position.z - this.transform.position.z);
        sensor.AddObservation(weight.velocity.x - rb.velocity.x);
        sensor.AddObservation(weight.velocity.y);
        sensor.AddObservation(weight.velocity.z - rb.velocity.z);
        sensor.AddObservation(this.transform.position.x);
        sensor.AddObservation(this.transform.position.z);
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

    }

    public override void OnActionReceived(float[] vectorAction)
    {
        UnityEngine.Vector3 force = UnityEngine.Vector3.zero;
        Debug.Log(vectorAction[0]);
        if (vectorAction[0] > 0)
        {

            force.x = delta;
        }
        else if (vectorAction[0] == 0)
        {
            force.x = 0;
        }
        else //vector action is negative 
        {
            force.x = -delta;
        }

        if (vectorAction[1] > 0)
        {
            force.z = delta;
        }
        else if (vectorAction[1] == 0)
        {
            force.z = 0;
        }
        else //vector action is negative 
        {
            force.z = -delta;
        }


        rb.AddForce(force);

        SetReward(ball.transform.position.y);

        if (this.transform.position.x > 10 || this.transform.position.x < -10 || this.transform.position.z < -10 || this.transform.position.z > 10)
        {
            EndEpisode();
        }

    }

    public override void Heuristic(float[] actionsOut)
    {
        // actionsOut[0] = Input.GetAxis("Horizontal");
        float input_x = Input.GetAxis("Horizontal");
        float input_z = Input.GetAxis("Vertical");
        
       // Debug.Log(input_act);
        actionsOut[0] = input_x;
        actionsOut[1] = input_z;

    }

}
