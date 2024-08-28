using System;
using System.Security.Cryptography;

public static class Settings
{
    // Gera uma chave secreta segura com o comprimento especificado
    public static string GenerateRandomKey(int size)
    {
        var key = new byte[size];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }

    // Define a chave secreta com um comprimento adequado
    // A chave secreta pode ser gerada uma vez e armazenada de forma segura
    // Aqui geramos uma chave secreta de 256 bits (32 bytes) como exemplo
    public static string Secret = "mBURT6NHkEJidnNOWFpnX1ywC0uKloulSZq2oMoiE1I="; // 32 bytes * 8 bits/byte = 256 bits
}
