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

public class AgentMove : Agent
{
    public UnityEngine.Vector3 CurrentPosition;
    public Rigidbody rb;
    public UnityEngine.Vector3 force;
    public float delta; //amount the square changes in a frame
    public GameObject rod;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("started");
        rb = this.GetComponent<Rigidbody>();
        delta = 100f;
        rod = GameObject.Find("Cylinder");
        Debug.Log("started");
    }


    public override void OnEpisodeBegin()
    {
        this.transform.position = UnityEngine.Vector3.zero;
        //CurrentPosition = this.transform.position;
        ////move to origin if you are out of bounds
        //if (CurrentPosition.x > 10 || CurrentPosition.x<-10)
        //{
        //    CurrentPosition = UnityEngine.Vector3.zero;
        //}

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rod.transform.position);
        sensor.AddObservation(this.transform.position);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        UnityEngine.Vector3 force = UnityEngine.Vector3.zero;


        if (vectorAction[0] > 0)
        {
            force.x = delta;
        }

        else
        {
            force.x = -delta;
        }

        rb.AddForce(force);

        SetReward(rod.transform.position.y);

        if (this.transform.position.x > 10 || this.transform.position.x < -10)
        {
            EndEpisode();
        }

    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
    }
    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.LeftArrow))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.x = position.x - delta;
    //        this.transform.position = position;

    //    }


    //    if (Input.GetKey(KeyCode.RightArrow))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.x = position.x + delta;
    //        this.transform.position = position;
    //    }

    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.z = position.z + delta;
    //        this.transform.position = position;
    //    }



    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.z = position.z - delta;
    //        this.transform.position = position;
    //    }


    //    if (Input.GetKey(KeyCode.UpArrow))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.y = position.y + delta;
    //        this.transform.position = position;
    //    }

    //    if (Input.GetKey(KeyCode.DownArrow))
    //    {
    //        Vector3 position = this.transform.position;
    //        position.y = position.y - delta;
    //        this.transform.position = position;
    //    }
    //}
}
