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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Brahma;
using Brahma.DirectX; // This is the only line we have to change to make this sample run on OpenGL! Isn't that awesome?
using Brahma.Platform;

namespace OddEvenTranspositionSort
{
    internal class Program
    {
        private static void WriteLine(string message, params object[] arguments)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, message, arguments));
        }

        public static bool TestSortResults(IEnumerable<float> data)
        {
            float previous = float.MinValue;
            foreach (float value in data)
            {
                // Make sure the value monotonically increases
                if (previous > value)
                    return false; // Boooo!

                previous = value; // save this value so the next element can check against it
            }

            return true; // Yayyyy!
        }

        public static bool TestSortResults(IEnumerable<Vector4> data)
        {
            float previous = float.MinValue;
            foreach (Vector4 value in data)
            {
                // Calculate the length
                var length = (float)Math.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z);

                // Make sure the value monotonically increases
                if (previous > length)
                    return false; // Boooo!

                // save this length so the next element can check against it
                previous = length;
            }

            return true; // Yayyyy!
        }

        private static void Main()
        {
            // Create a computation provider
            var provider = new ComputationProvider();

            var random = new Random();

            WriteLine("Testing odd-even transposition sort using compiled queries");
            WriteLine("\nSort 4096 floats");

            // This test shows us that relatively simple computations run comparably (or worse) than a CPU implementation of the same algorithm.

            #region Sort floats

            #region Execute on the GPU

            {
                WriteLine("\n Execute on the GPU");
                PerformanceTimer.Start();
                var data = new DataParallelArray<float>(provider, 4096, x => (float)random.NextDouble());
                // Fill a large data-parallel array with floats
                WriteLine("Array was set up in {0:F4} seconds", PerformanceTimer.Stop());
                // Measure how long that took. First one will take a while, subsequent ones will be faster since the system memory surfaces used for the transfer are cached

                PerformanceTimer.Start();
                // query to be applied to even passes
                // We're using value (which we've defined) and indexing with offsets
                CompiledQuery evenSelector = provider.Compile<DataParallelArray<float>>(
                    d => from value in d
                         select (output.Current % 2 == 0)
                                    ? Math.Min(value, d[output.Current + 1])
                                    : Math.Max(value, d[output.Current - 1])
                    );
                WriteLine("Query evenselector compiled in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();
                // query to be applied to odd passes
                CompiledQuery oddSelector = provider.Compile<DataParallelArray<float>>(
                    d => from float value in d
                         select (output.Current % 2 != 0)
                                    ? Math.Min(value, d[output.Current + 1])
                                    : Math.Max(value, d[output.Current - 1])
                    );
                WriteLine("Query oddselector compiled in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();

                double totalTime = 0;

                // This is the coolest bit. We can now run a CPU loop, with the two kernel computations running inside it alternately!
                // Running compiled queries is super-fast since the pixel shader has already been built during Compile
                // Data is also never brought back to the CPU, until enumerated so this provides maximum performance
                for (int i = 0; i < data.Length; i++)
                {
                    PerformanceTimer.Start();
                    IQueryable result = i % 2 == 0
                                            ? provider.Run(evenSelector, data)
                                            : provider.Run(oddSelector, data);

                    // Dispose the previous input. Don't worry, textures are cached so you can Dispose as much as you want.
                    // It doesn't degrade performance one bit!
                    data.Dispose();

                    // Output of this iteration is the input of the next. GPU ping-ponging in C#!
                    data = result as DataParallelArray<float>;

                    totalTime += PerformanceTimer.Stop();
                }

                WriteLine("Sorted {0} floats on the GPU in {1:F4} seconds, average time for each iteration was {2:F4}",
                          data.Length, PerformanceTimer.Stop(), totalTime / data.Length);
                // Let's see if the sort happened ok
                WriteLine("Validating sort results: {0}", TestSortResults(data)
                                                              ? "OK"
                                                              : "Error");
            }

            #endregion

            #region Execute on the CPU

            {
                WriteLine("\n Execute on the CPU");

                PerformanceTimer.Start();

                // Create and fill the array with random floats
                var data = new float[4096];
                for (int i = 0; i < 4096; i++)
                    data[i] = (float)random.NextDouble();
                WriteLine("Array was set up in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();

                double totalTime = 0;
                for (int i = 0; i < 4096; i++)
                {
                    PerformanceTimer.Start();

                    var result = new float[4096];
                    if (i % 2 == 0) // Choose the selector for even/odd passes
                        for (int x = 0; x < 4096; x++)
                            // Loop through and serially do what the select kernel does in the GPU bit
                            result[x] = x % 2 == 0
                                            ? Math.Min(data[x], data[(x + 1 > 4095)
                                                                         ? 4095
                                                                         : x + 1])
                                            : Math.Max(data[x], data[(x - 1 < 0)
                                                                         ? 0
                                                                         : x - 1]);
                    else
                        for (int x = 0; x < 4096; x++)
                            // Loop through and serially do what the select kernel does in the GPU bit
                            result[x] = x % 2 != 0
                                            ? Math.Min(data[x], data[(x + 1 > 4095)
                                                                         ? 4095
                                                                         : x + 1])
                                            : Math.Max(data[x], data[(x - 1 < 0)
                                                                         ? 0
                                                                         : x - 1]);

                    data = result; // Save the result from this iteration

                    totalTime += PerformanceTimer.Stop(); // Add up the iteration time
                }

                WriteLine(
                    "Sorted {0} floats on the  CPU in {1:F4} seconds, average time for each iteration was {2:F4}",
                    data.Length, PerformanceTimer.Stop(), totalTime / data.Length);
                // Did it sort the array correctly?
                WriteLine("Validating sort results: {0}", TestSortResults(data)
                                                              ? "OK"
                                                              : "Error");
            }

            #endregion

            #endregion

            WriteLine("\nSort 4096 4-component vectors by length");

            // This test shows us that complex calculations leave CPU implementations in the dust, the GPU iterations run several times!

            #region Sort 4-component vectors by length

            #region Execute on the GPU

            {
                WriteLine("\n Execute on the GPU");
                PerformanceTimer.Start();
                // Fill a huge array with random 4-component vectors
                var data = new DataParallelArray<Vector4>(provider, 4096, x => new Vector4((float)random.NextDouble(),
                                                                                           (float)random.NextDouble(),
                                                                                           (float)random.NextDouble(),
                                                                                           (float)random.NextDouble()));
                WriteLine("Array was set up in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();
                // Write the query that runs on even passes. Look at the liberal use of let's. Brahma encourages you to use them
                // because each one becomes a temporary variable and is therefore evaluated only once.
                CompiledQuery evenSelector = provider.Compile<DataParallelArray<Vector4>>(
                    d => from Vector4 value in d
                         let prevValue = d[output.Current - 1]
                         let nextValue = d[output.Current + 1]
                         let length = Math.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z)
                         let prevLength = Math.Sqrt(prevValue.x * prevValue.x + prevValue.y * prevValue.y + prevValue.z * prevValue.z)
                         let nextLength = Math.Sqrt(nextValue.x * nextValue.x + nextValue.y * nextValue.y + nextValue.z * nextValue.z)
                         select (output.Current % 2 == 0)
                                    ? Math.Min(length, nextLength) == length
                                          ? value
                                          : nextValue
                                    : Math.Max(length, prevLength) == length
                                          ? value
                                          : prevValue
                    );
                WriteLine("Query evenselector compiled in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();

                // Write the query that runs on odd passes.
                CompiledQuery oddSelector = provider.Compile<DataParallelArray<Vector4>>(
                    d => from Vector4 value in d
                         let prevValue = d[output.Current - 1]
                         let nextValue = d[output.Current + 1]
                         let length = Math.Sqrt(value.x * value.x + value.y * value.y + value.z * value.z)
                         let prevLength = Math.Sqrt(prevValue.x * prevValue.x + prevValue.y * prevValue.y + prevValue.z * prevValue.z)
                         let nextLength = Math.Sqrt(nextValue.x * nextValue.x + nextValue.y * nextValue.y + nextValue.z * nextValue.z)
                         select (output.Current % 2 != 0)
                                    ? Math.Min(length, nextLength) == length
                                          ? value
                                          : nextValue
                                    : Math.Max(length, prevLength) == length
                                          ? value
                                          : prevValue
                    );
                WriteLine("Query oddselector compiled in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();

                double totalTime = 0;
                // Begin the CPU iterations, and run the compiled queries alternately
                for (int i = 0; i < data.Length; i++)
                {
                    PerformanceTimer.Start();
                    IQueryable result = i % 2 == 0
                                            ? provider.Run(evenSelector, data)
                                            : provider.Run(oddSelector, data);

                    data.Dispose(); // Dispose the input
                    data = result as DataParallelArray<Vector4>; // Output of current iteration = input for the next one

                    totalTime += PerformanceTimer.Stop();
                }

                WriteLine(
                    "Sorted {0} Vector4s on the GPU in {1:F4} seconds, average time for each iteration was {2:F4}",
                    data.Length, PerformanceTimer.Stop(), totalTime / data.Length);
                WriteLine("Validating sort results: {0}", TestSortResults(data)
                                                              ? "OK"
                                                              : "Error");
            }

            #endregion

            #region Execute on the CPU

            {
                WriteLine("\n Execute on the CPU");

                PerformanceTimer.Start();

                // Create and fill up the array with random floats
                var data = new Vector4[4096];
                for (int i = 0; i < 4096; i++)
                    data[i] = new Vector4((float)random.NextDouble(), (float)random.NextDouble(),
                                          (float)random.NextDouble(), (float)random.NextDouble());
                WriteLine("Array was set up in {0:F4} seconds", PerformanceTimer.Stop());

                PerformanceTimer.Start();

                double totalTime = 0;
                for (int i = 0; i < data.Length; i++) // Use a for loop to run through each element
                {
                    PerformanceTimer.Start();

                    var result = new Vector4[data.Length];
                    if (i % 2 == 0) // Run the even/odd selector
                        for (int x = 0; x < data.Length; x++)
                            // If you're wondering what the extra nested ternary's are, they allow clamped addressing, 
                            // i.e., addressing out of bounds of the array returns the end elements
                            // Don't ask. I don't know what I was on when I wrote this. Sheesh!
                            result[x] = x % 2 == 0
                                            ? Math.Min(Vector4.Length(data[x]), Vector4.Length(data[(x + 1 > data.Length - 1)
                                                                                                        ? data.Length - 1
                                                                                                        : x + 1])) == Vector4.Length(data[x])
                                                  ? data[x]
                                                  : data[(x + 1 > data.Length - 1)
                                                             ? data.Length - 1
                                                             : x + 1]
                                            : Math.Max(Vector4.Length(data[x]), Vector4.Length(data[(x - 1 < 0)
                                                                                                        ? 0
                                                                                                        : x - 1])) == Vector4.Length(data[x])
                                                  ? data[x]
                                                  : data[(x - 1 < 0)
                                                             ? 0
                                                             : x - 1];
                    else
                        for (int x = 0; x < data.Length; x++)
                            result[x] = x % 2 != 0
                                            ? (Math.Min(Vector4.Length(data[x]), Vector4.Length(data[(x + 1 > 4095)
                                                                                                         ? 4095
                                                                                                         : x + 1])) == Vector4.Length(data[x])
                                                   ? data[x]
                                                   : data[(x + 1 > 4095)
                                                              ? 4095
                                                              : x + 1])
                                            : (Math.Max(Vector4.Length(data[x]), Vector4.Length(data[(x - 1 < 0)
                                                                                                         ? 0
                                                                                                         : x - 1])) == Vector4.Length(data[x])
                                                   ? data[x]
                                                   : data[(x - 1 < 0)
                                                              ? 0
                                                              : x - 1]);

                    data = result; // Result of this iteration is the input for the next one

                    totalTime += PerformanceTimer.Stop();
                }

                WriteLine(
                    "Sorted {0} Vector4s on the  CPU in {1:F4} seconds, average time for each iteration was {2:F4}",
                    data.Length, PerformanceTimer.Stop(), totalTime / data.Length);
                WriteLine("Validating sort results: {0}", TestSortResults(data)
                                                              ? "OK"
                                                              : "Error");
            }

            #endregion

            // I'm not going to say anything. You tell me :). Was that fast, or was that fast?

            #endregion

            provider.Dispose();

            WriteLine("Press any key");
            Console.ReadKey(); // Wait for a key
        }
    }
}