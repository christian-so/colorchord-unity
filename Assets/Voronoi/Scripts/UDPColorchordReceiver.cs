using System;
using System.Net;
using System.Net.Sockets;

public class UDPColorchordReceiver {
    public bool stop = false;
    UdpClient client;
    int port;
    int bufferSize;
    int colorchordFloatSize;
    float[] inputBuffer;
    object inputBufferLock = new object();

    public float[] latestData {
        get
        {
            float[] ret = null;
            lock (inputBufferLock)
            {
                ret = new float[bufferSize];
                Array.Copy(inputBuffer, ret, bufferSize);
            }
            return ret;
        }
    }

	// Use this for initialization
	public UDPColorchordReceiver(int Port, int bufferSize = 24, int colorchordFloatSize = 4) {
        this.client = new UdpClient(Port);
        this.port = Port;
        this.bufferSize = bufferSize;
        this.colorchordFloatSize = colorchordFloatSize;

        lock (inputBufferLock)
        {
            inputBuffer = new float[bufferSize];
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
        byte[] byteInput = client.EndReceive(res, ref RemoteIpEndPoint);

        if (byteInput.Length == bufferSize * colorchordFloatSize)
        {
            lock (inputBufferLock)
            {
                Buffer.BlockCopy(byteInput, 0, inputBuffer, 0, byteInput.Length);
            }
        }
        if (!stop) client.BeginReceive(new AsyncCallback(recv), null);
	}
}
