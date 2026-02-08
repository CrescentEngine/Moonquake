// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;
using System.Data;

namespace Moonquake.DSL.Constructs
{
    // Overlying 
    public enum ConstructFieldType
    {
        // String
        String,
        // Based on String, but can only be of select few values.
        // Language-wise it is treated the same as a String.
        Constraint,
        // Based on Constraint (which itself is based on String), with only valid values of "Yes" and "No".
        // Language-wise it is treated the same as a String.
        Boolean,
        // Array of Strings Literals
        Array
    }
    public enum BooleanConstraint
    {
        No  = 0,
        Yes = 1
    }
    [Flags]
    public enum ConstructFieldFlags
    {
        None      = 0 << 0,
        Pure      = 1 << 0,
        Unset     = 1 << 1,
        Protected = 1 << 2
    }
    public abstract class ConstructField
    {
        public string FieldName = "";
        private ConstructFieldFlags Flags = ConstructFieldFlags.Pure | ConstructFieldFlags.Unset;
        
        public abstract void Reset();
        public abstract ConstructFieldType GetFieldType();

        public void Protect()
        {
            AddFlag(ConstructFieldFlags.Protected);
        }

        public bool HasFlags(ConstructFieldFlags InFlags) { return (Flags & InFlags) == InFlags; }
        public bool IsPure() => HasFlags(ConstructFieldFlags.Pure);
        public bool IsUnset() => HasFlags(ConstructFieldFlags.Unset);
        public bool IsProtected() => HasFlags(ConstructFieldFlags.Protected);

        public bool IsUnassigned() => HasFlags(ConstructFieldFlags.Unset);

        protected void AddFlag(ConstructFieldFlags InFlag)
        {
            if (!HasFlags(InFlag))
            {
                Flags |= InFlag;
            }
        }
        protected void RemoveFlag(ConstructFieldFlags InFlag)
        {
            if (HasFlags(InFlag))
            {
                Flags &= ~(InFlag);
            }
        }
    }
    public class StringField : ConstructField
    {
        private string DefaultValue;
        private string Value = "";

        public StringField() : this("")
        {
        }
        public StringField(string InDefaultValue)
        {
            DefaultValue = InDefaultValue;
            Value = DefaultValue;
        }

        public override void Reset() 
        {
            Value = DefaultValue;
            AddFlag(ConstructFieldFlags.Unset);
        }
        public void Assign(string InValue)
        {
            Value = InValue;
            RemoveFlag(ConstructFieldFlags.Pure);
            RemoveFlag(ConstructFieldFlags.Unset);
        }

        public string GetDefault() => DefaultValue;
        public string Get() => Value;
        public override ConstructFieldType GetFieldType() => ConstructFieldType.String;
    }
    public class ConstraintField : StringField
    {
        private string[] ValidValues = [];

        public ConstraintField(string[] InValidValues, string InDefaultValue) : base(InDefaultValue)
        {
            ValidValues = InValidValues;
            if (!IsValidValue(InDefaultValue))
            {
                throw new Exception($"ConstraintField ctor error: Constraint field {FieldName} has a default value that isn't in its ValidValues constaint.");
            }
        }

        public new bool Assign(string InValue)
        {
            if (!IsValidValue(InValue))
            {
                return false;
            }
            base.Assign(InValue);
            return true;
        }

        public string[] GetValidValues() => ValidValues;
        public bool IsValidValue(string InValue) => ValidValues.Contains(InValue);
        public bool HasValidValue() => IsValidValue(Get());
        public override ConstructFieldType GetFieldType() => ConstructFieldType.Constraint;
    }
    /// <summary>
    /// An ordinary constraint field but its validity is centered around an enum.
    /// Its valid values are the same as the enum its provided with.
    /// It also has convenience functions.
    /// </summary>
    /// <typeparam name="T">Enum to center the constraint around.</typeparam>
    public class ConstraintField<T> : ConstraintField where T : Enum
    {
        public ConstraintField() : this(Enum.GetValues(typeof(T)).Cast<T>().ToArray()[0])
        {
        }
        public ConstraintField(T InDefaultValue) : base(Enum.GetValues(typeof(T)).Cast<T>().Select(v => v.ToString()).ToArray(), InDefaultValue.ToString())
        {
        }

        public void Assign(T InValue)
        {
            ((StringField) this).Assign(InValue.ToString());
        }

        public T Convert()
        {
            if (!HasValidValue()) throw new Exception($"Tried to call ConstraintField<{typeof(T)}>.Get() while HasValidValue() is false.");
            return (T) Enum.Parse(typeof(T), Get());
        }
    }
    public class BooleanField : ConstraintField<BooleanConstraint>
    {
        public BooleanField() : this(false)
        {
        }
        public BooleanField(bool InDefaultValue) : base(InDefaultValue ? BooleanConstraint.Yes : BooleanConstraint.No)
        {
        }
        public new bool Convert() => base.Convert() == BooleanConstraint.Yes;
        public override ConstructFieldType GetFieldType() => ConstructFieldType.Boolean;
    }
    public class ArrayField : ConstructField
    {
        private string[] DefaultValue = [];
        private List<string> Value = [];

