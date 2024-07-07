namespace WebService.Exceptions;

public class BadRequestException(string message) : Exception(message);