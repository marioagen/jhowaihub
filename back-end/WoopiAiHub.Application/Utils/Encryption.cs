using System;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace WoopiAiHub.Application.Utils
{
    public static class Encryption
    {
        /// <summary>
        /// Gera um hash para a senha fornecida e retorna como byte[].
        /// </summary>
        /// <param name="password">A senha em texto plano.</param>
        /// <returns>O hash gerado como um array de bytes.</returns>
        public static byte[] GenerateHash(string password)
        {
            string hash = Argon2.Hash(password);
            return Encoding.UTF8.GetBytes(hash);
        }

        /// <summary>
        /// Verifica se a senha fornecida corresponde ao hash armazenado.
        /// </summary>
        /// <param name="password">A senha fornecida pelo usuário.</param>
        /// <param name="storedHashBytes">O hash armazenado no banco de dados como byte[].</param>
        /// <returns>Verdadeiro se a senha for válida; caso contrário, falso.</returns>
        public static bool VerifyHash(string password, byte[] storedHashBytes)
        {
            string storedHash = Encoding.UTF8.GetString(storedHashBytes);
            return Argon2.Verify(storedHash, password);
        }
    }
}