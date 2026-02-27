namespace Catharsis.DataStructures;

/// <summary>
/// Helper class representing a tree node for testing purposes.
/// </summary>
public class TreeNode
{
    public string Name { get; set; }
    public List<TreeNode> Children { get; }

    public TreeNode(string name)
    {
        Name = name;
        Children = new List<TreeNode>();
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
    }
}