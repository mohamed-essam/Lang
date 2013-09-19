using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;
using System.Drawing;

namespace Lang.language
{
    public class ValueContainer<T>
    {
        public T Value;
    }

    public enum ObjectType
    {
        STRING, NUMBER, ARRAY, MAP,
        STATE,
        CLASS,
        IMAGE
    }

    public abstract class LangObject
    {
        public ObjectType objectType;
        internal Interpreter handler;

        public LangObject(ObjectType _objectType, Interpreter _handler)
        {
            objectType = _objectType;
            handler = _handler;
        }

        #region Mathematical Operators
        abstract public LangObject Plus(LangObject other);
        abstract public LangObject Minus(LangObject other);
        abstract public LangObject Multiply(LangObject other);
        abstract public LangObject Divide(LangObject other);
        abstract public LangObject Pow(LangObject other);
        abstract public LangObject Mod(LangObject other);
        #endregion

        #region Logical Operators
        abstract public bool Smaller(LangObject other);
        public bool SmallerEqual(LangObject other)
        {
            return !other.Smaller(this);
        }
        public bool Greater(LangObject other)
        {
            return !(this.Smaller(other) || this.Equal(other));
        }
        public bool GreaterEqual(LangObject other)
        {
            return !this.Smaller(other);
        }
        public bool Equal(LangObject other)
        {
            return !(this.Smaller(other) || other.Smaller(this));
        }
        public bool NotEqual(LangObject other)
        {
            return this.Smaller(other) || other.Smaller(this);
        }
        #endregion

        abstract public LangObject Clone();
    }

    public class LangNumber : LangObject
    {
        public ValueContainer<double> numberValue;

        public LangNumber(double _numberValue, Interpreter _inter)
            : base(ObjectType.NUMBER, _inter)
        {
            numberValue = new ValueContainer<double>();
            numberValue.Value = _numberValue;
        }

