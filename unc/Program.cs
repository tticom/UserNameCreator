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
            var sha256Hash = ComputeSha256Hash(firstName + lastName + dob + salt, verbose);
            var ripemdHash = ComputeRipeMdHash(sha256Hash, verbose);
            var crc32 = ComputeCrc32(ripemdHash, verbose);
            return firstName + lastName + crc32;
        }
        private static string ComputeSha256Hash(string data, bool verbose)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                var computedSha256Hash = byteArrayToString(bytes);
                if(verbose) Console.WriteLine("SHA256: " + computedSha256Hash);
                return computedSha256Hash;
            }
        }

        private static string ComputeRipeMdHash(string data, bool verbose)
        {
            using (RIPEMD160 ripemd160 = RIPEMD160Managed.Create())
            {
                var bytes = ripemd160.ComputeHash(Encoding.UTF8.GetBytes(data));
                var computedRipeMdHash = byteArrayToString(bytes);
                if (verbose) Console.WriteLine("RIPEMD 160: " + computedRipeMdHash);
                return computedRipeMdHash;
            }
        }

        private static string ComputeCrc32(string data, bool verbose)
        {
            using (var crc32 = new Crc32())
            {
                var bytes = crc32.ComputeHash(Encoding.UTF8.GetBytes(data));
                var computedCrc32 = byteArrayToString(bytes);
                if (verbose) Console.WriteLine("CRC32: " + computedCrc32);
                return computedCrc32;
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
