using UnityEngine;
using System.Collections;

public class iPlayerController : MonoBehaviour {
	
	public Animator anim;
	public CharacterController CharCont;
	public CharacterMotor CharMotor;

	public WalkingState walkingstate = WalkingState.Idle;

	public float WalkSpeed;
	public float RunSpeed;
	public float VelocityMagnitude;
	
	public int movementstate = Animator.StringToHash("Base Layer.Kantrael_m36_idle");
	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{

		float move = Input.GetAxis ("Vertical");
		anim.SetFloat("Speed", move);


	}

	public enum WalkingState
	{
		Idle,
		Walking,
		Running
	}
	
	public void SpeedController()
	{
		if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && VelocityMagnitude > 0)
		{
			if (Input.GetButton("Run"))
			{
				walkingstate = WalkingState.Running;
				CharMotor.movement.maxForwardSpeed = RunSpeed;
				CharMotor.movement.maxSidewaysSpeed = RunSpeed;
				CharMotor.movement.maxBackwardsSpeed = RunSpeed / 2;
			}
			else
			{
				walkingstate = WalkingState.Walking;
				CharMotor.movement.maxForwardSpeed = WalkSpeed;
				CharMotor.movement.maxSidewaysSpeed = WalkSpeed;
				CharMotor.movement.maxBackwardsSpeed = WalkSpeed / 2;
			}
		}
		else
		{
			walkingstate = WalkingState.Idle;
		}
	}
}
