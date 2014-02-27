

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerManager : MonoBehaviour
{
	public static MultiplayerManager instance;

	public GameObject PlayerManagerPrefab;
	
	public string PlayerName;
	
	private string MatchName = "";
	private string MatchPassword = "";
	private int MatchMaxUsers = 32;
	
	public List<MPPlayer> PlayerList = new List<MPPlayer>();
	public List<MapSetting> MapList = new List<MapSetting>();
	
	public MapSetting CurrentMap = null;
	public int oldprefix;
	public bool IsMatchStarted = false;
	
	//General Multiplayer Modes
	public bool MatchLoaded;
	public MPPlayer MyPlayer;
	public GameObject[] Spawnpoints;
	
	void Start()
	{
		instance = this;
		PlayerName = PlayerPrefs.GetString("PlayerName");
		CurrentMap = MapList[0];
		DontDestroyOnLoad(gameObject);
	}
	
	void FixedUpdate()
	{
		instance = this;
	}
	
	public void StartServer(string servername, string serverpassword, int maxusers)
	{
		MatchName = servername;
		MatchPassword = serverpassword;
		MatchMaxUsers = maxusers;
		Network.InitializeServer(MatchMaxUsers, 2550, false);
		MasterServer.RegisterHost("DeathMatch", MatchName, "");
		//Network.InitializeSecurity();
	}
	
	void OnServerInitialized()
	{
		Server_PlayerJoinRequest(PlayerName, Network.player);
	}
	
	void OnConnectedToServer()
	{
		networkView.RPC("Server_PlayerJoinRequest", RPCMode.Server, PlayerName, Network.player);
	}
	
	void OnPlayerDisconnected(NetworkPlayer id)
	{
		networkView.RPC("Client_RemovePlayer", RPCMode.All, id);
	}
	
	void OnPlayerConnected(NetworkPlayer player)
	{
		foreach(MPPlayer pl in PlayerList)
		{
			networkView.RPC("Client_AddPlayerToList", player, pl.PlayerName, pl.PlayerNetwork);
		}
		networkView.RPC("Client_GetMultiplayerMatchSettings", player, CurrentMap.MapName, "", "");
	}
	
	void OnDisconnectedFromServer()
	{
		PlayerList.Clear();
	}
	
	[RPC]
	void Server_PlayerJoinRequest(string playername, NetworkPlayer view)
	{
		networkView.RPC("Client_AddPlayerToList", RPCMode.All, playername, view);
	}
	
	[RPC]
	void Client_AddPlayerToList(string playername, NetworkPlayer view)
	{
		MPPlayer tempplayer = new MPPlayer();
		tempplayer.PlayerName = playername;
		tempplayer.PlayerNetwork = view;
		PlayerList.Add(tempplayer);
		if (Network.player == view)
		{
			Debug.Log("Client_AddPlayerToList,if (Network.player == view), called");
			MyPlayer = tempplayer;
			GameObject play = Network.Instantiate(PlayerManagerPrefab, Vector3.zero, Quaternion.identity, 5) as GameObject;
			//play.GetComponent<PlayerManager>().thisplayer = MyPlayer; //Remove This Part
		}
	}
	
	[RPC]
	void Client_RemovePlayer(NetworkPlayer view)
	{
		MPPlayer temppl = null;
		foreach(MPPlayer pl in PlayerList)
		{
			if (pl.PlayerNetwork == view)
			{
				temppl = pl;
			}
		}
		if (temppl != null)
		{
			PlayerList.Remove(temppl);
		}
	}
	
	[RPC]
	void Client_GetMultiplayerMatchSettings(string map, string mode, string others)
	{
		CurrentMap = GetMap(map);
	}
	
	public MapSetting GetMap(string name)
	{
		MapSetting get = null;
		
		foreach (MapSetting st in MapList)
		{
			if (st.MapName == name)
			{
				get = st;
				break;
			}
		}
		
		return get;
	}
	
	[RPC]
	void Client_LoadMultiplayerMap(string map, int prefix)
	{
		//Network.SetLevelPrefix(prefix);
		Application.LoadLevel(map);
	}
	
	void OnGUI()
	{
		if (!MyPlayer.PlayerIsAlive && IsMatchStarted)
			SpawnMenu();
		if (IsMatchStarted && Input.GetKey(KeyCode.Tab))
		{

		}
	}
	
	[RPC]
	void Server_SpawnPlayer(NetworkPlayer player)
	{
		int numberspawn = Random.Range(0, Spawnpoints.Length - 1);
		networkView.RPC("Client_SpawnPlayer", RPCMode.All, player, Spawnpoints[numberspawn].transform.position + new Vector3(0, 2, 0), Spawnpoints[numberspawn].transform.rotation);
	}
	
	[RPC]
	void Client_SpawnPlayer(NetworkPlayer player, Vector3 position, Quaternion rotation)
	{
		MultiplayerManager.GetMPPlayer(player).PlayerIsAlive = true;
		MultiplayerManager.GetMPPlayer(player).PlayerHealth = 100;
		if (player == MyPlayer.PlayerNetwork)
		{
			MyPlayer.PlayerManager.ControllerTransform.position = position;
			MyPlayer.PlayerManager.ControllerTransform.rotation = rotation;
			MyPlayer.PlayerManager.networkView.RPC("Client_PlayerAlive", RPCMode.All);
		}
		else
		{
			
		}
	}
	
	void OnLevelWasLoaded()
	{
		if (Application.loadedLevelName == CurrentMap.MapLoadName && Network.isServer)
		{
			MatchLoaded = true;
			Spawnpoints = GameObject.FindGameObjectsWithTag("spawnpoint");
			networkView.RPC("Client_ServerLoaded", RPCMode.AllBuffered, IsMatchStarted);//Add parameter for boolean IsMatchStarted
		}
	}
	
	[RPC]
	void Client_ServerLoaded(bool started)//Add parameter for boolean IsMatchStarted
	{
		MatchLoaded = true;
		IsMatchStarted = started;
	}
	
	public static MPPlayer GetMPPlayer(NetworkPlayer player)
	{
		foreach (MPPlayer play in MultiplayerManager.instance.PlayerList)
		{
			if (play.PlayerNetwork == player)
			{
				return play;
			}
		}
		return null;
	}
	
	//Deathmatch
	void SpawnMenu()
	{
		if (GUI.Button(new Rect(5, Screen.height - 40, 250, 35), "Spawn"))
		{
			if (Network.isServer)
				Server_SpawnPlayer(Network.player);
			else
				networkView.RPC("Server_SpawnPlayer", RPCMode.Server, Network.player);
		}
	}
	
	[RPC]
	public void Client_UpdatePlayerHealthAndAlive(NetworkPlayer targetplayer, bool IsAlive, int CurrentHealth)
	{
		MPPlayer player = MultiplayerManager.GetMPPlayer(targetplayer);
		player.PlayerIsAlive = IsAlive;
		player.PlayerHealth = CurrentHealth;
	}
}

[System.Serializable]
public class MPPlayer
{
	public string PlayerName = "";
	public NetworkPlayer PlayerNetwork;
	public PlayerManager PlayerManager;
	public int PlayerHealth = 100;
	public bool PlayerIsAlive;
	public int PlayerScore;
	public int PlayerKills;
	public int PlayerDeahts;
}

[System.Serializable]
public class MapSetting
{
	public string MapName;
	public string MapLoadName;
	public Texture MapLoadTexture;
}