        public ArrayField()
        {
        }
        public ArrayField(string[] InDefaultValue)
        {
            DefaultValue = InDefaultValue;
            Value = DefaultValue.ToList();
        }

        public override void Reset()
        {
            Value = new(DefaultValue);
            AddFlag(ConstructFieldFlags.Unset);
        }
        public void Assign(string[] InValue)
        {
            Value = new(InValue);
            RemoveFlag(ConstructFieldFlags.Pure);
            RemoveFlag(ConstructFieldFlags.Unset);
        }

        public void Append(string Element)
        {
            Value.Add(Element);
            RemoveFlag(ConstructFieldFlags.Pure);
            RemoveFlag(ConstructFieldFlags.Unset);
        }
        public void Append(string[] Elements)
        {
            Value.AddRange(Elements);
            RemoveFlag(ConstructFieldFlags.Pure);
            RemoveFlag(ConstructFieldFlags.Unset);
        }

        public void Erase(string Element)
        {
            Value.RemoveAll(s => s == Element);
        }
        public void Erase(string[] Elements)
        {
            Value.RemoveAll(s => Elements.Contains(s));
        }

        public List<string> Get() => Value;
        public string[] GetDefaultValue() => DefaultValue;
        public override ConstructFieldType GetFieldType() => ConstructFieldType.Array;
    }

    public enum ConstructType
    {
        Root,
        Module,
        Schema
    }

    public enum FieldMutationResult
    {
        // Field mutated successfully.
        Successful,

        // Gonna be honest this is a fuck all reason no one should use.
        Failure,

        // Field to mutate is nonexistent.
        FieldDoesNotExist,

        // Field cannot be operated on like this (i.e. AppendToField called on a StringField).
        InvalidOperation,

        // Field cannot be mutated with the type of the given data.
        InvalidType,

        // Field cannot be mutated with the given data.
        InvalidData,

        // Below are specific to dubious assignment

        // Field was declared protected and cannot be dubiously assigned to.
        FieldProtected,

        // Field was already assigned explicitly therefore dubious assignment did nothing.
        FieldNotUnset
    }

    public abstract class Construct
    {
        public string Name = "";
        public Dictionary<string, ConstructField> Fields = new();

        public ConstructField Field(string FieldName)
        {
            ConstructField? Field;
            Fields.TryGetValue(FieldName, out Field);

            if (Field is null)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' in construct '{GetType()}' and no such field exists.");
            }

