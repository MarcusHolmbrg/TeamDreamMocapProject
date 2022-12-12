using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using JetBrains.Annotations;

/*
  Desktop version of script for running the simulation and manipulating certain
  aspects of it. Added functions for controlling playback speed of animation
  (left and right arrow keys), rewinding/playing (up and down arrows keys) and
  for toggling the transparency of the brain and skull on and off (Q key). Some
  additional functions exist in the script but were not working as intended
  before user tests were held and therefore not used.
*/


public class Simulator : MonoBehaviour
{
    public GameObject cathTop, cathTL, cathTR, cathBL, cathBR,
                    skullTL, skullTR, skullBL, skullBR, skullBrow; //references to marker spheres

    public GameObject cathCenter, skullCenter;//references to the barycenters of markers
    public Quaternion cathCenterRot, skullCenterRot;

    private Vector3 cathRight = Vector3.one;
    private Vector3 cathUp = Vector3.one;

    public Text[] FrameStuff;

    float timeToCall;
    float timeDelay = 1.0f; //the code will be run every 2 seconds
    const string separator = "\t"; //tab separation string
    string path = "Assets/Recordings/catheter006.txt"; //path to tsv file
    int index, fileSize; //index to cycle through arrays
    bool readyToUpdate;
    bool paused;
    bool rewind;
    bool forward;
    private float playBackSpeed = 1f;
    public float maxPlaybackSpeed = 50f;
    private float timer = 0;
    private bool transparencyEnabled;

    //arrays with data from each row
    float[] field, time;
    float[,] headTopLeft, headTopRight, headBottomLeft, headBottomRight, headHole, headBrow,
            cathTip, cathTopLeft, cathTopRight, cathBottomLeft, cathBottomRight, cathEnd;

    //coordinates for 3D objects transform update
    private float x1, x2, x3, x4, x5, x6, x7, x8, x9, x10,
        y1, y2, y3, y4, y5, y6, y7, y8, y9, y10,
        z1, z2, z3, z4, z5, z6, z7, z8, z9, z10;

    public Slider slider; //slider to control the animation speed

    //For material transparency
    public GameObject phantomSkull;
    public GameObject phantomBrain;
    private Material solidSkullMat;
    private Material solidBrainMat;

    public Material transparentSkullMat;
    public Material transparentBrainMat;

    //Custom transform coordinates for the skull
    private Dictionary<String, Vector3> skullOffsetPos = new Dictionary<String, Vector3> {
        {"Assets/Recordings/catheter001.txt",new Vector3(-0.939999998f,-14.1099997f,5.55000019f)}, //file : cathether001 NOT WELL ALIGNED
        {"Assets/Recordings/catheter002.txt", new Vector3(-1.88f,-13.8599997f,4.67000008f) }, //file : cathether002 NOT WELL ALIGNED
        {"Assets/Recordings/catheter003.txt", new Vector3(-1.10000002f,-14.1099997f,6.32000017f)}, //file : cathether003
        {"Assets/Recordings/catheter004.txt",new Vector3(-1.28999996f,-13.4799995f,6.07999992f) }, //file : cathether004 NOT WELL ALIGNED
        {"Assets/Recordings/catheter005.txt",new Vector3(-1.08000004f,-13.6199999f,6.5f) }, //file : cathether005
        {"Assets/Recordings/catheter006.txt",new Vector3(-0.639999986f,-12.6899996f,5.57000017f) }, //file : cathether006
        {"Assets/Recordings/catheter007.txt",new Vector3(-0.850000024f,-14.1099997f,5.6500001f) } //file : cathether007
        };
    private Dictionary<String, Vector3> skullOffsetRot = new Dictionary<String, Vector3> {
        {"Assets/Recordings/catheter001.txt",new Vector3(38.116478f,177.862823f,358.404968f)}, //file : cathether001
        {"Assets/Recordings/catheter002.txt", new Vector3(42.3742065f,181.589996f,3.92515182f) }, //file : cathether002
        {"Assets/Recordings/catheter003.txt", new Vector3(43.9130974f,177.666306f,358.909271f) }, //file : cathether003
        {"Assets/Recordings/catheter004.txt",new Vector3(42.3742065f,181.589996f,3.92515182f) }, //file : cathether004
        {"Assets/Recordings/catheter005.txt",new Vector3(42.3742065f,181.589996f,3.92515182f) }, //file : cathether005
        {"Assets/Recordings/catheter006.txt",new Vector3(42.3742104f,181.589996f,5.4209547f) }, //file : cathether006
        {"Assets/Recordings/catheter007.txt",new Vector3(41.510006f,177.755005f,359.040009f) } //file : cathether007
        };

