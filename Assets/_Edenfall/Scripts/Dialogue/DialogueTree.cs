using System.Collections.Generic;
using UnityEngine;

namespace Edenfall.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueTree", menuName = "Edenfall/Dialogue Tree")]
    public class DialogueTree : ScriptableObject
    {
        public string treeId;
        public string characterId;
        public string initialNodeId;
        public List<DialogueNode> nodes = new();

        public DialogueNode GetNode(string id)
        {
            foreach (var node in nodes)
            {
                if (node.nodeId == id) return node;
            }
            return null;
        }
    }
}
