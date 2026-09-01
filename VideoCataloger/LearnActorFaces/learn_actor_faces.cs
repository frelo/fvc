using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using VideoCataloger;

// Learn faces for every actor that has companion images without a learned face embedding.
// Does the same work as the "Learn Face" button in the Edit Actor dialog, but for all
// actors in the catalog in one go.
//
// Rules (same as the Edit Actor dialog):
//  - Images that already have an embedding are skipped.
//  - If the actor has no learned faces yet, only images with exactly one face are used.
//    Single-face images are processed first so they can seed matching for group photos.
//  - Once the actor has learned faces, a face in a new image must match them (>= 50%
//    average similarity) before it is saved - this keeps other people in group photos
//    from being learned as this actor.
class Script
{
    // FNV-1a 32-bit hash of the normalized image path. Must match SImageInfo.PathHash in
    // the Edit Actor dialog - embeddings are keyed by source "companion_{hash}" and this is
    // how both sides know an image is already learned.
    static string PathHash(string path)
    {
        string normalized = path.ToLowerInvariant().Replace("/", "\\");
        const uint FNV_PRIME = 16777619;
        const uint FNV_OFFSET_BASIS = 2166136261;
        uint hash = FNV_OFFSET_BASIS;
        foreach (char c in normalized)
        {
            hash ^= c;
            hash *= FNV_PRIME;
        }
        return hash.ToString("X8");
    }

