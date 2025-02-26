﻿using System.Collections.Generic;
using System.Text;

namespace o2.Runtime.ScriptGeneration {
    /// <summary>
    /// The ScriptBuilder class is a helper for dynamically generating a C# script file. 
    /// It allows the user to add features such as class type, access modifiers, methods, fields, attributes, and interfaces.
    /// </summary>
    public class ScriptBuilder : ICSBuilder {
        /// <summary>
        /// The namespace to be included in the generated script.
        /// </summary>
        public string Namespace;

        /// <summary>
        /// The base class for the generated class.
        /// </summary>
        public string BaseClass;

        /// <summary>
        /// The type of class (e.g., normal, static, sealed, etc.).
        /// </summary>
        public ClassType ClassType = ClassType._normal;

        /// <summary>
        /// The access modifier for the generated class (e.g., public, internal, private).
        /// </summary>
        public AccessModifier AccessModifier = AccessModifier._internal;

        readonly string ScriptName;
        readonly List<UsingBuilder> _usings = new();
        readonly List<MethodBuilder> _methods = new();
        readonly List<FieldBuilder> _fields = new();
        readonly InterfaceBuilder _interfaces = new();
        readonly List<string> _appendedStructures = new();
        readonly List<string> _preprocessorDirectives = new();
        readonly List<string> _attributes = new();

        readonly List<string> _classCommends = new()
        {
            "This class was generated by o2 Script Generator",
        };

        /// <summary>
        /// Initializes a new instance of the ScriptBuilder class with the specified script name.
        /// </summary>
        public ScriptBuilder(string scriptName) {
            ScriptName = scriptName;
        }

        /// <summary>
        /// Adds an interface of type T to the generated script.
        /// </summary>
        public ScriptBuilder AddInterface<T>() {
            AddUsing(typeof(T).Namespace);
            _interfaces.AddInterface<T>();
            return this;
        }

        /// <summary>
        /// Adds a custom attribute to the generated class.
        /// </summary>
        public ScriptBuilder AddAttribute(string attribute) {
            _attributes.Add(attribute);
            return this;
        }

        /// <summary>
        /// Appends an external structure (file path) to the script.
        /// </summary>
        public ScriptBuilder AppendStructure(string path) {
            _appendedStructures.Add(path);
            return this;
        }

        /// <summary>
        /// Adds a comment to the generated class.
        /// </summary>
        public ScriptBuilder AddComment(string comment) {
            _classCommends.Add(comment);
            return this;
        }

        /// <summary>
        /// Adds an interface by its name to the generated script.
        /// </summary>
        public ScriptBuilder AddInterface(string interfaceName) {
            _interfaces.AddInterface(interfaceName);
            return this;
        }

        /// <summary>
        /// Sets the base class for the generated class.
        /// </summary>
        public ScriptBuilder SetBaseClass<T>() where T : class {
            BaseClass = typeof(T).FullName;
            return this;
        }

        /// <summary>
        /// Adds a 'using' directive for the specified namespace.
        /// </summary>
        public ScriptBuilder AddUsing(string @using) {
            if (_usings.Exists(u => u.NamespaceName == @using))
                return this;

            _usings.Add(new UsingBuilder(@using));
            return this;
        }

        /// <summary>
        /// Adds a method to the generated script.
        /// </summary>
        public ScriptBuilder AddMethod(MethodBuilder methodBuilder) {
            _methods.Add(methodBuilder);
            return this;
        }

        /// <summary>
        /// Adds a field to the generated script.
        /// </summary>
        public ScriptBuilder AddField(FieldBuilder fieldBuilder) {
            _fields.Add(fieldBuilder);
            return this;
        }

        public ScriptBuilder AddCompilationCondition(string condition) {
            _preprocessorDirectives.Add(condition);
            return this;
        }

        /// <summary>
        /// Builds and returns the final C# script as a string.
        /// </summary>
        public string Build() {
            StringBuilder builder = new();

            if (_preprocessorDirectives.Count > 0)
            {
                builder.Append("#if ");
                for (var i = 0; i < _preprocessorDirectives.Count; i++)
                {
                    var directive = _preprocessorDirectives[i];
                    builder.Append(directive);
                    builder.Append(i != _preprocessorDirectives.Count - 1 ? " || " : "\n");
                }
            }


            builder.Append(IBuildable.BuildMultiple(_usings, true));
            builder.Append("\n");

            if (!string.IsNullOrEmpty(Namespace))
                builder.Append("namespace ").Append(Namespace).Append("\n{\n");

            if (AccessModifier == AccessModifier._private || AccessModifier == AccessModifier._protected)
                AccessModifier = AccessModifier._internal;

            var accessModifier = AccessModifier.ToString().Replace("_", " ");

            foreach (var commend in _classCommends)
                builder.Append("/// ").Append(commend).Append("\n");

            foreach (var attribute in _attributes)
                builder.AppendLine(attribute);

            var classType = ClassType == ClassType._normal ? "" : ClassType + " ";
            classType = classType.Replace("_", " ");

            builder.Append(accessModifier).Append(" ").Append(classType.ToLower()).Append("class ")
                .Append(ScriptName.Replace(" ", ""));

            if (!string.IsNullOrEmpty(BaseClass))
                builder.Append(" : ").Append(BaseClass);

            if (!string.IsNullOrEmpty(BaseClass) && _interfaces.Count > 0)
                builder.Append(" , ");
            else if (_interfaces.Count > 0)
                builder.Append(" : ");

            builder.Append(_interfaces.Build())
                .Append("\n{\n")
                .Append(IBuildable.BuildMultiple(_fields))
                .Append("\n");

            foreach (var method in _methods)
            {
                builder.Append(method.Build());
                builder.Append("\n");
            }

            builder.Append("}\n");

            foreach (var structure in _appendedStructures)
                builder.Append(structure);

            if (!string.IsNullOrEmpty(Namespace))
                builder.Append("\n}");

            if (_preprocessorDirectives.Count > 0)
                builder.Append("\n#endif");

            return builder.ToString();
        }

        public override string ToString() {
            return Build();
        }
    }
}