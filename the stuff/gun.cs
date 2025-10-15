using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gun : MonoBehaviour
{
    public float timeBetweenShots;
    public bool oneShotAtATime;
    public bool active;
    public Transform shootPos;
    private bool ready;
    public GameObject line;
    public Rigidbody rb;
    public Collider colli;
    public Camera cam;
    public Image aimer;
    public Material lineMaterial;
    public bool enemyGun;

    [Header("Bullet Mark Settings")]
    public GameObject bulletMarkPrefab;  // small quad/plane with bullet hole texture
    public float markOffset = 0.01f;      // offset so mark is not inside wall
    public float markLifetime = 10f;      // destroy mark after this time

    void Start()
    {
        ready = true;
    }

    void Update()
    {
        // Optional: trigger shoot here if using mouse/keyboard
        // if (Input.GetMouseButtonDown(0)) shoot(null);
    }

    public void shoot(Transform target)
    {
        if (ready && active)
        {
            if (!oneShotAtATime)
            {
                StartCoroutine(wait());
            }

            RaycastHit hit;
            Ray vRay = cam.ScreenPointToRay(aimer.transform.position);

            if (Physics.Raycast(vRay, out hit))
            {
                // Kill player if hitting layer 8 or 9
                if (hit.transform.gameObject.layer == 9 || hit.transform.gameObject.layer == 8)
                {
                    var deathComp = hit.transform.parent.GetComponent<death>();
                    if (deathComp != null)
                        deathComp.die();
                }

                // Apply force if rigidbody
                Rigidbody hitRb = hit.transform.GetComponent<Rigidbody>();
                if (hitRb)
                {
                    hitRb.AddForce(cam.transform.forward * 50, ForceMode.Impulse);
                }

                // Spawn bullet mark
                if (bulletMarkPrefab != null)
                {
                    // Rotate mark to be flat against surface
                    Quaternion rotation = Quaternion.LookRotation(hit.normal) * Quaternion.Euler(90f, 0f, 0f);

                    GameObject mark = Instantiate(
                        bulletMarkPrefab,
                        hit.point + hit.normal * markOffset,
                        rotation
                    );

                    // Parent to hit object so it moves with it
                    mark.transform.SetParent(hit.collider.transform);

                    // Optional: destroy after some time
                    Destroy(mark, markLifetime);
                }

                // Draw laser/line
                StartCoroutine(makeLine(hit));
            }
        }
    }

    public void Activate(Transform hand)
    {
        active = true;
        rb.isKinematic = true;
        colli.enabled = false;

        transform.parent = hand;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    public void DeActivate()
    {
        active = false;
        transform.parent = null;

        rb.isKinematic = false;
        colli.enabled = true;
    }

    public IEnumerator wait()
    {
        ready = false;
        yield return new WaitForSeconds(timeBetweenShots);
        ready = true;
    }

    public IEnumerator makeLine(RaycastHit hitplace)
    {
        var go = new GameObject();
        var lr = go.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.SetPosition(0, shootPos.position);
        lr.SetPosition(1, hitplace.point);
        lr.startColor = Color.red;
        lr.endColor = Color.white;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        yield return new WaitForSeconds(0.07f);
        Destroy(go);
    }
}
