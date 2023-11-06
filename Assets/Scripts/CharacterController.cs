using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float playerSpeed;
    public float sprintSpeed = 4f;
    public float walkSpeed = 2f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 3f;
    private bool isMoving = false;
    private bool isSprinting = false;
    private float yRot;

    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = walkSpeed;
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        yRot += Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);

        isMoving = false;

        if (Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f)
        {
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * playerSpeed);
            //rigidBody.velocity += transform.right * Input.GetAxisRaw("Horizontal") * playerSpeed;
            isMoving = true;
        }
        if (Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Vertical") < -0.5f)
        {
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * playerSpeed);
            //rigidBody.velocity += transform.forward * Input.GetAxisRaw("Vertical") * playerSpeed;
            isMoving = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.Translate(Vector3.up * jumpHeight);
        }

        if (Input.GetAxisRaw("Sprint") > 0f)
        {
            playerSpeed = sprintSpeed;
            isSprinting = true;
        }
        else if (Input.GetAxisRaw("Sprint") < 1f)
        {
            playerSpeed = walkSpeed;
            isSprinting = false;
        }

        //anim.SetBool("isMoving", isMoving);
        //anim.SetBool("isSprinting", isSprinting);

    }
    //public float moveSpeed = 5f; // Adjust the speed as needed
    //public float mouseSensitivity = 2f;
    //private float yRot;

    //void Update()
    //{

    //    yRot += Input.GetAxis("Mouse X") * mouseSensitivity;
    //    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);

    //    // Get input from the keyboard
    //    float horizontalInput = Input.GetAxis("Horizontal");
    //    float verticalInput = Input.GetAxis("Vertical");

    //    // Calculate movement direction
    //    Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

    //    // Move the character
    //    MoveCharacter(movement);
    //}

    //void MoveCharacter(Vector3 direction)
    //{
    //    // Calculate the movement amount
    //    Vector3 movementAmount = direction * moveSpeed * Time.deltaTime;

    //    // Apply the movement to the character
    //    transform.Translate(movementAmount, Space.World);
    //}

}
