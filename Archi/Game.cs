using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Graphics;

namespace Archi
{
    public class Game : GameWindow
    {
        Shader fragShader;
        Shader vertShader;

        Camera camera;
        Vector2 lastMousePos = new Vector2();


        

        private string fsCode;
        private string vsCode;

        public Game()
    : base(800, // initial width
        800, // initial height
        GraphicsMode.Default,
        "memes",  // initial title
        GameWindowFlags.Default,
        DisplayDevice.Default,
        4, // OpenGL major version
        0, // OpenGL minor version
        GraphicsContextFlags.ForwardCompatible) {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            Console.WriteLine("GLSL Version: " + GL.GetString(StringName.Version));
            Title = "Ray-Marching";

            vsCode = System.IO.File.ReadAllText(@"vs.vert");
            vertShader = new Shader(vsCode, Shader.Type.Vertex);

            Shader.Bind(vertShader);

            fsCode = System.IO.File.ReadAllText(@"raymarch.frag");
            fragShader = new Shader(fsCode, Shader.Type.Fragment);
            fragShader.SetVariable("viewportSize", new Vector2(ClientRectangle.Width, ClientRectangle.Height));
            Shader.Bind(fragShader);
            VSync = VSyncMode.Off;
            GL.ClearColor(Color.CornflowerBlue);

            camera = new Camera();
            camera.bindShader(fragShader);
        }

        private void ResetCursor() {
            OpenTK.Input.Mouse.SetPosition(Bounds.Left + Bounds.Width / 2, Bounds.Top + Bounds.Height / 2);
            lastMousePos = new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e) {
            base.OnUpdateFrame(e);

            //Check keyboard controls
            var keys = Keyboard.GetState();

            if (keys.IsKeyDown(OpenTK.Input.Key.Escape)) {
                Exit();
            }

            if (keys.IsKeyDown(OpenTK.Input.Key.D)) {
                camera.smoothTranslate(0, 1f, 0f);
            }
            if (keys.IsKeyDown(OpenTK.Input.Key.A)) {
                camera.smoothTranslate(0, -1f, 0f);
            }
            if (keys.IsKeyDown(OpenTK.Input.Key.W)) {
                camera.smoothTranslate(1, 0f, 0f);
            }
            if (keys.IsKeyDown(OpenTK.Input.Key.S)) {
                camera.smoothTranslate(-1, 0f, 0f);
            }
            if (keys.IsKeyDown(OpenTK.Input.Key.E)) {
                camera.smoothRotate(0f, 0f, 3f);
            }
            if (keys.IsKeyDown(OpenTK.Input.Key.Q)) {
                camera.smoothRotate(0, 0f, -3f);
            }

            //Check mouse controls
            if (Focused) {
                Vector2 delta = lastMousePos - new Vector2(OpenTK.Input.Mouse.GetState().X, OpenTK.Input.Mouse.GetState().Y);
                camera.smoothRotate(-delta.X, delta.Y, 0);

                ResetCursor();
            }
            camera.inertCamera();
            camera.bindShader(fragShader);
        }

        double netTime = 0f;
        double minFrame = 0f;

        protected override void OnRenderFrame(FrameEventArgs e) {
            //base.OnRenderFrame(e);
            Title = $"(Vsync: {VSync}) FPS: {minFrame:0}";

            netTime += e.Time;        
            if(netTime>1) {
                netTime = 0;
                minFrame = double.MaxValue;
            }
            minFrame = Math.Min(minFrame, 1f / e.Time);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(-1, -1);
            GL.Vertex2(1, -1);
            GL.Vertex2(1, 1);
            GL.Vertex2(-1, 1);
            GL.End();

            GL.Flush();

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            fragShader.SetVariable("viewportSize", new Vector2(ClientRectangle.Width, ClientRectangle.Height));
            Shader.Bind(fragShader);
            //Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            // GL.MatrixMode(MatrixMode.Projection);
            GL.MatrixMode(MatrixMode.Texture);
            //GL.LoadMatrix(ref projection);
        }

        private void rayMarchSetup() {
            GL.ClearColor(Color.CornflowerBlue);
        }
    }
}
