using System.Text;

namespace Catharsis.Text.RegularExpressions;

///<summary>
///Splits a single CSV line into fields, honoring quoted fields that contain the delimiter, the quote character
///itself (escaped by doubling it), or line breaks embedded within the quotes. Complements
///<see cref="RegexTokenizer"/>, which cannot express quote-aware splitting as a single regular expression.
///</summary>
///<param name="delimiter">The field delimiter. Defaults to a comma.</param>
///<param name="quote">The quote character used to wrap fields containing the delimiter. Defaults to a double quote.</param>
public sealed class CsvLineTokenizer(char delimiter = ',', char quote = '"')
{
    #region Public methods
    ///<summary>
    ///Splits <paramref name="line"/> into its constituent fields.
    ///</summary>
    ///<param name="line">The CSV line to split.</param>
    ///<returns>The fields in <paramref name="line"/>, in order, with surrounding quotes removed and escaped quotes unescaped.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="line"/> is <c>null</c>.</exception>
    public IReadOnlyList<string> Tokenize(string line)
    {
        if(line is null)
        {
            throw new ArgumentNullException(nameof(line), "Line must not be null.");
        }

        List<string> fields = [];
        StringBuilder current = new();
        bool inQuotes = false;
        int index = 0;

        while(index < line.Length)
        {
            char c = line[index];

            if(inQuotes)
            {
                if(c == quote)
                {
                    if(((index + 1) < line.Length) && (line[index + 1] == quote))
                    {
                        current.Append(quote);
                        index += 2;
                        continue;
                    }

                    inQuotes = false;
                    index++;
                    continue;
                }

                current.Append(c);
                index++;
            } else if(c == quote)
            {
                inQuotes = true;
                index++;
            } else if(c == delimiter)
            {
                fields.Add(current.ToString());
                current.Clear();
                index++;
            } else
            {
                current.Append(c);
                index++;
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
    #endregion
}
