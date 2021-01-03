using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public float movespeed = 7;
    public float smoothMoveTime = .1f;
    public float turnspeed = 8;
    float smoothInputMagnitude, smoothMoveVelocity, angle;
    Vector3 velocity;
    bool disabled;
    Rigidbody rigidbody;
    public event System.Action OnReachedEndOfLevel;
    void Start()
    {
        Guard.OnGuardHasSpottedPlayer += Disable;
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDirection = Vector3.zero;
        if(!disabled) {
        inputDirection = new Vector3 (Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;
        }
        float inputMagnitude = inputDirection.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude,inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z)*Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime*turnspeed * inputMagnitude);
        velocity = transform.forward * movespeed * smoothInputMagnitude;
        // transform.eulerAngles = Vector3.up * angle;
        // transform.Translate(transform.forward*movespeed*Time.deltaTime * smoothInputMagnitude,Space.World);
    }

    void Disable () {
        disabled = true;
    }

    void OnTriggerEnter(Collider hitCollider) {
        if(hitCollider.tag == "Finish") {
            if(OnReachedEndOfLevel != null) {
                OnReachedEndOfLevel ();
            }
        }
    }

    void OnDestroy() {
        Guard.OnGuardHasSpottedPlayer -= Disable;
    }
// pour le rigidbody
    void FixedUpdate() {
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidbody.MovePosition (rigidbody.position + velocity * Time.deltaTime);
    }
}
