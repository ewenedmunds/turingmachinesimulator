using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Rule {
    public string readState;
    public char readChar;
    public string finalState;
    public char finalChar;
    public char finalMove;

    public Rule(string rS, char rC, string fS, char fC, char fM)
    {
        this.readState = rS;
        this.readChar = rC;
        this.finalState = fS;
        this.finalChar = fC;
        this.finalMove = fM;
    }
}

public class TapeScript : MonoBehaviour
{
    public Rule[] rules;

    private bool isRunning = false;

    public TextMeshProUGUI[] ruleBasis;
    public TextMeshProUGUI inputWord;
    public TextMeshProUGUI status;
    public TextMeshProUGUI stateStatus;

    public Slider slider;
    public Slider tapeSlider;

    private float internalTimer = 0f;
    private float stepDelay = 0.2f;
    public float StepDelay
    {
        get { return stepDelay; }
        set { stepDelay = Mathf.Max(value, 0.001f); }
    }

    private int tapeHeadLocation;
    private string tapeContents;
    private string state;

    private int maxCells = 100;

    public GameObject tapeCell;
    public GameObject head;
    private GameObject tapeHead;

    public GameObject ruleHolder;
    public GameObject rule;

    public Animator ruleAddedAnimator;

    // Start is called before the first frame update
    void Start()
    {
        //Create tape cells, start the head in the right place
        for (int i = 0; i < maxCells; i++)
        {
            GameObject newCell = Instantiate(tapeCell,transform.GetChild(0));
            newCell.transform.position = new Vector3(( Screen.width/ 20)*(i+1), transform.GetChild(0).transform.position.y, 0);
        }
        tapeHead = Instantiate(head,transform);
        tapeHead.transform.position = transform.GetChild(0).GetChild(1).transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            internalTimer -= Time.deltaTime;
            if (internalTimer <= 0)
            {
                internalTimer = StepDelay;
                TapeStep();
            }
        }
    }


    //Read state, char combo, try and move on
    public void TapeStep()
    {
        char readChar = transform.GetChild(0).GetChild(tapeHeadLocation).GetChild(1).GetComponent<TextMeshProUGUI>().text[0];

        bool flag = false;

        //Find a matching rule for the given state and char combo
        foreach(Rule rule in rules)
        {
            if (rule.readState.Substring(0,rule.readState.Length-1) == state && rule.readChar == readChar)
            {
                flag = true;
                state = rule.finalState.Substring(0, rule.finalState.Length - 1);
                SetStatus("State: " + state);

                //IF we reach a finishing state, stop
                if (state == "qacc" || state == "qrej")
                {
                    isRunning = false;
                }

                transform.GetChild(0).GetChild(tapeHeadLocation).GetChild(1).GetComponent<TextMeshProUGUI>().text = rule.finalChar.ToString();

                if (rule.finalMove == 'R' || rule.finalMove == 'r')
                {
                    MoveMarker(tapeHeadLocation += 1);
                }
                else
                {
                    MoveMarker(tapeHeadLocation -= 1);
                }

                break;
            }
        }

        if (flag == false)
        {
            isRunning = false;
            Debug.Log("No applicable rules to this situation!");
            stateStatus.text = "No applicable rules!";
        }
    }

    public void MoveMarker(int newPos)
    {
        if (newPos <= 100 && newPos >= 0)
        {
            tapeHeadLocation = newPos;

            tapeHead.transform.position = transform.GetChild(0).GetChild(tapeHeadLocation).transform.position;

        }
    }


    //Start running the TM from button press
    public void SimulateButton()
    {
        Simulate(inputWord.text);
    }

    //Set conditions for simulating the TM
    public void Simulate(string word)
    {
        stateStatus.text = "";
        tapeContents = '>' + word;
        for (int i = 0; i < maxCells; i++)
        {
            transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = " ";
        }
        for (int i = 0; i < tapeContents.Length; i++)
        {
            transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = tapeContents[i].ToString();
        }
        MoveMarker(1);
        isRunning = true;
        state = "q0";

        SetStatus("State: q0");
    }


    public void SetStatus(string newStatus)
    {
        status.text = newStatus;
    }

    //Add a new transition function rule
    public void AddRule()
    {
        if (isRunning)
        {
            Debug.Log("Can't add the rule right now!");
        }
        else
        {
            Rule[] newRules = new Rule[rules.Length + 1];
            for (int i = 0; i < rules.Length; i++)
            {
                newRules[i] = rules[i];
            }

            //Construct the new rule
            bool flag = true;

            string rS = ruleBasis[0].text;

            char rC = ' ';
            if (ruleBasis[1].text.Length > 0)// && ruleBasis[1].text[0] != ' ')
            {
                 rC = ruleBasis[1].text[0];
            }
            else
            {
                flag = false;
                AddRuleErrorMessage("Enter a proper character to be read!");
            }

            string fS = ruleBasis[2].text;

            char fC = ' ';
            if (ruleBasis[3].text.Length > 0)
            {
                fC = ruleBasis[3].text[0];
            }
            else
            {
                 fC = ' ';
            }

            char fM = ' ';
            if (ruleBasis[4].text.Length > 0)
            {
                fM = ruleBasis[4].text[0];
            }
            else
            {
                flag = false;
                AddRuleErrorMessage("Enter a final move character (L or R)!");
            }


            if (flag == true)
            {
                if (rS.Length <= 0)
                {
                    flag = false;
                    AddRuleErrorMessage("Enter a state to be read!");
                }
                if (fS.Length <= 0)
                {
                    flag = false;
                    AddRuleErrorMessage("Enter a final state to be entered!");
                }
                if (fM != 'R' && fM != 'r' && fM != 'L' && fM != 'l')
                {
                    flag = false;
                    AddRuleErrorMessage("Enter 'R' or 'L' for the final move!");
                }

                if (flag)
                {
                    newRules[rules.Length] = new Rule(rS, rC, fS, fC, fM);

                    rules = newRules;

                    ruleAddedAnimator.gameObject.GetComponent<TextMeshProUGUI>().text = "Rule Added!";

                }
                ruleAddedAnimator.Play("RuleAdded");
            }

            UpdateDisplay();
        }
    }

    public void RemoveRule(int ruleIndex)
    {
        Rule[] newRules = new Rule[rules.Length - 1];
        int count = 0;
        for (int i = 0; i < rules.Length; i++)
        {
            if (i != ruleIndex)
            {
                newRules[count] = rules[i];
                count += 1;
            }
        }

        rules = newRules;

        UpdateDisplay();
    }

    //Updates display of all added rules
    public void UpdateDisplay()
    {
        int total = ruleHolder.transform.childCount;
        for (int i = 0; i < total; i++)
        {
            DestroyImmediate(ruleHolder.transform.GetChild(0).gameObject);
        }

        int count = 0;

        foreach(Rule r in rules)
        {
            GameObject newRule = Instantiate(rule, ruleHolder.transform);
            newRule.transform.GetChild(1).GetComponent<RuleScript>().ruleIndex = count;
            newRule.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = r.readState + " " + r.readChar.ToString() + " " + r.finalState + " " + r.finalChar.ToString() + " " + r.finalMove.ToString();
            newRule.transform.position = new Vector3(1200, (count+1) * 150);
            count += 1;
                
        }

        slider.value = 0;
        slider.maxValue = Mathf.Max(0,count - 1);

        SetRulePosition(-1);
    }

    public void SetRulePosition(float value)
    {
        if (value <= -1)
        {
            value = slider.value;
        }
        ruleHolder.transform.position = new Vector3(ruleHolder.transform.position.x, -value*150);
    }

    public void SetTapePosition(float value) { 
        if (value <= -1)
        {
            value = tapeSlider.value;
        }
        transform.position = new Vector3(-(-5+value) * (Screen.width / 20), transform.position.y);

    }

    public void AddRuleErrorMessage(string message)
    {
        ruleAddedAnimator.gameObject.GetComponent<TextMeshProUGUI>().text = message;
        Debug.Log(message);
    }
}
