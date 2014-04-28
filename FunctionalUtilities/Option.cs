using System;

namespace FunctionalUtilities
{
    public sealed class Option<T> 
    {
        private readonly  T _value;
        private readonly  bool _isSome ;

        private Option()
        {
                _isSome = false; 
        }

        private Option(T value)
        {
            _value = value;
            _isSome = true;
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public bool IsSome
        {
            get
            {
                return _isSome;
            }
        }

        public static readonly Option<T> None =  new Option<T>();

        public static Option<T> Some(T value)
        {
            return new Option<T>(value);
        }
     
        public override int GetHashCode()
        {
            var code1 = _isSome.GetHashCode();
            if (IsSome)
            {
                 code1 ^= Value.GetHashCode();
            }
            return code1;
        }

        public override bool Equals(object obj)
        {
            var o = (obj as Option<T>);
            if (o != null)
            {
                if (IsSome && o.IsSome) return Value.Equals(o.Value);
                return (!IsSome && !o.IsSome);
            }


            if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Option<>).GetGenericTypeDefinition())
            {
                return (IsSome == false) && (false == obj.GetType().InvokeMember("IsSome", System.Reflection.BindingFlags.GetProperty, null, obj, null) as bool?);
            }
            return base.Equals(obj);
        }

    }

    public static class OptionExtensions
    {
        public static Option<T> Some<T>(this T value)
        {
            return Option<T>.Some(value);
        }

        public static Option<T> Return<T>(this T value)
        {
            if (value != null) return value.Some();
            return Option<T>.None; 
        }

        public static Option<S> Bind<T, S>(this Option<T> opt, Func<T, Option<S>> f)
        {
            if (opt.IsSome)
            {
                return f(opt.Value);
            }
            return Option<S>.None;
        }

        public static Option<S> Map<T, S>(this Option<T> opt, Func<T, S> f)
        {
            if (opt.IsSome)
            {
                return f(opt.Value).Some();
            }
            return Option<S>.None;
        }

        public static Option<S> Select<T,S>(this Option<T> o, Func<T,S> select)
        {
            
            return o.Map(select);
        }

        public static Option<S> SelectMany<T, S>(this Option<T> o, Func<T, Option<S>> select)
        {

            return o.Bind(select);
        }

        public static Option<S> SelectMany<T, TCollection,S>(this Option<T> o, Func<T, Option<TCollection>> f, Func<T,TCollection,S> select)
        {
         
            if (o.IsSome && o.Bind(f).IsSome)
            {
                return select(o.Value, o.Bind(f).Value).Some();
            }
            return Option<S>.None;
        }

        public static S Maybe<T, S>(this Option<T> opt, Func<T, S> f, S def)
        {
            

            if (opt.IsSome)
            {
                return f(opt.Value);
            }
            return def;
        }

    }

   

   
    
}
