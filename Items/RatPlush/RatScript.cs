using System;
using System.Collections;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static CoolPeopleMod.Plugin;

namespace CoolPeopleMod.Items.RatPlush
{
    internal class RatScript : NetworkBehaviour // TODO: Finish this
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AudioSource RatAudio;
        public AudioClip BirthdayMixtapeSFX;
        public NavMeshAgent agent;
        public float AIIntervalTime;
        public ParticleSystem particleSystem;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        float timeSinceAIInterval;
        Vector3 mainEntrancePosition;
        NavMeshPath path1;
        bool moveTowardsDestination;
        Vector3 destination;
        bool inSpecialAnimation;

        public void Start()
        {
            mainEntrancePosition = RoundManager.FindMainEntrancePosition(true);
        }

        public void Update()
        {
            if (!IsServerOrHost) { return; }
            timeSinceAIInterval += Time.deltaTime;
            if (timeSinceAIInterval > AIIntervalTime)
            {
                DoAIInterval();
                timeSinceAIInterval = 0;
            }
        }

        void DoAIInterval()
        {
            if (inSpecialAnimation) { return; }
            if (moveTowardsDestination)
            {
                agent.SetDestination(destination);
            }

            if (!SetDestinationToPosition(mainEntrancePosition, true) || Vector3.Distance(transform.position, mainEntrancePosition) < 1f)
            {
                inSpecialAnimation = true;
                DisappearClientRpc();
            }
        }

        public bool SetDestinationToPosition(Vector3 position, bool checkForPath = false)
        {
            if (checkForPath)
            {
                position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);
                path1 = new NavMeshPath();
                if (!agent.CalculatePath(position, path1))
                {
                    return false;
                }
                if (Vector3.Distance(path1.corners[path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 2.7f)) > 1.55f)
                {
                    return false;
                }
            }
            moveTowardsDestination = true;
            destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
            return true;
        }

        public void Disappear()
        {
            particleSystem.Play();
            if (!IsServerOrHost) { return; }
            //StartCoroutine(DisappearCoroutine(0.5f));
            NetworkObject.Despawn(destroy: true);
        }

        IEnumerator DisappearCoroutine(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            NetworkObject.Despawn(destroy: true);
        }

        [ClientRpc]
        public void DisappearClientRpc()
        {
            Disappear();
        }
    }
}
