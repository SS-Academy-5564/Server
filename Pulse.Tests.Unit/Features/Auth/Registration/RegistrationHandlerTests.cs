using System.Data;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.BL.Features.Auth.Registration;
using Pulse.BL.Features.Email;
using Pulse.DAL.Commands.EmailVerificationTokens;
using Pulse.DAL.Commands.Members;
using Pulse.DAL.Commands.Users;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.Registration;

public class RegistrationHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserQueries> _userQueries = new();
    private readonly Mock<IUserCommands> _userCommands = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IMemberCommands> _memberCommands = new();
    private readonly Mock<IEmailVerificationTokenCommands> _verificationTokenCommands = new();
    private readonly Mock<IEmailVerificationTokenService> _verificationTokenService = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<RegistrationHandler>> _logger = new();
    private readonly DateTimeOffset _now = new(2026, 8, 4, 12, 0, 0, TimeSpan.Zero);
    private readonly RegistrationHandler _handler;

    private const string RawVerificationToken = "secure-verification-token";
    private const string VerificationTokenHash = "TOKEN_HASH";

    public RegistrationHandlerTests()
    {
        _unitOfWork.Setup(u => u.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _unitOfWorkFactory
            .Setup(f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);
        _verificationTokenService.Setup(s => s.GenerateToken()).Returns(RawVerificationToken);
        _verificationTokenService
            .Setup(s => s.ComputeHash(RawVerificationToken))
            .Returns(VerificationTokenHash);
        _emailService
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new RegistrationHandler(
            _unitOfWorkFactory.Object,
            _userCommands.Object,
            _userQueries.Object,
            _passwordHasher.Object,
            _memberCommands.Object,
            _verificationTokenCommands.Object,
            _verificationTokenService.Object,
            _emailService.Object,
            new FixedTimeProvider(_now),
            Options.Create(new EmailVerificationOptions
            {
                TokenLifetimeHours = 24,
                ResendCooldownSeconds = 60,
                VerificationPageUrl = "https://pulse.example.com/verify-email"
            }),
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_EmailAlreadyExists_ReturnsSuccessResultWithoutCreatingUser()
    {
        _userQueries
            .Setup(q => q.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Result<RegistrationResult> result = await _handler.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userCommands.Verify(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsSuccessResultAndDoesNotCreateMember()
    {
        _userQueries
            .Setup(q => q.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed");
        _userCommands
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateUserResult(CreateUserStatus.DuplicateEmail, null));

        Result<RegistrationResult> result = await _handler.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _memberCommands.Verify(m => m.CreateMemberAsync(It.IsAny<CreateMemberInput>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesAccountTokenAndEnglishVerificationEmail()
    {
        RegistrationCommand command = ValidCommand();
        var userId = Guid.NewGuid();
        const string hashedPassword = "hashed_password";
        const string expectedVerificationUrl =
            "https://pulse.example.com/verify-email?token=secure-verification-token";

        _userQueries
            .Setup(q => q.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(command.Password))
            .Returns(hashedPassword);
        _userCommands
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateUserResult(CreateUserStatus.Succeeded, userId));

        Result<RegistrationResult> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ResendCooldownSeconds.Should().Be(60);

        _userCommands.Verify(c => c.CreateUserAsync(
            It.Is<CreateUserInput>(u =>
                u.Email == command.Email &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName &&
                u.PasswordHash == hashedPassword),
            It.IsAny<CancellationToken>()), Times.Once);

        _memberCommands.Verify(m => m.CreateMemberAsync(
            It.Is<CreateMemberInput>(mi =>
                mi.UserId == userId &&
                mi.OrganizationId == SeededIds.Organizations.Default &&
                mi.RoleId == SeededIds.Roles.User),
            It.IsAny<CancellationToken>()), Times.Once);

        _verificationTokenCommands.Verify(c => c.CreateAsync(
            It.Is<CreateEmailVerificationTokenInput>(input =>
                input.UserId == userId &&
                input.TokenHash == VerificationTokenHash &&
                input.CreatedAt == _now &&
                input.ExpiresAt == _now.AddHours(24)),
            It.IsAny<CancellationToken>()), Times.Once);

        _emailService.Verify(s => s.SendEmailAsync(
            It.Is<SendEmailDto>(email =>
                email.To.Single() == command.Email &&
                email.Subject == "Verify your Pulse email address" &&
                email.HtmlBody != null &&
                email.HtmlBody.Contains("Verify email address") &&
                email.HtmlBody.Contains("24 hours") &&
                email.HtmlBody.Contains(expectedVerificationUrl) &&
                email.PlainTextBody != null &&
                email.PlainTextBody.Contains("24 hours") &&
                email.PlainTextBody.Contains(expectedVerificationUrl)),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UkrainianLanguage_SendsUkrainianVerificationEmailWithValidLinkAndExpiration()
    {
        RegistrationCommand command = ValidCommand("uk");
        var userId = Guid.NewGuid();
        const string expectedVerificationUrl =
            "https://pulse.example.com/verify-email?token=secure-verification-token";

        _userQueries
            .Setup(q => q.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher
            .Setup(h => h.HashPassword(command.Password))
            .Returns("hashed_password");
        _userCommands
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateUserResult(CreateUserStatus.Succeeded, userId));

        Result<RegistrationResult> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        Uri.TryCreate(expectedVerificationUrl, UriKind.Absolute, out Uri? verificationUri).Should().BeTrue();
        verificationUri!.Scheme.Should().Be(Uri.UriSchemeHttps);

        _emailService.Verify(s => s.SendEmailAsync(
            It.Is<SendEmailDto>(email =>
                email.To.Single() == command.Email &&
                email.Subject == "Підтвердьте адресу електронної пошти Pulse" &&
                email.HtmlBody != null &&
                email.HtmlBody.Contains("Підтвердити електронну адресу") &&
                email.HtmlBody.Contains("24 години") &&
                email.HtmlBody.Contains(expectedVerificationUrl) &&
                email.PlainTextBody != null &&
                email.PlainTextBody.Contains("24 години") &&
                email.PlainTextBody.Contains(expectedVerificationUrl)),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmailDeliveryFails_ReturnsInternalErrorWithoutLoggingToken()
    {
        RegistrationCommand command = ValidCommand();

        _userQueries
            .Setup(q => q.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.HashPassword(command.Password)).Returns("hashed_password");
        _userCommands
            .Setup(c => c.CreateUserAsync(It.IsAny<CreateUserInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateUserResult(CreateUserStatus.Succeeded, Guid.NewGuid()));
        _emailService
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Delivery failed"));

        Result<RegistrationResult> result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<InternalError>();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    !state.ToString()!.Contains(RawVerificationToken, StringComparison.Ordinal)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static RegistrationCommand ValidCommand(string language = "en") => new
    (
        "john.doe@example.com",
        "John",
        "Doe",
        "SecurePass1",
        language
    );

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        /// <summary>
        /// Gets the current UTC time.
        /// </summary>
        /// <returns>The fixed UTC time.</returns>
        public override DateTimeOffset GetUtcNow() => now;
    }
}