    // Fast Video Cataloger references images inside zip archives as
    // "C:\path\archive.zip#folder/image.jpg". The "#" separator and the entry format must
    // match what the catalog stores, so the PathHash keys stay compatible with the
    // Edit Actor dialog.
    static bool IsZipEntryPath(string path)
    {
        int separator = path.IndexOf('#');
        return separator > 0 &&
               path.Substring(0, separator).EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsZipFile(string path)
    {
        return path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsImageFile(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".jpg" || ext == ".jpeg" || ext == ".jpe" || ext == ".png" || ext == ".gif";
    }

    // Read image bytes from a plain file or a zip-internal path ("archive.zip#entry.jpg").
    static byte[] ReadImageBytes(string path)
    {
        if (IsZipEntryPath(path))
        {
            int separator = path.IndexOf('#');
            string zip_path = path.Substring(0, separator);
            string entry_path = path.Substring(separator + 1);
            using (var archive = ZipFile.OpenRead(zip_path))
            {
                var entry = archive.GetEntry(entry_path);
                if (entry == null)
                    return null;
                using (var entry_stream = entry.Open())
                using (var memory = new MemoryStream((int)entry.Length))
                {
                    entry_stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    // List all image entries in a zip archive as "archive.zip#entry" paths.
    static List<string> GetImagesFromZip(string zip_path)
    {
        var result = new List<string>();
        try
        {
            using (var archive = ZipFile.OpenRead(zip_path))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) || entry.FullName.EndsWith("/"))
                        continue;
                    if (IsImageFile(entry.Name))
                        result.Add(zip_path + "#" + entry.FullName);
                }
            }
        }
        catch (Exception)
        {
            // Corrupt or unreadable archive - treat as containing no images.
        }
        return result;
    }

    // A companion entry can be a single image or a folder reference - expand folders to
    // the image files inside them (including images inside zip archives), like the dialog does.
    static List<string> ExpandCompanionEntry(string path)
    {
        var result = new List<string>();
        if (Directory.Exists(path))
        {
            foreach (string file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                if (IsImageFile(file))
                    result.Add(file);
                else if (IsZipFile(file))
                    result.AddRange(GetImagesFromZip(file));
            }
        }
        else
        {
            result.Add(path);
        }
        return result;
    }

    static public async System.Threading.Tasks.Task Run(IScripting scripting, string arguments)
    {
        IConsole console = scripting.GetConsole();
        IGUI gui = scripting.GetGUI();
        IUtilities utilities = scripting.GetUtilities();
        var catalog = scripting.GetVideoCatalogService();

        console.Clear();

        // The face engine is initialized on demand; null means it is not available.
        var face_service = scripting.GetFaceRecognition();
        if (face_service == null)
        {
            console.WriteLine("Face recognition is not initialized. Download the face models (AI settings) and try again.");
            return;
        }

        gui.SetCancelSupported(true);

        var actors = await Task.Run(() => catalog.GetActors(null, null, null, true));
        if (actors == null || actors.Length == 0)
        {
            console.WriteLine("No actors in the catalog.");
            return;
        }

        // Existing embeddings per actor, plus a set of already-learned image keys so we
        // never learn the same image twice.
        var all_embeddings = await Task.Run(() => catalog.GetAllActorEmbeddings());
        var embeddings_by_actor = new Dictionary<long, List<VideoCatalogService.FaceRecognition.FaceEmbedding>>();
        var learned_sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (all_embeddings != null)
        {
            foreach (var entry in all_embeddings)
            {
                if (!string.IsNullOrEmpty(entry.SourceType))
                    learned_sources.Add(entry.ActorID + ":" + entry.SourceType);
                var embedding = VideoCatalogService.FaceRecognition.FaceEmbedding.FromBytes(entry.Embedding);
                if (embedding == null)
                    continue;
                List<VideoCatalogService.FaceRecognition.FaceEmbedding> list;
                if (!embeddings_by_actor.TryGetValue(entry.ActorID, out list))
                {
                    list = new List<VideoCatalogService.FaceRecognition.FaceEmbedding>();
                    embeddings_by_actor[entry.ActorID] = list;
                }
                list.Add(embedding);
            }
        }

        int total_learned = 0;
        int total_failed = 0;
        int total_skipped = 0;

        for (int actor_index = 0; actor_index < actors.Length; ++actor_index)
        {
            if (gui.IsCancelRequested())
                break;

            var actor = actors[actor_index];
            string actor_name = (actor.FirstName + " " + actor.LastName).Trim();
            gui.SetProgress(actor_index, 0, actors.Length, "Learning faces: " + actor_name);

            var companions = await Task.Run(() => catalog.GetActorCompanionImages(actor.ID));
            if (companions == null || companions.Length == 0)
                continue;

            List<VideoCatalogService.FaceRecognition.FaceEmbedding> existing;
            if (!embeddings_by_actor.TryGetValue(actor.ID, out existing))
            {
                existing = new List<VideoCatalogService.FaceRecognition.FaceEmbedding>();
                embeddings_by_actor[actor.ID] = existing;
            }

            // Collect the unlearned images for this actor.
            var candidates = new List<string>();
            foreach (var companion in companions)
            {
                string entry_path = string.IsNullOrEmpty(companion.Path) ? companion.ServerPath : companion.Path;
                if (string.IsNullOrEmpty(entry_path))
                    continue;
                entry_path = utilities.ConvertToLocalPath(entry_path);
                entry_path = entry_path.Replace("''", "'");

                foreach (string image_path in ExpandCompanionEntry(entry_path))
                {
                    if (!learned_sources.Contains(actor.ID + ":companion_" + PathHash(image_path)))
                        candidates.Add(image_path);
                    else
                        total_skipped++;
                }
            }
            if (candidates.Count == 0)
                continue;

            // Detect faces in every candidate, then order single-face images first so an
            // actor with no learned faces gets seeded before any group photos are matched.
            var detected = new List<Tuple<string, VideoCatalogService.FaceRecognition.FaceEmbedding[]>>();
            foreach (string image_path in candidates)
            {
                if (gui.IsCancelRequested())
                    break;
                gui.SetProgress(actor_index, 0, actors.Length, actor_name + ": " + System.IO.Path.GetFileName(image_path));

                byte[] image_data = null;
                try
                {
                    image_data = await Task.Run(() => ReadImageBytes(image_path));
                }
                catch (Exception)
                {
                }
                if (image_data == null)
                {
                    console.WriteLine(actor_name + ": could not read " + image_path);
                    total_failed++;
                    continue;
                }

                var faces = await Task.Run(() => face_service.DetectFacesWithEmbeddings(image_data, true));
                if (faces == null || faces.Length == 0)
                {
                    console.WriteLine(actor_name + ": no face detected in " + System.IO.Path.GetFileName(image_path));
                    total_failed++;
                    continue;
                }
                detected.Add(Tuple.Create(image_path, faces));
            }
            detected.Sort((a, b) => a.Item2.Length.CompareTo(b.Item2.Length));

            foreach (var item in detected)
            {
                string image_path = item.Item1;
                var faces = item.Item2;

                VideoCatalogService.FaceRecognition.FaceEmbedding face_to_save = null;
                if (existing.Count > 0)
                {
                    // Pick the face that best matches the actor's already-learned faces.
                    double best_similarity = 0;
                    foreach (var face in faces)
                    {
                        double avg_similarity = 0;
                        foreach (var known in existing)
                            avg_similarity += face_service.CalculateSimilarity(face, known);
                        avg_similarity /= existing.Count;
                        if (avg_similarity > best_similarity)
                        {
                            best_similarity = avg_similarity;
                            face_to_save = face;
                        }
                    }
                    if (best_similarity < 0.5)
                    {
                        console.WriteLine(actor_name + ": no matching face in " + System.IO.Path.GetFileName(image_path) + " (best " + best_similarity.ToString("P0") + ")");
                        total_failed++;
                        continue;
                    }
                }
                else
                {
                    if (faces.Length > 1)
                    {
                        console.WriteLine(actor_name + ": " + faces.Length + " faces in " + System.IO.Path.GetFileName(image_path) + " and no learned face to match against - skipped");
                        total_failed++;
                        continue;
                    }
                    face_to_save = faces[0];
                }

                string source_key = "companion_" + PathHash(image_path);
                await Task.Run(() => catalog.SaveActorFaceEmbedding(actor.ID, face_to_save.ToBytes(), source_key));
                existing.Add(face_to_save);
                learned_sources.Add(actor.ID + ":" + source_key);
                total_learned++;
                console.WriteLine(actor_name + ": learned face from " + System.IO.Path.GetFileName(image_path));
            }
        }

        gui.SetProgress(actors.Length, 0, actors.Length, "Learn faces finished");
        // -1 takes the progress display down again. Without it the title bar kept showing
        // "Learn faces finished" at 100% until the program was restarted.
        gui.SetProgress(-1, 0, 0, "");
        console.WriteLine("");
        console.WriteLine("Done. Learned " + total_learned + " face(s), " + total_skipped + " already learned, " + total_failed + " failed.");
    }
}
