using System;
using System.Security.Cryptography;
using System.Text;
using CommandLine;
using unc.Security.Cryptography;

namespace unc
{
    public class Options
    {
        public const string HelpText = "unc -f <first name> -l <last name> -d <Date Of Birth> or unc -h";

        [Option('f', "firstname", Required = true, HelpText = "The users first name")]
        public string FirstName { get; set; }
        [Option('l', "lastname", Required = true, HelpText = "The users last name")]
        public string LastName { get; set; }
        [Option('d', "dateOfBirth", Required = true, HelpText = "The users date of birth")]
        public string DateOfBirth { get; set; }
        [Option('s', "salt", Required = false, HelpText = "an optional secret")]
        public string Salt { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option('h', "Help", Required = false, HelpText = HelpText)]
        public string Help { get; set; }
    }
    public class Program
    {
        public static void Main(params string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                if (String.IsNullOrEmpty(o.FirstName) || String.IsNullOrEmpty(o.LastName) ||
                    String.IsNullOrEmpty(o.DateOfBirth))
                    OutputHelpText();
                else
                    Console.WriteLine(CalculateUserName(o.FirstName, o.LastName, o.DateOfBirth, o.Salt, o.Verbose));
            });
        }

        public static void OutputHelpText()
        {
            Console.WriteLine(Options.HelpText);
        }

        public static string CalculateUserName(String firstName, String lastName, String dob, String salt, bool verbose = false)
        {
            var sha256Hash = ComputeSha256Hash(Encoding.UTF8.GetBytes(firstName + lastName + dob + salt));
            if (verbose) Console.WriteLine("SHA256: " + byteArrayToString(sha256Hash));
            var ripemdHash = ComputeRipeMdHash(sha256Hash);
            if (verbose) Console.WriteLine("RIPEMD 160: " + byteArrayToString(ripemdHash));
            var crc32 = ComputeCrc32(ripemdHash);
            if (verbose) Console.WriteLine("CRC32: " + byteArrayToString(crc32));
            return firstName + lastName + byteArrayToString(crc32);
        }
        private static byte[] ComputeSha256Hash(byte[] data)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(data);
            }
        }

        private static byte[] ComputeRipeMdHash(byte[] data)
        {
            using (RIPEMD160 ripemd160 = RIPEMD160Managed.Create())
            {
                return ripemd160.ComputeHash(data);
            }
        }

        private static byte[] ComputeCrc32(byte[] data)
        {
            using (var crc32 = new Crc32())
            {
                return crc32.ComputeHash(data);
            }
        }
        
        private static string byteArrayToString(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var t in bytes)
                builder.Append(t.ToString("x2"));

            return builder.ToString();
        }
    }
}
