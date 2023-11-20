using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// The paint functionality works with five scripts:
// - PaintableShader: It is the shader applied to the material selected in the gameobject we want to paint (Paintable Material).
// The goal of this shader is to interpolate the MainTexture that can be seen before painting, with the MaskTexture that starts
// as an empty texture ("transparent") and gets modified while painting. The interpolation depends on this MaskTexture alpha value.
// Higher alphas favours the MaskTexture vs the MainTexture. Therefore, at the beginning we can only see the MainTexture (since the MaskTexture
// is transparent with alpha = 0 in all points).
// - PaintManager: This file. It is in charge of telling the shader that paints the transparent mask, what are the values of the painting parame
// ters, so the shader can paint this mask. In order to do it, it is completely necessary to use a "support" texture that will be used to accummulate
// the effects of each paint call, as explained later in this file. This script is Monobehaviour and can be set in any object enabled once the painting
// process start. In this case it is attached to the sphere in this folder
// - Paintable: script attached to any object we want to paint. It basically defines both mask and support texture that will be used on that object, so
// this paint manager can call them in order to do the painting process. It also defines the renderer of the gameobject, which is also needed by this Paint
// Manager script.
// - TextureMaskPainter: it is the shader that paints the transparent mask as explained later in the current script. The main piece of code is the mask
// function, which basically generates values close to 1 in the points close to the painting point. These 1s will be used to interpolate between the old value
// of the texture passed through that shader (the mask texture) and the paint color. Therefore, points with interpolation values closer to 1 will get a color
// close to the paint color.
// - MousePainter: it is just an example to generate events that call the paint function, a mouse click in this case, which will generate 1s in the mask function
// in the areas closer to the clicking point. But this point can be defined with any other event, as for instance an object hitting a surface.

// There is a small bug in this functionality, since in the limits of the UVs map a thin line without paint can be appreciated. In order to solve it an additional 
// shader can be applied (the "ExtendIslands" shader of this link https://github.com/mixandjam/Splatoon-Ink/tree/main/Assets/Shaders), and the paint manager and
// paintable must be modified too to take it into account (similar to how is done here https://github.com/mixandjam/Splatoon-Ink/tree/main/Assets/Scripts). 
// This video also explains a little bit the whole process https://www.youtube.com/watch?v=YUWfHX_ZNCw&t=202s
// This logic cannot be used to directly paint meshes generated with LightShip ARDK, I think because it depends a lot of how the UV values of the textures are 
// used in TextureMaskPainter, and probably they are not saved as needed by this script in the case of the ARDK Mesh. I tried to change these scripts, with a lot
// of combinations (two days making tests) and I wasn't successful, so..."DON'T TRY TO MODIFY THEM AGAIN!". If you really want to paint the scanned mesh with
// this script, it is better if you understand first how the UV of the mesh is generated and make the corresponding modifications so the TextureMaskPainter works
// properly. I didn't find a lot information about that though.

public class PaintManager : MonoBehaviour
{

    // Defining a static shared instance variable so other scripts can access to this unique class
    private static PaintManager _sharedInstance;
    public static PaintManager SharedInstance => _sharedInstance;

    public Shader texturePaint; // This must be set in the inspector as the shader that will paint the mask texture of the gameobject, the TextureMaskPaint in this case
    Material paintMaskMaterial;
    CommandBuffer command;

    int positionID = Shader.PropertyToID("_PainterPosition");
    int hardnessID = Shader.PropertyToID("_Hardness");
    int strengthID = Shader.PropertyToID("_Strength");
    int radiusID = Shader.PropertyToID("_Radius");
    int painterColorID = Shader.PropertyToID("_PainterColor");


    public void Awake()
    {
        if (_sharedInstance != null && _sharedInstance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _sharedInstance = this;
        }

        // Generate the material that will be used in the paint function to "paint" the initially transparent mask texture
        paintMaskMaterial = new Material(texturePaint);
        command = new CommandBuffer();
        command.name = "CommmandBuffer - " + gameObject.name;
    }

    // UGLY
    //public void paint(Paintable paintable, Vector4[] pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)

    // This function is call for the event that is goint to paint (a mouse, an object hitting a wall, etc.)
    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        // Setting the parameters of the mask funciton in the shader of this material (TextureMaskPainter.shader)
        // The goal of this maskfunction is basically have values close to 1, in points close to the painting positions
        // This value is later used in a interpolation that favours the painting color when it is close to 1
        paintMaskMaterial.SetVector(positionID, pos); // position of the painting object (mouse click, object hitting, etc.)
        paintMaskMaterial.SetFloat(hardnessID, hardness);
        paintMaskMaterial.SetFloat(strengthID, strength);
        paintMaskMaterial.SetFloat(radiusID, radius);
        paintMaskMaterial.SetColor(painterColorID, color ?? Color.red); 
        
        
        // Setting the support texture resultant of the previous paint call as the new Main Texture of the shader that paints the mask
        paintMaskMaterial.SetTexture("_MainTex", support);

        // All these 3 functions are run once Graphics.ExecuteCommandBuffer(command) is call, one after another
        command.SetRenderTarget(mask); // This line tells the program that the function of the following line will be rendered on the mask texture
        command.DrawRenderer(rend, paintMaskMaterial, 0); // The mask texture is passed trough the paintMaskMaterial shader and changes its value 
        // according to the transformations made in this shader (i.e, depending on the values of the variables of the mask function, it will get
        // a new "dot" of paint. Take into account that the MainTex of the shader was just set to the support texture on the previous "paint" call,
        // so this new iteration is made on the latest updated "drawing"
        command.Blit(mask, support); // Copying the mask value in the support texture, which will be set as _MainTex in the next "paint" call, generating
        // the cummulative effect that recreates the painting

        Graphics.ExecuteCommandBuffer(command);
        command.Clear();

    }

}
