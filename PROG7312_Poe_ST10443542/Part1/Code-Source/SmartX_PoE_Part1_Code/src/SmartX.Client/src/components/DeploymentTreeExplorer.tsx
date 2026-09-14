import { useEffect, useState } from "react";
import { api } from "../api";
import type { DeviceNode, TreeValidationResult } from "../types";

/** Renders a device-deployment tree node, recursing into its children — the client-side
 * mirror of the server's recursive DeploymentTreeValidator. */
function TreeNodeView({ node }: { node: DeviceNode }) {
  return (
    <div className="tree-node">
      <div className={`tree-node-label ${node.isLeafDevice ? "leaf" : ""}`}>
        {node.isLeafDevice ? "📟" : "📁"} {node.name}
        {node.deviceMacAddress ? ` (${node.deviceMacAddress})` : ""}
      </div>
      {node.children.map((child, i) => (
        <TreeNodeView key={`${child.name}-${i}`} node={child} />
      ))}
    </div>
  );
}

export function DeploymentTreeExplorer() {
  const [tree, setTree] = useState<DeviceNode | null>(null);
  const [result, setResult] = useState<TreeValidationResult | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.getSampleTree().then(setTree).catch(() => {});
  }, []);

  async function runValidation() {
    if (!tree) return;
    setLoading(true);
    try {
      setResult(await api.validateTree(tree));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <p className="small-note">
        Sample multi-tier deployment profile (Facility → Zone → Sub-Zone → Node), validated
        server-side by a recursive descent over the tree.
      </p>
      {tree && <TreeNodeView node={tree} />}
      <button className="secondary" style={{ marginTop: 12 }} onClick={runValidation} disabled={loading || !tree}>
        {loading ? "Validating…" : "Run Recursive Validation"}
      </button>
      {result && (
        <div style={{ marginTop: 10 }}>
          <p className="small-note">
            Visited {result.nodesVisited} nodes, max depth {result.maxDepthReached} —{" "}
            {result.isValid ? "✅ tree is valid." : `❌ ${result.issues.length} issue(s) found.`}
          </p>
          {result.issues.length > 0 && (
            <ul className="issue-list">
              {result.issues.map((issue, i) => (
                <li key={i}>
                  <code>{issue.path}</code>: {issue.message}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
