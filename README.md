# WordDuel – Local Hosting Guide

## Prerequisites
- All players must be connected to the same Wi-Fi network
- The host must run the application on their device

## How to Host

1. Open PowerShell and run:
ipconfig

2. Copy your **IPv4 Address** (e.g. `192.168.1.45`)

3. Start the application via **Start without debugging** in Visual Studio

4. Share the following link with other players:
https://YOUR_IP:7057

## How to Join

1. Open the link shared by the host in your browser
2. If you see a security warning, click **Advanced** and proceed anyway
   - This is expected since the application uses a local development certificate

## Notes
- The game is only accessible while the host is running the application
- If players cannot connect, make sure everyone is on the same Wi-Fi network
