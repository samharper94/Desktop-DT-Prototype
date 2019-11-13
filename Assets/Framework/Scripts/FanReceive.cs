using System;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FanReceive : MonoBehaviour
{
    public int viewerID;
    public Text serverTime;
    public int fanCurrentJob;
    public Button setAuto;
    public Text autoText;
    public bool auto = false;

    //text that shows the received value and previewed value
    public Text speedDisp_Recieve, speedDisp_Preview, twinLabel;
    //button to send the new value
    public Button btn;
    //slider to control speed
    public Slider fanSpeedInput;
    //network received speed
    string _fanSpeed = "0";
    //float of network received speed
    float fanSpeed;
    //script to send/receive speed 
    public DataReceive dataReceive;
    //twin fan game object
    public GameObject twinFan;
    //real fan game object
    public GameObject realFan;
    //speed at which to rotate fan (obtained from slider)
    float speed;
    //factor for the visualisation
    public float speedFactor;
    //toggle between received and chosen speed
    bool isPreview = false;
    //keyword recogniser for speech
    KeywordRecognizer keywordRecogniser;
    //dictionary to store keywords and actions
    Dictionary<string, Action> keywords = new Dictionary<string, Action>();
    //text that shows the received value
    public Text tempDisp_Receive;
    //text that shows the temperature sensor status
    public Text tempDisp_Status;
    //text that shows the fan status
    public Text fanDisp_Status;
    //text that shows any extra IDs
    public Text extraIDDisp;
    //text that sets the threshold for the warning image
    public Text tempWarningThreshold;
    //network received temperature
    string _temperature = "0";
    //float of network received temperature
    float temperature;
    //temperature status string
    string temperatureStatus;
    //image to warn of high temperatures
    public Image tempWarning;
    //booleans to check presence of sensors
    bool fanPresent, temperaturePresent;
    //gameobjects for the fan case of the twin and the blades of the real fan
    public GameObject twinFanCase, realFanBlade;

    // Start is called before the first frame update
    void Start()
    {
        //find instance of network manager
        dataReceive = GameObject.Find("Manager").GetComponent<DataReceive>();
        //listen for value change in the slider and record the value
        fanSpeedInput.onValueChanged.AddListener(speedChange);
        //on button click, send the previewed fan speed
        btn.onClick.AddListener(sendSpeed);
        //disable the temperature warning image
        tempWarning.enabled = false;
        serverTime = GameObject.Find("serverTime").GetComponent<Text>();
        autoText = GameObject.Find("setAuto").GetComponentInChildren<Text>();
        setAuto = GameObject.Find("setAuto").GetComponent<Button>();
        setAuto.onClick.AddListener(autoPress);
        #region speech

        keywords.Add("full", () =>
        {
            //toggle preview mode
            isPreview = true;
            //set the fan speed to full
            speed = 100;
            //show the previewed speed on the slider
            fanSpeedInput.value = speed;
            //show previewed fan speed as a string
            speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
        });

        keywords.Add("max", () =>
        {
            //toggle preview mode
            isPreview = true;
            //set the fan speed to full
            speed = 100;
            //show the previewed speed on the slider
            fanSpeedInput.value = speed;
            //show previewed fan speed as a string
            speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
        });

        keywords.Add("confirm", () =>
        {
            //send the new speed
            dataReceive.SendMsg("0+" + speed.ToString());
            //toggle preview mode
            isPreview = false;
        });

        keywords.Add("ok", () =>
        {
            //send the new speed
            dataReceive.SendMsg("0+" + speed.ToString());
            //toggle preview mode
            isPreview = false;
        });

        keywords.Add("off", () =>
        {
            //toggle preview mode
            isPreview = true;
            //set the fan speed to zero
            speed = 0;
            //show the previewed speed on the slider
            fanSpeedInput.value = speed;
            //show previewed fan speed as a string
            speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
        });

        keywords.Add("disable", () =>
        {
            //toggle preview mode
            isPreview = true;
            //set the fan speed to zero
            speed = 0;
            //show the previewed speed on the slider
            fanSpeedInput.value = speed;
            //show previewed fan speed as a string
            speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
        });

        keywords.Add("mid", () =>
        {
            //toggle preview mode
            isPreview = true;
            //set the fan speed to medium
            speed = 50;
            //show the previewed speed on the slider
            fanSpeedInput.value = speed;
            //show previewed fan speed as a string
            speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
        });

        keywordRecogniser = new KeywordRecognizer(keywords.Keys.ToArray());

        keywordRecogniser.OnPhraseRecognized += KeywordRecogniser_OnPhraseRecognised;

        keywordRecogniser.Start();

        #endregion
    }

    void KeywordRecogniser_OnPhraseRecognised(PhraseRecognizedEventArgs args)
    {
        Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    //called when slider is moved
    void speedChange(float val)
    {
        //toggle preview mode
        isPreview = true;
        //set the fan speed to the slider value
        speed = fanSpeedInput.value;
        //show previewed fan speed as a string
        speedDisp_Preview.text = "Previewed speed: " + speed.ToString() + "%";
    }

    //called when button is pressed
    void sendSpeed()
    {
        //send the new speed
        dataReceive.SendMsg("0+" + speed.ToString());
        //disable preview mode
        isPreview = false;
    }

    void autoPress()
    {
        if (auto)
        {
            autoText.text = "Set Auto";
            auto = false;
            fanSpeedInput.interactable = true; ;
            btn.interactable = true;
            return;
        }
        else
        {
            autoText.text = "Disable Auto";
            auto = true;
            fanSpeedInput.interactable = false; ;
            btn.interactable = false;
            return;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
        //assume the sensors are not present
        temperaturePresent = false;
        fanPresent = false;
        if (isPreview)
        {
            twinFanCase.SetActive(true);
            twinFan.SetActive(true);
            twinLabel.enabled = true;
            speedDisp_Preview.enabled = true;
        }
        else
        {
            twinFanCase.SetActive(false);
            twinFan.SetActive(false);
            twinLabel.enabled = false;
            speedDisp_Preview.enabled = false;
        }
        #region parsing
        //don't run if there's no message received
        if (dataReceive.serverMessage.Length < 5 && dataReceive.serverMessage.Length > 0)
        {
            viewerID = int.Parse(dataReceive.serverMessage);
        }
        else if (dataReceive.serverMessage.Length > 0)
        {
            string[] timeData = dataReceive.serverMessage.Split('!');
            serverTime.text = timeData[0];
            if (timeData.Length > 1)
            {
                try
                {
                    #region server message
                    //decode the received string by making an array (string received as "fan null potentiometer")
                    string[] strArray = timeData[1].Split('@');
                    //check through array for received sensor data
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        //set up a temporary string for each part of the received string
                        string temp = strArray[i];
                        //split the string using the + signs (ID+Data+Status)
                        string[] tempData = temp.Split('+');
                        //if only id+data
                        /*if (tempData.Length == 2)
                        {
                            //take the id from the first part (fan = 0, temperature = 2)
                            int id = int.Parse(tempData[0]);

                            //if id = 0 (fan), set fan speed and confirm its presence
                            if (id == 0)
                            {
                                fanPresent = true;
                                _fanSpeed = tempData[1];
                            }
                            //if id = 2 (sensor), set text and confirm its presence
                            if (id == 2)
                            {
                                temperaturePresent = true;
                                _temperature = tempData[1];
                                if (_temperature != "" || _temperature != "null")
                                {
                                    tempDisp_Receive.text = _temperature + " Celcius \n Status: N/A";
                                    if (float.Parse(_temperature) < float.Parse(tempWarningThreshold.text))
                                    {
                                        tempWarning.enabled = true;
                                    }
                                    else
                                    {
                                        tempWarning.enabled = false;
                                    }
                                }
                            }
                            if (id == 1)
                            {
                                extraIDDisp.text = "ID " + id + ": Testing Device";
                            }
                        }*/
                        //if id+status+data
                        int id = int.Parse(tempData[0]);
                        if (id == 0) //fan
                        {
                            fanPresent = true;
                            _fanSpeed = tempData[2];
                            string status = tempData[1];
                            //tempData[3] is the current behaviour of the fan
                            fanCurrentJob = int.Parse(tempData[3]);
                            switch (status)
                            {
                                case "a":
                                    fanDisp_Status.text = "Status: OK";
                                    fanDisp_Status.color = Color.green;
                                    realFanBlade.GetComponent<Renderer>().material.color = Color.black;
                                    break;
                                case "b":
                                    fanDisp_Status.text = "Status: Check";
                                    fanDisp_Status.color = Color.yellow;
                                    realFanBlade.GetComponent<Renderer>().material.color = Color.yellow;
                                    break;
                                case "c":
                                    fanDisp_Status.text = "Status: Warning";
                                    fanDisp_Status.color = Color.red;
                                    realFanBlade.GetComponent<Renderer>().material.color = Color.red;
                                    break;
                            }
                        }
                        if (id == 2) //temperature
                        {
                            temperaturePresent = true;
                            _temperature = tempData[2];
                            string status = tempData[1];
                            if (_temperature != "" || _temperature != "null")
                            {
                                tempDisp_Receive.text = _temperature + " Celcius";
                            }
                            if (float.Parse(_temperature) < float.Parse(tempWarningThreshold.text))
                            {
                                tempWarning.enabled = false;
                                if (auto && fanCurrentJob > 0)
                                    dataReceive.SendMsg("0+0");
                            }
                            else
                            {
                                tempWarning.enabled = true;
                                if (auto && fanCurrentJob < 100)
                                    dataReceive.SendMsg("0+100");
                            }
                            switch (status)
                            {
                                case "a":
                                    tempDisp_Status.text = "Status: OK";
                                    tempDisp_Status.color = Color.green;
                                    break;
                                case "b":
                                    tempDisp_Status.text = "Status: Check";
                                    tempDisp_Status.color = Color.yellow;
                                    break;
                                case "c":
                                    tempDisp_Status.text = "Status: Warning";
                                    tempDisp_Status.color = Color.red;
                                    break;
                            }
                        }

                        /*if (tempData.Length == 3)
                        {
                            //take the id from the first part (fan = 0, temperature = 2)
                            int id = int.Parse(tempData[0]);
                            if (id == 1)
                            {
                                string st = tempData[1];
                                extraIDDisp.text = "ID " + id + ": Testing Device       Value:" + tempData[2] + "        Status: " + st;

                                break;
                            }
                            //take the status from the second part
                            string status = tempData[1];
                            //if id = 0 (fan), set fan speed and confirm its presence
                            if (id == 0)
                            {
                                fanPresent = true;
                                _fanSpeed = tempData[2];
                                switch (status)
                                {
                                    case "a":
                                        fanDisp_Status.text = "Status: OK";
                                        fanDisp_Status.color = Color.green;
                                        realFanBlade.GetComponent<Renderer>().material.color = Color.black;
                                        break;
                                    case "b":
                                        fanDisp_Status.text = "Status: Check";
                                        fanDisp_Status.color = Color.yellow;
                                        realFanBlade.GetComponent<Renderer>().material.color = Color.yellow;
                                        break;
                                    case "c":
                                        fanDisp_Status.text = "Status: Warning";
                                        fanDisp_Status.color = Color.red;
                                        realFanBlade.GetComponent<Renderer>().material.color = Color.red;
                                        break;
                                }
                            }
                            //if id = 2 (sensor), set text and confirm its presence
                            if (id == 2)
                            {
                                temperaturePresent = true;
                                _temperature = tempData[2];
                                temperatureStatus = tempData[1];
                                if (_temperature != "" || _temperature != "null")
                                {
                                    tempDisp_Receive.text = _temperature + " Celcius";
                                }
                                if (float.Parse(_temperature) < float.Parse(tempWarningThreshold.text))
                                {
                                    tempWarning.enabled = false;
                                }
                                else
                                {
                                    tempWarning.enabled = true;
                                }
                                switch (status)
                                {
                                    case "a":
                                        tempDisp_Status.text = "Status: OK";
                                        tempDisp_Status.color = Color.green;
                                        break;
                                    case "b":
                                        tempDisp_Status.text = "Status: Check";
                                        tempDisp_Status.color = Color.yellow;
                                        break;
                                    case "c":
                                        tempDisp_Status.text = "Status: Warning";
                                        tempDisp_Status.color = Color.red;
                                        break;
                                }
                            }
                            else if (id > 2)
                            {
                                string st = tempData[1];
                                extraIDDisp.text = "ID " + id + ": Testing Device       Value:" + tempData[2] + "        Status: " + st;

                                break;
                            }

                        }*/
                    }
                    //take the first element as the fan speed
                    //_fanSpeed = strArray[0];
                    #endregion
                }
                catch (FormatException e)
                {
                    Debug.Log(e);
                }

                //don't change the fan speed if the message does not contain the speed
                if (_fanSpeed != "" || _fanSpeed != "null" || _fanSpeed != null || fanPresent == true)
                {
                    btn.interactable = true;
                    //parse the string
                    fanSpeed = float.Parse(_fanSpeed);
                    //make text colour more red as speed increases
                    speedDisp_Recieve.color = new Color(1, (100 - fanSpeed) / 100, (100 - fanSpeed) / 100);
                    //show the received fan speed on both fans
                    speedDisp_Recieve.text = "Actual fan speed: " + _fanSpeed + " rpm";
                    //update the visual speed by parsing the string
                    realFan.transform.Rotate(new Vector3(0, 0, -1), speedFactor * fanSpeed);
                    //don't run if in preview mode
                    if (isPreview == false)
                    {
                        twinFan.transform.Rotate(new Vector3(0, 0, -1), speedFactor * fanSpeed);
                    }
                    //if there's a connection but in preview mode, spin blades at slider speed
                    else
                    {
                        speedDisp_Preview.color = new Color(1, (100 - speed) / 100, (100 - speed) / 100);
                        twinFan.transform.Rotate(new Vector3(0, 0, -1), speedFactor * speed);
                    }
                }
                //show user that the fan is not connected
                else
                {
                    speedDisp_Recieve.text = "Actual fan not connected!";
                    btn.interactable = false;
                }
            }
            else
            {
                //if there's no connection but in preview mode, spin blades at slider speed
                if (isPreview == true)
                {
                    speedDisp_Preview.color = new Color(1, (255 - speed) / 255, (255 - speed) / 255);
                    twinFan.transform.Rotate(new Vector3(0, 0, -1), speedFactor * speed);
                }
            }
            #endregion

            //tell user if sensors are missing
            if (temperaturePresent == false)
            {
                tempDisp_Receive.text = "Sensor not connected!";
            }
            if (fanPresent == false)
            {
                speedDisp_Recieve.text = "Actual fan not connected!";
                btn.interactable = false;
            }
        }
    }

    private void OnApplicationQuit()
    {
        //keywordRecogniser.Stop();
    }
}