    // Start is called before the first frame update
    void Start()
    {
        //initialize indexes
        index = fileSize = 0;
        readyToUpdate = false;
        paused = false;
        rewind = false;
        forward = true;

        timeToCall = timeDelay;

        StreamReader sr = ReadFile(path); //read from file
        fileSize = FindSize(sr); //find size of file

        //initialize offset for 3dmodels
        cathCenterRot = cathCenter.transform.rotation;
        skullCenterRot = skullCenter.transform.rotation;  //==> Should be hardcoded and set to private once we have the right alignment

        //initialize arrays
        field = time = new float[fileSize];
        headTopLeft = new float[fileSize, 3];
        headTopRight = new float[fileSize, 3];
        headBottomLeft = new float[fileSize, 3];
        headBottomRight = new float[fileSize, 3];
        headHole = new float[fileSize, 3];
        headBrow = new float[fileSize, 3];
        cathTip = new float[fileSize, 3];
        cathTopLeft = new float[fileSize, 3];
        cathTopRight = new float[fileSize, 3];
        cathBottomLeft = new float[fileSize, 3];
        cathBottomRight = new float[fileSize, 3];
        cathEnd = new float[fileSize, 3];

        //extract and distribute info
        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        Extract(sr);
        readyToUpdate = true;

        //close reader
        sr.Close();

        //set offset of skull depending on recording
        skullCenter.gameObject.transform.GetChild(0).transform.localPosition = skullOffsetPos[path];
        skullCenter.gameObject.transform.GetChild(0).transform.localEulerAngles = skullOffsetRot[path];

        //For material transparency
        SetInitialColors();
}

