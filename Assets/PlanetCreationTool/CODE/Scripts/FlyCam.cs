using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour
{
	public float forwardSpeed = 10.0f;
	public float strafeSpeed = 10.0f;
	public float updownSpeed = 10.0f;
	public float rotSpeed = 10.0f;
	public float tiltSpeed = 10.0f;

	private float rot, tilt = 0.0f;
	private float forward, strafe, updown;

	void Start()
	{
		//Screen.SetResolution(1280,720,true);
		rot = transform.rotation.eulerAngles.y; //sets initial rotation and tilt
		tilt = transform.rotation.eulerAngles.x;
	}

	void Update()
	{
#if UNITY_EDITOR
		rot += Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
		tilt -= Input.GetAxis("Mouse Y") * tiltSpeed * Time.deltaTime;
		rot += Input.GetAxis("RstickH") * rotSpeed * Time.deltaTime;
		tilt += Input.GetAxis("RstickV") * tiltSpeed * Time.deltaTime;
        tilt = Mathf.Clamp(tilt, -90, 90);
		transform.eulerAngles = new Vector3(tilt, rot, 0.0f);

		forward = Input.GetAxis("Vertical") * forwardSpeed * Time.deltaTime;
		strafe = Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime;
		updown = Input.GetAxis("UpDown") * updownSpeed * Time.deltaTime;
		transform.Translate(strafe, updown, forward);
#else
#if UNITY_ANDROID
		rot += Input.GetAxis("UpDown") * rotSpeed * Time.deltaTime;
		tilt += Input.GetAxis("RstickH") * tiltSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(tilt, rot, 0.0f);

		forward = Input.GetAxis("Vertical") * forwardSpeed * Time.deltaTime;
		strafe = Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime;
		updown = (Input.GetAxis("RightShoulderShield")-Input.GetAxis("LeftShoulderShield")) * updownSpeed * Time.deltaTime;
		transform.Translate(strafe, updown, forward);
#endif
#if UNITY_STANDALONE
		if (Input.GetAxis("Fire1") == 1)
		{
			//a=a+b <=> a+=b
			rot += Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
			tilt -= Input.GetAxis("Mouse Y") * tiltSpeed * Time.deltaTime;
		}
		rot += Input.GetAxis("RstickH") * rotSpeed * Time.deltaTime;
		tilt += Input.GetAxis("RstickV") * tiltSpeed * Time.deltaTime;
		transform.eulerAngles = new Vector3(tilt, rot, 0.0f);

		forward = Input.GetAxis("Vertical") * forwardSpeed * Time.deltaTime;
		strafe = Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime;
		updown = Input.GetAxis("UpDown") * updownSpeed * Time.deltaTime;
		transform.Translate(strafe, updown, forward);
#endif
#endif
	}
}
