using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Mathematics;

public class DrawingShader: Shader
{

    private readonly Texture _brushTexture;

	private PointF lastPoint = PointF.Empty;
    private List<PointF> points = new List<PointF>();
    private Size Size;
    private int _vertexArrayObject;

    public DrawingShader(): base("Drawing/Shaders/drawingShader.vert", "Drawing/Shaders/drawingShader.frag")
	{
        this.Use();
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        SetFloat("thickness", 20.0f);
        SetVector3("color", new Vector3(1.0f, 0.0f, 0.0f));
        SetVector2("viewPortSize", new Vector2((float)this.Size.Width, (float)this.Size.Height));

        _brushTexture = Texture.LoadFromFile("Assets/brush.png");
    }


    public void DrawTransientPoints(List<PointF> tPoints)
    {
        this.Use();
        GL.BindVertexArray(_vertexArrayObject);


        SetShaderUniforms(true);

        GL.EnableVertexAttribArray(0);
        GL.BufferData(BufferTarget.ArrayBuffer, tPoints.Count * sizeof(float) * 2, tPoints.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);


        EnableBlend();

        _brushTexture.Use(TextureUnit.Texture0);
        GL.DrawArrays(PrimitiveType.Points, 0, tPoints.Count);

        DisableBlend();

    }

    public void SetShaderUniforms(bool isTransient)
    {
        //Set Thickness, color and ViewportSize as Uniforms.
        if (isTransient)
        {
            SetVector3("color", new Vector3(1.0f, 0.0f, 0.0f));
            SetFloat("thickness", 50.0f);
        }
        else
        {
            SetVector3("color", new Vector3(0.0f, 1.0f, 0.0f));
            SetFloat("thickness", 30.0f);
        }
        SetVector2("viewPortSize", new Vector2((float)this.Size.Width, (float)this.Size.Height));
    }

    public void EnableBlend()
    {
        GL.Enable(EnableCap.ProgramPointSize);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public void DisableBlend()
    {
        //Disable the previously enabled States
        GL.Disable(EnableCap.Blend);
        GL.Disable(EnableCap.ProgramPointSize);
        GL.DisableVertexAttribArray(0);
    }

    public void drawCurrentStrokePendingPoints()
    {
        this.Use();
        GL.BindVertexArray(_vertexArrayObject);

        //Set Thickness, color and ViewportSize as Uniforms.
        SetShaderUniforms(false);

        GL.EnableVertexAttribArray(0);
        GL.BufferData(BufferTarget.ArrayBuffer, points.Count * sizeof(float) * 2, points.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false,  sizeof(float) * 2, 0);


        EnableBlend();

        _brushTexture.Use(TextureUnit.Texture0);
        GL.DrawArrays(PrimitiveType.Points, 0, points.Count);

        DisableBlend();

        //Temporarily Clear off everything, as these points are rendered
        points.Clear();
    }

    public void StartWriting(Size size)
    {
        this.Size = size;
		lastPoint = PointF.Empty;
        this.points.Clear();
    }

	public void WritePoint(PointF pCurrent)
    {

        if (lastPoint.IsEmpty)
        {
           PointF normalized = Normalize(pCurrent.X, pCurrent.Y);
           this.points.Add(normalized);
            //this.points.Add(pCurrent);
        }
        else
        {
            int count = (int)Math.Sqrt(Math.Pow((pCurrent.X - lastPoint.X), 2) + Math.Pow((pCurrent.Y - lastPoint.Y), 2));
            count = Math.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                float x = (lastPoint.X + (pCurrent.X - lastPoint.X) * ((float)i / (float)count));
                float y = (lastPoint.Y + (pCurrent.Y - lastPoint.Y) * ((float)i / (float)count));

                PointF pDerived = Normalize(x, y);
                this.points.Add(pDerived);
               // this.points.Add(new PointF(x, y));
            }
        }
        lastPoint = pCurrent;
    }
    private PointF Normalize(float x, float y)
    {
        float xN = (x - this.Size.Width / 2f) / (this.Size.Width / 2f);
        float yN = -(y - this.Size.Height / 2f) / (this.Size.Height / 2f);
        return new PointF(xN, yN);
    }

    public void StopWriting()
    {
	}
}
