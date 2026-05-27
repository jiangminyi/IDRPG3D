using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;

public class ProjectileSlowZone : MonoBehaviour
{

    public float velocityModifier = 0.1f;

    private class ProjectileList
    {
        public GameObject GO;
        public Vector3 cachedVelocity;
    }

    private List<ProjectileList> projectileList = new List<ProjectileList>();
    private void OnTriggerEnter(Collider other)
    {
        ProjectileHitDetection projRef = other.gameObject.GetComponent<ProjectileHitDetection>();
        if (projRef == null) return;
        if (projRef.RB == null) return;
        ProjectileList newProjectileList = new ProjectileList();
        newProjectileList.GO = other.gameObject;
        newProjectileList.cachedVelocity = projRef.RB.linearVelocity;
        projectileList.Add(newProjectileList);
        projRef.RB.linearVelocity *= velocityModifier;
    }
    
    private void OnTriggerExit(Collider other)
    {
        ProjectileHitDetection projRef = other.gameObject.GetComponent<ProjectileHitDetection>();
        if (projRef == null) return;
        if (projRef.RB == null) return;
        foreach (var proj in projectileList)
        {
            if(proj.GO != other.gameObject) continue;
            projRef.RB.linearVelocity = proj.cachedVelocity;
            projectileList.Remove(proj);
            break;
        }
    }
}
