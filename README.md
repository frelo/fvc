# Fast Video Cataloger — C# script samples

Sample C# scripts for [Fast Video Cataloger](https://videocataloger.com), the video
cataloging and search application for Windows.

Fast Video Cataloger has a built-in C# scripting console. A script runs inside the running
application and can reach your catalog, the selection, the video player and the UI — so you
can automate the parts of your workflow the menus do not cover.

**Updated for Fast Video Cataloger 10.3.**

## Documentation

| | |
|---|---|
| [Scripting overview](https://videocataloger.com/docs/scripting/) | What scripting can do and how the console works |
| [Write your first script](https://videocataloger.com/docs/getting-started/first-script/) | Start here if you have not written one before |
| [Script structure](https://videocataloger.com/docs/getting-started/script-structure/) | How a script is laid out and what it must contain |
| [IScripting API reference](https://videocataloger.com/docs/api-reference/videocataloger-iscripting/) | Every method the scripting interface exposes |
| [Developer resources](https://videocataloger.com/developers/) | Scripting, the REST API and the rest of the programmable surface |
| [Full documentation](https://videocataloger.com/docs/) | The complete user guide |

Fast Video Cataloger 10.3 also ships a REST API and an
[MCP connector](https://videocataloger.com/docs/server/serving-media/mcp-server/) for
connecting an AI assistant to a catalog. Those run against the Fast Video Cataloger server
rather than inside the application — see
[Let an AI assistant search your video library](https://videocataloger.com/let-an-ai-assistant-search-your-video-library/)
for when to reach for which.

## Using the samples

Each folder is one sample with its own `.csproj`, and `CatalogerSampleScripts.sln` opens the
whole set in Visual Studio. The projects are there so you get IntelliSense and compile errors
while you edit; to actually run a script, paste it into the scripting console inside Fast
Video Cataloger.

`VideoCataloger/` is the shared project the samples reference — it holds the generated service
client for talking to the catalog.

A few starting points:

- **HelloWorld** — the smallest script that does something
- **BasicSelection** — read what the user has selected
- **FilterToBin** — search the catalog and collect the results into a bin
- **ExportVideoList** / **ExportThumbs** — get data and images out of a catalog
- **ImportCSV** / **ImportMDB** — bring metadata in from elsewhere
- **AutoIndexFolder** — index new files as they appear
- **CaptureAtPlayhead** — grab the frame currently showing in the player
- **FindScenesWithFaces**, **LearnActorFaces** — work with the face-recognition data
- **HelloWPF** — build your own window

## Notes

The `.csproj` files are an editing aid only. They compile `ScriptInterface.cs` and the
generated service client directly rather than referencing the application, so a sample opens
and builds on its own and Visual Studio can give you IntelliSense as you write.

They still declare .NET Framework 4.8 — left over from before Fast Video Cataloger moved to
.NET 10. That target does not decide anything at runtime: a script is compiled and run inside
the application by its scripting console, so it runs on whatever the application runs on,
which is .NET 10 from version 10 onwards.

These samples are provided as a starting point — use and adapt them however you like.

Questions and ideas are welcome on our [Discord](https://discord.com/invite/Yz4zxRA6Nh).
