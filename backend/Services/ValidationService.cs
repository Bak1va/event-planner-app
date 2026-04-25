namespace Backend.Services;

public interface IValidationService
{
    string? ValidateUserRequest(string? name, string? email, string? password = null);
    string? ValidateEventRequest(string? name, string? status, string? description, string? imageUrl = null);
}

public class ValidationService : IValidationService
{
    public string? ValidateUserRequest(string? name, string? email, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (name.Trim().Length > 100)
        {
            return "Name cannot exceed 100 characters.";
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return "Email is required.";
        }

        if (email.Trim().Length > 150)
        {
            return "Email cannot exceed 150 characters.";
        }

        if (password is not null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return "Password is required.";
            }

            if (password.Length < 6)
            {
                return "Password must be at least 6 characters.";
            }
        }

        return null;
    }

    public string? ValidateEventRequest(string? name, string? status, string? description, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Event name is required.";
        }

        if (name.Trim().Length > 150)
        {
            return "Event name cannot exceed 150 characters.";
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return "Status is required.";
        }

        if (status.Trim().Length > 100)
        {
            return "Status cannot exceed 100 characters.";
        }

        if (description is not null && description.Trim().Length > 1000)
        {
            return "Description cannot exceed 1000 characters.";
        }

        return null;
    }
}

