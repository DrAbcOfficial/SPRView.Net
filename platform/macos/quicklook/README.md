# macOS Quick Look thumbnail provider for `.spr`

Quick Look on modern macOS only accepts thumbnails from an **app extension**
(`com.apple.quicklook.thumbnail`). App extensions must be built and code
signed with Xcode, so this directory ships the Swift sources plus the
packaging glue; the sprite rendering itself lives in the NativeAOT
`sprview-thumbnailer` executable that the extension invokes.

```
Finder ──► Quick Look ──► SPRView QuickLook.appex (Swift, this folder)
                                │ runs
                                ▼
                     /usr/local/bin/sprview-thumbnailer   (NativeAOT, src/SPRView.Net.Thumbnailer)
                                │ renders
                                ▼
                          temp PNG ◄── QLThumbnailReply(imageFileURL:)
```

## 1. Build and install the renderer

```sh
dotnet publish src/SPRView.Net.Thumbnailer -c Release -r osx-arm64 \
    -o build/thumbnailer/osx-arm64
sudo platform/macos/install.sh build/thumbnailer/osx-arm64/sprview-thumbnailer
```

(Use `-r osx-x64` on Intel Macs; the extension only talks to the binary, so
both architectures work identically.)

## 2. Build the Quick Look extension

1. In Xcode: **File ▸ New ▸ Project ▸ macOS ▸ App**, then add a
   **Quick Look Thumbnail Extension** target to it (File ▸ New ▸ Target).
2. Replace the generated `ThumbnailProvider.swift` and `Info.plist` with the
   ones from this folder (`NSExtensionPrincipalClass` uses
   `$(PRODUCT_MODULE_NAME).ThumbnailProvider`, so the type name must stay
   `ThumbnailProvider`).
3. In the **host app's** target, declare the `.spr` file type so Quick Look
   knows which files to route to the extension
   (target ▸ Info ▸ Document Types / UTImportedTypeDeclarations):

   ```xml
   <key>UTImportedTypeDeclarations</key>
   <array>
     <dict>
       <key>UTTypeIdentifier</key>
       <string>net.sprview.spr</string>
       <key>UTTypeConformsTo</key>
       <array><string>public.data</string></array>
       <key>UTTypeDescription</key>
       <string>GoldSrc sprite</string>
       <key>UTTypeIdentifierFiles</key><array/>
       <key>UTTypeTagSpecification</key>
       <dict>
         <key>public.filename-extension</key>
         <array><string>spr</string></array>
         <key>public.mime-type</key>
         <array><string>application/x-spr</string></array>
       </dict>
     </dict>
   </array>
   ```

   The appex `Info.plist` in this folder already references
   `net.sprview.spr` under `QLSupportedContentTypes`.
4. The extension runs sandboxed by default; it only spawns
   `/usr/local/bin/sprview-thumbnailer`, so **no** extra entitlements
   (network, user files) are needed. Keep
   `com.apple.security.app-sandbox` enabled.
5. Build and run the host app once — macOS registers the extension; Quick
   Look on any `.spr` file now renders the sprite's first frame.

## Alternative: link the C ABI directly

If you would rather render inside the extension process, publish the core as
a dylib and call the C ABI from Swift (import `sprview_core.h`):

```sh
dotnet publish src/SPRView.Net.Core -c Release -r osx-arm64 \
    -p:NativeLib=Shared -p:PublishAot=true
```

The header at `src/SPRView.Net.Core/Native/sprview_core.h` documents every
entry point (`sprview_open`, `sprview_render_png`, …). The process-based
design above is recommended instead: it isolates renderer crashes from
Quick Look and keeps a single code path shared with Linux.