        public override LangObject Multiply(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.STRING:
                    return ((LangString)(other)).Multiply(this);
                case ObjectType.NUMBER:
                    return new LangNumber(numberValue.Value * ((LangNumber)(other)).numberValue.Value, handler);
                default:
                    throw new InterpreterException("Invalid operation 'number' * '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Divide(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.NUMBER:
                    return new LangNumber(numberValue.Value / ((LangNumber)(other)).numberValue.Value, handler);
                default:
                    throw new InterpreterException("Invalid operation 'number' / '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Plus(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.STRING:
                    return new LangString(Convert.ToString(numberValue.Value) + ((LangString)(other)).stringValue.Value, handler);
                case ObjectType.NUMBER:
                    return new LangNumber(numberValue.Value + ((LangNumber)(other)).numberValue.Value, handler);
                default:
                    throw new InterpreterException("Invalid operation 'number' + '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Minus(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.NUMBER:
                    return new LangNumber(numberValue.Value - ((LangNumber)(other)).numberValue.Value, handler);
                default:
                    throw new InterpreterException("Invalid operation 'number' - '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Pow(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.NUMBER:
                    return new LangNumber(Math.Pow(numberValue.Value, ((LangNumber)(other)).numberValue.Value), handler);
                default:
                    throw new InterpreterException("Invalid operation 'number' ^ '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Mod(LangObject other)
        {
            if (other.objectType == ObjectType.NUMBER)
            {
                return new LangNumber(numberValue.Value % ((LangNumber)other).numberValue.Value, handler);
            }
            throw new InterpreterException("Invalid operation 'number' % '" + Convert.ToString(other.objectType).ToLower() + "'");
        }

        public override bool Smaller(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.NUMBER:
                    return numberValue.Value < ((LangNumber)(other)).numberValue.Value;
                default:
                    throw new InterpreterException("Invalid operation 'number' < '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Clone()
        {
            return new LangNumber(numberValue.Value, handler);
        }
    }

    public class LangString : LangObject
    {
        public ValueContainer<string> stringValue;

        public LangString(string _stringValue, Interpreter _inter)
            : base(ObjectType.STRING, _inter)
        {
            stringValue = new ValueContainer<string>();
            stringValue.Value = _stringValue;
        }

        public override LangObject Plus(LangObject other)
        {
            switch (other.objectType)
            {
                case ObjectType.STRING:
                    return new LangString(stringValue.Value + ((LangString)other).stringValue.Value, handler);
                case ObjectType.NUMBER:
                    return new LangString(stringValue.Value + Convert.ToString(((LangNumber)other).numberValue.Value), handler);
                default:
                    throw new InterpreterException("Invalid operation '" + Convert.ToString(this.objectType) + "' + '" + Convert.ToString(other.objectType) + "'");
            }
        }

        public override LangObject Divide(LangObject other)
        {
            throw new InterpreterException("Invalid operation 'string' / '" + Convert.ToString(other.objectType).ToLower() + "'");
        }

        public override LangObject Minus(LangObject other)
        {
            throw new InterpreterException("Invalid operation 'string' - '" + Convert.ToString(other.objectType).ToLower() + "'");
        }

        public override LangObject Pow(LangObject other)
        {
            throw new InterpreterException("Invalid operation 'string' ^ '" + Convert.ToString(other.objectType).ToLower() + "'");
        }

        public override LangObject Multiply(LangObject other)
        {
            if (other.objectType == ObjectType.NUMBER)
            {
                string _otherValue = "";
                LangNumber langNumber = (LangNumber)other;
                for (int i = 0; i < langNumber.numberValue.Value; i++)
                {
                    _otherValue += stringValue.Value;
                }
                return new LangString(_otherValue, handler);
            }
            else
            {
                throw new InterpreterException("Invalid operation 'string' * '" + Convert.ToString(other.objectType).ToLower() + "'");
            }
        }

        public override LangObject Mod(LangObject other)
        {
            throw new InterpreterException("Invalid operation 'string' % '" + Convert.ToString(other.objectType).ToLower() + "'");
        }

        public override bool Smaller(LangObject other)
        {
            if (other.objectType == ObjectType.STRING)
            {
                string _stringValue = ((LangString)(other)).stringValue.Value;
                if (stringValue.Value.Length < _stringValue.Length)
                    return true;
                else if (stringValue.Value.Length > _stringValue.Length)
                    return false;
                for (int i = 0; i < stringValue.Value.Length; i++)
                {
                    if (stringValue.Value[i] != _stringValue[i])
                        return stringValue.Value[i] < _stringValue[i];
                }
                return false;
            }
            else
            {
                throw new InterpreterException("Invalid logical operation on types 'string' and '" + Convert.ToString(other.objectType).ToLower() + "'");
            }
        }

        public override LangObject Clone()
        {
            return new LangString(stringValue.Value, handler);
        }
    }

    public class LangMap : LangObject
    {
        public ValueContainer<Hashtable> arrayValue;

        public LangMap(Hashtable _arrayValue, Interpreter _inter)
            : base(ObjectType.MAP, _inter)
        {
            arrayValue = new ValueContainer<Hashtable>();
            arrayValue.Value = _arrayValue;
        }

        public override LangObject Divide(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Minus(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Mod(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Multiply(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Plus(LangObject other)
        {
            if (other.objectType == ObjectType.MAP)
            {
                LangMap ret = (LangMap)this.Clone();
                foreach (DictionaryEntry dic in arrayValue.Value)
                {
                    ret.arrayValue.Value[dic.Key] = dic.Value;
                }
                return ret;
            }
            throw new InterpreterException();
        }

        public override LangObject Pow(LangObject other)
        {
            throw new InterpreterException();
        }

        public override bool Smaller(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Clone()
        {
            Hashtable arrVal = new Hashtable();
            foreach (DictionaryEntry dic in arrayValue.Value)
            {
                arrVal[dic.Key] = ((LangObject)dic.Value).Clone();
            }
            return new LangMap(arrVal, handler);
        }
    }

    public class LangState : LangObject
    {
        public string message;
        public LangObject optionalMessage;

        public LangState(string _message, Interpreter _inter)
            : base(ObjectType.STATE, _inter)
        {
            message = _message;
        }

        public LangState(string _message, LangObject _optional, Interpreter _inter)
            : base(ObjectType.STATE, _inter)
        {
            message = _message;
            optionalMessage = _optional;
        }

        public override LangObject Divide(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Minus(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Mod(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Multiply(LangObject other)
        {
            throw new InterpreterException();
        }
        public override LangObject Plus(LangObject other)
        {
            throw new InterpreterException();
        }
        public override LangObject Pow(LangObject other)
        {
            throw new InterpreterException();
        }
        public override bool Smaller(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Clone()
        {
            throw new InterpreterException();
        }
    }

    public class LangClass : LangObject
    {
        public ValueContainer<Hashtable> vars, methods;
        internal ValueContainer<Hashtable> permissions;
        public ValueContainer<ArrayList> constructors;
        public ValueContainer<string> name;

        public LangClass(ArrayList _vars, ArrayList _methods, ArrayList _constructor, string _name, Interpreter _inter)
            : base(ObjectType.CLASS, _inter)
        {
            vars = new ValueContainer<Hashtable>();
            vars.Value = new Hashtable();
            permissions = new ValueContainer<Hashtable>();
            permissions.Value = new Hashtable();
            foreach (ClassMember varia in _vars)
            {
                vars.Value[varia.name] = new LangNumber(0, handler);
                permissions.Value[varia.name] = varia.Modifiers;
            }
            methods = new ValueContainer<Hashtable>();
            methods.Value = new Hashtable();
            foreach (FunctionStatement func in _methods)
            {
                methods.Value[func.name] = func;
            }
            constructors = new ValueContainer<ArrayList>();
            constructors.Value = _constructor;
            name = new ValueContainer<string>();
            name.Value = _name;
        }

        public LangClass(Hashtable _vars, Hashtable _perms, Hashtable _methods, ArrayList _constructor, string _name, Interpreter _inter)
            : base(ObjectType.CLASS, _inter)
        {
            vars = new ValueContainer<Hashtable>();
            vars.Value = new Hashtable();
            permissions = new ValueContainer<Hashtable>();
            permissions.Value = _perms;
            foreach (DictionaryEntry varia in _vars)
            {
                vars.Value[varia.Key] = ((LangObject)varia.Value).Clone();
            }
            methods = new ValueContainer<Hashtable>();
            methods.Value = new Hashtable();
            foreach (DictionaryEntry func in _methods)
            {
                methods.Value[func.Key] = func.Value;
            }
            constructors = new ValueContainer<ArrayList>();
            constructors.Value = _constructor;
            name = new ValueContainer<string>();
            name.Value = _name;
        }

        public LangClass(ClassStatement _stat, ClassInitStatement _call, Interpreter _inter)
            : base(ObjectType.CLASS, _inter)
        {
            vars = new ValueContainer<Hashtable>();
            vars.Value = new Hashtable();
            permissions = new ValueContainer<Hashtable>();
            permissions.Value = new Hashtable();
            foreach (ClassMember str in _stat.vars)
            {
                vars.Value[str.name] = new LangNumber(0, handler);
                permissions.Value[str.name] = str.Modifiers;
            }
            methods = new ValueContainer<Hashtable>();
            methods.Value = _stat.methods;
            constructors = new ValueContainer<ArrayList>();
            constructors.Value = _stat.constructors;
            name = new ValueContainer<string>();
            name.Value = _stat.name;
        }

        public override LangObject Clone()
        {
            Hashtable _vars = new Hashtable();
            foreach (DictionaryEntry dic in vars.Value)
            {
                _vars[dic.Key] = ((LangObject)dic.Value).Clone();
            }
            return new LangClass(_vars, permissions.Value, methods.Value, constructors.Value, name.Value, handler);
        }

        public override LangObject Plus(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' + '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' + '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_plus"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' + '" + right.name + "'");
            }
        }
        public override LangObject Minus(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' - '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' - '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_minus"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' - '" + right.name + "'");
            }
        }
        public override LangObject Divide(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' / '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' / '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_divide"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' / '" + right.name + "'");
            }
        }
        public override LangObject Multiply(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' * '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' * '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_multiply"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' * '" + right.name + "'");
            }
        }
        public override LangObject Pow(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' ^ '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' ^ '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_power"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' ^ '" + right.name + "'");
            }
        }
        public override LangObject Mod(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' % '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' % '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_plus"))
            {
                FunctionStatement stat = (FunctionStatement)((ArrayList)this.methods.Value["__operator_mod"])[0];
                return handler.RunClassOperator(stat, this, right);
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' % '" + right.name + "'");
            }
        }

        public override bool Smaller(LangObject other)
        {
            if (other.objectType != ObjectType.CLASS)
            {
                throw new InterpreterException("Invalid operation 'class' < '" + Convert.ToString(other.objectType) + "'");
            }
            LangClass right = (LangClass)other;
            if (right.name != this.name)
            {
                throw new InterpreterException("Invalid Operation '" + this.name + "' < '" + right.name + "'");
            }
            if (this.methods.Value.ContainsKey("__operator_smaller"))
            {
                FunctionStatement stat = (FunctionStatement)(((ArrayList)this.methods.Value["__operator_smaller"])[0]);
                LangObject obj = handler.RunClassOperator(stat, this, right);
                if (obj.objectType == ObjectType.NUMBER)
                {
                    return (((LangNumber)obj).numberValue.Value == 1);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new InterpreterException("No overloaded function for '" + this.name + "' < '" + right.name + "'");
            }
        }
    }

    public class LangImage : LangObject
    {
        internal ValueContainer<Bitmap> imageValue;

        public LangImage(Bitmap _imageValue, Interpreter _inter)
            : base(ObjectType.IMAGE, _inter)
        {
            imageValue = new ValueContainer<Bitmap>();
            imageValue.Value = _imageValue;
        }

        public override LangObject Clone()
        {
            while (true)
            {
                try
                {
                    return new LangImage(new Bitmap(imageValue.Value), handler);
                }
                catch (Exception)
                {

                }
            }
        }

        public override LangObject Divide(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Minus(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Multiply(LangObject other)
        {
            throw new InterpreterException();
        }

        public override bool Smaller(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Pow(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Mod(LangObject other)
        {
            throw new InterpreterException();
        }

        public override LangObject Plus(LangObject other)
        {
            throw new InterpreterException();
        }
    }
}
