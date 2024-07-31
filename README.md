Introduction
MiniLight is a comprehensive ray tracing project designed to produce photorealistic images by simulating the physical behavior of light. Ray tracing is a powerful technique in computer graphics that calculates the color of pixels by tracing paths of light as they interact with objects in a scene. This document provides an in-depth guide to using MiniLight, understanding its core components, and exploring its algorithms and data structures.

Project Structure Overview
The project is organized into several directories, each containing specific components and utilities:

PpmResizer: Tools for resizing PPM images.
PpmViewer: A viewer for PPM images.
Scenes: Sample scene files for rendering.
Blender Import: Scripts for importing scenes into Blender.
csharp: Contains C# implementations of core functionalities.
fsharp: Contains F# implementations of core functionalities.
exportply.cs: A script for exporting scene data to PLY format.
Core Components and Algorithms
Ray Tracing Algorithm
The core of MiniLight is its ray tracing algorithm, which follows these steps:

Ray Generation:

Rays are generated from the camera through each pixel of the image plane.
The direction of each ray is calculated based on the camera's position and orientation.
Intersection Testing:

Each ray is tested for intersections with objects in the scene.
The algorithm checks each object to determine if and where the ray intersects it.
Efficient data structures like Bounding Volume Hierarchies (BVH) are used to speed up this process.
Shading:

Once an intersection is found, the shading model calculates the color at the intersection point.
The shading model considers material properties, light sources, and other factors like shadows and reflections.
Reflection and Refraction:

For reflective and refractive surfaces, additional rays are generated.
Reflection rays simulate the way light bounces off shiny surfaces.
Refraction rays simulate the bending of light as it passes through transparent materials.
Recursion:

The algorithm recursively traces these secondary rays until a termination condition is met (e.g., maximum recursion depth or ray energy below a threshold).
Scene Parser
The scene parser reads spec files and constructs the scene data structure. The parser handles the following tasks:

Geometry Parsing: Reads and interprets the geometry section of the spec file to create objects like spheres, triangles, and meshes.
Material Parsing: Interprets the material properties defined in the spec file, including diffuse, specular, and reflective properties.
Light Parsing: Extracts light source information from the spec file, including type (e.g., point, directional), position, and intensity.
Data Structures
Bounding Volume Hierarchy (BVH):

BVH is a tree structure used to organize geometric objects in the scene for efficient intersection testing. It reduces the number of intersection tests by quickly eliminating large portions of the scene that do not intersect with a ray.
Construction: Objects are recursively divided into bounding volumes.
Traversal: During ray tracing, the BVH is traversed to find potential intersections efficiently.
Spatial Grid:

A spatial grid divides the scene into a grid of cells, each containing a list of objects. This structure allows for efficient querying of nearby objects, useful for both intersection tests and shading calculations.
Grid Construction: The scene is divided into uniform cells, and objects are assigned to the cells they occupy.
Grid Traversal: Rays traverse the grid, checking only the cells they pass through for potential intersections.
Scene Graph:

A scene graph is a hierarchical tree structure that represents the spatial organization of the scene. It helps manage hierarchical transformations and optimizations in rendering.
Nodes: Each node represents an object or group of objects.
Transforms: Nodes can have transformations (translation, rotation, scaling) that affect their children.
Module-by-Module Guide
PpmResizer
The PpmResizer module contains tools for resizing PPM images. This is useful for adjusting the resolution of rendered images.

Resize Algorithm: Uses bilinear or bicubic interpolation to resize images.
Usage: The tool can be invoked from the command line with parameters for input file, output file, and target resolution.
PpmViewer
PpmViewer is a viewer application for displaying PPM images. It allows users to inspect the rendered scenes in detail.

Rendering: Uses a simple GUI to display the image.
Navigation: Provides zoom and pan functionality to examine image details.
Blender Import
The Blender Import module contains scripts for importing MiniLight scene data into Blender. This is useful for users who want to manipulate or animate scenes using Blender's tools.

Import Script: Parses MiniLight spec files and creates corresponding objects in Blender.
Usage: The script can be run from Blender's scripting interface.
C# and F# Implementations
The csharp and fsharp directories contain implementations of core functionalities in C# and F#.

Ray-Object Intersection: Functions for testing ray intersections with various geometric objects.
Shading Models: Implementations of different shading techniques, including Phong and Lambertian shading.
Scene Management: Classes for managing and organizing scene data.
Procedures
Preparing Spec Files
Spec files define the scenes rendered by MiniLight. Here’s how to prepare them:

Geometry Definition: Describe objects in the scene, including their shapes, positions, and sizes.
Material Properties: Define the material attributes for each object, such as color, reflectivity, and transparency.
Light Sources: Specify the types, positions, and intensities of light sources in the scene.
Running MiniLight
To render a scene with MiniLight:

Command Line Interface (CLI):

css
Copy code
minilight -i scene.spec -o output.ppm
This command takes an input spec file (scene.spec) and produces an output image (output.ppm).

Output Inspection: Use PpmViewer to examine the rendered image.

Exporting to PLY
The exportply.cs script allows exporting scene data to the PLY format, which is widely used for 3D models.

Usage: Run the script with the scene data to generate a PLY file.
Integration: PLY files can be imported into various 3D modeling tools for further processing.
Optimization Techniques
Acceleration Structures
Using BVH and spatial grids significantly improves performance by reducing the number of intersection tests needed during ray tracing.

BVH Optimization: Implementing a BVH construction algorithm that balances the tree can lead to faster traversal times.
Grid Resolution: Choosing an appropriate grid resolution based on scene complexity can enhance performance.
Adaptive Sampling
Adaptive sampling reduces the number of rays traced in areas of the image that converge quickly to a stable color, saving computation time.

Thresholding: Setting thresholds for color variance can help decide when to stop tracing additional rays.
Parallel Processing
Leveraging multi-core CPUs and GPUs can significantly speed up the ray tracing process.

CPU Parallelism: Using threading libraries to distribute ray tracing computations across multiple CPU cores.
GPU Acceleration: Implementing the ray tracing algorithm on a GPU using frameworks like CUDA or OpenCL.
Troubleshooting
Installation Issues
Compatibility: Ensure your system meets the minimum requirements.
Dependencies: Verify that all required libraries are installed correctly.
Rendering Artifacts
Incorrect Spec Files: Check for syntax errors or incorrect values in spec files.
Precision Errors: Floating-point precision issues can cause artifacts. Consider using higher precision data types if necessary.
Advanced Techniques and Future Development
Global Illumination
Global illumination techniques, such as path tracing and photon mapping, simulate the indirect lighting in a scene, resulting in more realistic images.

Path Tracing: Extends ray tracing by accounting for multiple light bounces.
Photon Mapping: Simulates the distribution of light energy in a scene by tracing photons from light sources.
Real-time Ray Tracing
Optimizing the ray tracing algorithm to achieve interactive frame rates.

Ray Tracing in Games: Integrating ray tracing with rasterization techniques for real-time applications.
Hardware Acceleration: Utilizing dedicated ray tracing hardware available in modern GPUs.
Appendices and References
File Format Specifications
Detailed descriptions of the PPM and spec file formats used in MiniLight.
