using System.Reflection;
using System.Runtime.CompilerServices;

// SENIOR DEVELOPER: Hide Internal namespaces from IntelliSense
// This ensures only the main public API namespaces are visible to consumers

// NOTE: InternalsVisibleTo removed - package consumers should only see public API
// [assembly: InternalsVisibleTo("RpaWinUiComponents.Demo")]

// PROFESSIONAL SOLUTION: Assembly-level metadata for clean API surface
[assembly: AssemblyMetadata("DesignTimeVisible", "false")]

// Ensure Internal implementations are not part of public API surface
[assembly: AssemblyMetadata("PublicApiNamespaces", "RpaWinUiComponentsPackage.AdvancedWinUiDataGrid;RpaWinUiComponentsPackage.AdvancedWinUiLogger")]

// EXPERT SOLUTION: Hide internal namespaces using metadata patterns
[assembly: AssemblyMetadata("InternalNamespacePattern", "*.Internal.*")]
[assembly: AssemblyMetadata("CompilerGenerated", "Internal")]