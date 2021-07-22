using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Canvas : GameWindow
{

    private Size WindowSize;

    private TextureShader textureShader;
    private HighlighterShader highlighterShader;
    private DrawingShader drawingShader;
    private bool isWriting;
    
    private Texture _finalRenderTexture;
    private Texture _highlighterTexture;
    private int _finalRenderFrameBuffer;
    private int _highlighterFrameBuffer;
    public Texture _backgroundTexture;
    private bool _isBackgroundDrawn;
    private List<PointF> transientPoints = new List<PointF>();

    public bool isHighlighter = true;
    public Canvas(int width, int height, string title)
       : base(
             new GameWindowSettings(),
             new NativeWindowSettings()
             {
                 Size = new OpenTK.Mathematics.Vector2i(width, height),
                 Title = title
             })
    {
        WindowSize = new Size(width, height);

        TabletDevice myCurrentTabletDevice = Tablet.CurrentTabletDevice;
        if(myCurrentTabletDevice != null)
        {
            Debug.WriteLine($"Debug Window {myCurrentTabletDevice.Name}");
        }
        
    }

    protected override void OnLoad()
    {
        GL.Viewport(0, 0, this.WindowSize.Width, this.WindowSize.Height);
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        //Initialize Shaders/Renderers
        textureShader = new TextureShader();
        drawingShader = new DrawingShader();
        highlighterShader = new HighlighterShader();

        //Generate Textures
        _finalRenderTexture = Texture.CreateTexture(this.WindowSize.Width, this.WindowSize.Height);
        _finalRenderFrameBuffer = _finalRenderTexture.FrameBufferObject();
        ClearFrameBuffer(_finalRenderFrameBuffer, Color.Green);

        // Background texture
        // _backgroundTexture = Texture.LoadFromFile("Drawing/container.png");

        //Highlighter texture
        _highlighterTexture = Texture.CreateTexture(this.WindowSize.Width, this.WindowSize.Height);
        _highlighterFrameBuffer = _highlighterTexture.FrameBufferObject();
        ClearFrameBuffer(_highlighterFrameBuffer, Color.Transparent);

        base.OnLoad();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        //Draw Background Texture on Final Render Texture       
        if (!_isBackgroundDrawn)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _finalRenderFrameBuffer);
            _isBackgroundDrawn = true;
            textureShader.drawTexture(_backgroundTexture.Handle);
        }
        //For highlighter, bind highlighter framebuffer
        if (isHighlighter)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _highlighterFrameBuffer);
        }
        else
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _finalRenderFrameBuffer);
        }

        //Draw Stroke
        drawingShader.drawCurrentStrokePendingPoints();

        // Draw Highlighter texture on to final render texture
        if (isHighlighter)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _finalRenderFrameBuffer);
            highlighterShader.drawTexture(_highlighterTexture.Handle);
            GL.Disable(EnableCap.Blend);
        }

        //Draw Final Render Texture on Screen
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        textureShader.drawTexture(_finalRenderTexture.Handle);

        //Draw Transient Stroke
        drawingShader.DrawTransientPoints(transientPoints);
        transientPoints.Clear();
        //Mandatory steps to reuse the Screen Frame Buffer
        SwapBuffers();
        base.OnRenderFrame(e);
        GL.Flush();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        Size newSize = new Size(e.Width, e.Height);
        if(newSize != this.WindowSize)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            this.WindowSize.Width = e.Width;
            this.WindowSize.Height = e.Height;

            _finalRenderTexture = Texture.CreateTexture(this.WindowSize.Width, this.WindowSize.Height);
            _finalRenderFrameBuffer = _finalRenderTexture.FrameBufferObject();
            ClearFrameBuffer(_finalRenderFrameBuffer, Color.AntiqueWhite);
        }
        base.OnResize(e);
    }
    protected override void OnUnload()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteTexture(_finalRenderTexture.Handle);
        base.OnUnload();
    }
    protected override void OnMouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
        {
          //  VSync = VSyncMode.On;
            drawingShader.StartWriting(this.WindowSize);
            isWriting = true;
        }
    }
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        if (isWriting)
        {
            PointF pCurrent = new PointF(e.X, e.Y);
            drawingShader.WritePoint(pCurrent);

            transientPoints.Add(Normalize(e.X, e.Y));
        }
    }

    protected override void OnMouseUp(OpenTK.Windowing.Common.MouseButtonEventArgs e)
    {
        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
        {
            drawingShader.StopWriting();
            isWriting = false;
            //VSync = VSyncMode.Off;
        }
    }
    private void ClearFrameBuffer(int buffer, Color color)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, buffer);
        GL.ClearColor(color);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Flush();
    }

    private PointF Normalize(float x, float y)
    {
        float xN = (x - this.Size.X / 2f) / (this.Size.X / 2f);
        float yN = -(y - this.Size.Y / 2f) / (this.Size.Y / 2f);
        return new PointF(xN, yN);
    }
}