using System.ComponentModel.DataAnnotations;
using Projects.Api.Models;
using Xunit;

namespace Projects.Api.Tests;

public class ProjectValidationTests
{
    // Допоміжний метод для емуляції валідації, яку ASP.NET Core виконує автоматично
    private List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Fact]
    public void Request_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Valid Project Name",
            Description = "Valid description"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Request_WithEmptyOrWhitespaceName_ShouldFailValidation(string invalidName)
    {
        // Arrange
        var request = new CreateProjectRequest { Name = invalidName };

        // Act
        var results = ValidateModel(request);

        // Assert
        var failure = Assert.Single(results);
        Assert.Contains("Name", failure.MemberNames);
    }

    [Fact]
    public void Request_WithNameLongerThan100Chars_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('A', 101);
        var request = new CreateProjectRequest { Name = longName };

        // Act
        var results = ValidateModel(request);

        // Assert
        var failure = Assert.Single(results);
        Assert.Contains("Name", failure.MemberNames);
    }

    [Fact]
    public void Request_WithDescriptionLongerThan500Chars_ShouldFailValidation()
    {
        // Arrange
        var longDescription = new string('B', 501);
        var request = new CreateProjectRequest
        {
            Name = "Valid Name",
            Description = longDescription
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        var failure = Assert.Single(results);
        Assert.Contains("Description", failure.MemberNames);
    }
}