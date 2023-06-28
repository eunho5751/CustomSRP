using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
    }
}