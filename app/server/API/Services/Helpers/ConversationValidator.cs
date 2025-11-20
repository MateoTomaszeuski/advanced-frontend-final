using API.Models;
using API.Repositories;

namespace API.Services.Helpers;

public class ConversationValidator {
    private readonly IConversationRepository _conversationRepository;

    public ConversationValidator(IConversationRepository conversationRepository) {
        _conversationRepository = conversationRepository;
    }

    public async Task<(bool isValid, Conversation? conversation)> ValidateUserOwnsConversationAsync(
        int conversationId,
        int userId) {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId) {
            return (false, null);
        }
        return (true, conversation);
    }
}