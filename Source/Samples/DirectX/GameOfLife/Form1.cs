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

namespace GameOfLife
{
    public partial class Form1: Form
    {
        // The width and height of the grid
        private const int GridHeight = 512;
        private const int GridWidth = 512;
        private readonly CompiledQuery _nextGeneration;

        private readonly ComputationProvider _provider;
        private DataParallelArray2D<float> _currentGeneration;

        public Form1()
        {
            InitializeComponent();

            ClientSize = new Size(GridWidth, GridHeight);
            _provider = new ComputationProvider(this);

            // Set up the initial configuration
            // TODO: We should be able to load configurations, too
            var random = new Random();

            // a step function to determine live and dead cells to start the game
            _currentGeneration = new DataParallelArray2D<float>(_provider, GridWidth, GridHeight,
                                                                (x, y) =>
                                                                ((float)random.NextDouble()) < 0.5f
                                                                    ? 255f
                                                                    : 0f);

            // Compile the query that creates the next generation
            _nextGeneration = _provider.Compile<DataParallelArray2D<float>>(
                d => from value in d
                     // Find all the neighbors, binarize them
                     let topLeft = d[output.CurrentX - 1, output.CurrentY - 1] > 0f
                                       ? 1f
                                       : 0f
                     let top = d[output.CurrentX, output.CurrentY - 1] > 0f
                                   ? 1f
                                   : 0f
                     let topRight = d[output.CurrentX + 1, output.CurrentY - 1] > 0f
                                        ? 1f
                                        : 0f
                     let left = d[output.CurrentX - 1, output.CurrentY] > 0f
                                    ? 1f
                                    : 0f
                     let current = value > 0f
                                       ? 1f
                                       : 0f
                     let right = d[output.CurrentX + 1, output.CurrentY] > 0f
                                     ? 1f
                                     : 0f
                     let bottomLeft = d[output.CurrentX - 1, output.CurrentY + 1] > 0f
                                          ? 1f
                                          : 0f
                     let bottom = d[output.CurrentX, output.CurrentY + 1] > 0f
                                      ? 1f
                                      : 0f
                     let bottomRight = d[output.CurrentX + 1, output.CurrentY + 1] > 0f
                                           ? 1f
                                           : 0f
                     // Count the number of live neighbors
                     let liveNeighbors = topLeft + top + topRight +
                                         left + right +
                                         bottomLeft + bottom + bottomRight
                     // Determine state of the current cell
                     select value > 0f
                                ? ((liveNeighbors < 2f) || (liveNeighbors > 3f)
                                       ? 0f // It dies of loneliness or overcrowding
                                       : 255f) // 2 or 3 neighbors, it continues to live
                                : liveNeighbors == 3f
                                      ? 255f // Bring it to life
                                      : 0f); // It's dead

            renderTimer.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            renderTimer.Enabled = false;
            _provider.Dispose();
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            Render(); // We don't want an update
        }

        private void Render()
        {
            var result = _provider.Run(_nextGeneration, _currentGeneration) as DataParallelArray2D<float>;

            _currentGeneration.Dispose(); // Dispose the current generation
            _currentGeneration = result; // The output of the previous iteration is the input for the next one

            Visualizer.Display(_provider, _currentGeneration);
        }
    }
}