    private void Update()
    {
        // Toggle transparency on and off with the Q key
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleTransparency();
        }
        if (transparencyEnabled)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
               // AdjustTransparency(10);
            }
            if (Input.GetKey(KeyCode.Mouse1))
            {
               //AdjustTransparency(-10);
            }
        }

        // Right arrow key plays the animation in the right direction
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rewind = false;
            forward = true;
        }
        // Left arrow key rewinds the animation
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rewind = true;
            forward = false;
        }

        // Down arrow key reduces playback speed of the animation
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (playBackSpeed > 0f)
            {
                playBackSpeed -= maxPlaybackSpeed / 2f * Time.deltaTime;
            }
            else
            {
                paused = true;
            }

            if (playBackSpeed < 0.1f && !paused)
            {
                playBackSpeed = 0.5f;
            }
        }
        // Up arrow key increases playback speed of the animation
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            if (playBackSpeed < maxPlaybackSpeed)
            {
                if (paused)
                {
                    paused = false;
                }
                playBackSpeed += maxPlaybackSpeed / 2f * Time.deltaTime;
            }
        }
        // The slider visible in the scene displays current playback speed
        if (slider)
        {
            slider.value = playBackSpeed;
        }

        // R key restarts scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            Invoke(nameof(RestartScene), 1f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= timeToCall && MarkerCheck() && fileSize > 0 && readyToUpdate && !paused)
        {
            //normalize positions
            Normalize();

            //update marker positions
            cathTop.transform.position = new Vector3(x1, y1, z1);
            cathTL.transform.position = new Vector3(x2, y2, z2);
            cathTR.transform.position = new Vector3(x3, y3, z3);
            cathBL.transform.position = new Vector3(x4, y4, z4);
            cathBR.transform.position = new Vector3(x5, y5, z5);
            skullTL.transform.position = new Vector3(x6, y6, z6);
            skullTR.transform.position = new Vector3(x7, y7, z7);
            skullBL.transform.position = new Vector3(x8, y8, z8);
            skullBR.transform.position = new Vector3(x9, y9, z9);
            skullBrow.transform.position = new Vector3(x10, y10, z10);

            if (index >= fileSize)
            {
                readyToUpdate = false; //stop simulation if eod is reached
            }

            if (rewind && index > 0)
            {
                index--;
            }

            if (forward)
            {
                index++;
            }

            if (index >= fileSize)
            {
                readyToUpdate = false; //stop simulation if eod is reached
                Invoke(nameof(RestartScene), 1f);
            }
            timer = 0f;
            timeToCall = timeDelay / playBackSpeed;
            AlignModels();

            if (FrameStuff[0])
            {
                FrameStuff[0].text = "Current frame: " + index;
            }
        }
    }

    private void OnApplicationQuit()
    {
        // Debug.Log("Skull model relative pos = " + skullCenter.transform.GetChild(0).localPosition);
        // Debug.Log("Catheter model relative position = "+cathCenter.transform.GetChild(0).localPosition);
    }

    //Method to display the models of catheter and skull according to the markers positions
    private void AlignModels()
    {
        //Align orientation of the catheter
        cathRight = cathTL.transform.position - cathTR.transform.position;
        cathUp = cathTR.transform.position - cathBR.transform.position - Vector3.Project(cathTR.transform.position - cathBR.transform.position, cathRight);
        cathCenter.transform.rotation = Quaternion.LookRotation(cathRight, cathUp);
        //Align positions at the barycenter
        cathCenter.transform.position = new Vector3((x1 + x2 + x3 + x4 + x5) / 5.0f, (y1 + y2 + y3 + y4 + y5) / 5.0f, (z1 + z2 + z3 + z4 + z5) / 5.0f);
        skullCenter.transform.position = new Vector3((x6 + x7 + x8 + x9 + x10) / 5.0f, (y6 + y7 + y8 + y9 + y10) / 5.0f, (z6 + z7 + z8 + z9 + z10) / 5.0f);

    }

    //method to normalize coordinates in Unity scene
    private void Normalize()
    {
        //x coordinate
        x1 = cathTip[index, 0] / 1000.0f;
        x2 = cathTopLeft[index, 0] / 1000.0f;
        x3 = cathTopRight[index, 0] / 1000.0f;
        x4 = cathBottomLeft[index, 0] / 1000.0f;
        x5 = cathBottomRight[index, 0] / 1000.0f;
        x6 = headTopLeft[index, 0] / 1000.0f;
        x7 = headTopRight[index, 0] / 1000.0f;
        x8 = headBottomLeft[index, 0] / 1000.0f;
        x9 = headBottomRight[index, 0] / 1000.0f;
        x10 = headBrow[index, 0] / 1000.0f;

        //y coordinate
        y1 = cathTip[index, 1] / 1000.0f;
        y2 = cathTopLeft[index, 1] / 1000.0f;
        y3 = cathTopRight[index, 1] / 1000.0f;
        y4 = cathBottomLeft[index, 1] / 1000.0f;
        y5 = cathBottomRight[index, 1] / 1000.0f;
        y6 = headTopLeft[index, 1] / 1000.0f;
        y7 = headTopRight[index, 1] / 1000.0f;
        y8 = headBottomLeft[index, 1] / 1000.0f;
        y9 = headBottomRight[index, 1] / 1000.0f;
        y10 = headBrow[index, 1] / 1000.0f;

        //z coordinate
        z1 = cathTip[index, 2] / 1000.0f;
        z2 = cathTopLeft[index, 2] / 1000.0f;
        z3 = cathTopRight[index, 2] / 1000.0f;
        z4 = cathBottomLeft[index, 2] / 1000.0f;
        z5 = cathBottomRight[index, 2] / 1000.0f;
        z6 = headTopLeft[index, 2] / 1000.0f;
        z7 = headTopRight[index, 2] / 1000.0f;
        z8 = headBottomLeft[index, 2] / 1000.0f;
        z9 = headBottomRight[index, 2] / 1000.0f;
        z10 = headBrow[index, 2] / 1000.0f;
    }

    //function to check if objects assigned to markers are not null
    private bool MarkerCheck()
    {
        if (cathTop != null && cathTL != null && cathTR != null && cathBL != null && cathBR != null
            && skullBL != null && skullBR != null && skullTL != null && skullTR != null && skullBrow != null)
            return true;
        else return false;
    }

    //function to read the file with recorded MoCap data
    private StreamReader ReadFile(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line = reader.ReadLine(); //first line = headers
        return reader;
    }

    //function to find the total number of lines in the file being read
    private int FindSize(StreamReader reader)
    {
        int i = 1;
        string line = reader.ReadLine();
        while (line != null)
        {
            i++;
            line = reader.ReadLine();
        }
        return i;
    }

    //method to extract coordinates from the file being read
    private void Extract(StreamReader reader)
    {
        string line;
        for (int i = 0; i < 1; i++)         // change to i<5 for catheter_008
            line = reader.ReadLine(); //skip headers
        line = reader.ReadLine(); //first line

        //extract info and distribute
        while (line != null && line != "") //interrupt at empty line or end of file
        {
            string[] temp = line.Split(separator.ToCharArray());
            //string[] temp = line.Split("\t");
            int runtimeField = Int32.Parse(temp[0]); //current array id

           // Debug.Log(runtimeField);

            //populate arrays
            field[runtimeField] = runtimeField + 1.0f;
            time[runtimeField] = runtimeField / 100.0f;


            //marker tree attached to the skull

            //float test = float.Parse(temp[2] + 36f);
            //Debug.Log(temp[2]);

            headTopLeft[runtimeField, 0] = float.Parse(temp[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 1 x
            headTopLeft[runtimeField, 1] = float.Parse(temp[4], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 1 y
            headTopLeft[runtimeField, 2] = float.Parse(temp[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 1 z
            headTopRight[runtimeField, 0] = float.Parse(temp[5], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 2 x
            headTopRight[runtimeField, 1] = float.Parse(temp[7], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 2 y
            headTopRight[runtimeField, 2] = float.Parse(temp[6], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 2 z
            headBottomLeft[runtimeField, 0] = float.Parse(temp[8], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 3 x
            headBottomLeft[runtimeField, 1] = float.Parse(temp[10], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 3 y
            headBottomLeft[runtimeField, 2] = float.Parse(temp[9], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 3 z
            headBottomRight[runtimeField, 0] = float.Parse(temp[11], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 4 x
            headBottomRight[runtimeField, 1] = float.Parse(temp[13], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 4 y
            headBottomRight[runtimeField, 2] = float.Parse(temp[12], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull 4 z

            /* calibration marker on burr hole is always 0
            headHole[runtimeField, 0] = float.Parse(temp[14]); //burr hole x
            headHole[runtimeField, 1] = float.Parse(temp[16]); //burr hole y
            headHole[runtimeField, 2] = float.Parse(temp[15]); //burr hole z
            */

            //marker attached to the skull brow
            /* headBrow[runtimeField, 0] = float.Parse(temp[17], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow x
            headBrow[runtimeField, 1] = float.Parse(temp[19], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow y
            headBrow[runtimeField, 2] = float.Parse(temp[18], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow z

            //marker tree attached to the catheter
            cathTip[runtimeField, 0] = float.Parse(temp[20], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 x
            cathTip[runtimeField, 1] = float.Parse(temp[22], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 y
            cathTip[runtimeField, 2] = float.Parse(temp[21], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 z
            cathTopLeft[runtimeField, 0] = float.Parse(temp[23], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 x
            cathTopLeft[runtimeField, 1] = float.Parse(temp[25], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 y
            cathTopLeft[runtimeField, 2] = float.Parse(temp[24], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 z
            cathTopRight[runtimeField, 0] = float.Parse(temp[26], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 x
            cathTopRight[runtimeField, 1] = float.Parse(temp[28], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 y
            cathTopRight[runtimeField, 2] = float.Parse(temp[27], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 z
            cathBottomLeft[runtimeField, 0] = float.Parse(temp[29], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 x
            cathBottomLeft[runtimeField, 1] = float.Parse(temp[31], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 y
            cathBottomLeft[runtimeField, 2] = float.Parse(temp[30], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 z
            cathBottomRight[runtimeField, 0] = float.Parse(temp[32], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 x
            cathBottomRight[runtimeField, 1] = float.Parse(temp[34], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 y
            cathBottomRight[runtimeField, 2] = float.Parse(temp[33], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 z
            */
            headBrow[runtimeField, 0] = float.Parse(temp[14], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow x
            headBrow[runtimeField, 1] = float.Parse(temp[16], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow y
            headBrow[runtimeField, 2] = float.Parse(temp[15], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //skull brow z

            //marker tree attached to the catheter
            cathTip[runtimeField, 0] = float.Parse(temp[17], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 x
            cathTip[runtimeField, 1] = float.Parse(temp[19], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 y
            cathTip[runtimeField, 2] = float.Parse(temp[18], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 1 z
            cathTopLeft[runtimeField, 0] = float.Parse(temp[20], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 x
            cathTopLeft[runtimeField, 1] = float.Parse(temp[22], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 y
            cathTopLeft[runtimeField, 2] = float.Parse(temp[21], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 2 z
            cathTopRight[runtimeField, 0] = float.Parse(temp[23], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 x
            cathTopRight[runtimeField, 1] = float.Parse(temp[25], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 y
            cathTopRight[runtimeField, 2] = float.Parse(temp[24], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 3 z
            cathBottomLeft[runtimeField, 0] = float.Parse(temp[26], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 x
            cathBottomLeft[runtimeField, 1] = float.Parse(temp[28], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 y
            cathBottomLeft[runtimeField, 2] = float.Parse(temp[27], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 4 z
            cathBottomRight[runtimeField, 0] = float.Parse(temp[29], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 x
            cathBottomRight[runtimeField, 1] = float.Parse(temp[31], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 y
            cathBottomRight[runtimeField, 2] = float.Parse(temp[30], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); //catheter 5 z

            /* calibration marker on catheter tip is always 0
            cathEnd[runtimeField, 0] = float.Parse(temp[32]); //catheter tip x
            cathEnd[runtimeField, 1] = float.Parse(temp[34]); //catheter tip y
            cathEnd[runtimeField, 2] = float.Parse(temp[33]); //catheter tip z
            */

            line = reader.ReadLine();
        }
    }


    private void RestartScene()
    {
        Debug.Log("Restart");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        index = 0;
        readyToUpdate = true;
        timer = 0;
    }

    private void ToggleTransparency()
    {
        Renderer skullRenderer = phantomSkull.GetComponent<Renderer>();
        if (!transparencyEnabled)
        {
            SkullTransparent();
            BrainTransparent();
            transparencyEnabled = true;
        }
        else if (transparencyEnabled)
        {
            SkullSolid();
            BrainSolid();
            transparencyEnabled = false;
        }
    }
    private void SetInitialColors()
    {
        Renderer skullRenderer = phantomSkull.GetComponent<Renderer>();
        solidSkullMat = skullRenderer.material;
        Renderer brainRenderer = phantomBrain.GetComponent<Renderer>();
        solidBrainMat = brainRenderer.material;
    }
    private void SkullTransparent()
    {
        Renderer skullRenderer = phantomSkull.GetComponent<Renderer>();
        skullRenderer.material = transparentSkullMat;
    }
    private void BrainTransparent()
    {
        Renderer brainRenderer = phantomBrain.GetComponent<Renderer>();
        brainRenderer.material = transparentBrainMat;
    }
    private void AdjustTransparency(float val)
    {
        Renderer skullRenderer = phantomSkull.GetComponent<Renderer>();
        Color newSkullColor = skullRenderer.material.color;

        //Debug.Log(newSkullColor.a + " | " + 255 + "-" + (val + 1) + " | " + (newSkullColor.a > (val + 1)) + (newSkullColor.a < 255 - (val + 1)));

        if(newSkullColor.a > (val + 1) && newSkullColor.a < 255 - (val+1))
        {
            newSkullColor.a += (int)val * Time.deltaTime;
        }
        skullRenderer.material.color = newSkullColor;
        transparentSkullMat = skullRenderer.material;

        Renderer brainRenderer = phantomBrain.GetComponent<Renderer>();
        Color newBrainColor = brainRenderer.material.color;

        if (newBrainColor.a > (val + 1) && newBrainColor.a < 255 - (val + 1))
        {
            newBrainColor.a += (int)val * Time.deltaTime;
        }
        brainRenderer.material.color = newBrainColor;
        //Debug.Log(skullRenderer.material.color + " | " + brainRenderer.material.color);

        transparentBrainMat = brainRenderer.material;

    }

    private void SkullSolid()
    {
        Renderer skullRenderer = phantomSkull.GetComponent<Renderer>();
        skullRenderer.material = solidSkullMat;
    }


    private void BrainSolid()
    {
        Renderer brainRenderer = phantomBrain.GetComponent<Renderer>();
        brainRenderer.material = solidBrainMat;
    }
    public int GetCurrentIndex()
    {
        return index;
    }
}
