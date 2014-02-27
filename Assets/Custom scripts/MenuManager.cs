

using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
	public MenuManager instance;
	
	public string CurrentMenu;
	
	public string MatchName = "";
	public string MatchPassword = "";
	public int MatchMaxPlayers = 32;
	
	private Vector2 ScrollLobby = Vector2.zero;
	private string iptemp = "127.0.0.1";
	
	void Start()
	{
		instance = this;
		CurrentMenu = "Main";
		MatchName = "HeyHeyWelcome " + Random.Range(0, 5000);
	}
	
	void FixedUpdate()
	{
		instance = this;
	}
	
	void OnGUI()
	{
		if (CurrentMenu == "Main")
			Menu_Main();
		if (CurrentMenu == "Lobby")
			Menu_Lobby();
		if (CurrentMenu == "Host")
			Menu_HostGame();
		if (CurrentMenu == "ChoMap")
			Menu_ChooseMap();
	}
	
	public void NavigateTo(string nextmenu)
	{
		CurrentMenu = nextmenu;
	}
	
	private void Menu_Main()
	{
		if (GUI.Button(new Rect(10, 10, 200, 50), "Host Game"))
		{
			NavigateTo("Host");
		}
		if (GUI.Button(new Rect(10, 70, 200, 50), "Refresh"))
		{
			MasterServer.RequestHostList("DeathMatch");
		}
		
		GUI.Label(new Rect(220, 10, 130, 30), "Player Name");
		MultiplayerManager.instance.PlayerName = GUI.TextField(new Rect(350, 10, 150, 30), MultiplayerManager.instance.PlayerName);
		if (GUI.Button(new Rect(510, 10, 100, 30), "Save Name"))
		{
			PlayerPrefs.SetString("PlayerName", MultiplayerManager.instance.PlayerName);
		}
		
		GUI.Label(new Rect(220, 50, 130, 30), "Direct Connect");
		iptemp = GUI.TextField(new Rect(350, 50, 150, 30), iptemp);
		if (GUI.Button(new Rect(510, 50, 100, 30), "Connect"))
		{
			Network.Connect(iptemp, 2550);
		}
		
		GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height), "Server List", "Box");
		GUILayout.Space(20);
		foreach (HostData match in MasterServer.PollHostList())
		{
			GUILayout.BeginHorizontal("Box");
			
			GUILayout.Label(match.gameName);
			if (GUILayout.Button("Connect"))
			{
				Network.Connect(match);
			}
			
			GUILayout.EndHorizontal();
		}
		
		GUILayout.EndArea();
	}
	
	private void Menu_HostGame()
	{
		//Buttons Host Game
		if (GUI.Button(new Rect(10, 10, 200, 50), "Back"))
		{
			NavigateTo("Main");
		}
		
		if (GUI.Button(new Rect(10, 60, 200, 50), "Start Server"))
		{
			MultiplayerManager.instance.StartServer(MatchName, MatchPassword, MatchMaxPlayers);
		}
		
		if (GUI.Button(new Rect(10, 160, 200, 50), "Choose Map"))
		{
			NavigateTo("ChoMap");
		}
		
		GUI.Label(new Rect(220, 10, 130, 30), "Match Name");
		MatchName = GUI.TextField(new Rect(400, 10, 200, 30), MatchName);
		
		GUI.Label(new Rect(220, 50, 130, 30), "Match Password");
		MatchPassword = GUI.PasswordField(new Rect(400, 50, 200, 30), MatchPassword, '*');
		
		GUI.Label(new Rect(220, 90, 130, 30), "Match Max Players");
		GUI.Label(new Rect(400, 90, 200, 30), (MatchMaxPlayers + 1).ToString());
		MatchMaxPlayers = Mathf.Clamp(MatchMaxPlayers, 8, 31);
		
		if (GUI.Button(new Rect(425, 90, 30, 20), "+"))
			MatchMaxPlayers += 2;
		if (GUI.Button(new Rect(455, 90, 30, 20), "-"))
			MatchMaxPlayers -= 2;
		
		GUI.Label(new Rect(650, 10, 130, 30), MultiplayerManager.instance.CurrentMap.MapName);
	}
	
	private void Menu_Lobby()
	{
		ScrollLobby = GUILayout.BeginScrollView(ScrollLobby, GUILayout.MaxWidth(200));
		
		foreach (MPPlayer pl in MultiplayerManager.instance.PlayerList)
		{
			if (pl.PlayerNetwork == Network.player)
				GUI.color = Color.blue;
			GUILayout.Box(pl.PlayerName);
			GUI.color = Color.white;
		}
		
		GUILayout.EndScrollView();
		
		GUI.Box(new Rect(250, 10, 200, 40), MultiplayerManager.instance.CurrentMap.MapName);
		
		if (Network.isServer)
		{
			if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 80, 200, 40), "Start Match"))
			{
				MultiplayerManager.instance.networkView.RPC("Client_LoadMultiplayerMap", RPCMode.AllBuffered, MultiplayerManager.instance.CurrentMap.MapLoadName, MultiplayerManager.instance.oldprefix + 1);
				MultiplayerManager.instance.oldprefix += 1;
				MultiplayerManager.instance.IsMatchStarted = true;
			}
		}
		if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 40, 200, 40), "Disconnect"))
		{
			Network.Disconnect();
		}
	}
	
	private void Menu_ChooseMap()
	{
		if (GUI.Button(new Rect(10, 10, 200, 50), "Back"))
		{
			NavigateTo("Host");
		}
		
		GUI.Label(new Rect(220, 10, 130, 30), "Choose Map");
		GUILayout.BeginArea(new Rect(350, 10, 150, Screen.height));
		
		foreach(MapSetting map in MultiplayerManager.instance.MapList)
		{
			if (GUILayout.Button(map.MapName))
			{
				NavigateTo("Host");
				MultiplayerManager.instance.CurrentMap = map;
			}
		}
		
		GUILayout.EndArea();
	}
	
	void OnServerInitialized()
	{
		NavigateTo("Lobby");
	}
	
	void OnConnectedToServer()
	{
		NavigateTo("Lobby");
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		NavigateTo("Main");
	}
}

