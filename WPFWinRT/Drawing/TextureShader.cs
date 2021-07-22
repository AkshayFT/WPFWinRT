using System;
using System.Linq;
using System.Windows;
using OpenTK.Graphics.OpenGL4;

public class TextureShader: Shader
{
    private readonly float[] _vertices =
    {
            // Position         Texture coordinates
             1.0f,  1.0f, 1.0f, 1.0f, // top right
             1.0f, -1.0f, 1.0f, 0.0f, // bottom right
            -1.0f,  1.0f, 0.0f, 1.0f,  // top left
            -1.0f, -1.0f, 0.0f, 0.0f // bottom left
        };

    private readonly uint[] _indices = {
            0, 1, 2, 3
        };

    private int _elementBufferObject;
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    public TextureShader(): base("Drawing/Shaders/textureShader.vert", "Drawing/Shaders/textureShader.frag")
	{
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        _vertexBufferObject = GL.GenBuffer();
        _elementBufferObject = GL.GenBuffer();
    }

    public void drawTexture(int texture)
    {
        this.Use();
        GL.BindVertexArray(_vertexArrayObject);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        var vertexLocation = GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        var texCoordLocation = GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.DrawElements(PrimitiveType.TriangleStrip, _indices.Length, DrawElementsType.UnsignedInt, 0);
        GL.DisableVertexAttribArray(vertexLocation);
        GL.DisableVertexAttribArray(texCoordLocation);
    }

    public void destroy()
    {
        //TODO: destroy buffers here, if created.
    }
}
