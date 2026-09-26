namespace Catharsis.DataStructures;

///<summary>
///Helper class representing a tree node for testing purposes.
///</summary>
public class TreeNode(string name)
{

    #region Constructors
    #endregion

    #region Public methods
    public void AddChild(TreeNode child) { Children.Add(child); }
    #endregion

    #region Public properties
    public List<TreeNode> Children { get; } = [];

    public string Name { get; set; } = name;
    #endregion
}