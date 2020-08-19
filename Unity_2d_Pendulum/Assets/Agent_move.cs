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

/// <summary> The Agent_move class contains methods to interface with the Unity Ml-agents module. This class contains methods to reset episodes, set a reward
/// function, collect agent observations, and actuate agent actions. 
/// </summary>
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

    /// <summary> Generates the cube, rod, and weight, and constrains the motion of the cube to a 2d plane
    /// </summary>
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        delta = 25f; //set force to 25 newtons
        rod = GameObject.Find("Cylinder");
        ball = GameObject.Find("Sphere");
        weight = ball.GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;  //cube can only move in the xz plane
        //store initial ball, rod, and cube positions to use when resetting the episode
        ball_start = ball.transform.position;
        rod_start = rod.transform.position;
        cube_start = this.transform.position;

    }

    /// <summary> OnEpisodeBegin removes the joints connecting the ball, rod, and cube, and moves them back to their starting positions and reapplies the joints.
    /// The weight is given an initial velocity at the start of the episode to avoid being a stationary system.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //set to Kinematic to avoid wacky forces due to instantaneous movement
        rb.isKinematic = true;
        rod.GetComponent<Rigidbody>().isKinematic = true;
        weight.isKinematic = true;
        
        //remove joints before resetting positions
        rod.GetComponent<CharacterJoint>().connectedBody = null;
        ball.GetComponent<FixedJoint>().connectedBody = null;
        
        //reset angular and translational velocities
        weight.velocity = UnityEngine.Vector3.zero;
        weight.angularVelocity = UnityEngine.Vector3.zero;
        rod.GetComponent<Rigidbody>().velocity = UnityEngine.Vector3.zero;
        rod.GetComponent<Rigidbody>().angularVelocity = UnityEngine.Vector3.zero;
        rb.velocity = UnityEngine.Vector3.zero;
        rb.angularVelocity = UnityEngine.Vector3.zero;

        //reset ball, rod, and cube back to their start positions
        ball.transform.position = ball_start;
        ball.transform.localRotation = UnityEngine.Quaternion.identity;
        rod.transform.position = rod_start;
        rod.transform.localRotation = UnityEngine.Quaternion.identity;
        this.transform.position = cube_start;


        //re-attach joints after moving rigid bodies back to starting locations
        rod.GetComponent<CharacterJoint>().connectedAnchor = new UnityEngine.Vector3(0, .4f, 0);
        rod.GetComponent<CharacterJoint>().anchor = new UnityEngine.Vector3(0, -1, 0);
        rod.GetComponent<CharacterJoint>().connectedBody = rb;        
        rod.GetComponent<CharacterJoint>().axis = new UnityEngine.Vector3(1, 0, 0);
        ball.GetComponent<FixedJoint>().connectedBody = rod.GetComponent<Rigidbody>();


        //convert .isKinematic to false to allow for forces to act on the objects
        rb.isKinematic = false;
        rod.GetComponent<Rigidbody>().isKinematic = false;
        weight.isKinematic = false;

        //add a starting velocity for the weight to avoid a stationary system
        weight.velocity = UnityEngine.Vector3.left;
    }

    /// <summary> Collect observations modifies a given VectorSensor object to reflect the agent's observations at the current state. The first 6 observations reflect
    /// the relative positions and velocities of the weight to the cube. The last 4 observations represent the x and z positions and velocities of the cube.
    /// </summary>
    /// <param name="sensor">The VectorSensor object to be modified</param>
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
    
    /// <summary> OnActionreceived actuates a given 2d vector, which represents the force to be applied to the cube in the x and z directions. This method also
    /// handles calling the EndEpisode() function, which is called any time the ball's y coordinate dips below the cube's y coordinate, or if the cube moves 
    /// outside of the 10x10 square in the xz plane
    /// </summary>
    /// <param name="vectorAction"> A float array of length 2 which represents the agent's output to be actuated. The first index is the force to be applied
    /// to the cube in the x direction, and the second index is the force to be applied to the cube in the z direction</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        UnityEngine.Vector3 force = UnityEngine.Vector3.zero;
        Debug.Log(vectorAction[0]);
        
        //define force vector
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

        //apply the force
        rb.AddForce(force);
        
        //add reward for state
        SetReward(ball.transform.position.y);

        //reset episode if restart conditions are met
        if (this.transform.position.x > 10 || this.transform.position.x < -10 || this.transform.position.z < -10 || this.transform.position.z > 10 || ball.transform.position.y<0 )
        {
            EndEpisode(); //calls OnEpisodeBegin();
        }

    }

    /// <summary> Heuristic is used to test the physics environment before training the agent. The forces applied to the cube can be changed by pressing the arrow keys. 
    /// </summary>
    /// <param name="actionsOut">The vector object to be passed to the OnActionReceived method</param>
    public override void Heuristic(float[] actionsOut)
    {
        float input_x = Input.GetAxis("Horizontal"); //up and down arrows control the force applied in the x direction
        float input_z = Input.GetAxis("Vertical"); // left and right arrows control the force applied in the y direction

        actionsOut[0] = input_x;
        actionsOut[1] = input_z;
    }

}
