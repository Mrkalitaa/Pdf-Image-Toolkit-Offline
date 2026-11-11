# Third-Party Software Licenses & Notices

This application ("PDF & Image Toolkit") uses several open-source libraries and frameworks. In strict accordance with our architectural guidelines, **no AGPL, GPL, or restrictive copyleft libraries are used in this product**. All third-party dependencies are licensed under permissive licenses (MIT, Apache-2.0, or Microsoft Software Licenses).

---

## 1. Summary of Dependencies

| Component | License | Purpose | Project / Repository |
| :--- | :--- | :--- | :--- |
| **PDFsharp** | MIT License | PDF merging, splitting, rotation, and PDF generation | [PDFsharp GitHub](https://github.com/empira/PDFsharp) |
| **Magick.NET** (`Magick.NET-Q8-x64`) | Apache License 2.0 | High-performance image conversion, resizing, and compression | [Magick.NET GitHub](https://github.com/dlemstra/Magick.NET) |
| **Windows App SDK** | MIT License | WinUI 3 modern desktop presentation, Mica backdrop, Windows controls | [WindowsAppSDK GitHub](https://github.com/microsoft/WindowsAppSDK) |
| **CommunityToolkit.Mvvm** | MIT License | High-performance MVVM framework, code generators, ObservableObject | [CommunityToolkit GitHub](https://github.com/CommunityToolkit/dotnet) |
| **Microsoft.Extensions.DependencyInjection** | MIT License | Inversion of Control (IoC) and dependency injection container | [dotnet/runtime GitHub](https://github.com/dotnet/runtime) |
| **Microsoft.Windows.SDK.BuildTools** | Microsoft Software License | Header files, IDL compilers, and Windows App SDK build targets | Microsoft Corporation |
| **xUnit.net** | Apache License 2.0 | Unit test execution and assertion engine | [xUnit GitHub](https://github.com/xunit/xunit) |
| **Coverlet** (`coverlet.collector`) | MIT License | Code coverage collector for automated tests | [coverlet GitHub](https://github.com/coverlet-coverage/coverlet) |

---

## 2. Full License Texts

### MIT License

Used by: PDFsharp, Windows App SDK, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Coverlet.

```
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

### Apache License, Version 2.0

Used by: Magick.NET, xUnit.net.

```
                              Apache License
                        Version 2.0, January 2004
                     http://www.apache.org/licenses/

TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

1. Definitions.
"License" shall mean the terms and conditions for use, reproduction, and
distribution as defined by Sections 1 through 9 of this document.
"Licensor" shall mean the copyright owner or entity authorized by the copyright
owner that is granting the License.
"Legal Entity" shall mean the union of the acting entity and all other entities
that control, are controlled by, or are under common control with that entity.
"You" (or "Your") shall mean an individual or Legal Entity exercising permissions
granted by this License.
"Source" form shall mean the preferred form for making modifications, including
but not limited to software source code, documentation source, and configuration files.
"Object" form shall mean any form resulting from mechanical transformation or
translation of a Source form, including but not limited to compiled object code,
generated documentation, and conversions to other media types.
"Work" shall mean the work of authorship, whether in Source or Object form, made
available under the License, as indicated by a copyright notice that is included
in or attached to the work.
"Derivative Works" shall mean any work, whether in Source or Object form, that is
based on (or derived from) the Work and for which the editorial revisions,
annotations, elaborations, or other modifications represent, as a whole, an
original work of authorship.

2. Grant of Copyright License. Subject to the terms and conditions of this
License, each Contributor hereby grants to You a perpetual, worldwide,
non-exclusive, no-charge, royalty-free, irrevocable copyright license to
reproduce, prepare Derivative Works of, publicly display, publicly perform,
sublicense, and distribute the Work and such Derivative Works in Source or
Object form.

3. Grant of Patent License. Subject to the terms and conditions of this License,
each Contributor hereby grants to You a perpetual, worldwide, non-exclusive,
no-charge, royalty-free, irrevocable patent license to make, have made, use,
offer to sell, sell, import, and otherwise transfer the Work.

4. Redistribution. You may reproduce and distribute copies of the Work or
Derivative Works thereof in any medium, with or without modifications, and in
Source or Object form, provided that You meet the following conditions:
(a) You must give any other recipients of the Work or Derivative Works a copy of
    this License; and
(b) You must cause any modified files to carry prominent notices stating that You
    changed the files; and
(c) You must retain, in the Source form of any Derivative Works that You
    distribute, all copyright, patent, trademark, and attribution notices from
    the Source form of the Work; and
(d) If the Work includes a "NOTICE" text file as part of its distribution, You
    must include a readable copy of the attribution notices contained within
    such NOTICE file.

5. Disclaimer of Warranty. Unless required by applicable law or agreed to in
writing, Licensor provides the Work on an "AS IS" BASIS, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied, including, without limitation,
any warranties or conditions of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or
FITNESS FOR A PARTICULAR PURPOSE.

6. Limitation of Liability. In no event and under no legal theory, whether in
tort (including negligence), contract, or otherwise, unless required by applicable
law (such as deliberate and grossly negligent acts) or agreed to in writing, shall
any Contributor be liable to You for damages, including any direct, indirect,
special, incidental, or consequential damages of any character arising as a
result of this License or out of the use or inability to use the Work.
```

---

## 3. Privacy & Offline Processing Guarantee

PDF & Image Toolkit operates **100% locally on your device**:
- No user accounts, logins, or tracking identifiers.
- No analytics, telemetry, or diagnostic tracking in application logic.
- Files never leave your PC; all PDF rendering, image conversions, compression, and manipulations occur in local RAM and private application temp storage.
- The Microsoft Store purchase interaction utilizes official Microsoft Store APIs (`Windows.Services.Store.StoreContext`) solely for licensing verification.
