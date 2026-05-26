using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DTeamRouteController : MonoBehaviour
    {
        [SerializeField] private SplineComputer route;
        [SerializeField] private float routeSpeed = 2.4f;
        [SerializeField] private float sideSpacing = 1.35f;
        [SerializeField] private float rowSpacing = 1.15f;
        [SerializeField] private float formationRefreshInterval = 0.15f;
        [SerializeField] private float enemyScanInterval = 0.25f;
        [SerializeField] private float detectionRadius = 7f;
        [SerializeField] private LayerMask scanMask = ~0;

        private readonly List<IDRPG3DCombatUnit> heroes = new List<IDRPG3DCombatUnit>(5);
        private readonly List<IDRPG3DFormationMember> formationMembers = new List<IDRPG3DFormationMember>(5);
        private readonly List<IDRPG3DFormationDestination> destinations = new List<IDRPG3DFormationDestination>(5);
        private readonly Collider[] scanResults = new Collider[32];
        private readonly Dictionary<int, IDRPG3DCombatUnit> heroesById = new Dictionary<int, IDRPG3DCombatUnit>(5);

        private double routePercent;
        private Vector3 anchorPosition;
        private Vector3 anchorForward = Vector3.forward;
        private float nextFormationRefreshTime;
        private float nextEnemyScanTime;
        private IDRPG3DCombatUnit activeEnemy;
        private SplineSample routeSample;

        public Vector3 AnchorPosition => anchorPosition;
        public Vector3 AnchorForward => anchorForward;

        public void SetDetectionRadius(float radius)
        {
            detectionRadius = Mathf.Max(0.1f, radius);
        }

        public void Configure(SplineComputer targetRoute, IReadOnlyList<IDRPG3DCombatUnit> teamHeroes)
        {
            route = targetRoute;
            heroes.Clear();
            heroesById.Clear();
            for (var i = 0; i < teamHeroes.Count; i++)
            {
                if (teamHeroes[i] == null)
                {
                    continue;
                }

                heroes.Add(teamHeroes[i]);
                heroesById[teamHeroes[i].UnitId] = teamHeroes[i];
            }

            if (route != null && heroes.Count > 0)
            {
                route.Project(heroes[0].transform.position, ref routeSample);
                routePercent = routeSample.percent;
                UpdateAnchor(0f);
            }
        }

        private void Update()
        {
            if (route == null || heroes.Count == 0)
            {
                return;
            }

            if (activeEnemy == null || !activeEnemy.IsAlive)
            {
                if (activeEnemy != null && !activeEnemy.IsAlive)
                {
                    ClearHeroTargets();
                    route.Project(GetTeamCenter(), ref routeSample);
                    routePercent = routeSample.percent;
                }

                activeEnemy = null;
                UpdateAnchor(Time.deltaTime * routeSpeed);
                TickEnemyScan();
                TickFormationFollow();
            }
            else
            {
                CommandHeroesToAttack(activeEnemy);
            }
        }

        private void UpdateAnchor(float travelDistance)
        {
            if (travelDistance > 0f)
            {
                routePercent = route.Travel(routePercent, travelDistance, Spline.Direction.Forward);
                if (routePercent >= 0.9999 && route.isClosed)
                {
                    routePercent = 0.0;
                }
            }

            route.Evaluate(routePercent, ref routeSample);
            anchorPosition = routeSample.position;
            anchorForward = routeSample.forward;
            anchorForward.y = 0f;
            if (anchorForward.sqrMagnitude < 0.0001f)
            {
                anchorForward = Vector3.forward;
            }
            else
            {
                anchorForward.Normalize();
            }
        }

        private void TickFormationFollow()
        {
            if (Time.time < nextFormationRefreshTime)
            {
                return;
            }

            nextFormationRefreshTime = Time.time + formationRefreshInterval;
            formationMembers.Clear();
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                if (hero == null || !hero.IsAlive)
                {
                    continue;
                }

                formationMembers.Add(new IDRPG3DFormationMember(
                    hero.UnitId,
                    hero.TeamOrder,
                    hero.MovePriority,
                    hero.Radius));
            }

            IDRPG3DTeamFormationSolver.BuildFormation(
                formationMembers,
                anchorPosition,
                anchorForward,
                sideSpacing,
                rowSpacing,
                destinations);

            for (var i = 0; i < destinations.Count; i++)
            {
                if (!heroesById.TryGetValue(destinations[i].UnitId, out var hero))
                {
                    continue;
                }

                var brain = hero.GetComponent<IDRPG3DAutoCombatBrain>();
                if (brain != null && brain.HasTarget)
                {
                    continue;
                }

                var mover = hero.GetComponent<IDRPG3DNavMoveAgent>();
                mover?.MoveTo(destinations[i].WorldPosition, 0.08f);
            }
        }

        private void TickEnemyScan()
        {
            if (Time.time < nextEnemyScanTime)
            {
                return;
            }

            nextEnemyScanTime = Time.time + enemyScanInterval;
            var count = Physics.OverlapSphereNonAlloc(anchorPosition, detectionRadius, scanResults, scanMask, QueryTriggerInteraction.Ignore);
            var bestSqrDistance = float.MaxValue;
            IDRPG3DCombatUnit bestEnemy = null;

            for (var i = 0; i < count; i++)
            {
                var hit = scanResults[i];
                if (hit == null || !hit.TryGetComponent<IDRPG3DCombatUnit>(out var candidate))
                {
                    candidate = hit != null ? hit.GetComponentInParent<IDRPG3DCombatUnit>() : null;
                }

                if (candidate == null || !candidate.IsAlive || candidate.Faction != IDRPG3DCombatFaction.Enemy)
                {
                    continue;
                }

                var sqrDistance = (candidate.transform.position - anchorPosition).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    bestEnemy = candidate;
                }
            }

            if (bestEnemy != null)
            {
                activeEnemy = bestEnemy;
                CommandHeroesToAttack(activeEnemy);
            }
        }

        private void CommandHeroesToAttack(IDRPG3DCombatUnit enemy)
        {
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                if (hero == null || !hero.IsAlive)
                {
                    continue;
                }

                var brain = hero.GetComponent<IDRPG3DAutoCombatBrain>();
                if (brain != null)
                {
                    brain.SetTarget(enemy);
                }
            }
        }

        private void ClearHeroTargets()
        {
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                if (hero == null)
                {
                    continue;
                }

                var brain = hero.GetComponent<IDRPG3DAutoCombatBrain>();
                brain?.ClearTarget();
            }
        }

        private Vector3 GetTeamCenter()
        {
            var center = Vector3.zero;
            var count = 0;
            for (var i = 0; i < heroes.Count; i++)
            {
                var hero = heroes[i];
                if (hero == null || !hero.IsAlive)
                {
                    continue;
                }

                center += hero.transform.position;
                count++;
            }

            return count > 0 ? center / count : anchorPosition;
        }
    }
}
