# LanLord

## Control your network bandwidth with LanLord.

> **Security notice:** Only download LanLord from the official source at **https://github.com/brandodt/LanLord**.  
> Copies obtained elsewhere may contain malware or other harmful modifications.

---

## Requirements

- **Windows 10/11** (x86 or x64)
- **Npcap** installed in WinPcap-compatible mode — the installer will prompt you to install it automatically if it is missing.
- **Administrator privileges** — required so the app can open the network driver.
- A Wi-Fi or Ethernet adapter that supports promiscuous / monitor mode (most modern adapters work fine).

---

## Installation

1. Download the latest release installer (`LanLord-Setup.exe`) from the [Releases](https://github.com/brandodt/LanLord/releases) page.
2. Run the installer and follow the on-screen instructions.
3. If Npcap is not already installed the setup will guide you through installing it — make sure to tick **"WinPcap API-compatible mode"** during the Npcap install.
4. Launch **LanLord** from the Start Menu or the desktop shortcut.

---

## First Launch

On every launch you will see a **security notice** reminding you to verify that your copy came from the official GitHub repository.  
Read it, then click **I Understand — Continue** to proceed, or **Quit** to exit.

---

## How to Use

### 1. Select a Network Adapter

When LanLord starts, the **Adapter Selection** window opens automatically.

- Choose the network adapter you are connected to from the drop-down list (e.g. your Wi-Fi or Ethernet card).
- Your adapter's IP address and type are shown below the list to help you identify the right one.
- Click **OK** to confirm.

### 2. Scan for Devices

- The main window shows a tree-grid with your router at the root and discovered devices as child rows.
- Click the **Scan** button (toolbar) to actively discover all devices currently on your network.  
  The status bar updates in real time: `Scanning network... X/Y` → `Scan complete — N device(s) found`.
- LanLord also re-scans automatically every 60 seconds in the background.

### 3. Understand the Device Table

| Column               | Description                                                                                                                                                                                                                                                                                                                                           |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**             | Friendly name of the device. Right-click → **Rename** to set a custom label. DNS / HTTP hostnames are appended automatically (`→ hostname`). Active interventions are prefixed: `[SCAN]` port-scan detected.                                                                                                                                          |
| **IP**               | Local IPv4 address of the device.                                                                                                                                                                                                                                                                                                                     |
| **MAC / Vendor**     | Hardware MAC address plus the OUI vendor name looked up from the IEEE database.                                                                                                                                                                                                                                                                       |
| **Download**         | Current download bandwidth (KB/s or MB/s), updated every second.                                                                                                                                                                                                                                                                                      |
| **Upload**           | Current upload bandwidth (KB/s or MB/s), updated every second.                                                                                                                                                                                                                                                                                        |
| **DownCap (KB/s)**   | Download speed cap for this device (0 = unlimited). Requires **Start Spoofing** to be active.                                                                                                                                                                                                                                                         |
| **UploadCap (KB/s)** | Upload speed cap for this device (0 = unlimited). Requires **Start Spoofing** to be active.                                                                                                                                                                                                                                                           |
| **Block**            | Tick to completely cut off this device from the internet. Requires **Start Spoofing** to be active.                                                                                                                                                                                                                                                   |

### 4. Limit Bandwidth

1. Click **Start Spoofing** in the toolbar. The status bar will change to `Spoofing active — intercepting network traffic`.
2. In the **DownCap (KB/s)** or **UploadCap (KB/s)** column for a device, type the speed limit you want (e.g. `512` for 512 KB/s, `0` for unlimited).
3. Use the right-click context menu **Cut Off** shortcut to instantly cap a device to 1 KB/s, or **Remove Caps** to restore unlimited access.

### 5. Block a Device

- Tick the **Block** checkbox for any device to deny it internet access entirely.
- To restore access, untick the checkbox.
- Block/unblock states are saved in `profiles.xml` and restored on the next launch.

### 6. Rename a Device

- Right-click any device row and choose **Rename** to give it a custom label.
- The name is saved to `profiles.xml` and restored automatically on future launches.

### 7. Live Bandwidth Graph

- Double-click any device row to open a **live bandwidth graph** that plots their download and upload speed over time in real time.

### 8. Device Profiles (Persistent Settings)

- Any custom name, bandwidth cap, or block state you set is automatically saved per MAC address in `profiles.xml`.
- The next time the same device appears on the network its settings are restored automatically — no need to reconfigure.

### 9. Security Alerts

- **Rogue ARP** — if another device on the network starts impersonating your router, LanLord shows a system-tray balloon notification and logs the event to `security_alerts.txt`.
- **Port Scan** — if a device probes many ports on a single host, a `[SCAN]` tag is added to its name row and a notification is shown.
- **DNS / HTTP logs** — all DNS queries and plain-text HTTP hostnames seen from redirected devices are written to `dns_log.txt` and `http_log.txt` respectively.

### 10. System Tray

- Minimizing the window sends LanLord to the system tray — it keeps running in the background.
- Right-click the tray icon for quick **Show / Exit** options.

---

## Features at a Glance

- ARP spoofing-based bandwidth control (cap upload & download per device)
- Block any device from accessing the internet
- Live bandwidth graph per device
- Automatic device re-discovery every 60 seconds
- OUI vendor lookup from the IEEE database
- Passive OS fingerprinting (TTL-based)
- DNS query sniffer (logs hostnames to `dns_log.txt`)
- HTTP Host sniffer (logs browsing destinations to `http_log.txt`)
- Rogue ARP detector (alerts when another host impersonates your router)
- Port scan detector
- Persistent device profiles (name, caps, block state saved per MAC)
- System tray support with minimize-to-tray

---

## Troubleshooting

| Symptom                                                       | Fix                                                                                                                                                                                                            |
| ------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| "Problem installing the drivers" on startup                   | Run LanLord as Administrator.                                                                                                                                                                                  |
| No devices appear after scanning                              | Ensure you selected the correct adapter and that Npcap is installed in WinPcap-compatible mode.                                                                                                                |
| Bandwidth caps have no effect                                 | Make sure **Start Spoofing** has been clicked and the device is listed in the table.                                                                                                                           |
| App is not visible, but icon is in the tray                   | Click the tray icon or right-click → **Show**.                                                                                                                                                                 |

---

## Credits

LanLord is based on **SelfishNet 3.0.1** originally created by [nov0caina](https://github.com/abelmez/SelfishNet).  
Full credit and thanks to **nov0caina** for the original SelfishNet project, its architecture, and implementation.
