using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCube : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 force;
    public float delta; //amount the square changes in a frame
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        delta = .1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x - delta; 
            this.transform.position = position;

        }


        if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 position = this.transform.position;
            position.x = position.x + delta;
            this.transform.position = position;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 position = this.transform.position;
            position.z = position.z + delta;
            this.transform.position = position;
        }



        if (Input.GetKey(KeyCode.S))
        {
            Vector3 position = this.transform.position;
            position.z = position.z - delta;
            this.transform.position = position;
        }


        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 position = this.transform.position;
            position.y = position.y + delta;
            this.transform.position = position;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 position = this.transform.position;
            position.y = position.y - delta;
            this.transform.position = position;
        }
    }
}
