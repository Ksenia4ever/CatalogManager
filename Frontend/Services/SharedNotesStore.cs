using System.Text.Json;
using System.Text.RegularExpressions;

namespace CatalogManager.Services;

public sealed record NoteBlock(string Id, string Type, string Text);
public sealed record NotesSnapshot(string DocumentId, long Version, List<NoteBlock> Blocks);

public sealed class NotesVersionConflictException : Exception
{
    public NotesVersionConflictException() : base("The document has changed in another browser.") { }
}

public sealed class SharedNotesStore
{
    private static readonly HashSet<string> ValidTypes =
        new(StringComparer.Ordinal) { "paragraph", "heading1", "heading2", "bullets", "numbers" };

    private readonly string _folder;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public SharedNotesStore(IWebHostEnvironment environment)
    {
        _folder = Path.Combine(environment.ContentRootPath, "App_Data", "SharedNotes");
        Directory.CreateDirectory(_folder);
    }

    public static string ValidateId(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId) ||
            !Regex.IsMatch(documentId, @"^[a-zA-Z0-9_-]{1,64}$"))
            throw new ArgumentException("Document ID must contain 1–64 letters, numbers, - or _.");
        return documentId;
    }

    private string FileName(string documentId) =>
        Path.Combine(_folder, ValidateId(documentId) + ".json");

    private async Task<NotesSnapshot> ReadUnsafeAsync(string documentId)
    {
        var path = FileName(documentId);
        if (!File.Exists(path)) return new NotesSnapshot(documentId, 0, new());

        await using var stream = File.OpenRead(path);
        var snapshot = await JsonSerializer.DeserializeAsync<NotesSnapshot>(stream, _json);
        return snapshot ?? new NotesSnapshot(documentId, 0, new());
    }

    public async Task<NotesSnapshot> GetAsync(string documentId)
    {
        ValidateId(documentId);
        await _gate.WaitAsync();
        try { return await ReadUnsafeAsync(documentId); }
        finally { _gate.Release(); }
    }

    public async Task<NotesSnapshot> SaveAsync(
        string documentId, long expectedVersion, List<NoteBlock>? input)
    {
        ValidateId(documentId);
        if (input is null || input.Count > 500)
            throw new ArgumentException("A document must contain at most 500 blocks.");

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var block in input)
        {
            if (block is null || string.IsNullOrWhiteSpace(block.Id) ||
                block.Id.Length > 100 || !ids.Add(block.Id) ||
                !ValidTypes.Contains(block.Type) || block.Text is null ||
                block.Text.Length > 20_000)
                throw new ArgumentException("Invalid block in document.");
        }

        await _gate.WaitAsync();
        try
        {
            var current = await ReadUnsafeAsync(documentId);
            if (current.Version != expectedVersion)
                throw new NotesVersionConflictException();

            var next = new NotesSnapshot(documentId, current.Version + 1,
                input.Select(b => new NoteBlock(b.Id, b.Type, b.Text)).ToList());
            var path = FileName(documentId);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await using (var stream = File.Create(temporary))
                    await JsonSerializer.SerializeAsync(stream, next, _json);
                File.Move(temporary, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
            return next;
        }
        finally { _gate.Release(); }
    }
}