using System;
using BCrypt.Net;

class ScratchProgram
{
    static void Main()
    {
        string hash = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
        Console.WriteLine(hash);
    }
}
