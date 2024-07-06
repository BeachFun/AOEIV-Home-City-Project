using RTSEngine.BuildingExtension;
using RTSEngine.Entities;
using RTSEngine.ResourceExtension;
using RTSEngine.Game;
using RTSEngine.Logging;
using RTSEngine.NPC;
using RTSEngine.UnitExtension;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeTransformation : MonoBehaviour
{
    private IGameManager gameMgr;
    private IResourceManager resourceMgr;
    public GameObject treeLogPrefab;

    private void Start()
    {
        this.gameMgr = FindObjectOfType<GameManager>();
        this.resourceMgr = gameMgr.GetService<IResourceManager>();
    }

    public void SpawnTreeLog()
    {
        resourceMgr.CreateResource(treeLogPrefab.GetComponent<IResource>(), transform.position, transform.rotation, new InitResourceParameters
        {
            free = true,
            factionID = -1,
            setInitialHealth = false,
        });
    }
}
