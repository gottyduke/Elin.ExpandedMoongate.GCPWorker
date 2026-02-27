namespace EGate.GCP.Helper;

internal static class StringHelper
{
    extension(string input)
    {
        public string ExtractInBetween(string left, string right)
        {
            var i = input.IndexOf(left, StringComparison.Ordinal);
            if (i < 0) {
                return "";
            }
            i += left.Length;
            var j = input.IndexOf(right, i, StringComparison.Ordinal);
            return j < 0 ? input[i..] : input[i..j];
        }

        public string SanitizeFileName(char replacement)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = input
                .Select(c => invalid.Contains(c) ? replacement : c)
                .ToArray();
            return new(chars);
        }
    }
}