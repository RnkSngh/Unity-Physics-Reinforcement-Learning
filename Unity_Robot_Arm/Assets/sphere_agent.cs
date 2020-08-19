using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Numerics;
using System;

/// <summary> The sphere_agent class contains methods to interface the robot arm agent with the Unity Ml-agents module. This class contains methods to reset episodes, 
/// set rewards for each state, collect agent observations, and actuate agent actions. 
/// </summary>
public class sphere_agent : Agent
{

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
    int velocity_scale = 90; // scale of which to apply a target_velocity for the hinge joint
    int torque_scale = 100; // scale for which to apply torques to the joints
    UnityEngine.Vector3 shoulder_pos;
    UnityEngine.Vector3 upperarm_pos;
    UnityEngine.Vector3 elbow_pos;
    UnityEngine.Vector3 forearm_pos;
    UnityEngine.Vector3 hand_pos;
    int framecount = 0; //used to keep track of if an episode should be reset
    UnityEngine.Vector3 target_position;

    /// <summary> The start method adds pointers to existing rigidbody objects and joints in the scene. Additionally, the start positions of all rigid bodies are taken,
    /// which will be used to reset objects at the beginning of each episode
    /// </summary>
    void Start()
    {
        //Apply constraints to shoulder
        shoulder = GameObject.Find("Shoulder").GetComponent<Rigidbody>();
        
        //shoulderjoint = shoulder.GetComponent<CharacterJoint>();
        upperarm = GameObject.Find("UpperArm").GetComponent<Rigidbody>();
        forearm = GameObject.Find("Forearm").GetComponent<Rigidbody>();
        shoulder.constraints = RigidbodyConstraints.FreezePosition;
        elbow = GameObject.Find("Elbow").GetComponent<Rigidbody>();
        elbowjoint = elbow.GetComponent<HingeJoint>();
        hand = GameObject.Find("Hand").GetComponent<Rigidbody>();
        
        //store current eblow motor
        elbowmotor = elbowjoint.motor;
        elbowmotor.force = 100;
        elbowmotor.targetVelocity = 90;
        target = GameObject.Find("Target").GetComponent<Rigidbody>();

        // create a motor object for moving the elbow joint
        var motor = elbowjoint.motor;
        motor.force = 100;
        motor.targetVelocity = 0;
        motor.freeSpin = false;
        elbowjoint.motor = motor;
        elbowjoint.useMotor = true;
        

        //store positions of each component for when episode resets
        shoulder_pos = shoulder.transform.position;
        upperarm_pos = upperarm.transform.position;
        elbow_pos = elbow.transform.position;
        forearm_pos = forearm.transform.position;
        hand_pos = hand.transform.position;
    }

    /// <summary> Collect observations modifies a given VectorSensor object to reflect the agent's observations at the current state. The first 12 observations reflect
    /// the relative positions and velocities of the hand and elbow. The last 4 observations represent the quaternion of the shoulder's orientation.
    /// </summary>
    /// <param name="sensor">The VectorSensor object to be modified</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(hand.transform.position - target_position); //3D vector 
        sensor.AddObservation(hand.velocity); //3D vector 
        sensor.AddObservation(elbow.position - target_position); //3D vector 
        sensor.AddObservation(elbow.velocity); //3D vector 
        sensor.AddObservation(shoulder.rotation); // 4D quaternion
    }

    /// <summary> Heuristic is used to test the physics environment before training the agent. The torques applied to the joints can be changed by pressing the arrow keys
    /// and the O, K, P, L keys. The episode can be reset with the Q key. 
    /// </summary>
    /// <param name="actionsOut">The vector object to be passed to the OnActionReceived method</param>
    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.Q)){
            EndEpisode();
        }

        actionsOut[0] = Input.GetAxis("Horizontal");

        //apply torques if keys are pressed by changing the actionsOut vector
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

    /// <summary> OnActionreceived actuates a given 4d vector, which represents the torques to be applied to the joints. This method also handles calling the 
    /// EndEpisode() function, which is called any time the hand reaches the target position, or if the episode lasts for more than 100000 timesteps.
    /// </summary>
    /// <param name="vectorAction"> A float array of length 4 which represents the agent's output to be actuated. The first index is the torque to be applied
    /// to the elbow joint, and the last 3 indicies represent the torques to be applied about the shoulder's 3 axes. </param>
    public override void OnActionReceived(float[] vectorAction)
    {
        //apply goal velocity to elbow
        var motor = elbowjoint.motor;
        motor.force = 200;
        motor.targetVelocity = vectorAction[0]*velocity_scale;
        motor.freeSpin = false;
        elbowjoint.motor = motor;
        elbowjoint.useMotor = true;

        //scale torque
        torque = new UnityEngine.Vector3(vectorAction[1] * 20* torque_scale, vectorAction[2] * torque_scale, vectorAction[3] * torque_scale);

        //apply rotational torque to shoulder
        shoulder.AddTorque(torque);
        
        //calculate distance to check if hand is within a given tolerance of the goal position
        UnityEngine.Vector3 distance =  hand.transform.position - target_position;
        if (distance.magnitude < .7 || framecount>100000) //reset if target is reached
        {
            EndEpisode(); 
        }

        // Reward is the negative difference between hand position and goal position
        SetReward(-distance.magnitude);
        framecount++;

    }

    /// <summary> OnEpisodeBegin randomly generates a new target position to move the hand toward, and resets the positions of the arm's joints. 
    /// </summary>
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
