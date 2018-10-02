using UnityEngine;
public class TransferDataModel
{
    public readonly Color color;
    public readonly float width;

    public TransferDataModel(Color color, float width)
    {
        this.color = color;
        this.width = width;
    }

    public TransferDataModel(float h, float s, float v, float width)
    {
        this.color = Color.HSVToRGB(h, s, v);
        this.width = width;
    }
}
