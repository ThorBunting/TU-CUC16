using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Client {
    //public string _id;
    //public string name;
    //public string mac;
    public float lat;
    public float lng;
    //public float unc;
    public string seenString;
    //public float seenEpoch;
    //public string manufacturer;
    //public string os;
    //public string ssid;
    //public string ap;
}

[System.Serializable]
public struct Clients {
    public List<Client> _;
}

public class REST : MonoBehaviour {

    UnityWebRequest m_request;
    //string url = "http://52.50.211.86/cmx/api/location/v1/beacon/count";
    string url = "http://52.50.211.86:1880/clients";

    // Use this for initialization
    void Start () {
        StartCoroutine(Request());
	}

    IEnumerator Request() {
        WWW www = new WWW(url);
        yield return www;
        string json = www.text;
        json = json.Insert(0, @"{""_"":");
        json += "}";
        Clients c = JsonUtility.FromJson<Clients>(json);
        Debug.Log(c._.Capacity);
    }
}