            return Field;
        }
        public StringField String(string FieldName)
        {
            ConstructField Noncasted = Field(FieldName);
            if (Noncasted.GetFieldType() != ConstructFieldType.String)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' of type 'String' in construct '{GetType()}' but the field is actually a(n) {Noncasted.GetFieldType()}.");
            }
            return (StringField) Noncasted;
        }
        public ConstraintField Constraint(string FieldName)
        {
            ConstructField Noncasted = Field(FieldName);
            if (Noncasted.GetFieldType() != ConstructFieldType.Constraint)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' of type 'Constraint' in construct '{GetType()}' but the field is actually a(n) {Noncasted.GetFieldType()}.");
            }
            return (ConstraintField) Noncasted;
        }
        public ConstraintField<T> Constraint<T>(string FieldName) where T : Enum
        {
            ConstructField Noncasted = Field(FieldName);
            if (Noncasted.GetFieldType() != ConstructFieldType.Constraint)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' of type 'Constraint<{typeof(T)}>' in construct '{GetType()}' but the field is actually a(n) {Noncasted.GetFieldType()}.");
            }
            return (ConstraintField<T>) Noncasted;
        }
        public BooleanField Boolean(string FieldName)
        {
            ConstructField Noncasted = Field(FieldName);
            if (Noncasted.GetFieldType() != ConstructFieldType.Boolean)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' of type 'Boolean' in construct '{GetType()}' but the field is actually a(n) {Noncasted.GetFieldType()}.");
            }
            return (BooleanField) Noncasted;
        }
        public ArrayField Array(string FieldName)
        {
            ConstructField Noncasted = Field(FieldName);
            if (Noncasted.GetFieldType() != ConstructFieldType.Array)
            {
                throw new Exception($"Developer Warning: Trying to get field '{Field}' of type 'Array' in construct '{GetType()}' but the field is actually a(n) {Noncasted.GetFieldType()}.");
            }
            return (ArrayField) Noncasted;
        }

        public string Str(string FieldName) => String(FieldName).Get();
        public string Con(string FieldName) => Constraint(FieldName).Get();
        public T Con<T>(string FieldName) where T : Enum => Constraint<T>(FieldName).Convert();
        public bool Bool(string FieldName) => Boolean(FieldName).Convert();
        public List<string> Arr(string FieldName) => Array(FieldName).Get();

        public FieldMutationResult AssignField(string FieldName, ExpressionAST Data)
        {
            ConstructField? Field;
            if (!Fields.TryGetValue(FieldName, out Field))
            {
                Console.WriteLine($"Tried assigning to field '{FieldName}', but no such field exists in this context.");
                return FieldMutationResult.FieldDoesNotExist;
            }

            switch (Field.GetFieldType())
            {
            case ConstructFieldType.String:
            {
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried assigning value of type '{Data.Type}' to field '{FieldName}', this field can only be assigned string values.");
                    return FieldMutationResult.InvalidType;
                }
                ((StringField) Field).Assign(((StringAST) Data).Resolved);
                break;
            }
            case ConstructFieldType.Constraint:
            case ConstructFieldType.Boolean:
            {
                ConstraintField Constraint = (ConstraintField) Field;
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried assigning value of type '{Data.Type}' to field '{FieldName}'. This field can only be assigned these string values: {string.Join(", ", Constraint.GetValidValues())}.");
                    return FieldMutationResult.InvalidType;
                }
                
                StringAST String = (StringAST) Data;
                if (!Constraint.Assign(String.Resolved))
                {
                    Console.WriteLine($"Tried assigning value '{String.Resolved}' to field '{FieldName}'. This field can only be assigned these values: {string.Join(", ", Constraint.GetValidValues())}.");
                    return FieldMutationResult.InvalidData;
                }
                break;
            }
            case ConstructFieldType.Array:
            {
                if (Data.Type != ASTType.Array)
                {
                    Console.WriteLine($"Tried assigning value of type '{Data.Type}' to field '{Field.FieldName}', this field can only be assigned array values.");
                    return FieldMutationResult.InvalidType;
                }
                ArrayField Array = (ArrayField) Field;
                Array.Assign(((ArrayAST) Data).ConstructResolvedStringArray());
                break;
            }
            }

            return FieldMutationResult.Successful;
        }

        public FieldMutationResult AppendToField(string FieldName, ExpressionAST Data)
        {
            ConstructField? Field;
            if (!Fields.TryGetValue(FieldName, out Field))
            {
                Console.WriteLine($"Tried appending to field '{FieldName}', but no such field exists in this context.");
                return FieldMutationResult.FieldDoesNotExist;
            }

            switch (Field.GetFieldType())
            {
            case ConstructFieldType.String:
            case ConstructFieldType.Constraint:
            case ConstructFieldType.Boolean:
            {
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried appending to field '{FieldName}' which is of field type '{Field.GetFieldType()}', but this type of field cannot be appended to. Only Array Fields can be appended to.");
                    return FieldMutationResult.InvalidOperation;
                }
                break;
            }
            case ConstructFieldType.Array:
            {
                ArrayField Array = (ArrayField) Field;
                switch (Data.Type)
                {
                case ASTType.String:
                {
                    Array.Append(((StringAST) Data).Resolved);
                    break;
                }
                case ASTType.Array:
                {
                    Array.Append(((ArrayAST) Data).ConstructResolvedStringArray());
                    break;
                }
                }
                break;
            }
            }

            return FieldMutationResult.Successful;
        }

        public FieldMutationResult EraseFromField(string FieldName, ExpressionAST Data)
        {
            ConstructField? Field;
            if (!Fields.TryGetValue(FieldName, out Field))
            {
                Console.WriteLine($"Tried erasing from field '{FieldName}', but no such field exists in this context.");
                return FieldMutationResult.FieldDoesNotExist;
            }

            switch (Field.GetFieldType())
            {
            case ConstructFieldType.String:
            case ConstructFieldType.Constraint:
            case ConstructFieldType.Boolean:
            {
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried erasing from to field '{FieldName}' which is of field type '{Field.GetFieldType()}', but this type of field cannot be erased from. Only Array Fields can be erased from.");
                    return FieldMutationResult.InvalidOperation;
                }
                break;
            }
            case ConstructFieldType.Array:
            {
                ArrayField Array = (ArrayField) Field;
                switch (Data.Type)
                {
                case ASTType.String:
                {
                    Array.Erase(((StringAST) Data).Resolved);
                    break;
                }
                case ASTType.Array:
                {
                    Array.Erase(((ArrayAST) Data).ConstructResolvedStringArray());
                    break;
                }
                }
                break;
            }
            }

            return FieldMutationResult.Successful;
        }

        public FieldMutationResult UnassignField(string FieldName)
        {
            ConstructField? Field;
            if (!Fields.TryGetValue(FieldName, out Field))
            {
                Console.WriteLine($"Tried unassigning field '{FieldName}', but no such field exists in this context.");
                return FieldMutationResult.FieldDoesNotExist;
            }
            Field.Reset();
            return FieldMutationResult.Successful;
        }

        public FieldMutationResult DubiousAssign(string FieldName, ExpressionAST Data)
        {
            ConstructField? Field;
            if (!Fields.TryGetValue(FieldName, out Field))
            {
                Console.WriteLine($"Tried dubious assigning to field '{FieldName}', but no such field exists in this context.");
                return FieldMutationResult.FieldDoesNotExist;
            }

            // if (Field.IsProtected())
            // {
            //     return FieldMutationResult.FieldProtected;
            // }
            // if (!Field.IsUnassigned())
            // {
            //     return FieldMutationResult.FieldNotUnset;
            // }
            // We do this individually in switch because those contain actual language level validation of data types.
            // If we do here we reject stuff that are invalid by language specification,
            // we should instead error out on nonstandard compliance first and then check for these two.

            switch (Field.GetFieldType())
            {
            case ConstructFieldType.String:
            {
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried dubious assigning value of type '{Data.Type}' to field '{FieldName}', this field can only be assigned string values.");
                    return FieldMutationResult.InvalidType;
                }
                if (Field.IsProtected())
                {
                    return FieldMutationResult.FieldProtected;
                }
                if (!Field.IsUnassigned())
                {
                    return FieldMutationResult.FieldNotUnset;
                }
                ((StringField) Field).Assign(((StringAST) Data).Resolved);
                break;
            }
            case ConstructFieldType.Constraint:
            case ConstructFieldType.Boolean:
            {
                ConstraintField Constraint = (ConstraintField) Field;
                if (Data.Type != ASTType.String)
                {
                    Console.WriteLine($"Tried dubiously assigning value of type '{Data.Type}' to field '{FieldName}'. This field can only be assigned these string values: {string.Join(", ", Constraint.GetValidValues())}.");
                    return FieldMutationResult.InvalidType;
                }
                if (Field.IsProtected())
                {
                    return FieldMutationResult.FieldProtected;
                }
                if (!Field.IsUnassigned())
                {
                    return FieldMutationResult.FieldNotUnset;
                }
                
                StringAST String = (StringAST) Data;
                if (!Constraint.Assign(String.Resolved))
                {
                    Console.WriteLine($"Tried dubiously assigning value '{String.Resolved}' to field '{FieldName}'. This field can only be assigned these values: {string.Join(", ", Constraint.GetValidValues())}.");
                    return FieldMutationResult.InvalidData;
                }
                break;
            }
            case ConstructFieldType.Array:
            {
                if (Data.Type != ASTType.Array)
                {
                    Console.WriteLine($"Tried dubiously assigning value of type '{Data.Type}' to field '{Field.FieldName}', this field can only be assigned array values.");
                    return FieldMutationResult.InvalidType;
                }
                if (Field.IsProtected())
                {
                    return FieldMutationResult.FieldProtected;
                }
                if (!Field.IsUnassigned())
                {
                    return FieldMutationResult.FieldNotUnset;
                }
                ArrayField Array = (ArrayField) Field;
                Array.Assign(((ArrayAST) Data).ConstructResolvedStringArray());
                break;
            }
            }

            return FieldMutationResult.Successful;
        }

        protected T NewField<T>(string FieldName, params object[] Arguments) where T : ConstructField
        {
            T Field = (T) Activator.CreateInstance(typeof(T), Arguments)!;
            Field.FieldName = FieldName;
            Fields[FieldName] = Field;
            return Field;
        }
        protected StringField NewString(string FieldName, string DefaultValue = "")
        {
            return NewField<StringField>(FieldName, DefaultValue);
        }
        protected ConstraintField NewConstraint(string FieldName, string[] ValidValues, string DefaultValue)
        {
            return NewField<ConstraintField>(FieldName, ValidValues, DefaultValue);
        }
        protected ConstraintField<T> NewConstraint<T>(string FieldName, T DefaultValue) where T : Enum
        {
            return NewField<ConstraintField<T>>(FieldName, DefaultValue);
        }
        protected BooleanField NewBoolean(string FieldName, bool DefaultValue = false)
        {
            return NewField<BooleanField>(FieldName, DefaultValue);
        }
        protected ArrayField NewArray(string FieldName)
        {
            return NewArray(FieldName, []);
        }
        protected ArrayField NewArray(string FieldName, string[] InDefaultValue)
        {
            return NewField<ArrayField>(FieldName, (object) InDefaultValue);
        }

        public abstract ConstructType GetConstructType();
    }
}
