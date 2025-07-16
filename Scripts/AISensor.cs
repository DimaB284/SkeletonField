using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AISensor : MonoBehaviour
{
    public float distance = 10;
    public float angle = 30;
    public float height = 1.0f;
    public Color meshColor = Color.green;
    public int scanFrequency = 30;
    public LayerMask layers;

    public float yRotationOffset = 0f; // Додаю можливість обертати сектор поля зору лише по Y

    Collider [] colliders = new Collider[50];
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;
    // Start is called before the first frame update
    void Start()
    {
		scanInterval = 1.0f / scanFrequency;

	}

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
    }

    Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numOfTriangles = (segments * 4) + 2 + 2;
        int numOfVertices = numOfTriangles * 3;
        Vector3[] vertices = new Vector3[numOfVertices]; 
        int[] triangles = new int[numOfVertices]; 

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
		Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;

        //left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

		//right side
		vertices[vert++] = bottomCenter;
		vertices[vert++] = topCenter;
		vertices[vert++] = topRight;

		vertices[vert++] = topRight;
		vertices[vert++] = bottomRight;
		vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (currentAngle * 2) / segments;
        for (int i = 0; i < segments; ++i)
        {
			bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
			bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

			topLeft = bottomLeft + Vector3.up * height;
			topRight = bottomRight + Vector3.up * height;


			//far side
			vertices[vert++] = bottomLeft;
			vertices[vert++] = bottomRight;
			vertices[vert++] = topRight;

			vertices[vert++] = topRight;
			vertices[vert++] = topLeft;
			vertices[vert++] = bottomLeft;

			//top
			vertices[vert++] = topCenter;
			vertices[vert++] = topLeft;
			vertices[vert++] = topRight;

			//bottom
			vertices[vert++] = bottomCenter;
			vertices[vert++] = bottomRight;
			vertices[vert++] = bottomLeft;


			currentAngle += deltaAngle;
        }

        for (int i = 0; i < numOfVertices; ++i)
        {
            triangles[i] = i;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

		return mesh;
    }

	private void OnValidate()
	{
		mesh = CreateWedgeMesh();
	}

	private void OnDrawGizmos()
	{
		if (mesh)
        {
            Gizmos.color = meshColor;
            // Додаємо обертання offset по Y
            Quaternion rot = transform.rotation * Quaternion.Euler(0, yRotationOffset, 0);
            Gizmos.DrawMesh(mesh, transform.position, rot);
        }
        Gizmos.DrawWireSphere(transform.position, distance);
        /*for (int i = 0; i < scanFrequency; ++i)
        {

        }*/
	}
}
