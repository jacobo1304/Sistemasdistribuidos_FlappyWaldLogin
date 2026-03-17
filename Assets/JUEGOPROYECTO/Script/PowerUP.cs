using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class PowerUP : MonoBehaviour , MMEventListener<CorgiEngineEvent>
{
    // Start is called before the first frame update
    private Character character;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        public Character GetCharacter()
    {
        return character;
    }
     public void SetCharacter(Character newCharacter)
    {
        character = newCharacter;
    }
     void OnEnable()
    {
        this.MMEventStartListening<CorgiEngineEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<CorgiEngineEvent>();
    }
    public virtual void OnMMEvent(CorgiEngineEvent engineEvent){
        
        switch (engineEvent.EventType)
        {
            case CorgiEngineEventTypes.LevelStart:
                Debug.Log(LevelManager.Instance.Players);
                character = LevelManager.Instance.Players[0]; //character instanciado cuando ocurre el evento level start

                break;
            default:
                break;
        }
    }
}
