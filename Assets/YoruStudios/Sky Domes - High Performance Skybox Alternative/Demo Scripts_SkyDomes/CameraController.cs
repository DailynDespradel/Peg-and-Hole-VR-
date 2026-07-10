using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float panSpeed = 20f;        // Speed of panning
	public float panAcceleration = 2f;  // Acceleration factor for panning
	public float turnSpeed = 4f;        // Speed of camera rotation
	public float verticalSpeed = 20f;   // Speed of vertical movement
	private float horizontalInput;
	private float verticalInput;
	private float verticalMovement;
	private float mouseX;
	private float mouseY;

	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private bool isReturning = false;   // Flag indicating whether the camera is returning to the initial position
	public float returnSpeed = 2f;      // Speed of smooth return

	void Start()
	{
		// Save the initial position and rotation of the camera
		initialPosition = transform.position;
		initialRotation = transform.rotation;
	}

	void Update()
	{
		if (isReturning)
		{
			ReturnToInitialPositionSmoothly();
		}
		else
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		verticalInput = Input.GetAxis("Vertical");
		mouseX = Input.GetAxis("Mouse X");
		mouseY = Input.GetAxis("Mouse Y");

		if (Input.GetMouseButton(1))
		{
			RotateCamera();
			MoveCamera();

			if (Input.GetKey(KeyCode.Q))
			{
				LowerCamera();
			}
			if (Input.GetKey(KeyCode.E))
			{
				RaiseCamera();
			}
		}

		// Add functionality for smooth return when the "R" key is pressed
		if (Input.GetKeyDown(KeyCode.R))
		{
			StartCoroutine(ReturnToInitialPositionSmoothly());
		}
	}

	void MoveCamera()
	{
		float speed = Input.GetKey(KeyCode.LeftShift) ? panSpeed * panAcceleration : panSpeed;
		Vector3 move = new Vector3(horizontalInput * speed * Time.deltaTime, 0, verticalInput * speed * Time.deltaTime);
		move = transform.TransformDirection(move);
		transform.position += move;
	}

	void RotateCamera()
	{
		transform.rotation *= Quaternion.Euler(-mouseY * turnSpeed, mouseX * turnSpeed, 0);
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
	}

	void LowerCamera()
	{
		transform.position -= new Vector3(0, verticalSpeed * Time.deltaTime, 0);
	}

	void RaiseCamera()
	{
		transform.position += new Vector3(0, verticalSpeed * Time.deltaTime, 0);
	}

	IEnumerator ReturnToInitialPositionSmoothly()
	{
		isReturning = true;

		while (Vector3.Distance(transform.position, initialPosition) > 0.01f || Quaternion.Angle(transform.rotation, initialRotation) > 0.01f)
		{
			transform.position = Vector3.Lerp(transform.position, initialPosition, returnSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, returnSpeed * Time.deltaTime);

			yield return null;
		}

		isReturning = false;
	}
}
