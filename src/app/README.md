# App BLE Central

## About
The app has been writen in C# using the .NET MAUI framework. The app is a BLE central that connects to the Room sensors. We are using the [Plugin-BLE](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le) nuget package for the BLE communication.

## Prerequisites
You will need the following tools and dependencies installed on your machine to run the app:

* [Dotnet 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* MAUI workload
    ```bash
    dotnet workload install maui
    ```
* WASM-tools workload
    ```bash
    dotnet workload install wasm-tools
    ```
* Your favorite C# IDE or editor. We recommend [Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/)

## Running the app
The app can not run locally on a windows PC inside of an Android emulator. The reason for this is that the emulator does not support BLE. The app can be ran on a physical Android device. The app can also be ran on a physical iOS device, we have not tested this because we do not have access to a Mac. Compiling IOS apps requires a Mac.

### Rider IDE
When using Rider IDE, you can simply connect your Android device, after installing the [`Rider Xamarin Android Support`](https://plugins.jetbrains.com/plugin/12056-rider-xamarin-android-support) plugin, to your PC and run the app. Rider will automatically detect the device and run the app on it. If it does not, you can select the device in the top right corner of the IDE.

*Note: Please make sure your device is in developer mode and USB debugging is enabled.*

