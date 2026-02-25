---
description: "Use when adding features, fixing bugs, or refactoring any part of the SelfishNet project. Covers C# naming conventions, WinForms UI patterns, WinPcap/ARP networking patterns, and threading model used throughout this codebase."
applyTo: "**"
---

# SelfishNet Project Conventions

## Project Overview

SelfishNet is a WinForms C# desktop application that performs ARP spoofing and bandwidth throttling on a local network. It uses **WinPcap via PcapNet** for raw packet capture and injection, requires **administrator privileges**, and targets Windows.

- Namespace: `SelfishNetv3`
- Framework: .NET Framework (WinForms)
- Key dependencies: `PcapNet`, `AdvancedDataGridView`

---

## Naming

| Element         | Convention                                  | Example                                               |
| --------------- | ------------------------------------------- | ----------------------------------------------------- |
| Classes         | PascalCase                                  | `CArp`, `PcList`, `ArpForm`                           |
| Public methods  | PascalCase                                  | `NicIsSelected()`, `StartArpListener()`               |
| Private methods | camelCase                                   | `discoverer()`, `redirector()`                        |
| Public fields   | camelCase                                   | `public IPAddress ip`, `public bool isGateway`        |
| Private fields  | camelCase                                   | `private bool isRedirecting`, `private PcList pcList` |
| Delegates       | PascalCase starting with `delegate` keyword | `delegateOnNewPC`, `DelUpdateName`                    |

> Note: The legacy classes `tools` and `main` use lowercase names — do **not** follow this pattern in new code.

---

## Class and File Structure

- One class per file, filename matches class name.
- Wrap class body with `#pragma warning disable` / `#pragma warning restore` to suppress missing XML comment warnings, as done consistently in existing files.
- Group related methods with `#region` / `#endregion` blocks, especially in Form classes.

```csharp
#pragma warning disable
public class MyNewClass
{
    // ...
}
#pragma warning restore
```

---

## WinForms UI Patterns

- Use `DllImport("user32.DLL")` with `ReleaseCapture` and `SendMessage` for custom form dragging.
- Override `WndProc` (with `WM_NCHITTEST`) for custom resize hit-testing.
- Colors are stored as `int` using `Color.FromArgb(...).ToArgb()`, not as `Color` directly.
- Minimize/maximize logic toggles visibility between `btnMaximizar` and `btnRestaurar`.
- Open child forms with a lambda wrapper: `AbrirFormulario(() => new MyForm())`.
- The entry form is `ArpForm`; it holds references to `CArp`, `CAdapter`, `PcList`, and `NetworkInterface`.

---

## Networking / WinPcap / ARP Patterns

- IP addresses and MAC addresses are always handled as `byte[]`, not as strings.
- Use `tools.areValuesEqual(byte[], byte[])` to compare byte arrays — never `==` or `SequenceEqual`.
- Use `tools.getIpAddress(string)` to convert dotted-decimal strings to `IPAddress`.
- Packet data is manipulated via `Array.Copy` with explicit byte offsets into the raw Ethernet frame.
- `CArp` is the main ARP engine; `ArpForm` owns its lifecycle.
- `PcList` manages the list of discovered `PC` objects on the network.
- `PC` is a plain data class with public fields (not properties).

---

## Threading Model

- Use `System.Threading.Thread` directly — do not use `Task`/`async-await` or `ThreadPool` for network operations.
- Use `EventWaitHandle` (with `EventResetMode.AutoReset`) to signal thread termination.
- Stop threads by setting a `bool` flag (e.g., `isRedirecting = false`) and then calling `.WaitOne()` on the termination event handle.
- Thread methods (`discoverer`, `redirector`, `arpListener`) are private, camelCase, and take no parameters.

```csharp
// Starting a thread
(myThread = new Thread(myWorkerMethod)).Start();

// Stopping a thread
isRunning = false;
myThreadTerminated.WaitOne();
```

---

## Error Handling

- Show user-facing errors with `MessageBox.Show`. Cast the return value to `int` to discard it when the result is unused:
  ```csharp
  int num = (int)MessageBox.Show("Something went wrong.");
  ```
- Do not add `try/catch` blocks unless directly dealing with known exception-throwing boundaries (e.g., driver install, file access).
- Null-check `PC` references returned from `PcList` before use (e.g., `getRouter()` can return `null`).

---

## Security Requirements

- This tool manipulates raw network packets and installs kernel drivers — always validate that required privileges are present before proceeding.
- Never send or capture traffic unless the user has explicitly confirmed license acceptance and NIC selection.
- ARP spoofing must only operate on the locally selected `NetworkInterface` — never broadcast to unrelated interfaces.
- Packet byte offsets (e.g., source MAC at offset 6, source IP at offset 26) must be treated as constants and not guessed or hardcoded arbitrarily.
