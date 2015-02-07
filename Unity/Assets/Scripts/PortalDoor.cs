﻿using UnityEngine;

public class PortalDoor : MonoBehaviour
{
    public GameObject targetWaypoint;
    public GameObject portalPrefab;

    public bool doorActivated
    {
        get;
        set;
    }

    // Use this for initialization
    private void Start()
    {
        if (targetWaypoint == null)
        {
            Debug.LogError(this.gameObject.name + " could not find its target waypoint");
        }
    }

    public void ActiveDoor()
    {
        if (!doorActivated)
        {
            doorActivated = true;

            this.renderer.enabled = false;

            var visualPortal = Instantiate(portalPrefab, this.transform.position, this.transform.rotation) as GameObject;
            visualPortal.transform.localScale = this.transform.localScale;        
        }
    }

    private void Teleport(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("Could not teleport player, player is null");
            return;
        }

        player.transform.position = targetWaypoint.transform.position;
    }

    void OnCollisionEnter(Collision coll)
    {
        if (!doorActivated)
        {
            return;
        }

        if (coll.transform.CompareTag("Player"))
        {
            var other = coll.transform.root;
            Teleport(other.gameObject);
        }
    }
}