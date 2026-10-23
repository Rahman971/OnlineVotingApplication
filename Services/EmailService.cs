using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OnlineVotingApplication.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // =========================================
        // PASSWORD RESET LINK EMAIL
        // =========================================

        public async Task SendPasswordResetEmailAsync(
            string recipientEmail,
            string recipientName,
            string resetLink)
        {
            var senderEmail =
                _configuration["EmailSettings:Email"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:Email is not configured.");

            var appPassword =
                _configuration["EmailSettings:AppPassword"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:AppPassword is not configured.");

            var displayName =
                _configuration["EmailSettings:DisplayName"]
                ?? "Thulasi College Online Voting System";

            var smtpHost =
                _configuration["EmailSettings:SmtpHost"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:SmtpHost is not configured.");

            var smtpPort =
                _configuration.GetValue<int>(
                    "EmailSettings:SmtpPort");

            if (smtpPort <= 0)
            {
                smtpPort = 587;
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    displayName,
                    senderEmail));

            message.To.Add(
                new MailboxAddress(
                    recipientName,
                    recipientEmail));

            message.Subject =
                "Reset Your Admin Password - Thulasi College";

            var safeName =
                System.Net.WebUtility.HtmlEncode(
                    recipientName);

            var safeResetLink =
                System.Net.WebUtility.HtmlEncode(
                    resetLink);

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Password Reset</title>
</head>

<body style='font-family:Arial,sans-serif;
             background:#f4f7fb;
             padding:30px;'>

    <div style='max-width:600px;
                margin:auto;
                background:white;
                padding:30px;
                border-radius:12px;'>

        <h2 style='color:#123b66;'>
            Thulasi College Online Voting System
        </h2>

        <p>
            Hello {safeName},
        </p>

        <p>
            We received a request to reset your
            administrator password.
        </p>

        <p>
            Click the button below to create a new password.
        </p>

        <p style='margin:30px 0;'>

            <a href='{safeResetLink}'
               style='background:#123b66;
                      color:white;
                      padding:12px 22px;
                      text-decoration:none;
                      border-radius:6px;
                      display:inline-block;'>
                Reset Password
            </a>

        </p>

        <p>
            This password reset link will expire automatically.
        </p>

        <p>
            If you did not request this password reset,
            you can safely ignore this email.
        </p>

        <hr>

        <p style='font-size:12px;color:#777;'>
            Thulasi College of Arts and Science for Women<br>
            Online Voting System
        </p>

    </div>

</body>
</html>";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpHost,
                smtpPort,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                senderEmail,
                appPassword);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }


        // =========================================
        // ADMIN OTP EMAIL
        // =========================================

        public async Task SendAdminOtpEmailAsync(
            string recipientEmail,
            string recipientName,
            string otp)
        {
            var senderEmail =
                _configuration["EmailSettings:Email"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:Email is not configured.");

            var appPassword =
                _configuration["EmailSettings:AppPassword"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:AppPassword is not configured.");

            var displayName =
                _configuration["EmailSettings:DisplayName"]
                ?? "Thulasi College Online Voting System";

            var smtpHost =
                _configuration["EmailSettings:SmtpHost"]
                ?? throw new InvalidOperationException(
                    "EmailSettings:SmtpHost is not configured.");

            var smtpPort =
                _configuration.GetValue<int>(
                    "EmailSettings:SmtpPort");

            if (smtpPort <= 0)
            {
                smtpPort = 587;
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    displayName,
                    senderEmail));

            message.To.Add(
                new MailboxAddress(
                    recipientName,
                    recipientEmail));

            message.Subject =
                "Admin Password Reset OTP - Thulasi College";

            var safeName =
                System.Net.WebUtility.HtmlEncode(
                    recipientName);

            var safeOtp =
                System.Net.WebUtility.HtmlEncode(
                    otp);

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Admin Password Reset OTP</title>
</head>

<body style='font-family:Arial,sans-serif;
             background:#f4f7fb;
             padding:30px;'>

    <div style='max-width:600px;
                margin:auto;
                background:white;
                padding:35px;
                border-radius:14px;
                box-shadow:0 8px 30px rgba(0,0,0,0.08);'>

        <h2 style='color:#123b66;
                   margin-bottom:10px;'>
            Thulasi College Online Voting System
        </h2>

        <p>
            Hello {safeName},
        </p>

        <p>
            We received a request to reset your
            administrator password.
        </p>

        <p>
            Your One-Time Password (OTP) is:
        </p>

        <div style='text-align:center;
                    margin:28px 0;'>

            <span style='display:inline-block;
                         background:#eef4ff;
                         color:#123b66;
                         padding:16px 30px;
                         border-radius:10px;
                         font-size:30px;
                         font-weight:bold;
                         letter-spacing:8px;'>
                {safeOtp}
            </span>

        </div>

        <p style='text-align:center;
                  color:#555;'>
            This OTP is valid for
            <strong>5 minutes</strong>.
        </p>

        <p style='color:#555;'>
            Please do not share this OTP with anyone.
        </p>

        <p style='color:#555;'>
            If you did not request a password reset,
            you can safely ignore this email.
        </p>

        <hr style='border:none;
                   border-top:1px solid #e5e7eb;
                   margin:25px 0;'>

        <p style='font-size:12px;
                  color:#777;
                  line-height:1.6;'>
            Thulasi College of Arts and Science for Women<br>
            Online Voting System<br>
            Secure • Transparent • Digital Elections
        </p>

    </div>

</body>
</html>";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body =
                bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpHost,
                smtpPort,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                senderEmail,
                appPassword);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);
        }
    }
}