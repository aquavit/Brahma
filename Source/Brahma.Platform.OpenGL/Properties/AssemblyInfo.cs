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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//General assembly information

[assembly: AssemblyTitle("Brahma.Platform.OpenGL")]
[assembly: AssemblyDescription("Provides interfaces and platform-specific implementations of OpenGL contexts/functionality")]

// GUID for the ID of the typelib if this project is exposed to COM

[assembly: Guid("65688159-fa9d-4775-be9f-a249824fc9af")]
[assembly: InternalsVisibleTo("Brahma.OpenGL")] // All internal classes are exposed to Brahma.OpenGL