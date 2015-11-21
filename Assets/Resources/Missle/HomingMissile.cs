using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HomingMissile : Projectile
{
    public float MissileVelocity = 50;
    public float Turn = 5;
    public Rigidbody HomingMiissle;
    public GameObject MissileMod;
    public ParticleSystem SmokePrefab;
    public AudioClip MissileClip;

    private Transform _target;

	/*// Use this for initialization
	void Start ()
	{
	    Fire();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (_target == null || HomingMiissle == null)
        {
            return;
        }

        HomingMiissle.velocity = transform.forward * MissileVelocity;

        var targetRotation = Quaternion.LookRotation(_target.position - transform.position);

        HomingMiissle.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, Turn));
	}

    void FixedUpdate()
    {
        
    }

    private void Fire()
    {
        //AudioSource.PlayClipAtPoint(MissileClip, transform.position);
        var distance = Mathf.Infinity;

        foreach(var go in GameObject.FindGameObjectsWithTag("target"))
        {
            var diff = (go.transform.position - transform.position).sqrMagnitude;

            if (diff < distance)
            {
                distance = diff;
                _target = go.transform;
            }
        }
    }

    void OnCollisionEnter(Collision theCollision)
    {
        if (theCollision.gameObject.name == "Cube")
        {
            SmokePrefab.emissionRate = 0.0f;
            Destroy(MissileMod.gameObject);
            //yield return new WaitForSeconds(3);
            Destroy(gameObject);
        }
    }*/
    
}
