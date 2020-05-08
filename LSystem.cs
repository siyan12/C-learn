using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rule
{
    public string before;
    public string after;

    public Rule(string b, string a) { before = b; after = a; }

    public string Before { get;  }
    public string After { get;  }
}

public enum LType { Movement, Tree};

public class LSystem : MonoBehaviour
{
    public LType lType;
    public string instruction;
    public float angle;
    public int iterationTime;
    public bool instantResult = false;
    public Rule[] rules;
    public Transform locator;

    LineRenderer line;
    List<Transform> transforms = new List<Transform>();
    int index = 0;
    string currentLetter;
    Vector4 positionMax;
    int locIndex = 0;
    Vector3 growPosi;
    Quaternion growQua;
    float rotationAngle;
    int indexSpacer = 0;

    private void Start()
    {
        //growPosi = locator.position;
        //growQua = locator.rotation;

        transforms.Add(locator);
        print("currentLetter = " + currentLetter + ", transforms.Count = " + transforms.Count + ", locIndex = " + locIndex);

        line = GetComponent<LineRenderer>();
        line.positionCount = 1;
        line.SetPosition(0, locator.transform.position);

        for(int i=0; i < iterationTime; i++)
        {
            Iteration();
        }

        if (instantResult)
        {
            while(index < instruction.Length)
            {
                RunInstruction();
            }
            UpdateCamera();
        }
    }    

    void Iteration()
    {
        string newString = "";
        for(int i=0; i< instruction.Length; i++)
        {
            bool isFitRule = false;
            foreach(Rule r in rules)
            {
                if (instruction[i].ToString() == r.before)
                {
                    isFitRule = true;
                    newString += r.after;
                    break;
                }
            }
            if (!isFitRule)
            {
                newString += instruction[i];
            }
        }
        instruction = newString;
    }

    private void Update()
    {
        if (instantResult)
            return;

        if (index < instruction.Length)
        {
            RunInstruction();
            UpdateCamera();
        }
    }

    void RunInstruction()
    {
        currentLetter = instruction[index].ToString();
        MovementRules();
        index++;

        line.positionCount++;
        line.SetPosition(index+indexSpacer, locator.transform.position);

        RecordMaxPosition(locator.transform.position);
    }

    void MovementRules()
    {
        if (lType == LType.Movement)
        {
            if (currentLetter == "f" || currentLetter == "F") locator.transform.Translate(0, 1, 0);
            if (currentLetter == "A" || currentLetter == "B") locator.transform.Translate(1, 0, 0);
            if (currentLetter == "+") locator.transform.Rotate(0, 0, angle);
            if (currentLetter == "-") locator.transform.Rotate(0, 0, -angle);
        }
        if (lType == LType.Tree)
        {
            if (currentLetter == "f" || currentLetter == "F")
            {
                //find grow point
                locator.transform.position = growPosi;
                locator.transform.rotation = growQua;

                //rotate
                if (rotationAngle != 0)
                {
                    locator.transform.Rotate(0, 0, rotationAngle);
                    rotationAngle = 0;
                }
                //print("grow at " + locator.position + ". to " + locator.rotation.eulerAngles);

                //set a line point
                line.positionCount++;
                indexSpacer++;
                line.SetPosition(index + indexSpacer, locator.transform.position);

                //grow
                locator.transform.Translate(0, 1, 0);

                //record location
                if (transforms.Count > locIndex + 1)
                    transforms[locIndex] = locator; //replace current record
                else
                    transforms.Add(locator.transform); //new record count++

                locIndex++;
                /*
                while (transforms.Count < locIndex)
                {
                    locIndex--;
                }*/
                print("currentLetter = " + currentLetter + ", transforms.Count = " + transforms.Count + ", locIndex = " + locIndex);
            }
            if (currentLetter == "+") rotationAngle = angle;
            if (currentLetter == "-") rotationAngle = -angle;
            if (currentLetter == "[")
            {
                growPosi = transforms[locIndex].position;
                growQua = transforms[locIndex].rotation;
                //rotationAngle = 0;
                //locIndex++;
                print("currentLetter = " + currentLetter + ", transforms.Count = " + transforms.Count + ", locIndex = " + locIndex);
            }
            if (currentLetter == "]")
            {
                locIndex--;
                //print("locIndex--");
                if (locIndex < 0)
                    locIndex = 0;

                print("currentLetter = " + currentLetter + ", transforms.Count = " + transforms.Count + ", locIndex = " + locIndex);
                //growTrans = transforms[locIndex];
            }

        }
    }

    void RecordMaxPosition(Vector3 v3)
    {
        //x xMin; y xMax; z yMin; w yMax
        
            if (v3.x < positionMax.x)
                positionMax.x = v3.x;
        
            if (v3.x > positionMax.y)
                positionMax.y = v3.x;
        
            if (v3.y < positionMax.z)
                positionMax.z = v3.y;
        
            if (v3.y > positionMax.w)
                positionMax.w = v3.y;      

    }

    void UpdateCamera()
    {
        Camera.main.transform.position = new Vector3
            (
            (positionMax.x + positionMax.y) / 2f, 
            (positionMax.z + positionMax.w) / 2f, 
            Camera.main.transform.position.z
            );

        Camera.main.orthographicSize = (positionMax.w - (positionMax.z + positionMax.w) / 2f) * 1.1f;
    }

}
