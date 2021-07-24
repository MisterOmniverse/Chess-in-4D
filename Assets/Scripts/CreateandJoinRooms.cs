using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateandJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField joinRoom;
    public InputField createRoom;

    public void JoinRoom() {
        PhotonNetwork.JoinRoom(joinRoom.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Multiplayer");
    }

     public void CreateRoom() {
        PhotonNetwork.CreateRoom(createRoom.text);
    }
}
