using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FunctionalUtilities
{
    public class Parser<I,O>
    {
        
        public Func<IEnumerable<I>, Tuple<Option<O>, IEnumerable<I>>> ParserFunc { get; private set; }
    
        public Parser(Func<IEnumerable<I>,Tuple<Option<O>, IEnumerable<I>>> parserFunc)
        {
            ParserFunc = parserFunc;
        }
       
    }


    public static class ParserExtensions
    {
        public static Parser<I, O> Return<I, O>(this O output) 
        {
            
            return new Parser<I, O>(x => Tuple.Create(output.Some(), x));
        }

        public static Func<IEnumerable<I>, Tuple<Option<O>, IEnumerable<I>>> Run<I, O>(this Parser<I, O> p)
        {
            return p.ParserFunc;
        }

        public static Parser<I,O> Bind<X,I,O>(this Parser<I,X> p, Func<X, Parser<I,O>> func)
        {
            return new Parser<I, O>(
                s =>
                {
                    var f = p.Run();
                    var r1 = f(s);
                    if (r1.Item1.IsSome)
                    {
                        var p2 = func(r1.Item1.Value);
                        var r2 = p2.Run()(r1.Item2);
                        if(r2.Item1.IsSome)
                        {
                            return r2;
                        }
                        return Tuple.Create(Option<O>.None,s);
                    }
                    return Tuple.Create(Option<O>.None,s);
                }
                );

        }

       
    }
}
