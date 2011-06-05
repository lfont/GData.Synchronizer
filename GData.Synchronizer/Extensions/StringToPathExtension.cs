using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GData.Synchronizer.Extensions
{
	public static class StringToPathExtension
	{
		private static readonly List<char> InvalidFileNameCharacters = new List<char> (Path.GetInvalidFileNameChars ());
		private static readonly List<char> InvalidPathCharacters = new List<char> (Path.GetInvalidPathChars ()) { '.' };

		public static string GetValidFileName (this string str)
		{
		    return str.Aggregate(String.Empty, (current, c) => current + (InvalidFileNameCharacters.Contains(c) ? '_' : c));
		}
		
		public static string GetValidPath (this string str)
		{
			return str.Aggregate(String.Empty, (current, c) => current + (InvalidPathCharacters.Contains(c) ? '_' : c));
		}

        public static string Combine(this string str, params string[] args)
        {
            if (args == null || args.Length == 0)
            {
                throw new ArgumentException("args");
            }

            return args.Aggregate(str, Path.Combine);
        }
	}
}

