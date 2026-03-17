using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
public class Volver : MonoBehaviour
{
    public string escenaQueCarga;
    public void IrAEscena()
    {
        Debug.Log("Cargando escena: " + escenaQueCarga);
       MMSceneLoadingManager.LoadScene(escenaQueCarga);
    }
     public void SalirDeLaApp()
    {
        Debug.Log("Saliendo de la aplicación");
        Application.Quit();
    }
}
