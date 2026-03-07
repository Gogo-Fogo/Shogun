const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const repoRoot = path.resolve(__dirname, "..", "..");
const directCandidates = [
  path.join(repoRoot, "Packages", "com.gamelovers.mcp-unity", "Server~", "build", "index.js"),
  path.join(repoRoot, "Packages", "com.gamelovers.mcp-unity", "Server~", "dist", "index.js"),
];

function findPackageCacheEntry() {
  const packageCacheRoot = path.join(repoRoot, "Library", "PackageCache");
  if (!fs.existsSync(packageCacheRoot)) {
    return null;
  }

  const entries = fs
    .readdirSync(packageCacheRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory() && entry.name.startsWith("com.gamelovers.mcp-unity@"))
    .map((entry) => entry.name)
    .sort();

  for (const entry of entries) {
    const candidates = [
      path.join(packageCacheRoot, entry, "Server~", "build", "index.js"),
      path.join(packageCacheRoot, entry, "Server~", "dist", "index.js"),
    ];

    for (const candidate of candidates) {
      if (fs.existsSync(candidate)) {
        return candidate;
      }
    }
  }

  return null;
}

function resolveServerEntry() {
  for (const candidate of directCandidates) {
    if (fs.existsSync(candidate)) {
      return candidate;
    }
  }

  return findPackageCacheEntry();
}

function fail(message) {
  console.error(`[unity-mcp] ${message}`);
  process.exit(1);
}

const serverEntry = resolveServerEntry();

if (!serverEntry) {
  fail(
    [
      "Could not find the MCP Unity server build.",
      "Next steps:",
      "1. Open the Shogun project in Unity once so Package Manager resolves com.gamelovers.mcp-unity.",
      "2. In Unity, open Tools > MCP Unity > Server Window.",
      "3. If the server build is still missing, use the window's install/configure action and retry.",
    ].join("\n"),
  );
}

const child = spawn(process.execPath, [serverEntry, ...process.argv.slice(2)], {
  cwd: path.dirname(serverEntry),
  stdio: "inherit",
});

child.on("exit", (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 1);
});

child.on("error", (error) => {
  fail(`Failed to launch MCP Unity server: ${error.message}`);
});
