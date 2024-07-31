Licenced for home use. Inquire about pricing for commercial use. kns98@yahoo.com

# MiniLight

MiniLight is a comprehensive ray tracing project designed to produce photorealistic images by simulating the physical behavior of light. Ray tracing is a powerful technique in computer graphics that calculates the color of pixels by tracing paths of light as they interact with objects in a scene. This document provides an in-depth guide to using MiniLight, understanding its core components, and exploring its algorithms and data structures.

## Table of Contents

1. [Introduction](#introduction)
2. [Project Structure Overview](#project-structure-overview)
3. [Core Components and Algorithms](#core-components-and-algorithms)
   - [Ray Tracing Algorithm](#ray-tracing-algorithm)
   - [Scene Parser](#scene-parser)
   - [Data Structures](#data-structures)
4. [Module-by-Module Guide](#module-by-module-guide)
   - [PpmResizer](#ppmresizer)
   - [PpmViewer](#ppmviewer)
   - [Blender Import](#blender-import)
   - [C# and F# Implementations](#c-and-f-implementations)
5. [Procedures](#procedures)
   - [Preparing Spec Files](#preparing-spec-files)
   - [Running MiniLight](#running-minilight)
   - [Exporting to PLY](#exporting-to-ply)
6. [Optimization Techniques](#optimization-techniques)
7. [Troubleshooting](#troubleshooting)
8. [Advanced Techniques and Future Development](#advanced-techniques-and-future-development)
9. [Appendices and References](#appendices-and-references)

## Introduction

MiniLight is a comprehensive ray tracing project designed to produce photorealistic images by simulating the physical behavior of light. Ray tracing is a powerful technique in computer graphics that calculates the color of pixels by tracing paths of light as they interact with objects in a scene. This document provides an in-depth guide to using MiniLight, understanding its core components, and exploring its algorithms and data structures.

## Project Structure Overview

The project is organized into several directories, each containing specific components and utilities:

- **PpmResizer**: Tools for resizing PPM images.
- **PpmViewer**: A viewer for PPM images.
- **Scenes**: Sample scene files for rendering.
- **Blender Import**: Scripts for importing scenes into Blender.
- **csharp**: Contains C# implementations of core functionalities.
- **fsharp**: Contains F# implementations of core functionalities.
- **exportply.cs**: A script for exporting scene data to PLY format.

## Core Components and Algorithms

### Ray Tracing Algorithm

The core of MiniLight is its ray tracing algorithm, which follows these steps:

1. **Ray Generation**:
   - Rays are generated from the camera through each pixel of the image plane.
   - The direction of each ray is calculated based on the camera's position and orientation.

2. **Intersection Testing**:
   - Each ray is tested for intersections with objects in the scene.
   - Efficient data structures like Bounding Volume Hierarchies (BVH) are used to speed up this process.

3. **Shading**:
   - Once an intersection is found, the shading model calculates the color at the intersection point.
   - The shading model considers material properties, light sources, and other factors like shadows and reflections.

4. **Reflection and Refraction**:
   - For reflective and refractive surfaces, additional rays are generated.
   - Reflection rays simulate the way light bounces off shiny surfaces.
   - Refraction rays simulate the bending of light as it passes through transparent materials.

5. **Recursion**:
   - The algorithm recursively traces these secondary rays until a termination condition is met.

### Scene Parser

The scene parser reads spec files and constructs the scene data structure, handling:

- **Geometry Parsing**: Creates objects like spheres, triangles, and meshes.
- **Material Parsing**: Interprets material properties including diffuse, specular, and reflective properties.
- **Light Parsing**: Extracts light source information including type, position, and intensity.

### Data Structures

- **Bounding Volume Hierarchy (BVH)**: Organizes geometric objects for efficient intersection testing.
- **Spatial Grid**: Divides the scene into a grid of cells for efficient querying of nearby objects.
- **Scene Graph**: Represents the spatial organization of the scene, managing hierarchical transformations and optimizations.

## Module-by-Module Guide

### PpmResizer

The PpmResizer module contains tools for resizing PPM images using bilinear or bicubic interpolation.

**Usage**: 
```sh
ppmresizer -i input.ppm -o output.ppm -w width -h height
