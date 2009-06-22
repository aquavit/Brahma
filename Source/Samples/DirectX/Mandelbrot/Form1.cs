#region License and Copyright Notice

//Brahma 2.0: Framework for streaming/parallel computing with an emphasis on GPGPU

//Copyright (c) 2007 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Brahma;
using Brahma.DirectX;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using Vector2=Brahma.Vector2;
using Vector4=Brahma.Vector4;

namespace Mandelbrot
{
    public partial class Form1: Form
    {
        // Constants for various factors, including the escape factor
        private const float DefaultIterations = 100f;
        private const float EscapeRadiusSquared = 4f;
        private const int WindowHeight = 512;
        private const int WindowWidth = 512;
        private const float ZoomFactor = 0.1f / 2;

        private readonly Visualizer.ColorMap _colorMap; // We're going to use this to color the fractal

        // All our queries
        private readonly CompiledQuery _mandelbrot; // This query performs the z * z + c calculations.
        private readonly CompiledQuery _mandelbrotVisualizer; // Converts the complex number data-parallel array to something suitable for visualization
        private readonly ComputationProvider _provider; // Our computation provider
        private readonly CompiledQuery _reset; // This query simply "resets" the value of our current buffer so that we can recalculate the zoomed area
        private float _bottom = -2f;

        private DataParallelArray2D<Vector4> _currentBuffer;

        private float _currentIteration;

        // Keep track of our zooming window
        private float _left = -2f;
        private float _maxIterations = DefaultIterations;
        private float _right = 2f;
        private float _top = 2f;

        public Form1()
        {
            InitializeComponent();

            ClientSize = new Size(WindowWidth, WindowHeight);
            _provider = new ComputationProvider(this);

            // Compile the queries

            // This query performs the z * z + c calculations.
            _mandelbrot = _provider.Compile<DataParallelArray2D<Vector4>>(
                d => from value in d
                     let z = new Vector2(value.x, value.y)
                     let coord = new Vector2(d.CurrentX / (float)WindowWidth, d.CurrentY / (float)WindowHeight)
                     let window_mins = new Vector2(_left, _bottom)
                     let window_extents = new Vector2(_right - _left, _top - _bottom)
                     let c = window_mins + coord * window_extents
                     let zModulusSquared = z.x * z.x + z.y * z.y
                     select zModulusSquared > EscapeRadiusSquared * 8f
                                ? value
                                : new Vector4
                                      {
                                          x = z.x * z.x - z.y * z.y + c.x,
                                          y = 2f * z.x * z.y + c.y,
                                          z = _currentIteration,
                                          w = 0f
                                      }
                );

            // Converts the complex number data-parallel array to something suitable for visualization
            // This now visualizes smoothly, thanks to changes by WeirdBro
            _mandelbrotVisualizer =
                _provider.Compile<DataParallelArray2D<Vector4>>(
                    d => from value in d
                         let z = new Vector2(value.x, value.y)
                         let zModulusSquared = z.x * z.x + z.y * z.y
                         select zModulusSquared > EscapeRadiusSquared
                                    ? ((value.z) + 1 - (Math.Log(Math.Log(Math.Sqrt(zModulusSquared))) / Math.Log(2))) / _maxIterations
                                    : 0f
                    );

            // This query simply "resets" the value of our current buffer so that we can recalculate the zoomed area
            _reset = _provider.Compile<DataParallelArray2D<Vector4>>(
                d => from value in d
                     select new Vector4
                                {
                                    x = _left + ((d.CurrentX / (float)WindowWidth) * (_right - _left)),
                                    y = _bottom + (d.CurrentY / (float)WindowHeight) * (_top - _bottom),
                                    z = 0f,
                                    w = 0f
                                }
                );

            // Set up the data-parallel array with initial values
            _currentBuffer = new DataParallelArray2D<Vector4>(_provider, WindowWidth, WindowHeight,
                                                              (x, y) =>
                                                              new Vector4
                                                                  {
                                                                      x = _left + ((x / (float)WindowWidth) * (_right - _left)),
                                                                      y = _bottom + (y / (float)WindowHeight) * (_top - _bottom),
                                                                      z = 0f,
                                                                      w = 0f
                                                                  }
                );

            // Create a color map so we can "visualize" it better
            _colorMap = Visualizer.ColorMap.FromFile(_provider, AppDomain.CurrentDomain.BaseDirectory + "colormap2.jpg");

            MessageBox.Show(this, "Use the mouse wheel to increase and decrease number of iterations, and left-click on a point to go 10x into it. Right-click to reset");

            renderTimer.Enabled = true; // Begin rendering
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            renderTimer.Enabled = false;

            // Dispose everything
            _currentBuffer.Dispose();
            _mandelbrot.Dispose();
            _mandelbrotVisualizer.Dispose();
            _colorMap.Dispose();
            _provider.Dispose();
        }

        private void Render()
        {
            if (_currentIteration < _maxIterations)
            {
                _currentIteration++; // Set this field, it is used by the shader
                var result = _provider.Run(_mandelbrot, _currentBuffer) as DataParallelArray2D<Vector4>;

                _currentBuffer.Dispose();
                _currentBuffer = result; // current output = next input
            }

            // Convert it to a visual
            var visual = _provider.Run(_mandelbrotVisualizer, _currentBuffer) as DataParallelArray2D<float>;
            Visualizer.Display(_provider, visual, _colorMap);
            visual.Dispose();
        }

        private void Redraw()
        {
            renderTimer.Enabled = false; // Stop rendering for a bit

            _currentIteration = 0;

            var result = _provider.Run(_reset, _currentBuffer) as DataParallelArray2D<Vector4>; // Reset all values
            _currentBuffer.Dispose(); // Dispose this
            _currentBuffer = result; // Swap

            renderTimer.Enabled = true; // Render
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            Render(); // We don't want an update
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            _maxIterations += 10 * Math.Sign(e.Delta);
            Redraw();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    // Zooming is fixed thanks to WierdBro
                    float width = _right - _left;
                    float height = _top - _bottom;
                    float x = _left + e.X * width / WindowWidth;
                    float y = _bottom + e.Y * height / WindowHeight;
                    _left = x - ZoomFactor * width;
                    _right = x + ZoomFactor * width;
                    _top = y + ZoomFactor * height;
                    _bottom = y - ZoomFactor * height;

                    _provider.Device.SetTransform(TransformType.Projection,
                                                  Matrix.OrthoOffCenterLH(_left, _right, -_bottom, -_top, -1f, 1f));

                    Redraw();

                    break;

                case MouseButtons.Right:
                    _left = -2f;
                    _top = 2f;
                    _right = 2f;
                    _bottom = -2f;

                    _maxIterations = DefaultIterations;

                    Redraw();

                    break;
            }

            base.OnMouseDown(e);
        }
    }
}