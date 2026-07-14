using UnityEngine;

public class TextureMovement_SkyDomes : MonoBehaviour
{
	public float movementSpeed = 0.5f;
	public Sprite[] spriteArray;
	private int currentSpriteIndex = 0;

	private Renderer materialRenderer;
	private Material material;
	private SpriteRenderer spriteRenderer;

	void Start()
{
		// Get the Renderer component and the associated material
		materialRenderer = GetComponent<Renderer>();
		material = materialRenderer.material;

		// Get the SpriteRenderer component
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Set the initial sprite
		if (spriteArray.Length > 0)
		{
			spriteRenderer.sprite = spriteArray[currentSpriteIndex];
		}
	}

	void Update()
	{
		// Calculate the new value for OffsetX based on time
		float offsetX = Time.time * movementSpeed;

		// Update the material with the new OffsetX value
		material.SetTextureOffset("_MainTex", new Vector2(offsetX, 0f));

		// Check for sprite changing input
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ChangeSprite(1); // Move to the next sprite
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ChangeSprite(-1); // Move to the previous sprite
		}
	}

	void ChangeSprite(int direction)
	{
		// Update the current index based on the direction
		currentSpriteIndex += direction;

		// Check for bounds to avoid index out of range
		if (currentSpriteIndex < 0)
		{
			currentSpriteIndex = 0;
		}
		else if (currentSpriteIndex >= spriteArray.Length)
		{
			currentSpriteIndex = spriteArray.Length - 1;
		}

		// Update the sprite
		spriteRenderer.sprite = spriteArray[currentSpriteIndex];
	}
}
