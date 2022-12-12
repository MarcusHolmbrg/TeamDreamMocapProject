using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
  Script which enables annotation of points in both VR and desktop setting.

  In VR, annotation is done by holding down the trigger on the back of the right
  controller. This makes a laser shoot out of the controller which is used to
  aim at the point to be annotated. When the trigger is released, the point
  aimed at by the laser is annotated and a red or green marker (a sphere) is
  placed at that point.

  On desktop, annotation is done by holding down the spacebar key. This makes a
  laser shoot out of from the observer which is used to assist in aiming at the
  point to be annotated. The aiming itself is done by moving the mouse. When the
  spacebar key is released, the point is annotated and a red or green marker (a
  sphere) is placed at that point.

  For both settings, the following applies:
  The points have to be annotated in the following order: beginning of
  insertion, end of insertion, beginning of extraction, end of extraction. This
  is because of how collision detection is currently setup. For the first and
  fourth annotations, the laser collides with the outer surface of the brain.
  For the second and third annotations, the laser first penetrates the brain and
  only collides with the ventricles. This setup was chosen based on our
  definitions of the points that were to be annotated.
*/

public class AnnotationScript : MonoBehaviour
{
    public bool isDesktopVersion = true; //False if VR version
    public Transform LineOrigin;
    public LineRenderer desktopLineRenderer = null;
    public int annotationIndex;
    public int[] annotationPoints;
    public List<GameObject> annotationMarkers;
    private bool laserActive;
    private Vector3 annotationPoint;

    private Simulator deskTopSim = null;
    private VRSimulator vrSim = null;

    public GameObject AnnotationPoint1;
    public GameObject AnnotationPoint2;

    private MeshCollider BrainCollider;

    // Start is called before the first frame update
    void Start()
    {
        if (isDesktopVersion)
        {
            desktopLineRenderer = GetComponentInChildren<LineRenderer>();
            deskTopSim = GameObject.FindGameObjectWithTag("Simulator").GetComponent<Simulator>();
        }
        else
        {
            vrSim = GameObject.FindGameObjectWithTag("VRSimulator").GetComponent<VRSimulator>();
        }

        BrainCollider = GameObject.Find("Brain").GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDesktopVersion)
        {
            // Spacebar key used for annotation
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitiateAnnotation();
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ExecuteAnnotation();
            }
        }
        else
        {
            //VR CONTROLLER ANNOTATION INPUT
        }

        if (Input.GetKeyDown(KeyCode.Return) && annotationIndex > 0)
        {
            annotationPoints[annotationIndex] = 0;
            annotationIndex -= 1;
            annotationMarkers.RemoveAt(annotationMarkers.Count - 1);
        }


        if (laserActive)
        {
            RaycastHit hit;

            if (Physics.Raycast(LineOrigin.position, LineOrigin.forward, out hit))
            {
                annotationPoint = hit.point;

                desktopLineRenderer.SetPosition(0, LineOrigin.position);
                desktopLineRenderer.SetPosition(desktopLineRenderer.positionCount - 1, hit.point);
                if (annotationIndex.Equals(0))
                {
                    AnnotationPoint1.transform.position = hit.point;
                }
                if (annotationIndex.Equals(1))
                {
                    AnnotationPoint2.transform.position = hit.point;
                }
            }

        }
    }
    private void InitiateAnnotation()
    {

        if (annotationIndex.Equals(0))
        {
            AnnotationPoint1.SetActive(true);
        }

        if (annotationIndex.Equals(1))
        {
            AnnotationPoint2.SetActive(true);
        }

        if (isDesktopVersion)
        {
            desktopLineRenderer.enabled = true;
        }
        else
        {

        }
        laserActive = true;
    }

    private void ExecuteAnnotation()
    {
        Debug.Log("Annotated point " + annotationIndex + "| Position: " + annotationPoint.x  + " " + annotationPoint.y + " "+ annotationPoint.z +"| Time: " + deskTopSim.GetCurrentIndex()) ;
        if (isDesktopVersion)
        {
            desktopLineRenderer.enabled = false;
        }
        else
        {

        }
        laserActive = false;
        annotationIndex += 1;
        if (annotationIndex == 1 || annotationIndex == 2)
        {
            BrainCollider.enabled = false;
            AnnotationPoint1.SetActive(false);
            AnnotationPoint2.SetActive(false);
            annotationMarkers.Add(Instantiate(AnnotationPoint1, annotationPoint, Quaternion.identity));
        }
        else
        {
            BrainCollider.enabled = true;
            AnnotationPoint2.SetActive(false);
            annotationMarkers.Add(Instantiate(AnnotationPoint2, annotationPoint, Quaternion.identity));
        }
        annotationMarkers[annotationMarkers.Count - 1].SetActive(true);
    }
}
