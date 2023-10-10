using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]      
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    [SerializeField]
    private Rigidbody rb;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;    
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f) * moveSpeed;
    }
}
