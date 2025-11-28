using UnityEngine;
using System.Collections;

public abstract class Agent : MonoBehaviour
{
    [Header("Agent Settings")]
    public float moveSpeed = 3f;

    [Header("Identification")]
    [SerializeField] protected string agentLabel;
    [SerializeField] private float labelYOffset = -0.6f;
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private int labelFontSize = 32;
    [SerializeField] private float labelCharacterSize = 0.12f;

    public Ingredient currentIngredient { get; protected set; }
    public GameObject currentPlate { get; protected set; }
    protected Vector3 targetPosition;
    public bool isMoving { get; protected set; } = false;
    protected bool isWorking = false;

    private TextMesh labelMesh;

    protected enum AgentState
    {
        Idle,
        Moving,
        Working,
        Waiting
    }

    protected AgentState currentState = AgentState.Idle;

    protected virtual void Start()
    {
        InitializeLabel();
    }

    protected virtual void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        
        // S'assurer que les ingrédients et assiettes portés restent visibles
        if (currentIngredient != null && currentIngredient.GameObject != null)
        {
            currentIngredient.GameObject.SetActive(true);
            if (currentIngredient.SpriteRenderer != null)
            {
                currentIngredient.SpriteRenderer.sortingOrder = 2;
            }
        }
        
        if (currentPlate != null)
        {
            currentPlate.SetActive(true);
            SpriteRenderer sr = currentPlate.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 2;
            }
        }
    }

    protected void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            isMoving = false;
            OnReachedTarget();
        }
    }

    protected virtual void OnReachedTarget()
    {
        currentState = AgentState.Idle;
    }

    protected void MoveTo(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
        currentState = AgentState.Moving;
    }

    protected void MoveTo(Transform target)
    {
        if (target != null)
        {
            MoveTo(target.position);
        }
    }

    protected void PickUpIngredient(Ingredient ingredient)
    {
        currentIngredient = ingredient;
        if (ingredient != null && ingredient.GameObject != null)
        {
            ingredient.GameObject.transform.SetParent(transform);
            ingredient.GameObject.transform.localPosition = Vector3.up * 0.5f; // Au-dessus de l'agent
            ingredient.GameObject.SetActive(true); // S'assurer que l'ingrédient est visible
            
            // S'assurer que le sortingOrder est correct
            if (ingredient.SpriteRenderer != null)
            {
                ingredient.SpriteRenderer.sortingOrder = 2;
            }
        }
    }

    protected void DropIngredient()
    {
        if (currentIngredient != null && currentIngredient.GameObject != null)
        {
            currentIngredient.GameObject.transform.SetParent(null);
        }
        currentIngredient = null;
    }

    protected void PickUpPlate(GameObject plate)
    {
        currentPlate = plate;
        if (plate != null)
        {
            plate.transform.SetParent(transform);
            plate.transform.localPosition = Vector3.up * 0.5f;
            plate.SetActive(true); // S'assurer que l'assiette est visible
            
            // S'assurer que le sortingOrder est correct
            SpriteRenderer sr = plate.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 2;
            }
        }
    }

    protected void DropPlate()
    {
        if (currentPlate != null)
        {
            currentPlate.transform.SetParent(null);
        }
        currentPlate = null;
    }

    protected IEnumerator Wait(float seconds)
    {
        currentState = AgentState.Waiting;
        yield return new WaitForSeconds(seconds);
        currentState = AgentState.Idle;
    }

    private void InitializeLabel()
    {
        if (labelMesh != null)
        {
            return;
        }

        string labelText = string.IsNullOrEmpty(agentLabel) ? gameObject.name : agentLabel;

        GameObject labelGO = new GameObject("AgentLabel");
        labelGO.transform.SetParent(transform);
        labelGO.transform.localPosition = new Vector3(0f, labelYOffset, 0f);
        labelGO.transform.localRotation = Quaternion.identity;

        labelMesh = labelGO.AddComponent<TextMesh>();
        labelMesh.text = labelText;
        labelMesh.anchor = TextAnchor.UpperCenter;
        labelMesh.alignment = TextAlignment.Center;
        labelMesh.color = labelColor;
        labelMesh.fontSize = labelFontSize;
        labelMesh.characterSize = labelCharacterSize;

        MeshRenderer meshRenderer = labelGO.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 3;
        }
    }
}

