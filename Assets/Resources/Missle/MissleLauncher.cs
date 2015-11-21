using UnityEngine;
using System.Collections;

public class MissleLauncher : MonoBehaviour
{
    public Rigidbody Missle;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetButtonDown("Fire1"))
	    {
	        Instantiate(Missle, transform.position, transform.rotation);
	    }	
	}
}
