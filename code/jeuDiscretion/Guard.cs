using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{

    public Light spotlight;
    public float viewDistance;
    float viewAngle;
    public Transform pathHolder;
    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90;
    public LayerMask viewMask;
    Color originalSportLightColor; 
    public static event System.Action OnGuardHasSpottedPlayer;

    public float timeToSpotPlayer = .5f;
    float playerVisibleTimer;

    Transform player;
    // afficher dans unity
    void OnDrawGizmos() {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach(Transform waypoint in pathHolder) {
            Gizmos.DrawSphere(waypoint.position,.3f);
            Gizmos.DrawLine(previousPosition,waypoint.position);
            previousPosition = waypoint.position;
        }
        // dernière ligne pour faire une boucle
        Gizmos.DrawLine(previousPosition,startPosition);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position,transform.forward*viewDistance);
    }

    bool CanSeePlayer() {
        if(Vector3.Distance(transform.position,player.position)<viewDistance) {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweeGuardAndPlayer = Vector3.Angle(transform.forward,dirToPlayer);
            if(angleBetweeGuardAndPlayer < viewAngle/2f) {
                if(!Physics.Linecast(transform.position,player.position,viewMask)) {
                    return true;
                }
            }
        }
        return false;
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSportLightColor = spotlight.color;
        viewAngle = spotlight.spotAngle;
        Vector3 [] waypoints = new Vector3 [pathHolder.childCount];
        for (int i=0; i<waypoints.Length; i++) {
            waypoints[i] =pathHolder.GetChild(i).position;
            // on fait en sorte que chaque point ai la même hauteur pour que notre déplacement ne soit pas chelou
            waypoints[i] = new Vector3(waypoints[i].x,transform.position.y,waypoints[i].z);
        }   
            StartCoroutine(FollowPath(waypoints));
    }

    void Update() {
        if(CanSeePlayer()) {
            // spotlight.color = Color.red;
            playerVisibleTimer += Time.deltaTime;
        }
        else {
            playerVisibleTimer -= Time.deltaTime;
            spotlight.color = originalSportLightColor;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer,0,timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSportLightColor,Color.red, playerVisibleTimer/timeToSpotPlayer);
        if(playerVisibleTimer >= timeToSpotPlayer) {
             if(OnGuardHasSpottedPlayer != null) {
                 OnGuardHasSpottedPlayer();
             }
        }
    }
    IEnumerator FollowPath(Vector3 [] Waypoints) {
        transform.position = Waypoints[1];
        int targetWaypointIndex =1;
        Vector3 targetWaypoint = Waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint);
        while(true) {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint,speed*Time.deltaTime);
            if(transform.position == targetWaypoint) {
                targetWaypointIndex = (targetWaypointIndex +1)%  Waypoints.Length;
                targetWaypoint = Waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            // juste 1 frame
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget) {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x)*Mathf.Rad2Deg;
        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle))>0.05f) {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }
}
