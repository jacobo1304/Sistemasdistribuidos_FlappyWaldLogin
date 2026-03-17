using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System;

public class MecanicaFinNivel : MonoBehaviour, MMEventListener<PickableItemEvent>
{
    [SerializeField]
    private GameObject puerta;
    [SerializeField]
    private GameObject easterEggPanel;
    [SerializeField]
    private CharacterInstantiate characterInstantiate;
    [SerializeField]
    private string escenaQueCarga;

    private Character character; // Referencia al personaje

    public void IrAEscena()
    {
        Debug.Log("Cargando escena: " + escenaQueCarga);
        MMSceneLoadingManager.LoadScene(escenaQueCarga);
    }

    void Start()
    {
        // Inicializamos la referencia al personaje al inicio
        character = characterInstantiate.GetCharacter();
        if (character == null)
        {
            Debug.LogWarning("No se pudo obtener el personaje desde CharacterInstantiate.");
        }
    }

    void OnEnable()
    {
        this.MMEventStartListening<PickableItemEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<PickableItemEvent>();
    }

    public virtual void OnMMEvent(PickableItemEvent e)
    {
        Stimpack stimpack = e.PickedItem.GetComponent<Stimpack>();

        if (stimpack != null)
        {
            switch (stimpack.name)
            {
                case "semilla":
                    Debug.Log("Has recogido la semilla, puerta abierta");
                    puerta.SetActive(true);
                    break;

                case "arbol":
                    Debug.Log("Has recogido el arbol transfiriendo a nivel 3");
                    IrAEscena();
                    break;

                case "Easter Egg":
                    Debug.Log("Has recogido el Easter Egg, mostrando panel");
                    easterEggPanel.SetActive(true);
                    break;

                case "banano":
                    Debug.Log("Has recogido el banano, aumentando velocidad");
                    AumentarVelocidad();
                    break;

                case "uva":
                    Debug.Log("Has recogido la uva, desbloqueando habilidad Jetpack");
                    DesbloquearJetpack();
                    break;

                case "cerezas":
                    Debug.Log("Has recogido las cerezas, Desbloqueando habilidad dash");
                    DesbloquearDash();
                    break;

                default:
                    Debug.Log("Has recogido el estimulante, pero no es la semilla, es " + stimpack.name);
                    break;
            }
        }
        else
        {
            Debug.Log("No es un estimulante, picked item es: " + e.PickedItem);
        }
    }

    private void AumentarVelocidad()
    {
        if (character != null)
        {
            CharacterHorizontalMovement characterMovement = character.FindAbility<CharacterHorizontalMovement>();
            if (characterMovement != null)
            {
                characterMovement.MovementSpeed *= 1.5f; // Incrementa la velocidad en un 50%
                Debug.Log("Nueva velocidad de movimiento: " + characterMovement.MovementSpeed);
            }
            else
            {
                Debug.LogWarning("El personaje no tiene la habilidad de movimiento horizontal.");
            }
        }
        else
        {
            Debug.LogWarning("No se pudo acceder al personaje para aumentar la velocidad.");
        }
    }

    private void DesbloquearJetpack()
    {
        if (character != null)
        {
            CharacterJetpack jetpack = character.GetComponent<CharacterJetpack>();
            if (jetpack != null)
            {
                jetpack.enabled = true; // Activa el componente Jetpack
                Debug.Log("Habilidad Jetpack activada");
            }
            else
            {
                Debug.LogWarning("El personaje no tiene el componente CharacterJetpack.");
            }
        }
        else
        {
            Debug.LogWarning("No se pudo acceder al personaje para desbloquear el Jetpack.");
        }
    }
    private void DesbloquearDash()
{
    if (character != null)
    {
        CharacterDash dash = character.GetComponent<CharacterDash>();
        if (dash != null)
        {
            dash.enabled = true; // Activa el componente Dash
            Debug.Log("Habilidad Dash activada");
        }
        else
        {
            Debug.LogWarning("El personaje no tiene el componente CharacterDash.");
        }
    }
    else
    {
        Debug.LogWarning("No se pudo acceder al personaje para desbloquear el Dash.");
    }
}
}