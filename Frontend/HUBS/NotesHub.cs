using CatalogManager.Services;
using Microsoft.AspNetCore.SignalR;

namespace CatalogManager.Hubs;

public sealed class NotesHub(SharedNotesStore store) : Hub
{
    public async Task<NotesSnapshot> JoinDocument(string documentId)
    {
        documentId = SharedNotesStore.ValidateId(documentId);
        await Groups.AddToGroupAsync(Context.ConnectionId, documentId);
        return await store.GetAsync(documentId);
    }

    public async Task<NotesSnapshot> SaveDocument(
        string documentId, long expectedVersion, List<NoteBlock>? blocks)
    {
        documentId = SharedNotesStore.ValidateId(documentId);
        var saved = await store.SaveAsync(documentId, expectedVersion, blocks);
        await Clients.GroupExcept(documentId, Context.ConnectionId)
            .SendAsync("DocumentChanged", saved);
        return saved;
    }
}