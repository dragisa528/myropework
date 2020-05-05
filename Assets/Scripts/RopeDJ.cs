using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RopeDJ : MonoBehaviour
{
    public Rigidbody2D  connectedObject;
    public LayerMask    collisionMask;
    public float        collisionTolerance = 1;
    public float        thickness = 0;

    class Joint
    {
        public DistanceJoint2D  dj;
        public FixedJoint2D     fj;
    }

    List<Joint>     joints;
    LineRenderer    lineRenderer;

    private void Awake()
    {
        var children = GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child != transform)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
    }

    void Start()
    {
        joints = new List<Joint>();

        CreateNewJoint(transform.position);

        lineRenderer = GetComponent<LineRenderer>();
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) return;
        if (UnityEditor.EditorApplication.isCompiling) return;
#endif
        CreateNewLink();
    }

    private void Update()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer)
        {
            if ((joints == null) || (joints.Count == 0))
            {
                if (connectedObject)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, connectedObject.transform.position);
                }
            }
            else
            {
#if UNITY_EDITOR
                joints.RemoveAll((x) => (x.dj == null));
#endif

                lineRenderer.positionCount = joints.Count + 1;

                lineRenderer.SetPosition(0, transform.position);

                for (int i = 0; i < joints.Count; i++)
                {
                    lineRenderer.SetPosition(i + 1, joints[i].dj.connectedBody.position);
                }
            }
        }
    }

    void CreateNewLink()
    {
        Joint j = joints[joints.Count - 1];

        // See if we have to break the segment 
        Vector2 startPos = j.dj.transform.position;
        Vector2 endPos = j.dj.connectedBody.position;

        var hits = GetHits(startPos, endPos);

        foreach (var hit in hits)
        {
            Vector2 hookPos = GetHookPosition(j.dj.transform.position);

            j.dj.enabled = false;
            j.fj = j.dj.gameObject.AddComponent<FixedJoint2D>();
            j.fj.connectedBody = connectedObject;

            var newRB = CreateNewJoint(hookPos);
            j.dj.connectedBody = newRB;
            j.fj.connectedBody = newRB;

            return;
        }

        if (joints.Count > 1)
        {
            // See if we can remove this link
            Vector3 prevLinkPos = joints[joints.Count - 2].dj.transform.position;
            Vector3 currentLinkPos = joints[joints.Count - 1].dj.transform.position;
            Vector3 objectPos = connectedObject.transform.position;
            // Check if we can "see" the previous link start
            startPos = prevLinkPos;

            hits = GetHits(startPos, objectPos);

            if (hits.Count > 0) return;

            // Check for colinearity between this link and the previous
            Vector2 dir1 = currentLinkPos - prevLinkPos;
            Vector2 dir2 = objectPos - currentLinkPos;

            if (Vector2.Dot(dir1.normalized, dir2.normalized) < 0.9f) return;

            /*            endPos = joints[joints.Count - 1].dj.transform.position;

                        hits = GetHits(startPos, endPos);

                        if (hits.Count > 0) return;*/

            // We can destroy this segment
            Destroy(j.dj.gameObject);
            joints.Remove(j);

            // Reactivate previous link
            j = joints[joints.Count - 1];

            Destroy(j.fj); j.fj = null;
            j.dj.enabled = true;
            j.dj.connectedBody = connectedObject;
        }
    }

    Vector3 GetHookPosition(Vector2 startPos)
    {
        // Find position, raycast from object to position
        Vector2         dir = startPos - connectedObject.position;
        float           distance = dir.magnitude;
        Vector2         rayStart;
        RaycastHit2D[]  hits;

        if (thickness <= 0.0f)
        {
            rayStart = connectedObject.position;
            hits = Physics2D.RaycastAll(rayStart, dir, distance, collisionMask);
        }
        else
        {
            rayStart = connectedObject.position + dir.normalized * thickness;
            hits = Physics2D.CircleCastAll(rayStart, thickness, dir, distance - 2 * thickness, collisionMask);
        }

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != connectedObject.gameObject)
            {
                return rayStart + hit.distance * dir.normalized;
            }
        }

        return startPos;
    }

    List<RaycastHit2D> GetHits(Vector2 startPos, Vector2 endPos)
    {
        List<RaycastHit2D>  ret = new List<RaycastHit2D>();
        Vector2             dir = endPos - startPos;
        float               distance = dir.magnitude;
        RaycastHit2D[]      hits;

        if (thickness <= 0.0f)
        {
            hits = Physics2D.RaycastAll(startPos + dir.normalized * collisionTolerance, dir, distance - 2 * collisionTolerance, collisionMask);
        }
        else
        {
            hits = Physics2D.CircleCastAll(startPos + dir.normalized * (collisionTolerance + thickness), thickness, dir, distance - 2 * (collisionTolerance + thickness), collisionMask);
        }

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject != connectedObject.gameObject)
            {
                ret.Add(hit);
            }
        }

        return ret;
    }

    Rigidbody2D CreateNewJoint(Vector2 position)
    {
        // Create new joint
        Joint newJoint = new Joint();

        var go = new GameObject();
        go.name = "RopeElem";
        go.transform.parent = transform;
        go.transform.position = position;
        newJoint.dj = go.AddComponent<DistanceJoint2D>();
        newJoint.dj.connectedBody = connectedObject;
        newJoint.fj = null;
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Static;

        joints.Add(newJoint);

        return rb;
    }

    private void OnDrawGizmosSelected()
    {
        if (joints != null)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                var j = joints[i];
                if (i == joints.Count - 1) Gizmos.color = Color.green;
                else Gizmos.color = Color.yellow;

                Gizmos.DrawLine(j.dj.transform.position, j.dj.connectedBody.transform.position);
                if (thickness > 0.0f)
                {
                    Gizmos.DrawWireSphere(j.dj.transform.position, thickness);
                }
            }
        }
        else
        {
            if (connectedObject)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, connectedObject.transform.position);

                if (thickness > 0.0f)
                {
                    Gizmos.DrawWireSphere(transform.position, thickness);
                    Gizmos.DrawWireSphere(connectedObject.transform.position, thickness);
                }
            }
        }
    }
}
