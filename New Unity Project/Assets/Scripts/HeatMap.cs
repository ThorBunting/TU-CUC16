using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public struct Client
{
    public string _id;
    public string name;
    public string mac;
    public float latitude;
    public float longitude;
    public bool seen;
    public string seenEpoch;
    public string manufacturer;
    public string os;
    public string ssid;
}

[System.Serializable]
public struct Clients
{
    public List<Client> _;

}

public class HeatMap : MonoBehaviour
{
    // Range for normalisation
    [SerializeField, Range(1, 256)]
    private int xScale = 64;
    [SerializeField, Range(1, 256)]
    private int yScale = 64;

    // Dictionary to store dataset with timestamp
    public Dictionary<string, Clients> dataSet = new Dictionary<string, Clients>();

    // URL where the JSON data is hosted
    string url = "https://api.myjson.com/bins/44g0z";

    // Dictionary for storing particle systems and timestamp
    Dictionary<string, List<GameObject>> m_particles = new Dictionary<string, List<GameObject>>();

    //Int to indicate the active dataset
    int active = -1;

    // Try and remove this
    List<string> m_stamps = new List<string>();

    [SerializeField]
    GameObject container;

    [SerializeField]
    GameObject particles;

    [SerializeField]
    GameObject gui;

    [SerializeField]
    GameObject textMesh;

    private int systemNumber = 0;

    private IEnumerator getData()
    {
        WWW www = new WWW(url);
        Debug.Log(www.url);
        yield return www;
        string json = www.text;

        Debug.Log(json);
        Clients clients = JsonUtility.FromJson<Clients>(json);

        Debug.Log(clients._.Count + "Num of clients");

        SortData(clients._);
    }

    private void SortData(List<Client> clients)
    {
        Debug.Log(clients.Count + "Clients being sorted");
        Dictionary<string, List<Client>> byTime = new Dictionary<string, List<Client>>();
        foreach (Client c in clients)
        {
            if (c.seen != false)
            {
                string stamp = c.seenEpoch;

                if (!byTime.ContainsKey(stamp))
                {
                    m_stamps.Add(stamp);
                    byTime.Add(stamp, new List<Client>());
                }
                byTime[stamp].Add(c);
            }
        }

        Debug.Log(byTime.Count + "Number of vars in byTime");

        foreach (var t in byTime)
        {
            StartCoroutine(normalizeLocations(t.Key, t.Value));
        }
    }

    private IEnumerator normalizeLocations(string stamp, List<Client> clients)
    {
        Debug.Log(clients.Count + "Locations being normalized");

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        Client[] arr = clients.ToArray();
        int count = clients.Count;

        for (int ind = 0; ind < count; ind++)
        {
            arr[ind].longitude= (arr[ind].longitude > 0.0f) ? arr[ind].longitude : -arr[ind].longitude;
            arr[ind].latitude = (arr[ind].latitude > 0.0f) ? arr[ind].latitude : -arr[ind].latitude;

            if (arr[ind].longitude < minX)
            {
                minX = arr[ind].longitude;
            }
            else if (arr[ind].longitude > maxX)
            {
                maxX = arr[ind].longitude;
            }

            if (arr[ind].latitude < minY)
            {
                minY = arr[ind].latitude;
            }
            else if (arr[ind].latitude > maxY)
            {
                maxY = arr[ind].latitude;
            }
        }

        float difX = maxX - minX;
        float difY = maxY - minY;

        for (int ind = 0; ind < count; ind++)
        {
            arr[ind].longitude -= minX;
            arr[ind].longitude /= difX;

            arr[ind].latitude -= minY;
            arr[ind].latitude /= difY;
        }

        ProduceDataPoints(stamp, arr);
        systemNumber = 0;

        yield break;
    }

    private void ProduceDataPoints(string stamp, Client[] clients)
    {
        Debug.Log(clients.Length + "Particle Systems being produced");
        int max = 0;
        int[][] counter = new int[xScale + 1][];
        for (int i = 0; i <= xScale; i++) { counter[i] = new int[yScale + 1]; }

        foreach (Client c in clients)
        {
            int x = (int)(c.longitude * xScale);
            int y = (int)(c.latitude * yScale);

            counter[x][y] += 2;

            if (counter[x][y] > max)
            {
                max = counter[x][y];
            }
        }

        List<GameObject> list = new List<GameObject>();
        List<TextMesh> tmList = new List<TextMesh>();
        GameObject o = new GameObject();
        o.transform.parent = container.transform;
        o.SetActive(false);
        for (int j = 0; j <= yScale; j++)
        {
            for (int i = 0; i <= xScale; i++)
            {
                if (counter[i][j] <= max && counter[i][j] > 0)
                {
                    float step = (float)counter[i][j] / max;
                    Debug.Log("Step is: " + step);
                    GameObject g = Instantiate(particles);
                    GameObject t = Instantiate(textMesh);
                    ParticleSystem p = g.GetComponent<ParticleSystem>();
                    TextMesh tm = t.GetComponent<TextMesh>();
                    g.transform.position = new Vector3(i, step, j);
                    t.transform.position = new Vector3(i, step, j);
                    g.transform.parent = o.transform;
                    t.transform.parent = o.transform;
                    tm.text = "Device: " + clients[systemNumber].manufacturer + " - " + clients[systemNumber].name + "\nMac Address: " + clients[systemNumber].mac;
                    systemNumber += 1;
                    p.emissionRate = 15 * step;
                    if (step > 0.3f)
                    {
                        p.startColor = new Color(1.0f, 1.0f - ((step - 0.7f) * 3), 0.0f);
                    }
                    else
                    {
                        p.startColor = new Color((step * 3), 1.0f, 0.0f);
                    }
                    list.Add(g);
                }
                else
                {
                }
            }
        }
        m_particles.Add(stamp, list);
    }

    void Activate(int index)
    {
        Debug.Log("Activating");
        if (active != -1)
        {
            m_particles[m_stamps[active]][0].transform.parent.gameObject.SetActive(false);
        }
        active = index;
        try
        {
            GameObject g = m_particles[m_stamps[active]][0].transform.parent.gameObject;
            g.SetActive(true);
            g.transform.position = new Vector3(7, 0, -16);
            g.transform.rotation = Quaternion.Euler(0, 180, 0);
            g.transform.localScale = new Vector3(0.8f, 1, -0.8f);
            string text = m_stamps[active] + "0";
            text = text.Replace("-", "/");
            gui.GetComponent<Text>().text = text.Replace("T", " ");
        } catch(System.Exception e)
        {
            m_stamps.Remove(m_stamps[active]);
            active = active < m_stamps.Count ? active : 0;
            GameObject g = m_particles[m_stamps[active]][0].transform.parent.gameObject;
            g.SetActive(true);
            g.transform.position = new Vector3(7, 0, -16);
            g.transform.rotation = Quaternion.Euler(0, 180, 0);
            g.transform.localScale = new Vector3(0.8f, 1, -0.8f);
            string text = m_stamps[active] + "0";
            text = text.Replace("-", "/");
            gui.GetComponent<Text>().text = text.Replace("T", " ");
        }
   

    }

    void DoInput()
    {
        int ind = active;
        if (Input.anyKeyDown)
        {
            ind++;
            if (ind >= m_stamps.Count)
            {
                ind = 0;
            }
            Activate(ind);
        }
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(getData());
    }

    // Update is called once per frame
    void Update()
    {
        if (active == -1 && m_stamps.Count != 0)
        {
            m_stamps.Sort();
            Activate(0);
        }
        if (active != -1)
        {
            DoInput();
        }
    }
}