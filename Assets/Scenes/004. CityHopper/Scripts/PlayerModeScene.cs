using UnityEngine;
using UniRx;
using System;

namespace Bercetech.Games.Fleepas.CityBunny
{
    // Class to manage the activation of Single and Multiplayer modes
    // and to manage shared Menu Objects
    public class PlayerModeScene: PlayerModeGeospatial
    {
        //[SerializeField]
        //private GameObject _singlePlayerFleepSiteRanking;
        //[SerializeField]
        //private GameObject _gameSigns;
        override public void GameWarming()
        {
            base.GameWarming();
            // Adding additional objects to prewarm
            // TargetGenerator consumes time when loading on the first game launch, because it
            // has several methods in its awake, enable blocks
            // Not needed for Multiplayer-Peer
            if (_gameType != 2)
            {
                //TargetGenerator.SharedInstance.enabled = true;
                //_gameSigns.SetActive(true);
                // Wait a litte bit to disable. Otherwise some blocks are not completely preloaded
                // NOTE: not useful here because most of the targetGenerator logic needs a arMesh to
                // be loaded
                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => {
                    //TargetGenerator.SharedInstance.enabled = false;
                    //_gameSigns.SetActive(false);
                });
            }
            //// Prewarm some other in-game objects
            //_singlePlayerFleepSiteRanking.SetActive(true);
            //_singlePlayerFleepSiteRanking.SetActive(false);
            // Warm up Physics Raycast
            //Physics.Raycast(new Vector3(0, 0, 0), new Vector3(1, 0, 0), out RaycastHit hit, 100, TargetGenerator.SharedInstance.ArMeshChunksLayerMask, QueryTriggerInteraction.Collide);

        }
    }
}
