using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
		
	private Rigidbody rigidb;

	public enum MotorLocal { Front, Back };
	public enum MotorType { MotorBasic, MotorEfficiency };
	public enum GearMode { FiveGears, SixGears };
	public enum TorqueType { FWD, RWD, AWD };

	[Header ("Wheels")]
	public Vector3 tireInfo;
	public WheelCollider[] wheels;

	[Header ("Engine")]
	public MotorLocal motorLocal;
	public MotorType motorType;
	public GearMode gearMode;
	public TorqueType torqueType;
	public float maxKMH;
	public float maxRPM;
	public float torqueNm;
	public float torqueRPM;
	public float zeroTo100;
	[Range (0.1f, 1f)]
	public float efficiency;

	[Header ("Drag")]
	public bool useDrag;
	[Range (0.01f, 1f)]
	public float cx;
	[Tooltip ("m2")]
	public float frontArea;

	[Header ("Brake")]
	public float motorBrake = 2;
	public float brakeTorque = 5000;

	[Header ("Gears")]
	public float finalDrive;
	public float[] gears;

	public static float[] minSpeeds;
	public static float[] maxSpeeds;

	public static int kmH;
	public static float rpm;
	public static float drag;

	public static float torque;
	public static float motor;
	public static float currentTorque;
	public static float currentRpm = 1;
	public static float wheelDiameter;
	public static int currentGear;

	public static bool isGrounded = true;
	public static bool isDrifting = false;
	public static bool isBricked = false;
	public static bool isBraking = false;


	void Awake () {

		drag = 0.5f * cx * frontArea * 1.2f;
		rigidb = GetComponent<Rigidbody> ();
		rigidb.mass -= (wheels [0].mass * 4);

		//SPEEDS FOR GEARS

		minSpeeds = new float[gears.Length];
		maxSpeeds = new float[gears.Length];

		//WHEEL DIAMETER

		wheelDiameter = (((tireInfo.z * 25.4f) + (((tireInfo.x * tireInfo.y) / 100) * 2)) * 3.14f);

		Debug.Log (wheelDiameter);

		//SPEED LISTS

		for (int i = 1; i < gears.Length; i++) {

			maxSpeeds [i] = Mathf.RoundToInt ((((((maxRPM / gears [i] / finalDrive) * wheelDiameter) / 1000000) * 60) * 100) / 100);
			minSpeeds [i] = maxSpeeds [i] / 2;

			Debug.Log (i);
			Debug.Log ("MAX = " + maxSpeeds [i]);
		}

		//CENTER OF MASS

		switch (motorLocal) {

		case MotorLocal.Front:
			rigidb.centerOfMass = new Vector3 (0, 0, 0.5f);
			break;

		case MotorLocal.Back:
			rigidb.centerOfMass = new Vector3 (0, 0, -0.5f);
			break;
		
		}

		//NUMBER OF GEARS

		switch (gearMode) {

		case GearMode.FiveGears:
			StartCoroutine ("Gear5");
			break;

		case GearMode.SixGears:
			StartCoroutine ("Gear6");
			break;
		}

		//TYPE OF MOTORS

		switch (motorType) {

		case MotorType.MotorBasic:
			StartCoroutine ("MotorBasic");
			break;

		case MotorType.MotorEfficiency:
			StartCoroutine ("MotorEfficiency");
			break;
		}
			
		//TRACTION MODE

		switch (torqueType) {

		case TorqueType.FWD:
			StartCoroutine ("ApplyTorqueFWD");
			break;

		case TorqueType.RWD:
			StartCoroutine ("ApplyTorqueRWD");
			break;

		case TorqueType.AWD:
			StartCoroutine ("ApplyTorqueAWD");
			break;
		}

		//USE DRAG?

		switch (useDrag) {

		case true:
			StartCoroutine ("ApplyDrag");
			break;
		}
	}


	void FixedUpdate () {

		GetInput ();
		CheckSpeed ();
	}

	void GetInput () {

		if (isGrounded) {
				
			if (Input.GetKey (KeyCode.Space)) {

				isBraking = true;

				wheels [0].brakeTorque += motorBrake;
				wheels [1].brakeTorque += motorBrake;
				wheels [2].brakeTorque += brakeTorque;
				wheels [3].brakeTorque += brakeTorque;

			} else if (Input.GetKey (KeyCode.B)) {

				isBraking = true;

				wheels [0].brakeTorque += motorBrake;
				wheels [1].brakeTorque += motorBrake;
				wheels [2].brakeTorque += motorBrake;
				wheels [3].brakeTorque += motorBrake;
		
			} else {

				isBraking = false;

				wheels [0].brakeTorque = 0;
				wheels [1].brakeTorque = 0;
				wheels [2].brakeTorque = 0;
				wheels [3].brakeTorque = 0;
			}
		}
	}

	void CheckSpeed () {

		kmH = (int)(rigidb.velocity.magnitude * 3.6);
	}

	IEnumerator MotorBasic () {

		while (true) {

			//if (rpm > torqueRPM && kmH > maxKMH) {

				//maxTorque = Mathf.Lerp (maxTorque, 0, 0.001f);
			
			//} else { maxTorque = Mathf.Lerp (maxTorque, torqueNm, 0.01f); }

			rpm = currentRpm * gears [currentGear] * finalDrive;
			rpm = Mathf.Clamp (rpm, 500, maxRPM);

			torque = currentTorque * gears [currentGear] * finalDrive;
			if (kmH < maxKMH) motor = Input.GetAxis ("Vertical") * torque;

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator MotorEfficiency () {

		while (true) {

			//if (rpm > torqueRPM && kmH > maxKMH) {

			//maxTorque = Mathf.Lerp (maxTorque, 0, 0.001f);

			//} else { maxTorque = Mathf.Lerp (maxTorque, torqueNm, 0.01f); }

			rpm = currentRpm * gears [currentGear] * finalDrive;
			rpm = Mathf.Clamp (rpm, 500, maxRPM);

			currentTorque = torqueNm * gears [currentGear] * finalDrive * efficiency;
			torque = Mathf.Lerp (torque, currentTorque, 0.01f);
			motor = Input.GetAxis ("Vertical") * torque;

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator Gear5 () {

		while (true) {

			if (isGrounded) {

				if (kmH < maxSpeeds [1]) {

					currentGear = 1;

				} else if (kmH >= maxSpeeds [1] && kmH < maxSpeeds [2]) {

					currentGear = 2;

				} else if (kmH >= maxSpeeds [2] && kmH < maxSpeeds [3]) {

					currentGear = 3;

				} else if (kmH >= maxSpeeds [3] && kmH < maxSpeeds [4]) {

					currentGear = 4;

				} else if (kmH >= maxSpeeds [4]) {

					currentGear = 5;
				
				} else {
				
					currentGear = 0;
				}
			}

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator Gear6 () {

		while (true) {

			if (isGrounded) {

				if (kmH < maxSpeeds [1]) {

					currentGear = 1;

				} else if (kmH >= maxSpeeds [1] && kmH < maxSpeeds [2]) {

					currentGear = 2;

				} else if (kmH >= maxSpeeds [2] && kmH < maxSpeeds [3]) {

					currentGear = 3;

				} else if (kmH >= maxSpeeds [3] && kmH < maxSpeeds [4]) {

					currentGear = 4;

				} else if (kmH >= maxSpeeds [4] && kmH < maxSpeeds [5]) {

					currentGear = 5;

				} else if (kmH >= maxSpeeds [5]) {

					currentGear = 6;
				}
			}

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator ApplyTorqueFWD () {

		while (true) {

			if (wheels [0].isGrounded || wheels [1].isGrounded) {

				isGrounded = true;

				currentRpm = (wheels [0].rpm + wheels [1].rpm) / 2;

				wheels [0].motorTorque = motor;
				wheels [1].motorTorque = motor;
	
			} else { isGrounded = false; }

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator ApplyTorqueRWD () {

		while (true) {

			if (wheels [2].isGrounded || wheels [3].isGrounded) {

				isGrounded = true;

				currentRpm = (wheels [2].rpm + wheels [3].rpm) / 2;

				wheels [2].motorTorque = motor;
				wheels [3].motorTorque = motor;


			} else { isGrounded = false; }

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator ApplyTorqueAWD () {

		while (true) {

			if (wheels [0].isGrounded || wheels [1].isGrounded || wheels [2].isGrounded || wheels [3].isGrounded) {

				isGrounded = true;

				currentRpm = (wheels [0].rpm + wheels [1].rpm + wheels [2].rpm + wheels [3].rpm) / 4;

				wheels [0].motorTorque = motor / 2;
				wheels [1].motorTorque = motor / 2;
				wheels [2].motorTorque = motor / 2;
				wheels [3].motorTorque = motor / 2;

			} else { isGrounded = false; }

			yield return new WaitForFixedUpdate ();
		}
	}

	IEnumerator ApplyDrag () {

		float airDrag;

		while (true) {

			airDrag = drag * rigidb.velocity.sqrMagnitude;
			rigidb.AddForce (transform.forward * -airDrag, ForceMode.Force);

			yield return new WaitForFixedUpdate ();
		}
	}

	/*void CheckDrift () {

		if (isGrounded) {

			float driftValue = Vector3.Dot (rigidb.velocity, transform.forward);

			if (driftValue > 0.5f) {
			 
			}
		}
	}*/
}
