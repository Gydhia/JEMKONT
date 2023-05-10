#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System.Linq;
using UnityEditor;

namespace XNodeEditor
{
    [InitializeOnLoad]
    public static class OdinInspectorHelper
    {
        static OdinInspectorHelper()
        {
            EditorApplication.delayCall += () =>
            {
                IsOdinExtensionLoaded = AssemblyUtilities.GetAllAssemblies().FirstOrDefault( x => x.GetName().Name == "XNodeEditorOdin" ) != null;
                IsReady = true;
            };
        }

        public static bool IsReady { get; private set; }

        private static bool IsOdinExtensionLoaded;

        public static bool EnableOdinNodeDrawer
        {
            get
            {
                return IsOdinExtensionLoaded && InspectorConfig.Instance.EnableOdinInInspector;
            }
        }

        public static bool EnableOdinEditors
        {
            get
            {
                return InspectorConfig.Instance.EnableOdinInInspector;
            }
        }
    }
}
#endif