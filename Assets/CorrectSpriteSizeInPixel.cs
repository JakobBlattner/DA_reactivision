using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrectSpriteSizeInPixel : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //get world space size (this version handles rotating correctly)
        Vector2 sprite_size = GetComponent<SpriteRenderer>().sprite.rect.size;
        Vector2 local_sprite_size = sprite_size / GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        Vector3 world_size = local_sprite_size;
        world_size.x *= transform.lossyScale.x;
        world_size.y *= transform.lossyScale.y;

        //convert to screen space size
        Vector3 screen_size = 0.5f * world_size / Camera.main.orthographicSize;
        screen_size.y *= Camera.main.aspect;

        //size in pixels
        Vector3 in_pixels = new Vector3(screen_size.x * Camera.main.pixelWidth, screen_size.y * Camera.main.pixelHeight, 0) * 0.5f;

        Debug.Log(string.Format("World size: {0}, Screen size: {1}, Pixel size: {2}", world_size, screen_size, in_pixels));

        screen_size = new Vector3(in_pixels.x / Camera.main.pixelWidth, in_pixels.y / Camera.main.pixelHeight, 0) / 0.5f;
        //screen_size.y /= Camera.main.aspect;
        world_size = (screen_size * Camera.main.orthographicSize) / 0.5f;
        sprite_size = world_size * GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        sprite_size.x /= transform.lossyScale.x;
        //sprite_size.y /= transform.lossyScale.y;
        Debug.Log("Used sprite size in pixels: " + sprite_size.x);

        //in_pixels = new Vector3(77.5f, 77.5f, 1);
        //world_size.x 
        float result = (((in_pixels.x / Camera.main.pixelWidth / 0.5f) * Camera.main.orthographicSize / 0.5f) * GetComponent<SpriteRenderer>().sprite.pixelsPerUnit) / transform.lossyScale.x;
        //Debug.Log(result);

        result = (((155 / Camera.main.pixelWidth / 0.5f) * Camera.main.orthographicSize / 0.5f) * GetComponent<SpriteRenderer>().sprite.pixelsPerUnit) / 200;
        Debug.Log(result);
    }
}
