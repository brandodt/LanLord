---
mode: "agent"
description: "Scaffold a new class or feature for the SelfishNet WinForms ARP-spoofing project, following all project conventions."
---

Generate a new class or feature for the **SelfishNet** project (`SelfishNetv3` namespace, .NET Framework WinForms).

**Feature to implement:** ${input:featureDescription:Describe the class or feature to generate (e.g., "a throttle manager that tracks per-PC bandwidth limits")}

If a file reference is attached (e.g., a related existing class), use it as context for how the new code should integrate with existing classes.

---

## Mandatory Conventions

### File & Class Structure

- One class per file; filename must match class name exactly.
- Namespace: `SelfishNetv3`
- Wrap the entire class body with `#pragma warning disable` / `#pragma warning restore`.
- Group related methods in `#region` / `#endregion` blocks.

### Naming

| Element         | Convention | Example                                               |
| --------------- | ---------- | ----------------------------------------------------- |
| Classes         | PascalCase | `CArp`, `PcList`, `ArpForm`                           |
| Public methods  | PascalCase | `NicIsSelected()`, `StartArpListener()`               |
| Private methods | camelCase  | `discoverer()`, `redirector()`                        |
| Public fields   | camelCase  | `public IPAddress ip`, `public bool isGateway`        |
| Private fields  | camelCase  | `private bool isRedirecting`, `private PcList pcList` |
| Delegates       | PascalCase | `delegateOnNewPC`, `DelUpdateName`                    |

> Do **not** use lowercase class names (legacy `tools` / `main` are exceptions, not models).

### Threading

- Use `System.Threading.Thread` directly — **never** `Task`, `async/await`, or `ThreadPool` for network operations.
- Use `EventWaitHandle` (`EventResetMode.AutoReset`) to signal thread termination.
- Stop pattern: set the `bool` flag to `false`, then call `.WaitOne()` on the handle.
- Worker methods must be private, camelCase, and take no parameters.

```csharp
// Start
(myThread = new Thread(myWorkerMethod)).Start();

// Stop
isRunning = false;
myThreadTerminated.WaitOne();
```

### Networking / ARP

- IP addresses and MAC addresses are always `byte[]`, never strings.
- Compare byte arrays with `tools.areValuesEqual(byte[], byte[])` — never `==` or `SequenceEqual`.
- Convert dotted-decimal strings with `tools.getIpAddress(string)`.
- Packet byte offsets (e.g., source MAC offset 6, source IP offset 26) must be named constants, never inline magic numbers.

### WinForms (only if the feature includes a Form)

- Custom form dragging: `DllImport("user32.DLL")` + `ReleaseCapture` + `SendMessage`.
- Custom resize: override `WndProc` with `WM_NCHITTEST`.
- Open child forms via: `AbrirFormulario(() => new MyForm())`.
- Colors stored as `int`: `Color.FromArgb(...).ToArgb()`.

### Error Handling

- User-facing errors: `int num = (int)MessageBox.Show("...");`
- No `try/catch` unless the call is at a known exception-throwing boundary (driver install, file I/O).
- Always null-check results from `PcList` methods (e.g., `getRouter()` can return `null`).

### Security

- Never capture or inject packets unless the NIC is selected and the license has been accepted.
- ARP operations must be scoped exclusively to the user-selected `NetworkInterface`.
- Never broadcast to unrelated interfaces.

---

## Output Format

Provide, in order:

1. **Full `.cs` file** — ready to drop into `SelfishNetV3/`, with correct namespace, `#pragma` wrappers, and `#region` grouping.
2. **`ArpForm.cs` integration snippet** — only if the new class needs to be wired into `ArpForm`; show the minimal addition (field declaration + lifecycle calls).
3. **Dependencies** — list any new NuGet packages or project references required (if none, omit this section).
