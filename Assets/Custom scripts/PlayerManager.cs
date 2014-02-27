

using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
	public MPPlayer thisplayer
	{
		get
		{
			return MultiplayerManager.GetMPPlayer(networkView.owner);
		}
	}
	public PlayerController Controller;
	public Transform ControllerTransform;
	
	public GameObject OutsideView;
	
	public Vector3 CurrentPosition;
	public Quaternion CurrentRotation;
	
	void Start()
	{
		DontDestroyOnLoad(gameObject);
		ControllerTransform.gameObject.SetActive(false);
		OutsideView.SetActive(false);
		//thisplayer = MultiplayerManager.GetMPPlayer(networkView.owner); //FIX TUTORIAL #7 - Remove This Part and add the part above.
		thisplayer.PlayerManager = this;
	}
	
	void FixedUpdate()
	{
		if (networkView.isMine)
		{
			CurrentPosition = ControllerTransform.position;
			CurrentRotation = ControllerTransform.rotation;
		}
		else
		{
			//ControllerTransform.position = CurrentPosition; //FIX TUTORIAL #7 - Remove this part and add the part below.
			OutsideView.transform.position = CurrentPosition + new Vector3(0,-1f,0);
			OutsideView.transform.rotation = CurrentRotation;
		}
	}
	
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.Serialize(ref CurrentPosition);
			stream.Serialize(ref CurrentRotation);
		}
		else
		{
			stream.Serialize(ref CurrentPosition);
			stream.Serialize(ref CurrentRotation);
		}  
	}
	
	void GetBulletDamage(int damage)
	{
		if (Network.isServer)
			Server_HandleBulletDamage(damage);
		else
			networkView.RPC("Server_HandleBulletDamage", RPCMode.Server, damage);
	}
	
	[RPC]
	public void Server_HandleBulletDamage(int damage)
	{
		thisplayer.PlayerHealth -= damage;
		if (thisplayer.PlayerHealth <= 0) //WHEN DEAD
		{
			thisplayer.PlayerIsAlive = false;
			thisplayer.PlayerHealth = 0;
			networkView.RPC("Client_PlayerDead", RPCMode.All);
			MultiplayerManager.instance.networkView.RPC("Client_UpdatePlayerHealthAndAlive", RPCMode.Others,thisplayer.PlayerNetwork, false, 0);
		}
		else // WHEN ALIVE GETTITNG HIT
		{
			MultiplayerManager.instance.networkView.RPC("Client_UpdatePlayerHealthAndAlive", RPCMode.All, thisplayer.PlayerNetwork, thisplayer.PlayerIsAlive, thisplayer.PlayerHealth);
		}
	}
	
	[RPC]
	public void Client_PlayerDead()
	{
		OutsideView.SetActive(false);
		if (networkView.isMine)
			ControllerTransform.gameObject.SetActive(false);
	}
	
	[RPC]
	public void Client_PlayerAlive()
	{
		if (networkView.isMine)
			ControllerTransform.gameObject.SetActive(true);
		else
			OutsideView.SetActive(true);
	}
}

