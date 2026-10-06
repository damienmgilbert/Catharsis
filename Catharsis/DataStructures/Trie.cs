namespace Catharsis.DataStructures;

///<summary>
///A prefix tree (trie) optimized for string key lookup, prefix matching, and auto-complete scenarios.
///</summary>
public sealed class Trie
{
    #region Fields
    readonly TrieNode _root = new();
    #endregion

    #region Private methods
    TrieNode? FindNode(string prefix)
    {
        TrieNode current = _root;

        foreach (char c in prefix)
        {
            if (!current.Children.TryGetValue(c, out TrieNode? child))
            {
                return null;
            }

            current = child;
        }

        return current;
    }

    static bool Remove(TrieNode node, string word, int index)
    {
        if (index == word.Length)
        {
            if (!node.IsEndOfWord)
            {
                return false;
            }

            node.IsEndOfWord = false;
            return true;
        }

        char c = word[index];

        if (!node.Children.TryGetValue(c, out TrieNode? child))
        {
            return false;
        }

        bool removed = Remove(child, word, index + 1);

        if (removed && !child.IsEndOfWord && (child.Children.Count == 0))
        {
            node.Children.Remove(c);
        }

        return removed;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Removes all words from the trie.
    ///</summary>
    public void Clear()
    {
        _root.Children.Clear();
        _root.IsEndOfWord = false;
        Count = 0;
    }

    ///<summary>
    ///Returns all words in the trie that start with the specified <paramref name="prefix"/>.
    ///</summary>
    ///<param name="prefix">The prefix to match.</param>
    ///<returns>A sequence of matching words.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is <c>null</c>.</exception>
    public IEnumerable<string> GetWordsWithPrefix(string prefix)
    {
        if (prefix is null)
        {
            throw new ArgumentNullException(nameof(prefix), "Prefix must not be null.");
        }

        TrieNode? node = FindNode(prefix);

        if (node is null)
        {
            yield break;
        }

        Stack<(TrieNode Node, string Word)> stack = new();
        stack.Push((node, prefix));

        while (stack.Count > 0)
        {
            var (current, word) = stack.Pop();

            if (current.IsEndOfWord)
            {
                yield return word;
            }

            foreach (var (c, child) in current.Children)
            {
                stack.Push((child, $"{word}{c}"));
            }
        }
    }

    ///<summary>
    ///Inserts a word into the trie.
    ///</summary>
    ///<param name="word">The word to insert.</param>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="word"/> is <c>null</c>.</exception>
    public void Insert(string word)
    {
        if (word is null)
        {
            throw new ArgumentNullException(nameof(word), "Word must not be null.");
        }

        TrieNode current = _root;

        foreach (char c in word)
        {
            if (!current.Children.TryGetValue(c, out TrieNode? child))
            {
                child = new TrieNode();
                current.Children[c] = child;
            }

            current = child;
        }

        if (!current.IsEndOfWord)
        {
            current.IsEndOfWord = true;
            Count++;
        }
    }

    ///<summary>
    ///Removes a word from the trie.
    ///</summary>
    ///<param name="word">The word to remove.</param>
    ///<returns><c>true</c> if the word was found and removed; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="word"/> is <c>null</c>.</exception>
    public bool Remove(string word)
    {
        if (word is null)
        {
            throw new ArgumentNullException(nameof(word), "Word must not be null.");
        }

        if (Remove(_root, word, 0))
        {
            Count--;
            return true;
        }

        return false;
    }

    ///<summary>
    ///Determines whether the trie contains the exact <paramref name="word"/>.
    ///</summary>
    ///<param name="word">The word to search for.</param>
    ///<returns><c>true</c> if the word exists in the trie; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="word"/> is <c>null</c>.</exception>
    public bool Search(string word)
    {
        if (word is null)
        {
            throw new ArgumentNullException(nameof(word), "Word must not be null.");
        }

        TrieNode? node = FindNode(word);
        return (node is not null) && node.IsEndOfWord;
    }

    ///<summary>
    ///Determines whether any word in the trie starts with the specified <paramref name="prefix"/>.
    ///</summary>
    ///<param name="prefix">The prefix to check.</param>
    ///<returns><c>true</c> if at least one word starts with <paramref name="prefix"/>; otherwise <c>false</c>.</returns>
    ///<exception cref="ArgumentNullException">Thrown when <paramref name="prefix"/> is <c>null</c>.</exception>
    public bool StartsWith(string prefix)
    {
        if (prefix is null)
        {
            throw new ArgumentNullException(nameof(prefix), "Prefix must not be null.");
        }

        return FindNode(prefix) is not null;
    }
    #endregion

    #region Public properties
    ///<summary>
    ///Gets the number of distinct words stored in the trie.
    ///</summary>
    public int Count { get; private set; }
    #endregion

    sealed class TrieNode
    {
        #region Public properties
        public Dictionary<char, TrieNode> Children { get; } = [];

        public bool IsEndOfWord { get; set; }
        #endregion
    }
}
