public interface IHistoryObject
{
    // menyimpan state
    HistoryData SaveState();

    // mengapply undo
    void LoadState(HistoryData data);
}
