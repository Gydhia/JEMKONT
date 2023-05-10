﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace XNodeEditor.Internal {
	/// <summary> Handles caching of custom editor classes and their target types. Accessible with GetEditor(Type type) </summary>
	/// <typeparam name="T">Editor Type. Should be the type of the deriving script itself (eg. NodeEditor) </typeparam>
	/// <typeparam name="A">Attribute Type. The attribute used to connect with the runtime type (eg. CustomNodeEditorAttribute) </typeparam>
	/// <typeparam name="K">Runtime Type. The ScriptableObject this can be an editor for (eg. Node) </typeparam>
	public abstract class NodeEditorBase<T, A, K> : IDisposable where A : Attribute, NodeEditorBase<T, A, K>.INodeEditorAttrib where T : NodeEditorBase<T, A, K> where K : ScriptableObject {
		/// <summary> Custom editors defined with [CustomNodeEditor] </summary>
		private static Dictionary<Type, Type> editorTypes;
		private static Dictionary<NodeEditorWindow, Dictionary<K, T>> windowKeyedEditors = new Dictionary<NodeEditorWindow, Dictionary<K, T>>();
		public NodeEditorWindow window;
		public K target;
		public SerializedObject serializedObject;
#if ODIN_INSPECTOR
		private PropertyTree _objectTree;
		public PropertyTree objectTree {
			get {
                if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(K)))
                {
                    if (this._objectTree != null && ((UnityEngine.Object)this._objectTree.WeakTargets[0]) == null)
                    {
                        this._objectTree.Dispose();
                        this._objectTree = null;
                    }
                }

				if (this._objectTree == null) {
					try {
						bool wasInEditor = NodeEditor.InNodeEditor;
						NodeEditor.InNodeEditor = true;
						this._objectTree = PropertyTree.Create(this.serializedObject);
						NodeEditor.InNodeEditor = wasInEditor;
					} catch (ArgumentException ex) {
						Debug.Log(ex);
					}
				}
				return this._objectTree;
			}
		}
#endif

		public static T GetEditor(K target, NodeEditorWindow window) {
			if (target == null) return null;
			T editor;
            Dictionary<K, T> editors;
            if (!windowKeyedEditors.TryGetValue(window, out editors)) {
                windowKeyedEditors.Add( window, editors = new Dictionary<K, T>() );
            }
			if (!editors.TryGetValue(target, out editor)) {
				Type type = target.GetType();
				Type editorType = GetEditorType(type);
				editor = Activator.CreateInstance(editorType) as T;
				editor.target = target;
				editor.serializedObject = new SerializedObject(target);
				editor.window = window;
				editor.OnCreate();
                editors.Add(target, editor);
			}
			if (editor.target == null) editor.target = target;
			if (editor.window != window) editor.window = window;
			if (editor.serializedObject == null) editor.serializedObject = new SerializedObject(target);
			return editor;
		}

        public static void RemoveEditor( K target, NodeEditorWindow window )
        {
            if ( target == null ) return;
            T editor;
            Dictionary<K, T> editors;
            if ( !windowKeyedEditors.TryGetValue( window, out editors ) )
                return;
            if ( !editors.TryGetValue( target, out editor ) )
                return;

            IDisposable disposable = editor as IDisposable;
            if ( disposable != null )
                disposable.Dispose();

            editors.Remove( target );
            return;
        }

        public static void ClearEditors( NodeEditorWindow window )
        {
            Dictionary<K, T> editors;
            if ( !windowKeyedEditors.TryGetValue( window, out editors ) )
                return;

            foreach ( var kvp in editors )
            {
                IDisposable disposable = kvp.Value as IDisposable;
                if ( disposable != null )
                    disposable.Dispose();
            }

            editors.Clear();
            windowKeyedEditors.Remove( window );
        }

		private static Type GetEditorType(Type type) {
			if (type == null) return null;
			if (editorTypes == null) CacheCustomEditors();
			Type result;
			if (editorTypes.TryGetValue(type, out result)) return result;
			//If type isn't found, try base type
			return GetEditorType(type.BaseType);
		}

		private static void CacheCustomEditors() {
			editorTypes = new Dictionary<Type, Type>();

			//Get all classes deriving from NodeEditor via reflection
			Type[] nodeEditors = typeof(T).GetDerivedTypes();
			for (int i = 0; i < nodeEditors.Length; i++) {
				if (nodeEditors[i].IsAbstract) continue;
				var attribs = nodeEditors[i].GetCustomAttributes(typeof(A), false);
				if (attribs == null || attribs.Length == 0) continue;
				A attrib = attribs[0] as A;
				editorTypes.Add(attrib.GetInspectedType(), nodeEditors[i]);
			}
		}

		/// <summary> Called on creation, after references have been set </summary>
		public virtual void OnCreate() { }

        public virtual void OnClose() { }

        void IDisposable.Dispose()
        {
            OnClose();

#if ODIN_INSPECTOR
            if ( _objectTree != null )
            {
                _objectTree.Dispose();
                _objectTree = null;
            }
#endif
        }

        public interface INodeEditorAttrib {
			Type GetInspectedType();
		}
	}
}