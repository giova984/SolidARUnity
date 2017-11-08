using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public static class HumanFriendlyInteger
{
    static string[] ones = new string[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    public static string IntegerToWritten(int n)
    {
        if(n >= 0 && n<10)
            return ones[n];
        return "";
    }
}

    public class SpeechManager : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Awake()
    {
        if (!HoloHelper.isHololens())
            this.enabled = false;
    }

    // Use this for initialization
    void Start()
    {

        keywords.Add("Reference Calibration", () =>
        {
            if(!ReferenceCalibration.Instance.isCalibrating())
                ReferenceCalibration.Instance.SendMessage("OnStartReferenceCalibration");
        });

        for (int projID = (int)HoloID.Projector1; projID < (int)HoloID.Projector8; ++projID)
        {
            int pid = projID; //local loop variable (c#<4 compiler bug need a local variable not a loop one to correct handle closure in lambda expresisons. check: https://netmatze.wordpress.com/2012/05/11/using-loop-variables-in-lambda-expressions-in-c-5/
            if (ProjectorCalibration.GetInstance(pid) != null)
            {
                string cmdString = "Projector " + HumanFriendlyInteger.IntegerToWritten(pid + 1) + " Calibration" ;
                Debug.Log("Add SpeechCmd \"" + cmdString + "\"");
                System.Action action = null;


                //action = () =>
                //{
                //    Debug.Log("Calling OnStartProjCalib on " + (HoloID)pid);
                //    if (!ProjectorCalibration.GetInstance(pid).isCalibrating())
                //        ProjectorCalibration.GetInstance(pid).SendMessage("OnStartProjectorCalibration");
                //};

                //Loop unrolling because the compiler is not able to actualize the parameter(projID is always 7)
                switch (projID)
                {
                    case 0:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)0);
                            if (!ProjectorCalibration.GetInstance(0).isCalibrating())
                                ProjectorCalibration.GetInstance(0).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 1:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)1);
                            if (!ProjectorCalibration.GetInstance(1).isCalibrating())
                                ProjectorCalibration.GetInstance(1).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 2:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)2);
                            if (!ProjectorCalibration.GetInstance(2).isCalibrating())
                                ProjectorCalibration.GetInstance(2).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 3:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)3);
                            if (!ProjectorCalibration.GetInstance(3).isCalibrating())
                                ProjectorCalibration.GetInstance(3).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 4:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)4);
                            if (!ProjectorCalibration.GetInstance(4).isCalibrating())
                                ProjectorCalibration.GetInstance(4).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 5:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)5);
                            if (!ProjectorCalibration.GetInstance(5).isCalibrating())
                                ProjectorCalibration.GetInstance(5).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 6:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)6);
                            if (!ProjectorCalibration.GetInstance(6).isCalibrating())
                                ProjectorCalibration.GetInstance(6).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                    case 7:
                        action = () =>
                        {
                            //Debug.Log("Calling OnStartProjCalib on " + (HoloID)7);
                            if (!ProjectorCalibration.GetInstance(7).isCalibrating())
                                ProjectorCalibration.GetInstance(7).SendMessage("OnStartProjectorCalibration");
                        };
                        break;
                }

                // {
                //    Debug.Log("Calling OnStartProjCalib on Projector" + (projID + 1));
                //    if (!ProjectorCalibration.GetInstance(projID).isCalibrating())
                //        ProjectorCalibration.GetInstance(projID).SendMessage("OnStartProjectorCalibration");
                //};
                keywords.Add(cmdString, action);
            }
        }

        keywords.Add("Stop Calibration", () =>
        {
            if (ReferenceCalibration.Instance.isCalibrating())
                ReferenceCalibration.Instance.BroadcastMessage("OnStopReferenceCalibration");

            for (int projID = (int)HoloID.Projector1; projID < (int)HoloID.Projector8; ++projID)
            {
                if (ProjectorCalibration.GetInstance(projID) != null)
                {
                    if (ProjectorCalibration.GetInstance(projID).isCalibrating())
                        ProjectorCalibration.GetInstance(projID).SendMessage("OnStopProjectorCalibration");
                }
            }
        });

        keywords.Add("Clear Calibration", () =>
        {
            ReferenceCalibration.Instance.BroadcastMessage("OnClearCalibration");
        });

        keywords.Add("Show Calibration", () =>
        {
            BroadcastMessage("OnShowCalibration");
        });

        keywords.Add("Hide Calibration", () =>
        {
            BroadcastMessage("OnHideCalibration");
        });

        keywords.Add("Show People", () =>
        {
            BroadcastMessage("OnShowTelepresence");
        });

        keywords.Add("Hide People", () =>
        {
            BroadcastMessage("OnHideTelepresence");
        });

        keywords.Add("Quit Application", () =>
        {
            Application.Quit();
        });

        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
