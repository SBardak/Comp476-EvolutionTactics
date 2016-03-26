using UnityEngine;
using System.Collections;

public interface IState {
    void CheckState();
    void Process();
    void ExecuteAction();

    void Reset();
}
