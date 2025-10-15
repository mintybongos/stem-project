using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class death : MonoBehaviour
{
    public HingeJoint upLegL, downLegL, upLegR, downLegR, footL, footR, downArmL, downArmR, head;
    public arms upArmL, upArmR;
    public ConfigurableJoint balancer;
    public walk Walk;
    public Animator anim;
    public playerGun ps;
    public bool player;

    private bool isDead = false;
    private bool isFunRagdoll = false;

    // save balancer settings
    private JointDrive origXDrive, origYZDrive;

    void Start()
    {
        if (balancer != null)
        {
            origXDrive = balancer.angularXDrive;
            origYZDrive = balancer.angularYZDrive;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isDead)
                die(); // enter death ragdoll
            else
                respawn(); // recover from death ragdoll
        }

        if (Input.GetKeyDown(KeyCode.B) && !isDead)
        {
            if (!isFunRagdoll)
                funRagdoll();   // go limp but keep balance
            else
                recoverFunRagdoll(); // stand back up
        }
    }

    public void die()
    {
        if (player)
        {
            Walk.enabled = false;
            ps.enabled = false;
            upArmL.canUse = false;
            upArmR.canUse = false;
        }

        if (balancer != null)
        {
            // disable balancing forces
            JointDrive off = new JointDrive();
            off.positionSpring = 0;
            off.positionDamper = 0;
            off.maximumForce = 0;
            balancer.angularXDrive = off;
            balancer.angularYZDrive = off;
        }

        anim.Play("idle");
        setSprings(false);

        isDead = true;
    }

    public void respawn()
    {
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        if (player)
        {
            Walk.enabled = true;
            ps.enabled = true;
            upArmL.canUse = true;
            upArmR.canUse = true;
        }

        if (balancer != null)
        {
            // restore balancing forces
            balancer.angularXDrive = origXDrive;
            balancer.angularYZDrive = origYZDrive;
        }

        setSprings(true);
        anim.Play("idle");
        isDead = false;
    }

    public void funRagdoll()
    {
        anim.Play("idle");
        setSprings(false);
        isFunRagdoll = true;
    }

    public void recoverFunRagdoll()
    {
        setSprings(true);
        anim.Play("idle");
        isFunRagdoll = false;
    }

    void setSprings(bool enabled)
    {
        upLegL.useSpring = enabled;
        downLegL.useSpring = enabled;
        upLegR.useSpring = enabled;
        downLegR.useSpring = enabled;
        footL.useSpring = enabled;
        footR.useSpring = enabled;
        downArmL.useSpring = enabled;
        downArmR.useSpring = enabled;
        head.useSpring = enabled;
    }

    [Header("Respawn Settings")]
    public Transform respawnPoint;
}
