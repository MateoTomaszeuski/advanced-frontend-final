namespace API.DTOs.Chat;

public record ChatRequestDto(List<ChatMessageDto> Messages, int? ConversationId = null);

public record ChatMessageDto(string Role, string Content);