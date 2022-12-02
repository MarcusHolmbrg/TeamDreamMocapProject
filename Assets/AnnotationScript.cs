using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private InputActionReference annotatePoint = null;

    private Simulator deskTopSim = null;
    private VRSimulator vrSim = null;

    public GameObject AnnotationPoint1;
    public GameObject AnnotationPoint2;

    private MeshCollider BrainCollider;
    private bool annotationInProgress = false;
    private bool annotationSet = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if(isDesktopVersion)
        {
            desktopLineRenderer = GetComponentInChildren<LineRenderer>();
            deskTopSim = GameObject.FindGameObjectWithTag("Simulator").GetComponent<Simulator>();
        }
        else
        {
            desktopLineRenderer = GetComponent<LineRenderer>();
            vrSim = GameObject.FindGameObjectWithTag("VRSimulator").GetComponent<VRSimulator>();
        }

        BrainCollider = GameObject.Find("Brain").GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDesktopVersion)
        {
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
            float toggleVal = annotatePoint.action.ReadValue<float>();
            Debug.Log(toggleVal);
            if (toggleVal > 0.8f && !annotationInProgress)
            {
                //Debug.Log(toggleVal);
                annotationInProgress = true;
                InitiateAnnotation();
            }
            else if (toggleVal < 0.1f && annotationInProgress)
            {
                ExecuteAnnotation();
                annotationInProgress = false;
            }
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

            if(Physics.Raycast(LineOrigin.position, LineOrigin.forward, out hit))
            {
                annotationPoint = hit.point;

                //Debug.Log("itsCtiv");
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
        Debug.Log("Active");
        if(annotationIndex.Equals(0))
        {
            AnnotationPoint1.SetActive(true);
        }
        
        if(annotationIndex.Equals(1))
        {
            AnnotationPoint2.SetActive(true);
        }

        if (isDesktopVersion)
        {
            desktopLineRenderer.enabled = true;
            
        }
        else
        {
            desktopLineRenderer.enabled = true;
        }
        laserActive = true;
    }

    private void ExecuteAnnotation()
    {
        Debug.Log("Execute");
        if (isDesktopVersion)
        {
            Debug.Log("Annotated point " + annotationIndex + 1 + "| Position: " + annotationPoint + "| Time: " + deskTopSim.GetCurrentIndex());
            desktopLineRenderer.enabled = false;
        }
        else
        {
            Debug.Log("Annotated point " + annotationIndex + 1 + "| Position: " + annotationPoint + "| Time: " + vrSim.GetCurrentIndex());
        }
        laserActive = false;
        annotationIndex += 1;
        if(annotationIndex == 1)
        {
            BrainCollider.enabled = false;
            AnnotationPoint1.SetActive(false);
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
