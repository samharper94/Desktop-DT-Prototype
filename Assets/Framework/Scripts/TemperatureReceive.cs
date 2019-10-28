using UnityEngine;
using UnityEngine.UI;

public class TemperatureReceive : MonoBehaviour
{
    //text that shows the received value
    public Text tempDisp_Receive;
    //network received temperature
    string _temperature;
    //float of network received temperature
    float temperature;
    //script to send/receive temperature 
    DataReceive dataReceive;
    //image to warn of high temperatures
    public Image tempWarning;

    private void Start()
    {
        //find instance of network manager
        dataReceive = GameObject.Find("Manager").GetComponent<DataReceive>();
    }

    // Update is called once per frame
    void Update()
    {
        //don't run if there's no message received
        if (dataReceive.serverMessage != null)
        {
            //decode the received string by making an array (string received as "fan null potentiometer")
            string[] strArray = dataReceive.serverMessage.Split(' ');
            //make sure the element is present
            if (strArray.Length > 2)
            {
                //take the third element as the temperature
                _temperature = strArray[2];
                //don't run if the message does not contain the temperature
                if (_temperature != "" || _temperature != "null")
                {
                    //parse the string
                    temperature = float.Parse(_temperature);
                    if (temperature <= 0)
                    {
                        tempWarning.enabled = false;
                        tempDisp_Receive.color = new Color((255 - temperature * -1) / 255, (255 - temperature * -1) / 255, 1);
                    }
                    if(temperature > 0 && temperature <= 15)
                    {
                        tempWarning.enabled = false;
                        tempDisp_Receive.color = new Color(1, (255 - temperature) / 255, (255 - temperature) / 255);
                    }
                    if(temperature > 15)
                    {
                        tempDisp_Receive.color = new Color(1, (255 - temperature) / 255, (255 - temperature) / 255);
                        tempWarning.enabled = true;
                    }
                    tempDisp_Receive.text = _temperature + " Celcius";
                }
                //show user that the temperature sensor is not connected
                else
                {
                    tempDisp_Receive.text = "Temperature sensor not connected!";
                }
            }
            //if the element is not present, show user that the temperature sensor is not connected
            else
            {
                tempDisp_Receive.text = "Temperature sensor not connected!";
            }
        }
    }
}
