using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;

public class HeatMap : MonoBehaviour {

    [SerializeField, Range(1, 256)]
    private int xScale = 64;
    [SerializeField, Range(1, 256)]
    private int yScale = 64;

    string url = "http://52.50.211.86:1880/clients";
    Dictionary<string, Texture2D> m_textures = new Dictionary<string, Texture2D>();
    Dictionary<string, List<ParticleSystem>> m_particles = new Dictionary<string, List<ParticleSystem>>();

    [SerializeField]
    GameObject particles;

    private IEnumerator getData() {
        WWW www = new WWW(url);
        yield return www;
        string json = www.text;
        json = json.Insert(0, @"{""_"":");
        json += "}";
        Clients clients = JsonUtility.FromJson<Clients>(json);

        SortData(clients._);
    }

    private void SortData(List<Client> clients) {
        Dictionary<string, List<Client>> byTime = new Dictionary<string, List<Client>>();
        foreach(Client c in clients) {
            if(c.seenString != null) {
                string stamp = c.seenString.Substring(0, 15);
                if(!byTime.ContainsKey(stamp)) {
                    byTime.Add(stamp, new List<Client>());
                }
                byTime[stamp].Add(c);
            }
        }

        foreach(var t in byTime) {
            StartCoroutine(normalizeLocations(t.Key, t.Value));
        }
    }

    private IEnumerator normalizeLocations(string stamp, List<Client> clients) {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        Client[] arr = clients.ToArray();
        int count = clients.Count;

        for(int ind = 0; ind < count; ind++) {
            arr[ind].lng = (arr[ind].lng > 0.0f) ? arr[ind].lng : -arr[ind].lng;
            arr[ind].lat = (arr[ind].lat > 0.0f) ? arr[ind].lat : -arr[ind].lat;

            if(arr[ind].lng < minX) {
                minX = arr[ind].lng;
            } else if (arr[ind].lng > maxX) {
                maxX = arr[ind].lng;
            }

            if(arr[ind].lat < minY) {
                minY = arr[ind].lat;
            } else if (arr[ind].lat > maxY) {
                maxY = arr[ind].lat;
            }
        }

        float difX = maxX - minX;
        float difY = maxY - minY;

        for(int ind = 0; ind < count; ind++) {
            arr[ind].lng -= minX;
            arr[ind].lng /= difX;

            arr[ind].lat -= minY;
            arr[ind].lat /= difY;
        }

        ProduceTexture(stamp, arr);

        yield break;
    }

    private void ProduceTexture(string stamp, Client[] clients) {
        int max = 0;
        int[][] counter = new int[xScale + 1][];
        for(int i = 0; i <= xScale; i++) { counter[i] = new int[yScale + 1]; }

        foreach(Client c in clients) {
            int x = (int)(c.lng * xScale);
            int y = (int)(c.lat * yScale);

            try {
                counter[x - 1][y]++;
            } catch(Exception e) { }

            try {
                counter[x + 1][y]++;
            } catch(Exception e) { }

            try {
                counter[x][y - 1]++;
            } catch(Exception e) { }

            try {
                counter[x][y + 1]++;
            } catch(Exception e) { }

            counter[x][y] += 2;

            if(counter[x][y] > max) {
                max = counter[x][y];
            }
        }
        //Texture2D texture = new Texture2D(xScale + 1, yScale + 1);
        List<ParticleSystem> list = new List<ParticleSystem>();
        for(int j = 0; j <= yScale; j++) {
            for(int i = 0; i <= xScale; i++) {
                if(counter[i][j] <= max && counter[i][j] > 0) {
                    float step = (float)counter[i][j] / max;
                    GameObject g = Instantiate(particles);
                    ParticleSystem p = g.GetComponent<ParticleSystem>();
                    g.transform.position = new Vector3(i, step * 2, j);
                    p.emissionRate = 100 * step;
                    if(step > 0.3f) {
                        p.startColor = new Color(1.0f, 1.0f - ((step - 0.7f) * 3), 0.0f);
                        //texture.SetPixel(i, j, new Color(1.0f, 1.0f - ((step - 0.7f) * 3), 0.0f));
                    } else {
                        p.startColor = new Color((step * 3), 1.0f, 0.0f);
                        //texture.SetPixel(i, j, new Color((step * 3), 1.0f, 0.0f));
                    }
                    g.SetActive(false);
                    list.Add(p);
                } else {
                    //texture.SetPixel(i, j, new Color(0.0f, 0.0f, 1.0f));
                }
            }
        }
        m_particles.Add(stamp, list);
        //m_textures.Add(stamp, texture);
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(getData());
    }

    bool done = false;

	// Update is called once per frame
	void Update () {
        if(!done) {
            int i = 0;
            foreach(var texture in m_particles) {
                foreach(ParticleSystem g in texture.Value) {
                    g.gameObject.SetActive(true);
                    i++;
                }
                Debug.Log("Go " + i);
                /*MeshRenderer quadRend = quad.GetComponent<MeshRenderer>();
                quadRend.sharedMaterial.mainTexture = texture.Value;*/
                done = true;
                break;
            }
        }
	}
}
