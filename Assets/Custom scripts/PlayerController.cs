using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour 
{
	public CharacterController CharCont;
	public CharacterMotor CharMotor;
	public Animator anim;

	//weapon type vars
	public LineRenderer linerenderer;
	public float linerednererRenderTime = 0.1f;

	//Holder for Weapons

	public Transform JumpAnimationHolder;
	public Transform SwayHolder;
	public Transform CameraRecoilHolder;
	public Transform RecoilHolder;
	public Transform CameraMovementHolder;
	public Transform PlayerCamera;

	//swayholder variables
	public float mouseSensitivity = 10f;

	private Vector3 startPos;

	private float factorX;
	private float factorY;

	
	public bool bRotate;

    //walkstate variables
	public float WalkSpeed;
	public float RunSpeed;
	public bool WasStanding; 
	public WalkingState walkingstate = WalkingState.Idle;
	public Transform ADSHolder;
	
    public float VelocityMagnitude;

	//player variables
	public WeaponInfo CurrentWeapon;
	public List<WeaponInfo> WeaponList = new List<WeaponInfo>();
	public bool IsAiming;

	private float shoottime = 0;

	public Vector3 CurrentRecoil1;
	public Vector3 CurrentRecoil2;
	public Vector3 CurrentRecoil3;
	public Vector3 CurrentRecoil4;

	void Start ()
	{	
		CurrentWeapon = WeaponList[0];
		startPos = SwayHolder.transform.localPosition;
		linerenderer.enabled = false;
		linerenderer.light.enabled = false;
	
	}

	public void FixedUpdate()
	{
		ShootController ();
		SwayController ();
		SpeedController();
		RecoilController();
		ADSController ();
		VelocityMagnitude = CharCont.velocity.magnitude;

		float speed = VelocityMagnitude;

		bool isGrounded = CharCont.isGrounded;
		bool jump = Input.GetButtonDown ("Jump");

		anim.SetFloat("Speed", speed);
		anim.SetBool("isGrounded", isGrounded);
		anim.SetBool("Jump", jump);


	}

	public void Update ()
	{
		ShootController ();

	}

	public enum WalkingState
	{
		Idle,
		Walking,
		Running,
		Jumping,
		Crouching,
		Prone,
		Swiming
	}

	public void SpeedController()
	{
		if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && VelocityMagnitude > 0
		  && CharCont.isGrounded )
		{
			if (Input.GetButton("Run"))
			{
				walkingstate = WalkingState.Running;
				CharMotor.movement.maxForwardSpeed = RunSpeed;
				CharMotor.movement.maxSidewaysSpeed = RunSpeed;
				CharMotor.movement.maxBackwardsSpeed = RunSpeed / 2;
			}

			else if (Input.GetButtonDown("Jump"))
			{
				walkingstate = WalkingState.Jumping;
				anim.SetBool ("Jump", Input.GetButtonDown("Jump"));
			}

			else
			{
				walkingstate = WalkingState.Walking;
				CharMotor.movement.maxForwardSpeed = WalkSpeed;
				CharMotor.movement.maxSidewaysSpeed = WalkSpeed;
				CharMotor.movement.maxBackwardsSpeed = WalkSpeed / 2;
			}

		}

		else if (Input.GetButtonDown("Jump") && CharCont.isGrounded )
		{
			walkingstate = WalkingState.Jumping;
			anim.SetBool ("Jump",Input.GetButtonDown("Jump"));
		}

		else 
		{
			walkingstate = WalkingState.Idle;
		}

	}
	

	
        public void SwayController()
	{
		//gunsway function
		// STORE MOUSE MOVEMENT AMOUNTS, SMOOTH WITH LERP
		 float MouseInputX = Mathf.Clamp (Input.GetAxis ("Mouse X"),-0.04f,0.04f);
		 float MouseInputY = Mathf.Clamp (Input.GetAxis ("Mouse Y"),-0.04f,0.04f);

		factorX = Mathf.Lerp(factorX, -MouseInputX * mouseSensitivity, Time.smoothDeltaTime * mouseSensitivity);
		factorY = Mathf.Lerp(factorY, -MouseInputY * mouseSensitivity, Time.smoothDeltaTime * (mouseSensitivity/2));
		
		// CLAMP LIMITS
		if (factorX > CurrentWeapon.maxMoveAmount)
		
			factorX = CurrentWeapon.maxMoveAmount;
		
		
		if (factorX < -CurrentWeapon.maxMoveAmount)
		
			factorX = -CurrentWeapon.maxMoveAmount;
	    
		
		if (factorY > CurrentWeapon.maxMoveAmount)
	
		
			factorY = CurrentWeapon.maxMoveAmount;
	    
		
		if (factorY < -CurrentWeapon.maxMoveAmount)
		
			factorY = -CurrentWeapon.maxMoveAmount;
	    
		
		// SET TARGET POSITION (START POSITION + MOUSE MOVEMENT)
		Vector3 targetPos = new Vector3(startPos.x + factorX, startPos.y + factorY, startPos.z);
		
		// APPLY POSITION TO WEAPON, SMOOTH WITH LERP
		SwayHolder.transform.localPosition = Vector3.Lerp(SwayHolder.transform.localPosition, targetPos, CurrentWeapon.smoothSpeed * Time.smoothDeltaTime);
		
		// ROTATION

		if (bRotate)
		{
			float tiltAroundZ = Input.GetAxis("Mouse X") * CurrentWeapon.rotateAmount;
			float tiltAroundX = Input.GetAxis("Mouse Y") * CurrentWeapon.rotateAmount;
			Vector3 target = new Vector3(tiltAroundX, 0f, tiltAroundZ);
			SwayHolder.transform.localRotation = Quaternion.Slerp(SwayHolder.transform.localRotation, Quaternion.Euler(target), Time.smoothDeltaTime * CurrentWeapon.smoothRotationSpeed);
		}
	}

		public void RecoilController()
	{
		CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, CurrentWeapon.RotationRecovery); //goes back to starting point rotation, recovery from recoil muzzle kick up
		CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, CurrentWeapon.RotationIntensity);//rotation amount per shot, rotation
		CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, CurrentWeapon.RecoilRecovery); //goes back to starting point postion, recovery from recoil
		CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, CurrentWeapon.RecoilIntensity);// positional change per shot, recoil 

		RecoilHolder.localEulerAngles = CurrentRecoil2;
		RecoilHolder.localPosition = CurrentRecoil4;

		CameraRecoilHolder.localEulerAngles = CurrentRecoil2 / CurrentWeapon.CameraKickBack;
	}

	public void ADSController()
	{
		if (Input.GetButton("Fire2"))
		{
			IsAiming = true;
			ADSHolder.localPosition = Vector3.Lerp(ADSHolder.localPosition, CurrentWeapon.Scopes[CurrentWeapon.CurrentScope].adsposition, 0.25f);
			PlayerCamera.camera.fieldOfView = Mathf.Lerp(PlayerCamera.camera.fieldOfView, CurrentWeapon.Scopes[CurrentWeapon.CurrentScope].fov, 0.25f);
		}
		else
		{
			IsAiming = false;
			ADSHolder.localPosition = Vector3.Lerp(ADSHolder.localPosition, Vector3.zero, 0.25f);
			PlayerCamera.camera.fieldOfView = Mathf.Lerp(PlayerCamera.camera.fieldOfView, 60, 0.25f);
		}
	}

	public void ShootController()
	{

		float Firerate = CurrentWeapon.FirerateRPM;
		
		if (Input.GetButton("Fire1")&& walkingstate != WalkingState.Running)
		{
			if ( shoottime <= Time.time)
			{
				shoottime = Time.time + Firerate;

				CurrentRecoil1 += new Vector3(CurrentWeapon.RecoilRotation.x, Random.Range(-CurrentWeapon.RecoilRotation.y, CurrentWeapon.RecoilRotation.y));

				CurrentRecoil3 += new Vector3(Random.Range(-CurrentWeapon.RecoilKickback.x, CurrentWeapon.RecoilKickback.x), Random.Range(-CurrentWeapon.RecoilKickback.y, CurrentWeapon.RecoilKickback.y), CurrentWeapon.RecoilKickback.z);

				anim.SetBool("isFiring", true);


				RaycastHit hit;
				if (Physics.Raycast(CurrentWeapon.ProjectileSpawn.position, CurrentWeapon.ProjectileSpawn.TransformDirection(Vector3.forward), out hit, 500))
				{


					hit.transform.SendMessageUpwards("GetBulletDamage", CurrentWeapon.name, SendMessageOptions.DontRequireReceiver);
				 
					linerenderer.enabled = true;
					linerenderer.SetPosition(0, CurrentWeapon.ProjectileSpawn.position);
					linerenderer.SetPosition(1, hit.point);
					linerenderer.light.enabled = true;
					Debug.Log(linerenderer.isVisible);
				

				}
				else 
				{
					linerenderer.enabled = true;
					linerenderer.SetPosition(0, CurrentWeapon.ProjectileSpawn.position);
					linerenderer.SetPosition(1, new Vector3())
						;
					linerenderer.light.enabled = true;
					Debug.Log(linerenderer.isVisible);
				}
			
			}

			else
			{
				anim.SetBool("isFiring", false);
		    }
	    }
		else
		{
			anim.SetBool("isFiring", false);
			linerenderer.enabled = false;
			Debug.Log(linerenderer.isVisible);
			linerenderer.light.enabled = false;
		}
    }
}
[System.Serializable]
public class WeaponInfo
{	
	public string name = "Weapon";
	public string MuzzleFX = "MuzzleFlash";
	public float FirerateRPM = 0.1f; 
	public float RecoilIntensity = 0.45f;
	public float RotationIntensity = 0.01f;
	public float RecoilRecovery = 0.1f;
	public float RotationRecovery = 0.2f;
	public int WeaponDamageDirect = 25;
	public int WeaponDamageSplash = 25;

	public Transform ProjectileSpawn;
	public Transform WeaponTransform;

	public Vector3 RecoilRotation;
	public Vector3 RecoilKickback;
	
	public float maxMoveAmount = 0.5f;
	public float smoothSpeed = 3f;
	
	public float smoothRotationSpeed = 2f;
	public float rotateAmount = 45f;

	public float CameraKickBack = 6F;
	public int CurrentScope;
	public List<WeaponScope> Scopes = new List<WeaponScope>();
}   

[System.Serializable]
public class WeaponScope
{
	public string name;
	public float fov;
	public Vector3 adsposition;
	public Transform scopetransform;
}
