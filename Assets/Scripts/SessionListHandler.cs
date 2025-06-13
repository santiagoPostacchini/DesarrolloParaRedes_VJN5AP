using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListHandler : MonoBehaviour
{
    [SerializeField] private SessionItem _sessionItemPrefab;
    
    [SerializeField] private NetworkRunnerHandler _networkRunnerHandler;

    [SerializeField] private TMP_Text _statusText;

    [SerializeField] private VerticalLayoutGroup _verticalLayout;

    private void OnEnable()
    {
        _networkRunnerHandler.OnSessionListUpdate += ReceiveSessionList;
    }

    private void OnDisable()
    {
        _networkRunnerHandler.OnSessionListUpdate -= ReceiveSessionList;
    }

    void ReceiveSessionList(List<SessionInfo> sessionList)
    {
        ClearBrowser();
        
        if (sessionList.Count == 0)
        {
            NoSessionsFound();
            return;
        }
        
        foreach (var sessionInfo in sessionList)
        {
            AddToSessionBrowser(sessionInfo);
        }
    }

    void ClearBrowser()
    {
        foreach (Transform child in _verticalLayout.transform)
        {
            Destroy(child.gameObject);
        }
        
        _statusText.gameObject.SetActive(false);
    }
    
    void NoSessionsFound()
    {
        _statusText.text = "No sessions found";
        _statusText.gameObject.SetActive(true);
    }

    void AddToSessionBrowser(SessionInfo sessionInfo)
    {
        var sessionItem = Instantiate(_sessionItemPrefab, _verticalLayout.transform);
        sessionItem.Initialize(sessionInfo);
        sessionItem.OnJoinSession += JoinSelectedSession;
    }

    void JoinSelectedSession(SessionInfo sessionInfo)
    {
        _networkRunnerHandler.JoinGame(sessionInfo);
    }
}
