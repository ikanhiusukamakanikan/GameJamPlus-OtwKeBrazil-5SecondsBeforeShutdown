using System.Collections.Generic;
using UnityEngine;

public class TransformHistory : MonoBehaviour
{
    // ========================================
    // ORIGINAL SECTION (TETAP ADA)
    // ========================================

    [Header("Position History Target")]
    public Transform parentTarget;   // Child di sini disimpan posisinya

    [Header("Object History Target")]
    public Transform parentObject;   // Tempat spawn object
    public Transform playerPosition; // Lokasi spawn

    [Header("Prefab untuk Spawn")]
    public GameObject spawnPrefab;


    // ========================================
    // NEW SECTION (GENERIC UNDO SYSTEM)
    // ========================================

    [Header("Tracked Parents")]
    public Transform moveableParent;      // semua moveable object
    public Transform nonMoveableParent;   // semua button/lever dll


    // ---- Universal history entry ----
    [System.Serializable]
    public struct HistoryEntry
    {
        public IHistoryObject target;
        public HistoryData data;
    }

    // Satu snapshot = semua object yang disimpan state-nya
    private List<List<HistoryEntry>> universalHistory = new List<List<HistoryEntry>>();


    // ========================================
    // ORIGINAL HISTORY
    // ========================================
    private List<List<Vector3>> positionHistory = new List<List<Vector3>>();
    private List<GameObject> objectHistory = new List<GameObject>();

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            PrintAllHistory();
        }
    }

    // ========================================
    // MASTER SAVE
    // ========================================
    public void SaveLog()
    {
        if (parentTarget != null)
            SavePositionLog();

        // Save semua object universal (fitur baru)
        SaveUniversalLog();
    }



    // ========================================
    // SAVE POSITION (LAMA)
    // ========================================
    private void SavePositionLog()
    {
        List<Vector3> snapshot = new List<Vector3>();

        foreach (Transform child in parentTarget)
            snapshot.Add(child.localPosition);

        positionHistory.Add(snapshot);

        Debug.Log("Saved Position Log. Total: " + positionHistory.Count);
    }


    // ========================================
    // SAVE SPAWN OBJECT (LAMA)
    // ========================================
    public void SaveObjectLog()
    {
        GameObject newObj = Instantiate(
            spawnPrefab,
            playerPosition.position,
            Quaternion.identity,
            parentObject
        );

        objectHistory.Add(newObj);

        Debug.Log("Spawned & Saved Object Log. Total: " + objectHistory.Count);
    }


    // ========================================
    // SAVE UNIVERSAL OBJECT (BARU)
    // ========================================
    private void SaveUniversalLog()
    {
        List<HistoryEntry> snapshot = new List<HistoryEntry>();

        // Save dari moveable parent
        if (moveableParent != null)
        {
            foreach (Transform child in moveableParent)
            {
                IHistoryObject obj = child.GetComponent<IHistoryObject>();
                if (obj != null)
                {
                    snapshot.Add(new HistoryEntry
                    {
                        target = obj,
                        data = obj.SaveState()
                    });
                }
            }
        }

        // Save dari non-moveable parent
        if (nonMoveableParent != null)
        {
            foreach (Transform child in nonMoveableParent)
            {
                IHistoryObject obj = child.GetComponent<IHistoryObject>();
                if (obj != null)
                {
                    snapshot.Add(new HistoryEntry
                    {
                        target = obj,
                        data = obj.SaveState()
                    });
                }
            }
        }

        universalHistory.Add(snapshot);

        Debug.Log("Saved Universal Object Log. Total: " + universalHistory.Count);
    }



    // ========================================
    // MASTER UNDO
    // ========================================
    public void Undo()
    {
        UndoPosition();
        UndoObject();
        UndoUniversal();
    }



    // ========================================
    // UNDO POSITION (LAMA)
    // ========================================
    private void UndoPosition()
    {
        if (positionHistory.Count <= 1)
        {
            // Tidak bisa undo kalau cuma ada 0 atau 1 snapshot
            Debug.Log("UndoPosition skipped. Entry <= 1");
            return;
        }

        // 1. Hapus snapshot terbaru
        positionHistory.RemoveAt(positionHistory.Count - 1);

        // 2. Load snapshot terbaru yang tersisa
        List<Vector3> snapshot = positionHistory[positionHistory.Count - 1];

        int index = 0;
        foreach (Transform child in parentTarget)
        {
            child.localPosition = snapshot[index];
            index++;
        }

        Debug.Log($"UndoPosition → Remaining: {positionHistory.Count}");
    }

    // ========================================
    // UNDO SPAWNED OBJECT (LAMA)
    // ========================================
    private void UndoObject()
    {
        if (objectHistory.Count == 0)
        {
            Debug.Log("No object history.");
            return;
        }

        GameObject lastObj = objectHistory[objectHistory.Count - 1];

        if (lastObj != null)
            Destroy(lastObj);

        objectHistory.RemoveAt(objectHistory.Count - 1);

        Debug.Log("Undo Object. Remaining: " + objectHistory.Count);
    }



    // ========================================
    // UNDO UNIVERSAL OBJECT STATE (BARU)
    // ========================================
    private void UndoUniversal()
    {
        if (universalHistory.Count <= 1)
        {
            Debug.Log("UndoUniversal skipped. Entry <= 1");
            return;
        }

        // 1. Hapus state terbaru
        universalHistory.RemoveAt(universalHistory.Count - 1);

        // 2. Load state sebelumnya
        List<HistoryEntry> snapshot = universalHistory[universalHistory.Count - 1];

        foreach (var entry in snapshot)
        {
            if (entry.target != null)
                entry.target.LoadState(entry.data);
        }

        Debug.Log($"UndoUniversal → Remaining: {universalHistory.Count}");
    }


    // ========================================
    // CLEAR ALL HISTORY
    // ========================================
    public void ClearHistory()
    {
        positionHistory.Clear();
        objectHistory.Clear();
        universalHistory.Clear();

        Debug.Log("Cleared ALL history.");
    }


    // ========================================
    // CLEAR ONLY SPAWNED OBJECTS
    // ========================================
    public void ClearDeadBodies()
    {
        foreach (GameObject obj in objectHistory)
        {
            if (obj != null)
                Destroy(obj);
        }

        objectHistory.Clear();

        Debug.Log("All spawned objects destroyed.");
    }

    public void PrintAllHistory()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("========== HISTORY DEBUG ==========");

        // --- POSITION HISTORY ---
        sb.AppendLine($"PositionHistory Count = {positionHistory.Count}");
        for (int i = 0; i < positionHistory.Count; i++)
        {
            sb.AppendLine($"  [Position Snapshot {i}]");
            var snap = positionHistory[i];
            for (int j = 0; j < snap.Count; j++)
                sb.AppendLine($"     Child {j}: {snap[j]}");
        }

        // --- OBJECT HISTORY ---
        sb.AppendLine($"ObjectHistory Count = {objectHistory.Count}");
        for (int i = 0; i < objectHistory.Count; i++)
        {
            string name = objectHistory[i] != null ? objectHistory[i].name : "(destroyed)";
            sb.AppendLine($"   [{i}] {name}");
        }

        // --- UNIVERSAL HISTORY ---
        sb.AppendLine($"UniversalHistory Count = {universalHistory.Count}");
        for (int i = 0; i < universalHistory.Count; i++)
        {
            sb.AppendLine($"  [Universal Snapshot {i}] Entries = {universalHistory[i].Count}");

            foreach (var entry in universalHistory[i])
            {
                string targetName = (entry.target as MonoBehaviour != null)
                    ? (entry.target as MonoBehaviour).name
                    : "(null target)";

                sb.AppendLine($"     Target={targetName} | Int={entry.data.intValue} | Pos={entry.data.position}");
            }
        }

        sb.AppendLine("===================================");

        Debug.Log(sb.ToString());
    }

}
