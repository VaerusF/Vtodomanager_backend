using System;
using System.Security.Cryptography;
using System.Text;
using Vtodo.DomainServices.Interfaces;
using Vtodo.Entities.Models;

namespace Vtodo.DomainServices.Implementation
{
    internal class SecurityService : ISecurityService
    {
        public string HashPassword(string password, int keySize, int iterations, out byte[] salt)
        {
            salt = GenerateSalt(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize
            );
            return Convert.ToHexString(hash);
        }

        public bool VerifyPassword(string password, string hashedPassword, int keySize, int iterations, byte[] salt)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize
            );
            return hashedPassword.Equals(Convert.ToHexString(hash));
        }

        public byte[] GenerateSalt(int keySize)
        {
            return RandomNumberGenerator.GetBytes(keySize);
        }

        public string GenerateConfirmAccountHashedUrlPart(Account account, int keySize, int iterations, out byte[] salt)
        {
            salt = GenerateSalt(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes($"{Guid.NewGuid()}_{account.Email}_{account.Username}"),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize
            );

            return Convert.ToHexString(hash);
        }
    }
}