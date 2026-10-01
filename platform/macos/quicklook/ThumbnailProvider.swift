import Foundation
import QuickLookThumbnailing

/// Quick Look thumbnail extension for GoldSrc `.spr` sprite files.
///
/// The extension shells out to the NativeAOT `sprview-thumbnailer` executable
/// (see src/SPRView.Net.Thumbnailer), which renders the sprite into a PNG and
/// hands the file back to Quick Look. This keeps the Quick Look process free
/// of any .NET hosting concerns while reusing the exact same renderer as the
/// Linux XDG thumbnailer.
class ThumbnailProvider: QLThumbnailExtension {
    /// Location the install script places the NativeAOT binary at.
    private static let thumbnailerPath = "/usr/local/bin/sprview-thumbnailer"

    override func provideThumbnail(
        for request: QLFileThumbnailRequest,
        _ handler: @escaping (QLThumbnailReply?, Error?) -> Void
    ) {
        let maximumSize = max(request.maximumSize.width, request.maximumSize.height)
        let outputURL = FileManager.default.temporaryDirectory
            .appendingPathComponent("sprview-\(UUID().uuidString).png")

        deferCleanUp(of: outputURL)

        let thumbnailer = Process()
        thumbnailer.executableURL = URL(fileURLWithPath: Self.thumbnailerPath)
        thumbnailer.arguments = [
            "-s", String(Int(maximumSize)),
            request.fileURL.path,
            outputURL.path
        ]
        // Quick Look extensions run sandboxed; do not pass through the parent
        // environment so PATH surprises cannot redirect the executable.
        thumbnailer.environment = [:]

        do {
            try thumbnailer.run()
            thumbnailer.waitUntilExit()
            guard thumbnailer.terminationStatus == 0,
                  FileManager.default.fileExists(atPath: outputURL.path) else {
                throw CocoaError(.fileNoSuchFile,
                                 userInfo: [NSLocalizedDescriptionKey:
                                    "sprview-thumbnailer failed with status \(thumbnailer.terminationStatus)"])
            }
            handler(QLThumbnailReply(imageFileURL: outputURL), nil)
        } catch {
            handler(nil, error)
        }
    }

    /// Remove the temporary render as soon as Quick Look closes the reply.
    private func deferCleanUp(of url: URL) {
        DispatchQueue.global(qos: .utility).asyncAfter(deadline: .now() + 30) {
            try? FileManager.default.removeItem(at: url)
        }
    }
}
