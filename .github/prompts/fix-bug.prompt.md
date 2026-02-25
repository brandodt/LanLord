---
mode: "agent"
description: "Diagnose and fix a bug in the SelfishNet project, respecting its threading model, ARP/networking patterns, and WinForms conventions."
---

Diagnose and fix a bug in the **SelfishNet** project (`SelfishNetv3` namespace, .NET Framework WinForms).

**Bug description:** ${input:bugDescription:Describe the bug (e.g., "app hangs when stopping ARP redirection", "PC list shows duplicate entries after re-scan")}

Attach the relevant `.cs` file(s) using `#file` so the fix can be applied precisely.

---

## Investigation Checklist

Work through these categories in order — most SelfishNet bugs fall into one of them.

### 1. Threading

- Is a thread stopped by setting the flag to `false` **and** calling `.WaitOne()` on its `EventWaitHandle`? A missing `.WaitOne()` causes hangs; a missing flag reset causes runaway threads.
- Are UI updates (DataGridView, labels, etc.) marshalled back to the UI thread via `Invoke`/`BeginInvoke`? Direct cross-thread UI access causes `InvalidOperationException`.
- Is the `EventWaitHandle` constructed with `EventResetMode.AutoReset`? Using `ManualReset` can cause `.WaitOne()` to return prematurely or never unblock.

### 2. Byte Array Comparisons

- Are IP/MAC addresses compared with `tools.areValuesEqual(byte[], byte[])`? Using `==` always returns `false` for reference types; `SequenceEqual` is also forbidden per project convention.
- Are IP addresses stored as `byte[]`, not `string` or `IPAddress`? Mixing types silently breaks comparisons and packet construction.

### 3. Packet Offsets

- Are byte offsets into Ethernet frames named constants (`const int SRC_MAC_OFFSET = 6`, etc.)? An off-by-one in a raw offset corrupts the packet silently.
- Is `Array.Copy` called with the correct `sourceIndex`, `destinationIndex`, and `length`? Swapped source/destination is a common bug.

### 4. Null References from PcList

- Is the result of `getRouter()` (or any `PcList` lookup) null-checked before use? These return `null` when the device hasn't been discovered yet.
- Is a `PC` object accessed after the scan has been stopped and `PcList` cleared? Cache the reference locally before stopping.

### 5. WinForms / Form Lifecycle

- Is a child form opened directly with `new MyForm().Show()` instead of `AbrirFormulario(() => new MyForm())`? The wrapper prevents duplicate form instances and wires the panel correctly.
- Are colors compared or stored as `Color`? They must be stored as `int` via `.ToArgb()`.

### 6. Security / Privilege

- Is the selected `NetworkInterface` validated before any packet send/capture begins?
- Is the license-accepted flag checked before ARP operations start?

---

## Output Format

Provide, in order:

1. **Root cause** — one concise paragraph naming which category above applies and exactly why the bug occurs.
2. **Fix** — a minimal, targeted code diff or replacement snippet for the affected file(s). Do not refactor surrounding code.
3. **Convention violations** — bullet list of any other convention violations spotted in the same scope (naming, threading, etc.) that should be fixed, even if they are not the immediate cause of the bug. Keep this brief.
