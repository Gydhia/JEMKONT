using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using DownBelow.Entity;

namespace DownBelow.Network
{
    public abstract class BaseNetworkMethod : IOnEventCallback
    {
        protected int checkedPlayers = 0;

        public abstract NetworkParamDesc[] parameters { get; }

        public abstract string description_key { get; }

        public abstract string name { get; }

        /// <summary>
        /// Functions can be read by mayor, if debug mode is enabled. Or by Debug
        /// </summary>
        /// <returns>True if the value can be retreived</returns>
        public virtual bool canRun()
        {
            return true;
        }

        public virtual void PreProcess()
        {
            
        }

        public virtual void ValidateAnswer(string PlayerID)
        {
            this.checkedPlayers++;
        }

        public abstract bool run(in string[] parameters, ref string errorMsg);


        public void OnEvent(EventData photonEvent)
        {
            
        }
    }
}