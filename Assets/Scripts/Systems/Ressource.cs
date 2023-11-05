using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private GameObject m_connector;
    public List<Ressource> m_fusedNodes = new();

    [HideInInspector] public bool m_isGrabbed = false;
    private bool m_skip = false;
    private List<GameObject> m_connectors = new();
    private float lerpTimer = 0f;
    private Vector3 previousTrackPos;
    private Vector3 nextTrackPos;
    //checks recursively through the entire structure of fused connections to check if there already is one that's grabbed
    public void TryGetInvalidNodes(HashSet<Ressource> alreadyChecked, List<Ressource> problematicObjects, Ressource OriginNode)
    {
        foreach (Ressource connected in OriginNode.m_fusedNodes)
        {
            if (!alreadyChecked.Contains(connected))
            {
                alreadyChecked.Add(connected);
                if (connected.m_isGrabbed)
                {
                    problematicObjects.Add(connected);
                }
                connected.TryGetInvalidNodes(alreadyChecked, problematicObjects, connected);
            }
        }
    }
    public bool CanBeGrabbed
    {
        get
        {
            if (m_fusedNodes.Count > 0)
            {
                HashSet<Ressource> alreadyChecked = new(m_fusedNodes);
                List<Ressource> problematicObjects = m_fusedNodes.Where(x => x.m_isGrabbed).ToList();
                foreach (Ressource connected in m_fusedNodes)
                {
                    TryGetInvalidNodes(alreadyChecked, problematicObjects, connected);

                }
                if (problematicObjects.Count > 0)
                {
                    problematicObjects.Add(this);
                    ErrorManager.instance.RegisterMultiGrabException(problematicObjects);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (m_isGrabbed)
                {
                    ErrorManager.instance.RegisterMultiGrabException(new List<Ressource> { this });
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
    }
    public void TryFuse(Ressource res2)
    {
        //fuse the 2 ressources
        if (!m_fusedNodes.Contains(res2) && transform.childCount <= 0)
        {
            m_fusedNodes.Add(res2);
            res2.m_fusedNodes.Add(this);

            float angle = Mathf.Atan2(transform.position.x - res2.transform.position.x, transform.position.y - res2.transform.position.y) * Mathf.Rad2Deg;
            GameObject connector = Instantiate(m_connector, transform.position, Quaternion.Euler(0, 0, angle), transform);
            m_connectors.Add(connector);
            res2.m_connectors.Add(connector);

        }
    }
    public void RearrangeFusionHierarchy()
    {
        if (m_fusedNodes.Count > 0) //rearrange hierarchy so that the entire fused thing pivots around this now
        {
            foreach (Ressource _toReparent in m_fusedNodes)
            {
                _toReparent.transform.parent = transform;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ExecutionControls.instance.m_isPlaying)
        {
            ObjectDraggable comp = collision.collider.GetComponent<ObjectDraggable>();
            if (collision.collider.CompareTag("ArmGrabbable") || comp != null && comp.m_arm != null)
            {
                ErrorManager.instance.RegisterCollisionException(gameObject, collision.gameObject);
            }
        }

    }
    private IEnumerator MoveOnTrack(Track track)
    {
        previousTrackPos = track.transform.parent.position;
        nextTrackPos = track.GetNextTrackPos();
        
        while(!m_isGrabbed)
        {
            if (!ExecutionControls.instance.m_isPaused)
            {
                transform.position = Vector3.Lerp(previousTrackPos, nextTrackPos, lerpTimer);
                lerpTimer += Time.deltaTime / ActionExecutor.s_cycleDuration;
                lerpTimer = Mathf.Clamp01(lerpTimer);
                if ((!ExecutionControls.instance.m_stepByStep || ExecutionControls.instance.m_nextStep) && lerpTimer >= 1 || m_skip)
                {
                    if(track.m_nextTrack != null)
                    {
                        previousTrackPos = nextTrackPos;
                        track = track.m_nextTrack;
                        nextTrackPos = track.GetNextTrackPos();
                        lerpTimer = 0f;
                        m_skip = false;
                    }
                    else
                    {
                        ExecutionControls.instance.SkipToNextTrackEvent.RemoveListener(SkipToNextConveyor);
                        yield break;
                    }
                }
            } 
            yield return null;
        }
    }
    public void TryGetRecipient()
    {
        var HitInfo = Physics2D.Raycast(transform.position + Vector3.back * 5, Vector3.forward, 50f, LayerMask.GetMask("Tracks"));
        if(HitInfo.collider != null)
        {
            ExecutionControls.instance.SkipToNextTrackEvent.AddListener(SkipToNextConveyor);
            StartCoroutine(MoveOnTrack(HitInfo.collider.transform.parent.GetChild(0).GetComponent<Track>()));
        }
        /*HitInfo = Physics2D.Raycast(transform.position + Vector3.back * 5, Vector3.forward, 50f, LayerMask.GetMask("Validator"));
        if (HitInfo.collider != null)
        {
            HitInfo.collider.transform.parent.GetChild(0).GetComponent<Validator>().TryValidate(this, m_fusedNodes);
        }*/
    }
    private void SkipToNextConveyor()
    {
        m_skip = true;
        transform.position = nextTrackPos;
    }
}
