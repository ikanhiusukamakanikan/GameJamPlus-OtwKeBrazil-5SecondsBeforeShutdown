using UnityEngine;
public class MoveableObject : MonoBehaviour, IHistoryObject
{
    public enum MoveType { Pistol, ResetBox }
    public MoveType type;

    public HistoryData SaveState()
    {
        return new HistoryData { position = transform.position };
    }

    public void LoadState(HistoryData data)
    {
        transform.position = data.position;
    }
}
