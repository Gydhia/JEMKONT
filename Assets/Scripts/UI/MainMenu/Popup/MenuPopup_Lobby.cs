using DownBelow.Managers;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.UI.Menu
{
    public class MenuPopup_Lobby : BaseMenuPopup
    {
        public UIRoomItem RoomPrefab;
        public Transform RoomsHolder;

        private List<UIRoomItem> _roomList = new List<UIRoomItem>();

        public override void ShowPopup()
        {
            base.ShowPopup();

            foreach (var room in this._roomList)
            {
                room.JoinButton.interactable = true;
            }
        }

        public void UpdateRoomList(List<RoomInfo> roomList)
        {
            // Destroy existing rooms
            for (int i = 0; i < this._roomList.Count; i++)
                Destroy(this._roomList[i].gameObject);

            this._roomList.Clear();

            // Create the updated ones
            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].PlayerCount == 0)
                    continue;

                this._roomList.Add(Instantiate(this.RoomPrefab, this.RoomsHolder));
                this._roomList[i].SetName(roomList[i].Name, roomList[i].PlayerCount);
            }
        }
    }
}