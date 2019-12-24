using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace RN._Editor
{
    //
    partial class Inspector
    {
        protected bool _debug = false;

        protected bool _baseType = false;
        protected bool _static = false;
        protected bool _field = true;
        protected bool _property = false;
        protected bool _object = false;
        protected bool _nonSerialized = false;

        protected bool _refresh = false;

        protected void onDebugButtonState(bool read)
        {
            if (target == null)
                return;

            if (read)
            {
                _debug = EditorPrefs.GetBool(target.GetType() + "._debug", _debug);

                _baseType = EditorPrefs.GetBool(target.GetType() + "._baseType", _baseType);
                _static = EditorPrefs.GetBool(target.GetType() + "._static", _static);
                _field = EditorPrefs.GetBool(target.GetType() + "._field", true);
                _nonSerialized = EditorPrefs.GetBool(target.GetType() + "._nonSerialized", true);
                _property = EditorPrefs.GetBool(target.GetType() + "._property", _property);
                _object = EditorPrefs.GetBool(target.GetType() + "._object", _object);

                _refresh = EditorPrefs.GetBool(target.GetType() + "._refresh", _refresh);
            }
            else
            {
                EditorPrefs.SetBool(target.GetType() + "._debug", _debug);

                EditorPrefs.SetBool(target.GetType() + "._baseType", _baseType);
                EditorPrefs.SetBool(target.GetType() + "._static", _static);
                EditorPrefs.SetBool(target.GetType() + "._field", _field);
                EditorPrefs.SetBool(target.GetType() + "._nonSerialized", _nonSerialized);
                EditorPrefs.SetBool(target.GetType() + "._property", _property);
                EditorPrefs.SetBool(target.GetType() + "._object", _object);

                EditorPrefs.SetBool(target.GetType() + "._refresh", _refresh);
            }
        }

        void debugButton()
        {
            _debug = GUILayout.Toggle(_debug, "debug", buttonStyle, GUILayout.ExpandWidth(false));
        }

        void debugToggles()
        {
            //EditorGUILayout.Separator();
            _baseType = GUILayout.Toggle(_baseType, "base", buttonStyle, GUILayout.ExpandWidth(false));
            _static = GUILayout.Toggle(_static, "static", buttonStyle, GUILayout.ExpandWidth(false));
            _field = GUILayout.Toggle(_field, "field", buttonStyle, GUILayout.ExpandWidth(false));
            _nonSerialized = GUILayout.Toggle(_nonSerialized, "non serialized", buttonStyle, GUILayout.ExpandWidth(false));
            _property = GUILayout.Toggle(_property, "property", buttonStyle, GUILayout.ExpandWidth(false));
            _object = GUILayout.Toggle(_object, "object", buttonStyle, GUILayout.ExpandWidth(false));

            //GUILayout.Space(4);
            EditorGUILayout.Separator();
            _refresh = GUILayout.Toggle(_refresh, "refresh", buttonStyle, GUILayout.ExpandWidth(false));
        }

        protected void onDebug()
        {
            if (_debug)
            {
                using (horizontal())
                    debugToggles();

                using (vertical())
                {
                    GUI.enabled = false;

                    if (_static)
                    {
                        Area.separatorLine();
                        if (_field)
                            foreach (var field in getAllFields(target.GetType(), _baseType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                            {
                                if (excludeField(field.Name)) continue;

                                if (propertyField(field.FieldType, field.Name, field.GetValue(target)) == false)
                                    helpBox(field.Name, field.FieldType);
                            }
                        if (_property)
                            foreach (var property in getAllPropertys(target.GetType(), _baseType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
                            {
                                object v; try { v = property.GetValue(target, null); } catch (System.Exception e) { helpBox(property.Name, e.InnerException); continue; }

                                if (propertyField(property.PropertyType, property.Name, v) == false)
                                    helpBox(property.Name, property.PropertyType);
                            }
                    }


                    if (_baseType)
                    {
                        Area.separatorLine();
                        if (_field)
                            foreach (var field in getAllFields(target.GetType().BaseType, true, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                            {
                                if (excludeField(field.Name)) continue;
                                if (field.GetCustomAttributes(typeof(SerializeField), true).Length > 0) continue;

                                if (field.IsPublic == false || (_nonSerialized && field.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length > 0))
                                {
                                    if (propertyField(field.FieldType, field.Name, field.GetValue(target)) == false)
                                        helpBox(field.Name, field.FieldType);
                                }
                            }
                        if (_property)
                            foreach (var property in getAllPropertys(target.GetType().BaseType, true, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                            {
                                if (excludeProperty(property.Name)) continue;

                                object v; try { v = property.GetValue(target, null); } catch (System.Exception e) { helpBox(property.Name, e.InnerException); continue; }

                                if (propertyField(property.PropertyType, property.Name, v) == false)
                                    helpBox(property.Name, property.PropertyType);
                            }
                    }

                    Area.separatorLine();
                    if (_field)
                        foreach (var field in getAllFields(target.GetType(), false, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            if (excludeField(field.Name)) continue;
                            if (field.GetCustomAttributes(typeof(SerializeField), true).Length > 0) continue;

                            if (field.IsPublic == false || (_nonSerialized && field.GetCustomAttributes(typeof(System.NonSerializedAttribute), true).Length > 0))
                            {
                                if (propertyField(field.FieldType, field.Name, field.GetValue(target)) == false)
                                    helpBox(field.Name, field.FieldType);
                            }
                        }
                    if (_property)
                        foreach (var property in getAllPropertys(target.GetType(), false, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            if (excludeProperty(property.Name)) continue;

                            object v; try { v = property.GetValue(target, null); } catch (System.Exception e) { helpBox(property.Name, e.InnerException); continue; }

                            if (propertyField(property.PropertyType, property.Name, v) == false)
                                helpBox(property.Name, property.PropertyType);
                        }

                    GUI.enabled = true;
                }

                if (_refresh)
                    EditorUtility.SetDirty(target);
            }
        }

        // 
        protected bool excludeField(string n)
        {
            return n.IndexOf("k__BackingField") > 0;
        }
        protected bool excludeProperty(string n)
        {
            return n == "rigidbody"
                || n == "rigidbody2D"
                || n == "useGUILayout"
                || n == "isActiveAndEnabled"
                || n == "enabled"
                || n == "tag"
                || n == "camera"
                || n == "light"
                || n == "animation"
                || n == "constantForce"
                || n == "renderer"
                || n == "audio"
                || n == "guiText"
                || n == "networkView"
                || n == "guiElement"
                || n == "guiTexture"
                || n == "collider"
                || n == "collider2D"
                || n == "hingeJoint"
                || n == "particleEmitter"
                || n == "particleSystem"
                || n == "gameObject"
                || n == "name"
                || n == "hideFlags"
                || n == "transform"
                ;
        }
        protected void helpBox(string n, System.Exception e)
        {
            EditorGUILayout.HelpBox
                (target.GetType() + "." + n
                + "\n" + e.GetType()
                + "\n" + e.Message
                , MessageType.Warning);
        }

        protected bool propertyField(System.Type t, string n, object v)
        {
            //if (value == null)
            //    return true;
            if (t.IsClass == false)
            {
                if (t == typeof(bool))
                    EditorGUILayout.Toggle(n, (bool)v);
                else if (t == typeof(int))
                    EditorGUILayout.IntField(n, (int)v);
                else if (t == typeof(byte))
                    EditorGUILayout.IntField(n, (byte)v);
                else if (t == typeof(short))
                    EditorGUILayout.IntField(n, (short)v);
                else if (t == typeof(long))
                    EditorGUILayout.LongField(n, (long)v);
                else if (t == typeof(float))
                    EditorGUILayout.FloatField(n, (float)v);
                else if (t == typeof(double))
                    EditorGUILayout.DoubleField(n, (double)v);
                else if (t == typeof(Vector2))
                    EditorGUILayout.Vector2Field(n, (Vector2)v);
                else if (t == typeof(Vector3))
                    EditorGUILayout.Vector3Field(n, (Vector3)v);
                else if (t == typeof(Vector4))
                    EditorGUILayout.Vector4Field(n, (Vector4)v);
                else if (t == typeof(Quaternion))
                {
                    var q = (Quaternion)v;
                    EditorGUILayout.Vector4Field(n, new Vector4(q.x, q.y, q.z, q.w));
                }
                else if (t == typeof(Color))
                    EditorGUILayout.ColorField(n, (Color)v);
                else if (t == typeof(Rect))
                    EditorGUILayout.RectField(n, (Rect)v);
                else if (t == typeof(Bounds))
                    EditorGUILayout.BoundsField(n, (Bounds)v);
                else if (t == typeof(LayerMask))
                    EditorGUILayout.LayerField(n, (LayerMask)v);
                else if (t.IsEnum)
                    EditorGUILayout.EnumPopup(n, (System.Enum)v);
                else
                    return false;
            }
            else
            {
                if (t.IsSubclassOf(typeof(Object)))
                {
                    if (_object)
                        EditorGUILayout.ObjectField(n, v as Object, t, true);
                }
                else
                {
                    if (t == typeof(string))
                    {
                        var s = v as string;
                        if (s != null)
                            EditorGUILayout.TextField(n, s);
                        else
                            EditorGUILayout.LabelField(n + " = null");
                    }
                    else if (t == typeof(AnimationCurve))
                    {
                        var ac = v as AnimationCurve;
                        if (ac != null)
                            EditorGUILayout.CurveField(n, ac);
                        else
                            EditorGUILayout.LabelField(n + " = null");
                    }


                    else if (t == typeof(bool[]) || t == typeof(List<bool>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.Toggle("element " + index, (bool)o); });
                    else if (t == typeof(int[]) || t == typeof(List<int>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.IntField("element " + index, (int)o); });
                    else if (t == typeof(byte[]) || t == typeof(List<byte>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.IntField("element " + index, (byte)o); });
                    else if (t == typeof(short[]) || t == typeof(List<short>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.IntField("element " + index, (short)o); });
                    else if (t == typeof(long[]) || t == typeof(List<long>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.LongField("element " + index, (long)o); });
                    else if (t == typeof(float[]) || t == typeof(List<float>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.FloatField("element " + index, (float)o); });
                    else if (t == typeof(double[]) || t == typeof(List<double>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.DoubleField("element " + index, (double)o); });
                    else if (t == typeof(Vector2[]) || t == typeof(List<Vector2>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.Vector2Field("element " + index, (Vector2)o); });
                    else if (t == typeof(Vector3[]) || t == typeof(List<Vector3>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.Vector3Field("element " + index, (Vector3)o); });
                    else if (t == typeof(Vector4[]) || t == typeof(List<Vector4>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.Vector4Field("element " + index, (Vector4)o); });
                    else if (t == typeof(Quaternion[]) || t == typeof(List<Quaternion>))
                        debugArrayPropertyField(n, v as IList, (o, index) =>
                        {
                            var q = (Quaternion)o;
                            EditorGUILayout.Vector4Field("element " + index, new Vector4(q.x, q.y, q.z, q.w));
                        });
                    else if (t == typeof(Color[]) || t == typeof(List<Color>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.ColorField("element " + index, (Color)o); });
                    else if (t == typeof(Rect[]) || t == typeof(List<Rect>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.RectField("element " + index, (Rect)o); });
                    else if (t == typeof(Bounds[]) || t == typeof(List<Bounds>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.BoundsField("element " + index, (Bounds)o); });
                    else if (t == typeof(LayerMask[]) || t == typeof(List<LayerMask>))
                        debugArrayPropertyField(n, v as IList, (o, index) => { EditorGUILayout.LayerField("element " + index, (LayerMask)o); });


                    else if (t == typeof(string[]) || t == typeof(List<string>))
                    {
                        debugArrayPropertyField(n, v as IList, (o, index) =>
                        {
                            var s = (string)o;
                            if (s != null)
                                EditorGUILayout.TextField("element " + index, s);
                            else
                                EditorGUILayout.LabelField("element " + index + " = null");
                        });
                    }
                    else if (t == typeof(AnimationCurve[]) || t == typeof(List<AnimationCurve>))
                    {
                        debugArrayPropertyField(n, v as IList, (o, index) =>
                        {
                            var ac = (AnimationCurve)o;
                            if (ac != null)
                                EditorGUILayout.CurveField("element " + index, ac);
                            else
                                EditorGUILayout.LabelField("element " + index + " = null");
                        });
                    }
                    else if (t.IsArray || t.IsGenericType)
                    {
                        var l = v as IList;

                        if (l != null)
                        {
                            bool isObject = true;
                            bool isEnum = false;
                            if (l.Count > 0)
                            {
                                if (l[0] != null && l[0].GetType().IsSubclassOf(typeof(Object)) == false)
                                {
                                    isObject = false;
                                    isEnum = l[0].GetType().IsEnum;
                                    if (isEnum == false)
                                    {
                                        EditorGUILayout.LabelField(n + "            size    " + l.Count);
                                        return true;
                                    }
                                }
                            }

                            if (isObject && _object)
                            {
                                debugArrayPropertyField(n, l, (o, index) =>
                                {
                                    var O = o as Object;
                                    EditorGUILayout.ObjectField("element " + index, O, O == null ? typeof(Object) : O.GetType(), true);
                                });
                            }
                            else if (isEnum)
                            {
                                debugArrayPropertyField(n, l, (o, index) =>
                                {
                                    EditorGUILayout.EnumPopup("element " + index, (System.Enum)o);
                                });
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(n + " = null");
                        }
                    }


                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected void helpBox(string n, System.Type type)
        {
            EditorGUILayout.HelpBox
                (target.GetType() + "." + n
                + "\nunknow type!"
                + "  type=" + type
                , MessageType.Warning);
        }

        protected void debugArrayPropertyField(string propertyName, IList array, System.Action<object, int> handler, params GUILayoutOption[] options)
        {
            var key = target.GetType() + propertyName;
            var open = EditorPrefs.GetBool(key, false);
            Rect pos;
            using (horizontal())
            {
                GUI.enabled = true;
#if false
                open = EditorGUILayout.Foldout(open, propertyName, EditorStyles.foldout);
#else
                pos = GUILayoutUtility.GetRect(0.0f, 16.0f);
                pos.x -= 11;
                open = GUI.Toggle(pos, open, propertyName, EditorStyles.foldout);
#endif
                EditorPrefs.SetBool(key, open);

                EditorGUILayout.Separator();
#if false
                GUILayout.Label("size    " + array.Count, GUILayout.ExpandWidth(false));
#else
                pos = GUILayoutUtility.GetRect(0.0f, 16.0f, GUILayout.MinWidth(75f), GUILayout.ExpandWidth(false));
                EditorGUI.LabelField(pos, "size    " + array.Count);
#endif
                GUI.enabled = false;
            }


            if (open)
            {
                using (indentLevel())
                {
                    var index = 0;
                    while (index < array.Count)
                    {
                        using (horizontal())
                        {
                            var ae = array[index];
                            handler(ae, index);
                        }

                        ++index;
                    }
                }
            }
        }



        //--------------------------------------------------------------------------------------------------
        public class RNDebugInEditor : RNDebug
        {
            public List<object> info = new List<object>();
            public void log(object message, Object context = null)
            {
                info.Add(message);
            }
            public static RNDebugInEditor logs = new RNDebugInEditor();
            public static RNDebugInEditor warnings = new RNDebugInEditor();
            public static RNDebugInEditor errors = new RNDebugInEditor();
        }

        protected void onDebugInfo()
        {
            RNDebugInEditor.logs.info.Clear();
            RNDebugInEditor.warnings.info.Clear();
            RNDebugInEditor.errors.info.Clear();

            RNDebugEx.invoke(target, "onLogs", RNDebugInEditor.logs);
            RNDebugEx.invoke(target, "onWarnings", RNDebugInEditor.warnings);
            RNDebugEx.invoke(target, "onErrors", RNDebugInEditor.errors);

            if (RNDebugInEditor.logs.info.Count > 0
            || RNDebugInEditor.warnings.info.Count > 0
            || RNDebugInEditor.errors.info.Count > 0)
            {
                Area.separatorLine();

                //using (area())
                {
                    if (RNDebugInEditor.logs.info.Count > 0)
                        foreach (var info in RNDebugInEditor.logs.info)
                            EditorGUILayout.HelpBox(info.ToString(), MessageType.Info);
                    if (RNDebugInEditor.warnings.info.Count > 0)
                        foreach (var info in RNDebugInEditor.warnings.info)
                            EditorGUILayout.HelpBox(info.ToString(), MessageType.Warning);
                    if (RNDebugInEditor.errors.info.Count > 0)
                        foreach (var info in RNDebugInEditor.errors.info)
                            EditorGUILayout.HelpBox(info.ToString(), MessageType.Error);


                    EditorGUILayout.Separator();
                }
            }
        }
    }
}