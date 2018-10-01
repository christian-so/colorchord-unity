using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class VoronoiReceiver {
    public bool stop = false;
    UdpClient client;

    public byte[] lastPacket;
    float[] channel1;
    float[] channel2;
    int port;
    bool useC1 = true;
    object boolLock = new object();

    public float[] latestData {
        get
        {
            bool buff;
            lock(boolLock)
            {
                buff = useC1;
                useC1 = !useC1;
            }

            if (buff)
            {
                return channel1;
            }
            else
            {
                return channel2;
            }
        }

        private set
        {
            lock(boolLock)
            {
                if (useC1)
                {
                    channel1 = value;
                }
                else
                {
                    channel2 = value;
                }
            }
        }
    }

	// Use this for initialization
	public VoronoiReceiver(int Port) {
        client = new UdpClient(Port);
        port = Port;

        lock (boolLock)
        {
            channel1 = new float[24];
            channel2 = new float[24];
        }
    }

    public void Start()
    {
        stop = false;
        client.BeginReceive(new AsyncCallback(recv), null);
    }

    public void Stop()
    {
        client.Close();
        stop = true;
    }
	
	// Update is called once per frame
	void recv(IAsyncResult res) {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);

        byte[] inputArray = client.EndReceive(res, ref RemoteIpEndPoint);

        if (inputArray.Length == 24*4)
        {
            lock (boolLock)
            {
                if(useC1)
                {
                    Buffer.BlockCopy(inputArray, 0, channel1, 0, inputArray.Length);
                }
                else
                {
                    Buffer.BlockCopy(inputArray, 0, channel2, 0, inputArray.Length);
                }
            }
        }
        if (!stop) client.BeginReceive(new AsyncCallback(recv), null);
	}
}
