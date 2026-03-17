using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrizPalabras : MonoBehaviour
{
    // Prefab de la moneda (se puede asignar desde el Inspector)
    [SerializeField] private GameObject monedaPrefab;

    // Palabra editable desde el inspector
    [SerializeField] private string palabra = "LOVE"; // Palabra que se mostrar�

    // Contenedor para las monedas (se puede asignar desde el Inspector)
    [SerializeField] private Transform contenedorMonedas;

    // Tama�o de las celdas en el mundo (se puede asignar desde el Inspector)
    [SerializeField] private float tamanoCelda = 2f;

    // Diccionario de letras y sus representaciones en matrices
    private Dictionary<char, int[,]> letras;

    void Start()
    {
        // Inicializamos el diccionario con las letras del abecedario
        letras = new Dictionary<char, int[,]>
        {
            {'A', new int[,]{
                {0, 1, 0},
                {1, 0, 1},
                {1, 1, 1},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'B', new int[,]{
                {1, 1, 0},
                {1, 0, 1},
                {1, 1, 0},
                {1, 0, 1},
                {1, 1, 0}
            }},
            {'C', new int[,]{
                {0, 1, 1},
                {1, 0, 0},
                {1, 0, 0},
                {1, 0, 0},
                {0, 1, 1}
            }},
            {'D', new int[,]{
                {1, 1, 0},
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1},
                {1, 1, 0}
            }},
            {'E', new int[,]{
                {1, 1, 1},
                {1, 0, 0},
                {1, 1, 1},
                {1, 0, 0},
                {1, 1, 1}
            }},
            {'F', new int[,]{
                {1, 1, 1},
                {1, 0, 0},
                {1, 1, 1},
                {1, 0, 0},
                {1, 0, 0}
            }},
            {'G', new int[,]{
                {0, 1, 1},
                {1, 0, 0},
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 1}
            }},
            {'H', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {1, 1, 1},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'I', new int[,]{
                {1, 1, 1},
                {0, 1, 0},
                {0, 1, 0},
                {0, 1, 0},
                {1, 1, 1}
            }},
            {'J', new int[,]{
                {0, 1, 1},
                {0, 0, 1},
                {0, 0, 1},
                {1, 0, 1},
                {0, 1, 0}
            }},
            {'K', new int[,]{
                {1, 0, 1},
                {1, 1, 0},
                {1, 0, 0},
                {1, 1, 0},
                {1, 0, 1}
            }},
            {'L', new int[,]{
                {1, 0, 0},
                {1, 0, 0},
                {1, 0, 0},
                {1, 0, 0},
                {1, 1, 1}
            }},
            {'M', new int[,]{
                {1, 0, 1},
                {1, 1, 1},
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'N', new int[,]{
                {1, 0, 1},
                {1, 1, 0},
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'O', new int[,]{
                {0, 1, 0},
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 0}
            }},
            {'P', new int[,]{
                {1, 1, 0},
                {1, 0, 1},
                {1, 1, 0},
                {1, 0, 0},
                {1, 0, 0}
            }},
            {'Q', new int[,]{
                {0, 1, 0},
                {1, 0, 1},
                {1, 0, 1},
                {1, 1, 1},
                {0, 0, 1}
            }},
            {'R', new int[,]{
                {1, 1, 0},
                {1, 0, 1},
                {1, 1, 0},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'S', new int[,]{
                {0, 1, 1},
                {1, 0, 0},
                {0, 1, 0},
                {0, 0, 1},
                {1, 1, 0}
            }},
            {'T', new int[,]{
                {1, 1, 1},
                {0, 1, 0},
                {0, 1, 0},
                {0, 1, 0},
                {0, 1, 0}
            }},
            {'U', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 0}
            }},
            {'V', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 0},
                {0, 1, 0}
            }},
            {'W', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {1, 1, 1},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'X', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 0},
                {1, 0, 1},
                {1, 0, 1}
            }},
            {'Y', new int[,]{
                {1, 0, 1},
                {1, 0, 1},
                {0, 1, 0},
                {0, 1, 0},
                {0, 1, 0}
            }},
            {'Z', new int[,]{
                {1, 1, 1},
                {0, 0, 1},
                {0, 1, 0},
                {1, 0, 0},
                {1, 1, 1}
            }}

        };
        CrearMonedas();
    }

    // Este m�todo se llama para crear las monedas
    public void CrearMonedas()
    {
        // Verificar si el prefab de moneda est� asignado
        if (monedaPrefab == null)
        {
            Debug.LogError("�El prefab de moneda no est� asignado!");
            return;
        }

        // Verificar si el contenedor de monedas est� asignado
        if (contenedorMonedas == null)
        {
            Debug.LogError("�El contenedor de monedas no est� asignado!");
            return;
        }

        // Borra las monedas previas, si las hay
        foreach (Transform child in contenedorMonedas)
        {
            Destroy(child.gameObject);
        }

        // Usamos la palabra del SerializedField
        string palabra = this.palabra.ToUpper(); // Convertir la palabra a may�sculas
        Debug.Log("Palabra a formar: " + palabra); // Verificar la palabra

        float offsetX = 0f; // Desplazamiento horizontal entre letras
        float offsetY = 0f; // Desplazamiento vertical para las letras

        // Recorre cada letra de la palabra
        foreach (char c in palabra)
        {
            Debug.Log("Procesando letra: " + c); // Mostrar la letra que se est� procesando

            if (letras.ContainsKey(c)) // Verifica si la letra est� definida en el diccionario
            {
                Debug.Log("Letra encontrada en el diccionario: " + c); // Verificar que la letra existe en el diccionario
                int[,] letra = letras[c]; // Obtiene la matriz de la letra

                // Recorre la matriz de la letra
                for (int i = 0; i < letra.GetLength(0); i++) // Filas
                {
                    for (int j = 0; j < letra.GetLength(1); j++) // Columnas
                    {
                        if (letra[i, j] == 1) // Si hay un 1, generamos una moneda
                        {
                            // Nueva posici�n en 2D, calculada correctamente para distribuir en matriz
                            Vector2 posicion = new Vector2(offsetX + j * tamanoCelda, -i * tamanoCelda + offsetY);
                            Debug.Log("Instanciando moneda en: " + posicion); // Verificar la posici�n de la moneda
                            Instantiate(monedaPrefab, posicion, Quaternion.identity, contenedorMonedas);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Letra no encontrada en el diccionario: " + c); // Si la letra no est� en el diccionario
            }

            // Incrementar el desplazamiento horizontal para la siguiente letra
            offsetX += (letras[c].GetLength(1) + 1) * tamanoCelda; // A�adir un peque�o espacio entre letras

            // Incrementar el desplazamiento vertical para separar las letras entre s�
            
        }
    }
}




