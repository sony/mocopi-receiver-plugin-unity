# mocopi Receiver Plugin for Unity

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-6+-blue.svg)](https://unity.com/)

mocopi Receiver Plugin for Unity is a plugin for receiving motion data transmitted from the mocopi app and applying it to 3D avatars in Unity in real-time.

## License Notice
- This project is licensed under the Apache License 2.0 - see the [LICENSE](./LICENSE) file for details.
- Notwithstanding the foregoing, this repository does not include the mocopi logo or application icons. Use of these assets requires entering into a separate mocopi Logo and Icon License Agreement. ([here](https://www.sony.co.jp/en/Products/mocopi-dev/en/others/LogoGuideline.html))

## Overview

**mocopi** is a motion capture system that captures full-body motion data using a smartphone app or PC app combined with mocopi sensors. By using this Unity plugin, you can easily apply captured motion data to 3D avatars in your Unity applications.

### Key Features

- **Real-time Motion Capture**: Directly receive motion data from mocopi app via UDP
- **Avatar Integration**: Apply motion data to Unity avatars with automatic bone mapping
- **Cross-Platform Support**: Works on Windows, macOS, Android, and iOS
- **Easy Integration**: Simple APIs for quick setup and customization

## Supported Platforms

| Platform | Supported Versions |
|----------|-------------------|
| **Windows** | Windows 10/11 |
| **macOS** | macOS 10.15+ |
| **Android** | Android 11+ |
| **iOS** | iOS 15.7+ |

> **Note**: Linux is not currently supported.

## Requirements

### Development Environment
- **Unity**: Unity 6000.0.60f1 or later
- **Network**: Local network connection between mocopi app and Unity application

### Runtime Requirements
- mocopi app (Mobile version: Android/iOS, or PC version: Windows)
- UDP communication connection between mocopi app and Unity application
- Maximum motion data frame rate: Depends on mocopi app settings

## Installation

1. Download the latest release from [Releases](https://github.com/sony/mocopi-receiver-plugin-unity/releases)
2. Extract the package to your Unity project's `Packages` directory
3. Unity will automatically import the package

## Quick Start

### 1. Avatar Setup

1. Drag & drop a humanoid avatar model file (FBX format, etc.) into `Assets`
   - To use the sample model, please use `Assets/MocopiReceiver/Samples/ReceiverSample/Models/MocopiAvatar.fbx`
2. Select the model asset and click the Rig tab in the Inspector
3. Set the following items and click the `Apply` button:
   - **Animation Type**: Humanoid
   - **Avatar Definition**: Create From This Model
4. Click the `Configure...` button to check bone mapping
5. Drag & drop the model asset into the Hierarchy window
6. Select the avatar object and add `Mocopi Avatar` from `Add Component`

### 2. Receiver Setup

1. Drag & drop the following Prefab into the Hierarchy window:
   ```
   Assets/MocopiReceiver/Resources/Prefab/MocopiSimpleReceiver.prefab
   ```
2. Select the Receiver object and expand `Mocopi Simple Receiver (Script)` in the Inspector
3. Open `Avatar Settings` and click the `+` button
4. Set the following in `Element 0`:
   - **Mocopi Avatar**: Drag & drop the Avatar object from Hierarchy
   - **Port**: UDP communication port number with mocopi app (default: 12351)

### 3. Network Setup

Please ensure that the smartphone running the mocopi app and your Unity application are connected to the same local network. The plugin communicates via UDP on port 12351 by default.

## Sample Projects

This repository includes sample scenes demonstrating basic usage:

- **Basic Receiver Sample**: Shows fundamental setup and avatar control
- **Advanced Integration**: Demonstrates custom bone mapping and data processing

To check the samples, please refer to `Assets/MocopiReceiver/Samples/`.

## Troubleshooting

### Common Issues

**Q: Motion data is not being received**
- Please verify that both devices are connected to the same network
- Please check UDP port configuration (default: 12351)
- Please confirm that the mocopi app is transmitting data

**Q: Avatar movement appears jerky**
- Please check network latency and stability
- Please verify frame rate settings in the mocopi app
- Please consider implementing motion smoothing

For additional troubleshooting, please see the [FAQ](https://www.sony.co.jp/en/Products/mocopi-dev/jp/documents/ReceiverPlugin/Unity/TroubleShoot.html).

## Support

For technical support and questions, please join the following Discord server:

**Discord**: https://discord.gg/k55wY45y5N

## Resources

- **mocopi Official Developer Site**: [https://sony.net/mocopi-dev/](https://sony.net/mocopi-dev/)
- **Documentation**: [Unity Plugin Guide](https://www.sony.co.jp/Products/mocopi-dev/jp/documents/ReceiverPlugin/Unity/AboutPlugin.html)

---

**Copyright © 2025 Sony Corporation. All rights reserved.**