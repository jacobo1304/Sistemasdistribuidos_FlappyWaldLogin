using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;
public class ControlMonedas : MonoBehaviour, MMEventListener<PickableItemEvent>
{
    private int contadorMonedas = 0;
    [SerializeField]
    private CharacterInstantiate characterInstantiate;
void OnEnable()
{
this.MMEventStartListening<PickableItemEvent>();
}
void OnDisable()
{
this.MMEventStopListening<PickableItemEvent>();
}

void Start()
{
    
    contadorMonedas = 0;
}
public virtual void OnMMEvent(PickableItemEvent e)
{
        Coin coin = e.PickedItem.GetComponent<Coin>();
        Debug.Log("Hit");
        if (coin != null)
        {
            Debug.Log(coin.PointsToAdd);
            Debug.Log("Has recogido una moneda" + coin.PointsToAdd);
            contadorMonedas++;
            Debug.Log("Monedas recogidas: " + contadorMonedas);
        }
        else
        {
            Debug.Log("No es una moneda");
        }

        if (contadorMonedas == 3)
        {
            Character character = characterInstantiate.GetCharacter();
            Debug.Log("Has recogido todas las monedas");
            Debug.Log("Vida antes de la curación" + character.CharacterHealth.CurrentHealth);
            character.CharacterHealth.ResetHealthToMaxHealth();
            Debug.Log("Has recuperado toda tu vida, ahora tu vida es: " + character.CharacterHealth.MaximumHealth);
        }

    }
}