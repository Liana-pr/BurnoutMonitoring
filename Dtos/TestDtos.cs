namespace BurnoutMonitoring.Dtos;

// Запрос на создание сессии тестирования
public record CreateSessionRequest(Guid MethodId);

// Ответ на создание сессии — клиент получает хеш и вопросы
public record CreateSessionResponse(string SessionHash, List<QuestionDto> Questions);

public record QuestionDto(Guid QuestionId, int Ordinal, string Text, string ScaleCode);

// Запрос на отправку ответов
public record SubmitTestRequest(string SessionHash, float OvertimeHours, List<AnswerDto> Answers);
public record AnswerDto(Guid QuestionId, short Score);

// Ответ после отправки — что система решила про риск
public record SubmitTestResponse(string SessionHash, string RiskClass, decimal Probability, List<string> Factors);