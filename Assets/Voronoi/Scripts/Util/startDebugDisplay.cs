using System.IO;
using UnityEngine;

public class startDebugDisplay : MonoBehaviour {
    public string ConfigFileName = "";
    public PlaceVoronoiPoints voronoi = null;
	// Use this for initialization
	void Start () {
        using (var reader = new StreamReader(ConfigFileName))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.ToLower().StartsWith("port"))
                {
                    string[] strs = line.Replace(" ", string.Empty).Split(':');
                    int port;
                    if(int.TryParse(strs[1], out port) && voronoi != null)
                    {
                        //voronoi.port = port;
                    }
                }
            }
        }
    }
}
